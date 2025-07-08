using DicomViewCtrl.Dicom.Data;
using DicomViewCtrl.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DicomViewCtrl.Dicom.Process
{
    internal class DicomImageProcessor
    {
        public static void UpdateWindow(DicomFile dicomFile, double windowWidth, double windowCenter)
        {
            if (dicomFile.BitsStored == 8)
            {
                InternalUpdate8BitsWindow(dicomFile, windowWidth, windowCenter);
            }
            else
            {
                InternalUpdate16BitsWindow(dicomFile, windowWidth, windowCenter);
            }
        }

        public static (double,double) GetAutoWindow(DicomFile dicomFile)
        {
            if (dicomFile.BitsStored == 8)
            {
                return GetAutoWindow8Bit(dicomFile);
            }
            else
            {
                return GetAutoWindow16Bit(dicomFile);
            }
        }

        private static (double, double) GetAutoWindow8Bit(DicomFile dicomFile)
        {
            var orderBuffer = dicomFile.ImageData.OrderBy(x => x).Select(x => (byte)(x * dicomFile.RescaleSlope + dicomFile.RescaleIntercept)).ToArray();

            double low = PercentileByte(orderBuffer, 0.005);  // 0.5%
            double high = PercentileByte(orderBuffer, 0.995); // 99.5%

            double WW = high - low;
            double WL = (high + low) / 2.0;
            return (WW, WL);
        } 

        private static double PercentileByte(byte[] sortedData, double percentile)
        {
            if (sortedData == null || sortedData.Length == 0)
                throw new ArgumentException("Data cannot be null or empty.");

            if (percentile < 0 || percentile > 1)
                throw new ArgumentOutOfRangeException(nameof(percentile), "Percentile must be between 0 and 1.");

            double position = (sortedData.Length - 1) * percentile;
            int lowerIndex = (int)Math.Floor(position);
            int upperIndex = (int)Math.Ceiling(position);

            if (lowerIndex == upperIndex)
                return sortedData[lowerIndex];

            double lowerValue = sortedData[lowerIndex];
            double upperValue = sortedData[upperIndex];
            double fraction = position - lowerIndex;

            return lowerValue + (upperValue - lowerValue) * fraction;
        }

        /// <summary>
        /// 使用百分位法计算DICOM图像的自动窗宽和窗位(From doubao AI)
        /// </summary>
        /// <param name="pixels">DICOM图像像素数据</param>
        /// <param name="lowPercentile">低百分位值(0-1之间)</param>
        /// <param name="highPercentile">高百分位值(0-1之间)</param>
        /// <returns>包含窗宽和窗位的元组</returns>
        public static (double WindowWidth, double WindowCenter) GetAutoWindow16Bit(DicomFile dicomFile, double lowPercentile = 0.05, double highPercentile = 0.95)
        {
            ushort[] pixels = new ushort[dicomFile.ImageData.Length / 2];
            ConvertUtil.CopyByteArrayToUshortArray(dicomFile.ImageData, pixels);

            // 步骤1: 统计图像像素值范围
            ushort minPixel = ushort.MaxValue;
            ushort maxPixel = ushort.MinValue;

            foreach (var pixel in pixels)
            {
                if (pixel < minPixel) minPixel = pixel;
                if (pixel > maxPixel) maxPixel = pixel;
            }

            // 步骤2: 构建灰度直方图
            int[] histogram = new int[maxPixel - minPixel + 1];
            foreach (var pixel in pixels)
            {
                int binIndex = pixel - minPixel;
                histogram[binIndex]++;
            }

            // 步骤3: 计算累积分布函数(CDF)
            double[] cdf = new double[histogram.Length];
            cdf[0] = histogram[0];

            for (int i = 1; i < histogram.Length; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }

            // 归一化CDF到[0,1]范围
            double totalPixels = cdf[histogram.Length - 1];
            for (int i = 0; i < cdf.Length; i++)
            {
                cdf[i] /= totalPixels;
            }

            // 步骤4: 确定百分位对应的灰度值
            ushort pLowValue = minPixel;
            ushort pHighValue = maxPixel;

            // 查找低百分位对应的灰度值
            for (int i = 0; i < cdf.Length; i++)
            {
                if (cdf[i] >= lowPercentile)
                {
                    pLowValue = (ushort)(i + minPixel);
                    break;
                }
            }

            // 查找高百分位对应的灰度值
            for (int i = cdf.Length - 1; i >= 0; i--)
            {
                if (cdf[i] <= highPercentile)
                {
                    pHighValue = (ushort)(i + minPixel);
                    break;
                }
            }

            // 步骤5: 计算窗宽和窗位
            double windowCenter = (pHighValue + pLowValue) / 2.0;
            double windowWidth = pHighValue - pLowValue;

            // 确保窗宽至少为1
            if (windowWidth < 1)
                windowWidth = 1;

            return (windowWidth, windowCenter);
        }

        private static void InternalUpdate8BitsWindow(DicomFile dicomFile, double windowWidth, double windowCenter)
        {
            double min = windowCenter - windowWidth / 2.0;
            double max = windowCenter + windowWidth / 2.0;

            byte[] windowBuffer = new byte[dicomFile.Rows * dicomFile.Columns];

            for (int i = 0; i < dicomFile.ImageData.Length; i++)
            {
                byte value = (byte)(dicomFile.ImageData[i] * dicomFile.RescaleSlope + dicomFile.RescaleIntercept);

                if (value <= min)
                    windowBuffer[i] = 0;
                else if (value >= max)
                    windowBuffer[i] = 255;
                else
                    windowBuffer[i] = (byte)(((value - min) / (max - min)) * 255.0);
            }

            dicomFile.PreviewImage.WritePixels(
                new Int32Rect(0, 0, dicomFile.Columns, dicomFile.Rows),
                windowBuffer,
                dicomFile.Columns,
                0
            );
        }

        private static void InternalUpdate16BitsWindow(DicomFile dicomFile, double windowWidth, double windowCenter)
        {
            double min = windowCenter - windowWidth / 2.0;
            double max = windowCenter + windowWidth / 2.0;

            //Elapse 16ms
            ushort[] target16BitUshortArray = new ushort[dicomFile.ImageData.Length / 2];
            ConvertUtil.CopyByteArrayToUshortArray(dicomFile.ImageData, target16BitUshortArray);

            //Elapse 54ms
            for (int i = 0; i < target16BitUshortArray.Length; i++)
            {
                double value = target16BitUshortArray[i] * dicomFile.RescaleSlope + dicomFile.RescaleIntercept;

                if (value <= min)
                    target16BitUshortArray[i] = 0;
                else if (value >= max)
                    target16BitUshortArray[i] = 65535;
                else
                    target16BitUshortArray[i] = (ushort)(((value - min) / (max - min)) * 65535.0);
            }

            //Elapse 56ms
            dicomFile.PreviewImage.WritePixels(
                new Int32Rect(0, 0, dicomFile.Columns, dicomFile.Rows),
                target16BitUshortArray,
                dicomFile.Columns * 2,
                0
            );
        }
    }
}
