using Alp.Com.DataAccessLayer.DataTypes;
using Alp.Com.Igu.Connections;
using Alp.Com.Igu.Core;
using Alp.Com.Igu.Views;
using Crs.Base.CommonUtilsLibrary;
using log4net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Alp.Com.Igu.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger
                           ("Alp.Com.Igu.ViewModels.MainWindowViewModel");

        private static int _n_istanze = 0;
        private static Int32 auto_watchdog_value = 0;

        private System.Timers.Timer _checkRemoteInOutTimer;
        private System.Timers.Timer _checkRabbitMqStatusTimer;
        private System.Timers.Timer _checkPlcStatusTimer;
        private System.Timers.Timer _checkCamStatusTimer;
        private System.Timers.Timer _checkDipStatusTimer;
        private System.Timers.Timer _checkDatabaseAndFolderReachableTimer;

        private List<RemotaInOutWebApi> LST_REMOTE = new List<RemotaInOutWebApi>();
        private List<PlcInOutWebApi> LST_PLC = new List<PlcInOutWebApi>();

        public string Titolo { get; set; }

        //private readonly ProfilerViewModel _profilerVM;
        //private readonly StatoDevicesViewModel _statoDevVM;
        //private readonly ImpostazioniViewModel _impostazioniVM;
        //private readonly ILogger<MainWindowViewModel> _logger;
        private readonly ApplicationSettings _options;


        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand DiscoveryViewCommand { get; set; }
        public RelayCommand ChangeViewCommand { get; set; }
        public RelayCommand OpenHelpCommand { get; set; }
        public ICommand OpenPageProfilerCommand { get; private set; }

        private void ExecuteButtonCommand(object parameter)
        {
            // Logica quando il bottone viene cliccato
            MessageBox.Show("Bottone cliccato!");
        }

        //private bool CanExecuteButtonCommand(object parameter)
        //{
        //    // Condizione per abilitare/disabilitare il bottone
        //    return true;
        //}

        
        private object _currentPage;
        public object CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateToPageCommand { set;  get; }


        private void NavigateToPage(object pageName)
        {
            switch (pageName.ToString())
            {
                case "Home":
                    CurrentPage = new ProfilerView();
                    break;
                case "Alarms":
                    CurrentPage = new StatoDevicesView();
                    break;
                case "Settings":
                    //CurrentPage = new SettingsView();
                    break;
                case "Profile":
                    CurrentPage = new ProfilerView();
                    break;
            }
        }

        public MainWindowViewModel(ProfilerViewModel profilerViewModel
            //, ImpostazioniViewModel impostazioniViewModel
            //, ILogger<MainWindowViewModel> logger
            , ApplicationSettings options
            )
        {
            NavigateToPageCommand = new RelayCommand(NavigateToPage);
            try
            {
                //CurrentPage = new MainWindowView();

                //_profilerVM = profilerViewModel;
                //_impostazioniVM = impostazioniViewModel;

               //_logger = logger;
                _options = options;

                _logger.Debug($"MainWindowViewModel ctor... istanza N. {++_n_istanze}");

                RegolazioneImpostazioniAbilitata = _options.RegolazioneImpostazioniAbilitata;

                //_profilerVM.mainWindowVMParent = this;
                //_impostazioniVM.mainWindowVMParent = this;

                if (IsDesignMode)
                    Titolo = "Titolo dimostrativo";
                else
                    Titolo = " Alping Italia - Taglio a Misura Tondoni";

                //CurrentView = _profilerVM;

                //ChangeViewCommand = new RelayCommand(o =>
                //{
                //    // ... se si usano HandleCheck e HandleUnCheck, questo non serve...:
                //    //if (IsSettingsActive)
                //    //{
                //    //    CurrentView = _getDataDeviceVM;
                //    //    (CurrentView as AnalisiBilletteViewModel).mainWindowVMParent = this;
                //    //}
                //    //else
                //    //{
                //    //    CurrentView = _impostazioniVM;
                //    //    (CurrentView as ImpostazioniViewModel).mainWindowVMParent = this;
                //    //}
                //});

                //HomeViewCommand = new RelayCommand(o =>
                //{
                //    CurrentView = _profilerVM;
                //});

                //DiscoveryViewCommand = new RelayCommand(o =>
                //{
                //    CurrentView = _impostazioniVM;
                //});

                OpenHelpCommand = new RelayCommand(o =>
                {
                    IsHelpPopupOpen = true;
                });

                InitDevices();

                SetCheckRemoteInOutTimer();
                SetCheckRabbitMqStatusTimer();
                SetCheckPlcStatusTimer();
                SetCheckCamStatusTimer();
                SetCheckDipStatusTimer();
                SetCheckDatabaseAndFolderReachableTimer();

                _logger.Debug("MainWindowViewModel ctor.");

            }
            catch (Exception ex)
            {
                _logger.Error("MainWindowViewModel ctor: Errore: " + ex.Message);
            }
        }

      

        private bool regolazioneImpostazioniAbilitata = false;
        public bool RegolazioneImpostazioniAbilitata
        {
            get
            {
                return regolazioneImpostazioniAbilitata;
            }
            set
            {
                regolazioneImpostazioniAbilitata = value;
                OnPropertyChanged();
            }
        }

        protected void SetCheckRemoteInOutTimer()
        {
            _checkRemoteInOutTimer = new System.Timers.Timer(5000);
            _checkRemoteInOutTimer.Elapsed += OnCheckRemoteInOutEvent;
            _checkRemoteInOutTimer.AutoReset = true;
            _checkRemoteInOutTimer.Enabled = true;
        }

        protected void StopCheckRemoteInOutTimer()
        {
            _checkRemoteInOutTimer.Enabled = false;
            _checkRemoteInOutTimer.Elapsed -= OnCheckRemoteInOutEvent;
        }

        /// <summary>
        /// Init dei devices su cui voglio fare monitoraggio.I dati si trovano su appsettings.json in sezione AppSetting.
        /// Troviamo questa voci:
        /// - per devices: API_REQ_IDX_DEV__1, _2..: lita con indice dei devices
        /// - per PLC: API_REQ_IDX_PLC_1..: lista plc
        /// Il metodo SetDeviceConfig riempie le liste che saranno usate per inviare a WebApi la richiesta di stato (bool)
        /// </summary>
        private void InitDevices()
        {
            _logger.Info("Step read from config..START");
            ConfigManager ConfigurationManager = new ConfigManager("", "DeviceSettings");

            for (int i = 1; i < 10; i++)
            {
                SetDeviceConfig(ConfigurationManager, "API_REQ_IDX_DEV_", i, "DEV");
            }

            for (int i = 1; i < 10; i++)
            {
                SetDeviceConfig(ConfigurationManager, "API_REQ_IDX_PLC_", i, "PLC");
            }
        }

        private void SetDeviceConfig(ConfigManager ConfigurationManager, string lbldev, int idx, string typedev)
        {
            string namedev = string.Empty;
            string? rem1 = ConfigurationManager.GetValue(lbldev + idx.ToString());
            string name1 = (!string.IsNullOrEmpty(rem1) && rem1.Contains("/") ? rem1.Substring(rem1.IndexOf("/") + 1) : typedev + "_" + idx.ToString());
            if (!string.IsNullOrEmpty(rem1))
            {
                if (typedev.Contains("DEV")) 
                    LST_REMOTE.Add(new RemotaInOutWebApi(1, name1));
                else
                    LST_PLC.Add(new PlcInOutWebApi(1, name1));
            }
        }
        
        /// <summary>
        /// Evento che richiede stato devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnCheckRemoteInOutEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                foreach (RemotaInOutWebApi rem in this.LST_REMOTE)
                {
                    Esito esito = await rem.GetStatusEsito();
                    if (!esito.Ok)
                    {
                        _logger.Error(esito.ToString() + ": " + esito.Eccezione );
                        MsgStatoDevices = esito.Titolo + ".";  // "ATTENZIONE! " + esito.Titolo + ".";
                    }
                    else if (esito.Icona == System.Drawing.SystemIcons.Exclamation)
                    {
                        _logger.Warn(esito.Titolo + ": " + esito.Messaggio);
                        MsgStatoDevices = esito.Messaggio + ".";  // "ATTENZIONE! " + esito.Messaggio + ".";
                    }
                    else
                    {
                        MsgStatoDevices = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + " - " + ex.StackTrace);
                MsgStatoDevices = "Errore remota I/O durante verifica stato accensione illuminatori";
            }

            try
            {

                //Esito esitoTlc = await RemotaInOutWebApi.GetInstance().VerificaEAggiornaStatoTelecamereAsync();

                //if (!esitoTlc.Ok)
                //{
                //    _logger.LogError(esitoTlc.Eccezione, esitoTlc.ToString());
                //    MsgStatoAccensioneTelecamere = esitoTlc.Titolo + ".";
                //}
                //else if (esitoTlc.Icona == System.Drawing.SystemIcons.Exclamation)
                //{
                //    _logger.LogWarning(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + esitoTlc.Titolo + ": " + esitoTlc.Messaggio);
                //    MsgStatoAccensioneTelecamere = esitoTlc.Messaggio + ".";
                //}
                //else
                //{
                //    MsgStatoAccensioneTelecamere = null;
                //}
            }
            catch (Exception ex)
            {
                //_logger.LogError(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + ex.Message + " - " + ex.StackTrace);
                //MsgStatoAccensioneTelecamere = "Errore remota I/O durante verifica stato accensione telecamere";
            }
        }
        
        protected void SetCheckRabbitMqStatusTimer()
        {
            _checkRabbitMqStatusTimer = new System.Timers.Timer(3000);
            _checkRabbitMqStatusTimer.Elapsed += OnCheckRabbitMqStatusEvent;
            _checkRabbitMqStatusTimer.AutoReset = true;
            _checkRabbitMqStatusTimer.Enabled = true;
        }

        protected void StopCheckRabbitMqStatusTimer()
        {
            _checkRabbitMqStatusTimer.Enabled = false;
            _checkRabbitMqStatusTimer.Elapsed -= OnCheckRabbitMqStatusEvent;
        }

        private void OnCheckRabbitMqStatusEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (StatoRabbitMq.WatchDogStarted)
            {
                try
                {
                    if (StatoRabbitMq.Anomalia)
                    {
                        _logger.Info(" - Anomalia stato connessione con RabbitMq: " + StatoRabbitMq.MsgAnomalia);// RIESUMARE..?
                        MsgStatoRabbitMq = StatoRabbitMq.MsgAnomalia;
                    }
                    else
                    {
                        _logger.Info(" - Stato connessione con RabbitMq ok.");
                        MsgStatoRabbitMq = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(" - " + ex.Message + " - " + ex.StackTrace);
                    MsgStatoRabbitMq = "Errore durante verifica stato connessione con RabbitMq";
                }
            }

            // Dobbiamo anche mandare il messaggio da ricevere indietro!!
            SendRabbitMqAutoWatchdog();

        }

        private void SendRabbitMqAutoWatchdog()
        {
            try
            {
                // NB: Se il Rabbit è fermo, il seguente metodo dà un'eccezione e quindi la coda in StatoRabbitMq.WatchDogSent non viene riempita (bene così!).
                RabbitMqConn.SendTagInvoke("FromIgu", "RabbitMqAutoWatchdog", (auto_watchdog_value == Int32.MaxValue ? 0 : ++auto_watchdog_value).ToString());
                StatoRabbitMq.WatchDogSent(auto_watchdog_value);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + " - " + ex.StackTrace);
            }
        }

        protected void SetCheckPlcStatusTimer()
        {
            _checkPlcStatusTimer = new System.Timers.Timer(3000);
            _checkPlcStatusTimer.Elapsed += OnCheckPlcStatusEvent;
            _checkPlcStatusTimer.AutoReset = true;
            _checkPlcStatusTimer.Enabled = true;
        }

        protected void StopCheckPlcStatusTimer()
        {
            _checkPlcStatusTimer.Enabled = false;
            _checkPlcStatusTimer.Elapsed -= OnCheckPlcStatusEvent;
        }

        private void OnCheckPlcStatusEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (StatoServizioAutomazione.Anomalia)
                {
                    _logger.Info(" - Anomalia stato connessione con PLC: " + StatoServizioAutomazione.MsgAnomalia);// RIESUMARE..?
                    MsgStatoPlc = StatoServizioAutomazione.MsgAnomalia;
                    //AutomationDontTouch.DoTouch(); //Per forzare il riavvio del KSociety.Com (purché l'IP sia raggiungibile)
                }
                else
                {
                    _logger.Info(" - Stato connessione con PLC ok.");
                    MsgStatoPlc = null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + " - " + ex.StackTrace);
                MsgStatoPlc = "Errore durante verifica stato connessione con PLC";
            }
        }

        protected void SetCheckCamStatusTimer()
        {
            _checkCamStatusTimer = new System.Timers.Timer(3000);
            _checkCamStatusTimer.Elapsed += OnCheckCamStatusEvent;
            _checkCamStatusTimer.AutoReset = true;
            _checkCamStatusTimer.Enabled = true;
        }

        protected void StopCheckCamStatusTimer()
        {
            _checkCamStatusTimer.Enabled = false;
            _checkCamStatusTimer.Elapsed -= OnCheckCamStatusEvent;
        }

        private void OnCheckCamStatusEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (StatoConnessioneTelecamere.Anomalia)
                {
                    _logger.Info(" - Anomalia stato connessione telecamere: " + StatoConnessioneTelecamere.MsgAnomalia);
                    MsgStatoConnessioneTelecamere = StatoConnessioneTelecamere.MsgAnomalia;
                }
                else
                {
                    _logger.Info(" - Stato connessione telecamere ok.");
                    MsgStatoConnessioneTelecamere = null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + " - " + ex.StackTrace);
                MsgStatoConnessioneTelecamere = "Errore durante verifica stato connessione telecamere";
            }
        }

        protected void SetCheckDipStatusTimer()
        {
            _checkDipStatusTimer = new System.Timers.Timer(3000);
            _checkDipStatusTimer.Elapsed += OnCheckDipStatusEvent;
            _checkDipStatusTimer.AutoReset = true;
            _checkDipStatusTimer.Enabled = true;
        }

        protected void StopCheckDipStatusTimer()
        {
            _checkDipStatusTimer.Enabled = false;
            _checkDipStatusTimer.Elapsed -= OnCheckDipStatusEvent;
        }

        private void OnCheckDipStatusEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (StatoSistemaDip.Anomalia)
                {
                    _logger.Info(" - Anomalia stato sistema di analisi immagini: " + StatoSistemaDip.MsgAnomalia);// RIESUMARE..?
                    MsgStatoDipSystem = StatoSistemaDip.MsgAnomalia;
                }
                else
                {
                    _logger.Info(" - Stato sistema di analisi immagini ok.");
                    MsgStatoDipSystem = null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + " - " + ex.StackTrace);
                MsgStatoDipSystem = "Errore durante verifica stato sistema di analisi immagini";
            }
        }

        protected void SetCheckDatabaseAndFolderReachableTimer()
        {
            _checkDatabaseAndFolderReachableTimer = new System.Timers.Timer(6543);
            _checkDatabaseAndFolderReachableTimer.Elapsed += OnCheckDatabaseAndFolderReachableEvent;
            _checkDatabaseAndFolderReachableTimer.AutoReset = true;
            _checkDatabaseAndFolderReachableTimer.Enabled = true;
        }

        protected void StopCheckDatabaseAndFolderReachableTimer()
        {
            _checkDatabaseAndFolderReachableTimer.Enabled = false;
            _checkDatabaseAndFolderReachableTimer.Elapsed -= OnCheckDatabaseAndFolderReachableEvent;
        }

        private void OnCheckDatabaseAndFolderReachableEvent(object sender, System.Timers.ElapsedEventArgs e)
        {

            #region TEST CONNESSIONE DB
            try
            {
                //using (DbDifettiContext db = new DbDifettiContext())
                //{
                //    List<ImpostazioneIgu> listaImpostazioneIgu = db.ImpostazioneIgu.ToList();
                //    //_logger.LogTrace($"MainWindowViewModel: Test Connessione Db Ok: Lettura Impostazioni Igu: N. Elementi letti: {listaImpostazioneIgu.Count}"); // RIESUMARE..?
                //}
            }
            catch (Exception ex)
            {
                _logger.Error("MainWindowViewModel: Errore: Database non raggiungibile: " + ex.Message);
            }
            #endregion TEST CONNESSIONE DB

            #region TEST RAGGIUNGIMENTO FILESYSTEM PER LETTURA E SALVATAGGIO IMMAGINI

            //_azioniRadice.VerificaCartellaImmaginiRaggiungibile();

            #endregion TEST RAGGIUNGIMENTO FILESYSTEM PER LETTURA E SALVATAGGIO IMMAGINI

        }

        //private object _currentView;
        //public object CurrentView
        //{
        //    get { return _currentView; }
        //    set
        //    {
        //        #region OLD
        //        //if (_currentView is ImpostazioniViewModel)
        //        //{
        //        //    (_currentView as ImpostazioniViewModel).AssengnaNuoveImpostazioni();

        //        //    if (ImpostazioniDia.Instance.AreSomeValuesModified)
        //        //    {

        //        //        //GestioneImpostazioni<ImpostazioniDia, ImpostazioneDia>.GetInstance.RegistraImpostazioniSuFile(ApplicationSettingsStatic.PercorsoFileImpostazioniDia);
        //        //        // Non va. Facciamo così:
        //        //        //_ = GestioneImpostazioniDia.GetInstance.RegistraImpostazioniSuFileAsync(ApplicationSettingsStatic.PercorsoFileImpostazioniDia);

        //        //        // Meglio sincrono:
        //        //        GestioneImpostazioniDia.GetInstance.RegistraImpostazioniSuFile();

        //        //        _logger.LogInformation("Impostazoni Dia modificate.");
        //        //        // Notifica al Dia
        //        //        RabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniDia", "True");
        //        //        ImpostazioniDia.Instance.ResetModified();
        //        //        RabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniDia", "False");

        //        //    }

        //        //    if (ImpostazioniGenerali.Instance.AreSomeValuesModified)
        //        //    {

        //        //        GestioneImpostazioniGenerali.GetInstance.RegistraImpostazioniSuFile();

        //        //        _logger.LogInformation("Impostazoni Generali modificate.");
        //        //        (value as AnalisiBilletteViewModel).ApplicaImpostazioniGenerali();
        //        //        // Notifica al Dia
        //        //        RabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniGenerali", "True");
        //        //        ImpostazioniGenerali.Instance.ResetModified();
        //        //        RabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniGenerali", "False");

        //        //    }
        //        //    else if(!ImpostazioniGenerali.Instance.ModalitaAutomatica)
        //        //    {
        //        //        // Se era già in modalità manuale, riprendi ad acquisire:
        //        //        RabbitMqConn.SendTagInvoke("FromIgu", "StartAcq", "True");
        //        //    }

        //        //}
        //        //else if (_currentView is AnalisiBilletteViewModel)
        //        //{
        //        //    // Se è in modalità manuale, sospendi l'aquisizione:
        //        //    if (AreImmaginiInAcquisizione)
        //        //        RabbitMqConn.SendTagInvoke("FromIgu", "StopAcq", "True");
        //        //}
        //        #endregion

        //        if (_currentView is ProfilerViewModel)
        //        {

        //        }

        //        _currentView = value;

        //        OnPropertyChanged();
        //    }
        //}

        private bool _isSettingsActive;
        public bool IsSettingsActive
        {
            get { return _isSettingsActive; }
            set
            {
                _isSettingsActive = value;
                //if (_isSettingsActive)
                //    CurrentView = ImpostazioniVM ;
                //else
                //    CurrentView = AnalisiBilletteVM;
                OnPropertyChanged();
            }
        }


        private string msgStatoRabbitMq = null;

        public string MsgStatoRabbitMq
        {
            get { return msgStatoRabbitMq; }
            set
            {
                msgStatoRabbitMq = value;
                OnPropertyChanged();
                //_profilerVM.OnPropertyChangedPerAvvisi();
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.MsgStatoRabbitMq));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.IsMsgStatoRabbitMqVisibile));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoRabbitMq));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoPlc));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoStatoTelecamera1));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoStatoTelecamera2));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoDipSystem));
                //_profilerVM.RefreshAvvisi();
            }
        }

        private string msgStatoPlc = null;
        public string MsgStatoPlc
        {
            get { return msgStatoPlc; }
            set
            {
                msgStatoPlc = value;
                OnPropertyChanged();
                //_profilerVM.OnPropertyChangedPerAvvisi();
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.MsgStatoPlc));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.IsMsgStatoPlcVisibile));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoPlc));
                //_profilerVM.RefreshAvvisi();
            }
        }

        private string msgStatoAccensioneIlluminatori = null;
        public string MsgStatoDevices
        {
            get { return msgStatoAccensioneIlluminatori; }
            set
            {
                msgStatoAccensioneIlluminatori = value;
                OnPropertyChanged();
                //_profilerVM.RefreshAvvisi();
                //_profilerVM.OnPropertyChangedPerAvvisi();
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.MsgStatoRemota));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.IsMsgStatoRemotaVisibile));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoRemota));
            }
        }

        private string msgStatoAccensioneTelecamere = null;
        public string MsgStatoAccensioneTelecamere
        {
            get { return msgStatoAccensioneTelecamere; }
            set
            {
                msgStatoAccensioneTelecamere = value;
                OnPropertyChanged();
                //_profilerVM.RefreshAvvisi();
                //_profilerVM.OnPropertyChangedPerAvvisi();
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.MsgStatoRemota));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.IsMsgStatoRemotaVisibile));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.IsMsgStatoTelecamereVisibile));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoStatoTelecamera1));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoStatoTelecamera2));
            }
        }

        private string msgStatoConnessioneTelecamere = null;
        public string MsgStatoConnessioneTelecamere
        {
            get { return msgStatoConnessioneTelecamere; }
            set
            {
                msgStatoConnessioneTelecamere = value;
                OnPropertyChanged();
                //_profilerVM.OnPropertyChangedPerAvvisi();
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.MsgStatoTelecamere));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.IsMsgStatoTelecamereVisibile));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoStatoTelecamera1));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoStatoTelecamera2));
                //_profilerVM.RefreshAvvisi();
            }
        }

        public string MsgStatoTelecamere => MsgStatoConnessioneTelecamere == null ? "" : (MsgStatoConnessioneTelecamere + (MsgStatoAccensioneTelecamere == null ? "" : ("\n" + MsgStatoAccensioneTelecamere)));


        private string msgStatoDipSystem = null;
        public string MsgStatoDipSystem
        {
            get { return msgStatoDipSystem; }
            set
            {
                msgStatoDipSystem = value;
                OnPropertyChanged();
                //_profilerVM.OnPropertyChangedPerAvvisi();
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.MsgStatoDipSystem));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.IsMsgStatoDipSystemVisibile));
                //_profilerVM.OnPropertyChanged(nameof(_profilerVM.SemaforoDipSystem));
                //_profilerVM.RefreshAvvisi();
            }
        }

        private bool areImmaginiInAcquisizione = false;
        public bool AreImmaginiInAcquisizione
        {
            get { return areImmaginiInAcquisizione; }
            set
            {
                areImmaginiInAcquisizione = value;
                //_impostazioniVM.IsFreqAcquisizioneModificabile = !value;
                OnPropertyChanged();
            }
        }

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

        //        ImpostazioniGenerali.Instance.ModalitaAutomatica = value;

        //        if (ImpostazioniGenerali.Instance.AreSomeValuesModified)
        //        {

        //            //GestioneImpostazioni<ImpostazioniGenerali, ImpostazioneGenerale>.GetInstance.RegistraImpostazioniSuFile(ApplicationSettingsStatic.PercorsoFileImpostazioniGenerali);
        //            // Non va. Facciamo così:
        //            //_ = GestioneImpostazioniGenerali.GetInstance.RegistraImpostazioniSuFileAsync(ApplicationSettingsStatic.PercorsoFileImpostazioniGenerali);

        //            // Meglio sincrono:
        //            GestioneImpostazioniGenerali.GetInstance.RegistraImpostazioniSuFile();

        //            _logger.LogInformation("Impostazoni Generali modificate.");
        //            // Notifica al Dia
        //            ConnessoneRabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniGenerali", "True");
        //            ImpostazioniGenerali.Instance.ResetModified();
        //            ConnessoneRabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniGenerali", "False");
        //        }

        //        OnPropertyChanged();
        //        OnPropertyChanged(nameof(ModalitaManuale));
        //    }
        //}

        //public bool ModalitaManuale => !ModalitaAutomatica;

        

        public event MouseButtonEventHandler MouseUpForCanvas;

        public virtual void OnMouseUpForCanvas(MouseButtonEventArgs e)
        {
            MouseUpForCanvas?.Invoke(this, e);
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
                if (isLandscape != value)
                {
                    isLandscape = value;
                    //if (_profilerVM != null) _profilerVM.IsLandscape = value;
                    //if (_impostazioniVM != null) _impostazioniVM.IsLandscape = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsPortrait));
                }
            }
        }

        public bool IsPortrait => !IsLandscape;


        private bool isHelpPopupOpen;

        public bool IsHelpPopupOpen
        {
            get
            {
                return isHelpPopupOpen;
            }
            set
            {
                if (isHelpPopupOpen != value)
                {
                    isHelpPopupOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        //public void CaricaPaginaStati(object sender, RoutedEventArgs e)
        //{
        //    CurrentView = _statoDevVM;
        //}

        //public void CaricaPaginaPrincipale(object sender, RoutedEventArgs e)
        //{
        //    CurrentView = _profilerVM;
        //}

    }
}
