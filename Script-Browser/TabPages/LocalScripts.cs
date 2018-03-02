﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SaveManager;
using System.Threading;
using System.Diagnostics;
using MetroFramework;
using Script_Browser.Design;
using Newtonsoft.Json.Linq;

namespace Script_Browser.TabPages
{
    public partial class LocalScripts : UserControl
    {
        public Main form;
        string currentScriptID = "";

        public LocalScripts()
        {
            InitializeComponent();
            contextMenuStrip1.Renderer = new ArrowRenderer();
        }

        private void LocalScripts_Load(object sender, EventArgs e)
        {
            fileSystemWatcher1.Path = Main.sf.streamlabsPath; //TODO: check path
            UpdateList(Main.sf.streamlabsPath);
            //UpdateList(@"D:\Streamlabs Chatbot\");
            //UpdateList(@"C:\Users\18diyann\Desktop\Test Ordner\");
        }

        public void UpdateList(string path)
        {
            try
            {
                Main.sf.currentInstalled.Clear();
                string[] dirs = Directory.GetDirectories(path + @"Services\Scripts\");

                dataGridView1.Rows.Clear();
                dataGridView2.Rows.Clear();
                foreach (string dir in dirs)
                {
                    string[] files = Directory.GetFiles(dir);
                    string scriptFile = "UNDEF";
                    string type = "UNDEF";
                    string name = "UNDEF";
                    string description = "UNDEF";
                    string author = "UNDEF";
                    string version = "UNDEF";
                    int id = 0;

                    foreach (string file in files)
                    {
                        if (file.Split('\\')[file.Split('\\').Length - 1].Contains("_StreamlabsSystem.py") || file.Split('\\')[file.Split('\\').Length - 1].Contains("_AnkhBotSystem.py"))
                            type = "Command";
                        else if (file.Split('\\')[file.Split('\\').Length - 1].Contains("_StreamlabsParameter.py") || file.Split('\\')[file.Split('\\').Length - 1].Contains("_AnkhBotParameter.py"))
                            type = "Parameter";

                        if (type != "UNDEF")
                        {
                            scriptFile = file;

                            string[] lines = File.ReadAllLines(file);
                            foreach (string line in lines)
                            {
                                if (line.ToLower().Contains("scriptname") && name == "UNDEF")
                                    name = GetLineItem(line);
                                else if (line.ToLower().Contains("description") && description == "UNDEF")
                                    description = GetLineItem(line);
                                else if (line.ToLower().Contains("creator") && author == "UNDEF")
                                    author = GetLineItem(line);
                                else if (line.ToLower().Contains("version") && version == "UNDEF")
                                    version = GetLineItem(line);
                                else if (line.ToLower().Contains("scriptbrowserid"))
                                    id = Int32.Parse(GetLineItem(line));
                            }

                            break;
                        }
                    }

                    if (scriptFile != "UNDEF")
                    {
                        if (id == 0)
                            dataGridView1.Rows.Add(name, description, type, version, author, scriptFile);
                        else
                        {
                            dataGridView2.Rows.Add(name, description, type, version, author, scriptFile, id);
                            Main.sf.currentInstalled.Add(new KeyValuePair<int, string>(id, Path.GetDirectoryName(scriptFile)));
                        }
                    }
                }

                if (dataGridView1.RowCount == 0)
                {
                    label1.Visible = false;
                    dataGridView1.Visible = false;
                    tableLayoutPanel1.RowStyles[1].SizeType = SizeType.AutoSize;
                }
                else
                {
                    label1.Visible = true;
                    dataGridView1.Visible = true;
                    tableLayoutPanel1.RowStyles[1].SizeType = SizeType.Percent;
                    tableLayoutPanel1.RowStyles[1].Height = 50f;
                }

                if (dataGridView2.RowCount == 0)
                {
                    label2.Visible = false;
                    dataGridView2.Visible = false;
                    tableLayoutPanel1.RowStyles[3].SizeType = SizeType.AutoSize;
                }
                else
                {
                    label2.Visible = true;
                    dataGridView2.Visible = true;
                    tableLayoutPanel1.RowStyles[3].SizeType = SizeType.Percent;
                    tableLayoutPanel1.RowStyles[3].Height = 50;
                }

                label4.Visible = dataGridView1.RowCount == 0 && dataGridView2.RowCount == 0;
                dataGridView1.ClearSelection();
                dataGridView2.ClearSelection();
            }
            catch { }
        }

        public static string GetLineItem(string line)
        {
            try
            {
                if (line.IndexOf('"') != -1)
                {
                    string result = line.Substring(line.IndexOf('"') + 1);
                    result = result.Substring(0, result.IndexOf('"'));
                    return result;
                }
            }
            catch { }
            return "UNDEF";
        }

        //Change colors for the rows to get pattern
        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                DataGridView dgv = sender as DataGridView;
                if (e.RowIndex % 2 == 0)
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(22, 36, 45);
                    dgv.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(32, 53, 66);
                }
                else
                {
                    dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(18, 31, 39);
                    dgv.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(34, 55, 69);
                }
            }
            catch { }
        }

        //Only row-selection when entering a cell
        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridView dgv = sender as DataGridView;
                    dgv.ClearSelection();
                    dgv.Rows[e.RowIndex].Selected = true;

                    nameToolStripMenuItem.Text = dgv.Rows[e.RowIndex].Cells[0].Value.ToString();
                    uploadUpdateToolStripMenuItem.Visible = false;
                    uploadToolStripMenuItem.Visible = dgv.Tag.ToString().Contains("upload");
                    if (dgv.Tag.ToString().Contains("update"))
                    {
                        reportToolStripMenuItem.Visible = !Networking.scripts.Contains(dgv.Rows[e.RowIndex].Cells[6].Value.ToString());
                        checkForUpdatesToolStripMenuItem.Visible = !Networking.scripts.Contains(dgv.Rows[e.RowIndex].Cells[6].Value.ToString());
                        uploadUpdateToolStripMenuItem.Visible = Networking.scripts.Contains(dgv.Rows[e.RowIndex].Cells[6].Value.ToString());
                    }
                    else
                    {
                        reportToolStripMenuItem.Visible = false;
                        checkForUpdatesToolStripMenuItem.Visible = false;
                        uploadUpdateToolStripMenuItem.Visible = false;
                    }
                }
                catch { }
            }
            else
                (sender as DataGridView).ClearSelection();
        }

        private void dataGridView1_MouseLeave(object sender, EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            dgv.ClearSelection();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridView dgv = sender as DataGridView;
                    dgv.Rows[e.RowIndex].Selected = true;

                    button5.Visible = false;
                    button1.Visible = dgv.Tag.ToString().Contains("upload");
                    if (dgv.Tag.ToString().Contains("update"))
                    {
                        button3.Visible = !Networking.scripts.Contains(dgv.Rows[e.RowIndex].Cells[6].Value.ToString());
                        button4.Visible = !Networking.scripts.Contains(dgv.Rows[e.RowIndex].Cells[6].Value.ToString());
                        button5.Visible = Networking.scripts.Contains(dgv.Rows[e.RowIndex].Cells[6].Value.ToString());
                    }
                    else
                    {
                        button3.Visible = false;
                        button4.Visible = false;
                        button5.Visible = false;
                    }

                    label3.Text = dgv.Rows[e.RowIndex].Cells[0].Value.ToString();

                    tableLayoutPanel2.Visible = true;
                    tableLayoutPanel2.Tag = dgv.Rows[e.RowIndex].Cells[5].Value.ToString();

                    if (dgv.ColumnCount == 7)
                        currentScriptID = dgv.Rows[e.RowIndex].Cells[6].Value.ToString();
                }
                catch { }
            }
            else
                tableLayoutPanel2.Visible = false;
        }

        private void hideFooter_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv.HitTest(e.X, e.Y).Type == DataGridViewHitTestType.None)
                    tableLayoutPanel2.Visible = false;
            }
            catch { tableLayoutPanel2.Visible = false; }
        }

        //Upload script
        private void button1_Click(object sender, EventArgs e)
        {
            form.Opacity = 0.5;
            new UploadScript(tableLayoutPanel2.Tag.ToString(), null).ShowDialog();
            form.Opacity = 1;
            UpdateList(Main.sf.streamlabsPath);
        }

        //Upload update
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                JObject info = JObject.Parse(Networking.GetUploadUpdateInfo(form, currentScriptID));
                form.Opacity = 0.5;
                new UploadScript(tableLayoutPanel2.Tag.ToString(), info).ShowDialog();
                form.Opacity = 1;
                UpdateList(Main.sf.streamlabsPath);
            }
            catch { }
        }

        //Open Script Path
        private void button6_Click(object sender, EventArgs e)
        {
            try { Process.Start(Path.GetDirectoryName(tableLayoutPanel2.Tag.ToString())); } catch { }
        }

        //UpdateList on file changes
        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            UpdateList(Main.sf.streamlabsPath);
        }

        private void fileSystemWatcher1_Renamed(object sender, RenamedEventArgs e)
        {
            UpdateList(Main.sf.streamlabsPath);
        }

        //Uninstall script
        public static void UninstallScript(Main form, string path, string name)
        {
            try
            {
                DialogResult dr = MetroMessageBox.Show(form, "Do you really want to remove the script \"" + name + "\" from your PC?\nThis deletes all your save files & settings to!", "Delete Script", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3, 150);

                if (dr == DialogResult.Yes)
                    Directory.Delete(Path.GetDirectoryName(path), true);
            }
            catch (Exception ex) { MetroMessageBox.Show(form, "there was an unexpected exception during the process:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2, 150); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UninstallScript(form, tableLayoutPanel2.Tag.ToString(), label3.Text);
            tableLayoutPanel2.Visible = false;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                DataGridView dgv = (DataGridView)contextMenuStrip1.SourceControl;
                if (dgv.SelectedRows.Count < 1)
                    e.Cancel = true;
                else
                    dataGridView1_CellClick(dgv, new DataGridViewCellEventArgs(0, dgv.SelectedRows[0].Index));
            }
            catch { e.Cancel = true; }
        }
    }
}
