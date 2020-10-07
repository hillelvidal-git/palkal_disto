using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WideField
{
    public partial class ProjectAlert : Form
    {
        public ProjectAlert(string name)
        {
            InitializeComponent();
            this.Text = "פרויקט נוכחי - " + name;
            this.label3.Text = name;
            this.label3.Location = new Point((this.Width - label3.Width) / 2, this.label3.Location.Y);
        }
    }
}
