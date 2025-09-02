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
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using ImageViewer.Util;
using ImageViewer.Views.Dialog;
using ImageViewer.Infrastructure.Definitions;
using ImageViewer.Infrastructure.Attributes;
using ImageViewer.Controls;
using ImageViewer.Controls.UserControls;
using ImageViewer.Configuration;

namespace ImageViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Controls.CustomWindow
    {
        private ConfigurationManager configurationManager;

        private bool isPlay = false;
        private FpsType fpsType = FpsType.Fps_20;

        public MainWindow()
        {
            InitializeComponent();

            InitializeUI();
        }

        private void InitializeUI()
        {
            this.list_ImageList.ItemsSource = imgview.ImageList;

            InitializeConfig();
            InitializeToolBar();
        }

        private void InitializeConfig()
        {
            configurationManager = ConfigurationManager.Instance;

            var annotationConfig = configurationManager.AnnotationConfiguration;
            this.imgview.SetAnnotationTags(annotationConfig.LeftTop, 
                annotationConfig.RightTop, 
                annotationConfig.LeftBottom, 
                annotationConfig.RightBottom);
            this.imgview.SetAnnotationDescriptionVisibility(ConfigurationManager.Instance.
                AnnotationConfiguration.IsShowDescription);
        }

        private void InitializeToolBar()
        {
            List<DropDownButtonData> mouseWheelButtonList = new List<DropDownButtonData>()
            {
                new DropDownButtonData(){DisplayName = "切换帧",Handler = x=>{this.imgview.SetMouseWheelMode(DicomViewCtrl.Viewer.Data.MouseWheelMode.SwitchFrame);  },IconName = "IconMouseWheel" },
                new DropDownButtonData(){DisplayName = "缩放",Handler = x=>{this.imgview.SetMouseWheelMode(DicomViewCtrl.Viewer.Data.MouseWheelMode.Zoom);  },IconName = "IconZoom" }
            };
            this.btn_MouseWheel.DropdownButtons = mouseWheelButtonList;
            this.btn_MouseWheel.SelectDropDownListItem(0);

            List<DropDownButtonData> mouseLeftButtonList = new List<DropDownButtonData>()
            {
                 new DropDownButtonData(){DisplayName = "移动",Handler = x=>{this.imgview.SetMouseLeftButtonMode(DicomViewCtrl.Viewer.Data.MouseLeftButtonMode.Move);  },IconName = "IconHand" },
                new DropDownButtonData(){DisplayName = "窗宽窗位",Handler = x=>{this.imgview.SetMouseLeftButtonMode(DicomViewCtrl.Viewer.Data.MouseLeftButtonMode.SetWL);  },IconName = "IconWL" }
            };
            this.btn_MouseLeftButton.DropdownButtons = mouseLeftButtonList;
            this.btn_MouseLeftButton.SelectDropDownListItem(0);
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Dicom Files|*.dcm";
            
            if (openFileDialog.ShowDialog() == true)
            {
                OpenDicomFile(openFileDialog.FileName);
            }
        }

        private void OpenDicomFile(string fileName)
        {
            imgview.OpenImage(fileName);
            SelectLastItem();
        }

        private void SelectLastItem()
        {
            this.list_ImageList.SelectionChanged -= this.list_ImageList_SelectionChanged;
            this.list_ImageList.SelectedIndex = this.imgview.ImageList.Count -1;
            this.list_ImageList.SelectionChanged += this.list_ImageList_SelectionChanged;

            ShowPlayerUI(isAutoHidden: true);
            SelectFirstFrame();
            btn_Pause_Click(null, null);
        }

        private void SelectFirstFrame()
        {
            this.list_FrameList.SelectionChanged -= this.list_FrameList_SelectionChanged;
            this.list_FrameList.SelectedIndex = 0;
            this.list_FrameList.SelectionChanged += this.list_FrameList_SelectionChanged;

            if(this.list_FrameList.Items.Count > 0)
            {
                this.list_FrameList.ScrollIntoView(this.list_FrameList.Items[0]);
            }
        }

        private async void ShowPlayerUI(bool isShow = true, bool isAutoHidden = false)
        {
            if (this.imgview.ImageList == null || this.imgview.ImageList.Count == 0)
                return;

            var frameList = this.imgview.ImageList[this.list_ImageList.SelectedIndex].FrameList;
            if (frameList != null && frameList.Count > 0 && isShow == true)
            {
                SetFrameListPlayerUIVisibility(Visibility.Visible);

                if (isAutoHidden)
                {
                    await Task.Delay(3000);
                    if (TreeHelper.IsMouseOverControl(this.grid_FrameList) == false)
                    {
                        SetFrameListPlayerUIVisibility(Visibility.Hidden);
                    }
                }
            }
            else
            {
                SetFrameListPlayerUIVisibility(Visibility.Hidden);
            }
        }

        private void SetFrameListPlayerUIVisibility(Visibility visibility)
        {
            foreach (UIElement item in grid_FrameList.Children)
            {
                item.Visibility = visibility;
            }
        }

        private void frameListGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowPlayerUI();
        }

        private void frameListGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            ShowPlayerUI(false);
        }

        private void OpenRaw_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Raw Files|*.raw;*.RAW|All files|*.*";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                OpenRawFile(openFileDialog.FileName);
            }
        }

        private void OpenRawFile(string fileName)
        {
            InputImageSizeWindow inputImageSizeWindow = new InputImageSizeWindow();
            if (inputImageSizeWindow.ShowDialog() == true)
            {
                imgview.OpenRaw(fileName, inputImageSizeWindow.ImageWidth, inputImageSizeWindow.ImageHeight, inputImageSizeWindow.ImageBits);
                SelectLastItem();
            }
        }

        private async void OpenDicomDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dir = DialogHelper.BrowserForFolder("打开Dicom文件夹");
            await OpenDicomFolderAsync(dir);
        }

        private async Task OpenDicomFolderAsync(string dir)
        {
            if (string.IsNullOrEmpty(dir) || DirectoryHelper.Exist(dir) == false)
                return;

            var files = DirectoryHelper.GetFiles(dir, "*.dcm");

            if (files == null || files.Length == 0)
                return;

            OpenDicomFile(files[0]);

            for (int i = 1; i < files.Length; i++)
            {
                await Task.Run(() =>
                {
                    this.imgview.PrefetchImage(files[i]);
                });
            }
        }

        private void list_ImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.list_ImageList.SelectedIndex == -1)
                return;

            var selectedImage = this.imgview.ImageList[this.list_ImageList.SelectedIndex];

            if(selectedImage.IsRaw == true)
            {
                this.imgview.OpenRaw(selectedImage.FilePath, selectedImage.Width, selectedImage.Height, selectedImage.Bits);
            }
            else
            {
                this.imgview.OpenImage(selectedImage.FilePath);
            }

            ShowPlayerUI(isAutoHidden: true);
            SelectFirstFrame();
            btn_Pause_Click(null, null);
        }

        private void list_FrameList_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                var scrollViewer = TreeHelper.FindVisualChild<ScrollViewer>(listBox);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                    e.Handled = true;
                }
            }
        }

        private void list_FrameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.list_FrameList.SelectedIndex == -1)
                return;

            this.imgview.FetchFrame(this.list_ImageList.SelectedIndex, this.list_FrameList.SelectedIndex);
        }

        private void Play()
        {
            isPlay = true;

            Task.Run(async () => {
                while (isPlay)
                {
                    await Task.Delay(fpsType.GetFrameWaitMillionSeconds());
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SwitchNextFrame();
                    });
                }
            });
            
        }

        private void Pause()
        {
            isPlay = false;
        }

        private void SwitchNextFrame()
        {
            if (this.list_FrameList.SelectedIndex == this.imgview.ImageList[this.list_ImageList.SelectedIndex].FrameList.Count - 1)
                this.list_FrameList.SelectedIndex = 0;
            else
            {
                this.list_FrameList.SelectedIndex++;
                this.list_FrameList.ScrollIntoView(this.list_FrameList.SelectedItem);
            }

        }

        private void SwitchPreviousFrame()
        {
            if (this.list_FrameList.SelectedIndex == 0)
                this.list_FrameList.SelectedIndex = this.imgview.ImageList[this.list_ImageList.SelectedIndex].FrameList.Count - 1;
            else
            {
                this.list_FrameList.SelectedIndex--;
                this.list_FrameList.ScrollIntoView(this.list_FrameList.SelectedItem);
            }
        }

        private void btn_Previous_Click(object sender, RoutedEventArgs e)
        {
            SwitchPreviousFrame();
        }

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            SwitchNextFrame();
        }

        private void btn_Fps_Click(object sender, RoutedEventArgs e)
        {
            var fps = (int)fpsType;
            fps++;
            fps = fps % Enum.GetNames(typeof(FpsType)).Length;
            fpsType = (FpsType)fps;
            btn_Fps.Content = fpsType.GetDisplayName();
        }

        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            this.btn_Pause.Visibility = Visibility.Visible;
            this.btn_Play.Visibility = Visibility.Collapsed;

            Play();
        }

        private void btn_Pause_Click(object sender, RoutedEventArgs e)
        {
            this.btn_Pause.Visibility = Visibility.Collapsed;
            this.btn_Play.Visibility = Visibility.Visible;

            Pause();
        }

        private void CustomWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Pause();

            configurationManager.WriteConfiguration();
        }

        private void btn_DicomTags_Click(object sender, RoutedEventArgs e)
        {
            DicomTagsWindow dicomTagsWindow = new DicomTagsWindow(this.imgview.DicomTags);
            dicomTagsWindow.ShowDialog();
        }

        private void imgview_OnFrameChanged(object sender, DicomViewCtrl.Viewer.Event.FrameChangedEventArgs e)
        {
            this.list_FrameList.SelectionChanged -= this.list_FrameList_SelectionChanged;
            this.list_FrameList.SelectedIndex = e.FrameIndex;
            this.list_FrameList.ScrollIntoView(this.list_FrameList.SelectedItem);
            this.list_FrameList.SelectionChanged += this.list_FrameList_SelectionChanged;
        }

        private void btn_AutoWindow_Click(object sender, RoutedEventArgs e)
        {
            this.imgview.SetWindow(0, 0);
        }

        private void btn_Annotation_Click(object sender, RoutedEventArgs e)
        {
            DicomAnnotationWindow dicomAnnotationWindow = new DicomAnnotationWindow();
            dicomAnnotationWindow.ShowDialog();
            RefreshDicomAnnotationConfig();
        }

        private void RefreshDicomAnnotationConfig()
        {
            this.imgview.SetAnnotationDescriptionVisibility(ConfigurationManager.Instance.
                AnnotationConfiguration.IsShowDescription);
        }
    }
}
