using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Net;
using System.Windows.Forms;

namespace ChartButlerCS
{
    /// <summary>
    /// Diese Klasse stellt Verbindung mit dem gat24-Server her und ruft relevante Daten ab.
    /// </summary>
    public partial class CServerConnection
    {
        /// <summary>
        /// Session ID der Verbindung
        /// </summary>
        private string GAT24_SID;

        public struct GAT24_ChartLink
        {
            public string crypt; // encrypted ID of GAT24 
            public string pdfURL; // link to pdf
            public string previewURL; // link to preview jpg
        }

        /// <summary>
        /// Verbindet mit den hinterlegten Login-Daten zum Server und sucht nach geänderten Karten.
        /// </summary>
        private void GAT24_ConnectionWorker(string[] newFields, IntPtr dummy)
        {
            sts.Invoke((MethodInvoker)delegate {
                sts.progressBar.Maximum = (newFields != null && newFields.Length != 0) ? (newFields.Length * 6) : (2 + (chartButlerDataset.Airfields.Count * 4));
                sts.txtProgress.AppendText("Verbinde zu GAT24 Server..."); });
            string pw = null;
            if (Settings.Default.ServerPassword == null || Settings.Default.ServerPassword == "")
            {
                sts.Invoke((MethodInvoker)delegate { frmChartDB.InputBox("Passworteingabe", "Bitte geben Sie Ihr Login-Passwort ein:", ref pw, true); });
            }
            else
            {
                pw = Settings.Default.ServerPassword;
            }
            UTF8Encoding enc = new UTF8Encoding();            
            string parameter = "txtBenutzername=" + Settings.Default.ServerUsername + "&txtPasswort=" + pw + "&btnSubmitLogin=Login";
            byte[] parByte = enc.GetBytes(parameter);
            string resultSet = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ChartButlerCS.Settings.Default.GAT24_ServerURL);
                request.ServerCertificateValidationCallback += ServerCertificateValidationCallback;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = parByte.Length;
                Stream upstream = request.GetRequestStream();
                upstream.Write(parByte, 0, parByte.Length);
                upstream.Close();            
                request.Method = "GET";
                HttpWebResponse wres = (HttpWebResponse)request.GetResponse();
                StreamReader dsread = new StreamReader(wres.GetResponseStream());
                resultSet = dsread.ReadToEnd();
                wres.Close ();
                dsread.Close ();
            }
            catch (Exception exc) 
            {
                string assemblyVersion = typeof(HttpWebRequest).Assembly.GetName().Version.ToString();
                // try to find of exact file version of System Assembly
                object[] ver = typeof(HttpWebRequest).Assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
                if (ver.Length > 0)
                    assemblyVersion = ((AssemblyFileVersionAttribute)ver[0]).Version;

                errorText = "Entschuldigung. " + Environment.NewLine + "Die Verbindung zum GAT24 Server " + Environment.NewLine + "konnte nicht hergestellt werden."
                    + Environment.NewLine + Environment.NewLine + "Technische Details:"+ Environment.NewLine 
                    + exc.Message+ Environment.NewLine + "Framework Version: " + assemblyVersion;
                return;
            }
            GAT24_SID = GAT24_GetSID(resultSet);
            sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });
            if (GAT24_SID == "0")
            {
                errorText= "Ihre Sitzung wurde vom GAT24-Server nicht authorisiert!" + Environment.NewLine + "Bitte überprüfen Sie die Zugangsdaten.";
                return;
            }
            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("OK." + Environment.NewLine); });

            if (newFields != null)
            {
                foreach( var field in newFields )
                    GAT24_AddNewField(field);
            }
            else
            {
                GAT24_CheckForNewCharts();
            }
        }

        /// <summary>
        /// Fügt der Datenbank einen neuen Flugpatz hinzu.
        /// </summary>
        /// <param name="newFieldICAO">Der ICAO Code des Flugplatzes</param>
        private void GAT24_AddNewField(string newFieldICAO)
        {
            string FieldName = null;
            GAT24_SearchField(newFieldICAO, ref FieldName);
            if (FieldName != null)
            {
                newFieldICAO = newFieldICAO.ToUpper();
                sts.Invoke((MethodInvoker)delegate {
                    sts.txtProgress.AppendText("Hole neuen Flugplatz: " + newFieldICAO + Environment.NewLine);
                    sts.txtProgress.AppendText("Erzeuge Datenbank-Einträge...");
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

                    sts.txtProgress.AppendText("Lade Karten-Liste...");
                });
                string AFSite = ChartButlerCS.Settings.Default.GAT24_ServerAirFieldURL + newFieldICAO + "&SID=" + GAT24_SID;
                string afresult = Utility.GetURLText(AFSite, ServerCertificateValidationCallback);
                List<GAT24_ChartLink> chartLinks = GAT24_GetChartLinks(afresult);
                if (chartLinks.Count == 0)
                {
                    errorText = "Es sind keine Karten verfügbar!";
                    return;
                }
                sts.Invoke((MethodInvoker)delegate {
                    sts.txtProgress.AppendText("erledigt!" + Environment.NewLine);

                    sts.progressBar.Maximum = 3 + chartLinks.Count;
                    sts.progressBar.PerformStep();
                    sts.txtProgress.AppendText("Hole Karten-Dateien ab..." + Environment.NewLine);
                });

                // Kartenverzeichnis anlegen
                Directory.CreateDirectory(newFieldICAO + " - " + FieldName);
                try
                {
                    bool update_tripkit = true;
                    foreach (GAT24_ChartLink cl in chartLinks)
                    {
                        GAT24_DownloadAndCheckChart(true, cl, afrow, ref update_tripkit);
                        sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });
                    }
                }
                catch (Exception)
                {
                    Directory.Delete(newFieldICAO + " - " + FieldName, true);
                    throw;
                }

                sts.Invoke((MethodInvoker)delegate {
                    sts.txtProgress.AppendText("erledigt!" + Environment.NewLine);
                    sts.progressBar.Value = sts.progressBar.Maximum;
                });
                System.Threading.Thread.Sleep(3000);
            }
            else
            {
                errorText = "Flugplatz " + newFieldICAO + " nicht gefunden!";
            }
        }

        /// <summary>
        /// Ruft die Seite der zuletzt geänderten Charts auf,
        /// prüft, ob aus dem lokalen Bestand Charts betroffen sind
        /// und sorgt für deren Update.
        /// </summary>
        private void GAT24_CheckForNewCharts()
        {
            // Datum des letzten AIP Updates ermitteln
            string htmlText = Utility.GetURLText(GAT24_InsertSID(Settings.Default.GAT24_ServerAmendedURL, GAT24_SID), ServerCertificateValidationCallback);
            int pos = 0;
            Update = DateTime.Parse(Utility.GetTextBetween(htmlText, "Karten und Daten zum ", " berichtigt:", ref pos));

            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Letzte AIP Berichtigung auf GAT24: "+Update.ToShortDateString()+ Environment.NewLine); });

            bool full_update_required = true;
            bool same_chartbutler_version = (chartButlerDataset.ChartButler.Count != 0 
                && chartButlerDataset.ChartButler[0].Version == System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            if (chartButlerDataset.AIP.Count != 0)
            {
                DateTime lastUpdate = chartButlerDataset.AIP[0].LastUpdate;
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("AIP Stand bei letzter Überprüfung: " + lastUpdate.ToShortDateString() + Environment.NewLine); });
                if ((Update - lastUpdate).Days == Settings.Default.AiracUpdateInterval && same_chartbutler_version)
                    full_update_required = false;

                if (Update == lastUpdate && same_chartbutler_version)
                {
                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText(Environment.NewLine + "Keine Aktualisierung notwendig!" + Environment.NewLine);
                        sts.progressBar.Value = sts.progressBar.Maximum; });
                    System.Threading.Thread.Sleep(3000);
                    return;
                }
            }

            List<string> AFlist = new List<string>();

            if (full_update_required)
            {
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Überprüfe alle abonnierten Flugplätze..." + Environment.NewLine); });

                ChartButlerDataSet.AirfieldsDataTable d = chartButlerDataset.Airfields;
                for (int j = 0; j < d.Count; ++j)
                    AFlist.Add(d.Rows[j][d.ICAOColumn].ToString());
            }
            else
            {
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Überprüfe berichtigte Flugplätze..." + Environment.NewLine); });

                int i = 0;
                while (true)
                {
                    string Icao = Utility.GetTextAfterPhrase(htmlText, "&ICAO=", 4, ref i);
                    if (-1 == i)
                        break;
                    AFlist.Add(Icao);
                }
            }
            sts.Invoke((MethodInvoker)delegate {
                sts.progressBar.Maximum = 2 + AFlist.Count * 4;
                sts.progressBar.PerformStep(); });

            GAT24_UpdateCharts(AFlist);

            if (chartButlerDataset.AIP.Count == 0)
                chartButlerDataset.AIP.AddAIPRow(Update);
            else
                chartButlerDataset.AIP[0].LastUpdate = Update;
        }

       
        /// <summary>
        /// Prüft, ob die lokalen Karten den Karten auf dem Server entsprechen und ersetzt diese ggf.
        /// </summary>
        /// <param name="AFlist">Die Liste der amendierten Flugplätze.</param>
        public void GAT24_UpdateCharts(List<string> AFlist)
        {
            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Scanne Airfields..." + Environment.NewLine); });
            foreach (string ICAO in AFlist)
            {
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Prüfe " + ICAO + "... "); });
                ChartButlerDataSet.AirfieldsRow afrow = chartButlerDataset.Airfields.FindByICAO(ICAO);
                if (afrow != null)
                {
                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Prüfe Karten..." + Environment.NewLine); });
                    string AFSite = ChartButlerCS.Settings.Default.GAT24_ServerAirFieldURL + ICAO + "&SID=" + GAT24_SID;
                    string afresult = Utility.GetURLText(AFSite, ServerCertificateValidationCallback);
                    List<GAT24_ChartLink> chartLinks = GAT24_GetChartLinks(afresult);
                    sts.Invoke((MethodInvoker)delegate {
                        sts.progressBar.Maximum = sts.progressBar.Maximum - 3 + chartLinks.Count;
                        sts.progressBar.PerformStep(); });
                    bool tripkit_needs_update = false;
                    foreach (GAT24_ChartLink cl in chartLinks)
                    {
                        GAT24_DownloadAndCheckChart(false, cl, afrow, ref tripkit_needs_update);
                        sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });
                    }
                }
                else
                {
                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText("Nicht abonniert!" + Environment.NewLine);
                        sts.progressBar.Value += 4; });
                }                                                
            }
            
            sts.Invoke((MethodInvoker)delegate {
                sts.txtProgress.AppendText(Environment.NewLine + "Aktualisierung beendet." + Environment.NewLine);
                sts.progressBar.Value = sts.progressBar.Maximum; });
            System.Threading.Thread.Sleep(3000);
        }

        /// <summary>
        /// Sucht in einem HTML-Quelltext die erzeugte Session-ID.
        /// </summary>
        /// <param name="htmlStream">Der HTML-Quelltext, der durchsucht werden soll.</param>
        /// <returns>Die Session-ID.</returns>
        private string GAT24_GetSID(string htmlStream)
        {
            int pos = 0;
            return Utility.GetTextBetween(htmlStream, "SID=", "\"", ref pos);
        }

        /// <summary>
        /// Stellt die Liste der Download-Links für geänderte Charts bereit.
        /// </summary>
        /// <param name="AFstream">Der HTML-Quelltext, der die Links enthält.</param>
        /// <returns>Ein Liste mit den Download-Links. Das TripKit PDF, falls vorhanden, wird als letztes hinzugefügt.</returns>
        private List<GAT24_ChartLink> GAT24_GetChartLinks(string AFstream)
        {
            List<GAT24_ChartLink> ChartLinks = new List<GAT24_ChartLink>();
            int lpos = AFstream.IndexOf("pdfkarten.php?&icao=");
            if (lpos != -1)
            {
                int fstlpos = lpos;
                while (lpos >= fstlpos)
                {
                    string ChartBuf = Utility.GetTextBetween(AFstream, "pdfkarten.php?&icao=", "&", ref lpos);
                    if (lpos < fstlpos || ChartBuf.Contains("W3C"))
                        break;

                    string previewBuf = Utility.GetTextBetween(AFstream, "flugplaetze/karten/", "&", ref lpos);
                    if (lpos < fstlpos || previewBuf.Contains("W3C"))
                        break;

                    GAT24_ChartLink lnk = new GAT24_ChartLink();
                    lnk.crypt = ChartBuf;
                    lnk.pdfURL = ChartButlerCS.Settings.Default.GAT24_ServerChartURL + ChartBuf + "&SID=" + GAT24_SID;
                    lnk.previewURL = ChartButlerCS.Settings.Default.GAT24_ServerChartPreviewURL + previewBuf + "&SID=" + GAT24_SID;

                    ChartLinks.Add(lnk);
                }
            }
            lpos = AFstream.IndexOf("pdftripkit.php?icao=");
            if (lpos != -1)
            {
                int fstlpos = lpos;
                string TripKitBuf = Utility.GetTextBetween(AFstream, "pdftripkit.php?icao=", "&", ref lpos);

                if (lpos >= fstlpos && !TripKitBuf.Contains("W3C"))
                {
                    GAT24_ChartLink lnk = new GAT24_ChartLink();
                    lnk.crypt = TripKitBuf;
                    lnk.pdfURL = ChartButlerCS.Settings.Default.GAT24_ServerTripKitURL + TripKitBuf + "&SID=" + GAT24_SID;
                    lnk.previewURL = "tripkit";

                    ChartLinks.Add(lnk);
                }
            }
            return ChartLinks;
        }

        /// <summary>
        /// Prüft, ob eine Karte ersetzt werden muss und lädt diese dann herunter.
        /// </summary>
        /// <param name="init">Wahr, falls es sich um einen neuen Flugplatz handelt, falsch, falls ein existierender Platz aktualisiert wird.</param>
        /// <param name="chartLink">Der Download-Link zur Karte.</param>
        /// <returns>True, falls die Karte neu angelegt oder ersetzt wurde.</returns>
        private bool GAT24_DownloadAndCheckChart(bool init, GAT24_ChartLink chartLink, ChartButlerDataSet.AirfieldsRow afrow, ref bool tripkit_needs_update)
        {
            string tmpPreviewPath = Path.GetTempFileName();
            string tmpChartPath = Path.GetTempFileName();

            try
            {
                ChartButlerDataSet.AFChartsRow chartRow;
                bool is_new_chart = false;
                bool is_tripkit_chart = (chartLink.previewURL == "tripkit");

                BindingSource bsCH = new BindingSource(chartButlerDataset, "AFcharts");
                bsCH.Position = 0;
                int tpos = init ? -1 : bsCH.Find("Crypt", chartLink.crypt);

                if (tpos != -1)
                {
                    bsCH.Position = tpos;
                    chartRow = (ChartButlerDataSet.AFChartsRow)((System.Data.DataRowView)bsCH.Current).Row;

                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText(chartRow.Cname + "... "); });
                }
                else
                {
                    // Cname aus Preview Pfad "erraten"
                    string Cname;
                    if (is_tripkit_chart)
                        Cname = afrow.ICAO + "_" + "TripKit_Charts.pdf";
                    else
                    {
                        int pos = 0;
                        string previewName = Utility.GetTextBetween(chartLink.previewURL, afrow.ICAO.ToLower() + "_", ".jpg", ref pos);
                        if (previewName.StartsWith("voc"))
                            Cname = afrow.ICAO + "_" + "VisualOperationChart" + previewName.Substring(3) + ".pdf";
                        else if (previewName.StartsWith("adc"))
                            Cname = afrow.ICAO + "_" + "AerodromeChart" + previewName.Substring(3) + ".pdf";
                        else
                            Cname = afrow.ICAO + "_" + "UnknownChart" + previewName + ".pdf";
                    }

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
                    chartRow.Crypt = chartLink.crypt;
                }

                if (!is_tripkit_chart)
                    Utility.DownloadFileFromURL(tmpPreviewPath, chartLink.previewURL);
                string CFullPath = Utility.BuildChartPath(chartRow);
                string previewPath = is_tripkit_chart ? "" : Utility.BuildChartPreviewPath(chartRow, "jpg");

                if (!init && !is_new_chart 
                    && ((is_tripkit_chart && !tripkit_needs_update) || (!is_tripkit_chart && Utility.FileEquals(tmpPreviewPath, previewPath))))
                {
                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("ist aktuell." + Environment.NewLine); });
                    return false;
                }
                else
                {
                    if (is_new_chart)
                        sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("wird hinzugefügt... "); });
                    else
                        sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("wird aktualisiert... "); });

                    Utility.DownloadFileFromURL(tmpChartPath, chartLink.pdfURL);
                    File.Delete(CFullPath);
                    File.Move(tmpChartPath, CFullPath);

                    if (!is_tripkit_chart)
                    {
                        tripkit_needs_update = true;
                        File.Delete(previewPath);
                        File.SetAttributes(tmpPreviewPath, FileAttributes.Hidden);
                        File.Move(tmpPreviewPath, previewPath);
                    }

                    chartRow.CreationDate = File.GetLastWriteTime(CFullPath).Date;

                    if (!init)
                    {
                        ChartButlerDataSet.UpdatesRow updrow = chartButlerDataset.Updates.FindByDate(Update);
                        if (updrow == null)
                        {
                            updrow = chartButlerDataset.Updates.AddUpdatesRow(Update);
                            // nur die letzten 5 Aktualisierungen merken
                            while (chartButlerDataset.Updates.Count > 5)
                                chartButlerDataset.Updates.RemoveUpdatesRow(chartButlerDataset.Updates[0]);
                        }
                        chartRow.LastUpdate = Update;
                        chartRow.AirfieldsRow.LastUpdate = Update;
                    }

                    if (is_new_chart)
                        chartButlerDataset.AFCharts.AddAFChartsRow(chartRow);

                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("OK." + Environment.NewLine); });

                    CChart crt = new CChart();
                    crt.SetChartName(chartRow.Cname);
                    crt.SetChartPath(CFullPath);
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
        /// Fügt eine Session-ID in eine URL ein.
        /// </summary>
        /// <param name="URL">Die URL-Zeichenfolge.</param>
        /// <param name="SID">Die SID-Zeichenfolge.</param>
        /// <returns>Die URL mit SID als Zeichenfolge</returns>
        private static string GAT24_InsertSID(string URL, string SID)
        {
            string UrlSid = null;
            int Cnt = URL.IndexOf("&SID=") + 5;
            UrlSid = URL.Insert(Cnt, SID);
            return UrlSid;
        }


        /// <summary>
        /// Sucht nach einem Flugplatz.
        /// </summary>
        /// <param name="Searchstr">Die ICAO Kennung des Flugplatzes, nach dem gesucht werden soll</param>
        /// <param name="FieldName">Name des Flugplatzes</param>
        public void GAT24_SearchField(string Searchstr, ref string FieldName)
        {
            string AFSite = ChartButlerCS.Settings.Default.GAT24_ServerAirFieldURL + Searchstr + "&SID=" + GAT24_SID;
            string html = Utility.GetURLText(AFSite, ServerCertificateValidationCallback);
            int pos = 0;
            string Field = Utility.GetTextBetween(html, "300px\">", "</td>", ref pos);
            if (Field.Length != 0)
            {                
                Field = Field.Replace("/", "-"); 
                Field = Field.Replace("\"","");
                
                // Groß-/Kleinschreibung korrigieren
                string buf = Field.Substring(0,1).ToUpper();
                for (int i = 1; i < Field.Length; i++)
                {
                    if (Field.Substring(i - 1, 1) == "-" || Field.Substring(i-1,1) == " ")
                    {
                        buf += Field.Substring(i, 1).ToUpper();
                    }
                    else
                    {
                        buf += Field.Substring(i, 1).ToLower();
                    }                    
                }
                Field = buf;
                FieldName = Field;
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Platz erkannt: " + Field + Environment.NewLine); });
            }
        }

    }// END CLASS

}// END NAMESPACE
