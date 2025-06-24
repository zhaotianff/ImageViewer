using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Util
{
    public class DirectoryHelper
    {
        public static string[] GetFiles(string dir,string extension = "*.*")
        {
            return Directory.GetFiles(dir, extension, SearchOption.AllDirectories);
        }
    }
}
