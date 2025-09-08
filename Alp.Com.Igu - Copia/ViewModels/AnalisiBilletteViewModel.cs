using AlpTlc.App.Igu.Core;
using AlpTlc.Domain.Impostazioni;
using AlpTlc.Domain.Strumenti;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AlpTlc.Connessione.Broker.RabbitMq.Event;
using AlpTlc.Connessione.Broker.RabbitMq;
using Quartz.Util;
using AlpTlc.Domain.StatoMacchina;
using AlpTlc.Biz;
using AlpTlc.Biz.Strumenti;
using Microsoft.Extensions.Hosting;
using System.Drawing.Imaging;
using static System.Net.WebRequestMethods;
using System.ComponentModel;
using System.Windows.Markup;
using static AlpTlc.App.Igu.ViewModels.AnalisiBilletteViewModel;
using System.Net.NetworkInformation;
using AlpTlc.Connessione.SettingsFile;
using System.Media;

namespace AlpTlc.App.Igu.ViewModels
{
    public class AnalisiBilletteViewModel : ObservableObject
    {

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();

        private static int _n_istanze = 0;

        private readonly ILogger<AnalisiBilletteViewModel> _logger;
        private readonly ApplicationSettings _options;

        System.Windows.Threading.DispatcherTimer timerVerificaSeStannoArrivandoImmagini = new System.Windows.Threading.DispatcherTimer();

        private const double TEMPO_MASSIMO_ATTESA_IMMAGINI_SEC = 3;

        private const int N_IMMGINI_IN_BUFFER_MAX_ALARM = 10; // TODO parametrizzare nelle impostazioni?

        public MainWindowViewModel mainWindowVMParent { get; set; }

        int NSottoscrizioniaImmagineDaDiaEvent = 0;

        public float FrameRateFromImg { get; set; }

        //public double? FramePeriodSec => FrameRateFromImg == 0 ? FramePeriodSec : (1 / FrameRateFromImg);
        public double? FramePeriodSec => FrameRateFromImg == 0 ? null : (1 / FrameRateFromImg);

        public const int attesaPeriodiMaxImmagineSuccessiva = 10; // Numero di perodi massimi dopo i quali dichiarare interrotta l'acquisizione delle immagini se non ne arrivano più // TODO parametrizzare

        private static object _locker = new object();

        public delegate void Notify();
        public event Notify IsAvvisiVisibleChangedEvent;

        public virtual void OnIsAvvisiVisibleChangedEvent()
        {
            IsAvvisiVisibleChangedEvent?.Invoke();
        }

        public AnalisiBilletteViewModel(ILogger<AnalisiBilletteViewModel> logger, ApplicationSettings options)
        {
            _logger = logger;
            _options = options;

            Init();
        }

        private void Init()
        {
            _logger.LogTrace($"AnalisiBilletteViewModel ctor Init... istanza N. {++_n_istanze}");

            GestioneImpostazioniGenerali.GetInstance.RecuperaImpostazioniDaFile();
            ApplicaImpostazioniGenerali();
            ImpostazioniGenerali.Instance.ResetModified();

            NSottoscrizioniaImmagineDaDiaEvent = 0;

            // Registrazione agli eventi di arrivo immagini
            ConnessoneRabbitMq.GetInstance.ImmagineDaDiaEvent -= ConnessoneRabbitMq_ImmagineDaDiaEvent;
            ConnessoneRabbitMq.GetInstance.ImmagineDaDiaEvent += ConnessoneRabbitMq_ImmagineDaDiaEvent;

            ConnessoneRabbitMq.GetInstance.ImmagineDaDipEvent -= ConnessoneRabbitMq_ImmagineDaDipEvent;
            ConnessoneRabbitMq.GetInstance.ImmagineDaDipEvent += ConnessoneRabbitMq_ImmagineDaDipEvent;

            ConnessoneRabbitMq.GetInstance.ResetImmaginiEvent -= ConnessoneRabbitMq_ResetImmaginiEvent;
            ConnessoneRabbitMq.GetInstance.ResetImmaginiEvent += ConnessoneRabbitMq_ResetImmaginiEvent;

            InitTimerVerificaSeStannoArrivandoImmagini();

            System.Threading.Thread thread = System.Threading.Thread.CurrentThread;
            int threadId = GetCurrentThreadId();

            _logger.LogTrace($"AnalisiBilletteViewModel Init: Thread [{thread.ManagedThreadId}], Current Thread Id: [{threadId}], N. sottoscrizioni a ImmagineDaDiaEvent: [{(++NSottoscrizioniaImmagineDaDiaEvent)}]");

            _logger.LogTrace($"AnalisiBilletteViewModel Init.");
        }

