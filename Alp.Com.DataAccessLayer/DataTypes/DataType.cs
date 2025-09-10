using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;


namespace Alp.Com.DataAccessLayer.DataTypes
{
    public class ApplicationSettings
    {

        public string PercorsoFileImpostazioniGenerali { get; set; }

        public string PercorsoFileImpostazioniDia { get; set; }

        public bool RegolazioneImpostazioniAbilitata { get; set; }

        public string AutomationDontTouchFile { get; set; }

        public string ServerIP { get; set; }

        public LimitiImpostazioni LimitiImpostazioni { get; set; }

    }
    public class ApplicationSettingsStatic
    {

        public static string? PercorsoFileImpostazioniGenerali { get; set; }

        public static string? PercorsoDumpImmagineIngresso { get; set; }

        public static string? PercorsoDumpImmagineUscita { get; set; }

        public static string? PercorsoFileImpostazioniDia { get; set; }

        public static int FrameRateLive { get; set; }

        public static bool ImpostazioniAbilitate { get; set; }

        public static string? AutomationDontTouchFile { get; set; }

        public static string ServerIP { get; set; } = "localhost";

        public static ushort RitardoOnOffIlluminatoriMs { get; set; }

        public static LimitiImpostazioni? LimitiImpostazioni { get; set; }

    }

    public class LimitiImpostazioni
    {
        public decimal EsposizioneMsMin { get; set; }

        public decimal EsposizioneMsMax { get; set; }

        public int EsposizioneMsNTaccheMin { get; set; }

        public decimal GuadagnoMin { get; set; }

        public decimal GuadagnoMax { get; set; }

        public int GuadagnoNTaccheMin { get; set; }

        public decimal FreqAcquisizioneImmaginiMin { get; set; }

        public decimal FreqAcquisizioneImmaginiMax { get; set; }

        public int FreqAcquisizioneImmaginiNTaccheMin { get; set; }

    }

    public class ImpostazioneGenerale
    {
        [Key, Column(Order = 0)]
        [StringLength(50)]
        public string? Id { get; set; }

        private string? valore;

        [StringLength(255)]
        public string? Valore
        {
            get { return valore; }
            set
            {
                if (valore == null || !valore.Equals(value))
                    isModified = true;

                valore = value;
            }
        }

        private bool isModified = false;
        public bool IsModified
        {
            get { return isModified; }
        }

        public bool Persistente { get; set; }

        //public ImpostazioneGenerale()
        //{
        //    Id = string.Empty;
        //    Valore = string.Empty;
        //}

    }

    public enum SemaforoColor
    {
        Verde,
        Blu,
        Giallo,
        Rosso,
        Grigio
    }

    public static class StatoServizioAutomazione
    {
        private static int ALIVE_TIMEOUT_SEC = 5; // Dopo questo numero di secondi senza aver ricevuto un watchdog, è da considerarsi morto. // TODO spostarlo nelle impostazioni

        private static int watchDogValue;

        private static DateTime WatchDogLastLastUpdateTime { get; set; } = DateTime.MinValue;

        public static int WatchDogValue
        {
            get => watchDogValue;
            set
            {
                if (value != watchDogValue)
                    watchDogValue = value;
                WatchDogLastLastUpdateTime = DateTime.Now;
            }
        }

        public static bool IsKSocietyComAlive => (DateTime.Now - WatchDogLastLastUpdateTime).TotalSeconds < ALIVE_TIMEOUT_SEC;

        public static bool Anomalia => !IsKSocietyComAlive;

        public static string MsgAnomalia => !IsKSocietyComAlive ? "La connessione con il PLC è interrotta." : "";

        public static SemaforoColor SemaforoColore => IsKSocietyComAlive ? SemaforoColor.Verde : SemaforoColor.Rosso;

    }

    public static class StatoAcquisizione
    {
        public static bool Running { get; set; }

        public static bool Aborted { get; set; }

    }

    public static class StatoRabbitMq
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                           (" Alp.Com.Igu.DataTypes.StatoRabbitMq");

        private static int ALIVE_TIMEOUT_SEC = 5; // Dopo questo numero di secondi senza aver ricevuto un watchdog dal RabbitMq, è da considerarsi morto.
        private static int ALERT_LOOP_TIME_MS = 2000; // Tempo di viaggio (andata e ritorno) di un messaggio atraverso il RabbitMq, oltre il quale il log da info diventa warning (TODO: fare qualcosa in interfaccia utente...?)

