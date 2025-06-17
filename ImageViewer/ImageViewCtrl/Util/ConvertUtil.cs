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
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private unsafe static extern int memcpy(void* pDst, void* pSrc, int count);


        public static byte[] ConvertTo8BitImage(byte[] source)
        {
            return source;
        }

        public static WriteableBitmap GetWriteableBitmap(byte[] buffer,int width,int height,int bits)
        {
            if (bits > 8)
            {
                buffer = Convert16BitTo8Bit(buffer);
            }

            var size = width * height;
            var pixelFormat = PixelFormats.Gray8;
            WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96.0, 96.0, pixelFormat, BitmapPalettes.BlackAndWhite);
            writeableBitmap.Lock();
            Marshal.Copy(buffer, 0, writeableBitmap.BackBuffer, size);
            Int32Rect int32Rect = new Int32Rect(0, 0, width, height);
            writeableBitmap.AddDirtyRect(int32Rect);
            writeableBitmap.Unlock();
            return writeableBitmap;
        }

        public static byte[] Convert16BitTo8Bit(byte[] byteArray16Bit)
        {
            ushort[] target8BitArray = new ushort[byteArray16Bit.Length / 2];
            //CopyByteArrayToUshortArrayUnsafe(byteArray16Bit, target8BitArray);
            CopyByteArrayToUshortArray(byteArray16Bit, target8BitArray);
            return target8BitArray.Select(x => (byte)x).ToArray();
        }

        public unsafe static bool CopyByteArrayToUshortArrayUnsafe(byte[] srcData, ushort[] dstData)
        {
            try
            {
                fixed (void* ptr = (&dstData[0]))
                {
                    fixed (void* ptr2 = (&srcData[0]))
                    {
                        memcpy(ptr, ptr2, srcData.Length);
                    } 
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool CopyByteArrayToUshortArray(byte[] srcData, ushort[] dstData)
        {
            for (int i = 0; i < srcData.Length; i += 2)
            {
                //小端序
                dstData[i / 2] = (ushort)(srcData[i] | (srcData[i + 1] << 8));
            }
            return true;
        }

        public static ImageSource GetImageSource(WriteableBitmap writeableBitmap)
        {
            TransformedBitmap bitmap = new TransformedBitmap();
            bitmap.BeginInit();
            bitmap.Source = writeableBitmap;
            bitmap.EndInit();
            return bitmap;
        }
    }
}
