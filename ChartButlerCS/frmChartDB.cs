using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Script.Serialization;

namespace ChartButlerCS
{
    [Synchronization]
    public partial class frmChartDB : Form
    {
        private List<CChart> clist = new List<CChart>();
        private bool newReleaseAvailable = false;
        private string dataSource = null;

        public frmChartDB()
        {
            InitializeComponent();

            Settings.Default.Reload();

            // Alle SSL/TLS Protokolle außer TLS1.2 und höher global deaktivieren

            ServicePointManager.SecurityProtocol = 0;
            foreach (SecurityProtocolType protocol in SecurityProtocolType.GetValues(typeof(SecurityProtocolType)))
            {
                switch (protocol)
                {
                    case SecurityProtocolType.Ssl3:
                    case SecurityProtocolType.Tls:
                    case SecurityProtocolType.Tls11:
                        break;
                    default:
                        ServicePointManager.SecurityProtocol |= protocol;
                        break;
                }
            }
        }

        private void frmChartDB_Load(object sender, EventArgs e)
        {
            readDataBase();
            checkAndPromptMigration();
            updateTreeView();
            updateUpdateRequiredPanel();
            updateButtons();

            bool showMsgBox = false;
            bool showEula = false;
            bool showOptions = false;

            string text = "";

            const string eulaVersion = "v2"; // rechtlicher Hinweis in Version v2
            if (String.IsNullOrEmpty(Settings.Default.EulaRead))
            {
                showMsgBox = true;
                showEula = true;
                text +=
                    "Rechtlicher Hinweis:" + Environment.NewLine +
                    "Bitte beachten Sie, dass Sie als Pilot für die Aktualität der verwendeten " +
                    "AIP-Charts selbst verantwortlich sind." + Environment.NewLine +
                    Environment.NewLine +
                    "ChartButlerCS kann Ihnen dabei nur als Hilfe dienen! Es handelt sich um " +
                    "keine offiziell zugelassene Software zur Verwaltung von AIP-Charts." + Environment.NewLine +
                    Environment.NewLine +
                    "Diese Software wurde zwar mit größter Sorgfalt und bestem Gewissen " +
                    "programmiert, dennoch sind Fehlfunktionen grundsätzlich nicht auszuschließen." + Environment.NewLine +
                    "Der Autor dieser Software haftet nicht für Schäden, die hieraus entstehen." + Environment.NewLine +
                    Environment.NewLine;
            }
            if (String.IsNullOrEmpty(Settings.Default.EulaRead) || Settings.Default.EulaRead != eulaVersion)
            {
                showMsgBox = true;
                showEula = true;
                text +=
                    "ChartButler nutzt als Datenquelle für AIP-Charts die kostenfreie BasicVFR AIP Veröffentlichung der DFS." + Environment.NewLine +
                    Environment.NewLine +
                    "ACHTUNG: Beachten Sie bitte, dass dies eventuell eine schriftliche Zustimmung " +
                    "durch die DFS erfordert, siehe Nutzungsbedingungen des AIS-Portals der DFS!" + Environment.NewLine +
                    Environment.NewLine;
            }

            if (Settings.Default.ChartFolder.Length == 0 && chartButlerDataSet.Airfields.Count == 0)
            {
                showMsgBox = true;
                showOptions = true;
                text +=
                    "Bitte wählen Sie unter \"Optionen\" zunächst ein Karten-Hauptverzeichnis aus." + Environment.NewLine +
                    Environment.NewLine;
                }

            if (showMsgBox)
            {
                if (DialogResult.OK != MessageBox.Show(this, text, "Willkommen bei ChartButler!",
                    showEula ? MessageBoxButtons.OKCancel : MessageBoxButtons.OK,
                    showEula ? MessageBoxIcon.Exclamation : MessageBoxIcon.None, MessageBoxDefaultButton.Button2))
                {
                    this.Close();
                    return;
                }
            }

            if (showEula)
            {
                try
                {
                    Settings.Default.EulaRead = eulaVersion;
                    Settings.Default.Save();
                }
                catch (Exception)
                {
                    MessageBox.Show(Parent, "Die Einstellungen konnten nicht gespeichert werden!", "ChartButler");
                }
            }

            if (showOptions)
                cmdOptions_Click(this, new EventArgs());
#if !DEBUG
            checkForLatestReleaseAsync();
#endif
        }

