using System;
using System.Windows.Forms;

namespace ChartButlerCS
{
    public partial class frmOptions : Form
    {
        public Settings pending_settings;

        public frmOptions()
        {
            InitializeComponent();

            pending_settings = (Settings)Settings.Default.Clone();

            txtChartPath.DataBindings.Add(new System.Windows.Forms.Binding("Text", pending_settings, "ChartFolder", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            txtChartPath.Text = pending_settings.ChartFolder;

            txtUser.DataBindings.Add(new System.Windows.Forms.Binding("Text", pending_settings, "ServerUsername", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            txtUser.Text = pending_settings.ServerUsername;
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            dlgChartFolder.SelectedPath = pending_settings.ChartFolder;
            if (dlgChartFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (dlgChartFolder.SelectedPath != pending_settings.ChartFolder)
                {
                    txtChartPath.Text = dlgChartFolder.SelectedPath;
                    pending_settings.ChartFolder = dlgChartFolder.SelectedPath;
                }
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (chkSavePW.Checked == false)
            {
                pending_settings.ServerPassword = null;
            }
            else if (txtPW1.Text == txtPW2.Text)
            {
                pending_settings.ServerPassword = txtPW1.Text;
            }
            try
            {
                pending_settings.Save();
                Settings.Default = pending_settings;
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

            radioButtonDFS.Checked = ("DFS" == pending_settings.DataSource);
            radioButtonGAT24.Checked = ("GAT24" == pending_settings.DataSource);

            if (pending_settings.ServerPassword == null || pending_settings.ServerPassword == "")
            {
                chkSavePW.Checked = false;
            }
            else
            {
                chkSavePW.Checked = true;
                txtPW1.Text = txtPW2.Text = pending_settings.ServerPassword;
            }

            txtPW1.Enabled = chkSavePW.Checked;
            txtPW2.Enabled = chkSavePW.Checked;
        }

        private void radioButtonDFS_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDFS.Checked)
                pending_settings.DataSource = "DFS";
        }

        private void radioButtonGAT24_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonGAT24.Checked)
                pending_settings.DataSource = "GAT24";
        }
    }//end Class
}//end NameSpace
