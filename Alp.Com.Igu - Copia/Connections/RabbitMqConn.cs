using Alp.Com.Igu.DataTypes;
//using AlpTlc.Connessione;
//using AlpTlc.Connessione.Broker.RabbitMq.Event;
//using AlpTlc.Connessione.Broker.RabbitMq.Event.Com;
//using AlpTlc.Connessione.Broker.RabbitMq.ProtoModel;
//using AlpTlc.Domain.Impostazioni;
//using AlpTlc.Domain.StatoMacchina;
using Crs.Base.RabbitParsingHelper;
using Crs.Base.SendReceiveRabbit;
using KSociety.Base.EventBusRabbitMQ;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Alp.Com.Igu.Connections
{
    public class RabbitMqConn
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                           ("Alp.Com.Igu.Connections.RabbiMqConn");


        private static readonly RabbitMqConn connessoneRabbitMq = new RabbitMqConn();

        //IRabbitMqPersistentConnection PersistentConnection;

        //static ConnessoneRabbitMq() { }

        public static RabbitMqConn GetInstance => connessoneRabbitMq;

        //ConnectionFactory factory = new ConnectionFactory() { HostName = ApplicationSettingsStatic.ServerIP, UserName = "KSociety", Password = "KSociety" };
        
        //IConnection connection = null;
        //tatic IModel channel = null;
        static string exchCom = "";
        //EventingBasicConsumer consumer = null;
        static int FrameNumberDia = 0;

        static string hostname;

        //int DIP_ALIVE_TIMEOUT_SEC = 5; // Dopo questo numero di secondi senza aver ricevuto un watchdog dal Dip, è da considerarsi morto.
        //public bool IsDipAlive => (DateTime.Now - DipAliveLastDateTime).TotalSeconds < DIP_ALIVE_TIMEOUT_SEC; // TODO spostare DIP_ALIVE_TIMEOUT_SEC nelle impostazioni

        public bool IsDipAlive => StatoSistemaDip.IsDipAlive;

        public event EventHandler<ImmagineDaDiaEventArgs> ImmagineDaDiaEvent;

        public event EventHandler<ImmagineDaDipEventArgs> ImmagineDaDipEvent;

        public event EventHandler ResetImmaginiEvent;

        private void OnImmagineDaDiaEvent(ImageIntegrationEvent imageIntegrationEvent)
        {
            if (ImmagineDaDiaEvent != null)
            {
                log.Info($"OnImmagineDaDiaEvent: imageIntegrationEvent [{imageIntegrationEvent}]");
                ImmagineDaDiaEvent(this, new ImmagineDaDiaEventArgs { ImageIntegrationEvent = imageIntegrationEvent });
            }
        }

        private void OnImmagineDaDipEvent(ImageProcessedIntegrationEvent imageIntegrationEvent, int nImmaginiInBuffer)
        {
            if (ImmagineDaDipEvent != null)
            {
                log.Info($"OnImmagineDaDiaEvent: imageIntegrationEvent [{imageIntegrationEvent}] nImmaginiInBuffer [{nImmaginiInBuffer}]");
                ImmagineDaDipEvent(this, new ImmagineDaDipEventArgs { ImageIntegrationEvent = imageIntegrationEvent, FrameNumberDia = nImmaginiInBuffer });
            }
        }

        private void OnResetImmagini()
        {
            if (ImmagineDaDipEvent != null)
            {
                log.Info($"OnResetImmagini");
                ResetImmaginiEvent(this, EventArgs.Empty);
            }
        }


        private RabbitMqConn()
        {
        }

        public void Init()
        {

            log.Info("Init...");

            hostname = Dns.GetHostName();

            // TODO : usare DefaultRabbitMqPersistentConnection delle librerie KSociety.Base (così si usa anche il logger...?) - vedo il Dia.
            // NB : il pacchetto KSociety.Base.EventBusRabbitMQ e la using KSociety.Base.EventBusRabbitMQ le ho preparate solo a questo scopo
            // E anche i pezzi di codice qui sotto.
            // Per ora, Igu logga sul file indicato in "WriteTo" ... "Name": "File" anziché in "Name": "RabbitMq"

            //PersistentConnection = new DefaultRabbitMqPersistentConnection(_connectionFactory, _loggerFactory);

            //var rabbitMqConnectionFactoryDia = new ConnectionFactory
            //{
            //    HostName = _mqHostNameDia,
            //    UserName = _mqUserNameDia,
            //    Password = _mqPasswordDia,
            //    AutomaticRecoveryEnabled = true,
            //    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            //    RequestedHeartbeat = TimeSpan.FromSeconds(10),
            //    DispatchConsumersAsync = true
            //};

            // TODO FINE

            Crs.Base.SendReceiveRabbit.RabbitMq RAB = new Crs.Base.SendReceiveRabbit.RabbitMq(hostname, 5672, "","");
            

            // Questo è NECESSARIO per il corretto funzionamento della serializzazione / deserializzazione!!
            // Per via dell'ereditarietà (buona parte dei tipi serializzati sono classi derivate)
            //Configuration.ProtoBufConfiguration();

            //string exch = "direct-logs";
            exchCom = "k-society_com_direct";
            string exchDia = "alping_dia_direct";
            string exchDip = "alping_dip_direct";
            string exchLog = "k-society_log_direct";

            string queue = "CodaPerIgu_" + hostname;
            string queueDia = "CodaImmagineDiaPerIgu_" + hostname;
            string queueDip = "CodaImmagineDipPerIgu_" + hostname;

            RAB.AddKeyExchange("ImageIntegrationEvent.Dalsa_1.imagelive", exchDia, queueDia, true);
            RAB.AddKeyExchange("ImageIntegrationEvent.Dalsa_2.imagelive", exchDia, queueDia, true);
            RAB.AddKeyExchange("ImageIntegrationEvent.Dalsa_1.image", exchDia, queueDia, true);
            RAB.AddKeyExchange("ImageIntegrationEvent.Dalsa_2.image", exchDia, queueDia, true);
            RAB.AddKeyExchange("CamStatusIntegrationEvent.StatusMonitor.camstatus", exchDia, queueDia, true);

            RAB.AddKeyExchange("ImageProcessedIntegrationEvent.Dalsa_1.imageprocessed", exchDip, queueDip, true);
            RAB.AddKeyExchange("ImageProcessedIntegrationEvent.Dalsa_2.imageprocessed", exchDip, queueDip, true);
            List<string> lstqueue = new List<string>();
            lstqueue.Add(queue);
            lstqueue.Add(queueDia);
            lstqueue.Add(queueDip);
            RAB.Receive(ConsumerReceived, lstqueue);

            //connection = factory.CreateConnection();
            //channel = connection.CreateModel();

            //channel.ExchangeDeclare(exchange: exchLog, type: ExchangeType.Direct, durable: false, autoDelete: true);
            //channel.ExchangeDeclare(exchange: exchCom, type: ExchangeType.Direct, durable: false, autoDelete: true);
            //channel.ExchangeDeclare(exchange: exchDia, type: ExchangeType.Direct, durable: false, autoDelete: true);
            //channel.ExchangeDeclare(exchange: exchDip, type: ExchangeType.Direct, durable: false, autoDelete: true);

            //var queueName = channel.QueueDeclare(queue: "CodaPerIgu_" + hostname).QueueName;
            //var queueImageDiaName = channel.QueueDeclare(queue: "CodaImmagineDiaPerIgu_" + hostname).QueueName;
            //var queueImageDipName = channel.QueueDeclare(queue: "CodaImmagineDipPerIgu_" + hostname).QueueName;

            //channel.QueueBind(queue: queueImageDiaName, exchange: exchDia, routingKey: "ImageIntegrationEvent.Dalsa_1.imagelive");
            //channel.QueueBind(queue: queueImageDiaName, exchange: exchDia, routingKey: "ImageIntegrationEvent.Dalsa_2.imagelive");

            //// Queste immagini sono destinate al Dip, ma le rileviamo solo per ricavare il numero di immagine emesso dal Dia e vedere se il Dip rimane indietro
            //channel.QueueBind(queue: queueImageDiaName, exchange: exchDia, routingKey: "ImageIntegrationEvent.Dalsa_1.image");
            //channel.QueueBind(queue: queueImageDiaName, exchange: exchDia, routingKey: "ImageIntegrationEvent.Dalsa_2.image");

            //channel.QueueBind(queue: queueImageDipName, exchange: exchDip, routingKey: "ImageProcessedIntegrationEvent.Dalsa_1.imageprocessed");
            //channel.QueueBind(queue: queueImageDipName, exchange: exchDip, routingKey: "ImageProcessedIntegrationEvent.Dalsa_2.imageprocessed");

            //// MB + PER NOTIFICA STATO TELECAMERE A IGU :
            //channel.QueueBind(queue: queueName, exchange: exchDia, routingKey: "CamStatusIntegrationEvent.StatusMonitor.camstatus");

            //// Intercettiamo gli invoke che arrivano dal Plc:
            //channel.QueueBind(queue: queueName, exchange: exchCom, routingKey: "TagIntegrationEvent.FromPlc.automation.invoke");

            //channel.QueueBind(queue: queueName, exchange: exchCom, routingKey: "TagWriteIntegrationEvent.ToPlc.automation.write.server"); //Intercettiamo quello che il Dia o il Dip mandano al Plc
            //// ... usiamo gli stessi messaggi e poi li smistiamo sia verso il Plc che verso l'Igu (coda CodaPerIgu) per avere il watchdog dal Dia e dal Dip e il systemready dal Dip (si potevano anche creare messaggi separati dedicati all'Igu...)

            //// Per ricevere indietro un proprio stesso messaggio (loop per verificare il funzionamento del RabbitMq):
            //channel.QueueBind(queue: queueName, exchange: exchCom, routingKey: "TagIntegrationEvent.FromIgu.igu.invoke");

            //// Per ricevere messaggi dagli altri servizi:
            //channel.QueueBind(queue: queueName, exchange: exchCom, routingKey: "TagIntegrationEvent.ToIgu.igu.invoke");

            //var consumer = new EventingBasicConsumer(channel);

            //consumer.Received += ConsumerReceived;

            //channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            //channel.BasicConsume(queue: queueImageDiaName, autoAck: true, consumer: consumer);
            //channel.BasicConsume(queue: queueImageDipName, autoAck: true, consumer: consumer);

            log.Debug("In attesa di messaggi da coda Rabbit.");
            
            log.Info("Init.");
        }

        private void ConsumerReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            string[] result = eventArgs.RoutingKey.Split('.');
            string eventName = result.Length > 1 ? result[0] : eventArgs.RoutingKey;
            string device = result.Length > 1 ? result[1] : "";
            string what = result.Length > 2 ? result[2] : "";

            // TODO RIABILITARE LOG CON CONDIZIONE
            //_logger.Verbose($"ConsumerReceived... eventArgs.RoutingKey: {eventArgs.RoutingKey} [eventName: {eventName}, device: {device}, what: {what}]");

            try
            {
                bool doProcess = false;
                if (what == "igu")
                {
                    doProcess = true;
                }
                else if (what == "automation")
                {
                    doProcess = true;
                }
                else if (what.Equals("imagelive")) // Non dovrebbe arivare mai dal Dia... sono tutte .image...
                {
                    doProcess = true;
                }
                else if (what.Equals("image"))
                {
                    //doProcess = false; // Queste sono destinate al Dip e NON vanno mostrate
                    doProcess = true; // Queste sono destinate al Dip e NON vanno mostrate, ma vogliamo vedere il numero di immagine
                }
                else if (what.Equals("imageprocessed"))
                {
                    doProcess = true;
                }
                else if (what.Equals("camstatus"))
                {
                    doProcess = true;
                }
                else if (what.Equals("framenumberdia"))
                {
                    doProcess = true;
                }

                if (doProcess)
                    ProcessEvent(eventArgs.RoutingKey, eventName, eventArgs.Body);
            }
            catch (Exception ex)
            {
                log.Error("Errore in ConsumerReceived. " + ex.Message);
            }

            //try
            //{
            //    // Even on exception we take the message off the queue.
            //    // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
            //    // For more information see: https://www.rabbitmq.com/dlx.html
            //    ConsumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error(ex, "Errore in ConsumerReceived ConsumerChannel.BasicAck");
            //}

        }

        private void ProcessEvent(string routingKey, string eventName, ReadOnlyMemory<byte> message, CancellationToken cancel = default)
        {
            // TODO RIABILITARE LOG CON CONDIZIONE
            //_logger.Verbose($"ProcessEvent... routingKey: [{routingKey}], eventName: [{eventName}]");

            // Trovare l'event type...:
            //var eventType = SubsManager.GetEventTypeByName(routingKey);
            //if (eventType is null)
            //{
            //  _logger.Debug($"ProcessEvent... eventType is null! routingKey: [{routingKey}]");
            //	return;
            //}

            //if (eventName.StartsWith("Tag")) return; // PER TEST, SE iL WATCHDOG DA' FASTIDIO. CANCELLARE
            Type eventType = ByName(eventName);
            // TODO RIABILITARE LOG CON CONDIZIONE
            //_logger.Verbose($"ProcessEvent... eventType: [{eventType}]");

            //if (eventType.Name.StartsWith("Tag")) return; // PER TEST, SE iL WATCHDOG DA' FASTIDIO. CANCELLARE
            using var ms = new MemoryStream(message.ToArray());
            var integrationEvent = Serializer.Deserialize(eventType, ms);

            switch (eventName)
            {
                case "TagWriteIntegrationEvent":

                    TagWriteIntegrationEvent twie = (TagWriteIntegrationEvent)integrationEvent;
                    // TODO RIABILITARE LOG CON CONDIZIONE
                    //_logger.Information($"ProcessEvent: TagWriteIntegrationEvent Received: GroupName[{twie.GroupName}] Name[{twie.Name}] Value[{twie.Value}]");

                    if (twie.Name.Equals("DiaWatchdog"))
                    {
                        if (int.TryParse(twie.Value, out var diaWatchDogValue))
                        {
                            StatoConnessioneTelecamere.WatchDogValue = diaWatchDogValue;
                        }
                    }

                    if (twie.Name.Equals("DipWatchdog"))
                    {
                        //DipAliveLastDateTime = DateTime.Now; // Ad uso interno
                        if (int.TryParse(twie.Value, out var dipWatchDogValue))
                        {
                            StatoSistemaDip.WatchDogValue = dipWatchDogValue;
                        }
                    }

                    if (twie.Name.Equals("DipReady"))
                    {
                        StatoSistemaDip.SystemReady = twie.Value.Equals("True");
                        StatoSistemaDip.NextUpdateInterval = TimeSpan.FromMilliseconds(1000);

                        // TODO come nel caso dello stato di connessione delle telecamere,
                        // anche qui si potrebbe usare un messaggio apposito per l'Igu, che includa anche l'intervallo di aggiornamento.
                    }


                    break;

                case "TagIntegrationEvent":
                    TagIntegrationEvent tie = (TagIntegrationEvent)integrationEvent;
                    // TODO RIABILITARE LOG CON CONDIZIONE
                    //_logger.Information($"ProcessEvent: TagIntegrationEvent Received: GroupName[{tie.GroupName}] Name[{tie.Name}] Value[{tie.Value}]");

                    if (tie.Name.Equals("RabbitMqAutoWatchdog"))
                    {
                        if (int.TryParse(tie.Value, out var rabbitMqAutoWatchdogValue))
                        {
                            StatoRabbitMq.WatchDogReceived(rabbitMqAutoWatchdogValue);
                        }
                    }

                    if (tie.Name.Equals("WatchdogPLC"))
                    {
                        if (int.TryParse(tie.Value, out var plcWatchDogValue))
                        {
                            StatoServizioAutomazione.WatchDogValue = plcWatchDogValue;
                        }
                    }

                    if (tie.Name.Equals("SnapTrigger"))
                    {
                        log.Info($"ProcessEvent: TagIntegrationEvent Received: GroupName[{tie.GroupName}] Name[{tie.Name}] Value[{tie.Value}]");
                        // TODO : migliorare per evitare il rischio che questo arrivi dopo le immagini
                        if (bool.TryParse(tie.Value, out var plcSnapTriggerValue))
                        {
                            log.Info($"ProcessEvent: TagIntegrationEvent Received: Name[{tie.Name}] Value[{tie.Value}] ValueParse [{plcSnapTriggerValue}]");
                            if (plcSnapTriggerValue)
                                OnResetImmagini();
                        }
                    }

                    break;

                case "ImageIntegrationEvent":
                    ImageIntegrationEvent iie = (ImageIntegrationEvent)integrationEvent;
                    // TODO RIABILITARE LOG CON CONDIZIONE
                    //_logger.Information($"ProcessEvent: ImageIntegrationEvent Received: [{iie}]");

                    if (iie != null)
                    {
                        if (!IsDipAlive)
                            OnImmagineDaDiaEvent(iie);
                        else
                        {
                            int iiefn = iie.FrameNumber;
                            log.Debug($"ProcessEvent: ImageIntegrationEvent Received: FrameNumber from Dia [{iiefn}]");
                            if (iiefn != Int32.MaxValue)
                                FrameNumberDia = iiefn;
                        }
                    }

                    break;

                case "ImageProcessedIntegrationEvent":
                    ImageProcessedIntegrationEvent ipie = (ImageProcessedIntegrationEvent)integrationEvent;
                    // TODO RIABILITARE LOG CON CONDIZIONE
                    //_logger.Information($"ProcessEvent: ImageProcessedIntegrationEvent Received: [{ipie}]");

                    OnImmagineDaDipEvent(ipie, FrameNumberDia);

                    break;

                case "CamStatusIntegrationEvent":
                    CamStatusIntegrationEvent csie = (CamStatusIntegrationEvent)integrationEvent;
                    // TODO RIABILITARE LOG CON CONDIZIONE
                    //_logger.Information($"ProcessEvent: CamStatusIntegrationEvent Received: [{csie}]");
                    if (csie.Anomalia) log.Warn($"Anomalia connessione telecamere: {csie.MsgAnomalia}");

                    StatoConnessioneTelecamere.CamConnected = csie.CamConnected;
                    StatoConnessioneTelecamere.NextUpdateInterval = csie.UpdateInterval;

                    break;

            }


        }

        private static Type ByName(string name)
        {

            Assembly assem = typeof(RabbitMqConn).Assembly;

            //_logger.Verbose("TYPES:");
            foreach (Type type in assem.GetTypes())
            {
                string str_type = type.FullName.ToString();
                //_logger.Verbose(str_type);
                var arr_str_type = str_type.Split('.');

                if (arr_str_type.Last().Equals(name))
                    return type;
            }
            return null;
        }

        public static void SendTagInvoke(string nomeTagGroup, string nomeTag, string valoreTag)
        {

            //if (channel == null) return;

            Tag tg = new Tag() { Name = nomeTag, Value = valoreTag, TagGroup = new TagGroup() { Name = nomeTagGroup } };
            TagValueChanged notification = new TagValueChanged() { Tag = tg, Timestamp = DateTime.Now };

            TagIntegrationEvent evento = new TagIntegrationEvent(notification.Tag.TagGroup.Name + ".igu.invoke",
                                                                 notification.Tag.TagGroup.Name,
                                                                 notification.Tag.Name,
                                                                 notification.Tag.Value,
                                                                 notification.Timestamp);

            string routingKey = evento.RoutingKey;

            using var ms = new MemoryStream();
            Serializer.Serialize(ms, evento);
            var body = ms.ToArray();

            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 1; //2 = persistent, write on disk

            channel.BasicPublish(exchCom,
                                 routingKey,
                                 true,
                                 properties,
                                 body);

            log.Info($"SendTagInvoke: routingKey [{routingKey}], nomeTag [{nomeTag}], valoreTag [{valoreTag}]");

        }

        public void ReceiveCallback(int headerValue, string message, DateTime dateTime)
        {
            // message contiene L1L2_CFG_EX
            //ParsingType type = (ParsingType)headerValue;
            //ITM_IMG_DEBUG msg = CrsSerializer.Deserialize<ITM_IMG_DEBUG>(type, message);

            //switch (msg.COMMAND)
            //{
            //    // richiesta di invio informazioni di debug al configuratore: giro quanto ricevuto al configuratore
            //    // questo posso implementarlo direttamente su configuratore
            //    case MDW_COMMAND.REQ_SEND_DEBUG:
            //        if (!PAUSE_DEBUG)
            //        {
            //            try
            //            {
                            
            //            }
            //            catch (Exception ex)
            //            {
                            
            //            }
            //        }
            //        break;
            //}
        }

    }

    public class Tag
    {
        //public Guid TagId { get; protected set; }
        public Guid Id { get; protected set; }

        public int AutomationTypeId { get; protected set; }
        //public AutomationType AutomationType { get; protected set; }

        public string Name { get; set; }

        public Guid ConnectionId { get; protected set; }
        //public Connection Connection { get; protected set; }

        public bool Enable { get; protected set; }

        public string InputOutput { get; protected set; }
        //public InOut InOut { get; protected set; }

        public string AnalogDigitalSignal { get; protected set; }
        //public AnalogDigital AnalogDigital { get; protected set; }

        public string MemoryAddress { get; protected set; }

        public bool Invoke { get; protected set; }

        public Guid TagGroupId { get; protected set; }
        public TagGroup TagGroup { get; set; }

        public string Value { get; set; } = string.Empty;

        public string OldValue { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

    }

    public class TagGroup
    {
        //public Guid TagGroupId { get; private set; }
        public Guid Id { get; private set; }

        public string Name { get; set; }

        public int Clock { get; private set; }

        public int Update { get; private set; }

        public bool Enable { get; private set; }

        //public virtual ICollection<Tag> Tags { get; private set; } = new List<Tag>();
    }

    class SetTagValue
    {
        [ProtoMember(1)]
        public string GroupName { get; set; }

        [ProtoMember(2)]
        public string TagName { get; set; }

        [ProtoMember(3)]
        public string Value { get; set; }

        public SetTagValue()
        {

        }

        public SetTagValue(
            string groupName,
            string tagName,
            string value)
        {
            GroupName = groupName;
            TagName = tagName;
            Value = value;
        }
    }

    public class TagValueChanged
    {
        public Tag Tag { get; set; }
        public string Name { get; }
        public string Value { get; }
        public DateTime Timestamp { get; set; }
    }

    public class ImmagineDaDiaEventArgs : EventArgs
    {
        public ImageIntegrationEvent ImageIntegrationEvent { get; set; }
    }

    public class ImmagineDaDipEventArgs : EventArgs
    {
        public ImageProcessedIntegrationEvent ImageIntegrationEvent { get; set; }
        public int FrameNumberDia { get; set; }
    }
}
