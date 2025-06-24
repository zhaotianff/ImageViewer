using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageViewer.Util
{
    public class TreeHelper
    {
        public static T FindVisualChild<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T t)
                {
                    return t;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        public static bool IsMouseOverControl(FrameworkElement control)
        {
            Point mousePos = Mouse.GetPosition(control);
            return new Rect(new Size(control.ActualWidth, control.ActualHeight)).Contains(mousePos);
        }
    }
}
