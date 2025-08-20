using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ImageViewer.Configuration
{
    public interface IConfiguration
    {
        void SetDefaultConfiguration();

        XElement GetConfigurationXml();

        string ModuleName { get; }
    }
}
