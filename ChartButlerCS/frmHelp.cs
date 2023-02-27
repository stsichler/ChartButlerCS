using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ChartButlerCS
{
    public partial class frmHelp : Form
    {
        public frmHelp()
        {
            InitializeComponent();

            richTextBox2.Text = Environment.NewLine + "ChartButlerCS Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() 
                + Environment.NewLine
                + richTextBox2.Text;
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void richTextBox2_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { frmChartDB.OpenFileInDefaultApp(e.LinkText); }
            catch (Exception) {}
        }
  
    }
}
