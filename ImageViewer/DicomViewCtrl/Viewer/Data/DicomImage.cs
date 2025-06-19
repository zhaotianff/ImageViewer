using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DicomViewCtrl.Viewer.Data
{
    public class DicomImage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FilePath { get; internal set; }

        public ImageSource Thumbnail { get; internal set; }

        public int Width { get; internal set; }

        public int Height { get; internal set; }

        public int Bits { get; internal set; }

        internal string SopInstanceUID { get; set; }

        private string title;

        public string Title
        {
            get => title;
            set
            {
                title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title"));
            }
        }

        public bool IsRaw { get; internal set; }
    }
}
