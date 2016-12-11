using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private struct TextPos
        {
            public string text;
            public int pos;
        }
        private DateTime UpDate = new DateTime();
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

        static CServerConnection()
        {
            // ignore SSL certificate errors
            ServicePointManager.ServerCertificateValidationCallback += ServerCertificateValidationCallback;
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
                    "Das GAT24 Server-Zertifikat konnte nicht verifiziert werden.\n" +
                    "Möchten Sie dieses Zertifikat trotzdem dauerhaft akzeptieren?\n\n" +
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
			IntPtr dummy = sts.Handle;
            Thread worker = new Thread(() =>
               {
                   try
                   {
                       ConnectionWorker(newField,dummy);
                   }
                   catch (Exception exc)
                   {
                       errorText = "Entschuldigung. \nEs gab einen unerwarteten Fehler: \n\n" + exc.ToString();
                   }
                   finally { sts.Invoke((MethodInvoker)delegate { sts.Close(); }); }
               });
            worker.Start();
            sts.ShowDialog();
            worker.Join ();
            if (errorText != null)
                MessageBox.Show(parent, errorText, "Fehler");
            return cList;
        }

        /// <summary>
        /// Verbindet mit den hinterlegten Login-Daten zum Server und sucht nach geänderten Karten.
        /// </summary>
        /// <returns>Der Seiten-Quelltext, nur zu Testzwecken.</returns>
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
            catch (Exception) 
            {
                errorText = "Entschuldigung. \nDie Verbindung zum GAT24 Server \nkonnte nicht hergestellt werden.";
                return;
            }
            SID = GetSID(resultSet);
            sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });
            if (SID == "0")
            {
                errorText= "Ihre Sitzung wurde vom GAT24-Server nicht authorisiert!\nBitte überprüfen Sie die Zugangsdaten.";
                return;
            }
            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("OK.\n"); });
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
                        sts.txtProgress.AppendText("Hole neuen Flugplatz: " + newField + "\n");
                        sts.txtProgress.AppendText("Erzeuge Datenbank-Einträge..."); });
                    ChartButlerDataSet.AirfieldsRow afrow = chartButlerDataset.Airfields.FindByICAO(newField);
                    if (afrow != null)
                    {
                        errorText="Flugplatz " + newField + " ist bereits vorhanden.";
                        return;
                    }

                    // Kartenverzeichnis anlegen
                    Directory.CreateDirectory(newField.ToUpper() + " - " + FieldName);

                    afrow = chartButlerDataset.Airfields.NewAirfieldsRow();
                    afrow.ICAO = newField.ToUpper();
                    afrow.AFname = FieldName;
                    chartButlerDataset.Airfields.AddAirfieldsRow(afrow);

                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText("erledigt!\n");
                        sts.progressBar.PerformStep();

                        sts.txtProgress.AppendText("Lade Karten-Liste..." ); });
                    string AFSite = ChartButlerCS.Settings.Default.ServerAirFieldURL + newField + "&SID=" + SID;
                    string afresult = GetURLText(AFSite);
                    LinkList = GetChartLinks(afresult);
                    if (LinkList == null)
                    {
                        errorText="Es sind keine Karten verfügbar!";
                        return;
                    }
                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText("erledigt!\n");

                        sts.progressBar.Maximum = 3 + LinkList.Count;
                        sts.progressBar.PerformStep();
                        sts.txtProgress.AppendText("Hole Karten-Dateien ab...\n"); });

                    foreach (ChartLink cl in LinkList)
                    {
                        DownloadAndCheckChart(cl, afrow);
                        sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });
                    }

                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText("erledigt!\n");
                        sts.progressBar.Value = sts.progressBar.Maximum; });
                    System.Threading.Thread.Sleep(3000);
                }
                else
                {
                    errorText = "Flugplatz " + newField + " nicht gefunden!";
                }
            }
        }

        private static void DownloadFileFromURL(string tmpPdfPath, string URL)
        {
            using (WebClient dlcl = new WebClient())
            {
                dlcl.DownloadFile(new Uri(URL), tmpPdfPath);
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
            string htmlText = GetURLText(InsertSID(Settings.Default.ServerAmendedURL, SID));
            UpDate = DateTime.Parse(GetTextBetween(htmlText, "Karten und Daten zum ", " berichtigt:").text);

            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Letzte AIP Berichtigung auf GAT24: "+UpDate.ToShortDateString()+"\n"); });

            bool full_update_required = true;

            if (chartButlerDataset.AIP.Count == 0)
                chartButlerDataset.AIP.AddAIPRow(UpDate);
            else
            {
                DateTime lastUpdate = chartButlerDataset.AIP[0].LastUpdate;
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("AIP Stand bei letzter Überprüfung: " + lastUpdate.ToShortDateString() + "\n"); });
                if ((UpDate - lastUpdate).Days == Settings.Default.UpdateInterval)
                    full_update_required = false;

                if (UpDate == lastUpdate)
                {
                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText("\nKeine Aktualisierung notwendig!\n");
                        sts.progressBar.Value = sts.progressBar.Maximum; });
                    System.Threading.Thread.Sleep(3000);
                    return;
                }

                chartButlerDataset.AIP[0].LastUpdate = UpDate;
            }

            if (full_update_required)
            {
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Überprüfe alle abonnierten Flugplätze...\n"); });

                ChartButlerDataSet.AirfieldsDataTable d = chartButlerDataset.Airfields;
                for (int j = 0; j < d.Count; ++j)
                    AFlist.Add(d.Rows[j][d.ICAOColumn].ToString());
            }
            else
            {
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Überprüfe berichtigte Flugplätze...\n"); });

                int i = 0;
                int lasti = 0;
                while (true)
                {
                    TextPos Icao = GetTextAfterPhrase(htmlText, "&ICAO=", 4, i);
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
        }

       
        /// <summary>
        /// Prüft, ob die lokalen Karten den Karten auf dem Server entsprechen und ersetzt diese ggf.
        /// </summary>
        /// <param name="AFlist">Die Liste der amendierten Flugplätze.</param>
        public void UpdateCharts(List<string> AFlist)
        {
            sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Scanne Airfields...\n"); });
            foreach (string ICAO in AFlist)
            {
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Prüfe " + ICAO + "... "); });
                ChartButlerDataSet.AirfieldsRow afrow = chartButlerDataset.Airfields.FindByICAO(ICAO);
                if (afrow != null)
                {
                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Prüfe Karten...\n"); });
                    string AFSite = ChartButlerCS.Settings.Default.ServerAirFieldURL + ICAO + "&SID=" + SID;
                    string afresult = GetURLText(AFSite);
                    LinkList = GetChartLinks(afresult);
                    sts.Invoke((MethodInvoker)delegate {
                        sts.progressBar.Maximum = sts.progressBar.Maximum - 3 + LinkList.Count;
                        sts.progressBar.PerformStep(); });
                    foreach (ChartLink cl in LinkList)
                    {
                        DownloadAndCheckChart(cl, afrow);
                        sts.Invoke((MethodInvoker)delegate { sts.progressBar.PerformStep(); });
                    }
                }
                else
                {
                    sts.Invoke((MethodInvoker)delegate {
                        sts.txtProgress.AppendText("Nicht abonniert!\n");
                        sts.progressBar.Value += 4; });
                }                                                
            }
            
            sts.Invoke((MethodInvoker)delegate {
                sts.txtProgress.AppendText("\nAktualisierung beendet.\n");
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
            TextPos tp = GetTextBetween(htmlStream, "SID=", "\"");           
            return tp.text;
        }

        /// <summary>
        /// Stellt die Liste der Download-Links für geänderte Charts bereit.
        /// </summary>
        /// <param name="AFstream">Der HTML-Quelltext, der die Links enthält.</param>
        /// <returns>Ein Stringarray mit den Download-Links.</returns>
        private List<ChartLink> GetChartLinks(string AFstream)
        {
            List<ChartLink> ChartLinks = new List<ChartLink>();
            int lpos = AFstream.IndexOf("pdfkarten.php?&icao=");
            if (lpos == -1)
            {
                return null;
            }
            int fstlpos = lpos;
            while ( lpos >= fstlpos)
            {
                TextPos ChartBuf = GetTextBetween(AFstream, "pdfkarten.php?&icao=", "&", lpos);
                lpos = ChartBuf.pos;
                if (lpos < fstlpos || ChartBuf.text.Contains("W3C"))
                    break;

                TextPos previewBuf = GetTextBetween(AFstream, "flugplaetze/karten/", "&", lpos);
                lpos = previewBuf.pos;
                if (lpos < fstlpos || ChartBuf.text.Contains("W3C"))
                    break;

                ChartLink lnk = new ChartLink();
                lnk.crypt = ChartBuf.text;
                lnk.pdfURL = ChartButlerCS.Settings.Default.ServerChartURL + lnk.crypt + "&SID=" + SID;
                lnk.previewURL = ChartButlerCS.Settings.Default.ServerChartPreviewURL + previewBuf.text + "&SID=" + SID;

                ChartLinks.Add(lnk);                
            }
            return ChartLinks;
        }

        /// <summary>
        /// Prüft, ob eine Karte ersetzt werden muss und lädt diese dann herunter.
        /// </summary>
        /// <param name="chartLink">Der Download-Link zur Karte.</param>
        private void DownloadAndCheckChart(ChartLink chartLink, ChartButlerDataSet.AirfieldsRow afrow)
        {
            string tmpPreviewPath = Path.GetTempFileName();
            string tmpPdfPath = Path.GetTempFileName();

            try
            {
                ChartButlerDataSet.AFChartsRow chartRow;
                bool is_new_chart = false;

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
                    string previewName = GetTextBetween(chartLink.previewURL, afrow.ICAO.ToLower() + "_", ".jpg").text;
                    string Cname;
                    if (previewName.StartsWith("voc"))
                        Cname = afrow.ICAO + "_" + "VisualOperationChart" + previewName.Substring(3) + ".pdf";
                    else if (previewName.StartsWith("adc"))
                        Cname = afrow.ICAO + "_" + "AerodromeChart" + previewName.Substring(3) + ".pdf";
                    else
                        Cname = afrow.ICAO + "_" + "UnknownChart" + previewName + ".pdf";

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

                DownloadFileFromURL(tmpPreviewPath, chartLink.previewURL);
                string CFullPath = frmChartDB.BuildChartPdfPath(chartRow);
                string previewPath = frmChartDB.BuildChartPreviewJpgPath(chartRow);

                if (m_Upd && !is_new_chart && FileEquals(tmpPreviewPath, previewPath))
                {
                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("ist aktuell.\n"); });
                }
                else
                {
                    if (is_new_chart)
                        sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("wird hinzugefügt... "); });
                    else
                        sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("wird aktualisiert... "); });

                    File.Delete(CFullPath);
                    DownloadFileFromURL(CFullPath, chartLink.pdfURL);

                    File.Delete(previewPath);                    
                    File.SetAttributes(tmpPreviewPath, FileAttributes.Hidden);
                    File.Move(tmpPreviewPath, previewPath);

                    chartRow.CreationDate = File.GetLastWriteTime(CFullPath).Date;

                    if (m_Upd)
                    {
                        ChartButlerDataSet.UpdatesRow updrow = chartButlerDataset.Updates.FindByDate(UpDate);
                        if (updrow == null)
                        {
                            updrow = chartButlerDataset.Updates.AddUpdatesRow(UpDate);
                            // nur die letzten 5 Aktualisierungen merken
                            while (chartButlerDataset.Updates.Count > 5)
                                chartButlerDataset.Updates.RemoveUpdatesRow(chartButlerDataset.Updates[0]);
                        }
                        chartRow.LastUpdate = UpDate;
                        chartRow.AirfieldsRow.LastUpdate = UpDate;
                    }

                    if (is_new_chart)
                        chartButlerDataset.AFCharts.AddAFChartsRow(chartRow);

                    sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("OK.\n"); });

                    CChart crt = new CChart();
                    crt.SetChartName(chartRow.Cname);
                    crt.SetChartPath(CFullPath);
                    cList.Add(crt);
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
       /// Öffnet eine URL und stellt den Inhalt als Quelltext zur Verfügung.
       /// </summary>
       /// <param name="URL">Die auszulesende URL.</param>
       /// <returns>Der Seiten-Quelltext der URL als string-Objekt.</returns>
        private string GetURLText(string URL)
        {            
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
            request.Method = "GET";
            request.ContentType = "text/html;charset=iso-8859-1";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader dlread = new StreamReader(response.GetResponseStream(),Encoding.GetEncoding("ISO-8859-1"));
            string responseText = dlread.ReadToEnd();
            response.Close();
            dlread.Close();
            return responseText;
        }

        /// <summary>
        /// Sucht in einem String-Objekt nach einem spezifizierten Text-Teil und stellt 
        /// die [numChars] Zeichen nach dessen Vorkommen zur Verfügung.
        /// </summary>
        /// <param name="ContainingText">Der zu durchsuchende Text.</param>
        /// <param name="SearchPhrase">Das Einleitungs-Suchmuster</param>
        /// <param name="numChars">Die Anzahl der zu lesenden Zeichen.</param>
        /// <param name="StartAtPos">Optional: Der Startpunkt, default = 0.</param>
        /// <returns>Die gefundene Zeichenkette und die Position dahinter.</returns>
        private TextPos GetTextAfterPhrase(string ContainingText, string SearchPhrase, int numChars, int StartAtPos=0)
        {
            TextPos result = new TextPos();
            int Cnt = ContainingText.IndexOf(SearchPhrase, StartAtPos) + SearchPhrase.Length;
            result.pos = Cnt + numChars;
            result.text = ContainingText.Substring(Cnt, numChars);
            return result;
        }

        /// <summary>
        /// Stellt die zwischen zwei Teilzeichenfolgen stehende Zeichenfolge aus einem String zur Verfügung.
        /// </summary>
        /// <param name="ContainingText">Die zu durchsuchende Zeichenfolge.</param>
        /// <param name="SearchPhrase">Die einleitende Zeichenfolge.</param>
        /// <param name="StopPhrase">Die Abschluss-Zeichenfolge.</param>
        /// <param name="StartAtPos">Optional: Der Startpunkt, default = 0.</param>
        /// <returns>Die gefundene Zeichenkette.</returns>
        private TextPos GetTextBetween(string ContainingText, string SearchPhrase, string StopPhrase, int StartAtPos=0)
        {
            TextPos result = new TextPos();
            int CntStart = ContainingText.IndexOf(SearchPhrase, StartAtPos) + SearchPhrase.Length;
            int CntStop = ContainingText.IndexOf(StopPhrase, CntStart);
            result.pos = CntStop;
            result.text = ContainingText.Substring(CntStart, (CntStop - CntStart));
            return result;
        }

        /// <summary>
        /// Fügt eine Session-ID in eine URL ein.
        /// </summary>
        /// <param name="URL">Die URL-Zeichenfolge.</param>
        /// <param name="SID">Die SID-Zeichenfolge.</param>
        /// <returns>Die URL mit SID als Zeichenfolge</returns>
        private string InsertSID(string URL, string SID)
        {
            string UrlSid = null;
            int Cnt = URL.IndexOf("&SID=") + 5;
            UrlSid = URL.Insert(Cnt, SID);
            return UrlSid;
        }


        public void CheckForNewField(string Searchstr, ref string FieldIcao)
        {
            string AFSite = ChartButlerCS.Settings.Default.ServerAirFieldURL + Searchstr + "&SID=" + SID;
            string html = GetURLText(AFSite);
            string Field = GetTextBetween(html, "300px\">", "</td>").text;
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
                sts.Invoke((MethodInvoker)delegate { sts.txtProgress.AppendText("Platz erkannt: " + Field + "\n"); });
            }
        }

        /// <summary>
        /// Binary comparison of two files
        /// </summary>
        /// <param name="fileName1">the file to compare</param>
        /// <param name="fileName2">the other file to compare</param>
        /// <returns>a value indicateing weather the file are identical</returns>
        public static bool FileEquals(string fileName1, string fileName2)
        {
            if (!File.Exists(fileName1) || !File.Exists(fileName2))
                return false;

            FileInfo info1 = new FileInfo(fileName1);
            FileInfo info2 = new FileInfo(fileName2);
            bool same = info1.Length == info2.Length;
            
            if (same)
            {
                using (FileStream fs1 = info1.OpenRead())
                using (FileStream fs2 = info2.OpenRead())
                using (BufferedStream bs1 = new BufferedStream(fs1))
                using (BufferedStream bs2 = new BufferedStream(fs2))
                {
                    for (long i = 0; i < info1.Length; i++)
                    {
                        if (bs1.ReadByte() != bs2.ReadByte())
                        {
                            same = false;
                            break;
                        }
                    }
                }
            }

            return same;
        }

    }// END CLASS

}// END NAMESPACE