        private void frmChartDB_FormClosing(object sender, FormClosingEventArgs e)
        // Diese Methode ist eigentlich nicht nötig, verhindert aber einen Crash beim Beenden in Mono
        {
            contextMenuStrip1.SuspendLayout();
            try
            {
                checkLatestReleaseCts?.Cancel();
            }
            catch (Exception) { }
        }

        private void updateTreeView()
        {
            treeView1.SuspendLayout();
            treeView1.Nodes.Clear();

            TreeNode airfieldsNode = new TreeNode();
            // see http://stackoverflow.com/questions/13035701/bold-treeview-node-truncated-official-fix-will-not-work-because-code-in-constr
            airfieldsNode.NodeFont = new Font(treeView1.Font, FontStyle.Bold);
            treeView1.Nodes.Add(airfieldsNode);
            airfieldsNode.Text = "Flugplätze";
            {
                BindingSource airfieldsBindingSource = new BindingSource(chartButlerDataSet, "Airfields");
                airfieldsBindingSource.Sort = "ICAO";
                for (int afidx = 0; afidx < airfieldsBindingSource.Count; afidx++)
                {
                    ChartButlerDataSet.AirfieldsRow afrow = (ChartButlerDataSet.AirfieldsRow)((DataRowView)airfieldsBindingSource[afidx]).Row;
                    TreeNode afNode = airfieldsNode.Nodes.Add(afrow.ICAO + " - " + afrow.AFname);
                    afNode.Tag = afrow;
                    afNode.ContextMenuStrip = contextMenuStrip1;

                    BindingSource chartsBindingSource = new BindingSource(chartButlerDataSet, "AFcharts");
                    chartsBindingSource.Filter = "ICAO = '" + afrow.ICAO + "'";
                    chartsBindingSource.Sort = "Cname";

                    for (int chartidx = 0; chartidx < chartsBindingSource.Count; chartidx++)
                    {
                        ChartButlerDataSet.AFChartsRow chartrow = (ChartButlerDataSet.AFChartsRow)((DataRowView)chartsBindingSource[chartidx]).Row;
                        TreeNode chartNode = afNode.Nodes.Add(chartrow.Cname);
                        chartNode.Tag = chartrow;
                        chartNode.ContextMenuStrip = contextMenuStrip1;
                    }
                    chartsBindingSource.Dispose();
                }
                airfieldsBindingSource.Dispose();
                airfieldsNode.Expand();
            }

            TreeNode updatesNode = new TreeNode();
            // see http://stackoverflow.com/questions/13035701/bold-treeview-node-truncated-official-fix-will-not-work-because-code-in-constr
            updatesNode.NodeFont = new Font(treeView1.Font, FontStyle.Bold);
            treeView1.Nodes.Add(updatesNode);
            updatesNode.Text = "Aktualisierungen";
            using (BindingSource updatesBindingSource = new BindingSource(chartButlerDataSet, "Updates"))
            {
                updatesBindingSource.Sort = "Date";
                for (int updidx = updatesBindingSource.Count - 1; updidx >= 0; --updidx)
                {
                    ChartButlerDataSet.UpdatesRow updrow = (ChartButlerDataSet.UpdatesRow)((DataRowView)updatesBindingSource[updidx]).Row;
                    TreeNode updNode = new TreeNode(updrow.Date.ToShortDateString());
                    updNode.Tag = updrow;

                    BindingSource chartsBindingSource = new BindingSource(chartButlerDataSet, "AFcharts");
                    chartsBindingSource.Filter = "LastUpdate = '" + updrow.Date + "'";
                    chartsBindingSource.Sort = "Cname";

                    for (int chartidx = 0; chartidx < chartsBindingSource.Count; chartidx++)
                    {
                        ChartButlerDataSet.AFChartsRow chartrow = (ChartButlerDataSet.AFChartsRow)((DataRowView)chartsBindingSource[chartidx]).Row;
                        updNode.Nodes.Add(chartrow.Cname).Tag = chartrow;
                    }
                    if (updNode.Nodes.Count != 0)
                        updatesNode.Nodes.Add(updNode);
                    chartsBindingSource.Dispose();
                }
            }
            treeView1.ResumeLayout();
        }

