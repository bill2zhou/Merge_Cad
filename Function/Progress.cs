using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace merge_cad.Function
{
    public partial class Progress : Form
    {
        public string[] parameter=new string[10];
        public string zhou;
        public Progress(string[] parameter,string zhou)
        {
            InitializeComponent();
            button1.Enabled = false;
            proces tws = new proces(parameter); ;
            if (zhou == "x")
            {
                Thread t = new Thread(new ThreadStart(tws.Threadx));
                t.Start();
                new Thread(Runproce).Start(t);
                label1.Text = "源文件备份为"+ parameter[0] + "\r\n;新文件为"+ parameter[1] + ": ";
            }
            if (zhou == "y")
            {
                Thread t = new Thread(new ThreadStart(tws.Thready));
                t.Start();
                new Thread(Runproce).Start(t);
                label1.Text = "源文件备份为" + parameter[0] + "\r\n;新文件为" + parameter[1] + ": ";
            }

            if (zhou == "r")
            {
                Thread t = new Thread(new ThreadStart(tws.Threadr));
                t.Start();
                new Thread(Runproce).Start(t);
                label1.Text = "源文件备份为" + parameter[0] + "\r\n;新文件为" + parameter[1] + ": ";
            }


            if (zhou == "o")
            {
                Thread t = new Thread(new ThreadStart(tws.Threado));
                t.Start();
                new Thread(Runproce).Start(t);
                label1.Text = "源文件备份为" + parameter[4] + "\r\n;新文件为" + parameter[5] + ": ";
            }


            if (zhou == "m")
            {
                Thread t = new Thread(new ThreadStart(tws.Threadm));
                t.Start();
                new Thread(Runproce).Start(t);
                label1.Text = "源文件备份为" + parameter[0] +":"+ parameter[1]+ "\r\n;新文件为" + parameter[2] + ":";
            }



        }


        private void Runproce(object stats)
    {
           Thread t = (Thread)stats;

            Console.WriteLine(t.ThreadState);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            progressBar1.Value = 0;
            progressBar1.Style = ProgressBarStyle.Marquee; 
            progressBar1.Maximum = 100;

            while (t.ThreadState != System.Threading.ThreadState.Stopped)
            {
           
            }
            progressBar1.Style = ProgressBarStyle.Continuous;
            stopwatch.Stop();
            label1.Text = label1.Text+ "处理完成。";
           button1.Enabled = true;
    }


        private void button1_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
        }


    }
}
