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
    /// InputImageSizeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InputImageSizeWindow : Controls.CustomWindow
    {
        public int ImageWidth
        {
            get
            {
                int.TryParse(this.tbox_Width.Text, out int width);
                return width;
            }
        }

        public int ImageHeight
        {
            get
            {
                int.TryParse(this.tbox_Height.Text, out int height);
                return height;
            }
        }

        public int ImageBits
        {
            get
            {
                if(this.cbx_Bits.SelectedIndex == 0)
                    return 8;
                return 16;
            }
        }

        public InputImageSizeWindow()
        {
            InitializeComponent();
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            if(this.ImageWidth == 0 || this.ImageHeight == 0)
            {
                MessageBox.Show("输入格式错误");
                return;
            }

            this.DialogResult = true;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
