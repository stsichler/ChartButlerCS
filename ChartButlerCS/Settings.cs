using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.IO;
using System;

namespace ChartButlerCS
{
    public class Settings : INotifyPropertyChanged, ICloneable
    {
        public static Settings Default = new Settings();

        private Dictionary<string, string> userSettings;

        private string fileName = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "ChartButlerCS.config");

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void Init()
        {
            userSettings = new Dictionary<string, string>();
            userSettings.Add("ChartFolder", "");
            userSettings.Add("EulaRead", "");
        }

        public object Clone()
        {
            Settings new_settings = new Settings();
            new_settings.userSettings = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> entry in userSettings)
            {
                new_settings.userSettings.Add(entry.Key, (string)entry.Value?.Clone());
            }

            return new_settings;
        }

        public void Reload()
        {
            Init();

            try
            {
                if (File.Exists(fileName))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);

                    XmlNode root = doc.DocumentElement;

                    foreach (XmlNode node_setting in root.ChildNodes)
                    {
                        string key = node_setting.Attributes["name"]?.InnerText;
                        if (userSettings.ContainsKey(key))
                        {
                            if (userSettings[key] == null || userSettings[key] is string)
                                userSettings[key] = node_setting.ChildNodes[0].InnerText;
                        }
                    }
                }
            }
            catch(Exception)
            {
                Init();
            }

            foreach (KeyValuePair<string, string> entry in userSettings)
                OnPropertyChanged(entry.Key);
        }

        public void Save()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("configuration");
            doc.AppendChild(root);
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.InsertBefore(xmlDeclaration, root);

            foreach (KeyValuePair<string, string> entry in userSettings)
            {
                if (entry.Value == null)
                    continue;
                XmlNode node_setting = doc.CreateElement("setting");
                XmlAttribute attribute_name= doc.CreateAttribute("name");
                attribute_name.InnerText = entry.Key;
                node_setting.Attributes.Append(attribute_name);
                XmlNode node_value = doc.CreateElement("value");
                node_value.InnerText = entry.Value.ToString();
                node_setting.AppendChild(node_value);
                root.AppendChild(node_setting);
            }

            doc.Save(fileName);
        }

        public string this[string key]
        {
            get
            {
                return userSettings[key];
            }
            set
            {
                userSettings[key] = value;
                OnPropertyChanged(key);
            }
        }

        // Application Constants ---------------------------------------------------------------

        public int AiracUpdateInterval
        {
            get
            {
                return 28;
            }
        }

        public string ChartButlerURL
        {
            get
            {
                return "https://stsichler.github.io/ChartButlerCS/";
            }
        }

        // User Settings -----------------------------------------------------------------------


        public string ChartFolder
        {
            get
            {
                return ((string)(this["ChartFolder"]));
            }
            set
            {
                this["ChartFolder"] = value;
            }
        }

        public string EulaRead
        {
            get
            {
                return ((string)(this["EulaRead"]));
            }
            set
            {
                this["EulaRead"] = value;
            }
        }
    }
}
