using DicomViewCtrl.Dicom.Data;
using ImageViewer.Configuration;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ImageViewer.Views.Dialog
{
    /// <summary>
    /// DicomAnnotationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DicomAnnotationWindow : Controls.CustomWindow
    {
        public DicomAnnotationWindow()
        {
            InitializeComponent();

            this.list_common.ItemsSource = new ObservableCollection<DicomTagWithValue>(DicomTags.GetCommon());
            this.list_all.ItemsSource = new ObservableCollection<DicomTagWithValue>(DicomTags.GetAll());

            LoadConfig();
        }

        private void LoadConfig()
        {
            this.cbx_ShowDescrption.Checked -= cbx_ShowDescription_Checked;
            this.cbx_ShowDescrption.IsChecked = true;
            this.cbx_ShowDescrption.Checked += cbx_ShowDescription_Checked;

            this.leftTopAnnList.AnnotationList = ConfigurationManager.Instance.AnnotationConfiguration.LeftTop;
            this.rightTopAnnList.AnnotationList = ConfigurationManager.Instance.AnnotationConfiguration.RightTop;
            this.leftBottomAnnList.AnnotationList = ConfigurationManager.Instance.AnnotationConfiguration.LeftBottom;
            this.rightBottomAnnList.AnnotationList = ConfigurationManager.Instance.AnnotationConfiguration.RightBottom;
        }

        private void LoadDemoAnnotationList()
        {
            int index = 0;
            this.leftTopAnnList.AnnotationList = new ObservableCollection<DicomTagWithValue>(DicomTags.GetCommon().
                Take(5).
                Select(x =>
                {
                    x.Value = "Value" + (++index).ToString(); return x;
                }));
            this.rightTopAnnList.AnnotationList = new ObservableCollection<DicomTagWithValue>(DicomTags.GetCommon().Skip(5).
                Take(5).
                Select(x =>
                {
                    x.Value = "Value" + (++index).ToString(); return x;
                }));
            this.leftBottomAnnList.AnnotationList = new ObservableCollection<DicomTagWithValue>(DicomTags.GetCommon().Skip(10).
                Take(5).
                Select(x =>
                {
                    x.Value = "Value" + (++index).ToString(); return x;
                }));
            this.rightBottomAnnList.AnnotationList = new ObservableCollection<DicomTagWithValue>(DicomTags.GetAll().Skip(15).
                Take(5).
                Select(x =>
                {
                    x.Value = "Value" + (++index).ToString(); return x;
                }));
        }

        private void SetAnnotationListVisibility(bool isShow)
        {
            this.leftTopAnnList.ShowDescription = isShow;
            this.rightTopAnnList.ShowDescription = isShow;
            this.leftBottomAnnList.ShowDescription = isShow;
            this.rightBottomAnnList.ShowDescription = isShow;
        }

        private void cbx_ShowDescription_Checked(object sender, RoutedEventArgs e)
        {
            SetAnnotationListVisibility(true);
        }

        private void cbx_ShowDescription_Unchecked(object sender, RoutedEventArgs e)
        {
            SetAnnotationListVisibility(false);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.tab_Tags.SelectedIndex == 0)
            {
                SyncCommonTagList();
            }
            else
            {
                SyncAllTagList();
            }
        }

        private void SyncCommonTagList()
        {
            var commonTagsList = DicomTags.GetCommon()
                .Except(this.leftTopAnnList.AnnotationList)
                .Except(this.rightTopAnnList.AnnotationList)
                .Except(this.leftBottomAnnList.AnnotationList)
                .Except(this.rightBottomAnnList.AnnotationList);
            this.list_common.ItemsSource = new ObservableCollection<DicomTagWithValue>(commonTagsList);
            this.list_all.ItemsSource = new ObservableCollection<DicomTagWithValue>(DicomTags.GetAll());
        }

        private void SyncAllTagList()
        {
            this.list_common.ItemsSource = new ObservableCollection<DicomTagWithValue>(DicomTags.GetCommon());
            var allTagsList = DicomTags.GetAll()
                .Except(this.leftTopAnnList.AnnotationList)
                .Except(this.rightTopAnnList.AnnotationList)
                .Except(this.leftBottomAnnList.AnnotationList)
                .Except(this.rightBottomAnnList.AnnotationList);
            this.list_all.ItemsSource = new ObservableCollection<DicomTagWithValue>(allTagsList);
        }

        private void list_common_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
