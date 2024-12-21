using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using merge_cad.Function;
using Microsoft.Win32;


namespace merge_cad
{
    public partial class Form1 : Form
    {





        public Form1()
        {
            InitializeComponent();


        }







        /*打开小板*/
        private void button1_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            var filePath = string.Empty;

            openFileDialog.Filter = "CAD files (*.CAD)|*.CAD|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                this.label7.Text = filePath;
            }

        }

        /*打开大板*/
        private void button2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            var filePath = string.Empty;
            openFileDialog.Filter = "CAD files (*.CAD)|*.CAD|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                this.label8.Text = filePath;
            }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            var filePath = string.Empty;
            openFileDialog.Filter = "CAD files (*.CAD)|*.CAD|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                this.label10.Text = filePath;
            }

        }







        /* 文件保存*/
        private void button7_Click(object sender, EventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
        //    saveFileDialog.FileName = "me"; // Default file name
            saveFileDialog.DefaultExt = ".CAD"; // Default file extension
            saveFileDialog.Filter = "CAD files (*.CAD)|*.CAD"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = saveFileDialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {              // Save document
                label9.Text = saveFileDialog.FileName;
            }


        }

        /*x轴翻转*/
        private void button4_Click(object sender, EventArgs e)
        {
            string bak_file = "";
            string subboard = this.label7.Text;

            

          
            if (subboard == ":")
            {
                MessageBox.Show("小板参数不能为空");

            }
            else
            {
                string extension = Path.GetExtension(subboard);
                string paths= Path.GetDirectoryName(subboard);
                string filename = Path.GetFileNameWithoutExtension(subboard);
                bak_file = paths+"\\"+filename + "_bakx"+ extension;
                if (File.Exists(bak_file))
                {
                    string tm = DateTime.Now.ToString("HHmmss");
                    File.Move(bak_file,tm+ bak_file);
                  
                }
                File.Move(subboard, bak_file);


                string[] parameter = { bak_file, subboard };
                var ownedWindow = new Progress(parameter, "x");
                ownedWindow.Owner = this;
                if (ownedWindow.ShowDialog() == DialogResult.OK)
                {

                }

            }

        }

        /*y轴翻转*/
        private void button5_Click(object sender, EventArgs e)
        {
            string bak_file = "";
            string subboard = this.label7.Text;

            if (subboard == ":")
            {
                MessageBox.Show("小板参数不能为空");
            }
            else
            {
                string extension = Path.GetExtension(subboard);
                string paths = Path.GetDirectoryName(subboard);
                string filename = Path.GetFileNameWithoutExtension(subboard);
                bak_file = paths + "\\" + filename + "_baky" + extension;
                if (File.Exists(bak_file))
                {
                    string tm = DateTime.Now.ToString("HHmmss");
                    File.Move(bak_file, tm + bak_file);
                }
                File.Move(subboard, bak_file);
                string[] parameter = { bak_file, subboard };
                var ownedWindow = new Progress(parameter, "y");
                ownedWindow.Owner = this;
                if (ownedWindow.ShowDialog() == DialogResult.OK)
                {

                }
            }
        }

        /*旋转*/
        private void button6_Click(object sender, EventArgs e)
        {
            string bak_file = "";
            string subboard = this.label7.Text;

            if (subboard == ":")
            {
                MessageBox.Show("小板参数不能为空");

            }
            else
            {
                string extension = Path.GetExtension(subboard);
                string paths = Path.GetDirectoryName(subboard);
                string filename = Path.GetFileNameWithoutExtension(subboard);
                bak_file = paths + "\\" + filename + "_bakr" + extension;
                if (File.Exists(bak_file))
                {
                    string tm = DateTime.Now.ToString("HHmmss");
                    File.Move(bak_file, tm + bak_file);
                }
                File.Move(subboard, bak_file);
                string[] parameter = { bak_file, subboard };
                var ownedWindow = new Progress(parameter ,"r");
                ownedWindow.Owner = this;
                if (ownedWindow.ShowDialog() == DialogResult.OK)
                {

                }
            }

        }
        /*移动*/
        private void button8_Click(object sender, EventArgs e)
        {
            string bak_file = "";
            string xoffset = this.textBox1.Text;
            string yoffset = this.textBox2.Text;
            string pinoffset = this.textBox3.Text;
            string postfix = this.textBox4.Text;
            string subboard = this.label7.Text;

            
            if (subboard == ":" || string.IsNullOrEmpty(xoffset) || string.IsNullOrEmpty(yoffset) || string.IsNullOrEmpty(pinoffset) || string.IsNullOrEmpty(postfix) )
            {
                MessageBox.Show("参数不能为空");

            }
            else
            {
                string extension = Path.GetExtension(subboard);
                string paths = Path.GetDirectoryName(subboard);
                string filename = Path.GetFileNameWithoutExtension(subboard);
                bak_file = paths + "\\" + filename + "_bako" + extension;
                if (File.Exists(bak_file))
                {
                    string tm = DateTime.Now.ToString("HHmmss");
                    File.Move(bak_file, tm + bak_file);
                }
                File.Move(subboard, bak_file);

                string[] parameter = { xoffset, yoffset, pinoffset, postfix, bak_file, subboard };

                var ownedWindow = new Progress(parameter,"o");
                ownedWindow.Owner = this;
                if (ownedWindow.ShowDialog() == DialogResult.OK)
                {

                }

            }
        }

        /*合并*/
        private void button3_Click(object sender, EventArgs e)
        {

            string subboard = this.label10.Text;
            string motherboard = this.label8.Text;
            string margeboard = this.label9.Text;


            if ((subboard == ":") || (motherboard == ":") || (margeboard == ":"))
            {
                MessageBox.Show("参数不能为空");

            }
            else
            {


                string[] parameter = {  subboard, motherboard, margeboard };

                var ownedWindow = new Progress(parameter, "m");
                ownedWindow.Owner = this;
                if (ownedWindow.ShowDialog() == DialogResult.OK)
                {

                }


            }


        }


    }
}
