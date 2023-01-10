using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Net;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;

namespace ChartButlerCS
{
    /// <summary>
    /// Diese Klasse stellt Verbindung mit dem gat24-Server her und ruft relevante Daten ab.
    /// </summary>
    public class CServerConnection
    {
        /// <summary>
        /// parent window
        /// </summary>
        private IWin32Window parent;
        /// <summary>
        /// Session ID der Verbindung
        /// </summary>
        private string SID;
        /// <summary>
        /// Liste der aktualisierten Karten
        /// </summary>
        private List<CChart> cList = new List<CChart>();
        /// <summary>
        /// Das Dataset aus frmChartDB
        /// </summary>
        ChartButlerDataSet chartButlerDataset;
        /// <summary>
        /// Text, der nach der Aktualisierung in einer MessageBox
        /// angezeigt werden soll oder null, wenn alles ok war
        /// </summary>
        string errorText;

        /// <summary>
        /// Status Dialog, der den Fortschritt anzeigt.
        /// Ein Wort zum progressBar: folgende Aktionen lösen einen
        /// "Fortschritt" aus:
        /// - Verbindung zum Server
        /// - Download der AIP Update Liste
        /// - Download der Kartenliste eines Flugplatzes
        /// - Download einer Karte.
        /// Hinweis: zur Vorberechnung der voraussichtlichen Steps werden
        /// 3 Karten pro Flugplatz angenommen.
        /// </summary>
        dlgStatus sts = new dlgStatus();

        private DateTime Update = new DateTime();
        List<string> AFlist = new List<string>();

        public struct ChartLink
        {
            public string crypt; // encrypted ID of GAT24 
            public string pdfURL; // link to pdf
            public string previewURL; // link to preview jpg
        }

        List<ChartLink> LinkList = new List<ChartLink>();
        private bool m_Upd = false;

        public CServerConnection(IWin32Window parent, ChartButlerDataSet chartButlerDataSet)
        {
            this.parent = parent;
            this.chartButlerDataset = chartButlerDataSet;
        }

        private static bool ServerCertificateValidationCallback(object sender, 
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                // Good certificate.
                return true;
            }

            bool good=false;
            try
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);

