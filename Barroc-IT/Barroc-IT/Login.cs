﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Barroc_IT
{
    public partial class Frm_Login : Form
    {
        public Frm_Login()
        {
            InitializeComponent();            
        }

        private void bt_Login_Click(object sender, EventArgs e)
        {
            frm_Development developmentfrom = new frm_Development();
            developmentfrom.Show();
            
            
        }
    }
}