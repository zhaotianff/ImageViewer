using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Dicom.Imaging;
using Dicom;
using ImageViewCtrl.Util;
using System.Windows.Media;
using System.Data;
using Dicom.Imaging.Render;
using System.Windows.Media.Imaging;
namespace ImageViewCtrl.Dicom.Data
{
    public class DicomFile
    {
        private static readonly string[] SupportedPhotometricInterpretation = { "MONOCHROME2" };

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public int BitsStored { get; private set; }
        public int BytePerPixel { get; private set; }

        public byte[] ImageData { get; private set; }

        public ImageSource ThumbnailImage { get; private set; }

        public ImageSource PreviewImage { get; private set; }

        public string FilePath { get; private set; }

        public double WindowWidth { get; private set; }
        
        public double WindowCenter { get; private set; }

        public bool IsRawFormat { get; private set; }

        private int frameIndex = -1;

        public int FrameIndex
        {
            get
            {
                return frameIndex;
            }

            set
            {
                if(this.frameIndex != value && value != -1)
                {
                    this.frameIndex = value;

                    if(!string.IsNullOrEmpty(FilePath) && IsRawFormat == false)
                    {
                        OpenDicomFile(FilePath, this.frameIndex);
                    }
                }
            }
        }


        public DicomFile(string filePath)
        {
            this.FilePath = filePath;
            this.FrameIndex = -1;
        }     
        
        public void OpenAsDicomFile(int frameIndex)
        {
            var upperFileExtension = System.IO.Path.GetExtension(this.FilePath).ToUpper();

            if (upperFileExtension == ".DCM")
            {
                this.FrameIndex = 0;
            }
        }

        public void OpenAsRawFile(int width,int height,int bits)
        {
            OpenRawFile(this.FilePath, width, height, bits);
        }

        private void OpenDicomFile(string dicomFile, int frameIndex = 0)
        {
            try
            {
                var dicomImage = new DicomImage(dicomFile);
#pragma warning disable CS0618

                var photometricInterpretation = dicomImage.Dataset.Get<string>(DicomTag.PhotometricInterpretation);
                if (SupportedPhotometricInterpretation.Contains(photometricInterpretation) == false)
                    throw new Exception("Not supported PhotometricInterpretation");

                this.BitsStored = dicomImage.Dataset.Get<int>(DicomTag.BitsStored);
                this.Columns = dicomImage.Width;
                this.Rows = dicomImage.Height;
                this.WindowWidth = dicomImage.WindowWidth;
                this.WindowCenter = dicomImage.WindowCenter;

                var pixelData = dicomImage.PixelData.GetFrame(frameIndex).Data;

                if (this.BitsStored > 8)
                {
                    BytePerPixel = 2;
                }
                else
                {
                    BytePerPixel = 1;  
                }

                ImageData = new byte[this.Rows * this.Columns * BytePerPixel];
                Array.Copy(pixelData, this.ImageData, pixelData.Length);

                WriteableBitmap writeableBitmap = ConvertUtil.GetWriteableBitmap(this.ImageData, this.Columns, this.Rows,this.BitsStored);
                var imageSource = ConvertUtil.GetImageSource(writeableBitmap);
                this.PreviewImage = imageSource;
                this.ThumbnailImage = ConvertUtil.GetImageSourceThumbnail(writeableBitmap, 256, 256);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private byte[] ReadFrameDataFromPixelDataTag(DicomImage dicomImage, int frameIndex)
        {
            var byteFragment = dicomImage.Dataset.Get<DicomOtherByteFragment>(DicomTag.PixelData);
            if (byteFragment != null && byteFragment.Count() > 0)
                return byteFragment.ElementAt(frameIndex).Data;

            return null;
        }

        private void OpenRawFile(string rawFile,int width,int height,int bits)
        {
            IsRawFormat = true;
            this.Columns = width;
            this.Rows = height;
            this.BitsStored = bits;

            if(bits > 8)
            {
                this.BytePerPixel = 2;
            }
            else
            {
                this.BytePerPixel = 1;
            }

            var buffer = System.IO.File.ReadAllBytes(rawFile);

            if (buffer.Length != (this.Columns * this.Rows * this.BytePerPixel))
                throw new Exception("Raw format error!");

            WriteableBitmap writeableBitmap = ConvertUtil.GetWriteableBitmap(buffer, this.Columns, this.Rows, this.BitsStored);
            var imageSource = ConvertUtil.GetImageSource(writeableBitmap);
            this.PreviewImage = imageSource;
            this.ThumbnailImage = ConvertUtil.GetImageSourceThumbnail(writeableBitmap, 256, 256);
        }

        public static ushort[] ConvertRaw12BitBytesToUshortArray(byte[] rawData)
        {
            if (rawData.Length % 2 != 0)
                throw new ArgumentException("Raw byte array length must be even for 16-bit words.");

            int pixelCount = rawData.Length / 2;
            ushort[] result = new ushort[pixelCount];

            for (int i = 0; i < pixelCount; i++)
            {
                ushort value = (ushort)(rawData[2 * i] | (rawData[2 * i + 1] << 8));
                result[i] = (ushort)(value & 0x0FFF);
            }

            return result;
        }
    }
}