        private void updateUpdateRequiredPanel()
        {
            if (chartButlerDataSet.Airfields.Count != 0)
            {
                if (chartButlerDataSet.AIP.Count == 0 
                    || chartButlerDataSet.ChartButler.Count == 0 || chartButlerDataSet.ChartButler[0].Version != System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
                {
                    label_updateRequired.Text = "Bitte führen Sie einen Abgleich der Karten mit dem Server durch!";
                    panel_updateRequired.BackColor = Color.DarkRed;
                    panel_updateRequired.Visible = true;
                }
                else
                {
                    DateTime lastUpdate = chartButlerDataSet.AIP[0].LastUpdate;
                    int age = (DateTime.Now.Date - lastUpdate).Days;

                    if (age >= Settings.Default.AiracUpdateInterval)
                    {
                        label_updateRequired.Text = "Die Karten wurden zuletzt vor " + age + " Tagen mit dem Server abgeglichen und sollten daher aktualisiert werden!";
                        panel_updateRequired.BackColor = Color.DarkRed;
                        panel_updateRequired.Visible = true;
                    }
                    else
                        panel_updateRequired.Visible = false;
                }
            }
            else
                panel_updateRequired.Visible = false;

            if (!panel_updateRequired.Visible && newReleaseAvailable)
            {
                label_updateRequired.Text = "Eine neue Version von ChartButler steht zur Verfügung! Klicken Sie hier, um zur Webseite zu gelangen.";
                panel_updateRequired.BackColor = Color.SteelBlue;
                panel_updateRequired.Visible = true;
            }

            PerformLayout();
        }

        private void updateButtons()
        {
            cmdNewAF.Enabled = Settings.Default.ChartFolder.Length > 0;
            cmdUpdateCharts.Enabled = cmdNewAF.Enabled && chartButlerDataSet.Airfields.Count != 0;
        }

        private void label_updateRequired_Click(object sender, EventArgs e)
        {
            if (panel_updateRequired.Visible)
            {
                if (panel_updateRequired.BackColor == Color.DarkRed)
                {
                    cmdUpdateCharts_Click(this, new EventArgs());
                }
                else if (panel_updateRequired.BackColor == Color.SteelBlue)
                {
                    try { OpenFileInDefaultApp(Settings.Default.ChartButlerURL); }
                    catch (Exception) { }
                }
            }
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // sicherstellen, dass ein Rechtclick ebenfalls dauerhaft eine Node selektiert, damit
            // sich das ContextMenu auf den richtigen Eintrag bezieht 
            treeView1.SelectedNode = e.Node;
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;
            if (node != null && node.Tag != null && node.Tag.GetType() == typeof(ChartButlerDataSet.AFChartsRow))
            {
                ChartButlerDataSet.AFChartsRow chrow = (ChartButlerDataSet.AFChartsRow)(node.Tag);
                try { OpenFileInDefaultApp(Utility.BuildChartPath(chrow)); }
                catch (Exception) { }
            }
        }

        private void treeView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                treeView1_NodeMouseDoubleClick(null,
                    new TreeNodeMouseClickEventArgs(treeView1.SelectedNode,MouseButtons.Left,2,0,0));
                e.Handled = true;
            }
        }

