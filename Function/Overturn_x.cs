using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace merge_cad.Function
{
    internal class Overturn_x
    {
        // 正则表达式缓存
        private static readonly Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();
        
        // 镜像类型枚举
        private enum MirrorType { None = 0, MirrorY = 1, MirrorX = 2 }
        
        // 层类型枚举
        private enum LayerType { None = 0, Top = 1, Bottom = 2 }

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
        /// 文件翻转处理主入口
        /// </summary>
        public static void Overturn(string old_name, string new_name)
        {
            try
            {
                var mirrorType = MirrorType.None;
                var nailList = new List<string>();
                var viaPadList = new List<string>();

                // 第一遍：检测镜像类型和收集 NAILLOC
                AnalyzeFile(old_name, ref mirrorType, nailList);
                
                // 第二遍：收集 VIA_PAD
                CollectViaPads(old_name, nailList, viaPadList);
                
                // 第三遍：处理并输出新文件
                ProcessAndWriteFile(old_name, new_name, mirrorType, viaPadList);
            }
            catch (Exception e)
            {
                MessageBox.Show("处理文件时出错：" + e.Message);
            }
        }

        #endregion

        #region 文件分析

        /// <summary>
        /// 分析文件，获取镜像类型和 NAILLOC 列表
        /// </summary>
        private static void AnalyzeFile(string fileName, ref MirrorType mirrorType, List<string> nailList)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (Ismatchs(@"SHAPE\s.+\sMIRRORY\s.+", line))
                    {
                        mirrorType = MirrorType.MirrorY;
                    }
                    else if (Ismatchs(@"SHAPE\s.+\sMIRRORX\s.+", line))
                    {
                        mirrorType = MirrorType.MirrorX;
                    }

                    if (Ismatchs(@"NAILLOC\s(\w+)\s\d+\s\S+\s(\S+)\s.*\s\w+", line))
                    {
                        string[] output = Getmatch(@"NAILLOC\s(\w+)\s\d+\s\S+\s(\S+)\s.*\s\w+", line);
                        if (output.Length > 1 && !string.IsNullOrEmpty(output[1]))
                        {
                            nailList.Add(output[1]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 收集 VIA_PAD 列表
        /// </summary>
        private static void CollectViaPads(string fileName, List<string> nailList, List<string> viaPadList)
        {
            foreach (string nail in nailList)
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string pattern = @"VIA\s(\S+)\s.*\s" + nail + @"$";
                        if (Ismatchs(pattern, line))
                        {
                            string[] output = Getmatch(pattern, line);
                            if (output.Length > 1 && !string.IsNullOrEmpty(output[1]))
                            {
                                viaPadList.Add(output[1]);
                                Console.WriteLine(output[1]);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region 文件处理

        /// <summary>
        /// 处理文件并写入新文件
        /// </summary>
        private static void ProcessAndWriteFile(string oldName, string newName, MirrorType mirrorType, List<string> viaPadList)
        {
            using (StreamWriter wr = new StreamWriter(newName))
            using (StreamReader sr = new StreamReader(oldName))
            {
                SectionFlags flags = new SectionFlags();
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    line = ProcessLine(line, mirrorType, viaPadList, flags);
                    wr.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// 根据当前段处理单行内容
        /// </summary>
        private static string ProcessLine(string line, MirrorType mirrorType, List<string> viaPadList, SectionFlags flags)
        {
            UpdateSectionFlags(line, flags);

            if (flags.BoardFlag) line = ProcessBoardLine(line);
            if (flags.PadFlag) line = ProcessPadLine(line, viaPadList, flags);
            if (flags.CompFlag) line = ProcessComponentLine(line, mirrorType, flags);
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
            flags.PadFlag = ToggleFlag(line, @"\$PADSTACKS", @"\$ENDPADSTACKS", flags.PadFlag);
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
        /// 处理 BOARD 段
        /// </summary>
        private static string ProcessBoardLine(string line)
        {
            if (Ismatchs(@"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line))
            {
                string[] output = Getmatch(@"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line);
                if (output.Length >= 5)
                {
                    line = "LINE " + output[1] + " " + NegateDecimal(output[2]) + " " + output[3] + " " + NegateDecimal(output[4]);
                }
            }
            return line;
        }

        /// <summary>
        /// 处理 PADSTACK 段
        /// </summary>
        private static string ProcessPadLine(string line, List<string> viaPadList, SectionFlags flags)
        {
            if (Ismatchs(@"PADSTACK\s(\S+)\s.*", line))
            {
                string[] output = Getmatch(@"PADSTACK\s(\S+)\s.*", line);
                if (output.Length > 1)
                {
                    flags.ViaFlag = viaPadList.Contains(output[1]);
                }
            }

            if (flags.ViaFlag && Ismatchs(@"PAD (PAD\d+)\s(\w+)\s(.+)", line))
            {
                string[] output = Getmatch(@"PAD (PAD\d+)\s(\w+)\s(.+)", line);
                if (output.Length >= 4)
                {
                    string layer = output[2];
                    string newLayer = layer;
                    
                    if (layer == "SOLDERMASK_TOP") newLayer = "SOLDERMASK_BOTTOM";
                    else if (layer == "SOLDERMASK_BOTTOM") newLayer = "SOLDERMASK_TOP";
                    else if (layer == "TOP") newLayer = "BOTTOM";
                    else if (layer == "BOTTOM") newLayer = "TOP";
                    else if (layer == "FM_TOP") newLayer = "FM_BOTTOM";
                    else if (layer == "FM_BOTTOM") newLayer = "FM_TOP";

                    if (newLayer != layer)
                    {
                        line = "PAD " + output[1] + " " + newLayer + " " + output[3];
                    }
                }
            }

            return line;
        }

        /// <summary>
        /// 处理 COMPONENT 段
        /// </summary>
        private static string ProcessComponentLine(string line, MirrorType mirrorType, SectionFlags flags)
        {
            if (Ismatchs(@"LAYER\s(\S+)", line))
            {
                string[] output = Getmatch(@"LAYER\s(\S+)", line);
                if (output.Length > 1)
                {
                    if (output[1] == "TOP")
                    {
                        flags.LayFlag = LayerType.Top;
                        line = "LAYER BOTTOM";
                    }
                    else if (output[1] == "BOTTOM")
                    {
                        flags.LayFlag = LayerType.Bottom;
                        line = "LAYER TOP";
                    }
                    else
                    {
                        flags.LayFlag = LayerType.None;
                    }
                }
            }

            if (Ismatchs(@"ROTATION\s(\S+)", line))
            {
                string[] output = Getmatch(@"ROTATION\s(\S+)", line);
                if (output.Length > 1)
                {
                    decimal rotation = Convert.ToDecimal(output[1]);
                    decimal newRotation = CalculateRotation(rotation, flags.LayFlag, mirrorType);
                    line = "ROTATION " + newRotation.ToString();
                    flags.LayFlag = LayerType.None;
                }
            }

            if (Ismatchs(@"PLACE\s(\S+)\s(\S+)", line))
            {
                string[] output = Getmatch(@"PLACE\s(\S+)\s(\S+)", line);
                if (output.Length >= 3)
                {
                    line = "PLACE " + output[1] + " " + NegateDecimal(output[2]);
                }
            }

            if (Ismatchs(@"SHAPE\s(.+)\s(\S+)\s(\S+)", line))
            {
                string[] output = Getmatch(@"SHAPE\s(.+)\s(\S+)\s(\S+)", line);
                if (output.Length >= 4)
                {
                    line = ProcessShapeLine(output, mirrorType);
                }
            }

            return line;
        }

        /// <summary>
        /// 计算旋转角度
        /// </summary>
        private static decimal CalculateRotation(decimal rotation, LayerType layerType, MirrorType mirrorType)
        {
            if (layerType == LayerType.Top)
            {
                if (mirrorType == MirrorType.MirrorX)
                {
                    return rotation <= 180 ? 180 - rotation : 540 - rotation;
                }
                if (mirrorType == MirrorType.MirrorY)
                {
                    return 360 - rotation;
                }
            }
            else if (layerType == LayerType.Bottom)
            {
                if (mirrorType == MirrorType.MirrorY)
                {
                    return rotation <= 180 ? 180 - rotation : 540 - rotation;
                }
                if (mirrorType == MirrorType.MirrorX)
                {
                    return 360 - rotation;
                }
            }
            return rotation;
        }

        /// <summary>
        /// 处理 SHAPE 行
        /// </summary>
        private static string ProcessShapeLine(string[] output, MirrorType mirrorType)
        {
            string a = output[2];
            string b = output[3];
            string c = "0";
            string d = "0";

            if (a == "0" && b == "0")
            {
                if (mirrorType == MirrorType.MirrorY) c = "MIRRORX";
                else if (mirrorType == MirrorType.MirrorX) c = "MIRRORY";
                d = "FLIP";
            }
            else if (b == "FLIP")
            {
                c = "0";
                d = "0";
            }

            return "SHAPE " + output[1] + " " + c + " " + d;
        }

        /// <summary>
        /// 处理 ROUTE 段
        /// </summary>
        private static string ProcessRouteLine(string line)
        {
            // LAYER TOP/BOTTOM
            if (Ismatchs(@"LAYER\s+(TOP|BOTTOM).*", line))
            {
                string[] output = Getmatch(@"LAYER\s+(TOP|BOTTOM).*", line);
                if (output.Length > 1)
                {
                    string newLayer = output[1] == "TOP" ? "BOTTOM" : "TOP";
                    line = "LAYER " + newLayer;
                }
            }

            // LAYER SOLDERMASK
            if (Ismatchs(@"LAYER\s+(SOLDERMASK_TOP|SOLDERMASK_BOTTOM)", line))
            {
                string[] output = Getmatch(@"LAYER\s+(SOLDERMASK_TOP|SOLDERMASK_BOTTOM)", line);
                if (output.Length > 1)
                {
                    string newLayer = output[1] == "SOLDERMASK_TOP" ? "SOLDERMASK_BOTTOM" : "SOLDERMASK_TOP";
                    line = "LAYER " + newLayer;
                }
            }

            // LINE
            if (Ismatchs(@"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line))
            {
                string[] output = Getmatch(@"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line);
                if (output.Length >= 5)
                {
                    line = "LINE " + output[1] + " " + NegateDecimal(output[2]) + " " + output[3] + " " + NegateDecimal(output[4]);
                }
            }

            // ARC
            if (Ismatchs(@"ARC\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line))
            {
                string[] output = Getmatch(@"ARC\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)", line);
                if (output.Length >= 7)
                {
                    line = "ARC " + output[1] + " " + NegateDecimal(output[2]) + " " + output[3] + " " + NegateDecimal(output[4]) + " " + output[5] + " " + NegateDecimal(output[6]);
                }
            }

            // VIA
            if (Ismatchs(@"(VIA\s\S+)\s(\S+)\s(\S+)\s(.+)\s(VIA\d+)", line))
            {
                string[] output = Getmatch(@"(VIA\s\S+)\s(\S+)\s(\S+)\s(.+)\s(VIA\d+)", line);
                if (output.Length >= 6)
                {
                    line = output[1] + " " + output[2] + " " + NegateDecimal(output[3]) + " " + output[4] + " " + output[5];
                }
            }

            // NAILLOC
            if (Ismatchs(@"NAILLOC\s(\w+)\s(\d+)\s(\S+)\s(\S+)\s(.*)\s(\w+)", line))
            {
                string[] output = Getmatch(@"NAILLOC\s(\w+)\s(\d+)\s(\S+)\s(\S+)\s(.*)\s(\w+)", line);
                if (output.Length >= 7)
                {
                    string newLayer = output[6] == "TOP" ? "BOTTOM" : output[6] == "BOTTOM" ? "TOP" : output[6];
                    line = "NAILLOC " + output[1] + " " + output[2] + " " + output[3] + " " + NegateDecimal(output[4]) + " " + output[5] + " " + newLayer;
                }
            }

            return line;
        }

        /// <summary>
        /// 处理 TESTPIN 段
        /// </summary>
        private static string ProcessTestPinLine(string line)
        {
            if (Ismatchs(@"(TESTPIN\s)(\d+)\s(\S+\.\S+)\s(\S+\.\S+)\s(.*)\s(\w+)", line))
            {
                string[] output = Getmatch(@"(TESTPIN\s)(\d+)\s(\S+\.\S+)\s(\S+\.\S+)\s(.*)\s(\w+)", line);
                if (output.Length >= 7)
                {
                    string newLayer = output[6] == "TOP" ? "BOTTOM" : output[6] == "BOTTOM" ? "TOP" : output[6];
                    line = output[1] + output[2] + " " + output[3] + " " + NegateDecimal(output[4]) + " " + output[5] + " " + newLayer;
                }
            }
            return line;
        }

        /// <summary>
        /// 处理 POWERPIN 段
        /// </summary>
        private static string ProcessPowerPinLine(string line)
        {
            if (Ismatchs(@"POWERPIN\s(\S+)\s(\S+)\s(\S+)\s(.+)\s(\w+)", line))
            {
                string[] output = Getmatch(@"POWERPIN\s(\S+)\s(\S+)\s(\S+)\s(.+)\s(\w+)", line);
                if (output.Length >= 6)
                {
                    string newLayer = output[5] == "TOP" ? "BOTTOM" : output[5] == "BOTTOM" ? "TOP" : output[5];
                    line = "POWERPIN " + output[1] + " " + output[2] + " " + NegateDecimal(output[3]) + " " + output[4] + " " + newLayer;
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

        #endregion

        #region 辅助类

        /// <summary>
        /// 段标志类（必须是 class 才能在方法内修改属性）
        /// </summary>
        private class SectionFlags
        {
            public bool BoardFlag = false;
            public bool PadFlag = false;
            public bool ViaFlag = false;
            public bool CompFlag = false;
            public bool RoutFlag = false;
            public bool TestFlag = false;
            public bool PowerFlag = false;
            public LayerType LayFlag = LayerType.None;
        }

        #endregion
    }
}