using DicomViewCtrl.Controls;
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
using System.Windows.Resources;
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

        private const int SET_WL_Delta_8Bit = 2;
        private const int SET_WL_Delta_16Bit = 8;

        private Point startZoomPoint;
        private Point startMovePoint;
        private Point startWLPoint;
        private ProcType procType = ProcType.None;
        private Dicom.Data.DicomFile dicomFile;
        private MouseWheelMode mouseWheelMode = MouseWheelMode.None;
        private MouseLeftButtonMode mouseLeftButtonMode = MouseLeftButtonMode.None;
        private int frameImageIndex = -1;

        public ObservableCollection<DicomImage> ImageList { get; private set; } = new ObservableCollection<DicomImage>();

        private DicomInfo dicomInfo;
        
        public DicomInfo DicomInfo 
        {
            get
            {
                if(dicomInfo == null)
                {
                    dicomInfo = new DicomInfo(this.DicomTags);
                }

                return dicomInfo;
            }
        }

        private ObservableCollection<DicomTagWithValue> dicomTags;

        public ObservableCollection<DicomTagWithValue> DicomTags
        {
            get
            {
                if(dicomTags == null || dicomTags.Count == 0)
                {
                    ReadAllDicomTags();
                }
                
                return dicomTags;
            }
        }

        private bool hasImage = false;

        public bool HasImage 
        {
            get => hasImage;
            set
            {
                hasImage = value;
                SetAnnotationVisibility(value);
            }
        }   

        public int FrameIndex
        {
            get
            {
                if (dicomFile != null)
                    return dicomFile.FrameIndex;

                return -1;
            }
        }

        private int SetWLDelta
        {
            get
            {
                if (this.dicomFile.BitsStored <= 8)
                    return SET_WL_Delta_8Bit;

                return SET_WL_Delta_16Bit;
            }
        }

        /// <summary>
        /// Occurs when frame changed(mouse wheel scroll)
        /// </summary>
        public event Viewer.Event.FrameChangedEvent.FrameChangedEventHandler OnFrameChanged;


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
            SetWindow(dicomFile.WindowWidth, dicomFile.WindowCenter);
            this.image.Source = dicomFile.PreviewImage;
            AutoFit();
            LoadAnnotation();
            HasImage = true;
            AddToImageList(dicomFile);
            ResetImageViewStatus();
            return true;
        }

        public bool PrefetchImage(string dicomFilePath)
        {
            DicomFile localDicomFile = new DicomFile(dicomFilePath);
            localDicomFile.OpenAsPrefetch();
            AddToImageList(localDicomFile);
            ResetImageViewStatus();
            return true;
        }

        private void ResetImageViewStatus()
        {
            if (this.dicomTags != null)
            {
                dicomTags.Clear();
                dicomTags = null;
            }

            this.dicomInfo = null;
        }

        public bool FetchFrame(int imageIndex,int frameIndex)
        {
            if (this.dicomFile == null)
                return false;

            if (this.FrameIndex >= this.ImageList[imageIndex].FrameList.Count)
                return false;

            dicomFile.OpenDicomFrame(frameIndex);
            this.image.Source = dicomFile.PreviewImage;
            SetWindow(this.dicomFile.WindowWidth, this.dicomFile.WindowCenter);

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
            SetWindow(dicomFile.WindowWidth, dicomFile.WindowCenter);
            this.image.Source = dicomFile.PreviewImage;
            AutoFit();
            LoadAnnotation();
            HasImage = true;
            AddToImageList(dicomFile);
            ResetImageViewStatus();
            return true;
        }

        public bool OpenRaw(string rawFilePath,int width,int height,int bits)
        {
            dicomFile = new DicomFile(rawFilePath);
            dicomFile.OpenAsRawFile(width, height, bits);
            SetWindow(dicomFile.WindowWidth, dicomFile.WindowCenter);
            this.image.Source = dicomFile.PreviewImage;
            AutoFit();
            HasImage = true;
            AddToImageList(dicomFile);
            ResetImageViewStatus();
            return true;
        }

        private void AddToImageList(DicomFile dicomFile)
        {
            var checkDicomFile = this.ImageList.FirstOrDefault(x => x.SopInstanceUID == dicomFile.SopInstanceUID);
            if (checkDicomFile != null)
            {
                SelectFrameImage(this.ImageList.IndexOf(checkDicomFile));
                return;
            }

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
            SelectFrameImage(this.ImageList.Count - 1);
        }

        private void FetchAllFrames(DicomFile dicomFile, DicomImage dicomImage)
        {
            if (dicomFile.NumberOfFrames <= 1)
                return;

            dicomImage.FrameList = new ObservableCollection<FrameImage>(Enumerable.Range(0, dicomFile.NumberOfFrames).
                Select(x => new FrameImage() { FrameIndex = x, FrameThumbnail = null }));

            dicomImage.FrameList[0].FrameThumbnail = dicomFile.ThumbnailImage;

            ResetFrameImageSelectedIndex();
        }

        private void ResetFrameImageSelectedIndex()
        {
            frameImageIndex = -1;
        }

        private void SelectFrameImage(int imageIndex)
        {
            frameImageIndex = imageIndex;
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

        public void SetAutoWindow()
        {
            CheckImageOpenStatus();
            this.SetWindow(0, 0);
        }

        public void SetWindow(double ww,double wl)
        {
            this.dicomFile.UpdateWindowWidthAndLevel(ref ww, ref wl);

            UpdateDicomTagValue(Dicom.Data.DicomTags.WindowWidth, ww);
            UpdateDicomTagValue(Dicom.Data.DicomTags.WindowCenter, wl);

            UpdateScaleInfo();
        }

        private void ZoomImage(int delta)
        {
            CheckImageOpenStatus();

            double zoomFactor = delta > 0 ? 1.1 : 1 / 1.1;
            double newScaleX = scaleTransform.ScaleX * zoomFactor;
            double newScaleY = scaleTransform.ScaleY * zoomFactor;
            scaleTransform.ScaleX = newScaleX;
            scaleTransform.ScaleY = newScaleY;
        }

        private void ZoomImage(Point current)
        {
            CheckImageOpenStatus();

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

            if(scaleTransform.ScaleY < 0)
            {
                newScaleY = scaleTransform.ScaleY - zoomDelta;
            }

            if(scaleTransform.ScaleX < 0)
            {
                newScaleX = scaleTransform.ScaleX - zoomDelta;
            }

            LimitMaxScale(ref newScaleX, ref newScaleY);

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

        private void LimitMaxScale(ref double newScaleX,ref double newScaleY)
        {
            if(newScaleX >= 0)
            {
                newScaleX = Math.Max(MIN_ZOOMRATIO, Math.Min(MAX_ZOOMRATIO, newScaleX));
            }
            else
            {
                newScaleX = Math.Min(MIN_ZOOMRATIO, Math.Max(-MAX_ZOOMRATIO, newScaleX));
            }
            
            if(newScaleY >= 0)
            {
                newScaleY = Math.Max(MIN_ZOOMRATIO, Math.Min(MAX_ZOOMRATIO, newScaleY));
            }
            else
            {
                newScaleY = Math.Min(MIN_ZOOMRATIO, Math.Max(-MAX_ZOOMRATIO, newScaleY));
            }
        }

        private void MoveImage(Point current)
        {
            CheckImageOpenStatus();

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
            GeneralTransform transform = this.canvas.TransformToAncestor((Visual)this.canvas.Parent);
            Point transformedCenter = transform.Transform(center);

            return transformedCenter;
        }

        private void SetImageWL(Point current)
        {
            var delta = current - startWLPoint;
            this.startWLPoint = current;

            var newWW = this.dicomFile.WindowWidth + (int)delta.X * this.SetWLDelta;
            var newWL = this.dicomFile.WindowCenter + (int)delta.Y * this.SetWLDelta;

            (var min, var max) = DicomUtil.GetWindowInfoLimit(this.dicomFile.BitsStored);

            if (newWW > max)
                newWW = max;

            if (newWW < min)
                newWW = min;

            if (newWL > max)
                newWL = max;

            if (newWL < min)
                newWL = min;

            SetWindow(newWW, newWL);
            this.image.Source = this.dicomFile.PreviewImage;
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            switch (mouseWheelMode)
            {
                case MouseWheelMode.SwitchFrame:
                    SwitchFrame(e.Delta);
                    break;
                case MouseWheelMode.Zoom:
                    ZoomImage(e.Delta);
                    break;
                case MouseWheelMode.None:
                    break;
                default:
                    break;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            switch(procType)
            {
                case ProcType.ZoomImage:
                    ZoomImage(e.GetPosition(this.outer));
                    break;
                case ProcType.MoveImage:
                    MoveImage(e.GetPosition(this.outer));
                    break;
                case ProcType.SetWL:
                    SetImageWL(e.GetPosition(this.outer));
                    break;
                case ProcType.Magnifier:
                    Magnifier(e.GetPosition(this.image));
                    break;
                default:
                    break;
            }

            DisplayCurrentPointInfo(e.GetPosition(this.canvas));
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            procType = ProcType.ZoomImage;
            startZoomPoint = e.GetPosition(this.outer);
            this.outer.CaptureMouse();
        }

        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            procType = ProcType.None;
            this.outer.ReleaseMouseCapture();
        }

        private void AutoFit()
        {
            AutoSize();
            AutoPos();
        }

        public void SetAnnotationTags(ObservableCollection<DicomTagWithValue> leftTop,
            ObservableCollection<DicomTagWithValue> rightTop,
            ObservableCollection<DicomTagWithValue> leftBottom,
            ObservableCollection<DicomTagWithValue> rightBottom)
        {
            this.leftTopAnnList.AnnotationList = leftTop;
            this.rightTopAnnList.AnnotationList = rightTop;
            this.leftBottomAnnList.AnnotationList = leftBottom;
            this.rightBottomAnnList.AnnotationList = rightBottom;
        }

        private void LoadAnnotation()
        {
            FillDicomTagsValue(this.leftTopAnnList.AnnotationList);
            FillDicomTagsValue(this.rightTopAnnList.AnnotationList);
            FillDicomTagsValue(this.leftBottomAnnList.AnnotationList);
            FillDicomTagsValue(this.rightBottomAnnList.AnnotationList);
        }

        private void SetAnnotationVisibility(bool isShow)
        {
            this.leftTopAnnList.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
            this.rightTopAnnList.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
            this.leftBottomAnnList.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
            this.rightBottomAnnList.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetAnnotationDescriptionVisibility(bool isShow)
        {
            this.leftTopAnnList.ShowDescription = isShow;
            this.rightTopAnnList.ShowDescription = isShow;
            this.leftBottomAnnList.ShowDescription = isShow;
            this.rightBottomAnnList.ShowDescription = isShow;
        }

        private void FillDicomTagsValue(ObservableCollection<DicomTagWithValue> dicomTagWithValues)
        {
            foreach (var tag in dicomTagWithValues)
            {
                ReadDicomTagValue(tag);
            }
        }

        private void ReadDicomTagValue(DicomTagWithValue tag)
        {
            tag.Value = this.DicomTags.FirstOrDefault(x => x.GetHashCode() == tag.GetHashCode())?.Value;
        }

        private void AutoSize()
        {
            SetContainerSize();

            if (this.dicomFile.Rows < this.outer.ActualHeight
                && this.dicomFile.Columns < this.outer.ActualWidth)
            {
                scaleTransform.ScaleX = 1;
                scaleTransform.ScaleY = 1;
                return;
            }

            double scale = 1;

            if (this.dicomFile.Columns > this.dicomFile.Rows)
            {
                //scale = this.canvas.ActualHeight / this.dicomFile.Rows;
                scale = this.outer.ActualHeight / this.dicomFile.Rows;
            }
            else
            {
                //scale = this.canvas.ActualWidth / this.dicomFile.Columns;
                scale = this.outer.ActualHeight / this.dicomFile.Rows;

            }

            this.scaleTransform.CenterX = 0;
            this.scaleTransform.CenterY = 0;

            this.scaleTransform.ScaleX = scale;
            this.scaleTransform.ScaleY = scale;
        }

        private void SetContainerSize()
        {
            this.canvas.Width = this.dicomFile.Columns;
            this.canvas.Height = this.dicomFile.Rows;
        }

        private void AutoPos()
        {
            var translatex = (this.outer.ActualWidth - this.dicomFile.Columns ) / 2;
            var translatey = (this.outer.ActualHeight - this.dicomFile.Rows) / 2;
            var yOffset = this.dicomFile.NumberOfFrames > 1 ? MULTI_FRAME_TRANSLATE_Y_OFFSET : 0;

            this.translateTransform.X = translatex;
            this.translateTransform.Y = translatey + yOffset;
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch(mouseLeftButtonMode)
            {
                case MouseLeftButtonMode.Move:
                    CaptureMouseMove(e);
                    break;
                case MouseLeftButtonMode.SetWL:
                    CaptureMouseSetWL(e);
                    break;
                case MouseLeftButtonMode.Magnifier:
                    CaptureMouseMagnifier(e);
                    break;
                case MouseLeftButtonMode.None:
                    break;
            }
        }

        private void CaptureMouseMagnifier(MouseButtonEventArgs e)
        {
            UpdateMagnifierCirclePos(e.GetPosition(this.canvas));

            this.magnifierCircle.Visibility = Visibility.Visible;
            procType = ProcType.Magnifier;
            this.Cursor = Cursors.Cross;
        }

        private void CaptureMouseMove(MouseButtonEventArgs e)
        {
            procType = ProcType.MoveImage;
            this.Cursor = Cursors.ScrollAll;
            var canvasPoint = e.GetPosition(this.outer);
            startMovePoint = ToImageSpace(canvasPoint);
            this.outer.CaptureMouse();
        }

        private void CaptureMouseSetWL(MouseButtonEventArgs e)
        {
            procType = ProcType.SetWL;
            Uri resourceUri = new Uri("pack://application:,,,/DicomViewCtrl;component/Resources/cur/setwl.cur");
            StreamResourceInfo resourceInfo = Application.GetResourceStream(resourceUri);
            using(System.IO.Stream stream = resourceInfo.Stream)
            {
                this.Cursor = new Cursor(stream);
            }

            this.startWLPoint = e.GetPosition(this.outer);
            this.outer.CaptureMouse();
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
           switch(mouseLeftButtonMode)
            {
                case MouseLeftButtonMode.Move:
                    ReleaseMouseMoveCapture();
                    break;
                case MouseLeftButtonMode.SetWL:
                    ReleaseMouseSetWLCapture();
                    break;
                case MouseLeftButtonMode.Magnifier:
                    ReleaseMouseMagnifierCapture();
                    break;
                case MouseLeftButtonMode.None:
                    break;
            }
        }

        private void ReleaseMouseMagnifierCapture()
        {
            procType = ProcType.None;
            this.Cursor = Cursors.Arrow;
            this.magnifierCircle.Visibility = Visibility.Collapsed;
        }

        private void ReleaseMouseMoveCapture()
        {
            procType = ProcType.None;
            this.Cursor = Cursors.Arrow;
            this.outer.ReleaseMouseCapture();
        }

        private void ReleaseMouseSetWLCapture()
        {
            procType = ProcType.None;
            this.Cursor = Cursors.Arrow;
            this.outer.ReleaseMouseCapture();
            RefreshImageListThumbnail();
        }

        private void RefreshImageListThumbnail()
        {
            this.dicomFile.ManualUpdateThumbnail();

            if (this.dicomFile.FrameIndex == 0)
            {
                this.ImageList[frameImageIndex].Thumbnail = this.dicomFile.ThumbnailImage;
            }
        }

        private Point ToImageSpace(Point canvasPoint)
        {
            return new Point((canvasPoint.X - translateTransform.X) / scaleTransform.ScaleX,(canvasPoint.Y - translateTransform.Y) / scaleTransform.ScaleY);
        }

        private void ReadAllDicomTags()
        {
            if (this.dicomFile == null || this.dicomTags != null)
                return;

            dicomTags = new ObservableCollection<DicomTagWithValue>();

            foreach (var item in this.dicomFile.DataSet)
            {
                dicomTags.Add(new DicomTagWithValue()
                {
                    Group = item.Tag.Group,
                    Element = item.Tag.Element,
                    Description = item.Tag.DictionaryEntry.Name,
                    Value = this.dicomFile.DataSet.GetSingleValueOrDefault<object>(item.Tag, null),
                    ValueRepresentation = item.ValueRepresentation.Code
                });
            }
        }

        public void SetMouseWheelMode(MouseWheelMode wheelMode)
        {
            this.mouseWheelMode = wheelMode;
        }

        public void SetMouseLeftButtonMode(MouseLeftButtonMode mouseLeftButtonMode)
        {
            this.mouseLeftButtonMode = mouseLeftButtonMode;
        }

        private void SwitchFrame(int delta)
        {
            CheckImageOpenStatus();

            if (this.dicomFile.NumberOfFrames <= 1)
                return;

            //TODO
            //Update to
            //ImageChangedEvent
            if (delta < 0)
            {
                SwitchNextFrame();
            }
            else
            {
                SwitchPreviousFrame();
            }            
        }

        private void SwitchNextFrame()
        {
            int nextFrameIndex = 0;

            if (this.FrameIndex != this.ImageList[frameImageIndex].FrameList.Count - 1)
            {
                nextFrameIndex = this.FrameIndex + 1;
            }

            FetchFrame(this.frameImageIndex, nextFrameIndex);
            OnFrameChanged?.Invoke(this, new Viewer.Event.FrameChangedEventArgs(nextFrameIndex, frameImageIndex));
        }

        private void SwitchPreviousFrame()
        {
            int previousFrameIndex = this.dicomFile.NumberOfFrames - 1;
            
            if (this.FrameIndex != 0)
            {
                previousFrameIndex = this.FrameIndex - 1;
            }

            FetchFrame(this.frameImageIndex, previousFrameIndex);
            OnFrameChanged?.Invoke(this, new Viewer.Event.FrameChangedEventArgs(previousFrameIndex, frameImageIndex));
        }

        private void CheckImageOpenStatus()
        {
            if (this.HasImage == false)
                throw new Exception("Not open dicom image yet.");
        }

        private void DisplayCurrentPointInfo(Point current)
        {
            if (this.HasImage == false)
                return;

            var point = GetCurrentPoint(current);
            var value = GetPointValue(point);

            this.lbl_Value.Content = $"X:{point.X} Y:{point.Y} Value:{value}";
        }

        private Point GetCurrentPoint(Point current)
        {
            GeneralTransform transform = this.image.RenderTransform.Inverse;
            if (transform == null)
                return new Point(-1, -1); 

            Point imageCoord = transform.Transform(current);

            // Now scale from displayed image pixels to DICOM image resolution
            int x = (int)Math.Floor(imageCoord.X * this.dicomFile.Columns / this.image.ActualWidth);
            int y = (int)Math.Floor(imageCoord.Y * this.dicomFile.Rows / this.image.ActualHeight);

            if (x < 0)
                x = 0;

            if (x > this.dicomFile.Columns)
                x = this.dicomFile.Columns;

            if (y < 0)
                y = 0;

            if (y > this.dicomFile.Rows)
                y = this.dicomFile.Rows;

            return new Point(x, y);
        }

        private ushort GetPointValue(Point current)
        {
            if (this.dicomFile.BitsStored == 8)
            {
                var index = (int)(current.Y * this.dicomFile.Columns + current.X);
                if (index >= this.dicomFile.ImageData.Length)
                {
                    index = this.dicomFile.ImageData.Length - 1;
                }
                return this.dicomFile.ImageData[index];
            }
            else
            {
                var index = (int)(current.Y * this.dicomFile.Columns + current.X) * 2;
                if (index + 1 >= this.dicomFile.ImageData.Length)
                {
                    index = this.dicomFile.ImageData.Length - 2;
                }
                return (ushort)(this.dicomFile.ImageData[index] | (this.dicomFile.ImageData[index + 1] << 8));
            }
        }

        private void AnnList_AnnotationChanged(object sender, Controls.EventArgs.AnnotationControlEventArgs e)
        {
            ReadDicomTagValue(e.DicomTagWithValue);
        }

        private void UpdateStandAloneInfoPos()
        {
            Canvas.SetLeft(this.stackpanel_SpecialInfo, 0);
            Canvas.SetBottom(this.stackpanel_SpecialInfo, this.outer.ActualHeight / 2 -50);
        }

        private void UpdateScaleInfo()
        {
            this.lbl_Scale.Content = $"Scale:{this.scaleTransform.ScaleX.ToString("0.0")}";
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateStandAloneInfoPos();

            if (this.HasImage)
            {
                AutoPos();
            }
        }

        private void scaleTransform_Changed(object sender, EventArgs e)
        {
            UpdateScaleInfo();
        }

        private void UpdateDicomTagValue(DicomTagWithValue dicomTagWithValue,object value)
        {
            var findTag = this.DicomTags.FirstOrDefault(x => x.GetHashCode() == dicomTagWithValue.GetHashCode());

            if(findTag is not null)
            {
                findTag.Value = value;
            }
            else
            {
                this.DicomTags.Add(new DicomTagWithValue(dicomTagWithValue.Group, 
                    dicomTagWithValue.Element, 
                    dicomTagWithValue.Description, 
                    value));
            }

            //TODO
            //temporarily refresh all
            LoadAnnotation();
        }

        public void Invert()
        {
            CheckImageOpenStatus();
            this.dicomFile.InvertImage();
            this.SetWindow(0, 0);
        }

        private void Magnifier(Point center)
        {
            center = UpdateMagnifierCirclePos(center);

            double length = magnifierCircle.ActualWidth * 0.5; // 放大倍数
            double radius = length / 2;
            var viewboxRect = new Rect(center.X - radius, center.Y - radius, length, length);
            magnifierBrush.Viewbox = viewboxRect;
        }

        private Point UpdateMagnifierCirclePos(Point center)
        {
            //center.X *= this.scaleTransform.ScaleX;
            //center.Y *= this.scaleTransform.ScaleY;

            //center.X += this.translateTransform.X;
            //center.Y += this.translateTransform.Y;

            //magnifierCircle.SetValue(Canvas.LeftProperty, center.X - magnifierCircle.Width / 2);
            //magnifierCircle.SetValue(Canvas.TopProperty, center.Y - magnifierCircle.Height / 2);

            // Convert that point into 'outer' coordinates (after scale/translate/rotate)
            Point posInOuter = canvas.TransformToAncestor(outer).Transform(center);

            // Position the magnifier at this transformed point
            Canvas.SetLeft(magnifierCircle, posInOuter.X - magnifierCircle.Width / 2);
            Canvas.SetTop(magnifierCircle, posInOuter.Y - magnifierCircle.Height / 2);

            //magnifierCircle locate by outer,so need translateTransform offset
            //image magnifier not need to to do this, it depend on itself.
            //center.X -= this.translateTransform.X;
            //center.Y -= this.translateTransform.Y;

            return center;
        }

        public void Fts()
        {
            CheckImageOpenStatus();
            this.AutoFit();
        }

        public void RealSize()
        {
            CheckImageOpenStatus();

            this.scaleTransform.ScaleX = 1;
            this.scaleTransform.ScaleY = 1;

            this.AutoPos();
        }

        public void RotateRight()
        {
            Rotate(90);
        }

        public void RotateLeft()
        {
            Rotate(-90);
        }

        public void FlipVertically()
        {
            this.scaleTransform.ScaleY = -this.scaleTransform.ScaleY;
        }

        public void FlipHorizontally()
        {
            this.scaleTransform.ScaleX = -this.scaleTransform.ScaleX;
        }

        private void Rotate(double angle)
        {
            CheckImageOpenStatus();
            this.rotateTransform.Angle += angle;
        }

        private (double, double) GetRotateCenter()
        {
            double centerX = 0;
            double centerY = 0;

            //TODO
            //temp fixed angle
            if(this.rotateTransform.Angle == 0 || this.rotateTransform.Angle == 180)
            {
                centerX = (this.dicomFile.Columns * this.scaleTransform.ScaleX) / 2;
                centerY = (this.dicomFile.Rows * this.scaleTransform.ScaleY) / 2;
            }
           else
            {
                centerY = (this.dicomFile.Columns * this.scaleTransform.ScaleX) / 2;
                centerX  = (this.dicomFile.Rows * this.scaleTransform.ScaleY) / 2;
            }

            return (centerX, centerY);
        }
    }
}
