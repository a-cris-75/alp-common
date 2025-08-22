using Alp.Com.Igu.Core;
using AlpTlc.Domain.Impostazioni;
using Microsoft.Extensions.Logging;
using System;
using System.Windows.Input;
using AlpTlc.Biz.Core;
using AlpTlc.Connessione.SettingsFile;
using System.Threading.Tasks;
using AlpTlc.Biz.RemotaInOut;
using AlpTlc.Connessione.Broker.RabbitMq;
using Alp.Com.Igu.Core;
//using AlpTlc.Biz.RemotaInOut;

namespace Alp.Com.Igu.ViewModels
{
    public class ImpostazioniViewModel : ObservableObject
    {

        private static int _n_istanze = 0;

        private readonly ILogger<ImpostazioniViewModel> _logger;
        private readonly ApplicationSettings _options;

        decimal TempoEsposizioneMsMinorTickIncrement = 0.1M;

        decimal GuadagnoMinorTickIncrement = 0.1M;

        public MainWindowViewModel mainWindowVMParent { get; set; }

        static RemotaInOutAzioni _remotaInOutAzioni = RemotaInOutAzioni.GetInstance();

        public ImpostazioniViewModel(ILogger<ImpostazioniViewModel> logger, ApplicationSettings options)
        {
            _logger = logger;
            _options = options;
            Init();
        }

        private ImpostazioniViewModel()
        {
            Init();
        }

        private void Init()
        {
            _logger.LogTrace($"ImpostazioniViewModel ctor Init... istanza N. {++_n_istanze}");

            try
            {

                //GestioneImpostazioni<ImpostazioniDia, ImpostazioneDia>.GetInstance.RecuperaImpostazioniDaFile(ApplicationSettingsStatic.PercorsoFileImpostazioniDia);
                //Non va. Facciamo così:
                //
                //_ = GestioneImpostazioniDia.GetInstance.RecuperaImpostazioniDaFileAsync(ApplicationSettingsStatic.PercorsoFileImpostazioniDia);
                // Meglio sincrono:
                GestioneImpostazioniDia.GetInstance.RecuperaImpostazioniDaFile();
                RecuperaImpostazioni();

                GestioneImpostazioniGenerali.GetInstance.RecuperaImpostazioniDaFile();
                ModalitaAutomatica = ImpostazioniGenerali.Instance.ModalitaAutomatica;

                TempoEsposizioneMsMin = _options.LimitiImpostazioni.EsposizioneMsMin;
                (TempoEsposizioneMsMinorTickIncrement, TempoEsposizioneMsNMinorTicks, TempoEsposizioneMsMajorTickIncrement, TempoEsposizioneMsNMajorTicks) = TickFrequencyGrandiEPiccole(_options.LimitiImpostazioni.EsposizioneMsMax - _options.LimitiImpostazioni.EsposizioneMsMin, _options.LimitiImpostazioni.EsposizioneMsNTaccheMin);

                GuadagnoMin = _options.LimitiImpostazioni.GuadagnoMin;
                (GuadagnoMinorTickIncrement, GuadagnoNMinorTicks, GuadagnoMajorTickIncrement, GuadagnoNMajorTicks) = TickFrequencyGrandiEPiccole(_options.LimitiImpostazioni.GuadagnoMax - _options.LimitiImpostazioni.GuadagnoMin, _options.LimitiImpostazioni.GuadagnoNTaccheMin);

                FreqAcquisizioneImmaginiMin = _options.LimitiImpostazioni.FreqAcquisizioneImmaginiMin;
                FreqAcquisizioneImmaginiMax = _options.LimitiImpostazioni.FreqAcquisizioneImmaginiMax;

                (FreqAcquisizioneImmaginiTickFrequency, _) = TickFrequency(_options.LimitiImpostazioni.FreqAcquisizioneImmaginiMax - _options.LimitiImpostazioni.FreqAcquisizioneImmaginiMin, _options.LimitiImpostazioni.FreqAcquisizioneImmaginiNTaccheMin);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImpostazioniViewModel Init: Errore.");
            }

            _logger.LogTrace("ImpostazioniViewModel Init.");

        }

