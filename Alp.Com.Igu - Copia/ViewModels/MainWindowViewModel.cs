using AlpTlc.Connessione;
using AlpTlc.App.Igu.Core;
//using AlpTlc.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AlpTlc.Domain.Impostazioni;
using AlpTlc.App.Igu;
using AlpTlc.Biz;
using AlpTlc.Biz.Core;
using AlpTlc.Connessione.Broker;
using AlpTlc.Connessione.Broker.RabbitMq;
//using AlpTlc.Connessione.Db.DbDifetti;
//using AlpTlc.Biz.RemotaInOut;
using System.Threading;
using AlpTlc.Domain.StatoMacchina;
using AlpTlc.Connessione.SettingsFile;
using System.Threading.Tasks;
using AlpTlc.Biz.Strumenti;
using AlpTlc.Biz.RemotaInOut;
using AlpTlc.Connessione.WebAPI.AppSettings;
using Newtonsoft.Json.Linq;
using Alp.Com.Igu.Core;
using Alp.Com.Igu.DataTypes;
using Alp.Com.Igu.Connections;

namespace Alp.Com.Igu.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {

        private static int _n_istanze = 0;
        private static Int32 auto_watchdog_value = 0;

        private System.Timers.Timer _checkRemoteInOutTimer;
        private System.Timers.Timer _checkRabbitMqStatusTimer;
        private System.Timers.Timer _checkPlcStatusTimer;
        private System.Timers.Timer _checkCamStatusTimer;
        private System.Timers.Timer _checkDipStatusTimer;
        private System.Timers.Timer _checkDatabaseAndFolderReachableTimer;

        public string Titolo { get; set; }

        private readonly AnalisiBilletteViewModel _analisiBilletteVM;
        private readonly ImpostazioniViewModel _impostazioniVM;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly ApplicationSettings _options;

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand DiscoveryViewCommand { get; set; }
        public RelayCommand ChangeViewCommand { get; set; }
        public RelayCommand OpenHelpCommand { get; set; }

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

        private async void OnCheckRemoteInOutEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                Esito esitoIllum = await RemotaInOutAzioni.GetInstance().VerificaEAggiornaStatoIlluminatoriAsync();

