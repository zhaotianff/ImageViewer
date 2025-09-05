using Dicom.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewCtrl.Dicom.Data
{
    public class DicomInfo
    {
        private ObservableCollection<DicomTagWithValue> dicomTagWithValues = new ObservableCollection<DicomTagWithValue>();

        internal DicomInfo(ObservableCollection<DicomTagWithValue> dicomTagWithValues)
        {
            this.dicomTagWithValues = dicomTagWithValues;
        }

        private T Find<T>(DicomTagWithValue dicomTagWithValue)
        {
            var findResult = dicomTagWithValues.FirstOrDefault(x => x.Equals(dicomTagWithValue));

            if (findResult == null)
                return default(T);

            return (T)findResult.Value;
        }

        public int NumberOfFrames => Find<int>(DicomTags.NumberOfFrames);
    }
}
