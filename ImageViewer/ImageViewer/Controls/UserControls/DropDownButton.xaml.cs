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
        private int selectedIndex = -1;

        public int SelectedIndex => selectedIndex;

        private List<DropDownButtonData> dropdownButtons = new List<DropDownButtonData>();

        public List<DropDownButtonData> DropdownButtons
        {
            get => dropdownButtons;
            set
            {
                dropdownButtons = value;
                UpdateDrowDownList();
            }
        }

        private void UpdateDrowDownList()
        {
            menu.Items.Clear();

            for (int i = 0; i < this.DropdownButtons.Count; i++)
            {
                var dropDownButtonData = this.DropdownButtons[i];
                MenuItem menuItem = new MenuItem();
                menuItem.Width = 80;
                menuItem.Header = dropDownButtonData.DisplayName;
                var index = i;
                menuItem.Click += (sender, args) => 
                {
                    SelectDropDownListItem(index);
                    dropDownButtonData.Handler?.Invoke(null);
                };
                this.menu.Items.Add(menuItem);
            }
        }

        public void SelectDropDownListItem(int index)
        {
            SelectDropDownListItemWithoutEvent(index);
            dropdownButtons[index].Handler?.Invoke(this);
        }

        public void SelectDropDownListItemWithoutEvent(int index)
        {
            selectedIndex = index;
            var dropDownButtonData = this.DropdownButtons[index];
            this.btn_DropDown.Content = Application.Current.FindResource(dropDownButtonData.IconName);
            this.popup.IsOpen = false;
        }

        public DropDownButton()
        {
            InitializeComponent();
        }

        private void btn_Drop_Click(object sender, RoutedEventArgs e)
        {
            this.popup.IsOpen = !this.popup.IsOpen;
        }

        private void btn_DropDown_Click(object sender, RoutedEventArgs e)
        {
            if(selectedIndex < this.dropdownButtons.Count)
            {
                this.dropdownButtons[selectedIndex].Handler?.Invoke(this);
            }
        }
    }
}
