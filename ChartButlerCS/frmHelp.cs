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

            richTextBox2.Text = Environment.NewLine + "ChartButler Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() 
                + Environment.NewLine + Environment.NewLine
                + richTextBox2.Text;
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

  
    }
}
