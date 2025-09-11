using Alp.Com.Igu.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Alp.Com.Igu.Theme
{
    public class ComandiGenerici
    {

        public static readonly ICommand ClosePopupCommand = new RelayCommand(o => ((Popup)o).IsOpen = false);

    }
}
