using ImageViewer.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Infrastructure.Definitions
{
    public enum FpsType
    {
        [DisplayName("15f/s")]
        Fps_15,
        [DisplayName("20f/s")]
        Fps_20,
        [DisplayName("25f/s")]
        Fps_25
    }
}
