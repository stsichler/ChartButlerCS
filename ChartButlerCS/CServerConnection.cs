using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading;
using System.Net.Http;

namespace ChartButlerCS
{
    /// <summary>
    /// Diese Klasse stellt Verbindung mit dem Server her und ruft relevante Daten ab.
    /// </summary>
    public partial class CServerConnection
    {
        /// <summary>
        /// parent window
        /// </summary>
        private IWin32Window parent;
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
        /// HttpClient zur Serverkommunikation
        /// </summary>
        HttpClient httpClient = new HttpClient();

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

        /// <summary>
        /// derzeitiger AIRAC Update Zeitpunkt
        /// </summary>
        private DateTime Update = new DateTime();

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
                    "Das Server-Zertifikat konnte nicht verifiziert werden." + Environment.NewLine +
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

        /// <summary>
        /// Aktualisiert die Flugplatzdatenbank durch Daten vom Server oder fügt einen neuen Flugplatz hinzu.
        /// </summary>
        /// <param name="newField">Der ICAO Code eines neu hinzu zu fügenden Flugplatzes oder null, falls alle Karten aktualisiert werden sollen.</param>
        public List<CChart> doUpdate(string newField = null)
        {
            sts.CreateControl();
            IntPtr dummy = sts.Handle; // sicherstellen, dass das Fensterhandle vor Threadbeginn existiert
            Thread worker = new Thread(() =>
               {
                   try
                   {
                       if ("DFS" == Settings.Default.DataSource)
                           DFS_ConnectionWorker(newField, dummy);
                       else if ("GAT24" == Settings.Default.DataSource)
                           GAT24_ConnectionWorker(newField, dummy);
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
                           MessageBox.Show(parent, errorText, "Fehler");
                       });
                   }
                   sts.Invoke((MethodInvoker)delegate { sts.Close(); });
               });
            worker.Start();
            sts.ShowDialog();
            worker.Join ();
            return cList;
        }

    }// END CLASS

}// END NAMESPACE
