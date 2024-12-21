using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace merge_cad.Function
{
    internal class Mproces
    {
        private string xoffset;
        private string yoffset;
        private string pinoffset;

        private string postfix;
        private string subboard;
        private string tmp_file;



        // The constructor obtains the state information.
        public Mproces(string text1, string text2, string text3, string text4, string text5, string text6)
        {
            xoffset = text1;
            yoffset = text2;
            pinoffset = text3;
            postfix = text4;
            subboard = text5;
            tmp_file = text6;

        }

        // The thread procedure performs the task, such as formatting
        // and printing a document.
        public void Threado()
        {

            Move_coordinate.move(xoffset, yoffset, pinoffset, postfix, subboard, tmp_file);

       //     Merge.Merge_cad(tmp_file, motherboard, margeboard);

            //    MessageBox.Show(file2 + "X轴翻转完成.\r\n原始文件为：" + file1);
        }
    }
}
