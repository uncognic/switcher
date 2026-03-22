using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace switcher
{
    public class WindowItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        public string Title { get; set; } = "";
        public string ProcessName { get; set; } = "";
        public IntPtr Handle { get; set; }
        public ImageSource? Icon { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
