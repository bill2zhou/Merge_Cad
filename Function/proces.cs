using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace merge_cad.Function
{
    internal class proces
    {
        private string[] p;


        // The constructor obtains the state information.
        public proces(string[] pp)
        {
            p = pp;
        }

        // The thread procedure performs the task, such as formatting
        // and printing a document.
        public void Threadx()
        {

            Overturn_x.Overturn(p[0], p[1]);

        //    MessageBox.Show(file2 + "X轴翻转完成.\r\n原始文件为：" + file1);
        }

        public void Thready()
        {
            Overturn_y.Overturn(p[0], p[1]);
        }

        public void Threadr()
        {
            Rotate.Rotate_90(p[0], p[1]);
        }


        public void Threado()
        {

            Move_coordinate.move(p[0], p[1], p[2], p[3], p[4], p[5]);
        }

        public void Threadm()
        {
            Merge.Merge_cad(p[0], p[1], p[2]);
        }



    }



// The ThreadWithState class contains the information needed for
// a task, and the method that executes the task.
//


}