        private void previewPictureBox_DoubleClick(object sender, EventArgs e)
        {
            treeView1_NodeMouseDoubleClick(null,
                new TreeNodeMouseClickEventArgs(treeView1.SelectedNode,MouseButtons.Left,2,0,0));
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            Image preview = null;
            if (node != null && node.Tag != null && node.Tag.GetType() == typeof(ChartButlerDataSet.AFChartsRow))
            {
                ChartButlerDataSet.AFChartsRow chrow = (ChartButlerDataSet.AFChartsRow)(node.Tag);
                string previewPath = Utility.BuildChartPath(chrow);
                if (!previewPath.EndsWith(".png"))
                    previewPath = Utility.BuildChartPreviewPath(chrow, "jpg");
                try
                {
                    if (File.Exists(previewPath))
                        preview = Image.FromFile(previewPath);
                }
                catch (Exception)
                {
                }
            }            
            previewPictureBox.Image = preview;
        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "Ok";
            buttonCancel.Text = "Abbruch";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private void deleteFieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            if (node == null || node.Tag == null)
                return;
            if (node.Tag.GetType() == typeof(ChartButlerDataSet.AFChartsRow))
                node = node.Parent;
            if (node.Tag.GetType() != typeof(ChartButlerDataSet.AirfieldsRow))
                return;
            if (MessageBox.Show(this,"Soll " + node.Text + " wirklich gelöscht werden?", "Flugplatz löschen", MessageBoxButtons.YesNo,
                MessageBoxIcon.None,MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                string ICAO = node.Text.Substring(0, 4);
                ChartButlerDataSet.AirfieldsRow afrow = chartButlerDataSet.Airfields.FindByICAO(ICAO);
                if (afrow != null)
                {
                    ChartButlerDataSet.AFChartsRow[] chartrows = afrow.GetAFChartsRows();
                    foreach (ChartButlerDataSet.AFChartsRow crow in chartrows)
                        chartButlerDataSet.AFCharts.RemoveAFChartsRow(crow);

                    chartButlerDataSet.Airfields.RemoveAirfieldsRow(afrow);

                    try
                    {
                        Directory.Delete(node.Text, true);
                    }
                    catch(Exception)
                    {
                    }

                    updateTreeView();
                    updateDataBase();
                    updateButtons();
                }
            }
        }

        private void cmdUpdateCharts_Click(object sender, EventArgs e)
        {
            if (dataSource == "GAT24")
            {
                if (MessageBox.Show(this,
                    "Die aktuelle Karten-Datenbank verwendet noch GAT24 als Datenquelle, " +
                    "welche nicht mehr unterstützt wird." + Environment.NewLine + Environment.NewLine +
                    "Beim Update werden alle vorhandenen Karten gelöscht und erneut von der DFS " +
                    "heruntergeladen." + Environment.NewLine + Environment.NewLine +
                    "BITTE LEGEN SIE ZUVOR EIN BACKUP AN!",
                    "Migration erforderlich", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK)
                    return;
                dataSource = "DFS";
                switchDataBaseDataSource();
                updateTreeView();
                updateUpdateRequiredPanel();
                updateButtons();
                return;
            }
            dataSource = "DFS";
            CServerConnection cConn = new CServerConnection(this,chartButlerDataSet);            
            clist = cConn.doUpdate(null);
            updateTreeView(); 
            updateDataBase();
            updateUpdateRequiredPanel();
            if (clist != null && clist.Count != 0)
            {
                dlgUpdateOverview cupov = new dlgUpdateOverview(clist);
                cupov.Show(this);
            }
        }

        private void cmdNewAF_Click(object sender, EventArgs e)
        {
            if (dataSource == "GAT24")
            {
                MessageBox.Show(this,
                    "Bitte führen Sie zunächst ein Karten-Update durch, " +
                    "um die Datenbank auf DFS zu migrieren.",
                    "Migration erforderlich", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string srchString = null;
            DialogResult res = InputBox("Neuen Flugplatz abonnieren...", "Bitte ICAO-Code eingeben", ref srchString);
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                dataSource = "DFS";
                CServerConnection conn = new CServerConnection(this,chartButlerDataSet);
                clist = conn.doUpdate(new string[] { srchString });
                updateTreeView();
                updateDataBase();
                if (clist != null && clist.Count != 0)
                {
                    dlgUpdateOverview cupov = new dlgUpdateOverview(clist);
                    cupov.Show(this);
                }
                updateButtons();
            }
        }

        private void cmdOptions_Click(object sender, EventArgs e)
        {
            frmOptions opts = new frmOptions();
            string previous_chart_folder = (string)Settings.Default.ChartFolder.Clone();
            if (opts.ShowDialog(this) == DialogResult.OK)
            {
                if (previous_chart_folder != Settings.Default.ChartFolder)
                {
                    readDataBase();
                    checkAndPromptMigration();
                }
                updateTreeView();
            }
            updateUpdateRequiredPanel();
            updateButtons();
        }

        private void cmdHelp_Click(object sender, EventArgs e)
        {
            frmHelp fHelp = new frmHelp();
            fHelp.Show();
        }

        /// <summary>
        /// liest die Datenbank ein. Falls nicht vorhanden oder fehlerhaft, wird sie aus der vorhandenen Verzeichnisstruktur
        /// wiederhergestellt.
        /// </summary>
        public void readDataBase()
        {
            chartButlerDataSet.Clear();
            string windowTitle = "ChartButler(C) 2016-2023 Jörg Pauly / Stefan Sichler";

            if (Settings.Default.ChartFolder.Length > 0)
            {
                windowTitle += " - " + ChartButlerCS.Settings.Default.ChartFolder;
                Text = windowTitle;

                if (Directory.Exists(ChartButlerCS.Settings.Default.ChartFolder))
                {
                    Directory.SetCurrentDirectory(ChartButlerCS.Settings.Default.ChartFolder);
                    string dbPath = ".ChartButler.xml";
                    try
                    {
                        if (File.Exists(dbPath))
                            chartButlerDataSet.ReadXml(dbPath);
                    }
                    catch (Exception)
                    {
                        chartButlerDataSet.Clear();
                        MessageBox.Show(this,
                            "Beim Einlesen der Karten-Datenbank ist ein Fehler aufgetreten." + Environment.NewLine + Environment.NewLine +
                            "Es wird versucht, die Datenbank anhand der im Karten-Hauptverzeichnis" +
                            "gefundenen Karten wiederherzustellen." + Environment.NewLine +
                            "Bei der nächsten Karten-Aktualisierung werden dann alle Karten" + Environment.NewLine +
                            "auf Aktualität geprüft werden.",
                            "ChartButler", MessageBoxButtons.OK);
                    }

                    try
                    {
                        if (chartButlerDataSet.Airfields.Count == 0)
                            rebuildDataBaseFromChartDir();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(this,
                            "Beim Einlesen des Karten-Hauptverzeichnisses ist ein" + Environment.NewLine +
                            "Fehler aufgetreten." + Environment.NewLine + Environment.NewLine +
                            "Bitte überprüfen Sie die Karten manuell und starten" + Environment.NewLine +
                            "Sie ChartButler danach neu!",
                            "ChartButler", MessageBoxButtons.OK);
                    }

                    if (chartButlerDataSet.ChartButler.Count > 0)
                    {
                        dataSource = chartButlerDataSet.ChartButler[0].IsDataSourceNull()
                            ? "GAT24"
                            : chartButlerDataSet.ChartButler[0].DataSource;
                    }
                    else
                        dataSource = null;

                }
                else
                    MessageBox.Show(this,
                        "Das Karten-Hauptverzeichnis konnte nicht gefunden werden!" + Environment.NewLine + Environment.NewLine +
                        "Bitte überprüfen Sie den Pfad im \"Optionen\" Dialog." + Environment.NewLine,
                        "ChartButler", MessageBoxButtons.OK);
            }
            else
                Text = windowTitle;
        }

        public void rebuildDataBaseFromChartDir()
        {
            // Da die Aktualität der Karten über die Previews geprüft wird, ist es
            // ein Problem, wenn diese nicht vorhanden sind.
            bool l_update_needed = false;
            string[] dirList = Directory.GetDirectories(".");
            foreach (string dirName in dirList)
            {
                string strpDirName = Path.GetFileName(dirName);
                string ICAO = strpDirName.Substring(0, 4);
                if (ICAO.ToUpper() == "TEMP")
                    break;
                string airfieldName = strpDirName.Substring(7, strpDirName.Length - 7);
                CheckAFinDB(ICAO, airfieldName);
                string[] fileList = Directory.GetFiles(dirName);
                foreach (string fileName in fileList)
                {
                    if (Path.GetExtension(fileName).ToLower() == ".pdf")
                    {
                        string strpChartName = Path.GetFileName(fileName);
                        Console.WriteLine(ICAO + " -> Chart gefunden:" + strpChartName);
                        Console.WriteLine("Last Creation: " + File.GetLastWriteTime(fileName).Date);
                        ChartButlerDataSet.AFChartsRow chartRow = CheckChartInDb(ICAO, fileName);
                        if (!File.Exists(Utility.BuildChartPreviewPath(chartRow, "jpg")))
                            l_update_needed= true;
                    }
                    if (Path.GetExtension(fileName).ToLower() == ".png")
                    {
                        string strpChartName = Path.GetFileName(fileName);
                        if (!strpChartName.EndsWith("_preview.png"))
                        {
                            Console.WriteLine(ICAO + " -> Chart gefunden:" + strpChartName);
                            Console.WriteLine("Last Creation: " + File.GetLastWriteTime(fileName).Date);
                            ChartButlerDataSet.AFChartsRow chartRow = CheckChartInDb(ICAO, fileName);
                            if (!File.Exists(Utility.BuildChartPreviewPath(chartRow, "png")))
                                l_update_needed = true;
                        }
                    }
                }
            }
            if (l_update_needed)
            {
                MessageBox.Show(this,
                    "Die vorhandenen Kartendaten stammen offenbar aus"+ Environment.NewLine +
                    "einer älteren ChartButler Version." + Environment.NewLine + Environment.NewLine +
                    "Damit die Aktualität der Karten überprüft werden kann," + Environment.NewLine +
                    "werden bei der nächsten Karten-Aktualisierung eventuell" + Environment.NewLine +
                    "einige oder sogar alle Karten erneut herunter geladen!",
                    "ChartButler", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// aktualisiert die Datenbank auf der Festplatte. Falls keine Änderungen vorgenommen wurden,
        /// wird die vorhandene Datenbank nicht berührt.
        /// </summary>
        public void updateDataBase()
        {
            if (chartButlerDataSet.ChartButler.Count == 0)
                chartButlerDataSet.ChartButler.AddChartButlerRow("", "");
            chartButlerDataSet.ChartButler[0].Version = 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            chartButlerDataSet.ChartButler[0].DataSource = dataSource;

            if (Settings.Default.ChartFolder.Length > 0)
            {
                string dbPath = ".ChartButler.xml";
                if (chartButlerDataSet.AFCharts.Count != 0)
                {
                    string tmpDbPath = Path.GetTempFileName();
                    chartButlerDataSet.WriteXml(tmpDbPath);
                    if (!Utility.FileEquals(tmpDbPath, dbPath))
                    {
                        File.Delete(dbPath);
                        File.SetAttributes(tmpDbPath, FileAttributes.Hidden);
                        File.Move(tmpDbPath, dbPath);
                    }
                    else
                        File.Delete(tmpDbPath);
                }
                else
                    File.Delete(dbPath);
            }
        }

        private void checkAndPromptMigration()
        {
            if (dataSource != "GAT24")
                return;

            MessageBox.Show(this,
                "Hinweis: Die aktuelle Karten-Datenbank verwendet noch GAT24 als Datenquelle, " +
                "welche nicht mehr unterstützt wird." + Environment.NewLine + Environment.NewLine +
                "Beim nächsten Karten-Update wird die Datenbank automatisch auf die " +
                "DFS BasicVFR Datenquelle migriert.",
                "Migration erforderlich", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Wechselt die Datenquelle der aktuell geladenen Datenbank.
        /// Hierfür werden zunächst alle Karten aus dem Kartenverzeichnis gelöscht und dann alle
        /// abonnierten Flugplätze erneut zur Datenbank hinzugefügt.
        /// </summary>
        private void switchDataBaseDataSource()
        {
            string[] fieldsICAO = new string[chartButlerDataSet.Airfields.Count];
            for (int idx = 0; idx < chartButlerDataSet.Airfields.Count; ++idx)
                fieldsICAO[idx] = chartButlerDataSet.Airfields[idx].ICAO;

            // Vorhandenen Daten aus dem Weg verschieben

            string[] dirList = Directory.GetDirectories(".");

            string tempDirectory = "TEMP." + Path.GetRandomFileName();
            Directory.CreateDirectory(tempDirectory);
            foreach (string dirName in dirList)
                Directory.Move(dirName, Path.Combine(tempDirectory, Path.GetFileName(dirName)));
            if (File.Exists(".ChartButler.xml"))
                File.Move(".ChartButler.xml", Path.Combine(tempDirectory, ".ChartButler.xml"));

            try
            {
                // Datenbank neu initialisieren
                chartButlerDataSet.Clear();
                // und alle zuvor vorhandenen Flugplätze erneut laden
                CServerConnection conn = new CServerConnection(this, chartButlerDataSet);
                conn.doUpdate(fieldsICAO);

                if (chartButlerDataSet.Airfields.Count != fieldsICAO.Length)
                {
                    if (DialogResult.Cancel == MessageBox.Show(this,
                        "Entschuldigung!" + Environment.NewLine +
                        "Es konnten leider nur " + chartButlerDataSet.Airfields.Count.ToString() + " von " +
                        fieldsICAO.Length.ToString() + " Flugplätzen heruntergeladen werden!",
                        "Migration auf DFS",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
                    {
                        throw new Exception();
                    }
                }
                updateDataBase();
            }
            catch(Exception)
            {
                // im Fehlerfall alles wiederherstellen
                if (File.Exists(Path.Combine(tempDirectory, ".ChartButler.xml")))
                    File.Move(Path.Combine(tempDirectory, ".ChartButler.xml"), ".ChartButler.xml");

                string[] bakDirList = Directory.GetDirectories(tempDirectory);
                foreach (string dirName in bakDirList)
                    Directory.Move(dirName, Path.GetFileName(dirName));
                // und Datenbank neu einlesen
                readDataBase();
            }
            finally
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        private ChartButlerDataSet.AirfieldsRow CheckAFinDB(string ICAO, string AFname)
        {
            ChartButlerDataSet.AirfieldsRow rwAF = chartButlerDataSet.Airfields.FindByICAO(ICAO);
            if (rwAF == null)
            {
                rwAF = chartButlerDataSet.Airfields.NewAirfieldsRow();
                rwAF.ICAO = ICAO;
                rwAF.AFname = AFname;
                chartButlerDataSet.Airfields.AddAirfieldsRow(rwAF);
                Console.WriteLine(ICAO + "; " + AFname + " -> angelegt!");
            }
            return rwAF;
        }//CheckAFinDB

        private ChartButlerDataSet.AFChartsRow CheckChartInDb(string ICAO, string p_Path)
        {
            ChartButlerDataSet.AFChartsRow rwChart = chartButlerDataSet.AFCharts.FindByCname(Path.GetFileName(p_Path));
            if (rwChart == null)
            {
                rwChart = chartButlerDataSet.AFCharts.NewAFChartsRow();
                rwChart.ICAO = ICAO;
                rwChart.Cname = Path.GetFileName(p_Path);
                rwChart.CreationDate = File.GetLastWriteTime(p_Path).Date;
                chartButlerDataSet.AFCharts.AddAFChartsRow(rwChart);
            }
            return rwChart;
        }

        public static void OpenFileInDefaultApp(string path)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    System.Diagnostics.Process.Start("xdg-open", "\"" + path + "\"");
                    break;
                case PlatformID.MacOSX:
                    System.Diagnostics.Process.Start("open", "\"" + path + "\"");
                    break;
                case PlatformID.Win32NT:
                default:
                    System.Diagnostics.Process.Start(path);
                    break;
            }
        }

        private static readonly HttpClient checkLatestReleaseHttpClient = CreateCheckLatestReleaseHttpClient();
        private CancellationTokenSource checkLatestReleaseCts;

        private static HttpClient CreateCheckLatestReleaseHttpClient()
        {
            // Hinweis: akzeptiert wie bisher jedes Server-Zertifikat vorbehaltlos (siehe TODO.txt).
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (request, certificate, chain, sslPolicyErrors) => true
            };
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ChartButlerCS");
            return client;
        }

        private async void checkForLatestReleaseAsync()
        {
            checkLatestReleaseCts = new CancellationTokenSource();
            string latestReleaseTag = "";
            try
            {
                using (HttpResponseMessage response = await checkLatestReleaseHttpClient.GetAsync(
                    "https://api.github.com/repos/stsichler/ChartButlerCS/releases/latest", checkLatestReleaseCts.Token))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string responseFromServer = await response.Content.ReadAsStringAsync();
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        Dictionary<string, object> entries = serializer.Deserialize<Dictionary<string, object>>(responseFromServer);
                        latestReleaseTag = entries["tag_name"] as string;
                    }
                }
            }
            catch (Exception) { }

            if (latestReleaseTag != "" && latestReleaseTag != ("v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()))
            {
                newReleaseAvailable = true;
                updateUpdateRequiredPanel();
            }
        }

    }//END CLASS
}//END NAMESPACE
