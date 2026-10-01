using System;
using System.IO;
using System.Windows.Forms;
using merge_cad.Function;

namespace merge_cad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region 文件选择事件

        /// <summary>
        /// 打开小板文件
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            SelectFile(label7, "小板");
        }

        /// <summary>
        /// 打开大板文件
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            SelectFile(label8, "大板");
        }

        /// <summary>
        /// 打开主板文件
        /// </summary>
        private void button9_Click(object sender, EventArgs e)
        {
            SelectFile(label10, "主板");
        }

        /// <summary>
        /// 保存文件路径选择
        /// </summary>
        private void button7_Click(object sender, EventArgs e)
        {
            SaveFile(label9);
        }

        #endregion

        #region 处理任务事件

        /// <summary>
        /// X 轴翻转
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            string subboard = label7.Text;
            if (!ValidateFile(subboard, "小板")) return;

            string newFile = GenerateOutputPath(subboard, "_X");
            string[] parameters = new string[] { subboard, newFile };
            ShowProgress(parameters, TaskType.OverturnX);
        }

        /// <summary>
        /// Y 轴翻转
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            string subboard = label7.Text;
            if (!ValidateFile(subboard, "小板")) return;

            string newFile = GenerateOutputPath(subboard, "_Y");
            string[] parameters = new string[] { subboard, newFile };
            ShowProgress(parameters, TaskType.OverturnY);
        }

        /// <summary>
        /// 90 度旋转
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            string subboard = label7.Text;
            if (!ValidateFile(subboard, "小板")) return;

            string newFile = GenerateOutputPath(subboard, "_R");
            string[] parameters = new string[] { subboard, newFile };
            ShowProgress(parameters, TaskType.Rotate90);
        }

        /// <summary>
        /// 坐标移动
        /// </summary>
        private void button8_Click(object sender, EventArgs e)
        {
            string subboard = label7.Text;
            string xoffset = textBox1.Text;
            string yoffset = textBox2.Text;
            string pinoffset = textBox3.Text;
            string postfix = textBox4.Text;

            if (!ValidateFile(subboard, "小板")) return;
            if (string.IsNullOrEmpty(xoffset))
            {
                MessageBox.Show("X 偏移量不能为空");
                return;
            }
            if (string.IsNullOrEmpty(yoffset))
            {
                MessageBox.Show("Y 偏移量不能为空");
                return;
            }
            if (string.IsNullOrEmpty(pinoffset))
            {
                MessageBox.Show("PIN 偏移量不能为空");
                return;
            }
            if (string.IsNullOrEmpty(postfix))
            {
                MessageBox.Show("后缀名不能为空");
                return;
            }

            string newFile = GenerateOutputPath(subboard, "_O");
            string[] parameters = new string[] { xoffset, yoffset, pinoffset, postfix, subboard, newFile };
            ShowProgress(parameters, TaskType.MoveCoordinate);
        }

        /// <summary>
        /// 文件合并
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            string subboard = label8.Text;
            string motherboard = label10.Text;
            string mergeBoard = label9.Text;

            if (!ValidateFile(subboard, "小板")) return;
            if (!ValidateFile(motherboard, "主板")) return;
            if (string.IsNullOrEmpty(mergeBoard))
            {
                MessageBox.Show("请指定输出文件路径");
                return;
            }

            string[] parameters = new string[] { subboard, motherboard, mergeBoard };
            ShowProgress(parameters, TaskType.Merge);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 选择文件
        /// </summary>
        private void SelectFile(Label label, string fileType)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CAD files (*.CAD)|*.CAD|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "选择" + fileType + "文件";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                label.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        private void SaveFile(Label label)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "CAD";
            saveFileDialog.Filter = "CAD files (*.CAD)|*.CAD";
            saveFileDialog.Title = "保存文件";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                label.Text = saveFileDialog.FileName;
            }
        }

        /// <summary>
        /// 验证文件路径
        /// </summary>
        private bool ValidateFile(string filePath, string fileName)
        {
            if (string.IsNullOrEmpty(filePath) || filePath == ":")
            {
                MessageBox.Show(fileName + "参数不能为空");
                return false;
            }
            if (!File.Exists(filePath))
            {
                MessageBox.Show(fileName + "文件不存在：" + filePath);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 生成输出文件路径
        /// </summary>
        private string GenerateOutputPath(string inputPath, string suffix)
        {
            string extension = Path.GetExtension(inputPath);
            string directory = Path.GetDirectoryName(inputPath);
            string filename = Path.GetFileNameWithoutExtension(inputPath);
            return directory + "\\" + filename + suffix + extension;
        }

        /// <summary>
        /// 显示进度窗口
        /// </summary>
        private void ShowProgress(string[] parameters, TaskType taskType)
        {
            Progress progressForm = new Progress(parameters, taskType);
            progressForm.Owner = this;
            progressForm.ShowDialog();
        }

        #endregion
    }
}