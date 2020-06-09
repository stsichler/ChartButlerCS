using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Remoting.Contexts;

namespace ChartButlerCS
{
    [Synchronization]
    public partial class frmChartDB : Form
    {
        private List<CChart> clist = new List<CChart>();
        public frmChartDB()
        {
            InitializeComponent();
            cmdNewAF.Enabled = Settings.Default.ChartFolder.Length > 0 && Settings.Default.ServerUsername.Length > 0;
            cmdUpdateCharts.Enabled = Settings.Default.ChartFolder.Length > 0 && Settings.Default.ServerUsername.Length > 0;
        }

        private void frmChartDB_Load(object sender, EventArgs e)
        {
            readDataBase();
            updateTreeView();
            updateUpdateRequiredPanel();

            if (Settings.Default.ChartFolder.Length == 0 && Settings.Default.ServerUsername.Length == 0
                && chartButlerDataSet.Airfields.Count == 0)
            {
                MessageBox.Show(this,
                    "Willkommen bei ChartButler!" + Environment.NewLine + Environment.NewLine +
                    "Zur Benutzung dieser Software wird ein GAT24 Benutzerkonto benötigt." + Environment.NewLine + Environment.NewLine +
                    "Bitte wählen Sie unter \"Optionen\" zunächst ein Karten-Hauptverzeichnis " + Environment.NewLine +
                    "aus, in dem die Anflugkarten gespeichert werden sollen und tragen Sie " + Environment.NewLine +
                    "Ihre GAT24-Zugangsdaten ein.",
                    "Willkommen");
                cmdOptions_Click(this, new EventArgs());
            }
        }

        private void frmChartDB_FormClosing(object sender, FormClosingEventArgs e)
        // Diese Methode ist eigentlich nicht nötig, verhindert aber einen Crash beim Beenden in Mono
        {
            contextMenuStrip1.SuspendLayout();
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
            {
                // von neu (unten in der Tabelle) nach alt durchgehen
                for (int updidx= chartButlerDataSet.Updates.Count - 1; updidx>=0; --updidx)
                {
                    ChartButlerDataSet.UpdatesRow updrow = chartButlerDataSet.Updates[updidx];
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

            cmdUpdateCharts.Enabled = Settings.Default.ChartFolder.Length > 0 && Settings.Default.ServerUsername.Length > 0
                && chartButlerDataSet.Airfields.Count > 0;
        }

        private void updateUpdateRequiredPanel()
        {
            if (chartButlerDataSet.Airfields.Count != 0)
            {
                if (chartButlerDataSet.AIP.Count == 0 
                    || chartButlerDataSet.ChartButler.Count == 0 || chartButlerDataSet.ChartButler[0].Version != System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
                {
                    label_Hinweis.Text = "Bitte führen Sie einen Abgleich der Karten mit dem Server durch!";
                    panel_UpdateRequired.Visible = true;
                }
                else
                {
                    DateTime lastUpdate = chartButlerDataSet.AIP[0].LastUpdate;
                    int age = (DateTime.Now.Date - lastUpdate).Days;

                    if (age >= Settings.Default.UpdateInterval)
                    {
                        label_Hinweis.Text = "Die Karten wurden zuletzt vor "+age+" Tagen mit dem Server abgeglichen und sollten daher aktualisiert werden!";
                        panel_UpdateRequired.Visible = true;
                    }
                    else
                        panel_UpdateRequired.Visible = false;
                }
            }
            else
                panel_UpdateRequired.Visible = false;

            PerformLayout();
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
                try { OpenFileInDefaultApp(BuildChartPdfPath(chrow)); }
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
                string previewPath = BuildChartPreviewJpgPath(chrow);
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

        public static DialogResult InputBox(string title, string promptText, ref string value, bool Password = false)
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
            if (Password == true)
            {
                textBox.PasswordChar = '*' ;
            }            
            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private void flugplatzLöschenToolStripMenuItem_Click(object sender, EventArgs e)
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
                        Directory.Delete(Path.Combine(Settings.Default.ChartFolder,node.Text), true);
                    }
                    catch(Exception)
                    {
                    }

                    updateTreeView();
                    updateDataBase();
                }
            }
        }

