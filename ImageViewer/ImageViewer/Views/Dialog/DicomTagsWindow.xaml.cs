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
using System.Windows.Shapes;

namespace ImageViewer.Views.Dialog
{
    /// <summary>
    /// DicomTagsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DicomTagsWindow : Controls.CustomWindow
    {
        public DicomTagsWindow()
        {
            InitializeComponent();

           
            this.list_DicomTags.ItemsSource = new List<string>() { "sdfdsfdsf"};
        }

        private void AutoSizeColumns()
        {
            var width = this.Width;
            var gridView = list_DicomTags.View as GridView;

            gridView.Columns[0].Width = width * 0.15;
            gridView.Columns[1].Width = width * 0.15;
            gridView.Columns[2].Width = width * 0.3;
            gridView.Columns[3].Width = width * 0.3;
        }

        private void CustomWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AutoSizeColumns();
        }
    }
}
