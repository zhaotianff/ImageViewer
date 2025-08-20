using DicomViewCtrl.Dicom.Data;
using ImageViewer.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ImageViewer.Configuration.Annotation
{
    public class AnnotationConfiguration : IConfiguration
    {
        public ObservableCollection<DicomTagWithValue> LeftTop { get; set; }

        public ObservableCollection<DicomTagWithValue> RightTop { get; set; }

        public ObservableCollection<DicomTagWithValue> LeftBottom { get; set; }

        public ObservableCollection<DicomTagWithValue> RightBottom { get; set; }

        public bool IsShowDescription { get; set; }

        public string ModuleName => "AnnotationConfiguration";

        public AnnotationConfiguration(XElement xElement)
        {
            LoadAnnotationConfiguration(xElement);
        }

        public AnnotationConfiguration()
        {
            SetDefaultConfiguration();
        }

        private void LoadAnnotationConfiguration(XElement xElement)
        {
            try
            {
                if(xElement == null)
                {
                    SetDefaultConfiguration();
                    return;
                }

                this.LeftTop = new ObservableCollection<DicomTagWithValue>(XElementToList(xElement.Element("LeftTop")));
                this.RightTop = new ObservableCollection<DicomTagWithValue>(XElementToList(xElement.Element("RightTop")));
                this.LeftBottom = new ObservableCollection<DicomTagWithValue>(XElementToList(xElement.Element("LeftBottom")));
                this.RightBottom = new ObservableCollection<DicomTagWithValue>(XElementToList(xElement.Element("RightBottom")));
                this.IsShowDescription = XmlExtension.GetValue<bool>(xElement, "IsShowDescription");
            }
            catch
            {
                SetDefaultConfiguration();
            }
        }

        public void SetDefaultConfiguration()
        {
            this.LeftTop = new ObservableCollection<DicomTagWithValue>();
            this.RightTop = new ObservableCollection<DicomTagWithValue>();
            this.LeftBottom = new ObservableCollection<DicomTagWithValue>();
            this.RightBottom = new ObservableCollection<DicomTagWithValue>();
            this.IsShowDescription = true;
        }

        public XElement GetConfigurationXml()
        {
            XElement xElement = new XElement("ConfigurationModule", new XAttribute("ModuleName", ModuleName),
                new XElement("LeftTop", ObservableListToXElement(LeftTop).ToArray()),
                 new XElement("RightTop", ObservableListToXElement(RightTop).ToArray()),
                 new XElement("LeftBottom", ObservableListToXElement(LeftBottom).ToArray()),
                 new XElement("RightBottom", ObservableListToXElement(RightBottom).ToArray()),
                 new XElement("IsShowDescription", IsShowDescription));

            return xElement;
        }

        private IEnumerable<XElement> ObservableListToXElement(ObservableCollection<DicomTagWithValue> dicomTagWithValues)
        {
            foreach (var item in dicomTagWithValues)
            {
                yield return new XElement("DicomTagWithValue",
                    new XAttribute("Group", item.Group),
                    new XAttribute("Element", item.Element),
                    new XAttribute("Description", item.Description));
            }
        }

        private IEnumerable<DicomTagWithValue> XElementToList(XElement xElement)
        {
            foreach (var item in xElement.Elements())
            {
                yield return new DicomTagWithValue(
                    XmlExtension.GetAttributeValue<ushort>(xElement, "Group"),
                    XmlExtension.GetAttributeValue<ushort>(xElement, "Element"),
                    XmlExtension.GetAttributeValue<string>(xElement, "Description"));
            }
        }
    }
}
