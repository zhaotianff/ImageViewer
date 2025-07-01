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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageViewer.Controls.UserControls
{
    /// <summary>
    /// DropDownButton.xaml 的交互逻辑
    /// </summary>
    public partial class DropDownButton : UserControl
    {
        public DropDownButton()
        {
            InitializeComponent();
        }

        private void btn_Drop_Click(object sender, RoutedEventArgs e)
        {
            this.popup.IsOpen = !this.popup.IsOpen;
        }
    }
}
