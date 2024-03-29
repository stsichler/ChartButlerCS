﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Drawing;
using System.Drawing.Imaging;

namespace ChartButlerCS
{
    /// <summary>
    /// Diese Klasse stellt Verbindung mit dem gat24-Server her und ruft relevante Daten ab.
    /// </summary>
    public partial class CServerConnection
    {   
        private string DFS_airac;
        private Uri DFS_MainURL;
        
        [Serializable]
        public struct DFSAirfieldsCache
        {
            /// <summary>
            /// Die Main URL (aufgelöste URL des Permalinks zur BasicVFR Hauptseite), aus der der Cache extrahiert wurde.
            /// siehe DFS_MainURL
            /// </summary>
            public string mainURL;
            /// <summary>
            /// Liste der Flugplätze und URLs ihrer Übersichtsseiten
            /// </summary>
            public SortedDictionary<string, string> /* Platzname, Relative URL (href) */ airfields;
        }

        public DFSAirfieldsCache DFS_airfields_cache;

        private string DFS_airfields_cache_fileName = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData), "ChartButlerCS.DFS.AFcache");

        public struct DFS_ChartLink
        {
            public string airfield_permalink;
            public Uri document_link;
            public string name;
            public byte[] preview_data;
        }

        /// <summary>
        /// Verbindet mit dem Server und sucht nach geänderten Karten.
        /// </summary>
        private void DFS_ConnectionWorker(string[] newFields, IntPtr dummy)
        {
            sts.Invoke((MethodInvoker)delegate {
                sts.progressBar.Maximum = (newFields != null && newFields.Length != 0) ? (newFields.Length * 6) : (2 + (chartButlerDataset.Airfields.Count * 4));
                sts.txtProgress.AppendText("Verbinde zu DFS Server... "); });

            try
            {
                DFS_MainURL = new Uri(new Uri(Properties.Resources.DFS_PermalinkBaseURL), Properties.Resources.DFS_PermalinkMain);
                string text = DFS_GetRedirectURLText2(ref DFS_MainURL);

                sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });

                int pos = 0;
                DFS_airac = Utility.GetTextBetweenRegex(text, new Regex(@"<span\s+class=""expand-header"">Effective:\s+</span>"), new Regex(@"[\r\n\s]*</div>"), ref pos);
                if (!string.IsNullOrEmpty(DFS_airac))
                {
                    Update = DFS_CreateDateFromString(DFS_airac);
                }
                if (string.IsNullOrEmpty(DFS_airac) || Update == new DateTime())
                {
                    errorText = "Entschuldigung. " + Environment.NewLine
                        + "Der AIRAC Date Code kann nicht abgerufen werden." + Environment.NewLine
                        + "Es handelt sich um eine Inkompatibilität mit dem DFS Server.";
                    return;
                }
            }
            catch (Exception exc) 
            {
                string assemblyVersion = typeof(HttpWebRequest).Assembly.GetName().Version.ToString();
                // try to find of exact file version of System Assembly
                object[] ver = typeof(HttpWebRequest).Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
                if (ver.Length > 0)
                    assemblyVersion = ((AssemblyFileVersionAttribute)ver[0]).Version;

                errorText = "Entschuldigung. " + Environment.NewLine + "Die Verbindung zum DFS Server " + Environment.NewLine + "konnte nicht hergestellt werden."
                    + Environment.NewLine + Environment.NewLine + "Technische Details:"+ Environment.NewLine 
                    + exc.Message+ Environment.NewLine + "Framework Version: " + assemblyVersion;
                return;
            }

            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("OK. Effective: " + DFS_airac + Environment.NewLine); });

            if (newFields != null)
            {
                foreach(var field in newFields)
                    DFS_AddNewField(field);
                System.Threading.Thread.Sleep(3000);
            }
            else
            {
                DFS_CheckForNewCharts();
            }
        }

        /// <summary>
        /// Erzeugt ein DateTime von einem Date String der Form "TT MMM JJJJ", z.B. "02 FEB 2023".
        /// Falls der dateString nicht als Datum erkannt wird, wird eine leere DateTime Instanz zurück gegeben.
        /// </summary>
        /// <param name="dateString">Der String, der in ein DateTime gewandelt werden soll</param>
        /// <returns></returns>
        public static DateTime DFS_CreateDateFromString(string dateString)
        {
            DateTime date = new DateTime();

            Match m = Regex.Match(dateString,
                     @"(\d{2})\s+((JAN)|(FEB)|(MAR)|(APR)|(MAY)|(JUN)|(JUL)|(AUG)|(SEP)|(OCT)|(NOV)|(DEC))\s+(\d{4})");
            if (m.Success)
            {
                // capture group [1]: day
                int day = int.Parse(m.Groups[1].Value);

                // capture groups [3]-[14]: months
                int month = 1;
                while (!m.Groups[month + 2].Success)
                    ++month;

                // capture group [15]: yeat
                int year = int.Parse(m.Groups[15].Value);

                date = new DateTime(year, month, day);
            }

            return date;
        }

        /// <summary>
        /// Fügt der Datenbank einen neuen Flugpatz hinzu.
        /// </summary>
        /// <param name="newFieldICAO">Der ICAO Code des Flugplatzes</param>
        private void DFS_AddNewField(string newFieldICAO)
        {
            string FieldName = null;
            Uri AFSite = DFS_SearchField(newFieldICAO, ref FieldName);
            if (AFSite != null && FieldName != null)
            {
                newFieldICAO = newFieldICAO.ToUpper();
                sts.Invoke((MethodInvoker)delegate {
                    sts.txtProgress.AppendText("Hole neuen Flugplatz: " + newFieldICAO + Environment.NewLine);
                    sts.txtProgress.AppendText("Erzeuge Datenbank-Einträge... ");
                });
                ChartButlerDataSet.AirfieldsRow afrow = chartButlerDataset.Airfields.FindByICAO(newFieldICAO);
                if (afrow != null)
                {
                    errorText = "Flugplatz " + newFieldICAO + " ist bereits vorhanden.";
                    return;
                }

                afrow = chartButlerDataset.Airfields.NewAirfieldsRow();
                afrow.ICAO = newFieldICAO;
                afrow.AFname = FieldName;
                chartButlerDataset.Airfields.AddAirfieldsRow(afrow);

                sts.Invoke((MethodInvoker)delegate {
                    sts.txtProgress.AppendText("erledigt!" + Environment.NewLine);
                    sts.progressBar.PerformStep();

                    sts.txtProgress.AppendText("Lade Karten-Liste... ");
                });
                List<DFS_ChartLink> chartLinks = DFS_GetChartLinks(afrow.ICAO, AFSite);
                if (chartLinks.Count == 0)
                {
                    errorText = "Es sind keine Karten verfügbar!";
                    return;
                }
                sts.Invoke((MethodInvoker)delegate {
                    sts.txtProgress.AppendText("erledigt!" + Environment.NewLine);

                    sts.progressBar.Maximum += 3 + chartLinks.Count - 6;
                    sts.progressBar.PerformStep();
                    sts.txtProgress.AppendText("Hole Karten-Dateien ab... " + Environment.NewLine);
                });

                // Kartenverzeichnis anlegen
                Directory.CreateDirectory(newFieldICAO + " - " + FieldName);
                try
                {
                    bool update_tripkit = true;
                    foreach (DFS_ChartLink cl in chartLinks)
                    {
                        DFS_DownloadAndCheckChart(true, cl, afrow, ref update_tripkit);
                        sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });
                    }
                    DFS_UpdateTripKitCharts(true, afrow);
                }
                catch(Exception)
                { 
                    Directory.Delete(newFieldICAO + " - " + FieldName, true);
                    throw;
                }

                if (chartButlerDataset.AIP.Count == 0)
                    chartButlerDataset.AIP.AddAIPRow(Update);

                sts.Invoke((MethodInvoker)delegate {
                    sts.txtProgress.AppendText("erledigt!" + Environment.NewLine);
                    sts.progressBar.PerformStep();
                });
            }
            else
            {
                errorText = "Flugplatz " + newFieldICAO + " nicht gefunden!";
            }
        }

        /// <summary>
        /// Überprüft, ob die vorhandenen Karten noch aktuell sind und aktualisiert diese gegebenenfalls.
        /// </summary>
        private void DFS_CheckForNewCharts()
        {
            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Letzte veröffentlichte AIP Berichtigung: " + Update.ToShortDateString() + Environment.NewLine); });

            bool same_chartbutler_version = (chartButlerDataset.ChartButler.Count != 0 
                && chartButlerDataset.ChartButler[0].Version == System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            if (chartButlerDataset.AIP.Count != 0)
            {
                DateTime lastUpdate = chartButlerDataset.AIP[0].LastUpdate;
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("AIP Stand bei letzter Überprüfung: " + lastUpdate.ToShortDateString() + Environment.NewLine); });

                if (Update == lastUpdate && same_chartbutler_version)
                {
                    sts.Invoke((MethodInvoker)delegate
                    {
                        sts.txtProgress.AppendText(Environment.NewLine + "Keine Aktualisierung notwendig!" + Environment.NewLine);
                        sts.progressBar.Value = sts.progressBar.Maximum;
                    });
                    System.Threading.Thread.Sleep(3000);
                    return;
                }
            }

            List<string> AFlist = new List<string>();

            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Überprüfe alle abonnierten Flugplätze... " + Environment.NewLine); });

            ChartButlerDataSet.AirfieldsDataTable d = chartButlerDataset.Airfields;
            for (int j = 0; j < d.Count; ++j)
                AFlist.Add(d.Rows[j][d.ICAOColumn].ToString());

            sts.Invoke((MethodInvoker)delegate {
                sts.progressBar.Maximum = 2 + AFlist.Count * 4;
                sts.progressBar.PerformStep(); });

            DFS_UpdateCharts(AFlist);

            if (chartButlerDataset.AIP.Count == 0)
                chartButlerDataset.AIP.AddAIPRow(Update);
            else
                chartButlerDataset.AIP[0].LastUpdate = Update;
        }

        /// <summary>
        /// Prüft, ob die lokalen Karten den Karten auf dem Server entsprechen und ersetzt diese ggf.
        /// </summary>
        /// <param name="AFlist">Die Liste der amendierten Flugplätze.</param>
        public void DFS_UpdateCharts(List<string> AFlist)
        {
            foreach (string ICAO in AFlist)
            {
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Prüfe " + ICAO + "... "); });
                ChartButlerDataSet.AirfieldsRow afrow = chartButlerDataset.Airfields.FindByICAO(ICAO);
                if (afrow != null)
                {
                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Prüfe Karten... " + Environment.NewLine); });
                    
                    Uri AFSite = null;
                    string airfield_permalink = null;
                    if (afrow.GetAFChartsRows().Length > 0 && !afrow.GetAFChartsRows()[0].IsCryptNull())
                    {
                        airfield_permalink = afrow.GetAFChartsRows()[0].Crypt.Split(new char[] { '#' }, 2)[0];
                        AFSite = 
                            new Uri(new Uri(Properties.Resources.DFS_PermalinkBaseURL), airfield_permalink);
                    }

                    if (AFSite == null)
                    {
                        string FieldName = null;
                        AFSite = DFS_SearchField(ICAO, ref FieldName);
                    }

                    if (AFSite != null)
                    {
                        List<DFS_ChartLink> chartLinks = DFS_GetChartLinks(afrow.ICAO, AFSite);

                        if (chartLinks.Count > 0 && chartLinks[0].airfield_permalink != airfield_permalink)
                        {
                            sts.Invoke((MethodInvoker)delegate
                            {
                                sts.txtProgress.AppendText("ACHTUNG: Der Permalink der Flugplatzes " + ICAO + " hat sich verändert!" + Environment.NewLine);
                            });
                        }

                        sts.Invoke((MethodInvoker)delegate
                        {
                            sts.progressBar.Maximum = sts.progressBar.Maximum - 3 + chartLinks.Count;
                            sts.progressBar.PerformStep();
                        });
                        bool tripkit_needs_update = false;
                        foreach (DFS_ChartLink cl in chartLinks)
                        {
                            DFS_DownloadAndCheckChart(false, cl, afrow, ref tripkit_needs_update);
                            sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });
                        }

                        DFS_RemoveOrphanCharts(afrow, chartLinks, ref tripkit_needs_update);

                        if (tripkit_needs_update)
                            DFS_UpdateTripKitCharts(false, afrow);
                    }
                    else
                    {
                        errorText = "Flugplatz " + ICAO + " nicht gefunden!";
                        return;
                    }
                }
                else
                {
                    sts.Invoke((MethodInvoker)delegate
                    {
                        sts.txtProgress.AppendText("Nicht abonniert!" + Environment.NewLine);
                        sts.progressBar.Value += 4;
                    });
                }
            }

            sts.Invoke((MethodInvoker)delegate
            {
                sts.txtProgress.AppendText(Environment.NewLine + "Aktualisierung beendet." + Environment.NewLine);
                sts.progressBar.Value = sts.progressBar.Maximum;
            });
            System.Threading.Thread.Sleep(3000);
        }

        /// <summary>
        /// Entfernt Karten, die nicht mehr auf dem DFS Server bereit gestellt werden, aus der Datenbank und löscht diese aus dem Kartenverzeichnis.
        /// </summary>
        /// <param name="afrow">Der Datenbankeintrag des FLugplatzes</param>
        /// <param name="chartLinks">Die von DFS_GetCHartsLinks() ermittelten Karten auf dem DFS Server</param>
        /// <param name="tripkit_needs_update">wird auf wahr gesetzt, falls der Kartensatz verändert wird</param>
        public void DFS_RemoveOrphanCharts(ChartButlerDataSet.AirfieldsRow afrow, List<DFS_ChartLink> chartLinks, ref bool tripkit_needs_update)
        {
            for (int i = 0; i < afrow.GetAFChartsRows().Length;)
            {
                ChartButlerDataSet.AFChartsRow chartRow = afrow.GetAFChartsRows()[i];
                // Hinweis: Das TripKit hat keinen "Crypt" Eintrag, d.h. keine Verlinkung auf den Server.
                // -> es wird hier übersprungen
                if (!chartRow.IsCryptNull())
                {
                    string chartName = chartRow.Crypt.Split(new char[] { '#' })[1];
                    bool found = false;
                    foreach (DFS_ChartLink cl in chartLinks)
                        if (cl.name == chartName)
                            found = true;
                    if (!found)
                    {
                        string chartPath = Utility.BuildChartPath(chartRow);
                        string previewPath = Utility.BuildChartPreviewPath(chartRow, "png");
                        if (File.Exists(chartPath))
                            File.Delete(chartPath);
                        if (File.Exists(previewPath))
                            File.Delete(previewPath);

                        chartButlerDataset.AFCharts.RemoveAFChartsRow(chartRow);
                        tripkit_needs_update = true;
                    }
                    else
                        ++i;
                }
                else
                    ++i;
            }
        }

        /// <summary>
        /// Stellt die Liste der Download-Links für alle Charts eines Flugplatzes bereit.
        /// </summary>
        /// <param name="ICAO">Die ICAO Kennung des Flugplatzes</param>
        /// <param name="AFSite">Die URL der Flugplatz Seite</param>
        /// <returns>Eine Liste mit den Download-Links</returns>
        private List<DFS_ChartLink> DFS_GetChartLinks(string ICAO, Uri AFSite)
        {
            string AFstream = DFS_GetRedirectURLText2(ref AFSite);

            // Permalink extrahieren

            int pos = 0;
            string permalink = Utility.GetTextBetweenRegex(AFstream, 
                new Regex(@"const\smyPermalink\s=\s"""), new Regex(@""""), ref pos);

            string[] permalink_segments = permalink.Split(new char[] { '/' });
            permalink = permalink_segments[permalink_segments.Length - 1];

            // Chart Links extrahieren

            List<DFS_ChartLink> chartLinks = new List<DFS_ChartLink>();

            for (pos = 0; ; )
            {
                string text = Utility.GetTextBetweenRegex(AFstream,
                    new Regex(@"<li\s+class=""document-item"">"), new Regex(@"</li>"), ref pos);
                if (-1 == pos)
                    break;

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(text);
                if (xml["a"].GetAttribute("class") == "document-link")
                {
                    string href = xml["a"].GetAttribute("href");
                    string name = null;
                    string preview_base64 = null;

                    XmlNodeList span_nodes = xml["a"].GetElementsByTagName("span");
                    foreach (XmlNode node in span_nodes)
                    {
                        if (node.Attributes["class"].Value == "document-name"
                            && node.Attributes["lang"].Value == "de")
                        {
                            name = node.InnerText;
                            if (!name.StartsWith(ICAO))
                                name = ICAO + " " + name;
                        }
                        if (node.Attributes["class"].Value == "document-icon")
                        {
                            preview_base64 = node["img"].GetAttribute("src");
                            if (preview_base64.StartsWith("data:image/png;base64,"))
                                preview_base64 = preview_base64.Substring(22);
                            else
                                preview_base64 = null;
                        }
                    }

                    if (name != null)
                    {
                        DFS_ChartLink lnk = new DFS_ChartLink();
                        lnk.airfield_permalink = permalink;
                        lnk.document_link = new Uri(AFSite, href);
                        lnk.name = name;
                        lnk.preview_data = (preview_base64 != null) ? Convert.FromBase64String(preview_base64) : null;

                        chartLinks.Add(lnk);
                    }
                }
            }

            return chartLinks;
        }

        /// <summary>
        /// Prüft, ob eine Karte ersetzt werden muss und lädt diese dann herunter.
        /// </summary>
        /// <param name="init">Wahr, falls es sich um einen neuen Flugplatz handelt, falsch, falls ein existierender Platz aktualisiert wird.</param>
        /// <param name="chartLink">Der Download-Link zur Karte.</param>
        /// <param name="afrow">Der Datenbankeintrag des Flugplatzes</param>
        /// <returns>True, falls die Karte neu angelegt oder ersetzt wurde.</returns>
        private bool DFS_DownloadAndCheckChart(bool init, DFS_ChartLink chartLink, ChartButlerDataSet.AirfieldsRow afrow, ref bool tripkit_needs_update)
        {
            string tmpPreviewPath = Path.GetTempFileName();
            string tmpChartPath = Path.GetTempFileName();

            try
            {
                ChartButlerDataSet.AFChartsRow chartRow = null;
                bool is_new_chart = false;

                BindingSource bsCH = new BindingSource(chartButlerDataset, "AFcharts");
                bsCH.Position = 0;
                int tpos = init ? -1 : bsCH.Find("Crypt", chartLink.airfield_permalink + "#" + chartLink.name);

                if (tpos != -1)
                {
                    bsCH.Position = tpos;
                    chartRow = (ChartButlerDataSet.AFChartsRow)((System.Data.DataRowView)bsCH.Current).Row;

                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText(chartRow.Cname + "... "); });
                }
                else
                {
                    string Cname = Utility.GetFilenameFor(chartLink.name) + ".png";

                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText(Cname + "... "); });

                    bsCH.Position = 0;
                    tpos = bsCH.Find("Cname", Cname);
                    if (tpos == -1)
                    {
                        is_new_chart = true;
                        chartRow = chartButlerDataset.AFCharts.NewAFChartsRow();
                        chartRow.ICAO = afrow.ICAO;
                        chartRow.Cname = Cname;
                    }
                    else
                    {
                        bsCH.Position = tpos;
                        chartRow = (ChartButlerDataSet.AFChartsRow)((System.Data.DataRowView)bsCH.Current).Row;
                    }
                    chartRow.Crypt = chartLink.airfield_permalink + "#" + chartLink.name;
                }

                File.WriteAllBytes(tmpPreviewPath, chartLink.preview_data);

                string chartPath = Utility.BuildChartPath(chartRow);
                string previewPath = Utility.BuildChartPreviewPath(chartRow, "png");

                if (!init && !is_new_chart && Utility.FileEquals(tmpPreviewPath, previewPath))
                {
                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("ist aktuell." + Environment.NewLine); });
                    return false;
                }
                else
                {
                    tripkit_needs_update = true;

                    if (is_new_chart)
                        sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("wird hinzugefügt... "); });
                    else
                        sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("wird aktualisiert... "); });

                    DateTime chartUpdate = DFS_DownloadChartFromURL(tmpChartPath, chartLink.document_link);
                    if (new DateTime() == chartUpdate)
                        chartUpdate = Update;

                    File.Delete(chartPath);
                    File.Move(tmpChartPath, chartPath);

                    File.Delete(previewPath);
                    File.SetAttributes(tmpPreviewPath, FileAttributes.Hidden);
                    File.Move(tmpPreviewPath, previewPath);
 
                    chartRow.CreationDate = File.GetLastWriteTime(chartPath).Date;
                    chartRow.LastUpdate = chartUpdate;
                    chartRow.AirfieldsRow.LastUpdate = chartUpdate;

                    // in die Liste der Aktualisierungen eintragen

                    if (!init)
                    {
                        ChartButlerDataSet.UpdatesRow updrow = chartButlerDataset.Updates.FindByDate(chartUpdate);
                        if (updrow == null)
                        {
                            updrow = chartButlerDataset.Updates.AddUpdatesRow(chartUpdate);
                            // nur die letzten 5 Aktualisierungen merken
                            while (chartButlerDataset.Updates.Count > 5)
                                chartButlerDataset.Updates.RemoveUpdatesRow(chartButlerDataset.Updates[0]);
                        }
                    }

                    if (is_new_chart)
                        chartButlerDataset.AFCharts.AddAFChartsRow(chartRow);

                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("OK." + Environment.NewLine); });

                    CChart crt = new CChart();
                    crt.SetChartName(chartRow.Cname);
                    crt.SetChartPath(chartPath);
                    cList.Add(crt);
                    return true;
                }
            }
            finally
            {
                if (File.Exists(tmpPreviewPath))
                    File.Delete(tmpPreviewPath);
                if (File.Exists(tmpChartPath))
                    File.Delete(tmpChartPath);
            }
        }

        /// <summary>
        /// Lädt eine Karte vom angegebenen document-link herunter und legt sie in einer Datei ab.
        /// </summary>
        /// <param name="path">Der Dateipfad, in der die Karte abgelegt werden soll.</param>
        /// <param name="URL">Die URL, von der sie herunter geladen werden soll.</param>
        /// <returns>Die Datum der Karte (das i.d.R. links unten auf der Karte steht) als DateTime Objekt 
        /// oder DateTime(), falls es nicht extrahiert werden konnte.</returns>
        public DateTime DFS_DownloadChartFromURL(string path, Uri URL)
        {
            string text = Utility.GetURLText2(httpClient, URL);

            // Hinweis: Permalink extrahieren wird im Moment nicht benötigt

            // Datum des Dokuments extrahieren

            int pos = 0;
            string date_string = Utility.GetTextBetweenRegex(text,
                new Regex(@"<div\sclass=""headlineText float-end"">"), new Regex("</div>"), ref pos);

            DateTime date = new DateTime();
            if (pos != -1)
                date = DFS_CreateDateFromString(date_string);

            // die eigentlichen Daten des Dokuments (das PNG Bild) extrahieren

            pos = 0;
            string data_base64 = Utility.GetTextBetweenRegex(text,
                new Regex(@"<img\s+id=""imgAIP""\s+class=""pageImage""\s+src=""data:image/png;base64,"), new Regex(@""""), ref pos);

            if (pos != -1)
            {
                byte[] data = Convert.FromBase64String(data_base64);
                File.WriteAllBytes(path, data);
            }

            return date;
        }

        /// <summary>
        /// Erzeugt oder aktualisiert das TripKit Charts PDF für den angegeben Platz.
        /// </summary>
        /// <param name="init">Wahr, wenn der Flugplatz gerade erst zur Datenbank hinzu gefügt wird</param>
        /// <param name="afrow">Der Datenbankeintrag des Platzes</param>
        private void DFS_UpdateTripKitCharts(bool init, ChartButlerDataSet.AirfieldsRow afrow)
        {
            string tmpChartPath = Path.GetTempFileName();
            string tmpPreviewPath = Path.GetTempFileName();

            try
            {
                ChartButlerDataSet.AFChartsRow chartRow = null;
                bool is_new_chart = true;

                string Cname = afrow.ICAO + "_TripKit_Charts.pdf";

                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Erstelle " + Cname + "... "); });

                // PDF Dokument + Preview erstellen

                int pageCnt = 0; // zählt DINA5 Halbseiten in einem DINA4 Landscape PDF Dokument
                DateTime chartUpdate = new DateTime();

                using (var pdfDocument = new PdfDocument())
                using (var previewBitmap = new Bitmap(840, 594, PixelFormat.Format24bppRgb))
                using (var previewGfx = Graphics.FromImage(previewBitmap))
                {
                    previewGfx.Clear(Color.White);

                    foreach (ChartButlerDataSet.AFChartsRow subChartRow in afrow.GetAFChartsRows())
                    {
                        if (subChartRow.Cname == Cname) // TripKit Eintrag in der Datenbank überspringen
                        {
                            chartRow = subChartRow;
                            is_new_chart = false;
                            continue;
                        }

                        if (!subChartRow.IsLastUpdateNull() && subChartRow.LastUpdate.Subtract(chartUpdate).TotalDays > 0)
                            chartUpdate = subChartRow.LastUpdate;

                        string subChartPath = Utility.BuildChartPath(subChartRow);
                        if (File.Exists(subChartPath))
                        {
                            using (Image img = Image.FromFile(subChartPath))
                            {
                                bool is_landscape = (img.Width > img.Height);

                                // rechte Seitenhälfte überspringen, falls Chart DINA4 Landscape ist
                                if (is_landscape && (pageCnt % 2) != 0)
                                    pageCnt++;

                                // neue Seite erstellen, falls notwendig

                                PdfPage pdfPage;
                                if (pageCnt % 2 == 0)
                                {
                                    pdfPage = pdfDocument.AddPage();
                                    pdfPage.Size = PdfSharp.PageSize.A4;
                                    pdfPage.Orientation = PdfSharp.PageOrientation.Landscape;
                                }
                                else
                                {
                                    pdfPage = pdfDocument.Pages[pageCnt / 2];
                                }

                                // PDF Seite erstellen

                                using (XImage ximg = XImage.FromGdiPlusImage((Image)img.Clone()))
                                using (XGraphics xgfx = XGraphics.FromPdfPage(pdfPage))
                                {
                                    if (is_landscape)
                                        xgfx.DrawImage(ximg, 0, 0,
                                            pdfPage.Width, pdfPage.Height);
                                    else
                                        xgfx.DrawImage(ximg, (pageCnt % 2) * pdfPage.Width / 2, 0, 
                                            pdfPage.Width / 2, pdfPage.Height);
                                }

                                // Preview Grafik erstellen

                                if (pageCnt < 2)
                                {
                                    if (is_landscape)
                                        previewGfx.DrawImage(img, 0, 0,
                                            previewBitmap.Width, previewBitmap.Height);
                                    else
                                        previewGfx.DrawImage(img, (pageCnt % 2) * previewBitmap.Width / 2, 0,
                                            previewBitmap.Width / 2, previewBitmap.Height);
                                }

                                pageCnt += is_landscape ? 2 : 1;
                            }
                        }
                    }

                    pdfDocument.Save(tmpChartPath);
                    previewBitmap.Save(tmpPreviewPath, ImageFormat.Jpeg);
                }

                // Datenbank Eintrag erstellen oder aktualisieren

                if (is_new_chart)
                {
                    chartRow = chartButlerDataset.AFCharts.NewAFChartsRow();
                    chartRow.ICAO = afrow.ICAO;
                    chartRow.Cname = Cname;
                }

                string chartPath = Utility.BuildChartPath(chartRow);
                string previewPath = Utility.BuildChartPreviewPath(chartRow, "jpg");

                File.Delete(chartPath);
                File.Move(tmpChartPath, chartPath);

                File.Delete(previewPath);
                File.SetAttributes(tmpPreviewPath, FileAttributes.Hidden);
                File.Move(tmpPreviewPath, previewPath);

                chartRow.CreationDate = File.GetLastWriteTime(chartPath).Date;
                chartRow.LastUpdate = chartUpdate;
                // chartRow.AirfieldsRow.LastUpdate muss hier nicht gesetzt werden

                // in die Liste der Aktualisierungen eintragen

                if (!init)
                {
                    ChartButlerDataSet.UpdatesRow updrow = chartButlerDataset.Updates.FindByDate(chartUpdate);
                    if (updrow == null)
                    {
                        updrow = chartButlerDataset.Updates.AddUpdatesRow(chartUpdate);
                        // nur die letzten 5 Aktualisierungen merken
                        while (chartButlerDataset.Updates.Count > 5)
                            chartButlerDataset.Updates.RemoveUpdatesRow(chartButlerDataset.Updates[0]);
                    }
                }

                if (is_new_chart)
                    chartButlerDataset.AFCharts.AddAFChartsRow(chartRow);

                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("OK." + Environment.NewLine); });

                CChart crt = new CChart();
                crt.SetChartName(chartRow.Cname);
                crt.SetChartPath(chartPath);
                cList.Add(crt);
            }
            finally
            {
                if (File.Exists(tmpPreviewPath))
                    File.Delete(tmpPreviewPath);
                if (File.Exists(tmpChartPath))
                    File.Delete(tmpChartPath);
            }
        }

        /// <summary>
        /// Ruft eine Seite ab, die ggf. per Refresh meta tag weiter geleitet wird.
        /// </summary>
        /// <param name="uri">Die abzurufende (Permalink) URL. Die URI wird auf die Weiterleitung gesetzt</param>
        /// <returns>Der HTML Text der Zielseite</returns>
        public string DFS_GetRedirectURLText2(ref Uri uri)
        {
            string text = Utility.GetURLText2(httpClient, uri);
            int pos = 0;
            string relative_redirect_url = Utility.GetTextBetweenRegex(text,
                new Regex(@"<meta\s+http-equiv=""Refresh""\s+content=""0;\s*url="), new Regex(@""""), ref pos);
            if (pos != -1)
            {
                uri = new Uri(uri, relative_redirect_url);
                text = Utility.GetURLText2(httpClient, uri);
            }
            return text;
        }

        /// <summary>
        /// Sucht nach einem Flugplatz.
        /// </summary>
        /// <param name="ICAO">Die ICAO Kennung des Flugplatzes, nach dem gesucht werden soll 
        /// oder null, wenn nur die Platzliste vom Server bzw. aus dem Cache geladen werden soll. </param>
        /// <param name="FieldName">Name des Flugplatzes</param>
        /// <returns>Relative URL des Platzes oder null, falls er nicht gefunden wurde</returns>
        public Uri DFS_SearchField(string ICAO, ref string FieldName)
        {
            // zuerst versuchen Cache File zu laden

            if (DFS_airfields_cache.mainURL == null)
            {
                try
                {
                    using (Stream stream = File.OpenRead(DFS_airfields_cache_fileName))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        DFS_airfields_cache = (DFSAirfieldsCache)formatter.Deserialize(stream);
                    }
                }
                catch (Exception)
                { }
            }

            if (DFS_MainURL.ToString() != DFS_airfields_cache.mainURL)
            {
                // Flugplatzliste holen

                Uri AirfieldsUri = new Uri(Properties.Resources.DFS_PermalinkBaseURL + Properties.Resources.DFS_PermalinkAirfields);
                string text = DFS_GetRedirectURLText2(ref AirfieldsUri);

                MatchCollection page_matches = Regex.Matches(text,
                    @"<a\s+class=""folder-link""\s+href=""([^""]*)""\s*>\s*<span\s+lang=""de""\s+class=""folder-name"">([A-Z](-[A-Z])?)\s*</span>");

                SortedDictionary<string, string> /* airfield, href */ airfields = new SortedDictionary<string, string>();

                foreach (Match page_match in page_matches)
                {
                    string page_href = page_match.Groups[1].Value;
                    string letters = page_match.Groups[2].Value;
                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Lade Flugplatzliste " + letters + "... "); });

                    text = Utility.GetURLText2(httpClient, new Uri(AirfieldsUri, page_href));

                    MatchCollection airfield_matches = Regex.Matches(text,
                        @"<a\s+class=""folder-link""\s+href=""([^""]*)""\s*>\s*<span\s+lang=""de""\s+class=""folder-name"">([^<]+)</span>");

                    foreach (Match airfield_match in airfield_matches)
                    {
                        string airfield_href = airfield_match.Groups[1].Value;
                        string airfield = airfield_match.Groups[2].Value;

                        // Flugplatz href Link zum relativen Link zur DFS_MainURL konvertieren
                        Uri airfield_uri = new Uri(AirfieldsUri, airfield_href);
                        airfield_href = DFS_MainURL.MakeRelativeUri(airfield_uri).ToString();

                        airfields.Add(airfield, airfield_href);
                    }

                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("OK." + Environment.NewLine); });
                }

                // im Cache File ablegen

                DFS_airfields_cache.mainURL = DFS_MainURL.ToString();
                DFS_airfields_cache.airfields = airfields;
                try
                {
                    using (Stream stream = new FileStream(DFS_airfields_cache_fileName, FileMode.Create))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, DFS_airfields_cache);
                    }
                }
                catch(Exception)
                { }
            }
            
            // nach Flugplatz suchen

            if (!String.IsNullOrWhiteSpace(ICAO))
            {
                ICAO = ICAO.ToUpper();
                foreach (var airfield in DFS_airfields_cache.airfields)
                {
                    if (airfield.Key.EndsWith(" " + ICAO))
                    {
                        string Field = Utility.GetFilenameFor(airfield.Key.Remove(airfield.Key.Length - 5));
                        FieldName = Field;
                        sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Platz erkannt: " + Field + Environment.NewLine); });
                        return new Uri(DFS_MainURL, airfield.Value);
                    }
                }
            }

            return null;
        }

    }// END CLASS

}// END NAMESPACE
