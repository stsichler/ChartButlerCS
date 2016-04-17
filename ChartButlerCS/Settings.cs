using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.IO;
using System;

namespace ChartButlerCS
{
    public class Settings : INotifyPropertyChanged
    {
        private static Settings defaultInstance = new Settings();

        private Dictionary<string, object> userSettings;
        private string fileName;

        public event PropertyChangedEventHandler PropertyChanged;

        public Settings()
        {
            fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "ChartButlerCS.config");
            Reload();
        }

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

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
            userSettings = new Dictionary<string, object>();
            userSettings.Add("ChartFolder", "");
            userSettings.Add("ServerUsername", "");
            userSettings.Add("ServerPassword", null);
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

            foreach (KeyValuePair<string, object> entry in userSettings)
                OnPropertyChanged(entry.Key);
        }

        public void Save()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("configuration");
            doc.AppendChild(root);
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.InsertBefore(xmlDeclaration, root);

            foreach (KeyValuePair<string, object> entry in userSettings)
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

        public object this[string key]
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


        public string ServerURL
        {
            get
            {
                return "https://www.gat24.de/data.php?rubrik=aktuell&unterrubrik=neues&dokument=neues&SID=0";
            }
        }

        public string ServerAirFieldURL
        {
            get
            {
                return "https://www.gat24.de/data.php?rubrik=briefing&unterrubrik=flugplaetze&dokument=karten&ICAO=";
            }
        }

        public string ServerAmendedURL
        {
            get
            {
                return "https://www.gat24.de/data.php?rubrik=aktuell&unterrubrik=neues&dokument=aip_aktuell&SID=&printable=true";
            }
        }

        public int UpdateInterval
        {
            get
            {
                return 28;
            }
        }

        public string ServerChartURL
        {
            get
            {
                return "https://www.gat24.de/dokumente/briefing/flugplaetze/pdfkarten.php?&icao=";
            }
        }

        public string ServerChartPreviewURL
        {
            get
            {
                return "https://www.gat24.de/image.php?bild=dokumente/briefing/flugplaetze/karten/";
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

        public string ServerUsername
        {
            get
            {
                return ((string)(this["ServerUsername"]));
            }
            set
            {
                this["ServerUsername"] = value;
            }
        }

        public string ServerPassword
        {
            get
            {
                return ((string)(this["ServerPassword"]));
            }
            set
            {
                this["ServerPassword"] = value;
            }
        }

    }
}
