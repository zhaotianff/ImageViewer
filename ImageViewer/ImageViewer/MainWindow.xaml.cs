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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using ImageViewer.Util;
using ImageViewer.Views.Dialog;

namespace ImageViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Controls.CustomWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Dicom Files|*.dcm";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                imgview.OpenImage(openFileDialog.FileName);
            }
        }

        private void OpenRaw_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Raw Files|*.raw;*.RAW|All files|*.*";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                InputImageSizeWindow inputImageSizeWindow = new InputImageSizeWindow();
                if (inputImageSizeWindow.ShowDialog() == true)
                {
                    imgview.OpenRaw(openFileDialog.FileName, inputImageSizeWindow.ImageWidth, inputImageSizeWindow.ImageHeight, inputImageSizeWindow.ImageBits);
                }

            }
        }

        private void OpenDicomDirectory_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
