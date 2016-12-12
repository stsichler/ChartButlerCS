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

            txtChartPath.DataBindings.Add(new System.Windows.Forms.Binding("Text", ChartButlerCS.Settings.Default, "ChartFolder", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            txtChartPath.Text = ChartButlerCS.Settings.Default.ChartFolder;

            txtUser.DataBindings.Add(new System.Windows.Forms.Binding("Text", ChartButlerCS.Settings.Default, "ServerUsername", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            txtUser.Text = ChartButlerCS.Settings.Default.ServerUsername;

            txtPW1.Enabled = chkSavePW.Checked;
            txtPW2.Enabled = chkSavePW.Checked;
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            dlgChartFolder.SelectedPath = ChartButlerCS.Settings.Default.ChartFolder;
            if (dlgChartFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (dlgChartFolder.SelectedPath != ChartButlerCS.Settings.Default.ChartFolder)
                {
                    txtChartPath.Text = dlgChartFolder.SelectedPath;
                    ChartButlerCS.Settings.Default.ChartFolder = dlgChartFolder.SelectedPath;
                    m_ChartsPathChanged = true;
                }
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (chkSavePW.Checked == false)
            {
                Settings.Default.ServerPassword = null;
            }
            else if (txtPW1.Text == txtPW2.Text)
            {
                Settings.Default.ServerPassword = txtPW1.Text;
            }
            try
            {
                Settings.Default.Save();
                DialogResult = DialogResult.OK;
            }
            catch (Exception)
            {
                MessageBox.Show(Parent, "Die Einstellungen konnten nicht gespeichert werden!","ChartButler");
                DialogResult = DialogResult.Abort;
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Settings.Default.Reload();
            DialogResult = DialogResult.Cancel;
        }

        private void chkSavePW_CheckedChanged(object sender, EventArgs e)
        {
            txtPW1.Enabled = chkSavePW.Checked;
            txtPW2.Enabled = chkSavePW.Checked;
            if (!chkSavePW.Checked)
                txtPW1.Text = txtPW2.Text= "";
        }

        private void txtPW2_Leave(object sender, EventArgs e)
        {
            if (txtPW1.Text != txtPW2.Text)
            {
                MessageBox.Show(this,"Die Passwörter stimmen nicht überein!", "Fehler", MessageBoxButtons.OK);
                txtPW1.Focus();
            }
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            if (Settings.Default.ServerPassword == null || Settings.Default.ServerPassword == "")
            {
                chkSavePW.Checked = false;
            }
            else
            {
                chkSavePW.Checked = true;
                txtPW1.Text = txtPW2.Text = Settings.Default.ServerPassword;
            }
        }
    }//end Class
}//end NameSpace
