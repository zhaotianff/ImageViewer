using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Controls.UserControls
{
    public class DropDownButtonData
    {
        public string IconName { get; set; }

        public Action<object> Handler { get; set; }

        public string DisplayName { get; set; }
    }
}
