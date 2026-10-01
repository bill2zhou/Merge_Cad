using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace merge_cad.Function
{
    public partial class Progress : Form
    {
        private readonly string[] _parameters;
        private readonly TaskType _taskType;
        private Stopwatch _stopwatch;
        private System.Windows.Forms.Timer _progressTimer;
        private int _currentProgress;
        private bool _taskCompleted;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Progress(string[] parameters, TaskType taskType)
        {
            InitializeComponent();
            
            _parameters = parameters;
            _taskType = taskType;
            _stopwatch = new Stopwatch();
            _currentProgress = 0;
            _taskCompleted = false;
            
            // 初始化 UI
            button1.Enabled = false;
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            
            // 设置任务描述
            label1.Text = GetTaskDescription();
            label2.Text = GetTaskTitle();
            
            // 启动进度动画
            StartProgressAnimation();
            
            // 启动任务
            StartTask();
        }

        /// <summary>
        /// 获取任务标题
        /// </summary>
        private string GetTaskTitle()
        {
            switch (_taskType)
            {
                case TaskType.OverturnX: return "正在 X 轴翻转处理...";
                case TaskType.OverturnY: return "正在 Y 轴翻转处理...";
                case TaskType.Rotate90: return "正在 90 度旋转处理...";
                case TaskType.MoveCoordinate: return "正在坐标移动处理...";
                case TaskType.Merge: return "正在文件合并处理...";
                default: return "正在处理...";
            }
        }

        /// <summary>
        /// 获取任务描述文本
        /// </summary>
        private string GetTaskDescription()
        {
            switch (_taskType)
            {
                case TaskType.OverturnX:
                case TaskType.OverturnY:
                case TaskType.Rotate90:
                    return "源文件：" + _parameters[0] + "\r\n新文件：" + _parameters[1];
                
                case TaskType.MoveCoordinate:
                    return "源文件：" + _parameters[4] + "\r\n新文件：" + _parameters[5];
                
                case TaskType.Merge:
                    return "文件 1：" + _parameters[0] + "\r\n文件 2：" + _parameters[1] + "\r\n输出：" + _parameters[2];
                
                default:
                    return "处理中...";
            }
        }

        /// <summary>
        /// 启动进度动画定时器
        /// </summary>
        private void StartProgressAnimation()
        {
            _progressTimer = new System.Windows.Forms.Timer();
            _progressTimer.Interval = 200; // 每 200ms 更新一次
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        /// <summary>
        /// 进度定时器事件
        /// </summary>
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (_taskCompleted)
            {
                // 任务已完成，进度条到 100%
                if (_currentProgress < 100)
                {
                    _currentProgress += 5;
                    if (_currentProgress > 100) _currentProgress = 100;
                    progressBar1.Value = _currentProgress;
                }
                else
                {
                    _progressTimer.Stop();
                }
            }
            else
            {
                // 任务进行中，进度条缓慢增加到 90%
                if (_currentProgress < 90)
                {
                    _currentProgress += 2;
                    if (_currentProgress > 90) _currentProgress = 90;
                    progressBar1.Value = _currentProgress;
                    
                    // 更新状态文本
                    int dots = (_currentProgress / 10) % 4;
                    string loading = "." + new string('.', dots);
                    label2.Text = GetTaskTitle() + loading;
                }
            }
        }

        /// <summary>
        /// 启动处理任务
        /// </summary>
        private void StartTask()
        {
            // 创建任务（带完成回调）
            ProcessTask task = new ProcessTask(_taskType, _parameters, OnTaskComplete);
            
            // 启动工作线程
            Thread workThread = new Thread(new ThreadStart(task.Execute));
            workThread.IsBackground = true;
            workThread.Start();
            
            // 启动计时
            _stopwatch.Start();
        }

        /// <summary>
        /// 任务完成回调（可能在工作线程中调用）
        /// </summary>
        private void OnTaskComplete(bool success, string message)
        {
            // 跨线程更新 UI
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool, string>(OnTaskComplete), success, message);
                return;
            }
            
            _taskCompleted = true;
            _stopwatch.Stop();
            
            // 强制进度条到 100%
            _currentProgress = 100;
            progressBar1.Value = 100;
            
            // 更新状态文本
            if (success)
            {
                TimeSpan elapsed = _stopwatch.Elapsed;
                label2.Text = "✓ 处理完成！耗时：" + elapsed.TotalSeconds.ToString("F2") + " 秒";
                label2.ForeColor = System.Drawing.Color.FromArgb(0, 180, 80);
            }
            else
            {
                label2.Text = "✗ 处理失败：" + message;
                label2.ForeColor = System.Drawing.Color.FromArgb(200, 50, 50);
            }
            
            // 启用完成按钮
            button1.Enabled = true;
        }

        /// <summary>
        /// 完成按钮点击
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (_progressTimer != null)
            {
                _progressTimer.Stop();
                _progressTimer.Dispose();
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 窗体关闭时清理资源
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_stopwatch != null && _stopwatch.IsRunning)
            {
                _stopwatch.Stop();
            }
            if (_progressTimer != null)
            {
                _progressTimer.Stop();
                _progressTimer.Dispose();
            }
        }
    }
}