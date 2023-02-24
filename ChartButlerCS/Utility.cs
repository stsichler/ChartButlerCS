using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ChartButlerCS
{
    static class Utility
    {
        public static string BuildChartPath(ChartButlerDataSet.AFChartsRow chartRow)
        {
            return Path.Combine(Path.Combine(Settings.Default.ChartFolder,
                chartRow.ICAO + " - " + chartRow.AirfieldsRow.AFname), chartRow.Cname);
        }
        public static string BuildChartPreviewPath(ChartButlerDataSet.AFChartsRow chartRow, string extension)
        {
            return Path.Combine(Path.Combine(Settings.Default.ChartFolder,
                chartRow.ICAO + " - " + chartRow.AirfieldsRow.AFname),
                "." + chartRow.Cname + "_preview." + extension);
        }

        /// <summary>
        /// Lädt Daten von einer URL und speichert sie als Datei.
        /// </summary>
        /// <param name="URL">Die auszulesende URL</param>
        /// <param name="filePath">lokaler Zielpfad</param>
        public static void DownloadFileFromURL(string filePath, string URL)
        {
            using (WebClient dlcl = new WebClient())
            {
                dlcl.DownloadFile(new Uri(URL), filePath);
            }
        }

        /// <summary>
        /// Öffnet eine URL und stellt den Inhalt als Quelltext zur Verfügung.
        /// </summary>
        /// <param name="URL">Die auszulesende URL.</param>
        /// <returns>Der Seiten-Quelltext der URL als string-Objekt.</returns>
        public static string GetURLText(string URL, RemoteCertificateValidationCallback serverCertificateValidationCallback)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
            request.ServerCertificateValidationCallback += serverCertificateValidationCallback;
            request.Method = "GET";
            request.ContentType = "text/html;charset=iso-8859-1";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader dlread = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("ISO-8859-1"));
            string responseText = dlread.ReadToEnd();
            response.Close();
            dlread.Close();
            return responseText;
        }

        /// <summary>
        /// Öffnet eine URL und stellt den Inhalt als Quelltext zur Verfügung.
        /// Diese Methode ist praktisch identisch zu GetURLText, aber entspricht der Empfehlung
        /// HttpWebRequest durch HttpClient zu ersetzen.
        /// </summary>
        /// <param name="URL">Die auszulesende URL.</param>
        /// <returns>Der Seiten-Quelltext der URL als string-Objekt.</returns>
        public static string GetURLText2(HttpClient httpClient, Uri URL)
        {
            var httpResponseTask = httpClient.GetAsync(URL);
            httpResponseTask.Wait();
            HttpResponseMessage response = httpResponseTask.Result;
            response.EnsureSuccessStatusCode();
            var httpBodyTask = response.Content.ReadAsStringAsync();
            httpBodyTask.Wait();
            return httpBodyTask.Result;
        }

        /// <summary>
        /// Sucht in einem String-Objekt nach einem spezifizierten Text-Teil und stellt 
        /// die [numChars] Zeichen nach dessen Vorkommen zur Verfügung.
        /// </summary>
        /// <param name="ContainingText">Der zu durchsuchende Text.</param>
        /// <param name="SearchPhrase">Das Einleitungs-Suchmuster</param>
        /// <param name="numChars">Die Anzahl der zu lesenden Zeichen.</param>
        /// <param name="StartAtPos">Der Startpunkt. Er wird auf die Position nach dem abschließenden Ausdruck erhöht.</param>
        /// <returns>Die gefundene Zeichenkette.</returns>
        public static string GetTextAfterPhrase(string ContainingText, string SearchPhrase, int numChars, ref int StartAtPos)
        {
            int Cnt = ContainingText.IndexOf(SearchPhrase, StartAtPos) + SearchPhrase.Length;
            StartAtPos = Cnt + numChars;
            return ContainingText.Substring(Cnt, numChars);
        }

        /// <summary>
        /// Stellt die zwischen zwei Teilzeichenfolgen stehende Zeichenfolge aus einem String zur Verfügung.
        /// </summary>
        /// <param name="ContainingText">Die zu durchsuchende Zeichenfolge.</param>
        /// <param name="SearchPhrase">Die einleitende Zeichenfolge.</param>
        /// <param name="StopPhrase">Die Abschluss-Zeichenfolge.</param>
        /// <param name="StartAtPos">Der Startpunkt. Er wird auf die Position nach dem abschließenden Ausdruck erhöht
        /// oder auf -1 gesetzt, falls kein Fund vorliegt.</param>
        /// <returns>Die gefundene Zeichenkette.</returns>
        public static string GetTextBetween(string ContainingText, string SearchPhrase, string StopPhrase, ref int StartAtPos)
        {
            string result = string.Empty;
            int CntStart = ContainingText.IndexOf(SearchPhrase, StartAtPos);
            if (CntStart > 0)
            {
                CntStart += SearchPhrase.Length;
                int CntStop = ContainingText.IndexOf(StopPhrase, CntStart);
                StartAtPos = CntStop + StopPhrase.Length;
                result = ContainingText.Substring(CntStart, (CntStop - CntStart));
            }
            else
            {
                StartAtPos = -1;
            }
            return result;
        }

        /// <summary>
        /// Stellt die zwischen zwei regulären Ausdrücken stehende Zeichenfolge aus einem String zur Verfügung.
        /// </summary>
        /// <param name="ContainingText">Die zu durchsuchende Zeichenfolge.</param>
        /// <param name="StartRegex">Der einleitende reguläre Ausdruck.</param>
        /// <param name="StopRegex">Der abschließende reguläre Ausdruck.</param>
        /// <param name="StartAtPos">Der Startpunkt. Er wird auf die Position nach dem abschließenden Ausdruck erhöht 
        /// oder auf -1 gesetzt, falls kein Fund vorliegt.</param>
        /// <returns>Die gefundene Zeichenkette.</returns>
        public static string GetTextBetweenRegex(string ContainingText, Regex StartRegex, Regex StopRegex, ref int StartAtPos)
        {
            string result = string.Empty;
            Match MatchStart = StartRegex.Match(ContainingText, StartAtPos);
            if (MatchStart.Success)
            {
                int CntStart = MatchStart.Index + MatchStart.Length;
                Match MatchStop = StopRegex.Match(ContainingText, CntStart);
                if (MatchStop.Success)
                {
                    int CntStop = MatchStop.Index;
                    StartAtPos = CntStop + MatchStop.Length;
                    result = ContainingText.Substring(CntStart, (CntStop - CntStart));
                }
                else
                {
                    StartAtPos = -1;
                }
            }
            else
            {
                StartAtPos = -1;
            }
            return result;
        }

        /// <summary>
        /// Führt einen binären Vergleich zweier Dateien durch.
        /// </summary>
        /// <param name="fileName1">Pfad zu Datei 1</param>
        /// <param name="fileName2">Pfad zu Datei 2</param>
        /// <returns>Wahr, wenn die beiden Dateien binär identisch sind.</returns>
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

        /// <summary>
        /// Erzeugt einen Dateinamen für einen angegebenen String.
        /// Dabei werden aus dem String alle ungültigen Zeichen entweder ersetzt oder entfernt.
        /// Optional wird eine Extension angefügt.
        /// </summary>
        /// <param name="name">Der String</param>
        /// <returns>Einen Dateinamen</returns>
        public static string GetFilenameFor(string name, string extension = null)
        {
            string filename = Regex.Replace(name, @"[/\<>]", "-");
            filename = Regex.Replace(filename, @"['""]", "");
            return Path.ChangeExtension(filename.Trim(), extension);
        }

    }
}