        private (decimal, int) TickFrequency(decimal intervallo, int NTaccheMin)
        {
            // Calcoliamo il passo preciso teorico per avere esattamente NTaccheMin:
            double passoPreciso = (double)intervallo / (NTaccheMin - 1);

            // Vogliamo diminuire il PassoPreciso fino ad arrivare ad un numero
            // che abbia come prima cifra significativa un 1, un 2 o un 5 (e nessun'altra cifra dopo di questa)

            // Troviamo l'ordine di grandezza del passoPreciso, in base 10
            // (è la più grande potenza di 10 minore del passoPreciso):
            double ordineDiGrandezza = (passoPreciso).Scala(1);

            double tickFrequency = 0;
            int suddivisioneInTacchePiccole = 0; // In quante tacche piccole è suddivisa una tacca grande

            // Potrebbe già andare bene come tickFrequency...:
            if (passoPreciso < ordineDiGrandezza * 2)
            {
                tickFrequency = ordineDiGrandezza;
                suddivisioneInTacchePiccole = 10;
            }

            // ... ma verifichiamo se si può prendere il doppio...
            if (passoPreciso >= ordineDiGrandezza * 2 && passoPreciso < ordineDiGrandezza * 5)
            {
                tickFrequency = ordineDiGrandezza * 2;
                suddivisioneInTacchePiccole = 5;
            }

            // ... oppure addirittura il quintuplo... : 
            if (passoPreciso >= ordineDiGrandezza * 5)
            {
                tickFrequency = ordineDiGrandezza * 5;
                suddivisioneInTacchePiccole = 10;
            }

            return ((decimal)tickFrequency, suddivisioneInTacchePiccole);
        }

