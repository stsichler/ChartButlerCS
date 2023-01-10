using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace ChartButlerCS
{
    static class Utility
    {
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
        public static string GetURLText2(ref HttpClient httpClient, string URL)
        {
            var httpResponseTask = httpClient.GetAsync(URL);
            httpResponseTask.Wait();
            HttpResponseMessage response = httpResponseTask.Result;
            response.EnsureSuccessStatusCode();
            var httpBodyTask = response.Content.ReadAsStringAsync();
            httpBodyTask.Wait();
            return httpBodyTask.Result;
        }
        public struct TextPos
        {
            public string text;
            public int pos;
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
        public static TextPos GetTextAfterPhrase(string ContainingText, string SearchPhrase, int numChars, int StartAtPos = 0)
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
        public static TextPos GetTextBetween(string ContainingText, string SearchPhrase, string StopPhrase, int StartAtPos = 0)
        {
            TextPos result = new TextPos();
            int CntStart = ContainingText.IndexOf(SearchPhrase, StartAtPos);
            if (CntStart > 0)
            {
                CntStart += SearchPhrase.Length;
                int CntStop = ContainingText.IndexOf(StopPhrase, CntStart);
                result.pos = CntStop;
                result.text = ContainingText.Substring(CntStart, (CntStop - CntStart));
            }
            else
            {
                result.pos = -1;
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
        /// Konvertiert ein Bild (System.Drawing.Image) in ein PDF mit angegebener Größe.
        /// Dabei wird das Bild auf die angegebene Größe in X und Y Richtung gestreckt.
        /// </summary>
        /// <param name="image">Das zu konvertierende Bild</param>
        /// <param name="pageSize">Die Größe bzw. das Seitenformat des resultierenden PDF</param>
        /// <returns>Das PDF als Binärdaten (Byte array).</returns>
        public static byte[] ConvertImageToPdf(Image image, PdfSharp.PageSize pageSize = PdfSharp.PageSize.A5)
        {
            using (var document = new PdfDocument())
            {
                PdfPage page = document.AddPage();
                page.Size = pageSize;
                using (XImage img = XImage.FromGdiPlusImage((Image)image.Clone()))
                {
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    gfx.DrawImage(img, 0, 0, page.Width, page.Height);
                }
                using (var stream = new MemoryStream())
                {
                    document.Save(stream);
                    return stream.ToArray();
                }
            }
        }

        /// <summary>
        /// Konvertiert zwei Bilder (System.Drawing.Image) in ein PDF als "TripKit" Chart im DIN A4 Format.
        /// </summary>
        /// <param name="image1">Das Bild, das auf der linken Seitenhälfe (in DIN A5) dargestellt werden soll.</param>
        /// <param name="image1">Das Bild, das auf der rechten Seitenhälfe (in DIN A5) dargestellt werden soll.</param>
        /// <returns>Das PDF als Binärdaten (Byte array).</returns>
        public static byte[] ConvertImagesToTripKitPdf(Image image1, Image image2)
        {
            using (var document = new PdfDocument())
            {
                PdfPage page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                page.Orientation = PdfSharp.PageOrientation.Landscape;

                using (XImage img1 = XImage.FromGdiPlusImage((Image)image1.Clone()))
                using (XImage img2 = XImage.FromGdiPlusImage((Image)image2.Clone()))
                {
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    gfx.DrawImage(img1, 0, 0, page.Width / 2, page.Height);
                    gfx.DrawImage(img2, page.Width / 2, 0, page.Width / 2, page.Height);
                }
                using (var stream = new MemoryStream())
                {
                    document.Save(stream);
                    return stream.ToArray();
                }
            }
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

    }
}
