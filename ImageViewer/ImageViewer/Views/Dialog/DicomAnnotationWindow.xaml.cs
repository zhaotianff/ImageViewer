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
using System.Windows.Shapes;

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

            this.list_common.ItemsSource = DicomTags.GetCommon();
            this.list_all.ItemsSource = DicomTags.GetAll();

            LoadConfig();
            LoadDemoAnnotationList();
        }

        private void LoadConfig()
        {
            this.cbx_ShowDescrption.Checked -= cbx_ShowDescription_Checked;
            this.cbx_ShowDescrption.IsChecked = true;
            this.cbx_ShowDescrption.Checked += cbx_ShowDescription_Checked;
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
    }
}
