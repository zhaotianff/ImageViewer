using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ImageViewer.Util
{
    public class XmlExtension
    {
        public static T GetAttributeValue<T>(XElement element,string attributeName)
        {
            var data = element.Attribute(attributeName);

            if (data == null)
                return default(T);

            return ReflectionExtension.DynamicCast<T>(data.Value);
        }

        public static T GetValue<T>(XElement element,string childElementName)
        {
            var childElement = element.Element(childElementName);

            if (childElement == null)
                return default(T);

            var data = childElement.Value;

            if (string.IsNullOrEmpty(data))
                return default(T);

            return ReflectionExtension.DynamicCast<T>(data);
        }
    }
}
