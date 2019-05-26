using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StopWatch
{
    public partial class RunningDialog : Form
    {
        public RunningDialog()
        {
            InitializeComponent();

            cbSnooze.DisplayMember = "Text";
            cbSnooze.ValueMember = "Value";

            var items = new[] {
                new { Text = "10m", Value = 10 },
                new { Text = "30m", Value = 30 },
                new { Text = "1h", Value = 60 }
            };

            cbSnooze.DataSource = items;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void IdleDialog_Shown(object sender, EventArgs e)
        {
            cbSnooze.SelectedIndex = 0;
        }

        public int GetSnooze()
        {
            return ((int)cbSnooze.SelectedValue)*60*1000;
        }
    }
}
