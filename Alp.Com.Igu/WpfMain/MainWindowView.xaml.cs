using Alp.Com.Igu.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Alp.Com.Igu.WpfMain
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        ILogger _logger;

        MainWindowViewModel vm;

        public MainWindowView(ILogger<MainWindowView> logger)
        {
            _logger = logger;
            _logger.LogTrace("MainWindowView ctor...");

            InitializeComponent();

            this.DataContext = App.AppServiceProvider.GetRequiredService<MainWindowViewModel>();

            vm = this.DataContext as MainWindowViewModel;

            _logger.LogTrace("MainWindowView ctor.");
        }

        public void Resize(System.Drawing.Rectangle rect)
        {
            _logger.LogTrace("Resize...");

            MainWindowViewModel vm = this.DataContext as MainWindowViewModel;

            this.Left = rect.Left;
            this.Top = rect.Top;
            this.Width = rect.Width; // ...serve??
            this.Height = rect.Height; // ...serve??
            this.WindowState = WindowState.Maximized;

            //vm.IsLandscape = this.ActualHeight < this.ActualWidth;

            vm.IsLandscape = this.Height < this.Width;

            _logger.LogTrace("Resize.");
        
        }

        private void CambioPagina_Checked(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
            vm.CaricaPaginaImpostazioni(sender, e);
        }

        private void CambioPagina_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
            vm.CaricaPaginaPrincipale(sender, e);
        }

        private void btnMinimizza_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogTrace("btnMinimizza_Click...");

            this.WindowState = WindowState.Minimized;

            _logger.LogTrace("btnMinimizza_Click.");
        }

        private void btnChiudi_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogTrace("btnChiudi_Click...");

            this.Close();

            _logger.LogTrace("btnChiudi_Click.");
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _logger.LogTrace("Window_Closed...");

            Application.Current.Shutdown();
            System.Diagnostics.Process.GetCurrentProcess().Kill();

            _logger.LogTrace("Window_Closed.");
        }





        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{

        //    _logger?.LogInformation("Window_Closing- Executing ApplicationShutdownCommand...");

        //    // Non serve, è già in ApplicationShutDown (comando richiamato dal click sulla croce):
        //    //(DataContext as IMainWindowViewModel)?.KillImagersCommand.Execute(null);
        //    (DataContext as IMainWindowViewModel)?.ApplicationShutdownCommand.Execute(null);
        //}

    }
}
