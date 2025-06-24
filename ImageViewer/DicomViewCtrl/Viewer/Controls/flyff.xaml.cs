using DicomViewCtrl.Dicom.Data;
using DicomViewCtrl.Util;
using DicomViewCtrl.Viewer.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DicomViewCtrl
{
    /// <summary>
    /// flyff.xaml 的交互逻辑
    /// </summary>
    public partial class flyff : UserControl
    {
        private const float MIN_ZOOMRATIO = 0.2f;
        private const float MAX_ZOOMRATIO = 3f;
        private const float ZOOM_STEP = 0.2f;

        private float currentRatio = 1.0f;

        private Dicom.Data.DicomFile dicomFile;

        public ObservableCollection<DicomImage> ImageList { get; private set; } = new ObservableCollection<DicomImage>();

        public bool HasImage { get; set; } = false;
        private Point LastZoomPoint { get; set; }

        public int FrameIndex
        {
            get
            {
                if (dicomFile != null)
                    return dicomFile.FrameIndex;

                return -1;
            }
        }

        public flyff()
        {
            InitializeComponent();
        }

        /// <summary>
        /// open and show dicom file
        /// </summary>
        /// <param name="dicmFile"></param>
        /// <returns></returns>
        public bool OpenImage(string dicomFilePath)
        {
            dicomFile = new DicomFile(dicomFilePath);
            dicomFile.OpenAsDicomFile(0);
            SetWindowInfo(dicomFile.WindowWidth, dicomFile.WindowCenter);
            this.image.Source = dicomFile.PreviewImage;
            HasImage = true;
            ResetZoomPoint();
            AddToImageList(dicomFile);
            return true;
        }

        public bool PrefetchImage(string dicomFilePath)
        {
            return true;
        }

        public bool FetchFrame(int imageIndex,int frameIndex)
        {
            if (this.dicomFile == null)
                return false;

            if (this.FrameIndex >= this.ImageList[imageIndex].FrameList.Count)
                return false;

            dicomFile.OpenDicomFrame(frameIndex);
            this.image.Source = dicomFile.PreviewImage;

            if (this.ImageList[imageIndex].FrameList[frameIndex].FrameThumbnail == null)
            {
                this.ImageList[imageIndex].FrameList[frameIndex].FrameThumbnail = dicomFile.ThumbnailImage;
            }     

            return true;
        }

        /// <summary>
        /// open and show dicom file async
        /// </summary>
        /// <param name="dicmFile"></param>
        /// <returns></returns>
        public async Task<bool> OpenImageAsync(string dicomFilePath)
        {
            dicomFile = new DicomFile(dicomFilePath);
            await dicomFile.OpenAsDicomFileAsync(0);
            SetWindowInfo(dicomFile.WindowWidth, dicomFile.WindowCenter);
            this.image.Source = dicomFile.PreviewImage;
            HasImage = true;
            ResetZoomPoint();
            AddToImageList(dicomFile);
            return true;
        }

        public bool OpenRaw(string rawFilePath,int width,int height,int bits)
        {
            dicomFile = new DicomFile(rawFilePath);
            dicomFile.OpenAsRawFile(width, height, bits);
            SetWindowInfo(dicomFile.WindowWidth, dicomFile.WindowCenter);
            this.image.Source = dicomFile.PreviewImage;
            HasImage = true;
            ResetZoomPoint();
            AddToImageList(dicomFile);
            return true;
        }

        private void AddToImageList(DicomFile dicomFile)
        {
            if (this.ImageList.FirstOrDefault(x => x.SopInstanceUID == dicomFile.SopInstanceUID) != null)
                return;

            DicomImage dicomImage = new DicomImage();
            dicomImage.FilePath = dicomFile.FilePath;
            dicomImage.IsRaw = dicomFile.IsRawFormat;
            dicomImage.Thumbnail = dicomFile.ThumbnailImage;
            dicomImage.Width = dicomFile.Columns;
            dicomImage.Height = dicomFile.Rows;
            dicomImage.Bits = dicomFile.BitsStored;
            dicomImage.Title = dicomFile.Title ?? "No description";
            dicomImage.SopInstanceUID = dicomFile.SopInstanceUID;
            dicomImage.StudyDateTime = dicomFile.StudyDate + " " + dicomFile.StudyTime;
            dicomImage.PatientName = dicomFile.PatientName;

            FetchAllFrames(dicomFile, dicomImage);

            this.ImageList.Add(dicomImage);
        }

        private void FetchAllFrames(DicomFile dicomFile, DicomImage dicomImage)
        {
            if (dicomFile.NumberOfFrames <= 1)
                return;

            dicomImage.FrameList = new ObservableCollection<FrameImage>(Enumerable.Range(0, dicomFile.NumberOfFrames).
                Select(x => new FrameImage() { FrameIndex = x, FrameThumbnail = null }));

            dicomImage.FrameList[0].FrameThumbnail = dicomFile.ThumbnailImage;
        }


        public void PutImage(byte[] imageData, int width, int height, int bits)
        {
            int bytePerPixel = bits > 8 ? 2 : 1;

            if (imageData.Length != (width * height * bytePerPixel))
                throw new Exception("Image data error!");
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
            if (delta == 0)
                return;

            if (delta < 0 && scaleTransform.ScaleX < MIN_ZOOMRATIO)
                return;

            if (delta > 0 && scaleTransform.ScaleX > MAX_ZOOMRATIO)
                return;
                   
            var ratio = 0.0;
            if (delta > 0)
            {
                ratio = scaleTransform.ScaleX * ZOOM_STEP;
            }
            else
            {
                ratio = scaleTransform.ScaleX * -ZOOM_STEP;
    
            }
            scaleTransform.CenterX = this.image.ActualWidth / 2.0;
            scaleTransform.CenterY = this.image.ActualHeight / 2.0;

            //TODO use animation
            scaleTransform.ScaleX += ratio;
            scaleTransform.ScaleY = Math.Abs(scaleTransform.ScaleX);                
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

                if (Math.Abs(xPos) < 10 && Math.Abs(yPos) < 10)
                    return;

                //Hit test

                var ratio = currentRatio;
                if (xPos < 0)
                    ratio *= 1.1f;
                else
                    ratio *= 0.9f;

                LimitRatio(ref ratio);

                scaleTransform.CenterX = this.image.ActualWidth / 2.0;
                scaleTransform.CenterY = this.image.ActualHeight / 2.0;

                scaleTransform.ScaleX *= ratio / currentRatio;
                scaleTransform.ScaleY *= ratio / currentRatio;

                currentRatio = ratio;
                LastZoomPoint = point;
            }
        }

        private void LimitRatio(ref float ratio)
        {
            if (ratio > MAX_ZOOMRATIO)
                ratio = MAX_ZOOMRATIO;
            else if(ratio < MIN_ZOOMRATIO)
                ratio = MIN_ZOOMRATIO;
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