                X509Certificate2 cert2= new X509Certificate2(certificate);
                if (store.Certificates.Contains(cert2))
                    good= true;
                else if (
                    MessageBox.Show(
                    "Das GAT24 Server-Zertifikat konnte nicht verifiziert werden." + Environment.NewLine +
                    "Möchten Sie dieses Zertifikat trotzdem dauerhaft akzeptieren?" + Environment.NewLine + Environment.NewLine +
                    certificate.ToString(),
                    "Zertifizierungsfehler",
                    MessageBoxButtons.YesNo) == DialogResult.Yes )
                {
                    store.Add(new X509Certificate2(certificate));
                    good= true;
                }
                store.Close();
            }
            catch (Exception)
            {
            }
            return good;
        }

        public List<CChart> Establish(bool pUpdate = true, string newField = null)
        {
            m_Upd = pUpdate;
            sts.CreateControl();
			IntPtr dummy = sts.Handle; // sicherstellen, dass das Fensterhandle vor Threadbeginn existiert
            Thread worker = new Thread(() =>
               {
                   try
                   {
                       ConnectionWorker(newField,dummy);
                   }
                   catch (Exception exc)
                   {
                       errorText = "Entschuldigung. " + Environment.NewLine + "Es gab einen unerwarteten Fehler: " + Environment.NewLine + Environment.NewLine + exc.ToString();
                   }
                   if (errorText != null)
                   {
                       sts.Invoke((MethodInvoker)delegate
                       {
                           sts.txtProgress.AppendText("FEHLER! (siehe separates Fenster)" + Environment.NewLine);
                       });
                       MessageBox.Show(parent, errorText, "Fehler");
                   }
                   sts.Invoke((MethodInvoker)delegate { sts.Close(); });
               });
            worker.Start();
            sts.ShowDialog();
            worker.Join ();
            return cList;
        }

        /// <summary>
        /// Verbindet mit den hinterlegten Login-Daten zum Server und sucht nach geänderten Karten.
        /// </summary>
        private void ConnectionWorker(string newField, IntPtr dummy)
        {
            sts.Invoke((MethodInvoker)delegate {
                sts.progressBar.Maximum = (newField != null) ? 6 : (2 + (chartButlerDataset.Airfields.Count * 4));
                sts.txtProgress.AppendText("Verbinde zu Server..."); });
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
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ChartButlerCS.Settings.Default.ServerURL);
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
            SID = GetSID(resultSet);
            sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });
            if (SID == "0")
            {
                errorText= "Ihre Sitzung wurde vom GAT24-Server nicht authorisiert!" + Environment.NewLine + "Bitte überprüfen Sie die Zugangsdaten.";
                return;
            }
            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("OK." + Environment.NewLine); });
            if (m_Upd)
            {
                CheckForNewCharts();
            }
            else
            {
                string FieldName = null;
                CheckForNewField(newField, ref FieldName);
                if (FieldName != null)
                {
                    newField = newField.ToUpper();
                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText("Hole neuen Flugplatz: " + newField + Environment.NewLine);
                        sts.txtProgress.AppendText("Erzeuge Datenbank-Einträge..."); });
                    ChartButlerDataSet.AirfieldsRow afrow = chartButlerDataset.Airfields.FindByICAO(newField);
                    if (afrow != null)
                    {
                        errorText="Flugplatz " + newField + " ist bereits vorhanden.";
                        return;
                    }

                    afrow = chartButlerDataset.Airfields.NewAirfieldsRow();
                    afrow.ICAO = newField.ToUpper();
                    afrow.AFname = FieldName;
                    chartButlerDataset.Airfields.AddAirfieldsRow(afrow);

                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText("erledigt!" + Environment.NewLine);
                        sts.progressBar.PerformStep();

                        sts.txtProgress.AppendText("Lade Karten-Liste..." ); });
                    string AFSite = ChartButlerCS.Settings.Default.ServerAirFieldURL + newField + "&SID=" + SID;
                    string afresult = Utility.GetURLText(AFSite, ServerCertificateValidationCallback);
                    LinkList = GetChartLinks(afresult);
                    if (LinkList.Count == 0)
                    {
                        errorText="Es sind keine Karten verfügbar!";
                        return;
                    }
                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText("erledigt!" + Environment.NewLine);

                        sts.progressBar.Maximum = 3 + LinkList.Count;
                        sts.progressBar.PerformStep();
                        sts.txtProgress.AppendText("Hole Karten-Dateien ab..." + Environment.NewLine); });

                    // Kartenverzeichnis anlegen
                    Directory.CreateDirectory(newField.ToUpper() + " - " + FieldName);

                    bool update_tripkit = true;
                    foreach (ChartLink cl in LinkList)
                    {
                        DownloadAndCheckChart(cl, afrow, ref update_tripkit);
                        sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });
                    }

                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText("erledigt!" + Environment.NewLine);
                        sts.progressBar.Value = sts.progressBar.Maximum; });
                    System.Threading.Thread.Sleep(3000);
                }
                else
                {
                    errorText = "Flugplatz " + newField + " nicht gefunden!";
                }
            }
        }
        
        /// <summary>
        /// Ruft die Seite der zuletzt geänderten Charts auf,
        /// prüft, ob aus dem lokalen Bestand Charts betroffen sind
        /// und sorgt für deren Update.
        /// </summary>
        private void CheckForNewCharts()
        {
            // Datum des letzten AIP Updates ermitteln
            string htmlText = Utility.GetURLText(InsertSID(Settings.Default.ServerAmendedURL, SID), ServerCertificateValidationCallback);
            Update = DateTime.Parse(Utility.GetTextBetween(htmlText, "Karten und Daten zum ", " berichtigt:").text);

            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Letzte AIP Berichtigung auf GAT24: "+Update.ToShortDateString()+ Environment.NewLine); });

            bool full_update_required = true;
            bool same_chartbutler_version = (chartButlerDataset.ChartButler.Count != 0 
                && chartButlerDataset.ChartButler[0].Version == System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            if (chartButlerDataset.AIP.Count != 0)
            {
                DateTime lastUpdate = chartButlerDataset.AIP[0].LastUpdate;
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("AIP Stand bei letzter Überprüfung: " + lastUpdate.ToShortDateString() + Environment.NewLine); });
                if ((Update - lastUpdate).Days == Settings.Default.UpdateInterval && same_chartbutler_version)
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
                int lasti = 0;
                while (true)
                {
                    Utility.TextPos Icao = Utility.GetTextAfterPhrase(htmlText, "&ICAO=", 4, i);
                    i = Icao.pos;
                    if (i < lasti)
                        break;
                    lasti = i;
                    AFlist.Add(Icao.text);
                }
            }
            sts.Invoke((MethodInvoker)delegate {
                sts.progressBar.Maximum = 2 + AFlist.Count * 4;
                sts.progressBar.PerformStep(); });

            UpdateCharts(AFlist);

            if (chartButlerDataset.AIP.Count == 0)
                chartButlerDataset.AIP.AddAIPRow(Update);
            else
                chartButlerDataset.AIP[0].LastUpdate = Update;
        }

       
        /// <summary>
        /// Prüft, ob die lokalen Karten den Karten auf dem Server entsprechen und ersetzt diese ggf.
        /// </summary>
        /// <param name="AFlist">Die Liste der amendierten Flugplätze.</param>
        public void UpdateCharts(List<string> AFlist)
        {
            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Scanne Airfields..." + Environment.NewLine); });
            foreach (string ICAO in AFlist)
            {
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Prüfe " + ICAO + "... "); });
                ChartButlerDataSet.AirfieldsRow afrow = chartButlerDataset.Airfields.FindByICAO(ICAO);
                if (afrow != null)
                {
                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Prüfe Karten..." + Environment.NewLine); });
                    string AFSite = ChartButlerCS.Settings.Default.ServerAirFieldURL + ICAO + "&SID=" + SID;
                    string afresult = Utility.GetURLText(AFSite, ServerCertificateValidationCallback);
                    LinkList = GetChartLinks(afresult);
                    sts.Invoke((MethodInvoker)delegate {
                        sts.progressBar.Maximum = sts.progressBar.Maximum - 3 + LinkList.Count;
                        sts.progressBar.PerformStep(); });
                    bool tripkit_needs_update = false;
                    foreach (ChartLink cl in LinkList)
                    {
                        DownloadAndCheckChart(cl, afrow, ref tripkit_needs_update);
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
        private string GetSID(string htmlStream)
        {
            Utility.TextPos tp = Utility.GetTextBetween(htmlStream, "SID=", "\"");           
            return tp.text;
        }

        /// <summary>
        /// Stellt die Liste der Download-Links für geänderte Charts bereit.
        /// </summary>
        /// <param name="AFstream">Der HTML-Quelltext, der die Links enthält.</param>
        /// <returns>Ein Liste mit den Download-Links. Das TripKit PDF, falls vorhanden, wird als letztes hinzugefügt.</returns>
        private List<ChartLink> GetChartLinks(string AFstream)
        {
            List<ChartLink> ChartLinks = new List<ChartLink>();
            int lpos = AFstream.IndexOf("pdfkarten.php?&icao=");
            if (lpos != -1)
            {
                int fstlpos = lpos;
                while (lpos >= fstlpos)
                {
                    Utility.TextPos ChartBuf = Utility.GetTextBetween(AFstream, "pdfkarten.php?&icao=", "&", lpos);
                    lpos = ChartBuf.pos;
                    if (lpos < fstlpos || ChartBuf.text.Contains("W3C"))
                        break;

                    Utility.TextPos previewBuf = Utility.GetTextBetween(AFstream, "flugplaetze/karten/", "&", lpos);
                    lpos = previewBuf.pos;
                    if (lpos < fstlpos || ChartBuf.text.Contains("W3C"))
                        break;

                    ChartLink lnk = new ChartLink();
                    lnk.crypt = ChartBuf.text;
                    lnk.pdfURL = ChartButlerCS.Settings.Default.ServerChartURL + lnk.crypt + "&SID=" + SID;
                    lnk.previewURL = ChartButlerCS.Settings.Default.ServerChartPreviewURL + previewBuf.text + "&SID=" + SID;

                    ChartLinks.Add(lnk);
                }
            }
            lpos = AFstream.IndexOf("pdftripkit.php?icao=");
            if (lpos != -1)
            {
                int fstlpos = lpos;
                Utility.TextPos TripKitBuf = Utility.GetTextBetween(AFstream, "pdftripkit.php?icao=", "&", lpos);
                lpos = TripKitBuf.pos;
                if (lpos >= fstlpos && !TripKitBuf.text.Contains("W3C"))
                {
                    ChartLink lnk = new ChartLink();
                    lnk.crypt = TripKitBuf.text;
                    lnk.pdfURL = ChartButlerCS.Settings.Default.ServerTripKitURL + lnk.crypt + "&SID=" + SID;
                    lnk.previewURL = "tripkit";

                    ChartLinks.Add(lnk);
                }
            }
            return ChartLinks;
        }

        /// <summary>
        /// Prüft, ob eine Karte ersetzt werden muss und lädt diese dann herunter.
        /// </summary>
        /// <param name="chartLink">Der Download-Link zur Karte.</param>
        /// <returns>True, falls die Karte neu angelegt oder ersetzt wurde.</returns>
        private bool DownloadAndCheckChart(ChartLink chartLink, ChartButlerDataSet.AirfieldsRow afrow, ref bool tripkit_needs_update)
        {
            string tmpPreviewPath = Path.GetTempFileName();
            string tmpPdfPath = Path.GetTempFileName();

            try
            {
                ChartButlerDataSet.AFChartsRow chartRow;
                bool is_new_chart = false;
                bool is_tripkit_chart = (chartLink.previewURL == "tripkit");

                BindingSource bsCH = new BindingSource(chartButlerDataset, "AFcharts");
                bsCH.Position = 0;
                int tpos = m_Upd ? bsCH.Find("Crypt", chartLink.crypt) : -1;

                if (tpos != -1)
                {
                    bsCH.Position = tpos;
                    chartRow = (ChartButlerDataSet.AFChartsRow)((System.Data.DataRowView)bsCH.Current).Row;

                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText(chartRow.Cname + " ... "); });
                }
                else
                {
                    // Cname aus Preview Pfad "erraten"
                    string Cname;
                    if (is_tripkit_chart)
                        Cname = afrow.ICAO + "_" + "TripKit_Charts.pdf";
                    else
                    {
                        string previewName = Utility.GetTextBetween(chartLink.previewURL, afrow.ICAO.ToLower() + "_", ".jpg").text;
                        if (previewName.StartsWith("voc"))
                            Cname = afrow.ICAO + "_" + "VisualOperationChart" + previewName.Substring(3) + ".pdf";
                        else if (previewName.StartsWith("adc"))
                            Cname = afrow.ICAO + "_" + "AerodromeChart" + previewName.Substring(3) + ".pdf";
                        else
                            Cname = afrow.ICAO + "_" + "UnknownChart" + previewName + ".pdf";
                    }

                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText(Cname + " ... "); });

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
                string CFullPath = frmChartDB.BuildChartPdfPath(chartRow);
                string previewPath = is_tripkit_chart ? "" : frmChartDB.BuildChartPreviewJpgPath(chartRow);

                if (m_Upd && !is_new_chart 
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

                    Utility.DownloadFileFromURL(tmpPdfPath, chartLink.pdfURL);
                    File.Delete(CFullPath);
                    File.Move(tmpPdfPath, CFullPath);

                    if (!is_tripkit_chart)
                    {
                        tripkit_needs_update = true;
                        File.Delete(previewPath);
                        File.SetAttributes(tmpPreviewPath, FileAttributes.Hidden);
                        File.Move(tmpPreviewPath, previewPath);
                    }

                    chartRow.CreationDate = File.GetLastWriteTime(CFullPath).Date;

                    if (m_Upd)
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
                if (File.Exists(tmpPdfPath))
                    File.Delete(tmpPdfPath);
            }
        }
        
        /// <summary>
        /// Fügt eine Session-ID in eine URL ein.
        /// </summary>
        /// <param name="URL">Die URL-Zeichenfolge.</param>
        /// <param name="SID">Die SID-Zeichenfolge.</param>
        /// <returns>Die URL mit SID als Zeichenfolge</returns>
        private static string InsertSID(string URL, string SID)
        {
            string UrlSid = null;
            int Cnt = URL.IndexOf("&SID=") + 5;
            UrlSid = URL.Insert(Cnt, SID);
            return UrlSid;
        }


        public void CheckForNewField(string Searchstr, ref string FieldIcao)
        {
            string AFSite = ChartButlerCS.Settings.Default.ServerAirFieldURL + Searchstr + "&SID=" + SID;
            string html = Utility.GetURLText(AFSite, ServerCertificateValidationCallback);
            string Field = Utility.GetTextBetween(html, "300px\">", "</td>").text;
            if (Field.Length != 0)
            {                
                Field = Field.Replace("/", "-"); 
                Field = Field.Replace("\"","");
                //TODO: Anführungszeichen aus Namen entfernen
                string buf = Field.Substring(0,1).ToUpper();
                for (int i = 1; i < Field.Length; i++)
                {
                    if (Field.Substring(i - 1, 1) == "-" | Field.Substring(i-1,1) == " ")
                    {
                        buf += Field.Substring(i, 1).ToUpper();
                    }
                    else
                    {
                        buf += Field.Substring(i, 1).ToLower();
                    }                    
                }
                Field = buf;
                FieldIcao = Field;
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Platz erkannt: " + Field + Environment.NewLine); });
            }
        }

    }// END CLASS

}// END NAMESPACE
