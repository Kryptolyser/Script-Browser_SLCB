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
    public partial class Uninst : Form
    {
        public Uninst()
        {
            InitializeComponent();
            textBox1.Text = "" + AppDomain.CurrentDomain.BaseDirectory;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Desktopverlinkung löschen
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            try
            {
                File.Delete(desktopPath + "\\Script-Browser.url");
            }
            catch {}

            //Startmenüverlinkung löschen
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Programs) + "\\Script-Browser"))
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Programs) + "\\Script-Browser" + "\\Script-Browser.url");
            }

            //Deinstallieren
            DirectoryInfo di = new DirectoryInfo("" + AppDomain.CurrentDomain.BaseDirectory);

            //Löschen aller Dateien im Ordner
            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Name != "Uninstaller.exe")
                {
                    try {
                        file.Delete();
                    }
                    catch { }
                }
            }
            //Löschen aller Ordner in diesem Ordner
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            //Löschen von Uninstaller.exe mit cmd

            //Process.Start("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del " + Application.ExecutablePath);
            //Process.Start("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del " + AppDomain.CurrentDomain.BaseDirectory);
            ProcessStartInfo deleteApp = new ProcessStartInfo();
            deleteApp.Arguments = "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + Application.ExecutablePath + "\"";
            deleteApp.WindowStyle = ProcessWindowStyle.Hidden;
            deleteApp.CreateNoWindow = true;
            deleteApp.FileName = "cmd.exe";
            Process.Start(deleteApp);
            Application.Exit();
        }
    }
}
