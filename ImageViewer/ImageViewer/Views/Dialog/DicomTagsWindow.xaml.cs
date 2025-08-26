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
    /// DicomTagsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DicomTagsWindow : Controls.CustomWindow
    {
        private ObservableCollection<DicomViewCtrl.Dicom.Data.DicomTagWithValue> dicomTags = new ObservableCollection<DicomViewCtrl.Dicom.Data.DicomTagWithValue>();

        public DicomTagsWindow(ObservableCollection<DicomViewCtrl.Dicom.Data.DicomTagWithValue> dicomTags)
        {
            InitializeComponent();

            this.tbox_Keyword.Focus();
            this.dicomTags = dicomTags;
            this.list_DicomTags.ItemsSource = this.dicomTags;
        }

        private void AutoSizeColumns(double width)
        {
            var gridView = list_DicomTags.View as GridView;

            gridView.Columns[0].Width = 65;
            gridView.Columns[1].Width = 65;
            gridView.Columns[2].Width = 35;
            gridView.Columns[3].Width = (width - 130) * 0.3;
            gridView.Columns[4].Width = (width -130) * 0.7;
        }

        private void CustomWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AutoSizeColumns(e.NewSize.Width);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.dicomTags.Count == 0)
                return;

            var keyword = this.tbox_Keyword.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                this.list_DicomTags.ItemsSource = this.dicomTags;
            }
            else
            {
                this.list_DicomTags.ItemsSource = this.dicomTags.
                    Where(x=>x.Description.ToLower().Contains(keyword.ToLower()));
            }
        }
    }
}
