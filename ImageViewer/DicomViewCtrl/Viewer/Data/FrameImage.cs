using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DicomViewCtrl.Viewer.Data
{
    public class FrameImage
    {
        public int FrameIndex { get; internal set; }

        public ImageSource FrameThumbnail { get; internal set; }
    }
}
