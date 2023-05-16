namespace ChartButlerCS
{
    partial class frmOptions
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
            this.dlgChartFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.pgLogin = new System.Windows.Forms.TabPage();
            this.txtPW2 = new System.Windows.Forms.TextBox();
            this.txtPW1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.chkSavePW = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.pgGeneral = new System.Windows.Forms.TabPage();
            this.radioButtonGAT24 = new System.Windows.Forms.RadioButton();
            this.radioButtonDFS = new System.Windows.Forms.RadioButton();
            this.labelDataSource = new System.Windows.Forms.Label();
            this.labelChartsDir = new System.Windows.Forms.Label();
            this.cmdSearch = new System.Windows.Forms.Button();
            this.txtChartPath = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pgLogin.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.pgGeneral.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(225, 4);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "Ok";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.Location = new System.Drawing.Point(306, 4);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Abbruch";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // pgLogin
            // 
            this.pgLogin.Controls.Add(this.txtPW2);
            this.pgLogin.Controls.Add(this.txtPW1);
            this.pgLogin.Controls.Add(this.label1);
            this.pgLogin.Controls.Add(this.txtUser);
            this.pgLogin.Controls.Add(this.chkSavePW);
            this.pgLogin.Location = new System.Drawing.Point(4, 22);
            this.pgLogin.Name = "pgLogin";
            this.pgLogin.Padding = new System.Windows.Forms.Padding(3);
            this.pgLogin.Size = new System.Drawing.Size(368, 175);
            this.pgLogin.TabIndex = 1;
            this.pgLogin.Text = "GAT24 Login-Daten";
            this.pgLogin.UseVisualStyleBackColor = true;
            // 
            // txtPW2
            // 
            this.txtPW2.Location = new System.Drawing.Point(15, 132);
            this.txtPW2.Name = "txtPW2";
            this.txtPW2.PasswordChar = '*';
            this.txtPW2.Size = new System.Drawing.Size(176, 20);
            this.txtPW2.TabIndex = 4;
            this.txtPW2.Leave += new System.EventHandler(this.txtPW2_Leave);
            // 
            // txtPW1
            // 
            this.txtPW1.Location = new System.Drawing.Point(15, 106);
            this.txtPW1.Name = "txtPW1";
            this.txtPW1.PasswordChar = '*';
            this.txtPW1.Size = new System.Drawing.Size(176, 20);
            this.txtPW1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Benutzername";
            // 
            // txtUser
            // 
            this.txtUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUser.Location = new System.Drawing.Point(15, 40);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(336, 20);
            this.txtUser.TabIndex = 1;
            // 
            // chkSavePW
            // 
            this.chkSavePW.AutoSize = true;
            this.chkSavePW.Location = new System.Drawing.Point(15, 82);
            this.chkSavePW.Name = "chkSavePW";
            this.chkSavePW.Size = new System.Drawing.Size(118, 17);
            this.chkSavePW.TabIndex = 2;
            this.chkSavePW.Text = "Passwort speichern";
            this.chkSavePW.UseVisualStyleBackColor = true;
            this.chkSavePW.CheckedChanged += new System.EventHandler(this.chkSavePW_CheckedChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pgGeneral);
            this.tabControl1.Controls.Add(this.pgLogin);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(4, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(376, 201);
            this.tabControl1.TabIndex = 0;
            // 
            // pgGeneral
            // 
            this.pgGeneral.Controls.Add(this.radioButtonGAT24);
            this.pgGeneral.Controls.Add(this.radioButtonDFS);
            this.pgGeneral.Controls.Add(this.labelDataSource);
            this.pgGeneral.Controls.Add(this.labelChartsDir);
            this.pgGeneral.Controls.Add(this.cmdSearch);
            this.pgGeneral.Controls.Add(this.txtChartPath);
            this.pgGeneral.Location = new System.Drawing.Point(4, 22);
            this.pgGeneral.Name = "pgGeneral";
            this.pgGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.pgGeneral.Size = new System.Drawing.Size(368, 175);
            this.pgGeneral.TabIndex = 0;
            this.pgGeneral.Text = "Allgemein";
            this.pgGeneral.UseVisualStyleBackColor = true;
            // 
            // radioButtonGAT24
            // 
            this.radioButtonGAT24.AutoSize = true;
            this.radioButtonGAT24.Location = new System.Drawing.Point(18, 123);
            this.radioButtonGAT24.Name = "radioButtonGAT24";
            this.radioButtonGAT24.Size = new System.Drawing.Size(59, 17);
            this.radioButtonGAT24.TabIndex = 5;
            this.radioButtonGAT24.Text = "GAT24";
            this.radioButtonGAT24.UseVisualStyleBackColor = true;
            this.radioButtonGAT24.CheckedChanged += new System.EventHandler(this.radioButtonGAT24_CheckedChanged);
            // 
            // radioButtonDFS
            // 
            this.radioButtonDFS.AutoSize = true;
            this.radioButtonDFS.Location = new System.Drawing.Point(18, 99);
            this.radioButtonDFS.Name = "radioButtonDFS";
            this.radioButtonDFS.Size = new System.Drawing.Size(96, 17);
            this.radioButtonDFS.TabIndex = 4;
            this.radioButtonDFS.Text = "DFS BasicVFR";
            this.radioButtonDFS.UseVisualStyleBackColor = true;
            this.radioButtonDFS.CheckedChanged += new System.EventHandler(this.radioButtonDFS_CheckedChanged);
            // 
            // labelDataSource
            // 
            this.labelDataSource.AutoSize = true;
            this.labelDataSource.Location = new System.Drawing.Point(15, 82);
            this.labelDataSource.Name = "labelDataSource";
            this.labelDataSource.Size = new System.Drawing.Size(67, 13);
            this.labelDataSource.TabIndex = 3;
            this.labelDataSource.Text = "Datenquelle:";
            // 
            // labelChartsDir
            // 
            this.labelChartsDir.AutoSize = true;
            this.labelChartsDir.Location = new System.Drawing.Point(12, 24);
            this.labelChartsDir.Name = "labelChartsDir";
            this.labelChartsDir.Size = new System.Drawing.Size(126, 13);
            this.labelChartsDir.TabIndex = 2;
            this.labelChartsDir.Text = "Karten Hauptverzeichnis:";
            // 
            // cmdSearch
            // 
            this.cmdSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSearch.Location = new System.Drawing.Point(327, 40);
            this.cmdSearch.Name = "cmdSearch";
            this.cmdSearch.Size = new System.Drawing.Size(28, 20);
            this.cmdSearch.TabIndex = 1;
            this.cmdSearch.Text = "...";
            this.cmdSearch.UseVisualStyleBackColor = true;
            this.cmdSearch.Click += new System.EventHandler(this.cmdSearch_Click);
            // 
            // txtChartPath
            // 
            this.txtChartPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChartPath.BackColor = System.Drawing.SystemColors.Window;
            this.txtChartPath.Location = new System.Drawing.Point(15, 40);
            this.txtChartPath.Name = "txtChartPath";
            this.txtChartPath.ReadOnly = true;
            this.txtChartPath.Size = new System.Drawing.Size(306, 20);
            this.txtChartPath.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmdCancel);
            this.panel1.Controls.Add(this.cmdOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 209);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(384, 30);
            this.panel1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tabControl1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(4);
            this.panel2.Size = new System.Drawing.Size(384, 209);
            this.panel2.TabIndex = 0;
            // 
            // frmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 239);
            this.ControlBox = false;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.MinimumSize = new System.Drawing.Size(400, 278);
            this.Name = "frmOptions";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Optionen";
            this.Load += new System.EventHandler(this.frmOptions_Load);
            this.pgLogin.ResumeLayout(false);
            this.pgLogin.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.pgGeneral.ResumeLayout(false);
            this.pgGeneral.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog dlgChartFolder;
        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.TabPage pgLogin;
        private System.Windows.Forms.TextBox txtPW2;
        private System.Windows.Forms.TextBox txtPW1;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.CheckBox chkSavePW;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage pgGeneral;
        private System.Windows.Forms.Button cmdSearch;
        private System.Windows.Forms.TextBox txtChartPath;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton radioButtonGAT24;
        private System.Windows.Forms.RadioButton radioButtonDFS;
        private System.Windows.Forms.Label labelDataSource;
        private System.Windows.Forms.Label labelChartsDir;
    }
}