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

        private void frmOptions_Load(object sender, EventArgs e)
        {
        }
    }//end Class
}//end NameSpace
