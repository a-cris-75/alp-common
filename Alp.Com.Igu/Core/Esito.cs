using System;
using System.Collections.Generic;
using System.Drawing;
//using System.Drawing.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alp.Com.Igu.Core
{
    public class Esito
    {
        public bool Ok { get; set; }

        public string Titolo { get; set; }

        public string Messaggio { get; set; }

        public Exception Eccezione { get; set; }

        public System.Drawing.Icon Icona { get; set; }

        public DateTime DataOra { get; set; } = DateTime.Now;

        public string TipoEccezione => Eccezione?.GetType()?.FullName;

        public bool HaEccezione => Eccezione != null;

        public override string ToString()
        {
            return $"Ok: {Ok}, [{Titolo}] {Messaggio}"; 
        }

    }
}
