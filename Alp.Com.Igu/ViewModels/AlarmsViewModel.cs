using Alp.Com.DataAccessLayer.DataTypes;
using Alp.Com.Igu.Connections;
using Alp.Com.Igu.Core;
using Alp.Com.Igu.Utils;
using Alp.Com.Igu.Views.Converters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Alp.Com.Igu.ViewModels
{
    /// <summary>
    /// Case in cui sono implementati i metodi che restituiscono i dati da mostrare sull'interfaccia utente (Gui)
    /// </summary>
    public class AlarmsViewModel : ObservableObject
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger
                           ("Alp.Com.Igu.ViewModels.GetDataDeviceViewModel");

        public MainWindowViewModel mainWindowVMParent { get; set; }

        //static RemotaInOutWebApi remotaInOutAzioni = new RemotaInOutWebApi(0,);


        //public MainWindowViewModel mwvmParent { get; set; }


        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();

        public string BIND_DATE_ALARM1 => AnalisiLamiera.DataRicezioneDati.ToShortTimeString();
        public string BIND_DATE_ALARM2 => DateTime.MinValue.ToShortTimeString();

        // dati da plc
        public string BIND_MSG_ALARM1;
        // dati piano di taglio
        public string BIND_MSG_ALARM2;

        public Color BIND_COLOR_ALARM1 => (DateTime.Now.Subtract(AnalisiLamiera.DataRicezioneDati).TotalSeconds <= TEMPO_MASSIMO_ATTESA_DATI_SEC)? Colors.Green: Colors.Red;
        public Color BIND_COLOR_ALARM2 => Colors.Green;


        private readonly ApplicationSettings _options;

        System.Windows.Threading.DispatcherTimer timerCheckStatusDev = new System.Windows.Threading.DispatcherTimer();

        private const double TEMPO_MASSIMO_ATTESA_DATI_SEC = 10;

        
        private static object _locker = new object();

        public delegate void Notify();
        public event Notify IsAvvisiVisibleChangedEvent;

        public virtual void OnIsAvvisiVisibleChangedEvent()
        {
            IsAvvisiVisibleChangedEvent?.Invoke();
        }

        public AlarmsViewModel()
        {
            Init();
        }

        public AlarmsViewModel(ApplicationSettings options)
        {
            //_logger = logger;
            _options = options;

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
            
            //GestioneImpostazioniGenerali.GetInstance.RecuperaImpostazioniDaFile();
            //ApplicaImpostazioniGenerali();
            //ImpostazioniGenerali.Instance.ResetModified();

            //NSottoscrizioniaImmagineDaDiaEvent = 0;

            RabbitMqConn.GetInstance.DatiLamieraEvent -= ConnessoneRabbitMq_DataFromDiaEvent;
            // Registrazione agli eventi di arrivo immagini
            //RabbitMqConn.GetInstance.ImmagineDaDiaEvent -= ConnessoneRabbitMq_DataFromDiaEvent;
            //RabbitMqConn.GetInstance.ImmagineDaDiaEvent += ConnessoneRabbitMq_DataFromDiaEvent;

            //RabbitMqConn.GetInstance.ImmagineDaDipEvent -= ConnessoneRabbitMq_ImmagineDaDipEvent;
            //RabbitMqConn.GetInstance.ImmagineDaDipEvent += ConnessoneRabbitMq_ImmagineDaDipEvent;

            //RabbitMqConn.GetInstance.ResetImmaginiEvent -= ConnessoneRabbitMq_ResetImmaginiEvent;
            //RabbitMqConn.GetInstance.ResetImmaginiEvent += ConnessoneRabbitMq_ResetImmaginiEvent;

            InitTimerCheckIsNewData();

            System.Threading.Thread thread = System.Threading.Thread.CurrentThread;
            int threadId = GetCurrentThreadId();

            
            _logger.Info($"AnalisiBilletteViewModel Init.");
        }

        public void InitTimerCheckIsNewData()
        {
            timerCheckStatusDev.Tick += new EventHandler(timerCheckStatusDev_Tick);
            timerCheckStatusDev.Interval = TimeSpan.FromMilliseconds(1000); // Il tempo oltre il quale, se non arriva alcuna immagine, si mostra l'immagine vuota.
            _logger.Info("timerVerificaSeStannoArrivandoImmagini: inizializzato.");
        }

        private void timerCheckStatusDev_Tick(object sender, EventArgs e)
        {

            try
            {
                if (TEMPO_MASSIMO_ATTESA_DATI_SEC > 0)
                {
                    double attesaMassimaSec = TEMPO_MASSIMO_ATTESA_DATI_SEC;

                    TimeSpan delayData = (DateTime.Now - LastDataReceived);
                    if (delayData.TotalSeconds > attesaMassimaSec)
                    {
                        _logger.Warn($"Non stanno arrivando dati da più di {attesaMassimaSec} secondi.");

                        mainWindowVMParent.AreDatiInAcquisizione = false;
                    }
                }
                else
                {
                    //mainWindowVMParent.AreDatiInAcquisizione = false;
                }

            }
            catch (Exception ex)
            {
                _logger.Error("timerCheckStatusDev_Tick Error: " + ex.Message);
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

        #region DA MAIN VIEW
        public string MsgStatoRabbitMq => mainWindowVMParent.MsgStatoRabbitMq;
        public string MsgStatoPlc => mainWindowVMParent.MsgStatoPlc;
        //public string MsgStatoAccensioneIlluminatori => mainWindowVMParent.MsgStatoAccensioneIlluminatori;
        public string MsgStatoAccensioneTelecamere => mainWindowVMParent.MsgStatoAccensioneTelecamere;
        //public string MsgStatoRemota =>  (MsgStatoAccensioneTelecamere == null ? "" : ("\n" + MsgStatoAccensioneTelecamere));
        public string MsgStatoConnessioneTelecamere => mainWindowVMParent.MsgStatoConnessioneTelecamere;
        public string MsgStatoTelecamere => mainWindowVMParent.MsgStatoTelecamere;
        public string MsgStatoDipSystem => mainWindowVMParent.MsgStatoDipSystem;
        #endregion

        #region TO DO
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
        public SemaforoColor SemaforoRabbitMq => StatoRabbitMq.SemaforoColore;

        // Comunicazione con PLC (Rosso = non va , Verde = va)
        public SemaforoColor SemaforoPlc => StatoRabbitMq.IsRabbitMqAlive ? StatoServizioAutomazione.SemaforoColore : SemaforoColor.Grigio;

        // Stato connessione telecamere (Rosso = servizio Dia fermo, giallo = Il Dia va ma la Tlc non è connessa (magari perché è spenta))
        public SemaforoColor SemaforoStatoTelecamera1 => StatoRabbitMq.IsRabbitMqAlive ? StatoConnessioneTelecamere.SemaforoColorePerTlc(1) : SemaforoColor.Grigio;
        public SemaforoColor SemaforoStatoTelecamera2 => StatoRabbitMq.IsRabbitMqAlive ? StatoConnessioneTelecamere.SemaforoColorePerTlc(2) : SemaforoColor.Grigio;

        // Stato Servizio Dip (Rosso = servizio fermo, Verde = Halcon engine  & procedure caricate)
        public SemaforoColor SemaforoDipSystem => StatoRabbitMq.IsRabbitMqAlive ? StatoSistemaDip.SemaforoColore : SemaforoColor.Grigio;

        //public bool NoChangeSelectionBilletta { get; set; } = false;

        public string AvvisiTotale => StringUtils.JoinFilter("\n", new string[] { AvvisiGenerali, AvvisiPerScansione });

        public string AvvisiGenerali => StringUtils.JoinFilter("\n", new string[] { MsgStatoRabbitMq, MsgStatoPlc, MsgStatoTelecamere, MsgStatoDipSystem });

        public string AvvisiPerScansione => ""; // StringUtils.JoinFilter("\n", new string[] { AvvisoAnomalieValidazione});

        public string AvvisiTotalePerBarraMessaggi => AvvisiTotale.Replace('\n', ' ');

        public bool IsAvvisiVisibile => CiSonoAnomalieGenerali; // || (CiSonoAnomalieValidazionePerScansioneSelezionata);

        public bool CiSonoAnomalie => !string.IsNullOrWhiteSpace(AvvisiTotale);

        public bool CiSonoAnomalieGenerali => !string.IsNullOrWhiteSpace(AvvisiGenerali);
        #endregion

        public void RefreshAvvisi()
        {
            OnPropertyChanged(nameof(IsAvvisiVisibile));
            OnIsAvvisiVisibleChangedEvent();
        }

        private DateTime lastDataReceived;

        public DateTime LastDataReceived
        {
            get { return lastDataReceived; }

            set
            {
                lastDataReceived = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LabeDataOralLastData));
            }
        }

        public string LabeDataOralLastData => "Ultima acquisizione: " + LastDataReceived.ToString("HH:mm:ss");

        private void ConnessoneRabbitMq_DataFromDiaEvent(object sender, DatiLamieraEventArgs e)
        {

            Application.Current.Dispatcher.Invoke((Action)delegate
            {

                //_logger.Info("ConnessoneRabbitMq_DataFromDiaEvent: DatiLamieraIntegrationEvent: " + e..ToString());

                System.Threading.Thread thread = System.Threading.Thread.CurrentThread;
                int threadId = GetCurrentThreadId();

                //_logger.LogTrace($"ConnessoneRabbitMq_ImmagineDaDiaEvent Thread: [{thread?.ManagedThreadId}], Current Thread Id: [{threadId}]"); // TODO ... CANCELLARE..??

                try
                {
                    DatiLamieraIntegrationEvent iie = e.DatiLamiera;// IntegrationEvent;
                    if (e != null)
                    {
                        //FrameRateFromImg = e.CurrentFrameRate;
                        //LastDataReceived = DateTime.Now;
                        //if (iie.DeviceName.Equals("Dalsa_1"))
                        //    LastDataReceived = DateTime.Now;
                        ////else if (iie.DeviceName.Equals("Dalsa_2"))
                        ////    LastImage2Received = DateTime.Now;
                        ////LastImageIsModalitaAutomatica = iie.ModalitaAutomatica;
                        //_logger.Info("ConnessoneRabbitMq_ImmagineDaDiaEvent: ImageIntegrationEvent: iie.ModalitaAutomatica [" + iie.ModalitaAutomatica.ToString() + "]");
                        //EnableTimerVerificaSeStannoArrivandoImmagini(true);
                        //this.SetImmagine(iie.DeviceName, iie.FrameNumber, iie.ModalitaAutomatica, iie.ImageWidth, iie.ImageHeight, iie.ImageBytes, daDia: true);  //, flDemosaic:true); // Il demosaico lo facciamo nel Dia
                    }
                    else
                    {
                        _logger.Warn($"ConnessoneRabbitMq_ImmagineDaDiaEvent: e.ImageIntegrationEvent is null!");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("ConnessoneRabbitMq_ImmagineDaDiaEvent - Errore: " + ex.Message);
                }

                _logger.Info($"ConnessoneRabbitMq_ImmagineDaDiaEvent.");

            });
        }

        #region GESTIONE IMMAGINI (se sono presenti presenti telecamere)
        //private const int N_IMMGINI_IN_BUFFER_MAX_ALARM = 10; // TODO parametrizzare nelle impostazioni?


        //int NSottoscrizioniaImmagineDaDiaEvent = 0;

        //public float FrameRateFromImg { get; set; }

        //public double? FramePeriodSec => FrameRateFromImg == 0 ? FramePeriodSec : (1 / FrameRateFromImg);
        //public double? FramePeriodSec => FrameRateFromImg == 0 ? null : (1 / FrameRateFromImg);

        //public const int attesaPeriodiMaxImmagineSuccessiva = 10; // Numero di perodi massimi dopo i quali dichiarare interrotta l'acquisizione delle immagini se non ne arrivano più // TODO parametrizzare

        #endregion

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