        public void InitTimerVerificaSeStannoArrivandoImmagini()
        {
            timerVerificaSeStannoArrivandoImmagini.Tick += new EventHandler(timerVerificaSeStannoArrivandoImmagini_Tick);
            timerVerificaSeStannoArrivandoImmagini.Interval = TimeSpan.FromMilliseconds(1000); // Il tempo oltre il quale, se non arriva alcuna immagine, si mostra l'immagine vuota.
            _logger.LogInformation("timerVerificaSeStannoArrivandoImmagini: inizializzato.");
        }

        public void EnableTimerVerificaSeStannoArrivandoImmagini(bool enable)
        {
            if (timerVerificaSeStannoArrivandoImmagini.IsEnabled == enable) return;

            LastImageReceived = DateTime.Now;
            LastImage1Received = DateTime.Now;
            LastImage2Received = DateTime.Now;

            timerVerificaSeStannoArrivandoImmagini.IsEnabled = enable;
            _logger.LogInformation("timerVerificaSeStannoArrivandoImmagini: enable " + enable);
        }

        private void timerVerificaSeStannoArrivandoImmagini_Tick(object sender, EventArgs e)
        {

            try
            {
                if (FramePeriodSec != null)
                {
                    double attesaMassimaSec = (double)FramePeriodSec + TEMPO_MASSIMO_ATTESA_IMMAGINI_SEC;

                    TimeSpan delayImage = (DateTime.Now - LastImageReceived);
                    TimeSpan delayImage1 = (DateTime.Now - LastImage1Received);
                    TimeSpan delayImage2 = (DateTime.Now - LastImage2Received);

                    if (delayImage.TotalSeconds > attesaMassimaSec && !LastImageIsModalitaAutomatica)
                    {
                        _logger.LogWarning($"Siamo in modalità manuale e non stanno arrivando immagini da più di {attesaMassimaSec} secondi.");
                        Immagine1MostrataSourceObject = null;
                        Immagine2MostrataSourceObject = null;
                        mainWindowVMParent.AreImmaginiInAcquisizione = false;
                    }
                    if (delayImage1.TotalSeconds > attesaMassimaSec && !LastImageIsModalitaAutomatica)
                    {
                        _logger.LogWarning($"Siamo in modalità manuale e non stanno arrivando immagini dalla telecamera 1 da più di {attesaMassimaSec} secondi.");
                        Immagine1MostrataSourceObject = null;
                    }
                    if (delayImage2.TotalSeconds > attesaMassimaSec && !LastImageIsModalitaAutomatica)
                    {
                        _logger.LogWarning($"Siamo in modalità manuale e non stanno arrivando immagini dalla telecamera 2 da più di {attesaMassimaSec} secondi.");
                        Immagine2MostrataSourceObject = null;
                    }
                }
                else
                {
                    mainWindowVMParent.AreImmaginiInAcquisizione = false;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "timerVerificaSeStannoArrivandoImmagini_Tick Error.");
            }

        }

        private bool isLandscape;

