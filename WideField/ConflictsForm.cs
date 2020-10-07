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
    public partial class ConflictsForm : Form
    {
        public ConflictsForm(string[][] points)
        {
            InitializeComponent();
            object[] newRow;
            foreach (string[] point in points)
            {
                newRow = new object[] { point[0], point[1], point[2], point[3], point[4], "שנה שם", "100" + point[4] };
                this.dataGridView1.Rows.Add(newRow);
            }
        }
    }
}
