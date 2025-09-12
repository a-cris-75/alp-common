using Alp.Com.Igu.ViewModels;
//using AlpTlc.Pre.Igu;
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

namespace Alp.Com.Igu.Views
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {

        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger
                           ("Alp.Com.Igu.Views.MainWindowView");


        MainWindowViewModel vm;

        public MainWindowView()
        {
            //_logger = logger;
            _logger.Info("MainWindowView ctor...");

            InitializeComponent();

            this.DataContext = App.AppServiceProvider.GetRequiredService<MainWindowViewModel>();

            vm = this.DataContext as MainWindowViewModel;

            _logger.Info("MainWindowView ctor.");
        }

        public void Resize(System.Drawing.Rectangle rect)
        {
            _logger.Info("Resize...");

            MainWindowViewModel vm = this.DataContext as MainWindowViewModel;

            this.Left = rect.Left;
            this.Top = rect.Top;
            this.Width = rect.Width; // ...serve??
            this.Height = rect.Height; // ...serve??
            this.WindowState = WindowState.Maximized;

            //vm.IsLandscape = this.ActualHeight < this.ActualWidth;

            vm.IsLandscape = this.Height < this.Width;

            _logger.Info("Resize.");
        
        }

        //private void CambioPagina_Checked(object sender, RoutedEventArgs e)
        //{
        //    MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
        //    vm.CaricaPaginaStati(sender, e);
        //}

        //private void CambioPagina_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
        //    vm.CaricaPaginaPrincipale(sender, e);
        //}

        private void btnMinimizza_Click(object sender, RoutedEventArgs e)
        {
            _logger.Info("btnMinimizza_Click...");

            this.WindowState = WindowState.Minimized;

            _logger.Info("btnMinimizza_Click.");
        }

        private void btnChiudi_Click(object sender, RoutedEventArgs e)
        {
            _logger.Info("btnChiudi_Click...");

            this.Close();

            _logger.Info("btnChiudi_Click.");
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _logger.Info("Window_Closed...");

            Application.Current.Shutdown();
            System.Diagnostics.Process.GetCurrentProcess().Kill();

            _logger.Info("Window_Closed.");
        }


        public void ShowPage(Page page)
        {
            PageContainer.Content = page;
            vm.CurrentPage = page;
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
