using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;using System.Runtime.InteropServices;

namespace ImageViewCtrl.Dicom.Data
{
    public class DicomImage
    {
        public int Width { get; private set; }

        public int Height { get; private set; }

        public int BytesPerPixel { get; private set; }

        public byte[] ImageData { get; private set; }


        public DicomImage(string filePath)
        {
        }

        public DicomImage(int width, int height, byte[] imageData, int bytePerPixel = 2)
        {
            if (imageData == null || imageData.Length != width * height * bytePerPixel)
            {
                return;
            }
            this.ImageData = new byte[imageData.Length];
            this.Width = width;
            this.Height = height;
            this.BytesPerPixel = bytePerPixel;
            Array.Copy(imageData, this.ImageData, this.ImageData.Length);
        }

        public DicomImage(int width, int height, int bytePerPixel = 2)
        {
            this.Width = width;
            this.Height = height;
            this.BytesPerPixel = bytePerPixel;
            this.ImageData = new byte[width * height * bytePerPixel];
        }
    }
}
