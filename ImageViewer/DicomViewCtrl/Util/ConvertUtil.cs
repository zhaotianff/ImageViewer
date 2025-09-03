using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DicomViewCtrl.Util
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
                ushort[] target16BitUshortArray = new ushort[buffer.Length / 2];
                CopyByteArrayToUshortArray(buffer, target16BitUshortArray);
                return ConvertUshortArrayToWriteableBitmap(target16BitUshortArray, width, height);
            }
           else
            {
                return ConvertByteArrayToWriteableBitmap(buffer, width, height, bits);
            }
        }

        /// <summary>
        /// 8bit
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static WriteableBitmap ConvertByteArrayToWriteableBitmap(byte[] buffer, int width, int height, int bits)
        {
            var size = width * height;
            var pixelFormat = PixelFormats.Gray8;
            WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96.0, 96.0, pixelFormat, BitmapPalettes.BlackAndWhite);
            writeableBitmap.Lock();
            Marshal.Copy(buffer, 0, writeableBitmap.BackBuffer, size);
            Int32Rect int32Rect = new Int32Rect(0, 0, width, height);
            writeableBitmap.AddDirtyRect(int32Rect);
            writeableBitmap.Unlock();
            //writeableBitmap.Freeze();
            return writeableBitmap;
        }

        /// <summary>
        /// 12bit/16bit
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static WriteableBitmap ConvertUshortArrayToWriteableBitmap(ushort[] buffer,int width,int height,bool normalize = true)
        {
            var writeableBitmap = new WriteableBitmap(
                width,
                height,
                96, 96,  
                PixelFormats.Gray16,
                BitmapPalettes.BlackAndWhite);

            int stride = width * 2;  
            int pixelBufferSize = stride * height;

            if (buffer.Length != width * height)
            {
                throw new ArgumentException("The length of image data does not match the size of the image");
            }

            if (normalize)
            {
                ushort maxValue = 0;
                foreach (var value in buffer)
                {
                    if (value > maxValue)
                    {
                        maxValue = value;
                    }
                }

                if (maxValue > 0 && maxValue < 65535)
                {
                    float scale = 65535.0f / maxValue;
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = (ushort)(buffer[i] * scale);
                    }
                }
            }

            writeableBitmap.Lock();
            unsafe
            {
                ushort* pDest = (ushort*)writeableBitmap.BackBuffer;
                fixed(void* sourcePtr = &buffer[0])
                {
                    memcpy(pDest, sourcePtr, pixelBufferSize);
                }
            }
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            writeableBitmap.Unlock();
            //writeableBitmap.Freeze();
            return writeableBitmap;
        }

        public static byte[] Convert16BitTo8Bit(byte[] byteArray16Bit)
        {
            ushort[] target8BitArray = new ushort[byteArray16Bit.Length / 2];
            //CopyByteArrayToUshortArrayUnsafe(byteArray16Bit, target8BitArray);
            CopyByteArrayToUshortArray(byteArray16Bit, target8BitArray);
            return target8BitArray.Select(x => (byte)x).ToArray();
        }

        /// <summary>
        /// 16/12bit to 8bit(unsafe)
        /// </summary>
        /// <param name="srcData"></param>
        /// <param name="dstData"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 16/12bit to 8bit
        /// </summary>
        /// <param name="srcData"></param>
        /// <param name="dstData"></param>
        /// <returns></returns>
        public static bool CopyByteArrayToUshortArray(byte[] srcData, ushort[] dstData)
        {
            for (int i = 0; i < srcData.Length; i += 2)
            {
                //小端序
                dstData[i / 2] = (ushort)(srcData[i] | (srcData[i + 1] << 8));
            }
            return true;
        }

        public static bool CopyUshortArrayToByteArray(ushort[] srcData, byte[] dstData)
        {
            for (int i = 0; i < srcData.Length; i++)
            {
                byte[] invertedBytes = BitConverter.GetBytes(srcData[i]);

                dstData[i * 2] = invertedBytes[0];
                dstData[(i * 2) + 1] = invertedBytes[1];
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

        public static BitmapSource GetImageSourceThumbnail(WriteableBitmap originalBitmap, int thumbWidth, int thumbHeight)
        {
            double scaleX = (double)thumbWidth / originalBitmap.PixelWidth;
            double scaleY = (double)thumbHeight / originalBitmap.PixelHeight;

            var transform = new ScaleTransform(scaleX, scaleY);

            var thumbnail = new TransformedBitmap(originalBitmap, transform);
            return thumbnail;
        }
    }
}
