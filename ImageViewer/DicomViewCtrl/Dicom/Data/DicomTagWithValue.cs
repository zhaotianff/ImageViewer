using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewCtrl.Dicom.Data
{
    public class DicomTagWithValue
    {
        public ushort Group { get; internal set; }

        public ushort Element { get; internal set; }

        public string Description { get; internal set; }

        private object value;

        public object Value
        {
            get => value;
            set
            {
                this.value = value;
                IsModified = true;
            }
        }

        public string ValueRepresentation { get; internal set; }

        internal bool IsModified { get; private set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
