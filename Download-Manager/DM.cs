﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Download_Manager
{
    public partial class DM : Form
    {
        private bool mouseDown;
        private Point lastLocation;

        public DM()
        {
            InitializeComponent();
        }

        //close window by clicking on "X"
        private void labelX_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //minimize form by clicking on "_"
        private void labelMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        //moving of form
        //saving position if mouse button down
        private void tableLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }
        //calculating new position while mouse moves
        private void tableLayoutPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }
        //stop moving when mouse button up
        private void tableLayoutPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        //enable button "next" when user scrolled to end of richTextBoxTerms
        private void richTextBoxTerms_VScroll(object sender, EventArgs e)
        {
            if (richTextBoxTerms.ReachedBottom())
            {
                checkBoxAgree.Enabled = true;
            }
        }

        //checkbox "I agree." -> activation of button "next"
        private void checkBoxAgree_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAgree.Checked == true)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        //button "cancel" -> dialogue 
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure?", "", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        //button "next" -> show installation panel
        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            Pages.Installation x = new Pages.Installation();
            this.Controls.Add(x);
            x.Size = new Size(645, 370);
            x.Location = new Point(0,52);
        }

        private void linkLabelDP_MouseClick(object sender, MouseEventArgs e)
        {
            Process.Start("http://www.digital-programming.de");
        }
    }
}