                if (!esitoIllum.Ok)
                {
                    //_logger.LogError(esito.Eccezione, GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + esito.Titolo + ": " + esito.Messaggio);
                    _logger.LogError(esitoIllum.Eccezione, esitoIllum.ToString());
                    MsgStatoAccensioneIlluminatori = esitoIllum.Titolo + ".";  // "ATTENZIONE! " + esito.Titolo + ".";
                }
                else if (esitoIllum.Icona == System.Drawing.SystemIcons.Exclamation)
                {
                    _logger.LogWarning(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + esitoIllum.Titolo + ": " + esitoIllum.Messaggio);
                    MsgStatoAccensioneIlluminatori = esitoIllum.Messaggio + ".";  // "ATTENZIONE! " + esito.Messaggio + ".";
                }
                else
                {
                    MsgStatoAccensioneIlluminatori = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + ex.Message + " - " + ex.StackTrace);
                MsgStatoAccensioneIlluminatori = "Errore remota I/O durante verifica stato accensione illuminatori";
            }

            try
            {

                Esito esitoTlc = await RemotaInOutAzioni.GetInstance().VerificaEAggiornaStatoTelecamereAsync();

                if (!esitoTlc.Ok)
                {
                    _logger.LogError(esitoTlc.Eccezione, esitoTlc.ToString());
                    MsgStatoAccensioneTelecamere = esitoTlc.Titolo + ".";
                }
                else if (esitoTlc.Icona == System.Drawing.SystemIcons.Exclamation)
                {
                    _logger.LogWarning(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + esitoTlc.Titolo + ": " + esitoTlc.Messaggio);
                    MsgStatoAccensioneTelecamere = esitoTlc.Messaggio + ".";
                }
                else
                {
                    MsgStatoAccensioneTelecamere = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + ex.Message + " - " + ex.StackTrace);
                MsgStatoAccensioneTelecamere = "Errore remota I/O durante verifica stato accensione telecamere";
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
                        _logger.LogTrace(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - Anomalia stato connessione con RabbitMq: " + StatoRabbitMq.MsgAnomalia);// RIESUMARE..?
                        MsgStatoRabbitMq = StatoRabbitMq.MsgAnomalia;
                    }
                    else
                    {
                        _logger.LogTrace(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - Stato connessione con RabbitMq ok.");
                        MsgStatoRabbitMq = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + ex.Message + " - " + ex.StackTrace);
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
                _logger.LogError(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + ex.Message + " - " + ex.StackTrace);
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
                    _logger.LogTrace(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - Anomalia stato connessione con PLC: " + StatoServizioAutomazione.MsgAnomalia);// RIESUMARE..?
                    MsgStatoPlc = StatoServizioAutomazione.MsgAnomalia;
                    AutomationDontTouch.DoTouch(); //Per forzare il riavvio del KSociety.Com (purché l'IP sia raggiungibile)
                }
                else
                {
                    _logger.LogTrace(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - Stato connessione con PLC ok.");
                    MsgStatoPlc = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + ex.Message + " - " + ex.StackTrace);
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
                    _logger.LogTrace(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - Anomalia stato connessione telecamere: " + StatoConnessioneTelecamere.MsgAnomalia);
                    MsgStatoConnessioneTelecamere = StatoConnessioneTelecamere.MsgAnomalia;
                }
                else
                {
                    _logger.LogTrace(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - Stato connessione telecamere ok.");
                    MsgStatoConnessioneTelecamere = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + ex.Message + " - " + ex.StackTrace);
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
                    _logger.LogTrace(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - Anomalia stato sistema di analisi immagini: " + StatoSistemaDip.MsgAnomalia);// RIESUMARE..?
                    MsgStatoDipSystem = StatoSistemaDip.MsgAnomalia;
                }
                else
                {
                    _logger.LogTrace(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - Stato sistema di analisi immagini ok.");
                    MsgStatoDipSystem = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod()?.Name + " - " + ex.Message + " - " + ex.StackTrace);
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
                _logger.LogError(ex, "MainWindowViewModel: Errore: Database non raggiungibile.");
            }
            #endregion TEST CONNESSIONE DB

            #region TEST RAGGIUNGIMENTO FILESYSTEM PER LETTURA E SALVATAGGIO IMMAGINI

            //_azioniRadice.VerificaCartellaImmaginiRaggiungibile();

            #endregion TEST RAGGIUNGIMENTO FILESYSTEM PER LETTURA E SALVATAGGIO IMMAGINI

        }

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                if (_currentView is ImpostazioniViewModel)
                {
                    (_currentView as ImpostazioniViewModel).AssengnaNuoveImpostazioni();

                    if (ImpostazioniDia.Instance.AreSomeValuesModified)
                    {

                        //GestioneImpostazioni<ImpostazioniDia, ImpostazioneDia>.GetInstance.RegistraImpostazioniSuFile(ApplicationSettingsStatic.PercorsoFileImpostazioniDia);
                        // Non va. Facciamo così:
                        //_ = GestioneImpostazioniDia.GetInstance.RegistraImpostazioniSuFileAsync(ApplicationSettingsStatic.PercorsoFileImpostazioniDia);

                        // Meglio sincrono:
                        GestioneImpostazioniDia.GetInstance.RegistraImpostazioniSuFile();

                        _logger.LogInformation("Impostazoni Dia modificate.");
                        // Notifica al Dia
                        RabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniDia", "True");
                        ImpostazioniDia.Instance.ResetModified();
                        RabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniDia", "False");

                    }

                    if (ImpostazioniGenerali.Instance.AreSomeValuesModified)
                    {

                        GestioneImpostazioniGenerali.GetInstance.RegistraImpostazioniSuFile();

                        _logger.LogInformation("Impostazoni Generali modificate.");
                        (value as AnalisiBilletteViewModel).ApplicaImpostazioniGenerali();
                        // Notifica al Dia
                        RabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniGenerali", "True");
                        ImpostazioniGenerali.Instance.ResetModified();
                        RabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniGenerali", "False");

                    }
                    else if(!ImpostazioniGenerali.Instance.ModalitaAutomatica)
                    {
                        // Se era già in modalità manuale, riprendi ad acquisire:
                        RabbitMqConn.SendTagInvoke("FromIgu", "StartAcq", "True");
                    }

                }
                else if (_currentView is AnalisiBilletteViewModel)
                {
                    // Se è in modalità manuale, sospendi l'aquisizione:
                    if (AreImmaginiInAcquisizione)
                        RabbitMqConn.SendTagInvoke("FromIgu", "StopAcq", "True");
                }

                _currentView = value;

                OnPropertyChanged();
            }
        }

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
                _analisiBilletteVM.OnPropertyChangedPerAvvisi();
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.MsgStatoRabbitMq));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.IsMsgStatoRabbitMqVisibile));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoRabbitMq));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoPlc));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoStatoTelecamera1));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoStatoTelecamera2));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoDipSystem));
                _analisiBilletteVM.RefreshAvvisi();
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
                _analisiBilletteVM.OnPropertyChangedPerAvvisi();
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.MsgStatoPlc));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.IsMsgStatoPlcVisibile));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoPlc));
                _analisiBilletteVM.RefreshAvvisi();
            }
        }

        private string msgStatoAccensioneIlluminatori = null;
        public string MsgStatoAccensioneIlluminatori
        {
            get { return msgStatoAccensioneIlluminatori; }
            set
            {
                msgStatoAccensioneIlluminatori = value;
                OnPropertyChanged();
                _analisiBilletteVM.RefreshAvvisi();
                _analisiBilletteVM.OnPropertyChangedPerAvvisi();
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.MsgStatoRemota));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.IsMsgStatoRemotaVisibile));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoRemota));
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
                _analisiBilletteVM.RefreshAvvisi();
                _analisiBilletteVM.OnPropertyChangedPerAvvisi();
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.MsgStatoRemota));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.IsMsgStatoRemotaVisibile));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.IsMsgStatoTelecamereVisibile));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoStatoTelecamera1));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoStatoTelecamera2));
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
                _analisiBilletteVM.OnPropertyChangedPerAvvisi();
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.MsgStatoTelecamere));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.IsMsgStatoTelecamereVisibile));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoStatoTelecamera1));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoStatoTelecamera2));
                _analisiBilletteVM.RefreshAvvisi();
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
                _analisiBilletteVM.OnPropertyChangedPerAvvisi();
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.MsgStatoDipSystem));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.IsMsgStatoDipSystemVisibile));
                _analisiBilletteVM.OnPropertyChanged(nameof(_analisiBilletteVM.SemaforoDipSystem));
                _analisiBilletteVM.RefreshAvvisi();
            }
        }

        private bool areImmaginiInAcquisizione = false;
        public bool AreImmaginiInAcquisizione
        {
            get { return areImmaginiInAcquisizione; }
            set
            {
                areImmaginiInAcquisizione = value;
                _impostazioniVM.IsFreqAcquisizioneModificabile = !value;
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

        public MainWindowViewModel(AnalisiBilletteViewModel analisiBilletteViewModel, ImpostazioniViewModel impostazioniViewModel, ILogger<MainWindowViewModel> logger, ApplicationSettings options)
        {

            _analisiBilletteVM = analisiBilletteViewModel;
            _impostazioniVM = impostazioniViewModel;

            //_logger = _loggerFactory.CreateLogger<MainWindowViewModel>();
            _logger = logger;
            _options = options;

            _logger.LogDebug($"MainWindowViewModel ctor... istanza N. {++_n_istanze}");

            RegolazioneImpostazioniAbilitata = _options.RegolazioneImpostazioniAbilitata;

            _analisiBilletteVM.mainWindowVMParent = this;
            _impostazioniVM.mainWindowVMParent = this;

            //GestioneImpostazioniGenerali.GetInstance.RecuperaImpostazioniDaFile();

            try
            {

                if (IsDesignMode)
                    Titolo = "Titolo dimostrativo";
                else
                    Titolo = " Alping Italia - Taglio a Misura Tondoni";

                CurrentView = _analisiBilletteVM;

                ChangeViewCommand = new RelayCommand(o =>
                {
                    // ... se si usano HandleCheck e HandleUnCheck, questo non serve...:
                    if (IsSettingsActive)
                    {
                        CurrentView = _analisiBilletteVM;
                        (CurrentView as AnalisiBilletteViewModel).mainWindowVMParent = this;
                    }
                    else
                    {
                        CurrentView = _impostazioniVM;
                        (CurrentView as ImpostazioniViewModel).mainWindowVMParent = this;
                    }
                });

                HomeViewCommand = new RelayCommand(o =>
                {
                    CurrentView = _analisiBilletteVM;
                });

                DiscoveryViewCommand = new RelayCommand(o =>
                {
                    CurrentView = _impostazioniVM;
                });

                OpenHelpCommand = new RelayCommand(o =>
                {
                    IsHelpPopupOpen = true;
                });

                SetCheckRemoteInOutTimer();
                SetCheckRabbitMqStatusTimer();
                SetCheckPlcStatusTimer();
                SetCheckCamStatusTimer();
                SetCheckDipStatusTimer();
                SetCheckDatabaseAndFolderReachableTimer();

                _logger.LogDebug("MainWindowViewModel ctor.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MainWindowViewModel ctor: Errore.");
            }

        }


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
                    if (_analisiBilletteVM != null) _analisiBilletteVM.IsLandscape = value;
                    if (_impostazioniVM != null) _impostazioniVM.IsLandscape = value;
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

        public void CaricaPaginaImpostazioni(object sender, RoutedEventArgs e)
        {
            CurrentView = _impostazioniVM;
        }

        public void CaricaPaginaPrincipale(object sender, RoutedEventArgs e)
        {
            CurrentView = _analisiBilletteVM;
        }

    }
}
