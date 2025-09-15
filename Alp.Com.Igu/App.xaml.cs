using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using System.Drawing;
//using AlpTlc.Biz;
//using AlpTlc.App.Igu;
//using AlpTlc.App.Igu.ViewModels;
//using AlpTlc.Connessione.Broker.RabbitMq;
//using AlpTlc.Domain.Impostazioni;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Markup;
using Alp.Com.DataAccessLayer.DataTypes;
using Alp.Com.Igu.ViewModels;
using Alp.Com.Igu.Connections;
//using AlpTlc.Biz.Strumenti;

namespace Alp.Com.Igu
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public IConfiguration Config { get; private set; }

        //public IServiceProvider ServiceProvider { get; private set; }

        private readonly IHost _host;
        private readonly IConfiguration _configuration;

        private readonly bool stoppingApp = false;

        public static IServiceProvider AppServiceProvider { get; private set; } // MB per risolvere i servizi da qualunque punto del progetto (TODO...SERVE??)

        public App()
        {
            // Eccezioni non gestite. Vedere
            // https://stackoverflow.com/questions/1472498/wpf-global-exception-handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(AppDomain_CurrentDomain_UnhandledExceptionEventHandler);
            //this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                //.AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // NB: Se il servizio RabbitMQ non è in esecuzione, alla riga qui sotto il programma resta piantato indefinitamente:
            // TODO: evitare questa cosa

            // TODO: Per ora, Igu logga sul file indicato in "WriteTo" ... "Name": "File" anziché in "Name": "RabbitMq" dentro appsettings.json; occorrerebbe stabilire un'apposita connessione Rabbit, vedo il TODO nel progetto AlpTlc.Connessione.Broker

            // Per questo serve il pacchetto KSociety.Log.Serilog.Sinks.RabbitMq
            //Serilog.Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_configuration).CreateLogger();
            //Serilog.Log.ForContext(typeof(App)).Information("App ctor (Igu init main)");
            //Serilog.Log.ForContext(typeof(App)).Information("Cultura corrente: Thread.CurrentThread.CurrentCulture [" + Thread.CurrentThread.CurrentCulture + "]");
            //Serilog.Log.ForContext(typeof(App)).Information("Cultura corrente: Thread.CurrentThread.CurrentUICulture [" + Thread.CurrentThread.CurrentUICulture + "]");
            //Serilog.Log.ForContext(typeof(App)).Information("Cultura corrente: CultureInfo.DefaultThreadCurrentUICulture [" + CultureInfo.DefaultThreadCurrentUICulture + "]");
            //Serilog.Log.ForContext(typeof(App)).Information("Cultura corrente: CultureInfo.CurrentCulture [" + CultureInfo.CurrentCulture + "]");
            //Serilog.Log.ForContext(typeof(App)).Information("Cultura corrente: CultureInfo.CurrentUICulture [" + CultureInfo.CurrentUICulture + "]");

            #region Evita l'apertura dell'applicazione più volte

            string thisprocessname = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
            {
                stoppingApp = true;
                //Views.AvvisoView avvisoView = new Views.AvvisoView($"L'applicazione {thisprocessname} è già attiva.{Environment.NewLine}Non è possibile aprirne più istanze nella stessa macchina.");
                Views.AvvisoView avvisoView = new();
                avvisoView.vm.TestoAvvisoGenerale = $"L'applicazione {thisprocessname} è già attiva.{Environment.NewLine}Non è possibile aprirne più istanze nella stessa macchina.";
                avvisoView.ShowDialog();
                //MessageBox.Show($"L'applicazione {thisprocessname} è già attiva", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Log.Fatal($"L'applicazione {thisprocessname} è già attiva. Chiudiamo questa istanza.");
                System.Windows.Application.Current.Shutdown();
                return;
            }

            #endregion

            // Questo fa sì che i dati non stringa (es: DateTime) vengano convertiti in stringa nel formato locale:
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));


            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                //.UseSerilog()
                .Build();

            // Due modi per creare un serviceprovider. Se abbiamo l'host, è più facile il secondo!

            // 1 - 
            //// Create a service collection and configure our dependencies
            //var serviceCollection = new ServiceCollection();
            //ConfigureServices(serviceCollection);

            //// Build the our IServiceProvider and set our static reference to it
            //AppServiceProvider = serviceCollection.BuildServiceProvider();

            // 2 - 
            AppServiceProvider = _host.Services; // MB per risolvere i servizi da qualunque punto del progetto (TODO...SERVE??)

        }

        private void ConfigureServices(IServiceCollection services)
        {

            // TODO: per alcuni servizi riusciamo ad usare la DI per gli application settings, per altri no e quindi usiamo per ora una classe statica. RIVEDERE.
            ApplicationSettings options = _configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();
            services.AddSingleton(options);

            _configuration.GetSection("ApplicationSettings").Get<ApplicationSettingsStatic>();
            services.AddSingleton(typeof(IConfiguration), _configuration);

            //services.AddSingleton<AvvisoViewModel>();
            services.AddSingleton<Views.AvvisoView>();

            services.AddSingleton<ProfilerViewModel>();
            services.AddSingleton<Views.ProfilerView>();

            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<Views.SettingsView>();

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<Views.MainWindowView>();

        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (!stoppingApp)
            {
                Serilog.Log.ForContext<App>().Information("OnStartup");

                base.OnStartup(e);

                await _host.StartAsync();

                #region Finestra principale

                // Ci sono Varie opzioni per ricavare le istanze delle finestre che ci servono..:

                // 1 -
                //var analisiBilletteWindow = AppServiceProvider.GetRequiredService<Views.AnalisiBilletteView>();

                // 2-
                // var analisiBilletteWindow = _host.Services.GetRequiredService<Views.AnalisiBilletteView>();

                // 3- vedere https://stackoverflow.com/questions/58222775/how-to-register-dependencies-based-on-the-class-they-are-required-fromusing-bui
                //Views.AnalisiBilletteView analisiBilletteWindow = ActivatorUtilities.CreateInstance<Views.AnalisiBilletteView>(_host.Services);
                ////analisiBilletteWindow.DataContext = ActivatorUtilities.CreateInstance<ViewModels.AnalisiBilletteViewModel>(_host.Services);

                //Views.ImpostazioniView impostazioniWindow = ActivatorUtilities.CreateInstance<Views.ImpostazioniView>(_host.Services);
                ////impostazioniWindow.DataContext = ActivatorUtilities.CreateInstance<ViewModels.ImpostazioniViewModel>(_host.Services);

                //Views.MainWindowView mainWindow = ActivatorUtilities.CreateInstance<Views.MainWindowView>(_host.Services);
                ///mainWindow.DataContext = ActivatorUtilities.CreateInstance<ViewModels.MainWindowViewModel>(_host.Services);

                Views.MainWindowView mainWindow = _host.Services.GetRequiredService<Views.MainWindowView>();


                //var secondary = 0;
                //for (int index = 0; index < Screen.AllScreens.Length; index++)
                //{
                //    if (Screen.AllScreens[index].Primary) continue;
                //    secondary = index;
                //    break;
                //}

                mainWindow.Show();

                //var screen = Screen.AllScreens[secondary];
                //if (screen != null)
                //{
                //    Rectangle area = screen.WorkingArea;
                //    if (!area.IsEmpty)
                //    {
                //        mainWindow.Resize(area);
                //    }
                //}

                #endregion Finestra principale

                try
                {
                    //RabbitMq.GetInstance.Init();
                    RabbitMqConn.GetInstance.Init();

                }
                catch (Exception ex)
                {
                    Serilog.Log.ForContext<App>().Fatal(ex, $"La connessione a RabbitMq è fallita. Chiudiamo l'applicazione.");
                    System.Windows.Application.Current.Shutdown();
                }

                try
                {

                    //List<ImpostazioneGenerale> listaImpostazioneGenerale = new List<ImpostazioneGenerale>();

                    //listaImpostazioneGenerale.Add(new ImpostazioneGenerale() {Id="ModalitaAutomatica", Valore=""});
                    //ImpostazioniGenerali.Instance.SetImpostazioni(listaImpostazioneGenerale);

                    //AutomationDontTouch.InitTouchFile(ApplicationSettingsStatic.AutomationDontTouchFile);

                }
                catch (Exception ex)
                {
                    Serilog.Log.ForContext<App>().Fatal(ex, $"Problemi nel recuperare le impostaioni. Chiudiamo l'applicazione.");
                    System.Windows.Application.Current.Shutdown();
                }
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (!stoppingApp)
            {
                Serilog.Log.ForContext<App>().Information("OnExit");

                using (_host)
                {
                    await _host.StopAsync();
                }

                base.OnExit(e);
            }
        }

        static void AppDomain_CurrentDomain_UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Serilog.Log.ForContext(typeof(App)).Error(e, "Errore in AppDomain.CurrentDomain (UnhandledExceptionEventHandler).");
            Serilog.Log.ForContext(typeof(App)).Error("UnhandledExceptionEventHandler Runtime terminating: {0}", args.IsTerminating);
        }
    }
}
