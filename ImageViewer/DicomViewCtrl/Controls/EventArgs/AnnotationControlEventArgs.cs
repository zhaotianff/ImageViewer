using DicomViewCtrl.Dicom.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DicomViewCtrl.Controls.EventArgs
{
    public class AnnotationControlEventArgs : RoutedEventArgs
    {
        public DicomTagWithValue DicomTagWithValue { get; set; }

        public AnnotationControlEventArgs(RoutedEvent routedEvent, object source,DicomTagWithValue dicomTagWithValue) : base(routedEvent,source)
        {
            this.DicomTagWithValue = dicomTagWithValue;
        }
    }
}
