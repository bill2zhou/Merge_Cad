using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace merge_cad.Function
{
    internal class Rotate
    {
        // 正则表达式缓存
        private static readonly Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();

        #region 正则匹配方法

        /// <summary>
        /// 正则匹配并返回分组结果
        /// </summary>
        public static string[] Getmatch(string pattern, string matchStr)
        {
            Regex re = GetRegex(pattern);
            MatchCollection matches = re.Matches(matchStr);
            
            if (matches.Count == 0) return new string[0];
            
            GroupCollection groups = matches[0].Groups;
            string[] result = new string[groups.Count];
            
            for (int i = 0; i < groups.Count; i++)
            {
                result[i] = groups[i].Value;
            }
            
            return result;
        }

        /// <summary>
        /// 判断是否匹配
        /// </summary>
        public static bool Ismatchs(string pattern, string matchStr)
        {
            Regex re = GetRegex(pattern);
            return re.IsMatch(matchStr);
        }

        /// <summary>
        /// 获取或创建缓存的正则表达式
        /// </summary>
        private static Regex GetRegex(string pattern)
        {
            lock (_regexCache)
            {
                if (!_regexCache.ContainsKey(pattern))
                {
                    _regexCache[pattern] = new Regex(pattern);
                }
                return _regexCache[pattern];
            }
        }

        #endregion

        #region 主方法

        /// <summary>
        /// 文件 90 度旋转处理主入口
        /// </summary>
        public static void Rotate_90(string old_name, string new_name)
        {
            try
            {
                ProcessAndWriteFile(old_name, new_name);
            }
            catch (Exception e)
            {
                MessageBox.Show("处理文件时出错：" + e.Message);
            }
        }

        #endregion

        #region 文件处理

        /// <summary>
        /// 处理文件并写入新文件
        /// </summary>
        private static void ProcessAndWriteFile(string oldName, string newName)
        {
            using (StreamWriter wr = new StreamWriter(newName))
            using (StreamReader sr = new StreamReader(oldName))
            {
                SectionFlags flags = new SectionFlags();
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    line = ProcessLine(line, flags);
                    wr.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// 根据当前段处理单行内容
        /// </summary>
        private static string ProcessLine(string line, SectionFlags flags)
        {
            UpdateSectionFlags(line, flags);

            if (flags.BoardFlag) line = ProcessBoardLine(line);
            if (flags.CompFlag) line = ProcessComponentLine(line);
            if (flags.RoutFlag) line = ProcessRouteLine(line);
            if (flags.TestFlag) line = ProcessTestPinLine(line);
            if (flags.PowerFlag) line = ProcessPowerPinLine(line);

            return line;
        }

        /// <summary>
        /// 更新段标志
        /// </summary>
        private static void UpdateSectionFlags(string line, SectionFlags flags)
        {
            flags.BoardFlag = ToggleFlag(line, @"\$BOARD", @"\$ENDBOARD", flags.BoardFlag);
            flags.CompFlag = ToggleFlag(line, @"\$COMPONENTS", @"\$ENDCOMPONENTS", flags.CompFlag);
            flags.RoutFlag = ToggleFlag(line, @"\$ROUTES", @"\$ENDROUTES", flags.RoutFlag);
            flags.TestFlag = ToggleFlag(line, @"\$TESTPINS", @"\$ENDTESTPINS", flags.TestFlag);
            flags.PowerFlag = ToggleFlag(line, @"\$POWERPINS", @"\$ENDPOWERPINS", flags.PowerFlag);
        }

        private static bool ToggleFlag(string line, string startPattern, string endPattern, bool currentFlag)
        {
            if (Ismatchs(startPattern, line)) return true;
            if (Ismatchs(endPattern, line)) return false;
            return currentFlag;
        }

        #endregion

        #region 各段处理方法

        /// <summary>
        /// 处理 BOARD 段（90 度旋转：交换 X/Y，取反新 X）
        /// </summary>
        private static string ProcessBoardLine(string line)
        {
            if (Ismatchs(@"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line))
            {
                string[] output = Getmatch(@"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line);
                if (output.Length >= 5)
                {
                    line = "LINE " + NegateDecimal(output[2]) + " " + output[1] + " " + NegateDecimal(output[4]) + " " + output[3];
                }
            }
            return line;
        }

        /// <summary>
        /// 处理 COMPONENT 段
        /// </summary>
        private static string ProcessComponentLine(string line)
        {
            if (Ismatchs(@"ROTATION\s(\S+)", line))
            {
                string[] output = Getmatch(@"ROTATION\s(\S+)", line);
                if (output.Length > 1)
                {
                    decimal rotation = Convert.ToDecimal(output[1]);
                    decimal newRotation = NormalizeRotation(rotation + 90);
                    line = "ROTATION " + newRotation.ToString();
                }
            }

            if (Ismatchs(@"PLACE\s(\S+)\s(\S+)", line))
            {
                string[] output = Getmatch(@"PLACE\s(\S+)\s(\S+)", line);
                if (output.Length >= 3)
                {
                    line = "PLACE " + NegateDecimal(output[2]) + " " + output[1];
                }
            }

            return line;
        }

        /// <summary>
        /// 处理 ROUTE 段（90 度旋转：交换 X/Y，取反新 X）
        /// </summary>
        private static string ProcessRouteLine(string line)
        {
            // LINE
            if (Ismatchs(@"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line))
            {
                string[] output = Getmatch(@"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line);
                if (output.Length >= 5)
                {
                    line = "LINE " + NegateDecimal(output[2]) + " " + output[1] + " " + NegateDecimal(output[4]) + " " + output[3];
                }
            }

            // ARC
            if (Ismatchs(@"ARC\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line))
            {
                string[] output = Getmatch(@"ARC\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line);
                if (output.Length >= 7)
                {
                    line = "ARC " + NegateDecimal(output[2]) + " " + output[1] + " " + NegateDecimal(output[4]) + " " + output[3] + " " + NegateDecimal(output[6]) + " " + output[5];
                }
            }

            // VIA
            if (Ismatchs(@"(VIA\s\S+)\s(\S+)\s(\S+)\s(.+)\s(VIA\d+)", line))
            {
                string[] output = Getmatch(@"(VIA\s\S+)\s(\S+)\s(\S+)\s(.+)\s(VIA\d+)", line);
                if (output.Length >= 6)
                {
                    line = output[1] + " " + NegateDecimal(output[3]) + " " + output[2] + " " + output[4] + " " + output[5];
                }
            }

            // NAILLOC
            if (Ismatchs(@"(NAILLOC\s\w+\s\d+)\s(\S+)\s(\S+)\s(.*)", line))
            {
                string[] output = Getmatch(@"(NAILLOC\s\w+\s\d+)\s(\S+)\s(\S+)\s(.*)", line);
                if (output.Length >= 5)
                {
                    line = output[1] + " " + NegateDecimal(output[3]) + " " + output[2] + " " + output[4];
                }
            }

            return line;
        }

        /// <summary>
        /// 处理 TESTPIN 段（90 度旋转：交换 X/Y，取反新 X）
        /// </summary>
        private static string ProcessTestPinLine(string line)
        {
            if (Ismatchs(@"(TESTPIN\s)(\d+)\s(\S+\.\S+)\s(\S+\.\S+)\s(\S+)\s(.*)", line))
            {
                string[] output = Getmatch(@"(TESTPIN\s)(\d+)\s(\S+\.\S+)\s(\S+\.\S+)\s(\S+)\s(.*)", line);
                if (output.Length >= 7)
                {
                    line = output[1] + output[2] + " " + NegateDecimal(output[4]) + " " + output[3] + " " + output[5] + " " + output[6];
                }
            }
            return line;
        }

        /// <summary>
        /// 处理 POWERPIN 段（90 度旋转：交换 X/Y，取反新 X）
        /// </summary>
        private static string ProcessPowerPinLine(string line)
        {
            if (Ismatchs(@"POWERPIN\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(.+)", line))
            {
                string[] output = Getmatch(@"POWERPIN\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(.+)", line);
                if (output.Length >= 6)
                {
                    line = "POWERPIN " + output[1] + " " + NegateDecimal(output[3]) + " " + output[2] + " " + output[4] + " " + output[5];
                }
            }
            return line;
        }

        /// <summary>
        /// 坐标取反
        /// </summary>
        private static string NegateDecimal(string value)
        {
            return (0 - Convert.ToDecimal(value)).ToString();
        }

        /// <summary>
        /// 归一化旋转角度到 0-360 范围
        /// </summary>
        private static decimal NormalizeRotation(decimal rotation)
        {
            while (rotation < 0)
            {
                rotation += 360;
            }
            while (rotation >= 360)
            {
                rotation -= 360;
            }
            return rotation;
        }

        #endregion

        #region 辅助类

        /// <summary>
        /// 段标志类
        /// </summary>
        private class SectionFlags
        {
            public bool BoardFlag = false;
            public bool CompFlag = false;
            public bool RoutFlag = false;
            public bool TestFlag = false;
            public bool PowerFlag = false;
        }

        #endregion
    }
}