﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Uninstaller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //uninstall script-Browser
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);

            DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            foreach (FileInfo file in di.GetFiles())
            {
                //file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                //dir.Delete(true);
            }

            //delete uninstaller using cmd
            //Process.Start("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del " + Application.ExecutablePath);
            Application.Exit();
        }
    }
}