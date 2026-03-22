using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace switcher
{
    public class WindowItem
    {
        public string Title { get; set; } = "";
        public string ProcessName { get; set; } = "";
        public IntPtr Handle { get; set; }
        public ImageSource? Icon { get; set; }
    }
}
