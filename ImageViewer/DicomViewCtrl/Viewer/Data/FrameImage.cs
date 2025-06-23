using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DicomViewCtrl.Viewer.Data
{
    public class FrameImage : INotifyPropertyChanged
    {
        public int FrameIndex { get; internal set; }

        private ImageSource frameThumnail;

        public ImageSource FrameThumbnail 
        {
            get
            {
                return frameThumnail;
            }
            
            internal set
            {
                frameThumnail = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("FrameThumbnail"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
