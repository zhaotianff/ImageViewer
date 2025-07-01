using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewCtrl.Viewer.Event
{
    public class FrameChangedEvent
    {
        public delegate void FrameChangedEventHandler(object sender, FrameChangedEventArgs e);
    }
}