        private static int watchDogValue;
        private static bool watchDogStarted = false;

        private static DateTime WatchDogLastLastUpdateTime { get; set; } = DateTime.MinValue;

        public static Queue<(int, DateTime)> QWatchDog { get; set; } = new();

        public static int WatchDogValue
        {
            get => watchDogValue;
            set
            {
                if (value != watchDogValue)
                    watchDogValue = value;
                WatchDogLastLastUpdateTime = DateTime.Now;
            }
        }

        public static bool WatchDogStarted => watchDogStarted;

        public static TimeSpan LastIntervalSentReceived { get; set; }

        public static void WatchDogSent(int watchDogValue)
        {
            QWatchDog.Enqueue((watchDogValue, DateTime.Now));
            watchDogStarted = true;
        }

        public static void WatchDogReceived(int watchDogValue)
        {
            bool found = false;
            WatchDogLastLastUpdateTime = DateTime.Now;
            int value;
            DateTime dateTimeSent;

            try
            {

                if (QWatchDog.Count > 0)
                {
                    do
                    {
                        (value, dateTimeSent) = QWatchDog.Dequeue();
                        if (value == watchDogValue)
                        {
                            LastIntervalSentReceived = WatchDogLastLastUpdateTime - dateTimeSent;
                            found = true;

                            if (LastIntervalSentReceived.TotalMilliseconds > ALERT_LOOP_TIME_MS)
                                log.Warn($"StatoRabbitMq: WatchDogReceived: value received {watchDogValue}: loop time (ms): {LastIntervalSentReceived.TotalMilliseconds} is over alert limit {ALERT_LOOP_TIME_MS} !");
                            else
                                log.Info($"StatoRabbitMq: WatchDogReceived: value received {watchDogValue}: loop time (ms): {LastIntervalSentReceived.TotalMilliseconds}");

                        }
                        else
                        {
                            log.Warn($"StatoRabbitMq: WatchDogReceived: value received {watchDogValue} different from dequeued value {value}!");
                        }
                    } while (value != watchDogValue && QWatchDog.Count > 0);
                }

                if (!found)
                {
                    log.Warn($"StatoRabbitMq: WatchDogReceived: value received {watchDogValue} not found in queue (...already dequeued...?)!");
                }

            }
            catch (Exception ex)
            {
                log.Error($"Error in StatoRabbitMq: WatchDogReceived: value received {watchDogValue} (msg: " + ex.Message + ")");
            }
        }


        public static bool IsRabbitMqAlive => (DateTime.Now - WatchDogLastLastUpdateTime).TotalSeconds < ALIVE_TIMEOUT_SEC; // TODO spostare DIP_ALIVE_TIMEOUT_SEC nelle impostazioni

        // Mentre per il watchdog usiamo un intervallo limite fissato, per le info sulla connenssione l'intervallo atteso è incluso tra le informazioni dell'evento.

        public static bool Anomalia => !IsRabbitMqAlive;

        public static string MsgAnomalia => IsRabbitMqAlive ? "Il sistema di comunicazione RabbitMq è funzionante." : "Il servizio di comunicazione RabbitMq è fermo.";

