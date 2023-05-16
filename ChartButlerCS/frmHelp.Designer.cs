namespace ChartButlerCS
{
    partial class frmHelp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHelp));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.pgHelp = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.pgInfo = new System.Windows.Forms.TabPage();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.cmdClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.pgHelp.SuspendLayout();
            this.pgInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.cmdClose);
            this.splitContainer1.Size = new System.Drawing.Size(584, 562);
            this.splitContainer1.SplitterDistance = 519;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.pgHelp);
            this.tabControl1.Controls.Add(this.pgInfo);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(584, 519);
            this.tabControl1.TabIndex = 0;
            // 
            // pgHelp
            // 
            this.pgHelp.Controls.Add(this.richTextBox1);
            this.pgHelp.Location = new System.Drawing.Point(4, 22);
            this.pgHelp.Name = "pgHelp";
            this.pgHelp.Padding = new System.Windows.Forms.Padding(3);
            this.pgHelp.Size = new System.Drawing.Size(576, 493);
            this.pgHelp.TabIndex = 0;
            this.pgHelp.Text = "Bedienungshinweise";
            this.pgHelp.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(3, 3);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(570, 487);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // pgInfo
            // 
            this.pgInfo.Controls.Add(this.richTextBox2);
            this.pgInfo.Location = new System.Drawing.Point(4, 22);
            this.pgInfo.Name = "pgInfo";
            this.pgInfo.Padding = new System.Windows.Forms.Padding(3);
            this.pgInfo.Size = new System.Drawing.Size(576, 493);
            this.pgInfo.TabIndex = 1;
            this.pgInfo.Text = "Info";
            this.pgInfo.UseVisualStyleBackColor = true;
            // 
            // richTextBox2
            // 
            this.richTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox2.Location = new System.Drawing.Point(3, 3);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ReadOnly = true;
            this.richTextBox2.Size = new System.Drawing.Size(570, 487);
            this.richTextBox2.TabIndex = 0;
            this.richTextBox2.Text = resources.GetString("richTextBox2.Text");
            this.richTextBox2.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox2_LinkClicked);
            // 
            // cmdClose
            // 
            this.cmdClose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdClose.Location = new System.Drawing.Point(0, 0);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(584, 39);
            this.cmdClose.TabIndex = 0;
            this.cmdClose.Text = "Schließen";
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Click += new System.EventHandler(this.cmdClose_Click);
            // 
            // frmHelp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 562);
            this.Controls.Add(this.splitContainer1);
            this.Name = "frmHelp";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Hilfe / Information über ChartButler";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.pgHelp.ResumeLayout(false);
            this.pgInfo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage pgHelp;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TabPage pgInfo;
        private System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.RichTextBox richTextBox2;
    }
}