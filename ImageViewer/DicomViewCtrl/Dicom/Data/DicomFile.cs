using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using DicomViewCtrl.Util;
using System.Windows.Media;
using System.Data;
using System.Windows.Media.Imaging;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.NativeCodec;
using FellowOakDicom.Imaging.Render;
using FellowOakDicom.Imaging.Codec;
using System.Windows;

namespace DicomViewCtrl.Dicom.Data
{
    internal class DicomFile
    {
        private static readonly string[] SupportedPhotometricInterpretation = { "MONOCHROME2" };

        private const int ThumbnailWidth = 48;
        private const int ThumbnailHeight = 48;

        private FellowOakDicom.DicomFile dicomFile = null;

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public int BitsStored { get; private set; }
        public int BytePerPixel { get; private set; }

        public byte[] ImageData { get; internal set; }

        internal DicomPixelData UncompressedPixelData { get; set; }

        public ImageSource ThumbnailImage { get; private set; }

        public WriteableBitmap PreviewImage { get; internal set; }

        public string FilePath { get; private set; }

        public double WindowWidth { get; private set; }
        
        public double WindowCenter { get; private set; }

        public string Title { get; private set; }

        public string SopInstanceUID { get; private set; }

        public string PatientName { get; private set; }

        public string StudyDate { get; private set; }

        public string StudyTime { get; private set; }

        public bool IsRawFormat { get; private set; }

        public int NumberOfFrames { get; private set; }

        public int FrameIndex { get; private set; } = 0;

        public double RescaleSlope { get; private set; }

        public double RescaleIntercept { get; private set; }

        internal FellowOakDicom.DicomDataset DataSet => this.dicomFile?.Dataset;


        static DicomFile()
        {
            new FellowOakDicom.DicomSetupBuilder().
                RegisterServices(s => s.AddFellowOakDicom().
                AddTranscoderManager<FellowOakDicom.Imaging.NativeCodec.NativeTranscoderManager>()).
                SkipValidation().
                Build();
        }

        public DicomFile(string filePath)
        {
            this.FilePath = filePath;
        }     
        
        public void OpenAsDicomFile(int frameIndex)
        {
            var upperFileExtension = System.IO.Path.GetExtension(this.FilePath).ToUpper();

            if (upperFileExtension == ".DCM")
            {
                OpenDicomFile(FilePath, 0);
            }
        }

