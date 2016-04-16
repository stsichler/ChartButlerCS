using System;
using System.Windows.Forms;

namespace ChartButlerCS
{
    public partial class frmOptions : Form
    {
        public bool m_ChartsPathChanged = false;

        public frmOptions()
        {
            InitializeComponent();
            txtPW1.Enabled = chkSavePW.Checked;
            txtPW2.Enabled = chkSavePW.Checked;
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            dlgChartFolder.SelectedPath = ChartButlerCS.Properties.Settings.Default.ChartFolder;
            if (dlgChartFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (dlgChartFolder.SelectedPath != ChartButlerCS.Properties.Settings.Default.ChartFolder)
                {
                    txtChartPath.Text = dlgChartFolder.SelectedPath;
                    ChartButlerCS.Properties.Settings.Default.ChartFolder = dlgChartFolder.SelectedPath;
                    m_ChartsPathChanged = true;
                }
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (chkSavePW.Checked == false)
            {
                Properties.Settings.Default.ServerPassword = null;
            }
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();
            DialogResult = DialogResult.Cancel;
        }

        private void chkSavePW_CheckedChanged(object sender, EventArgs e)
        {
            txtPW1.Enabled = chkSavePW.Checked;
            txtPW2.Enabled = chkSavePW.Checked;            
        }

        private void txtPW2_Leave(object sender, EventArgs e)
        {
            if (txtPW1.Text == txtPW2.Text)
            {
                Properties.Settings.Default.ServerPassword = txtPW1.Text;
            }
            else
            {
                MessageBox.Show(this,"Die Passwörter stimmen nicht überein!", "Fehler", MessageBoxButtons.OK);
                txtPW1.Focus();
            }
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ServerPassword == null)
            {
                chkSavePW.Checked = false;
            }
            else
            {
                chkSavePW.Checked = true;
                txtPW1.Text = txtPW2.Text = Properties.Settings.Default.ServerPassword;
            }
        }
    }//end Class
}//end NameSpace
