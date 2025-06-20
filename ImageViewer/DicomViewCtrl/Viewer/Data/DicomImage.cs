using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private ImageSource thumbnail;

        public ImageSource Thumbnail 
        {
            get => thumbnail;
            internal set
            {
                thumbnail = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Thumbnail"));
            }
        }

        public int Width { get; internal set; }

        public int Height { get; internal set; }

        public int Bits { get; internal set; }

        internal string SopInstanceUID { get; set; }

        public string StudyDateTime { get; internal set; }

        public string PatientName { get; internal set; }

        public ObservableCollection<FrameImage> FrameList { get; internal set; }

        private int selectedFrameIndex = 0;

        public int SelectedFrameIndex
        {
            get => selectedFrameIndex;
            set
            {
                selectedFrameIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedFrameIndex"));
            }
        }

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