        private (decimal, int, decimal, int) TickFrequencyGrandiEPiccole(decimal intervallo, int NTaccheMin)
        {
            (decimal tickFrequency, int suddivisioneInTacchePiccole) = TickFrequency(intervallo, NTaccheMin);

            decimal minorTickIncrement = (decimal)tickFrequency;
            int numberOfMinorTicks = suddivisioneInTacchePiccole - 1;
            decimal majorTickIncrement = (decimal)(tickFrequency * suddivisioneInTacchePiccole);
            int numberOfMajorTicks = (int)(intervallo / majorTickIncrement) + 1;

            // Se intervallo non è un multiplo di majorTickIncrement, mettiamo una tacca in più piuttosto che una in meno.
            if ((numberOfMajorTicks - 1) * majorTickIncrement < intervallo) numberOfMajorTicks++;

            return (minorTickIncrement, numberOfMinorTicks, majorTickIncrement, numberOfMajorTicks);
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

        private RelayCommand accendiIlluminatoriCommand;
        public ICommand AccendiIlluminatoriCommand => accendiIlluminatoriCommand ??= new RelayCommand(AccendiIlluminatori);
        private async void AccendiIlluminatori(object commandParameter)
        {
            _logger.LogTrace("AccendiIlluminatori...");

            Esito esito;

            try
            {

                using (new WaitCursor())
                {
                    esito = await _remotaInOutAzioni.AccendiIlluminatoriAsync(checkLock: true);
                }

                if (!esito.Ok)
                {
                    _logger.LogError(esito.Eccezione, $"AccendiIlluminatori: esito: [{esito}]");
                }
                else if (esito.Icona == System.Drawing.SystemIcons.Exclamation)
                {
                    _logger.LogWarning($"AccendiIlluminatori: esito: [{esito}]");
                }
                else
                {
                    _logger.LogInformation($"AccendiIlluminatori: esito: [{esito}]");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore accensione illuminatori.");
                esito = new Esito { Eccezione = ex, Titolo = "Errore", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            EsitoAccensioneIlluminatori = new EsitoPopup(esito); //  { PopupOpen = true };

            _logger.LogTrace("AccendiIlluminatori.");
        }

        private EsitoPopup esitoAccensioneIlluminatori;

        public EsitoPopup EsitoAccensioneIlluminatori
        {
            get => esitoAccensioneIlluminatori;
            set
            {
                esitoAccensioneIlluminatori = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand spegniIlluminatoriCommand;
        public ICommand SpegniIlluminatoriCommand => spegniIlluminatoriCommand ??= new RelayCommand(SpegniIlluminatori);
        private async void SpegniIlluminatori(object commandParameter)
        {
            _logger.LogTrace("SpegniIlluminatori...");

            Esito esito;

            try
            {

                using (new WaitCursor())
                {
                    esito = await _remotaInOutAzioni.SpegniIlluminatoriAsync(checkLock: true);
                }

                if (!esito.Ok)
                {
                    _logger.LogError(esito.Eccezione, $"SpegniIlluminatori: esito: [{esito}]");
                }
                else if (esito.Icona == System.Drawing.SystemIcons.Exclamation)
                {
                    _logger.LogWarning($"SpegniIlluminatori: esito: [{esito}]");
                }
                else
                {
                    _logger.LogInformation($"SpegniIlluminatori: esito: [{esito}]");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore spegnimento illuminatori.");
                esito = new Esito { Eccezione = ex, Titolo = "Errore", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            EsitoSpegnimentoIlluminatori = new EsitoPopup(esito); //  { PopupOpen = true };

            _logger.LogTrace("SpegniIlluminatori.");

        }

        private EsitoPopup esitoSpegnimentoIlluminatori;

        public EsitoPopup EsitoSpegnimentoIlluminatori
        {
            get => esitoSpegnimentoIlluminatori;
            set
            {
                esitoSpegnimentoIlluminatori = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand resetTelecamereCommand;
        public ICommand ResetTelecamereCommand => resetTelecamereCommand ??= new RelayCommand(ResetTelecamere);
        private async void ResetTelecamere(object commandParameter)
        {
            _logger.LogTrace("ResetTelecamere...");

            Esito esito;

            try
            {

                using (new WaitCursor())
                {
                    esito = await _remotaInOutAzioni.ResetTelecamereAsync(checkLock: true);
                }

                if (!esito.Ok)
                {
                    _logger.LogError(esito.Eccezione, $"ResetTelecamere: esito: [{esito}]");
                }
                else if (esito.Icona == System.Drawing.SystemIcons.Exclamation)
                {
                    _logger.LogWarning($"ResetTelecamere: esito: [{esito}]");
                }
                else
                {
                    _logger.LogInformation($"ResetTelecamere: esito: [{esito}]");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore reset telecamere.");
                esito = new Esito { Eccezione = ex, Titolo = "Errore", Messaggio = ex.Message, Ok = false, Icona = System.Drawing.SystemIcons.Error };
            }

            EsitoResetTelecamere = new EsitoPopup(esito); //  { PopupOpen = true };

            _logger.LogTrace("ResetTelecamere.");

        }

        private EsitoPopup esitoResetTelecamere;

        public EsitoPopup EsitoResetTelecamere
        {
            get => esitoResetTelecamere;
            set
            {
                esitoResetTelecamere = value;
                OnPropertyChanged();
            }
        }


        private decimal freqAcquisizioneImmagini = 8;
        public decimal FreqAcquisizioneImmagini
        {
            get
            {
                return freqAcquisizioneImmagini;
            }
            set
            {
                freqAcquisizioneImmagini = value;
                OnPropertyChanged();
            }
        }



        private bool isFreqAcquisizioneModificabile = true;
        public bool IsFreqAcquisizioneModificabile
        {
            get
            {
                return isFreqAcquisizioneModificabile;
            }
            set
            {
                isFreqAcquisizioneModificabile = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFreqAcquisizioneNotModificabile));
            }
        }

        public bool IsFreqAcquisizioneNotModificabile => !IsFreqAcquisizioneModificabile;

        private int tempoEsposizioneUsTlc1 = 25000;
        public decimal TempoEsposizioneMsTlc1
        {
            get
            {
                return tempoEsposizioneUsTlc1 / 1000M;
            }
            set
            {
                tempoEsposizioneUsTlc1 = (int)((int)(value / TempoEsposizioneMsMinorTickIncrement) * TempoEsposizioneMsMinorTickIncrement * 1000);
                OnPropertyChanged();
            }
        }

        private int tempoEsposizioneUsTlc2 = 25000;
        public decimal TempoEsposizioneMsTlc2
        {
            get
            {
                return tempoEsposizioneUsTlc2 / 1000M;
            }
            set
            {
                tempoEsposizioneUsTlc2 = (int)((int)(value / TempoEsposizioneMsMinorTickIncrement) * TempoEsposizioneMsMinorTickIncrement * 1000);
                OnPropertyChanged();
            }
        }

        private decimal guadagnoTlc1 = 1;
        public decimal GuadagnoTlc1
        {
            get
            {
                return guadagnoTlc1;
            }
            set
            {
                guadagnoTlc1 = Math.Round(value / GuadagnoMinorTickIncrement) * GuadagnoMinorTickIncrement;
                OnPropertyChanged();
            }
        }

        private decimal guadagnoTlc2 = 1;
        public decimal GuadagnoTlc2
        {
            get
            {
                return guadagnoTlc2;
            }
            set
            {
                guadagnoTlc2 = Math.Round(value / GuadagnoMinorTickIncrement) * GuadagnoMinorTickIncrement;
                OnPropertyChanged();
            }
        }

        private bool mostraImpostazioniDiaPerEsperti = true;
        // Viene impostato nel costrutore leggendo il valore in appsettings.
        public bool MostraImpostazioniDiaPerEsperti
        {
            get
            {
                return mostraImpostazioniDiaPerEsperti;
            }
            set
            {
                mostraImpostazioniDiaPerEsperti = value;
                OnPropertyChanged();
            }
        }

        private decimal tempoEsposizioneMsMin = 0;
        public decimal TempoEsposizioneMsMin
        {
            get
            {
                return tempoEsposizioneMsMin;
            }
            set
            {
                tempoEsposizioneMsMin = value;
                OnPropertyChanged();
            }
        }

        private int tempoEsposizioneMsNMajorTicks = 11;
        public int TempoEsposizioneMsNMajorTicks
        {
            get
            {
                return tempoEsposizioneMsNMajorTicks;
            }
            set
            {
                tempoEsposizioneMsNMajorTicks = value;
                OnPropertyChanged();
            }
        }

        private decimal tempoEsposizioneMsMajorTickIncrement = 0.5M;
        public decimal TempoEsposizioneMsMajorTickIncrement
        {
            get
            {
                return tempoEsposizioneMsMajorTickIncrement;
            }
            set
            {
                tempoEsposizioneMsMajorTickIncrement = value;
                OnPropertyChanged();
            }
        }

        private int tempoEsposizioneMsNMinorTicks = 4;
        public int TempoEsposizioneMsNMinorTicks
        {
            get
            {
                return tempoEsposizioneMsNMinorTicks;
            }
            set
            {
                tempoEsposizioneMsNMinorTicks = value;
                OnPropertyChanged();
            }
        }

        // MB

        private decimal guadagnoMin = 1;
        public decimal GuadagnoMin
        {
            get
            {
                return guadagnoMin;
            }
            set
            {
                guadagnoMin = value;
                OnPropertyChanged();
            }
        }

        private int guadagnoNMajorTicks = 11;
        public int GuadagnoNMajorTicks
        {
            get
            {
                return guadagnoNMajorTicks;
            }
            set
            {
                guadagnoNMajorTicks = value;
                OnPropertyChanged();
            }
        }

        private decimal guadagnoMajorTickIncrement = 0.5M;
        public decimal GuadagnoMajorTickIncrement
        {
            get
            {
                return guadagnoMajorTickIncrement;
            }
            set
            {
                guadagnoMajorTickIncrement = value;
                OnPropertyChanged();
            }
        }

        private int guadagnoNMinorTicks = 4;
        public int GuadagnoNMinorTicks
        {
            get
            {
                return guadagnoNMinorTicks;
            }
            set
            {
                guadagnoNMinorTicks = value;
                OnPropertyChanged();
            }
        }


        private decimal freqAcquisizioneImmaginiMin = 1;
        public decimal FreqAcquisizioneImmaginiMin
        {
            get
            {
                return freqAcquisizioneImmaginiMin;
            }
            set
            {
                freqAcquisizioneImmaginiMin = value;
                OnPropertyChanged();
            }
        }

        private decimal freqAcquisizioneImmaginiMax = 24;
        public decimal FreqAcquisizioneImmaginiMax
        {
            get
            {
                return freqAcquisizioneImmaginiMax;
            }
            set
            {
                freqAcquisizioneImmaginiMax = value;
                OnPropertyChanged();
            }
        }

        private decimal freqAcquisizioneImmaginiTickFrequency = 5.0M;

        public decimal FreqAcquisizioneImmaginiTickFrequency
        {
            get
            {
                return freqAcquisizioneImmaginiTickFrequency;
            }
            set
            {
                freqAcquisizioneImmaginiTickFrequency = value;
                OnPropertyChanged();
            }
        }

        public void RecuperaImpostazioni()
        {

            _logger.LogDebug("RecuperaImpostazioni...");

            FreqAcquisizioneImmagini = ImpostazioniDia.Instance.FreqAcquisizioneImmagini;
            TempoEsposizioneMsTlc1 = ImpostazioniDia.Instance.TempoEsposizioneMsTlc1;
            TempoEsposizioneMsTlc2 = ImpostazioniDia.Instance.TempoEsposizioneMsTlc2;
            GuadagnoTlc1 = ImpostazioniDia.Instance.GuadagnoTlc1;
            GuadagnoTlc2 = ImpostazioniDia.Instance.GuadagnoTlc2;

            _logger.LogDebug("RecuperaImpostazioni.");

        }

        public void AssengnaNuoveImpostazioni()
        {
            _logger.LogDebug("AssengnaNuoveImpostazioni...");

            ImpostazioniDia.Instance.FreqAcquisizioneImmagini = FreqAcquisizioneImmagini;
            ImpostazioniDia.Instance.TempoEsposizioneMsTlc1 = TempoEsposizioneMsTlc1;
            ImpostazioniDia.Instance.TempoEsposizioneMsTlc2 = TempoEsposizioneMsTlc2;
            ImpostazioniDia.Instance.GuadagnoTlc1 = GuadagnoTlc1;
            ImpostazioniDia.Instance.GuadagnoTlc2 = GuadagnoTlc2;

            ImpostazioniGenerali.Instance.ModalitaAutomatica = ModalitaAutomatica;

            _logger.LogDebug("AssengnaNuoveImpostazioni.");

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

                //ImpostazioniGenerali.Instance.ModalitaAutomatica = value;

                //if (ImpostazioniGenerali.Instance.AreSomeValuesModified)
                //{

                //    GestioneImpostazioniGenerali.GetInstance.RegistraImpostazioniSuFile();

                //    _logger.LogInformation("Impostazoni Generali modificate.");
                //    // Notifica al Dia
                //    ConnessoneRabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniGenerali", "True");
                //    ImpostazioniGenerali.Instance.ResetModified();
                //    ConnessoneRabbitMq.SendTagInvoke("FromIgu", "AggiornamentoImpostazioniGenerali", "False");
                //}

                OnPropertyChanged();
                OnPropertyChanged(nameof(ModalitaManuale));
            }
        }

        public bool ModalitaManuale => !ModalitaAutomatica;

    }

    static class ImpostazioniViewModelExtensions
    {
        public static double TruncateToSignificantDigits(this double d, int digits)
        {
            if (d == 0) return 0;
            double scale = d.Scala(digits);
            return scale * Math.Truncate(d / scale);
        }

        public static double Scala(this double d, int digits)
        {
            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1 - digits);
            return scale;
        }
    }
}
