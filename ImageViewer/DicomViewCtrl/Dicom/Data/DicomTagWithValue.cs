using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewCtrl.Dicom.Data
{
    public class DicomTagWithValue : IEquatable<DicomTagWithValue>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
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

        public DicomTagWithValue(ushort group,ushort element,string description,object value) : this(group,element,description)
        {
            this.Value = value;
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
            if (other == null)
                return false;

            return this.Group == other.Group && this.Element == other.Element;
        }

        public override int GetHashCode()
        {
            return (Group,Element).GetHashCode();
        }
    }
}
