using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewCtrl.Util
{
    public class DicomUtil
    {
        public static Tuple<double,double> GetWindowInfoLimit(int bits)
        {
            //int storedMin = pixelData.Min();  
            //int storedMax = pixelData.Max();

            //double realMin = storedMin * slope + intercept;
            //double realMax = storedMax * slope + intercept;

            //current use byte and ushort max

            switch (bits)
            {
                case 8:
                    return new Tuple<double, double>(1, byte.MaxValue);
                case 16:
                    return new Tuple<double, double>(1, ushort.MaxValue);
                default:
                    return new Tuple<double, double>(1, ushort.MaxValue);
            }
        }
    }
}
