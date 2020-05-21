using Dicom;
using Dicom.Imaging;
using ImageViewCtrl.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageViewCtrl
{
    /// <summary>
    /// flyff.xaml 的交互逻辑
    /// </summary>
    public partial class flyff : UserControl
    {
        private byte[] raw8BitBuffer;
        private byte[] raw16BitBuffer;
        private int width;
        private int height;
        private int bits;
        private double ww;
        private double wl;            

        public flyff()
        {
            InitializeComponent();
        }

        /// <summary>
        /// open and show dicom file
        /// </summary>
        /// <param name="dicmFile"></param>
        /// <returns></returns>
        public bool OpenImage(string dicomFile)
        {
            try
            {
                var image = new DicomImage(dicomFile);
#pragma warning disable CS0618
                var pixelData = image.PixelData.GetFrame(0).Data;

                this.bits = image.Dataset.Get<int>(DicomTag.BitsStored);
                this.width = image.Width;
                this.height = image.Height;
                this.ww = image.WindowWidth;
                this.wl = image.WindowCenter;

                if(bits > 8)
                {
                    raw16BitBuffer = new byte[width * height * 2];
                    Array.Copy(pixelData, raw16BitBuffer, pixelData.Length);                    
                }
                else
                {
                    raw8BitBuffer = new byte[width * height];
                    Array.Copy(pixelData, raw8BitBuffer, pixelData.Length);
                }

                var writeableBitmap = ConvertUtil.GetWriteableBitmap(pixelData, this.width, this.height, this.bits);
                var imageSource = ConvertUtil.GetImageSource(writeableBitmap);

                this.image.Source = imageSource;
                return true;
            }
            catch
            {
                return false;
            }
        }      
    }
}