        private void cmdUpdateCharts_Click(object sender, EventArgs e)
        {
            CServerConnection cConn = new CServerConnection(this,chartButlerDataSet);            
            clist = cConn.Establish(true, null);
            updateTreeView(); 
            updateDataBase();
            updateUpdateRequiredPanel();
            if (clist != null && clist.Count != 0)
            {
                CUpdateOverview cupov = new CUpdateOverview(clist);
                cupov.Show(this);
            }
        }

        private void cmdNewAF_Click(object sender, EventArgs e)
        {
            string srchString = null;
            DialogResult res = InputBox("Neuen Flugplatz abonnieren...", "Bitte ICAO-Code eingeben", ref srchString);
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                CServerConnection conn = new CServerConnection(this,chartButlerDataSet);
                clist = conn.Establish(false, srchString);
                updateTreeView();
                updateDataBase();
                if (clist != null && clist.Count != 0)
                {
                    CUpdateOverview cupov = new CUpdateOverview(clist);
                    cupov.Show(this);
                }
            }
        }

        private void cmdOptions_Click(object sender, EventArgs e)
        {
            frmOptions opts = new frmOptions();
            if (opts.ShowDialog(this) == DialogResult.OK)
            {
                if (opts.m_ChartsPathChanged)
                    readDataBase();
                updateTreeView();
            }
            updateUpdateRequiredPanel();

            cmdNewAF.Enabled = Settings.Default.ChartFolder.Length > 0 && Settings.Default.ServerUsername.Length > 0;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try { OpenFileInDefaultApp("http://www.gat24.de"); }
            catch (Exception) {}
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
            string windowTitle = "ChartButler(C) 2020 Jörg Pauly / Stefan Sichler";

            if (Settings.Default.ChartFolder.Length > 0)
            {
                windowTitle += " - " + ChartButlerCS.Settings.Default.ChartFolder;
                Text = windowTitle;

                if (Directory.Exists(ChartButlerCS.Settings.Default.ChartFolder))
                {
                    Directory.SetCurrentDirectory(ChartButlerCS.Settings.Default.ChartFolder);
                    string dbPath = Path.Combine(Settings.Default.ChartFolder, ".ChartButler.xml");
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
            string[] dirList = Directory.GetDirectories(Settings.Default.ChartFolder.ToString());
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
                        if (l_update_needed || !File.Exists(BuildChartPreviewJpgPath(chartRow)))
                            l_update_needed= true;
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
                chartButlerDataSet.ChartButler.AddChartButlerRow("");
            chartButlerDataSet.ChartButler[0].Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (Settings.Default.ChartFolder.Length > 0)
            {
                string dbPath = Path.Combine(Settings.Default.ChartFolder, ".ChartButler.xml");
                if (chartButlerDataSet.AFCharts.Count != 0)
                {
                    string tmpDbPath = Path.GetTempFileName();
                    chartButlerDataSet.WriteXml(tmpDbPath);
                    if (!CServerConnection.FileEquals(tmpDbPath, dbPath))
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

        public static string BuildChartPdfPath(ChartButlerDataSet.AFChartsRow chartRow)
        {
            return Path.Combine(Path.Combine(Settings.Default.ChartFolder, chartRow.ICAO + " - " + chartRow.AirfieldsRow.AFname), chartRow.Cname);
        }
        public static string BuildChartPreviewJpgPath(ChartButlerDataSet.AFChartsRow chartRow)
        {
            return Path.Combine(Path.Combine(Settings.Default.ChartFolder, chartRow.ICAO + " - " + chartRow.AirfieldsRow.AFname), "." + chartRow.Cname + "_preview.jpg");
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
    }//END CLASS
}//END NAMESPACE
