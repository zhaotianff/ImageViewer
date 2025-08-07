using DicomViewCtrl.Controls.Definitions;
using DicomViewCtrl.Dicom.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DicomViewCtrl.Controls
{
    public class AnnotationControl : Control
    {
        static AnnotationControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnnotationControl), new FrameworkPropertyMetadata(typeof(AnnotationControl)));
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(AnnotationPosition), typeof(AnnotationControl), new PropertyMetadata(OnPositionChanged));
        public static readonly DependencyProperty AnnotationListProperty = DependencyProperty.Register("AnnotationList", typeof(ObservableCollection<DicomTagWithValue>), typeof(AnnotationControl));
        public static readonly DependencyProperty AnnotationFontSizeProperty = DependencyProperty.Register("AnnotationFontSize", typeof(int), typeof(AnnotationControl), new PropertyMetadata(12));
        public static readonly DependencyProperty AnnotationForegroundProperty = DependencyProperty.Register("AnnotationForeground", typeof(SolidColorBrush), typeof(AnnotationControl), new PropertyMetadata(Brushes.White));

        public AnnotationPosition Position
        {
            get
            {
                return (AnnotationPosition)GetValue(PositionProperty);
            }

            set
            {
                SetValue(PositionProperty, value);
            }
        }

        public ObservableCollection<DicomTagWithValue> AnnotationList
        {
            get
            {
                return (ObservableCollection<DicomTagWithValue>)GetValue(AnnotationListProperty);
            }

            set
            {
                SetValue(AnnotationListProperty, value);
            }
        }

        public int AnnotationFontSize
        {
            get
            {
                return (int)GetValue(AnnotationFontSizeProperty);
            }

            set
            {
                SetValue(AnnotationFontSizeProperty, value);
            }
        }

        public SolidColorBrush AnnotationForeground
        {
            get
            {
                return (SolidColorBrush)GetValue(AnnotationForegroundProperty);
            }

            set
            {
                SetValue(AnnotationForegroundProperty, value);
            }
        }

        public static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newPos = (AnnotationPosition)e.NewValue;
            var annotationControl = d as AnnotationControl;

            switch (newPos)
            {
                case AnnotationPosition.LeftTop:
                    Canvas.SetLeft(annotationControl, 0);
                    Canvas.SetTop(annotationControl, 0);
                    break;
                case AnnotationPosition.RightTop:
                    Canvas.SetRight(annotationControl, 0);
                    Canvas.SetTop(annotationControl, 0);
                    break;
                case AnnotationPosition.LeftBottom:
                    Canvas.SetLeft(annotationControl, 0);
                    Canvas.SetBottom(annotationControl, 0);
                    break;
                case AnnotationPosition.RightBottom:
                    Canvas.SetRight(annotationControl, 0);
                    Canvas.SetBottom(annotationControl, 0);
                    break;
            }
        }
    }
}
