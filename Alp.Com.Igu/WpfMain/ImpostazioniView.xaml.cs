using AlpTlc.App.Igu.ViewModels;
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

namespace AlpTlc.Pre.Igu.Views
{
    /// <summary>
    /// Logica di interazione per HomeView.xaml
    /// </summary>
    public partial class ImpostazioniView : UserControl
    {

        // TODO vedere come fare a passare un logger generico anziché Serilog attraverso la dependency injection
        //Microsoft.Extensions.Logging.ILogger _logger;
        // NB: in questo particolare caso (AnalisiBilletteView così come ImpostazioniView), ciò che impedisce l'uso della dependency injection per il logger
        // è il fatto che l'approccio utilizzato per passare da una vista all'altra (ImpostazioniView<->AnalisiBilletteView) è quello descritto qui...:
        // https://rachel53461.wordpress.com/2011/05/28/switching-between-viewsusercontrols-using-mvvm/
        // https://stackoverflow.com/questions/62333005/wpf-mvvm-switching-between-views
        // ... basato cioè sui DataTemplate DataType=... (vedi il file App.xaml) che richiede un costruttore senza parametri per ImpostazioniView e AnalisiBilletteView.
        // TODO capire se si può usare un costruttore con parametri...

        Serilog.ILogger _logger = Serilog.Log.ForContext<ImpostazioniView>();

        ImpostazioniViewModel vm;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        public ImpostazioniView()
        {
            _logger.Verbose("ImpostazioniView ctor...");

            InitializeComponent();

            this.DataContext = App.AppServiceProvider.GetRequiredService<ImpostazioniViewModel>();
            vm = this.DataContext as ImpostazioniViewModel;

            _logger.Verbose("ImpostazioniView ctor.");
        }

        // Negli schermi touch, dopo aver toccato un controllo, il puntatore del mouse resta lì (anche se non si vede); 
        // di conseguenza il controllo (bottone o elemento di una lista, ad esempio), assumono l'aspetto 
        // che devono avere quando il mouse è "sopra" (-> sembra selezionato, e non va bene)
        // Questa funzione sposta il mouse in basso
        private void TouchTogliMouseOver()
        {
            //Point relativePoint = tglBarraGialla.TransformToAncestor(this).Transform(new Point(0, 0));
            //SetCursorPos((int)relativePoint.X, (int)relativePoint.Y);

            SetCursorPos(1, 1); // RIPR
        }

        private void btnAccendiIlluminatori_TouchUp(object sender, TouchEventArgs e)
        {
            TouchTogliMouseOver();
        }

        private void btnSpegniIlluminatori_TouchUp(object sender, TouchEventArgs e)
        {
            TouchTogliMouseOver();
        }


        private void btnResetTelecamere_TouchUp(object sender, TouchEventArgs e)
        {
            TouchTogliMouseOver();
        }


        //bool _shown;

        //protected override void OnContentRendered(EventArgs e)
        //{
        //    base.OnContentRendered(e);

        //    if (_shown)
        //        return;

        //    _shown = true;

        //    // Your code here.
        //}

    }
}
