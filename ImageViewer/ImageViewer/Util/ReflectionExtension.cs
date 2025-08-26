using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ImageViewer.Util
{
    public class ReflectionExtension
    {
        public static T DynamicCast<T>(string strValue)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)strValue;

            var valueType = typeof(T);
            Type[] argTypes = { typeof(string), valueType.MakeByRefType() };
            var tryParseFunc = typeof(T).GetMethod("TryParse", argTypes);

            if (tryParseFunc != null)
            {
                object[] parameters = new object[] { strValue, null };
                object result = tryParseFunc.Invoke(null, parameters);
                bool blResult = (bool)result;
                if (blResult)
                {
                    return (T)parameters[1];
                }
            }

            return default(T);
        }
    }
}
