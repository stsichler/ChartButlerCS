namespace ChartButlerCS
{
    partial class frmChartDB
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChartDB));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.flugplatzLöschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.previewPictureBox = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.cmdNewAF = new System.Windows.Forms.Button();
            this.cmdUpdateCharts = new System.Windows.Forms.Button();
            this.cmdClose = new System.Windows.Forms.Button();
            this.cmdOptions = new System.Windows.Forms.Button();
            this.cmdHelp = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel_UpdateRequired = new System.Windows.Forms.Panel();
            this.label_Hinweis = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.chartButlerDataSet = new ChartButlerCS.ChartButlerDataSet();
            this.contextMenuStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel_UpdateRequired.SuspendLayout();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartButlerDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.PathSeparator = "";
            this.treeView1.Size = new System.Drawing.Size(188, 423);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.flugplatzLöschenToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(167, 26);
            // 
            // flugplatzLöschenToolStripMenuItem
            // 
            this.flugplatzLöschenToolStripMenuItem.Name = "flugplatzLöschenToolStripMenuItem";
            this.flugplatzLöschenToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.flugplatzLöschenToolStripMenuItem.Text = "Flugplatz löschen";
            this.flugplatzLöschenToolStripMenuItem.Click += new System.EventHandler(this.flugplatzLöschenToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(4, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.previewPictureBox);
            this.splitContainer1.Size = new System.Drawing.Size(466, 423);
            this.splitContainer1.SplitterDistance = 188;
            this.splitContainer1.TabIndex = 4;
            // 
            // previewPictureBox
            // 
            this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.previewPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewPictureBox.Location = new System.Drawing.Point(0, 0);
            this.previewPictureBox.Name = "previewPictureBox";
            this.previewPictureBox.Size = new System.Drawing.Size(274, 423);
            this.previewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.previewPictureBox.TabIndex = 0;
            this.previewPictureBox.TabStop = false;
            this.toolTip1.SetToolTip(this.previewPictureBox, "Doppelklick, um Karte in einem PDF-Reader anzuzeigen/zu drucken.");
            this.previewPictureBox.DoubleClick += new System.EventHandler(this.previewPictureBox_DoubleClick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(4, 146);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(222, 200);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox1, "Klicken, um GAT24.de zu öffnen.");
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // cmdNewAF
            // 
            this.cmdNewAF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdNewAF.Location = new System.Drawing.Point(4, 4);
            this.cmdNewAF.Name = "cmdNewAF";
            this.cmdNewAF.Size = new System.Drawing.Size(222, 37);
            this.cmdNewAF.TabIndex = 0;
            this.cmdNewAF.Text = "Flugplatz hinzufügen";
            this.cmdNewAF.UseVisualStyleBackColor = true;
            this.cmdNewAF.Click += new System.EventHandler(this.cmdNewAF_Click);
            // 
            // cmdUpdateCharts
            // 
            this.cmdUpdateCharts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdUpdateCharts.Location = new System.Drawing.Point(4, 47);
            this.cmdUpdateCharts.Name = "cmdUpdateCharts";
            this.cmdUpdateCharts.Size = new System.Drawing.Size(222, 37);
            this.cmdUpdateCharts.TabIndex = 1;
            this.cmdUpdateCharts.Text = "Karten aktualisieren";
            this.cmdUpdateCharts.UseVisualStyleBackColor = true;
            this.cmdUpdateCharts.Click += new System.EventHandler(this.cmdUpdateCharts_Click);
            // 
            // cmdClose
            // 
            this.cmdClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdClose.Location = new System.Drawing.Point(4, 388);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(222, 39);
            this.cmdClose.TabIndex = 5;
            this.cmdClose.Text = "Schließen";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // cmdOptions
            // 
            this.cmdOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOptions.Location = new System.Drawing.Point(4, 90);
            this.cmdOptions.Name = "cmdOptions";
            this.cmdOptions.Size = new System.Drawing.Size(222, 37);
            this.cmdOptions.TabIndex = 2;
            this.cmdOptions.Text = "Optionen";
            this.cmdOptions.UseVisualStyleBackColor = true;
            this.cmdOptions.Click += new System.EventHandler(this.cmdOptions_Click);
            // 
            // cmdHelp
            // 
            this.cmdHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdHelp.Location = new System.Drawing.Point(4, 343);
            this.cmdHelp.Name = "cmdHelp";
            this.cmdHelp.Size = new System.Drawing.Size(222, 39);
            this.cmdHelp.TabIndex = 4;
            this.cmdHelp.Text = "Hilfe/Info";
            this.cmdHelp.UseVisualStyleBackColor = true;
            this.cmdHelp.Click += new System.EventHandler(this.cmdHelp_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(38, 125);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "Kartenmaterial von";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmdNewAF);
            this.panel1.Controls.Add(this.cmdUpdateCharts);
            this.panel1.Controls.Add(this.cmdClose);
            this.panel1.Controls.Add(this.cmdHelp);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.cmdOptions);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(474, 0);
            this.panel1.MinimumSize = new System.Drawing.Size(222, 412);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(230, 431);
            this.panel1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.splitContainer1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(4);
            this.panel2.Size = new System.Drawing.Size(474, 431);
            this.panel2.TabIndex = 0;
            // 
            // panel_UpdateRequired
            // 
            this.panel_UpdateRequired.BackColor = System.Drawing.Color.DarkRed;
            this.panel_UpdateRequired.Controls.Add(this.label_Hinweis);
            this.panel_UpdateRequired.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_UpdateRequired.ForeColor = System.Drawing.Color.White;
            this.panel_UpdateRequired.Location = new System.Drawing.Point(0, 0);
            this.panel_UpdateRequired.Name = "panel_UpdateRequired";
            this.panel_UpdateRequired.Size = new System.Drawing.Size(704, 30);
            this.panel_UpdateRequired.TabIndex = 2;
            this.panel_UpdateRequired.Visible = false;
            // 
            // label_Hinweis
            // 
            this.label_Hinweis.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label_Hinweis.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Hinweis.Location = new System.Drawing.Point(12, 9);
            this.label_Hinweis.Name = "label_Hinweis";
            this.label_Hinweis.Size = new System.Drawing.Size(680, 13);
            this.label_Hinweis.TabIndex = 0;
            this.label_Hinweis.Text = "(Hinweis)";
            this.label_Hinweis.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.panel2);
            this.panel4.Controls.Add(this.panel1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 30);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(704, 431);
            this.panel4.TabIndex = 3;
            // 
            // chartButlerDataSet
            // 
            this.chartButlerDataSet.DataSetName = "ChartButlerDataSet";
            this.chartButlerDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // frmChartDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 461);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel_UpdateRequired);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(720, 500);
            this.Name = "frmChartDB";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmChartDB_FormClosing);
            this.Load += new System.EventHandler(this.frmChartDB_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel_UpdateRequired.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartButlerDataSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private ChartButlerDataSet chartButlerDataSet;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem flugplatzLöschenToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button cmdNewAF;
        private System.Windows.Forms.Button cmdUpdateCharts;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.Button cmdOptions;
        private System.Windows.Forms.Button cmdHelp;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox previewPictureBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel_UpdateRequired;
        private System.Windows.Forms.Label label_Hinweis;
        private System.Windows.Forms.Panel panel4;
    }
}