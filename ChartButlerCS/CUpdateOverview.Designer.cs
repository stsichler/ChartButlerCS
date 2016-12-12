namespace ChartButlerCS
{
    partial class CUpdateOverview
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cmdOK = new System.Windows.Forms.Button();
            this.vwCharts = new System.Windows.Forms.ListView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.sofortDruckenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdShowAll = new System.Windows.Forms.Button();
            this.cmdCopyAll = new System.Windows.Forms.Button();
            this.dlgCopyAllCharts = new System.Windows.Forms.FolderBrowserDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.contextMenuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Location = new System.Drawing.Point(3, 3);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(91, 31);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "Schließen";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // vwCharts
            // 
            this.vwCharts.ContextMenuStrip = this.contextMenuStrip1;
            this.vwCharts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vwCharts.Location = new System.Drawing.Point(4, 4);
            this.vwCharts.Name = "vwCharts";
            this.vwCharts.Size = new System.Drawing.Size(378, 254);
            this.vwCharts.TabIndex = 0;
            this.toolTip1.SetToolTip(this.vwCharts, "Doppelklick, um Karte in einem PDF-Reader anzuzeigen/zu drucken.");
            this.vwCharts.UseCompatibleStateImageBehavior = false;
            this.vwCharts.View = System.Windows.Forms.View.List;
            this.vwCharts.DoubleClick += new System.EventHandler(this.sofortDruckenToolStripMenuItem_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sofortDruckenToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(209, 26);
            // 
            // sofortDruckenToolStripMenuItem
            // 
            this.sofortDruckenToolStripMenuItem.Name = "sofortDruckenToolStripMenuItem";
            this.sofortDruckenToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.sofortDruckenToolStripMenuItem.Text = "Karte anzeigen/drucken...";
            this.sofortDruckenToolStripMenuItem.Click += new System.EventHandler(this.sofortDruckenToolStripMenuItem_Click);
            // 
            // cmdShowAll
            // 
            this.cmdShowAll.Location = new System.Drawing.Point(3, 40);
            this.cmdShowAll.Name = "cmdShowAll";
            this.cmdShowAll.Size = new System.Drawing.Size(91, 36);
            this.cmdShowAll.TabIndex = 1;
            this.cmdShowAll.Text = "Alle anzeigen / drucken";
            this.cmdShowAll.UseVisualStyleBackColor = true;
            this.cmdShowAll.Click += new System.EventHandler(this.cmdShowAll_Click);
            // 
            // cmdCopyAll
            // 
            this.cmdCopyAll.Location = new System.Drawing.Point(3, 82);
            this.cmdCopyAll.Name = "cmdCopyAll";
            this.cmdCopyAll.Size = new System.Drawing.Size(91, 31);
            this.cmdCopyAll.TabIndex = 2;
            this.cmdCopyAll.Text = "Alle kopieren...";
            this.cmdCopyAll.UseVisualStyleBackColor = true;
            this.cmdCopyAll.Click += new System.EventHandler(this.cmdCopyAll_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmdOK);
            this.panel1.Controls.Add(this.cmdShowAll);
            this.panel1.Controls.Add(this.cmdCopyAll);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(386, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(98, 262);
            this.panel1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.vwCharts);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(4);
            this.panel2.Size = new System.Drawing.Size(386, 262);
            this.panel2.TabIndex = 0;
            // 
            // CUpdateOverview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 262);
            this.ControlBox = false;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.MinimumSize = new System.Drawing.Size(364, 156);
            this.Name = "CUpdateOverview";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Neu erhaltene Karten";
            this.Load += new System.EventHandler(this.CUpdateOverview_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.ListView vwCharts;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem sofortDruckenToolStripMenuItem;
        private System.Windows.Forms.Button cmdShowAll;
        private System.Windows.Forms.Button cmdCopyAll;
        private System.Windows.Forms.FolderBrowserDialog dlgCopyAllCharts;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}