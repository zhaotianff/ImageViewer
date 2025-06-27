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
        private const float MIN_ZOOMRATIO = 0.1f;
        private const float MAX_ZOOMRATIO = 3f;
        private const float ZOOM_STEP = 0.001f;
        private const float MULTI_FRAME_TRANSLATE_Y_OFFSET = -30;

        private float currentRatio = 1.0f;
        private Point startZoomPoint;
        private Point startMovePoint;
        private ProcType procType = ProcType.None;
        private Dicom.Data.DicomFile dicomFile;

        public ObservableCollection<DicomImage> ImageList { get; private set; } = new ObservableCollection<DicomImage>();

        public bool HasImage { get; set; } = false;       

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
            AutoFit();
            HasImage = true;
            AddToImageList(dicomFile);
            return true;
        }

        public bool PrefetchImage(string dicomFilePath)
        {
            DicomFile localDicomFile = new DicomFile(dicomFilePath);
            localDicomFile.OpenAsPrefetch();
            AddToImageList(localDicomFile);
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
            AutoFit();
            HasImage = true;
            AddToImageList(dicomFile);
            return true;
        }

        public bool OpenRaw(string rawFilePath,int width,int height,int bits)
        {
            dicomFile = new DicomFile(rawFilePath);
            dicomFile.OpenAsRawFile(width, height, bits);
            SetWindowInfo(dicomFile.WindowWidth, dicomFile.WindowCenter);
            this.image.Source = dicomFile.PreviewImage;
            AutoFit();
            HasImage = true;
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

            DispatcherHelper.DispatherInvoke(() => { this.ImageList.Add(dicomImage); });
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

        private void ZoomImage(int delta)
        {
            double zoomFactor = delta > 0 ? 1.1 : 1 / 1.1;
            double oldScale = scaleTransform.ScaleX;
            double newScale = oldScale * zoomFactor;

            (var deltaWidth, var deltaHeight) = GetZoomImageOffset(newScale, newScale);

            translateTransform.X -= deltaWidth / 2;
            translateTransform.Y -= deltaHeight / 2;

            scaleTransform.ScaleX = scaleTransform.ScaleY = newScale;
        }

        private void ZoomImage(Point current)
        {
            Vector delta = current - startZoomPoint;

            double dx = delta.X;
            double dy = delta.Y;

            double zoomDelta = 0;

            if (dx < 0 && dy > 0)
            {
                zoomDelta = delta.Length * ZOOM_STEP;
            }
            else if (dx > 0 && dy < 0)
            {
                zoomDelta = -delta.Length * ZOOM_STEP;
            }
            else
            {
                startZoomPoint = current;
                return;
            }

            double newScaleX = scaleTransform.ScaleX + zoomDelta;
            double newScaleY = scaleTransform.ScaleY + zoomDelta;

            newScaleX = Math.Max(MIN_ZOOMRATIO, Math.Min(MAX_ZOOMRATIO, newScaleX));
            newScaleY = Math.Max(MIN_ZOOMRATIO, Math.Min(MAX_ZOOMRATIO, newScaleY));

            (var deltaWidth, var deltaHeight) = GetZoomImageOffset(newScaleX, newScaleY);
            translateTransform.X -= deltaWidth / 2;
            translateTransform.Y -= deltaHeight / 2;

            scaleTransform.ScaleX = newScaleX;
            scaleTransform.ScaleY = newScaleY;

            startZoomPoint = current;
        }

        private Tuple<double,double> GetZoomImageOffset(double newScaleX,double newScaleY)
        {
            double imageWidthOld = this.dicomFile.Columns * scaleTransform.ScaleX;
            double imageHeightOld = this.dicomFile.Rows * scaleTransform.ScaleY;

            double imageWidthNew = this.dicomFile.Columns * newScaleX;
            double imageHeightNew = this.dicomFile.Rows * newScaleY;

            double deltaWidth = imageWidthNew - imageWidthOld;
            double deltaHeight = imageHeightNew - imageHeightOld;

            return new Tuple<double, double>(deltaWidth, deltaHeight);
        }

        private void MoveImage(Point current)
        {
            Point currentImagePos = ToImageSpace(current);

            Vector deltaImage = currentImagePos - startMovePoint;

            translateTransform.X += deltaImage.X * scaleTransform.ScaleX;
            translateTransform.Y += deltaImage.Y * scaleTransform.ScaleY;

            startMovePoint = ToImageSpace(current);
        }

        private Point GetTransformedCenter()
        {
          
            double width = this.image.ActualWidth;
            double height = this.image.ActualHeight;

            // Original center
            Point center = new Point(width / 2, height / 2);

            // Get the RenderTransform (TransformGroup) and transform the point
            GeneralTransform transform = this.image.TransformToAncestor((Visual)this.image.Parent);
            Point transformedCenter = transform.Transform(center);

            return transformedCenter;
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomImage(e.Delta);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            switch(procType)
            {
                case ProcType.ZoomImage:
                    ZoomImage(e.GetPosition(this.canvas));
                    break;
                case ProcType.MoveImage:
                    MoveImage(e.GetPosition(this.canvas));
                    break;
                default:
                    break;
            }
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            procType = ProcType.ZoomImage;
            startZoomPoint = e.GetPosition(this.canvas);
            this.canvas.CaptureMouse();
        }

        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            procType = ProcType.None;
            this.canvas.ReleaseMouseCapture();
        }

        private void AutoFit()
        {
            AutoSize();
            AutoPos();
        }

        private void AutoSize()
        {
            if (this.dicomFile.Rows < this.canvas.ActualHeight
                && this.dicomFile.Columns < this.canvas.ActualWidth)
            {
                scaleTransform.ScaleX = 1;
                scaleTransform.ScaleY = 1;
                return;
            }

            double scale = 1;

            if (this.dicomFile.Columns > this.dicomFile.Rows)
            {
                scale = this.canvas.ActualHeight / this.dicomFile.Rows;
            }
            else
            {
                scale = this.canvas.ActualWidth / this.dicomFile.Columns;

            }

            this.scaleTransform.CenterX = 0;
            this.scaleTransform.CenterY = 0;

            this.scaleTransform.ScaleX = scale;
            this.scaleTransform.ScaleY = scale;
        }

        private void AutoPos()
        {
            var translatex = (this.canvas.ActualWidth - (this.dicomFile.Columns * this.scaleTransform.ScaleX) ) / 2;
            var translatey = (this.canvas.ActualHeight - (this.dicomFile.Rows * this.scaleTransform.ScaleY)) / 2;
            var yOffset = this.dicomFile.NumberOfFrames > 1 ? MULTI_FRAME_TRANSLATE_Y_OFFSET : 0;

            this.translateTransform.X = translatex;
            this.translateTransform.Y = translatey + yOffset;
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            procType = ProcType.MoveImage;
            this.Cursor = Cursors.ScrollAll;
            var canvasPoint = e.GetPosition(this.canvas);
            startMovePoint = ToImageSpace(canvasPoint);
            this.canvas.CaptureMouse();
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            procType = ProcType.None;
            this.Cursor = Cursors.Arrow;
            this.canvas.ReleaseMouseCapture();
        }

        private Point ToImageSpace(Point canvasPoint)
        {
            return new Point((canvasPoint.X - translateTransform.X) / scaleTransform.ScaleX,(canvasPoint.Y - translateTransform.Y) / scaleTransform.ScaleY);
        }
    }
}
