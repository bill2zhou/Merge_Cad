using System;
using System.Threading;
using System.Windows.Forms;

namespace merge_cad.Function
{
    /// <summary>
    /// CAD 文件处理任务类型枚举
    /// </summary>
    public enum TaskType
    {
        OverturnX,      // X 轴翻转
        OverturnY,      // Y 轴翻转
        Rotate90,       // 90 度旋转
        MoveCoordinate, // 坐标移动
        Merge           // 文件合并
    }

    /// <summary>
    /// CAD 文件处理任务类（线程安全）
    /// </summary>
    internal class ProcessTask
    {
        // 任务参数
        private readonly TaskType _taskType;
        private readonly string[] _parameters;
        private readonly Action<bool, string> _onComplete;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskType">任务类型</param>
        /// <param name="parameters">任务参数</param>
        /// <param name="onComplete">完成回调（成功/失败，消息）</param>
        public ProcessTask(TaskType taskType, string[] parameters, Action<bool, string> onComplete = null)
        {
            _taskType = taskType;
            _parameters = parameters;
            _onComplete = onComplete;
        }

        /// <summary>
        /// 线程入口方法
        /// </summary>
        public void Execute()
        {
            try
            {
                switch (_taskType)
                {
                    case TaskType.OverturnX:
                        ExecuteOverturnX();
                        break;
                    case TaskType.OverturnY:
                        ExecuteOverturnY();
                        break;
                    case TaskType.Rotate90:
                        ExecuteRotate90();
                        break;
                    case TaskType.MoveCoordinate:
                        ExecuteMoveCoordinate();
                        break;
                    case TaskType.Merge:
                        ExecuteMerge();
                        break;
                    default:
                        throw new ArgumentException("未知任务类型：" + _taskType);
                }

                // 触发成功回调
                if (_onComplete != null)
                {
                    _onComplete(true, "任务完成：" + _taskType);
                }
            }
            catch (Exception ex)
            {
                // 触发失败回调
                if (_onComplete != null)
                {
                    _onComplete(false, "任务失败：" + ex.Message);
                }
                else
                {
                    MessageBox.Show("处理出错：" + ex.Message);
                }
            }
        }

        #region 任务执行方法

        /// <summary>
        /// 执行 X 轴翻转
        /// </summary>
        private void ExecuteOverturnX()
        {
            ValidateParameters(2);
            Overturn_x.Overturn(_parameters[0], _parameters[1]);
        }

        /// <summary>
        /// 执行 Y 轴翻转
        /// </summary>
        private void ExecuteOverturnY()
        {
            ValidateParameters(2);
            Overturn_y.Overturn(_parameters[0], _parameters[1]);
        }

        /// <summary>
        /// 执行 90 度旋转
        /// </summary>
        private void ExecuteRotate90()
        {
            ValidateParameters(2);
            Rotate.Rotate_90(_parameters[0], _parameters[1]);
        }

        /// <summary>
        /// 执行坐标移动
        /// </summary>
        private void ExecuteMoveCoordinate()
        {
            ValidateParameters(6);
            Move_coordinate.move(
                _parameters[0], _parameters[1], 
                _parameters[2], _parameters[3], 
                _parameters[4], _parameters[5]);
        }

        /// <summary>
        /// 执行文件合并
        /// </summary>
        private void ExecuteMerge()
        {
            ValidateParameters(3);
            Merge.Merge_cad(_parameters[0], _parameters[1], _parameters[2]);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 验证参数数量
        /// </summary>
        /// <param name="requiredCount">所需参数数量</param>
        private void ValidateParameters(int requiredCount)
        {
            if (_parameters == null || _parameters.Length < requiredCount)
            {
                throw new ArgumentException(
                    string.Format("参数数量不足，需要 {0} 个，实际 {1} 个", 
                    requiredCount, _parameters == null ? 0 : _parameters.Length));
            }

            // 验证必填参数不为空
            for (int i = 0; i < requiredCount; i++)
            {
                if (string.IsNullOrEmpty(_parameters[i]))
                {
                    throw new ArgumentException(string.Format("参数 {0} 不能为空", i + 1));
                }
            }
        }

        #endregion
    }
}