using Alp.Com.Igu.Core;
using Alp.Com.Igu.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
using System.Windows.Shapes;

namespace Alp.Com.Igu.Views
{
    /// <summary>
    /// Interaction logic for Avviso.xaml
    /// </summary>
    public partial class AvvisoView : Window
    {

        public AvvisoViewModel vm;

        public AvvisoView()
        {
            InitializeComponent();

            //this.DataContext = App.AppServiceProvider.GetRequiredService<AvvisoViewModel>();
            vm = new AvvisoViewModel();
            this.DataContext = vm;
        }

        private void btnChiudi_Click(object sender, RoutedEventArgs e) //=> Close(); // Oppure Hide() se non la si vuole ricreare
        {
            Hide(); // Oppure Close() se la si vuole ricreare
        }


    }
}
