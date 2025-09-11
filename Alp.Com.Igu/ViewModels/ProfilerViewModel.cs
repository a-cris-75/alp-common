using Alp.Com.DataAccessLayer.DataTypes;
using Alp.Com.Igu.Connections;
using Alp.Com.Igu.Core;
using Alp.Com.Igu.Utils;
using Alp.Com.Igu.Views.Converters;
using KSociety.Base.EventBus.Abstractions;
using KSociety.Base.EventBus.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Alp.Com.Igu.ViewModels
{
    /// <summary>
    /// Case in cui sono implementati i metodi che restituiscono i dati da mostrare sull'interfaccia utente (Gui)
    /// </summary>
    public class ProfilerViewModel : ObservableObject
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger
                           ("Alp.Com.Igu.ViewModels.GetDataDeviceViewModel");

        public MainWindowViewModel mainWindowVMParent { get; set; }


        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();

        //private static int _n_istanze = 0;

        //private readonly ILogger<AnalisiBilletteViewModel> _logger;
        //private readonly ApplicationSettings _options;

        System.Windows.Threading.DispatcherTimer TIMER_NEW_DATA = new System.Windows.Threading.DispatcherTimer();

        private const double TIMER_MAX_WAIT_SEC = 3;

        #region GESTIONE IMMAGINI DA DIA DIP
        //private const int N_IMMGINI_IN_BUFFER_MAX_ALARM = 10; // TODO parametrizzare nelle impostazioni?
        //int NSottoscrizioniaImmagineDaDiaEvent = 0;
        //public float FrameRateFromImg { get; set; }

        //public double? FramePeriodSec => FrameRateFromImg == 0 ? FramePeriodSec : (1 / FrameRateFromImg);
        //public double? FramePeriodSec => FrameRateFromImg == 0 ? null : (1 / FrameRateFromImg);

        //public const int attesaPeriodiMaxImmagineSuccessiva = 10; // Numero di perodi massimi dopo i quali dichiarare interrotta l'acquisizione delle immagini se non ne arrivano più // TODO parametrizzare
        #endregion

        private static object _locker = new object();

        public delegate void Notify();
        public event Notify IsAvvisiVisibleChangedEvent;

        public virtual void OnIsAvvisiVisibleChangedEvent()
        {
            IsAvvisiVisibleChangedEvent?.Invoke();
        }

        public ProfilerViewModel()
        {
            Init();
        }


        /// <summary>
        /// Recupera parametri da appsettings.json: qua trovo gli idx dei dispositivi:
        /// - per chiedere informazioni di stato o altro
        /// - per inviare comandi
        /// Le richieste saranno gestite sul serizio WebApi Server
        /// </summary>

        private void Init()
        {
            //_logger.Info($"AnalisiBilletteViewModel ctor Init... istanza N. {++_n_istanze}");

            #region IN CASO DI IMMAGINI DA DIA DIP
            // NSottoscrizioniaImmagineDaDiaEvent = 0;

            //RabbitMqConn.GetInstance.ImmagineDaDiaEvent -= ConnessoneRabbitMq_ImmagineDaDiaEvent;
            //// Registrazione agli eventi di arrivo immagini
            //RabbitMqConn.GetInstance.ImmagineDaDiaEvent -= ConnessoneRabbitMq_ImmagineDaDiaEvent;
            //RabbitMqConn.GetInstance.ImmagineDaDiaEvent += ConnessoneRabbitMq_ImmagineDaDiaEvent;

            //RabbitMqConn.GetInstance.ImmagineDaDipEvent -= ConnessoneRabbitMq_ImmagineDaDipEvent;
            //RabbitMqConn.GetInstance.ImmagineDaDipEvent += ConnessoneRabbitMq_ImmagineDaDipEvent;

            //RabbitMqConn.GetInstance.ResetImmaginiEvent -= ConnessoneRabbitMq_ResetImmaginiEvent;
            //RabbitMqConn.GetInstance.ResetImmaginiEvent += ConnessoneRabbitMq_ResetImmaginiEvent;
            #endregion

            RabbitMqConn.GetInstance.DatiLamieraEvent -= ConnessoneRabbitMq_DatiLamieraEvent;
            RabbitMqConn.GetInstance.DatiLamieraEvent += ConnessoneRabbitMq_DatiLamieraEvent;

            InitTimerNewData();

            System.Threading.Thread thread = System.Threading.Thread.CurrentThread;
            int threadId = GetCurrentThreadId();

            _logger.Info($"ProfilerViewModel Init: Thread [{thread.ManagedThreadId}], Current Thread Id: [{threadId}]");
        }

        public void InitTimerNewData()
        {
            TIMER_NEW_DATA.Tick += new EventHandler(timerNewData_Tick);
            TIMER_NEW_DATA.Interval = TimeSpan.FromMilliseconds(1000); // Il tempo oltre il quale, se non arriva alcuna immagine, si mostra l'immagine vuota.
        }

        private void timerNewData_Tick(object sender, EventArgs e)
        {

            try
            {
                //DatiLamieraIntegrationEvent al = (DatiLamieraIntegrationEvent)integrationEvent;
                //AnalisiLamiera.Larghezza = al.Larghezza;
                //AnalisiLamiera.Lunghezza = al.Lunghezza;
                //AnalisiLamiera.Omega_L1 = al.Omega_L1;
                //AnalisiLamiera.Omega_L2 = al.Omega_L2;
                //AnalisiLamiera.Spessore = al.Spessore;
                //AnalisiLamiera.CoordinateTaglio = al.CoordinateTaglio;


                //if (FramePeriodSec != null)
                //{
                //    double attesaMassimaSec = (double)FramePeriodSec + TIMER_MAX_WAIT_SEC;

                //    TimeSpan delayImage = (DateTime.Now - LastImageReceived);
                //    TimeSpan delayImage1 = (DateTime.Now - LastImage1Received);
                //    TimeSpan delayImage2 = (DateTime.Now - LastImage2Received);

                //    if (delayImage.TotalSeconds > attesaMassimaSec && !LastImageIsModalitaAutomatica)
                //    {
                //        _logger.Warn($"Siamo in modalità manuale e non stanno arrivando immagini da più di {attesaMassimaSec} secondi.");
                //        Immagine1MostrataSourceObject = null;
                //        Immagine2MostrataSourceObject = null;
                //        mainWindowVMParent.AreImmaginiInAcquisizione = false;
                //    }
                //    if (delayImage1.TotalSeconds > attesaMassimaSec && !LastImageIsModalitaAutomatica)
                //    {
                //        _logger.Warn($"Siamo in modalità manuale e non stanno arrivando immagini dalla telecamera 1 da più di {attesaMassimaSec} secondi.");
                //        Immagine1MostrataSourceObject = null;
                //    }
                //    if (delayImage2.TotalSeconds > attesaMassimaSec && !LastImageIsModalitaAutomatica)
                //    {
                //        _logger.Warn($"Siamo in modalità manuale e non stanno arrivando immagini dalla telecamera 2 da più di {attesaMassimaSec} secondi.");
                //        Immagine2MostrataSourceObject = null;
                //    }
                //}
                //else
                //{
                //    mainWindowVMParent.AreImmaginiInAcquisizione = false;
                //}

            }
            catch (Exception ex)
            {
                _logger.Error("timerVerificaSeStannoArrivandoImmagini_Tick Error: " + ex.Message);
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

        public string BIND_LUNGHEZZA_TOT => AnalisiLamiera.Lunghezza.ToString() + " mm";
        public string BIND_LUNGHEZZA_TOT_LBL => "X0-X" + AnalisiLamiera.CoordinateTaglio.Count.ToString();
        public string BIND_LARGHEZZA_TOT => AnalisiLamiera.Larghezza.ToString() + " mm";
        public string BIND_LARGHEZZA_TOT_LBL => "Y0-Y" + AnalisiLamiera.CoordinateTaglio.Count.ToString() ;
        public string BIND_SPESSOE => AnalisiLamiera.Spessore.ToString() + " mm";
        public string BIND_OMEGA_L1 => AnalisiLamiera.Omega_L1.ToString() + " mm";
        public string BIND_OMEGA_L2 => AnalisiLamiera.Omega_L2.ToString() + " mm";
        public List<(string, string)> BIND_LST_TAGLI {
            get
            {
                List<(string, string)> res = new List<(string, string)>();

                if (AnalisiLamiera.CoordinateTaglio.Count > 0)
                {
                    int idxL = 1;
                    float firstlen = AnalisiLamiera.CoordinateTaglio.First().Item1;
                    foreach ((float, string) o in AnalisiLamiera.CoordinateTaglio.Skip(1))
                    {
                        float len = o.Item1 - firstlen;
                        string lbl = "X" + idxL.ToString();
                        res.Add(new(lbl, len.ToString() + " mm"));
                        idxL++;
                    }
                }
                return res;
            }
        }//=> AnalisiLamiera.CoordinateTaglio.Select(X => (X.Item2, X.Item1.ToString())).ToList();
        public List<(string, string)> BIND_LST_LUNGH { 
            get {
                List<(string, string)> res = new List<(string, string)>();
                
                if (AnalisiLamiera.CoordinateTaglio.Count > 0)
                {
                    float firstlen = AnalisiLamiera.CoordinateTaglio.First().Item1;
                    int idxL = 1;
                    int idxC = 1;
                    foreach ((float, string) o in AnalisiLamiera.CoordinateTaglio.Skip(1))
                    {
                        float len = o.Item1 - firstlen;
                        bool islam = o.Item2.Equals("L");
                        string lbl = islam ? "L" + idxL.ToString() : "C"+idxC.ToString(); 
                        res.Add(new(lbl, len.ToString() + " mm"));

                        if(islam) idxL++;
                        else idxC++;
                    }
                }
                return res;
            } 
        }

        public string MsgStatoRabbitMq => mainWindowVMParent.MsgStatoRabbitMq;
        public string MsgStatoPlc => mainWindowVMParent.MsgStatoPlc;
        //public string MsgStatoAccensioneIlluminatori => mainWindowVMParent.MsgStatoAccensioneIlluminatori;
        public string MsgStatoAccensioneTelecamere => mainWindowVMParent.MsgStatoAccensioneTelecamere;
        public string MsgStatoRemota =>  (MsgStatoAccensioneTelecamere == null ? "" : ("\n" + MsgStatoAccensioneTelecamere));
        public string MsgStatoConnessioneTelecamere => mainWindowVMParent.MsgStatoConnessioneTelecamere;
        public string MsgStatoTelecamere => mainWindowVMParent.MsgStatoTelecamere;
        public string MsgStatoDipSystem => mainWindowVMParent.MsgStatoDipSystem;
        public bool IsMsgStatoRabbitMqVisibile => MsgStatoRabbitMq != null && MsgStatoRabbitMq != "";
        public bool IsMsgStatoPlcVisibile => MsgStatoPlc != null && MsgStatoPlc != "";
        //public bool IsMsgStatoAccensioneIlluminatoriVisibile => MsgStatoAccensioneIlluminatori != null && MsgStatoAccensioneIlluminatori != "";
        public bool IsMsgStatoAccensioneTelecamereVisibile => MsgStatoAccensioneTelecamere != null && MsgStatoAccensioneTelecamere != "";
        public bool IsMsgStatoRemotaVisibile =>  IsMsgStatoAccensioneTelecamereVisibile;
        public bool IsMsgStatoConnessisoneTelecamereVisibile => MsgStatoConnessioneTelecamere != null && MsgStatoConnessioneTelecamere != "";
        public bool IsMsgStatoTelecamereVisibile => MsgStatoTelecamere != null && MsgStatoTelecamere != "";
        public bool IsMsgStatoDipSystemVisibile => MsgStatoDipSystem != null && MsgStatoDipSystem != "";

        // RabbitMq (Rosso = non va , Verde = va, Giallo = le immagini si stanno accumulando nella coda BizDipImageQueue_Dalsa_1 perché il Dip non riesce a smaltirle)
        //public bool AllarmeRitardoAnalisi => NImmaginiInBuffer > N_IMMGINI_IN_BUFFER_MAX_ALARM;

        // Stato di accensione di telecamere e/o illuminatori (Rosso = comunicazione con remota non funziona, Verde = tutto acceso, Giallo = gli altri casi)
        public SemaforoColor SemaforoRemota => StatoRemota.SemaforoColore;

        // RabbitMq (Rosso = non va , Verde = va)
        public SemaforoColor SemaforoRabbitMq => StatoRabbitMq.SemaforoColore;// == SemaforoColor.Rosso ? StatoRabbitMq.SemaforoColore : (AllarmeRitardoAnalisi ? SemaforoColor.Giallo : SemaforoColor.Verde);

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

        //public bool DataOralUltimaFotoVisibility => LastImageIsModalitaAutomatica && ModalitaAutomatica && ((Immagine1Visibility ?? false) || (Immagine2Visibility ?? false));
        
        public void RefreshAvvisi()
        {
            OnPropertyChanged(nameof(IsAvvisiVisibile));
            OnIsAvvisiVisibleChangedEvent();
        }

        #region GESTIONE IMMAGINI

        //public void EnableTimerVerificaSeStannoArrivandoImmagini(bool enable)
        //{
        //    if (TIMER_NEW_DATA.IsEnabled == enable) return;

        //    LastImageReceived = DateTime.Now;
        //    LastImage1Received = DateTime.Now;
        //    LastImage2Received = DateTime.Now;

        //    TIMER_NEW_DATA.IsEnabled = enable;
        //    _logger.Info("timerVerificaSeStannoArrivandoImmagini: enable " + enable);
        //}


        //private DateTime lastImageReceived;

        //public DateTime LastImageReceived
        //{
        //    get { return lastImageReceived; }

        //    set
        //    {
        //        lastImageReceived = value;
        //        OnPropertyChanged();
        //        OnPropertyChanged(nameof(LabeDataOralUltimaFoto));
        //    }
        //}

        //private DateTime lastImage1Received;
        //public DateTime LastImage1Received
        //{
        //    get { return lastImage1Received; }

        //    set
        //    {
        //        lastImage1Received = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private DateTime lastImage2Received;
        //public DateTime LastImage2Received
        //{
        //    get { return lastImage2Received; }

        //    set
        //    {
        //        lastImage2Received = value;
        //        OnPropertyChanged();
        //    }
        //}


        //public string LabeDataOralUltimaFoto => "Ultima acquisizione: " + LastImageReceived.ToString("HH:mm:ss");

        //private bool lastImageIsModalitaAutomatica = false;

        //public bool LastImageIsModalitaAutomatica
        //{
        //    get
        //    {
        //        return lastImageIsModalitaAutomatica;
        //    }
        //    set
        //    {
        //        lastImageIsModalitaAutomatica = value;
        //        OnPropertyChanged();
        //        OnPropertyChanged(nameof(DataOralUltimaFotoVisibility));
        //    }
        //}

        //private bool modalitaAutomatica = false;

        //public bool ModalitaAutomatica
        //{
        //    get
        //    {
        //        return modalitaAutomatica;
        //    }
        //    set
        //    {
        //        modalitaAutomatica = value;
        //        OnPropertyChanged();
        //        OnPropertyChanged(nameof(DataOralUltimaFotoVisibility));
        //    }
        //}

        #endregion
        public MainWindowViewModel mwvmParent { get; set; }

        private void ConnessoneRabbitMq_DatiLamieraEvent(object sender, DatiLamieraEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _logger.Info("ConnessoneRabbitMq_DatiLamieraEvent-");

                DatiLamieraEventArgs al = (DatiLamieraEventArgs)e;
                AnalisiLamiera.Larghezza = al.DatiLamiera.Larghezza;
                AnalisiLamiera.Lunghezza = al.DatiLamiera.Lunghezza;
                AnalisiLamiera.Omega_L1 = al.DatiLamiera.Omega_L1;
                AnalisiLamiera.Omega_L2 = al.DatiLamiera.Omega_L2;
                AnalisiLamiera.Spessore = al.DatiLamiera.Spessore;
                AnalisiLamiera.CoordinateTaglio = al.DatiLamiera.CoordinateTaglio;
            });
        }

        public void OnPropertyChangedPerAvvisi()
        {
            OnPropertyChanged(nameof(AvvisiGenerali));
            OnPropertyChanged(nameof(AvvisiPerScansione));
            OnPropertyChanged(nameof(AvvisiTotale));
            OnPropertyChanged(nameof(AvvisiTotalePerBarraMessaggi));
        }

        #region GESTIONE IMMAGINI

        //private void ConnessoneRabbitMq_ImmagineDaDiaEvent(object sender, ImmagineDaDiaEventArgs e)
        //{

        //    Application.Current.Dispatcher.Invoke((Action)delegate
        //    {

        //        _logger.Info("ConnessoneRabbitMq_ImmagineDaDiaEvent: ImageIntegrationEvent: " + e.ImageIntegrationEvent.ToString());

        //        System.Threading.Thread thread = System.Threading.Thread.CurrentThread;
        //        int threadId = GetCurrentThreadId();

        //        //_logger.LogTrace($"ConnessoneRabbitMq_ImmagineDaDiaEvent Thread: [{thread?.ManagedThreadId}], Current Thread Id: [{threadId}]"); // TODO ... CANCELLARE..??

        //        try
        //        {
        //            ImageIntegrationEvent iie = e.ImageIntegrationEvent;
        //            if (iie != null)
        //            {
        //                FrameRateFromImg = iie.CurrentFrameRate;
        //                LastImageReceived = DateTime.Now;
        //                if (iie.DeviceName.Equals("Dalsa_1"))
        //                    LastImage1Received = DateTime.Now;
        //                else if (iie.DeviceName.Equals("Dalsa_2"))
        //                    LastImage2Received = DateTime.Now;
        //                LastImageIsModalitaAutomatica = iie.ModalitaAutomatica;
        //                _logger.Info("ConnessoneRabbitMq_ImmagineDaDiaEvent: ImageIntegrationEvent: iie.ModalitaAutomatica [" + iie.ModalitaAutomatica.ToString() + "]");
        //                EnableTimerVerificaSeStannoArrivandoImmagini(true);
        //                this.SetImmagine(iie.DeviceName, iie.FrameNumber, iie.ModalitaAutomatica, iie.ImageWidth, iie.ImageHeight, iie.ImageBytes, daDia: true);  //, flDemosaic:true); // Il demosaico lo facciamo nel Dia
        //            }
        //            else
        //            {
        //                _logger.Warn($"ConnessoneRabbitMq_ImmagineDaDiaEvent: e.ImageIntegrationEvent is null!");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Error("ConnessoneRabbitMq_ImmagineDaDiaEvent - Errore: " + ex.Message);
        //        }

        //        _logger.Info($"ConnessoneRabbitMq_ImmagineDaDiaEvent.");

        //    });
        //}



        //private void ConnessoneRabbitMq_ImmagineDaDipEvent(object sender, ImmagineDaDipEventArgs e)
        //{

        //    Application.Current.Dispatcher.Invoke((Action)delegate
        //    {

        //        _logger.Info("ConnessoneRabbitMq_ImmagineDaDipEvent: ImageProcessedIntegrationEvent: " + e.ImageIntegrationEvent.ToString());

        //        System.Threading.Thread thread = System.Threading.Thread.CurrentThread;
        //        int threadId = GetCurrentThreadId();

        //        //_logger.LogTrace($"ConnessoneRabbitMq_ImmagineDaDiaEvent Thread: [{thread?.ManagedThreadId}], Current Thread Id: [{threadId}]"); // TODO ... CANCELLARE..??

        //        try
        //        {

        //            ImageProcessedIntegrationEvent ipie = e.ImageIntegrationEvent;

        //            if (ipie != null)
        //            {
        //                _logger.Debug($"ProcessEvent: ImageIntegrationEvent Received: ipie.DeviceName [{ipie.DeviceName}] ipie.FrameNumber [{ipie.FrameNumber}] e.FrameNumberDia [{e.FrameNumberDia}]");
        //                if (ipie.FrameNumber != Int32.MaxValue)
        //                    NImmaginiInBuffer = e.FrameNumberDia - ipie.FrameNumber;
        //                FrameRateFromImg = ipie.CurrentFrameRate;
        //                LastImageReceived = DateTime.Now;
        //                if (ipie.DeviceName.Equals("Dalsa_1"))
        //                    LastImage1Received = DateTime.Now;
        //                else if (ipie.DeviceName.Equals("Dalsa_2"))
        //                    LastImage2Received = DateTime.Now;
        //                LastImageIsModalitaAutomatica = ipie.ModalitaAutomatica;
        //                _logger.Info("ConnessoneRabbitMq_ImmagineDaDipEvent: ImageProcessedIntegrationEvent: ipie.ModalitaAutomatica [" + ipie.ModalitaAutomatica.ToString() + "]");
        //                EnableTimerVerificaSeStannoArrivandoImmagini(true);
        //                this.SetImmagine(ipie.DeviceName, ipie.FrameNumber, ipie.ModalitaAutomatica, ipie.ImageWidth, ipie.ImageHeight, ipie.ImageBytes);
        //            }
        //            else
        //            {
        //                _logger.Warn($"ConnessoneRabbitMq_ImmagineDaDipEvent: e.ImageIntegrationEvent is null!");
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.Error("ConnessoneRabbitMq_ImmagineDaDipEvent - Errore: " + ex.Message);
        //        }

        //        _logger.Info($"ConnessoneRabbitMq_ImmagineDaDipEvent.");

        //    });
        //}

        //private void ConnessoneRabbitMq_ResetImmaginiEvent(object sender, EventArgs e)
        //{
        //    Application.Current.Dispatcher.Invoke((Action)delegate
        //    {
        //        _logger.Info("ConnessoneRabbitMq_ResetImmaginiEvent-");

        //        Immagine1MostrataSourceObject = null;
        //        Immagine2MostrataSourceObject = null;
        //    });
        //}
        //private void SetImmagine(string deviceName, int frameNumber, bool modalitaAutomatica, int imgWidth, int imgHeight, byte[] imgBytes, bool daDia = false)
        //{

        //    _logger.Info($"SetImmagine...");

        //    try
        //    {

        //        //if(frameNumber == Int32.MaxValue || imgBytes == null)
        //        if (imgBytes == null)
        //        {
        //            if (deviceName == "Dalsa_1")
        //            {
        //                Immagine1MostrataSourceObject = null;
        //            }
        //            else if (deviceName == "Dalsa_2")
        //            {
        //                Immagine2MostrataSourceObject = null;
        //            }
        //            mainWindowVMParent.AreImmaginiInAcquisizione = false;
        //            _logger.Info($"SetImmagine: immagini terminate.");
        //        }
        //        else
        //        {
        //            if (!modalitaAutomatica)
        //                mainWindowVMParent.AreImmaginiInAcquisizione = true;

        //            System.Windows.Media.Imaging.BitmapSource image = null;

        //            if (daDia)
        //                image = ArrayToBitmapSource.ConvertGrayArrayToBitmapSource(imgBytes, imgWidth, imgHeight);
        //            else
        //                image = ArrayToBitmapSource.ConvertColorBgrArrayToBitmapSource(imgBytes, imgWidth, imgHeight);

        //            if (deviceName == "Dalsa_1")
        //            {
        //                Immagine1MostrataSourceObject = image; // CroppedBitmap(image, e.ImageIntegrationEvent.ImageCropTopPx, e.ImageIntegrationEvent.ImageCropBottomPx);
        //            }
        //            else if (deviceName == "Dalsa_2")
        //            {
        //                Immagine2MostrataSourceObject = image; // CroppedBitmap(image, e.ImageIntegrationEvent.ImageCropTopPx, e.ImageIntegrationEvent.ImageCropBottomPx);
        //            }

        //            OnPropertyChanged(nameof(DataOralUltimaFotoVisibility));

        //            //if (ModalitaAutomatica)
        //            //    SystemSounds.Beep.Play();


        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error( "Errore in set immagine: rilanciato!");
        //        throw;
        //    }

        //    _logger.Info($"SetImmagine.");

        //}

        //private int nImmaginiInBuffer = 0;

        //public int NImmaginiInBuffer
        //{
        //    get
        //    {
        //        return nImmaginiInBuffer;
        //    }
        //    set
        //    {
        //        nImmaginiInBuffer = value;
        //        OnPropertyChanged();
        //        OnPropertyChanged(nameof(AllarmeRitardoAnalisi));
        //    }
        //}

        //private object immagine1DaDiaSourceObject = null;
        //public object Immagine1MostrataSourceObject
        //{
        //    get
        //    {
        //        return immagine1DaDiaSourceObject;
        //    }
        //    set
        //    {
        //        immagine1DaDiaSourceObject = value;
        //        OnPropertyChanged();
        //        OnPropertyChanged(nameof(Immagine1Visibility));
        //    }
        //}

        //public bool? Immagine1Visibility
        //{
        //    get
        //    {
        //        return immagine1DaDiaSourceObject != null;
        //    }
        //}

        //private object immagine2DaDiaSourceObject = null;

        //public object Immagine2MostrataSourceObject
        //{
        //    get
        //    {
        //        return immagine2DaDiaSourceObject;
        //    }
        //    set
        //    {
        //        immagine2DaDiaSourceObject = value;
        //        OnPropertyChanged();
        //        OnPropertyChanged(nameof(Immagine2Visibility));
        //    }
        //}

        //public bool? Immagine2Visibility
        //{
        //    get
        //    {
        //        return immagine2DaDiaSourceObject != null;
        //    }
        //}


        //public void OnPropertyChangedPerImmagine1Mostrata()
        //{
        //    OnPropertyChanged(nameof(Immagine1MostrataSourceObject));
        //    OnPropertyChanged(nameof(Immagine1Visibility));
        //}

        //public void OnPropertyChangedPerImmagine2Mostrata()
        //{
        //    OnPropertyChanged(nameof(Immagine2MostrataSourceObject));
        //    OnPropertyChanged(nameof(Immagine2Visibility));
        //}


        #endregion
        public System.Drawing.Icon IconaAvviso { get; set; } = System.Drawing.SystemIcons.Warning;
    }
}