        public void OpenAsPrefetch()
        {
            try
            {
                dicomFile = FellowOakDicom.DicomFile.Open(FilePath);           
                ReadCommonDicomTag(dicomFile);
                InternalGetThumbnail(dicomFile);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task OpenAsDicomFileAsync(int frameIndex)
        {
            var upperFileExtension = System.IO.Path.GetExtension(this.FilePath).ToUpper();

            if (upperFileExtension != ".DCM")
                return;

            if (!string.IsNullOrEmpty(FilePath) && IsRawFormat == false)
            {
                await OpenDicomFileAsync(FilePath, 0);
            }
        }

        public void OpenDicomFrame(int frameIndex)
        {
            if (this.dicomFile == null)
            {
                throw new Exception("Not open yet");
            }

            if (frameIndex == this.FrameIndex)
                return;

            InternalOpenFrameAndPreview(this.dicomFile, frameIndex);
        }

        public void OpenAsRawFile(int width,int height,int bits)
        {
            OpenRawFile(this.FilePath, width, height, bits);
        }

        private void OpenDicomFile(string dicomFilePath, int frameIndex = 0)
        {
            try
            {
                dicomFile = FellowOakDicom.DicomFile.Open(dicomFilePath);
                var uid = dicomFile.Dataset.InternalTransferSyntax.UID.UID;
                var photometricInterpretation = GetPhotometricInterpretation(dicomFile);
                if (SupportedPhotometricInterpretation.Contains(photometricInterpretation) == false)
                    throw new Exception("Not supported PhotometricInterpretation");

                ReadCommonDicomTag(dicomFile);
                InternalOpenFrameAndPreview(dicomFile, frameIndex);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void InternalOpenFrameAndPreview(FellowOakDicom.DicomFile dicomFile, int frameIndex)
        {
            var pixelData = ReadDicomPixelData(dicomFile, frameIndex);

            if (pixelData == null)
                throw new Exception("Pixel data is null");

            ImageData = new byte[this.Rows * this.Columns * BytePerPixel];
            Array.Copy(pixelData, this.ImageData, pixelData.Length);

            this.PreviewImage = ConvertUtil.GetWriteableBitmap(this.ImageData, this.Columns, this.Rows, this.BitsStored);

            this.FrameIndex = frameIndex;
   
            this.ThumbnailImage = ConvertUtil.GetImageSourceThumbnail(this.PreviewImage, ThumbnailWidth, ThumbnailHeight);
        }

        private void InternalGetThumbnail(FellowOakDicom.DicomFile dicomFile)
        {
            var pixelData = ReadDicomPixelData(dicomFile, 0);

            if (pixelData == null)
                throw new Exception("Pixel data is null");

            ImageData = new byte[this.Rows * this.Columns * BytePerPixel];
            Array.Copy(pixelData, this.ImageData, pixelData.Length);

            WriteableBitmap writeableBitmap = ConvertUtil.GetWriteableBitmap(this.ImageData, this.Columns, this.Rows, this.BitsStored);
            DispatcherHelper.DispatherInvoke(() =>
            {
                this.ThumbnailImage = ConvertUtil.GetImageSourceThumbnail(writeableBitmap, ThumbnailWidth, ThumbnailHeight);
            });
        }

        private async Task OpenDicomFileAsync(string dicomFilePath, int frameIndex = 0)
        {
            try
            {
                dicomFile = await FellowOakDicom.DicomFile.OpenAsync(dicomFilePath);
                var uid = dicomFile.Dataset.InternalTransferSyntax.UID.UID;
                var photometricInterpretation = GetPhotometricInterpretation(dicomFile);
                if (SupportedPhotometricInterpretation.Contains(photometricInterpretation) == false)
                    throw new Exception("Not supported PhotometricInterpretation");

                ReadCommonDicomTag(dicomFile);
                var pixelData = ReadDicomPixelData(dicomFile, frameIndex);

                if (pixelData == null)
                    throw new Exception("Pixel data is null");

                ImageData = new byte[this.Rows * this.Columns * BytePerPixel];
                Array.Copy(pixelData, this.ImageData, pixelData.Length);

                this.PreviewImage = ConvertUtil.GetWriteableBitmap(this.ImageData, this.Columns, this.Rows, this.BitsStored);
                this.FrameIndex = frameIndex;
                this.ThumbnailImage = ConvertUtil.GetImageSourceThumbnail(this.PreviewImage, ThumbnailWidth, ThumbnailHeight);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private byte[] ReadDicomPixelData(FellowOakDicom.DicomFile dicomFile,int frameIndex)
        {
            if (dicomFile.Dataset.InternalTransferSyntax.IsEncapsulated == false)
            {
                if(UncompressedPixelData == null)
                {
                    UncompressedPixelData = DicomPixelData.Create(dicomFile.Dataset);
                }
                return UncompressedPixelData.GetFrame(frameIndex).Data;
            }

            if (UncompressedPixelData == null)
            {
                var transcoder = new DicomTranscoder(dicomFile.Dataset.InternalTransferSyntax, DicomTransferSyntax.ExplicitVRLittleEndian);
                var transcoded = transcoder.Transcode(dicomFile.Dataset);
                UncompressedPixelData = DicomPixelData.Create(transcoded);
            }
            return UncompressedPixelData.GetFrame(frameIndex).Data;
        }

        private void ReadCommonDicomTag(FellowOakDicom.DicomFile dicomFile)
        {
            this.BitsStored = dicomFile.Dataset.GetSingleValue<int>(DicomTag.BitsStored);
            this.Columns = dicomFile.Dataset.GetSingleValue<int>(DicomTag.Columns);
            this.Rows = dicomFile.Dataset.GetSingleValue<int>(DicomTag.Rows);
            dicomFile.Dataset.TryGetValue<double>(DicomTag.WindowWidth, 0, out double ww);
            dicomFile.Dataset.TryGetValue<double>(DicomTag.WindowCenter, 0, out double wl);
            this.WindowWidth = ww;
            this.WindowCenter = wl;
            dicomFile.Dataset.TryGetValue<string>(DicomTag.StudyDescription, 0,out string title);
            this.Title = title;
            this.SopInstanceUID = dicomFile.Dataset.GetValue<string>(DicomTag.SOPInstanceUID,0);
            this.PatientName = dicomFile.Dataset.GetValue<string>(DicomTag.PatientName, 0);
            var studyDate = "";
            var studyTime = "";
            dicomFile.Dataset.TryGetValue<string>(DicomTag.StudyDate, 0,out studyDate);
            dicomFile.Dataset.TryGetValue<string>(DicomTag.StudyTime, 0, out studyTime);
            this.StudyDate = studyDate;
            this.StudyTime = studyTime;

            var samplesPerPixel = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.SamplesPerPixel, 1); // 1 for grayscale, 3 for RGB
            var bitsAllocated = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.BitsAllocated, 8);

            this.BytePerPixel = (bitsAllocated / 8) * samplesPerPixel;
            this.NumberOfFrames = dicomFile.Dataset.GetSingleValueOrDefault(DicomTag.NumberOfFrames, 1);

            this.RescaleSlope = this.dicomFile.Dataset.GetValueOrDefault<double>(DicomTag.RescaleSlope, 0, 1.0);      
            this.RescaleIntercept = this.dicomFile.Dataset.GetValueOrDefault<double>(DicomTag.RescaleIntercept, 0, 0.0); 
        }

        public string GetPhotometricInterpretation(FellowOakDicom.DicomFile dicomFile)
        {
            return dicomFile.Dataset.GetSingleValue<string>(DicomTag.PhotometricInterpretation).Trim().Replace("\0","").ToUpper();
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

            this.PreviewImage = ConvertUtil.GetWriteableBitmap(buffer, this.Columns, this.Rows, this.BitsStored);
            this.ThumbnailImage = ConvertUtil.GetImageSourceThumbnail(this.PreviewImage, ThumbnailWidth, ThumbnailHeight);
        }

        public void UpdateWindowWidthAndLevel(ref double windowWidth,ref double windowLevel)
        {
            (var min, var max) = DicomUtil.GetWindowInfoLimit(this.BitsStored);

            if (windowWidth > max)
                windowWidth = max;

            if (windowWidth < min)
                windowWidth = min;

            if (windowLevel > max)
                windowLevel = max;

            if (windowLevel < min)
                windowLevel = min;

            InternalUpdateWindow(windowWidth, windowLevel);

            this.WindowWidth = windowWidth;
            this.WindowCenter = windowLevel;
        }

        private void InternalUpdateWindow(double windowWidth, double windowCenter)
        {
            if(this.BitsStored == 8)
            {
                InternalUpdate8BitsWindow(windowWidth, windowCenter);
            }
            else
            {
                InternalUpdate16BitsWindow(windowWidth, windowCenter);
            }
        }

        private void InternalUpdate8BitsWindow(double windowWidth, double windowCenter)
        {
            double min = windowCenter - windowWidth / 2.0;
            double max = windowCenter + windowWidth / 2.0;

            byte[] windowBuffer = new byte[this.Rows * this.Columns];

            for (int i = 0; i < this.ImageData.Length; i++)
            {
                byte value = (byte)(this.ImageData[i] * this.RescaleSlope + this.RescaleIntercept);

                if (value <= min)
                    windowBuffer[i] = 0;
                else if (value >= max)
                    windowBuffer[i] = 255;
                else
                    windowBuffer[i] = (byte)(((value - min) / (max - min)) * 255.0);
            }

            this.PreviewImage.WritePixels(
                new Int32Rect(0, 0, this.Columns, this.Rows),
                windowBuffer,
                this.Columns,
                0
            );
        }

        private void InternalUpdate16BitsWindow(double windowWidth,double windowCenter)
        {
            double min = windowCenter - windowWidth / 2.0;
            double max = windowCenter + windowWidth / 2.0;

            //Elapse 16ms
            ushort[] target16BitUshortArray = new ushort[this.ImageData.Length / 2];
            ConvertUtil.CopyByteArrayToUshortArray(this.ImageData, target16BitUshortArray);

            //Elapse 54ms
            for (int i = 0; i < target16BitUshortArray.Length; i++)
            {
                double value = target16BitUshortArray[i] * this.RescaleSlope + this.RescaleIntercept;

                if (value <= min)
                    target16BitUshortArray[i] = 0;
                else if (value >= max)
                    target16BitUshortArray[i] = 65535;
                else
                    target16BitUshortArray[i] = (ushort)(((value - min) / (max - min)) * 65535.0);
            }

            //Elapse 56ms
            this.PreviewImage.WritePixels(
                new Int32Rect(0, 0, this.Columns, this.Rows),
                target16BitUshortArray,
                this.Columns * 2,
                0
            );
        }

        public void ManualUpdateThumbnail()
        {
            this.ThumbnailImage = ConvertUtil.GetImageSourceThumbnail(this.PreviewImage, ThumbnailWidth, ThumbnailHeight);
        }
    }
}
