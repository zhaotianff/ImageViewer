using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewCtrl.Dicom.Data
{
    public class DicomTagWithValue : IEquatable<DicomTagWithValue>
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

        public DicomTagWithValue(ushort group,ushort element)
        {
            this.Group = group;
            this.Element = element;
        }

        public DicomTagWithValue(ushort group,ushort element,string description) : this(group,element)
        {
            this.Description = description;
        }

        public DicomTagWithValue()
        {

        }

        public override string ToString()
        {
            return Value == null ? "" : Value.ToString();
        }

        public bool Equals(DicomTagWithValue other)
        {
            return this.Group == other.Group && this.Element == other.Element;
        }

        public static bool operator ==(DicomTagWithValue value1,DicomTagWithValue value2)
        {
            return value1.Group == value2.Group && value1.Element == value2.Element;
        }

        public static bool operator !=(DicomTagWithValue value1, DicomTagWithValue value2)
        {
            return value1.Group != value2.Group || value1.Element != value2.Element;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is DicomTagWithValue dicomTagWithValue)
                return Equals(dicomTagWithValue);

            return base.Equals(obj);
        }
    }
}
