using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Win32
{
    public class Shell32
    {
        public const int BIF_NEWDIALOGSTYLE = 0x00000040;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool SHGetPathFromIDList(IntPtr pidl, [Out] StringBuilder pszPath);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        public static string BrowserFolder(string title)
        {
            BROWSEINFO bi = new BROWSEINFO();
            IntPtr buffer = Marshal.AllocHGlobal(260);
            IntPtr pidl = IntPtr.Zero;

            try
            {
                bi.hwndOwner = IntPtr.Zero;
                bi.pidlRoot = IntPtr.Zero;
                bi.pszDisplayName = buffer;
                bi.lpszTitle = title;
                bi.ulFlags = BIF_NEWDIALOGSTYLE;
                bi.lpfn = IntPtr.Zero;
                bi.lParam = IntPtr.Zero;
                bi.iImage = 0;

                pidl = SHBrowseForFolder(ref bi);

                if (pidl != IntPtr.Zero)
                {
                    StringBuilder path = new StringBuilder(260);
                    if (SHGetPathFromIDList(pidl, path))
                    {
                        return path.ToString();
                    }
                }
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
