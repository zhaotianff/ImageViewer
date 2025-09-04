using DicomViewCtrl.Dicom.Data;
using DicomViewCtrl.Viewer.Data;
using FellowOakDicom.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomViewCtrl.Viewer.Interface
{
    public interface IImageViewer
    {
        bool HasImage { get;  }

        int FrameIndex { get; }

        bool OpenImage(string dicomFilePath);

        bool CloseImage();

        bool PrefetchImage(string dicomFilePath);

        bool FetchFrame(int imageIndex, int frameIndex);

        Task<bool> OpenImageAsync(string dicomFilePath);

        bool OpenRaw(string rawFilePath, int width, int height, int bits);

        void PutImage(byte[] imageData, int width, int height, int bits);

        void SetWindow(double ww, double wl);

        void SetAnnotationTags(ObservableCollection<DicomTagWithValue> leftTop,
            ObservableCollection<DicomTagWithValue> rightTop,
            ObservableCollection<DicomTagWithValue> leftBottom,
            ObservableCollection<DicomTagWithValue> rightBottom);

        void SetAnnotationDescriptionVisibility(bool isShow);

        void SetMouseWheelMode(MouseWheelMode wheelMode);

        void SetMouseLeftButtonMode(MouseLeftButtonMode mouseLeftButtonMode);

        void Invert();

        void Fts();

        void RealSize();
    }
}
