using DicomViewCtrl.Dicom.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewCtrl.Util
{
    internal class GCUtil
    {
        internal void ForceFreeDicomFileMemory(DicomFile dicomFile)
        {
            if (dicomFile == null)
                return;

            dicomFile.ImageData = null;
            dicomFile.PreviewImage = null;

            GC.Collect();
        }
    }
}
