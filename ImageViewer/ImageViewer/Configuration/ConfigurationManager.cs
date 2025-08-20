using ImageViewer.Configuration.Annotation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace ImageViewer.Configuration
{
    public class ConfigurationManager
    {
        private static volatile ConfigurationManager configurationManager;
        private static readonly object obj = new object();
        private static readonly string configDir = Environment.ExpandEnvironmentVariables("%ProgramData%\\FF.ImageViewer");
        private static readonly string configFilePath = "\\config.xml";

        public static ConfigurationManager Instance
        {
            get
            {

                if (configurationManager == null)
                {
                    lock (obj)
                    {
                        if (configurationManager == null)
                        {
                            configurationManager = new ConfigurationManager();
                        }
                    }
                }


                return configurationManager;
            }
        }

        public AnnotationConfiguration AnnotationConfiguration { get; set; }


        public ConfigurationManager()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            if(System.IO.File.Exists(configFilePath))
            {
                LoadConfigurationFromFile(configFilePath);
            }
            else
            {
                LoadDefaultConfiguration();
            }
        }

        private void LoadConfigurationFromFile(string fileName)
        {
            try
            {
                XDocument doc = XDocument.Load(fileName);

                foreach (XElement moduleElement in doc.Root.Elements())
                {
                    var moduleName = moduleElement.Attribute("ModuleName").Value;
                    var propertyInfo = typeof(ConfigurationManager).GetProperty(moduleName);
                    var propertyType = propertyInfo.PropertyType;
                    propertyInfo.SetValue(this, Activator.CreateInstance(propertyType, new object[] { moduleElement }));
                }
            }
            catch
            {
                LoadDefaultConfiguration();
            }
        }

        private void LoadDefaultConfiguration()
        {
            var localProperties = typeof(ConfigurationManager).GetProperties(System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public);
            foreach (var property in localProperties)
            {
                var propertyType = property.PropertyType;
                property.SetValue(this, Activator.CreateInstance(propertyType));
            }
        }

        public void WriteConfiguration()
        {
            XDocument doc = new XDocument(new XElement("Configuration"));

            var localProperties = typeof(ConfigurationManager).GetProperties(System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public);

            foreach (var property in localProperties)
            {
                IConfiguration configuration = property.GetValue(this) as IConfiguration;
                doc.Root.Add(configuration.GetConfigurationXml());
            }

            if(System.IO.Directory.Exists(configDir) == false)
            {
                System.IO.Directory.CreateDirectory(configDir);
            }

            XmlWriterSettings xws = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8, Indent = true };
            using (XmlWriter xw = XmlWriter.Create(configDir + configFilePath, xws))
            {
                doc.Save(xw);
            }
        }
    }
}
