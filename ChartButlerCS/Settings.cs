using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.IO;
using System.Text;
using System;
using System.Security.Cryptography;

namespace ChartButlerCS
{
    public class Settings : INotifyPropertyChanged, ICloneable
    {
        public static Settings Default = new Settings();

        private static byte[] s_aditionalEntropy = { 23, 42, 1, 6, 5 };

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
            userSettings.Add("ServerUsername", "");
            userSettings.Add("ServerPassword", null);
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
            new_settings.m_DataSource = (string)m_DataSource?.Clone();

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

        public string GAT24_ServerURL
        {
            get
            {
                return "https://www.gat24.de/data.php?rubrik=aktuell&unterrubrik=neues&dokument=neues&SID=0";
            }
        }

        public string GAT24_ServerAirFieldURL
        {
            get
            {
                return "https://www.gat24.de/data.php?rubrik=briefing&unterrubrik=flugplaetze&dokument=karten&ICAO=";
            }
        }

        public string GAT24_ServerAmendedURL
        {
            get
            {
                return "https://www.gat24.de/data.php?rubrik=aktuell&unterrubrik=neues&dokument=aip_aktuell&SID=&printable=true";
            }
        }

        public string GAT24_ServerChartURL
        {
            get
            {
                return "https://www.gat24.de/dokumente/briefing/flugplaetze/pdfkarten.php?&icao=";
            }
        }

        public string GAT24_ServerChartPreviewURL
        {
            get
            {
                return "https://www.gat24.de/image.php?bild=dokumente/briefing/flugplaetze/karten/";
            }
        }

        public string GAT24_ServerTripKitURL
        {
            get
            {
                return "https://www.gat24.de/dokumente/briefing/flugplaetze/pdftripkit.php?icao=";
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
                try
                {
                    if (this["ServerPassword"] != null)
                        return UTF8Encoding.UTF8.GetString(
                            ProtectedData.Unprotect(
                                Convert.FromBase64String((string)this["ServerPassword"]),
                                s_aditionalEntropy, DataProtectionScope.CurrentUser));
                    else
                        return null;
                }
                catch (Exception) { }
                return null;
            }
            set
            {
                if (value != null)
                    this["ServerPassword"] = Convert.ToBase64String(
                        ProtectedData.Protect(
                            UTF8Encoding.UTF8.GetBytes(value),
                            s_aditionalEntropy,DataProtectionScope.CurrentUser));
                else
                    this["ServerPassword"] = null;
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

        // note: this is NOT stored in the config file
        
        public string DataSource
        {
            get
            {
                return m_DataSource;
            }
            set
            {
                m_DataSource = value;
                OnPropertyChanged("DataSource");
            }
        }
        private string m_DataSource;

    }
}
