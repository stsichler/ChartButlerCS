namespace ChartButlerCS
{
    partial class dlgStatus
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
            this.txtProgress = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // txtProgress
            // 
            this.txtProgress.BackColor = System.Drawing.SystemColors.Window;
            this.txtProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtProgress.Location = new System.Drawing.Point(4, 4);
            this.txtProgress.Multiline = true;
            this.txtProgress.Name = "txtProgress";
            this.txtProgress.ReadOnly = true;
            this.txtProgress.Size = new System.Drawing.Size(476, 253);
            this.txtProgress.TabIndex = 0;
            this.txtProgress.WordWrap = false;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(4, 257);
            this.progressBar.Maximum = 1;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(476, 23);
            this.progressBar.Step = 1;
            this.progressBar.TabIndex = 1;
            // 
            // dlgStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 284);
            this.ControlBox = false;
            this.Controls.Add(this.txtProgress);
            this.Controls.Add(this.progressBar);
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "dlgStatus";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Karten-Aktualisierung";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.TextBox txtProgress;
        public System.Windows.Forms.ProgressBar progressBar;
    }
}