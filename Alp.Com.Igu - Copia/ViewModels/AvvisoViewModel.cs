using Alp.Com.Igu.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alp.Com.Igu.ViewModels
{
    public class AvvisoViewModel :  ObservableObject
    {
        //private string _testoAvviso = "Testo Avviso";

        //public string TestoAvviso
        //{
        //    get => _testoAvviso;
        //    set
        //    {
        //        _testoAvviso = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public string TestoAvviso => TestoAvvisoGenerale +
        //                             (string.IsNullOrWhiteSpace(TestoAvvisoPerScansione) ? "" : ("\n" + TestoAvvisoPerScansione));

        public string TestoAvviso => Utils.StringUtils.JoinFilter("\n", new string[] { TestoAvvisoGenerale, TestoAvvisoPerScansione });

        private string _testoAvvisoGenerale = null; //"Testo Avviso Generale";

        public string TestoAvvisoGenerale
        {
            get => _testoAvvisoGenerale;
            set
            {
                _testoAvvisoGenerale = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TestoAvviso));
                OnPropertyChanged(nameof(CiSonoAnomalieValidazionePerScansioneSelezionata));
            }
        }

        private string _testoAvvisoPerScansione = null;//"Testo Avviso Per Scansione";

        public string TestoAvvisoPerScansione
        {
            get => _testoAvvisoPerScansione;
            set
            {
                _testoAvvisoPerScansione = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TestoAvviso));
                OnPropertyChanged(nameof(CiSonoAnomalieValidazionePerScansioneSelezionata));
            }
        }

        private int? _idScansioneSelezionata = null; 

        public int? IdScansioneInEsame
        {
            get => _idScansioneSelezionata;
            set
            {
                _idScansioneSelezionata = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TestoAvviso));
                OnPropertyChanged(nameof(CiSonoAnomalieValidazionePerScansioneSelezionata));
            }
        }

        public bool CiSonoAnomalieValidazionePerScansioneSelezionata => !string.IsNullOrEmpty(TestoAvvisoPerScansione);

        //public System.Drawing.Icon Icona { get; set; } = System.Drawing.SystemIcons.Warning;
    }
}
