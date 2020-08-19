using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 智能网络机器人控制系统1._0
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {
            MainForm frm = new MainForm();
            this.Hide();
            frm.Show();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            MainForm frm = new MainForm();
            this.Hide();
            frm.Show();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {

        }
    }
}
