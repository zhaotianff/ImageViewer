using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewCtrl.Viewer.Event
{
    public class FrameChangedEventArgs : EventArgs
    {
        public FrameChangedEventArgs(int frameIndex,int imageIndex) :base()
        {
            this.FrameIndex = frameIndex;
            this.ImageIndex = imageIndex;
        }

        public int FrameIndex { get;  }

        public int ImageIndex { get;  }
    }
}
