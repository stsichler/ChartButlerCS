using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ChartButlerCS
{
    public partial class CUpdateOverview : Form
    {
        private List<CChart> m_clist;

        public CUpdateOverview()
        {
            InitializeComponent();
        }
        ~CUpdateOverview() // Diese Methode ist eigentlich nicht nötig, verhindert aber einen Bug in Mono
        {
            sofortDruckenToolStripMenuItem.Dispose();
        }
        public CUpdateOverview(object clist)
        {
            InitializeComponent();
            if (((List<CChart>) clist).Count > 0)
            {
                m_clist = (List<CChart>)clist;
            }
        }

        private void CUpdateOverview_Load(object sender, EventArgs e)
        {
            if ( m_clist != null &&m_clist.Count > 0)
            {
                foreach (CChart crt in m_clist)
                {
                    vwCharts.Items.Add(crt.GetChartName(),crt.GetChartPath());
                }
            }
        }

        private void sofortDruckenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem itm = vwCharts.SelectedItems[0];
            System.Diagnostics.Process.Start(itm.ImageKey);
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmdCopyAll_Click(object sender, EventArgs e)
        {
            string targetPath;            
            if (dlgCopyAllCharts.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                targetPath = dlgCopyAllCharts.SelectedPath;
                foreach (CChart chrt in m_clist)
                {
                    System.IO.File.Copy(chrt.GetChartPath(), System.IO.Path.Combine(targetPath,chrt.GetChartName()), true);
                }
            }
        }

        private void cmdShowAll_Click(object sender, EventArgs e)
        {
            foreach (CChart chrt in m_clist)
            {
                System.Diagnostics.Process.Start(chrt.GetChartPath());
            }
        }

       

    }
}
