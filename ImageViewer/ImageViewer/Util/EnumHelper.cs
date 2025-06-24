using ImageViewer.Infrastructure.Attributes;
using ImageViewer.Infrastructure.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Util
{
   public static class EnumHelper
    {
        public static string GetDisplayName<T>(this T enumValue) where T : Enum
        {
            Type type = enumValue.GetType();
            string name = Enum.GetName(type, enumValue);
            if (name == null) return enumValue.ToString();

            var fieldInfo = type.GetField(name);
            var attribute = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();

            return attribute?.DisplayName ?? enumValue.ToString();
        }

        public static int GetFrameWaitMillionSeconds(this FpsType enumValue)
        {
            switch(enumValue)
            {
                case FpsType.Fps_15:
                    return (int)(1000/15f);
                case FpsType.Fps_20:
                    return (int)(1000/ 20f);
                case FpsType.Fps_25:
                    return (int)(1000/25f);
                default:
                    return 20;
            }    
        }
    }
}
