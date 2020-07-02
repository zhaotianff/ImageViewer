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

        public bool HasImage { get; set; } = false;
        private Point LastZoomPoint { get; set; }

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

                SetWindowInfo(ww, wl);

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

                HasImage = true;
                ResetZoomPoint();
                return true;
            }
            catch
            {
                return false;
            }
        } 
        
        public bool CloseImage()
        {
            try
            {
                this.image.Source = null;
                HasImage = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SetWindowInfo(double ww,double wl)
        {
            this.lbl_WL.Content = $"WL:{wl}";
            this.lbl_WW.Content = $"WW:{ww}";
        }
        
        public void ShowWindowInfo()
        {
            stackpanel_Window.Visibility = Visibility.Visible;
        }

        public void HideWindowInfo()
        {
            stackpanel_Window.Visibility = Visibility.Hidden;
        }

        private void ZoomImage(Point point, int delta)
        {
            if (delta < 0 && scaleTransform.ScaleX < 0.2)
                return;

            if (delta > 0 && scaleTransform.ScaleX > 3)
                return;

            if (delta != 0)
            {
                var ratio = 0.0;
                if (delta > 0)
                {
                    ratio = scaleTransform.ScaleX * 0.2;
                }
                else
                {
                    ratio = scaleTransform.ScaleX * -0.2;
    
                }
                scaleTransform.CenterX = this.image.ActualWidth / 2.0;
                scaleTransform.CenterY = this.image.ActualHeight / 2.0;

                //TODO use animation
                scaleTransform.ScaleX += ratio;
                scaleTransform.ScaleY = Math.Abs(scaleTransform.ScaleX);       
            }
        }

        private void Border_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomImage(e.GetPosition(this.image), e.Delta);
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.RightButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(this.image);

                if (LastZoomPoint.X == 0 && LastZoomPoint.Y == 0)
                {
                    LastZoomPoint = point;
                    return;
                }

                var xPos = point.X - LastZoomPoint.X;
                var yPos = point.Y - LastZoomPoint.Y;

                //TODO fix direction
                //use direction to determine zoom in/out

                if( (xPos > 20  && yPos > 20) || (xPos < 20 && yPos > 20) )
                {
                    ZoomImage(point, -110);
                    LastZoomPoint = point;
                }

                if((xPos > 20 && yPos < 20) || (xPos < 20 && yPos < 20))
                {
                    ZoomImage(point, 110);
                    LastZoomPoint = point;
                }
            }
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ResetZoomPoint();
        }

        private void ResetZoomPoint()
        {
            LastZoomPoint = new Point();
        }
    }
}
