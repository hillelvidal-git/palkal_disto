using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LogForm
{
    public partial class LogForm : Form
    {
        public LogForm(List<string> lines)
        {
            InitializeComponent();
            textBox1.Lines = lines.ToArray();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            textBox1.Font = new System.Drawing.Font(FontFamily.GenericSansSerif, (float)trackBar1.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
