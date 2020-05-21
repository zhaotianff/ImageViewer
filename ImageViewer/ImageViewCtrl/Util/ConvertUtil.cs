using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageViewCtrl.Util
{
    public class ConvertUtil
    {
        public static byte[] ConvertTo8BitImage(byte[] source)
        {
            return source;
        }

        public static WriteableBitmap GetWriteableBitmap(byte[] buffer,int width,int height,int bit)
        {
            var pixelFormat = bit > 8 ? PixelFormats.Gray16 : PixelFormats.Gray8;
            var size = bit > 8 ? width * height * 2 : width * height;
            WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96.0, 96.0, pixelFormat, BitmapPalettes.BlackAndWhite);;
            writeableBitmap.Lock();
            Marshal.Copy(buffer, 0, writeableBitmap.BackBuffer, size);
            Int32Rect int32Rect = new Int32Rect(0, 0, width, height);
            writeableBitmap.AddDirtyRect(int32Rect);
            writeableBitmap.Unlock();
            return writeableBitmap;
        }

        public static ImageSource GetImageSource(WriteableBitmap writeableBitmap)
        {
            TransformedBitmap bitmap = new TransformedBitmap();
            bitmap.BeginInit();
            bitmap.Source = writeableBitmap;
            bitmap.EndInit();
            return bitmap;
        }

        /*
         * private WriteableBitmap GetWriteableBitmap(string fileName)
        {
            var image = new DicomImage(fileName);

            //I need get raw data here
#pragma warning disable CS0618
            byte[] rawPixelData = image.PixelData.GetFrame(0).Data;

            WriteableBitmap writeableBitmap = new WriteableBitmap(image.Width, image.Height, 96.0, 96.0, PixelFormats.Gray8, BitmapPalettes.BlackAndWhite);

            //todo I should convert 12/16 bit image to 8-bit           
            var bits = image.Dataset.Get<int>(DicomTag.BitsAllocated);

            raw8BitBuffer = new byte[image.Width * image.Height];

            if (bits > 8)
            {
                rawPixelData = ConvertUtil.ConvertTo8BitImage(rawPixelData);
            }
            Array.Copy(rawPixelData, raw8BitBuffer, image.Width * image.Height);

            writeableBitmap.Lock();
            Marshal.Copy(raw8BitBuffer, 0, writeableBitmap.BackBuffer, image.Width * image.Height);
            Int32Rect int32Rect = new Int32Rect(0, 0, image.Width, image.Height);
            writeableBitmap.AddDirtyRect(int32Rect);
            writeableBitmap.Unlock();
            return writeableBitmap;
        }

        private ImageSource GetImageSource(string fileName)
        {
            var writeableBitmap = GetWriteableBitmap(fileName);
            TransformedBitmap bitmap = new TransformedBitmap();
            bitmap.BeginInit();
            bitmap.Source = writeableBitmap;
            bitmap.EndInit();
            return bitmap;
        }*/
    }
}
