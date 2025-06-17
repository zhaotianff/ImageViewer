using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;

namespace ImageViewer.Util
{
    internal class WindowHelper
    {
        public const uint WM_SYSTEMMENU = 0xa4;
        public const uint WP_SYSTEMMENU = 0x02;
        public const uint WM_NCRBUTTONUP = 0x00A5;

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern long GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern long SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

        private static readonly IntPtr HWND_BOTTOM = (IntPtr)1;
        private static readonly IntPtr HWND_TOP = (IntPtr)0;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;

        private const long WS_EX_TOOLWINDOW = 0x00000080L;
        private const int GWL_EXSTYLE = -20;

  
        public static void SetTopWindow(IntPtr hwndTop, IntPtr hwndBottom)
        {
            if (hwndBottom != IntPtr.Zero)
                SetWindowPos(hwndBottom, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            SetWindowPos(hwndTop, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        public static void HideWindowInSwitcher(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            int exStyle = (int)GetWindowLong(hwnd, GWL_EXSTYLE);

            exStyle |= (int)WS_EX_TOOLWINDOW;
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }
    }
}
