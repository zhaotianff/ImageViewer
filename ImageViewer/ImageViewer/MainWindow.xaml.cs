using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Win32;
using Dicom;
using Dicom.Imaging;
using System.Runtime.InteropServices;

namespace ImageViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {

        }

        private void Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Dicom Files|*.dcm";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if(openFileDialog.ShowDialog() == true)
            {
                this.image.Source = GetImageSource(openFileDialog.FileName);
            }
        }

        private WriteableBitmap GetWriteableBitmap(string fileName)
        {
            var image = new DicomImage(fileName);

            //I need get raw data here
#pragma warning disable CS0618
            byte[] rawPixelData = image.PixelData.GetFrame(0).Data;

            WriteableBitmap writeableBitmap = new WriteableBitmap(image.Width, image.Height, 96.0, 96.0, PixelFormats.Gray8, BitmapPalettes.BlackAndWhite);

            //todo I should convert 12/16 bit image to 8-bit           
            writeableBitmap.Lock();
            Marshal.Copy(rawPixelData, 0, writeableBitmap.BackBuffer, image.Width * image.Height);
            Int32Rect int32Rect = new Int32Rect(0, 0, image.Width, image.Height);
            writeableBitmap.AddDirtyRect(int32Rect);
            writeableBitmap.Unlock();
            return writeableBitmap;
        }

        private byte[] ConvertTo8BitImage(int bit,byte[] rawPixelData)
        {
            if (bit <= 8)
                return rawPixelData;

            return new byte[1];
        }

        private ImageSource GetImageSource(string fileName)
        {
            var writeableBitmap = GetWriteableBitmap(fileName);
            TransformedBitmap bitmap = new TransformedBitmap();
            bitmap.BeginInit();
            bitmap.Source = writeableBitmap;
            bitmap.EndInit();
            return bitmap;
        }
    }
}
