using ImageViewer.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Util
{
    public class DialogHelper
    {
        public static string BrowserForFolder(string description = "") => Shell32.BrowserFolder(description);
    }
}