        public bool IsLandscape
        {
            get
            {
                return isLandscape;
            }
            set
            {
                isLandscape = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPortrait));
            }
        }

        public bool IsPortrait => !IsLandscape;

        public string MsgStatoRabbitMq => mainWindowVMParent.MsgStatoRabbitMq;
        public string MsgStatoPlc => mainWindowVMParent.MsgStatoPlc;
        public string MsgStatoAccensioneIlluminatori => mainWindowVMParent.MsgStatoAccensioneIlluminatori;
        public string MsgStatoAccensioneTelecamere => mainWindowVMParent.MsgStatoAccensioneTelecamere;
        public string MsgStatoRemota => MsgStatoAccensioneIlluminatori + (MsgStatoAccensioneTelecamere == null ? "" : ("\n" + MsgStatoAccensioneTelecamere));
        public string MsgStatoConnessioneTelecamere => mainWindowVMParent.MsgStatoConnessioneTelecamere;
        public string MsgStatoTelecamere => mainWindowVMParent.MsgStatoTelecamere;
        public string MsgStatoDipSystem => mainWindowVMParent.MsgStatoDipSystem;
        public bool IsMsgStatoRabbitMqVisibile => MsgStatoRabbitMq != null && MsgStatoRabbitMq != "";
        public bool IsMsgStatoPlcVisibile => MsgStatoPlc != null && MsgStatoPlc != "";
        public bool IsMsgStatoAccensioneIlluminatoriVisibile => MsgStatoAccensioneIlluminatori != null && MsgStatoAccensioneIlluminatori != "";
        public bool IsMsgStatoAccensioneTelecamereVisibile => MsgStatoAccensioneTelecamere != null && MsgStatoAccensioneTelecamere != "";
        public bool IsMsgStatoRemotaVisibile => IsMsgStatoAccensioneIlluminatoriVisibile || IsMsgStatoAccensioneTelecamereVisibile;
        public bool IsMsgStatoConnessisoneTelecamereVisibile => MsgStatoConnessioneTelecamere != null && MsgStatoConnessioneTelecamere != "";
        public bool IsMsgStatoTelecamereVisibile => MsgStatoTelecamere != null && MsgStatoTelecamere != "";
        public bool IsMsgStatoDipSystemVisibile => MsgStatoDipSystem != null && MsgStatoDipSystem != "";

        // RabbitMq (Rosso = non va , Verde = va, Giallo = le immagini si stanno accumulando nella coda BizDipImageQueue_Dalsa_1 perché il Dip non riesce a smaltirle)
        public bool AllarmeRitardoAnalisi => NImmaginiInBuffer > N_IMMGINI_IN_BUFFER_MAX_ALARM;

        // Stato di accensione di telecamere e/o illuminatori (Rosso = comunicazione con remota non funziona, Verde = tutto acceso, Giallo = gli altri casi)
        public SemaforoColor SemaforoRemota => StatoRemota.SemaforoColore;

        // RabbitMq (Rosso = non va , Verde = va)
        public SemaforoColor SemaforoRabbitMq => StatoRabbitMq.SemaforoColore == SemaforoColor.Rosso ? StatoRabbitMq.SemaforoColore : (AllarmeRitardoAnalisi ? SemaforoColor.Giallo : SemaforoColor.Verde);

        // Comunicazione con PLC (Rosso = non va , Verde = va)
        public SemaforoColor SemaforoPlc => StatoRabbitMq.IsRabbitMqAlive ? StatoServizioAutomazione.SemaforoColore : SemaforoColor.Grigio;

        // Stato connessione telecamere (Rosso = servizio Dia fermo, giallo = Il Dia va ma la Tlc non è connessa (magari perché è spenta))
        public SemaforoColor SemaforoStatoTelecamera1 => StatoRabbitMq.IsRabbitMqAlive ? StatoConnessioneTelecamere.SemaforoColorePerTlc(1) : SemaforoColor.Grigio;
        public SemaforoColor SemaforoStatoTelecamera2 => StatoRabbitMq.IsRabbitMqAlive ? StatoConnessioneTelecamere.SemaforoColorePerTlc(2) : SemaforoColor.Grigio;

        // Stato Servizio Dip (Rosso = servizio fermo, Verde = Halcon engine  & procedure caricate)
        public SemaforoColor SemaforoDipSystem => StatoRabbitMq.IsRabbitMqAlive ? StatoSistemaDip.SemaforoColore : SemaforoColor.Grigio;

        public bool NoChangeSelectionBilletta { get; set; } = false;

        public string AvvisiTotale => StringUtils.JoinFilter("\n", new string[] { AvvisiGenerali, AvvisiPerScansione });

        //public string AvvisiGenerali => StringUtils.JoinFilter("\n", new string[] { MsgStatoRabbitMq, MsgStatoPlc, MsgStatoTelecamere, (ModalitaAutomatica ? null : MsgStatoAccensioneIlluminatori), MsgStatoDipSystem });

        //public string AvvisiGenerali => StringUtils.JoinFilter("\n", new string[] { MsgStatoRabbitMq, MsgStatoPlc, MsgStatoTelecamere, ((!mainWindowVMParent.AreImmaginiInAcquisizione || ModalitaAutomatica) ? null : MsgStatoAccensioneIlluminatori), MsgStatoDipSystem });
        public string AvvisiGenerali => StringUtils.JoinFilter("\n", new string[] { MsgStatoRabbitMq, MsgStatoPlc, MsgStatoTelecamere, MsgStatoDipSystem });

        public string AvvisiPerScansione => ""; // StringUtils.JoinFilter("\n", new string[] { AvvisoAnomalieValidazione});

        public string AvvisiTotalePerBarraMessaggi => AvvisiTotale.Replace('\n', ' ');

        public bool IsAvvisiVisibile => CiSonoAnomalieGenerali; // || (CiSonoAnomalieValidazionePerScansioneSelezionata);

        public bool CiSonoAnomalie => !string.IsNullOrWhiteSpace(AvvisiTotale);

        public bool CiSonoAnomalieGenerali => !string.IsNullOrWhiteSpace(AvvisiGenerali);

        public bool DataOralUltimaFotoVisibility => LastImageIsModalitaAutomatica && ModalitaAutomatica && ((Immagine1Visibility ?? false) || (Immagine2Visibility ?? false));
        //public bool DataOralUltimaFotoVisibility => ModalitaAutomatica;

        public void RefreshAvvisi()
        {
            OnPropertyChanged(nameof(IsAvvisiVisibile));
            OnIsAvvisiVisibleChangedEvent();
        }

        private DateTime lastImageReceived;

        public DateTime LastImageReceived
        {
            get { return lastImageReceived; }

            set
            {
                lastImageReceived = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LabeDataOralUltimaFoto));
            }
        }

        private DateTime lastImage1Received;
        public DateTime LastImage1Received
        {
            get { return lastImage1Received; }

            set
            {
                lastImage1Received = value;
                OnPropertyChanged();
            }
        }

        private DateTime lastImage2Received;
        public DateTime LastImage2Received
        {
            get { return lastImage2Received; }

            set
            {
                lastImage2Received = value;
                OnPropertyChanged();
            }
        }


        public string LabeDataOralUltimaFoto => "Ultima acquisizione: " + LastImageReceived.ToString("HH:mm:ss");

        private bool lastImageIsModalitaAutomatica = false;

        public bool LastImageIsModalitaAutomatica
        {
            get
            {
                return lastImageIsModalitaAutomatica;
            }
            set
            {
                lastImageIsModalitaAutomatica = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DataOralUltimaFotoVisibility));
            }
        }

        private bool modalitaAutomatica = false;

        public bool ModalitaAutomatica
        {
            get
            {
                return modalitaAutomatica;
            }
            set
            {
                modalitaAutomatica = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DataOralUltimaFotoVisibility));
            }
        }


        public MainWindowViewModel mwvmParent { get; set; }


        private void ConnessoneRabbitMq_ImmagineDaDiaEvent(object sender, ImmagineDaDiaEventArgs e)
        {

            Application.Current.Dispatcher.Invoke((Action)delegate
            {

                _logger.LogTrace("ConnessoneRabbitMq_ImmagineDaDiaEvent: ImageIntegrationEvent: " + e.ImageIntegrationEvent.ToString());

                System.Threading.Thread thread = System.Threading.Thread.CurrentThread;
                int threadId = GetCurrentThreadId();

                //_logger.LogTrace($"ConnessoneRabbitMq_ImmagineDaDiaEvent Thread: [{thread?.ManagedThreadId}], Current Thread Id: [{threadId}]"); // TODO ... CANCELLARE..??

                try
                {
                    ImageIntegrationEvent iie = e.ImageIntegrationEvent;
                    if (iie != null)
                    {
                        FrameRateFromImg = iie.CurrentFrameRate;
                        LastImageReceived = DateTime.Now;
                        if (iie.DeviceName.Equals("Dalsa_1"))
                            LastImage1Received = DateTime.Now;
                        else if (iie.DeviceName.Equals("Dalsa_2"))
                            LastImage2Received = DateTime.Now;
                        LastImageIsModalitaAutomatica = iie.ModalitaAutomatica;
                        _logger.LogTrace("ConnessoneRabbitMq_ImmagineDaDiaEvent: ImageIntegrationEvent: iie.ModalitaAutomatica [" + iie.ModalitaAutomatica.ToString() + "]");
                        EnableTimerVerificaSeStannoArrivandoImmagini(true);
                        this.SetImmagine(iie.DeviceName, iie.FrameNumber, iie.ModalitaAutomatica, iie.ImageWidth, iie.ImageHeight, iie.ImageBytes, daDia: true);  //, flDemosaic:true); // Il demosaico lo facciamo nel Dia
                    }
                    else
                    {
                        _logger.LogWarning($"ConnessoneRabbitMq_ImmagineDaDiaEvent: e.ImageIntegrationEvent is null!");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ConnessoneRabbitMq_ImmagineDaDiaEvent - Errore.");
                }

                _logger.LogTrace($"ConnessoneRabbitMq_ImmagineDaDiaEvent.");

            });
        }

        private void ConnessoneRabbitMq_ImmagineDaDipEvent(object sender, ImmagineDaDipEventArgs e)
        {

            Application.Current.Dispatcher.Invoke((Action)delegate
            {

                _logger.LogTrace("ConnessoneRabbitMq_ImmagineDaDipEvent: ImageProcessedIntegrationEvent: " + e.ImageIntegrationEvent.ToString());

                System.Threading.Thread thread = System.Threading.Thread.CurrentThread;
                int threadId = GetCurrentThreadId();

                //_logger.LogTrace($"ConnessoneRabbitMq_ImmagineDaDiaEvent Thread: [{thread?.ManagedThreadId}], Current Thread Id: [{threadId}]"); // TODO ... CANCELLARE..??

                try
                {

                    ImageProcessedIntegrationEvent ipie = e.ImageIntegrationEvent;

                    if (ipie != null)
                    {
                        _logger.LogDebug($"ProcessEvent: ImageIntegrationEvent Received: ipie.DeviceName [{ipie.DeviceName}] ipie.FrameNumber [{ipie.FrameNumber}] e.FrameNumberDia [{e.FrameNumberDia}]");
                        if (ipie.FrameNumber != Int32.MaxValue)
                            NImmaginiInBuffer = e.FrameNumberDia - ipie.FrameNumber;
                        FrameRateFromImg = ipie.CurrentFrameRate;
                        LastImageReceived = DateTime.Now;
                        if (ipie.DeviceName.Equals("Dalsa_1"))
                            LastImage1Received = DateTime.Now;
                        else if (ipie.DeviceName.Equals("Dalsa_2"))
                            LastImage2Received = DateTime.Now;
                        LastImageIsModalitaAutomatica = ipie.ModalitaAutomatica;
                        _logger.LogTrace("ConnessoneRabbitMq_ImmagineDaDipEvent: ImageProcessedIntegrationEvent: ipie.ModalitaAutomatica [" + ipie.ModalitaAutomatica.ToString() + "]");
                        EnableTimerVerificaSeStannoArrivandoImmagini(true);
                        this.SetImmagine(ipie.DeviceName, ipie.FrameNumber, ipie.ModalitaAutomatica, ipie.ImageWidth, ipie.ImageHeight, ipie.ImageBytes);
                    }
                    else
                    {
                        _logger.LogWarning($"ConnessoneRabbitMq_ImmagineDaDipEvent: e.ImageIntegrationEvent is null!");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ConnessoneRabbitMq_ImmagineDaDipEvent - Errore.");
                }

                _logger.LogTrace($"ConnessoneRabbitMq_ImmagineDaDipEvent.");

            });
        }

        private void ConnessoneRabbitMq_ResetImmaginiEvent(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _logger.LogTrace("ConnessoneRabbitMq_ResetImmaginiEvent-");

                Immagine1MostrataSourceObject = null;
                Immagine2MostrataSourceObject = null;
            });
        }

        private BitmapSource CroppedBitmap(BitmapSource image, int? taglioSup, int? taglioInf)
        {
            BitmapSource croppedBitmap = image;

            try
            {

                if (image != null && (taglioSup != null && taglioSup > 0) || (taglioInf != null && taglioInf > 0))
                {
                    croppedBitmap = new CroppedBitmap(image, new System.Windows.Int32Rect(0, (taglioSup ?? 0), image.PixelWidth, image.PixelHeight - ((taglioSup ?? 0) + (taglioInf ?? 0))));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore CroppedBitmap.");
            }

            return croppedBitmap;
        }

        private void SetImmagine(string deviceName, int frameNumber, bool modalitaAutomatica, int imgWidth, int imgHeight, byte[] imgBytes, bool daDia = false)
        {

            _logger.LogTrace($"SetImmagine...");

            try
            {

                //if(frameNumber == Int32.MaxValue || imgBytes == null)
                if (imgBytes == null)
                {
                    if (deviceName == "Dalsa_1")
                    {
                        Immagine1MostrataSourceObject = null;
                    }
                    else if (deviceName == "Dalsa_2")
                    {
                        Immagine2MostrataSourceObject = null;
                    }
                    mainWindowVMParent.AreImmaginiInAcquisizione = false;
                    _logger.LogTrace($"SetImmagine: immagini terminate.");
                }
                else
                {
                    if (!modalitaAutomatica)
                        mainWindowVMParent.AreImmaginiInAcquisizione = true;

                    System.Windows.Media.Imaging.BitmapSource image = null;

                    if (daDia)
                        image = ArrayToBitmapSource.ConvertGrayArrayToBitmapSource(imgBytes, imgWidth, imgHeight);
                    else
                        image = ArrayToBitmapSource.ConvertColorBgrArrayToBitmapSource(imgBytes, imgWidth, imgHeight);

                    if (deviceName == "Dalsa_1")
                    {
                        Immagine1MostrataSourceObject = image; // CroppedBitmap(image, e.ImageIntegrationEvent.ImageCropTopPx, e.ImageIntegrationEvent.ImageCropBottomPx);
                    }
                    else if (deviceName == "Dalsa_2")
                    {
                        Immagine2MostrataSourceObject = image; // CroppedBitmap(image, e.ImageIntegrationEvent.ImageCropTopPx, e.ImageIntegrationEvent.ImageCropBottomPx);
                    }

                    OnPropertyChanged(nameof(DataOralUltimaFotoVisibility));

                    //if (ModalitaAutomatica)
                    //    SystemSounds.Beep.Play();


                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in set immagine: rilanciato!");
                throw;
            }

            _logger.LogTrace($"SetImmagine.");

        }

        private int nImmaginiInBuffer = 0;

        // TMP PER MOSTRARE QUALCOSA ALL'AVVIO:
        //private object immagine1DaDiaSourceObject = new BitmapImage(new Uri(@"D:\AlpSDDSMolatriceImg\test3_TestColata_X46CR13\7357\Originali\B7357L1S01F14T2.png", UriKind.RelativeOrAbsolute));

        public int NImmaginiInBuffer
        {
            get
            {
                return nImmaginiInBuffer;
            }
            set
            {
                nImmaginiInBuffer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AllarmeRitardoAnalisi));
            }
        }

        private object immagine1DaDiaSourceObject = null;

        // TMP PER MOSTRARE QUALCOSA ALL'AVVIO:
        //private object immagine1DaDiaSourceObject = new BitmapImage(new Uri(@"D:\AlpSDDSMolatriceImg\test3_TestColata_X46CR13\7357\Originali\B7357L1S01F14T2.png", UriKind.RelativeOrAbsolute));

        public object Immagine1MostrataSourceObject
        {
            get
            {
                return immagine1DaDiaSourceObject;
            }
            set
            {
                immagine1DaDiaSourceObject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Immagine1Visibility));
            }
        }

        public bool? Immagine1Visibility
        {
            get
            {
                return immagine1DaDiaSourceObject != null;
            }
        }

        private object immagine2DaDiaSourceObject = null;

        public object Immagine2MostrataSourceObject
        {
            get
            {
                return immagine2DaDiaSourceObject;
            }
            set
            {
                immagine2DaDiaSourceObject = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Immagine2Visibility));
            }
        }

        public bool? Immagine2Visibility
        {
            get
            {
                return immagine2DaDiaSourceObject != null;
            }
        }

        public void ApplicaImpostazioniGenerali()
        {

            _logger.LogInformation($"ApplicaImpostazioniIgu...");

            try
            {
                ModalitaAutomatica = ImpostazioniGenerali.Instance.ModalitaAutomatica;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore in ApplicaImpostazioniIgu.");
            }

            _logger.LogInformation($"ApplicaImpostazioniIgu...");
        }


        public void OnPropertyChangedPerImmagine1Mostrata()
        {
            OnPropertyChanged(nameof(Immagine1MostrataSourceObject));
            OnPropertyChanged(nameof(Immagine1Visibility));
        }

        public void OnPropertyChangedPerImmagine2Mostrata()
        {
            OnPropertyChanged(nameof(Immagine2MostrataSourceObject));
            OnPropertyChanged(nameof(Immagine2Visibility));
        }

        public void OnPropertyChangedPerAvvisi()
        {
            OnPropertyChanged(nameof(AvvisiGenerali));
            OnPropertyChanged(nameof(AvvisiPerScansione));
            OnPropertyChanged(nameof(AvvisiTotale));
            OnPropertyChanged(nameof(AvvisiTotalePerBarraMessaggi));
        }

        public System.Drawing.Icon IconaAvviso { get; set; } = System.Drawing.SystemIcons.Warning;

    }

}