        public static SemaforoColor SemaforoColore => IsRabbitMqAlive ? SemaforoColor.Verde : SemaforoColor.Rosso;

    }

    public static class StatoSistemaDip
    {
        private static int ALIVE_TIMEOUT_SEC = 5; // Dopo questo numero di secondi senza aver ricevuto un watchdog dal Dip, è da considerarsi morto.

        private static int watchDogValue;

        private static DateTime WatchDogLastLastUpdateTime { get; set; } = DateTime.MinValue;

        public static int WatchDogValue
        {
            get => watchDogValue;
            set
            {
                if (value != watchDogValue)
                    watchDogValue = value;
                WatchDogLastLastUpdateTime = DateTime.Now;
            }
        }

        public static bool IsDipAlive => (DateTime.Now - WatchDogLastLastUpdateTime).TotalSeconds < ALIVE_TIMEOUT_SEC; // TODO spostare DIP_ALIVE_TIMEOUT_SEC nelle impostazioni

        // Mentre per il watchdog usiamo un intervallo limite fisato, per le info sulla connenssione l'intervallo atteso è incluso tra le informazioni dell'evento.

        public static TimeSpan TolleranzaAggiornamento { get; set; } = TimeSpan.FromSeconds(2);

        public static TimeSpan? UpdateInterval { get; set; } = TimeSpan.FromSeconds(2); // Come tempo iniziale fissiamo 2 secondi.

        public static TimeSpan? NextUpdateInterval { get; set; } = null;

        public static DateTime LastUpdateTime { get; set; }

        private static bool systemReady;

        public static bool SystemReady
        {
            get
            {
                return systemReady;
            }
            set
            {
                systemReady = value;
                LastUpdateTime = DateTime.Now;
                UpdateInterval = NextUpdateInterval;
            }
        }

        public static bool AnomaliaVerificaStato => (UpdateInterval != null) && (DateTime.Now > (LastUpdateTime + UpdateInterval + TolleranzaAggiornamento));

        public static bool AnomaliaStato => !SystemReady;

        public static bool Anomalia => !IsDipAlive || AnomaliaVerificaStato || AnomaliaStato;

        public static string MsgAnomalia => !IsDipAlive ? "Il servizio per l'analisi delle immagini è fermo." : (AnomaliaVerificaStato ? "Errore verifica stato sistema analisi immagini." : (AnomaliaStato ? "Il sistema non è pronto per l'analisi delle immagini." : ""));

        public static SemaforoColor SemaforoColore => IsDipAlive ? (SystemReady ? SemaforoColor.Verde : SemaforoColor.Giallo) : SemaforoColor.Rosso;

    }

    public static class StatoConnessioneTelecamere
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                           (" Alp.Com.Igu.DataTypes.StatoConnessioneTelecamere");


        private static int ALIVE_TIMEOUT_SEC = 5; // Dopo questo numero di secondi senza aver ricevuto un watchdog dal Dia, è da considerarsi morto.

        private static int watchDogValue;

        private static DateTime WatchDogLastLastUpdateTime { get; set; } = DateTime.MinValue;

        public static int WatchDogValue
        {
            get => watchDogValue;
            set
            {
                if (value != watchDogValue)
                    watchDogValue = value;
                WatchDogLastLastUpdateTime = DateTime.Now;
            }
        }

        public static bool IsDiaAlive => (DateTime.Now - WatchDogLastLastUpdateTime).TotalSeconds < ALIVE_TIMEOUT_SEC; // TODO spostare DIA_ALIVE_TIMEOUT_SEC nelle impostazioni

        // Mentre per il watchdog usiamo un intervallo limite fisato, per le info sulla connenssione l'intervallo atteso è incluso tra le informazioni dell'evento.

        public static TimeSpan TolleranzaAggiornamento { get; set; } = TimeSpan.FromSeconds(2);

        public static TimeSpan? UpdateInterval { get; set; } = TimeSpan.FromSeconds(2); // Come tempo iniziale fissiamo 2 secondi.

        public static TimeSpan? NextUpdateInterval { get; set; } = null;

        public static DateTime LastUpdateTime { get; set; }

        private static bool[] camConnected;


        public static bool[] CamConnected
        {
            get
            {
                return camConnected;
            }
            set
            {
                camConnected = value;
                LastUpdateTime = DateTime.Now;
                UpdateInterval = NextUpdateInterval;
            }
        }

        public static bool AnomaliaVerificaStato => (UpdateInterval != null) && (DateTime.Now > (LastUpdateTime + UpdateInterval + TolleranzaAggiornamento));

        public static bool AnomaliaStato => CamConnected.Any(CC => !CC);

        public static bool Anomalia => !IsDiaAlive || AnomaliaVerificaStato || AnomaliaStato;

        public static bool AnomaliaServizio => !IsDiaAlive || AnomaliaVerificaStato;

        //public static string MsgAnomalia => !IsDiaAlive ? "Il servizio per l'acquisizione delle immagini è fermo." : (AnomaliaVerificaStato ? "Errore verifica stato connessione telecamere." : (AnomaliaStato ? string.Join("\n", CamConnected.Where(CC => !CC).Select((CC, index) => $"Telecamera {index + 1} non connessa.").ToArray()) : ""));

        public static string MsgAnomalia => !IsDiaAlive ? "Il servizio per l'acquisizione delle immagini è fermo." : (AnomaliaVerificaStato ? "Errore verifica stato connessione telecamere." : (AnomaliaStato ? string.Join("\n", CamConnected.Select((valore, indice) => new { indice, valore }).Where(CC => !CC.valore).Select(CC => $"Telecamera {CC.indice + 1} non connessa.").ToArray()) : ""));

        public static SemaforoColor[] SemaforoColore
        {
            get
            {
                SemaforoColor[] v = new SemaforoColor[CamConnected?.Length ?? 0];
                for (int i = 0; i < v.Length; i++)
                    v[i] = (IsDiaAlive ? (CamConnected[i] ? SemaforoColor.Verde : SemaforoColor.Giallo) : SemaforoColor.Rosso);
                return v;
            }
        }

        public static SemaforoColor SemaforoColorePerTlc(int idxTlc)
        {
            SemaforoColor sc;
            int i = idxTlc - 1;

            if (AnomaliaServizio)
            {
                sc = SemaforoColor.Rosso;
            }
            else if ((CamConnected?.Length ?? 0) > i)
            {
                sc = (CamConnected[i] ? SemaforoColor.Verde : SemaforoColor.Giallo);
            }
            else
            {
                log.Warn($"StatoConnessioneTelecamere: SemaforoColorePerTlc: la telecamera {idxTlc} non esiste!");
                sc = SemaforoColor.Grigio;
            }

            return sc;
        }

        //Se NON arriva l'info dal Dia sullo stato delle telecamere, SemaforoGlobalColore è Rosso, altrimenti è Grigio.
        //public static SemaforoColor SemaforoGlobalColore => AnomaliaVerificaStato ? SemaforoColor.Rosso : SemaforoColor.Grigio;

    }

    [ProtoContract]
    public class ImageProcessedIntegrationEvent : KSociety.Base.EventBus.Events.IntegrationEvent
    {

        [ProtoMember(1)]
        public string DeviceName { get; set; }

        //[ProtoMember(6), CompatibilityLevel(CompatibilityLevel.Level200)]
        [ProtoMember(2)]
        public DateTime GrabDateTime { get; set; }

        [ProtoMember(3)]
        public int FrameNumber { get; set; }

        [ProtoMember(4)]
        public float CurrentFrameRate { get; set; }

        [ProtoMember(5)]
        public int ImageWidth { get; set; }

        [ProtoMember(6)]
        public int ImageHeight { get; set; }

        [ProtoMember(7)]
        public byte[] ImageBytes { get; set; }

        [ProtoMember(8)]
        public string TypeImage { get; set; }

        [ProtoMember(9)]
        public bool ModalitaAutomatica { get; set; }

        public ImageProcessedIntegrationEvent()
        {

        }

        public ImageProcessedIntegrationEvent(
            string routingKey,
            string deviceName,
            DateTime grabDateTime,
            int frameNumber,
            float currentFrameRate,
            int imageWidth,
            int imageHeight,
            byte[] imageBytes,
            string typeImage,
            bool modalitaAutomatica)
            : base(routingKey)
        {
            DeviceName = deviceName;
            GrabDateTime = grabDateTime;
            FrameNumber = frameNumber;
            CurrentFrameRate = currentFrameRate;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            ImageBytes = imageBytes;
            TypeImage = typeImage;
            ModalitaAutomatica = modalitaAutomatica;
        }
    }

    [ProtoContract]
    public class ImageIntegrationEvent : KSociety.Base.EventBus.Events.IntegrationEvent
    {

        [ProtoMember(1)]
        public string DeviceName { get; set; }

        //[ProtoMember(6), CompatibilityLevel(CompatibilityLevel.Level200)]
        [ProtoMember(2)]
        public DateTime GrabDateTime { get; set; }

        [ProtoMember(3)]
        public int FrameNumber { get; set; }

        [ProtoMember(4)]
        public float CurrentFrameRate { get; set; }

        [ProtoMember(5)]
        public int ImageWidth { get; set; }

        [ProtoMember(6)]
        public int ImageHeight { get; set; }

        [ProtoMember(7)]
        public byte[] ImageBytes { get; set; }

        [ProtoMember(8)]
        public string TypeImage { get; set; }

        [ProtoMember(9)]
        public bool ModalitaAutomatica { get; set; }

        public ImageIntegrationEvent()
        {

        }

        public ImageIntegrationEvent(
            string routingKey,
            string deviceName,
            DateTime grabDateTime,
            int frameNumber,
            float currentFrameRate,
            int imageWidth,
            int imageHeight,
            byte[] imageBytes,
            string typeImage,
            bool modalitaAutomatica)
        : base(routingKey)
        {
            DeviceName = deviceName;
            GrabDateTime = grabDateTime;
            FrameNumber = frameNumber;
            CurrentFrameRate = currentFrameRate;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            ImageBytes = imageBytes;
            TypeImage = typeImage;
            ModalitaAutomatica = modalitaAutomatica;
        }

        public override string ToString()
        {
            string str = "";
            str = "DeviceName[" + DeviceName + "] " +
                  "GrabDateTime[" + GrabDateTime + "] " +
                  "FrameNumber[" + FrameNumber + "] " +
                  "CurrentFrameRate[" + CurrentFrameRate + "] " +
                  "ImageWidth[" + ImageWidth + "] " +
                  "ImageHeight[" + ImageHeight + "]" +
                  "TypeImage[" + TypeImage + "]" +
                  "ModalitaAutomatica[" + ModalitaAutomatica + "]";
            return str;
        }
    }

    [ProtoContract]
    public class IntegrationComEvent : KSociety.Base.EventBus.Events.IntegrationEvent
    {
        public IntegrationComEvent()
        {

        }

        public IntegrationComEvent(string routingKey)
            : base(routingKey)
        {

        }
    }

    [ProtoContract]
    public class TagIntegrationEvent : IntegrationComEvent
    {
        [ProtoMember(1)]
        public string GroupName { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public string Value { get; set; }

        //[ProtoMember(4), CompatibilityLevel(CompatibilityLevel.Level200)]
        [ProtoMember(4)]
        public DateTime Timestamp { get; set; }

        public TagIntegrationEvent() { }

        public TagIntegrationEvent(
            string routingKey,
            string groupName,
            string name,
            string value,
            DateTime timestamp
        )
            : base(routingKey)
        {
            GroupName = groupName;
            Name = name;
            Value = value;
            Timestamp = timestamp;
        }
    }

    [ProtoContract]
    public class IntegrationComEventRpc : KSociety.Base.EventBus.Events.IntegrationEventRpc
    {
        public IntegrationComEventRpc()
        {

        }

        public IntegrationComEventRpc(string routingKey, string replyRoutingKey)
            : base(routingKey, replyRoutingKey)
        {

        }
    }

    [ProtoContract]
    public class TagWriteIntegrationEvent : IntegrationComEventRpc
    {
        [ProtoMember(1)]
        public string GroupName { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public string Value { get; set; }

        public TagWriteIntegrationEvent() { }

        public TagWriteIntegrationEvent(
            string routingKey,
            string replyRoutingKey,
            string groupName,
            string name,
            string value
        )
            : base(routingKey, replyRoutingKey)
        {
            GroupName = groupName;
            Name = name;
            Value = value;
        }
    }

    [ProtoContract]
    public class CamStatusIntegrationEvent : KSociety.Base.EventBus.Events.IntegrationEvent
    {
        [ProtoMember(1)]
        public TimeSpan UpdateInterval { get; set; }

        [ProtoMember(2)]
        public bool[] CamConnected { get; set; }

        public CamStatusIntegrationEvent()
        {

        }

        public CamStatusIntegrationEvent(
            string routingKey,
            TimeSpan updateInterval,
            bool[] camConnected)
        : base(routingKey)
        {
            UpdateInterval = updateInterval;
            CamConnected = camConnected;
        }

        public override string ToString()
        {
            string str = "";
            str = "CamConnected[" + string.Join(", ", CamConnected) + "]";
            return str;
        }

        public bool Anomalia => CamConnected.Any(CC => !CC);

        //public string MsgAnomalia => string.Join(" ", CamConnected.Where(CC => !CC).Select((CC, index) => $"Telecamera {index + 1} non connessa.").ToArray());

        public string MsgAnomalia => string.Join(" ", CamConnected.Select((valore, indice) => new { indice, valore }).Where(CC => !CC.valore).Select(CC => $"Telecamera {CC.indice + 1} non connessa.").ToArray());

    }
}
