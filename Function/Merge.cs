using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace merge_cad.Function
{
    internal class Merge
    {
        // 正则表达式缓存
        private static readonly Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();

        // 段配置信息
        private class SectionConfig
        {
            public string StartTag { get; set; }
            public string EndTag { get; set; }
            public string SkipPattern { get; set; }
        }

        #region 正则匹配方法

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
        /// 合并 CAD 文件（子文件合并到主文件）
        /// </summary>
        public static void Merge_cad(string sub_name, string mb_name, string new_name)
        {
            try
            {
                // 读取子文件内容
                List<string> subLines = ReadFileLines(sub_name);
                
                // 解析子文件各段内容
                Dictionary<string, List<string>> subSections = ParseSections(subLines);
                
                // 合并并写入新文件
                MergeAndWrite(mb_name, new_name, subSections);
            }
            catch (Exception e)
            {
                MessageBox.Show("合并文件时出错：" + e.Message);
            }
        }

        #endregion

        #region 文件读取

        /// <summary>
        /// 读取文件所有行
        /// </summary>
        private static List<string> ReadFileLines(string fileName)
        {
            List<string> lines = new List<string>();
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }

        /// <summary>
        /// 解析子文件的各个段
        /// </summary>
        private static Dictionary<string, List<string>> ParseSections(List<string> lines)
        {
            Dictionary<string, List<string>> sections = new Dictionary<string, List<string>>();
            
            // 定义所有需要提取的段
            SectionConfig[] configs = new SectionConfig[]
            {
                new SectionConfig { StartTag = @"\$BOARD$", EndTag = @"\$ENDBOARD$", SkipPattern = @"THICKNESS.+" },
                new SectionConfig { StartTag = @"\$PADS$", EndTag = @"\$ENDPADS$", SkipPattern = null },
                new SectionConfig { StartTag = @"\$PADSTACKS$", EndTag = @"\$ENDPADSTACKS$", SkipPattern = null },
                new SectionConfig { StartTag = @"\$SHAPES$", EndTag = @"\$ENDSHAPES$", SkipPattern = null },
                new SectionConfig { StartTag = @"\$DEVICES$", EndTag = @"\$ENDDEVICES$", SkipPattern = null },
                new SectionConfig { StartTag = @"\$COMPONENTS$", EndTag = @"\$ENDCOMPONENTS$", SkipPattern = null },
                new SectionConfig { StartTag = @"\$SIGNALS$", EndTag = @"\$ENDSIGNALS$", SkipPattern = null },
                new SectionConfig { StartTag = @"\$ROUTES$", EndTag = @"\$ENDROUTES$", SkipPattern = null },
                new SectionConfig { StartTag = @"\$TESTPINS$", EndTag = @"\$ENDTESTPINS$", SkipPattern = null },
                new SectionConfig { StartTag = @"\$POWERPINS$", EndTag = @"\$ENDPOWERPINS$", SkipPattern = null }
            };

            foreach (SectionConfig config in configs)
            {
                List<string> sectionLines = ExtractSection(lines, config);
                if (sectionLines.Count > 0)
                {
                    sections[config.StartTag] = sectionLines;
                }
            }

            return sections;
        }

        /// <summary>
        /// 提取指定段的内容
        /// </summary>
        private static List<string> ExtractSection(List<string> lines, SectionConfig config)
        {
            List<string> result = new List<string>();
            bool inSection = false;

            foreach (string line in lines)
            {
                if (Ismatchs(config.StartTag, line))
                {
                    inSection = true;
              //      result.Add(line);
                    continue;
                }

                if (inSection)
                {
                    // 跳过指定模式的行
                    if (config.SkipPattern != null && Ismatchs(config.SkipPattern, line))
                    {
                        continue;
                    }                 
                    if (Ismatchs(config.EndTag, line))
                    {
                        break;
                    }

                    result.Add(line);

                }
            }

            return result;
        }

        #endregion

        #region 文件合并

        /// <summary>
        /// 合并文件并写入
        /// </summary>
        private static void MergeAndWrite(string mb_name, string new_name, Dictionary<string, List<string>> subSections)
        {
            using (StreamWriter wr = new StreamWriter(new_name))
            using (StreamReader sr = new StreamReader(mb_name))
            {
                SectionContext context = new SectionContext();
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    line = ProcessLine(line, wr, subSections, context);
                    if (line != null)
                    {
                        wr.WriteLine(line);
                    }
                }
            }
        }

        /// <summary>
        /// 处理单行内容
        /// </summary>
        private static string ProcessLine(string line, StreamWriter wr, Dictionary<string, List<string>> subSections, SectionContext context)
        {
            // 检查是否进入段开始标记
            CheckSectionStart(line, context);

            // BOARD 段特殊处理：跳过 ATTRIBUTE /GRA3/
            if (context.InBoard && Ismatchs(@"ATTRIBUTE\s+\/GRA3\/.+", line))
            {
                return null;
            }

            // 检查是否到达段结束标记，插入子文件对应段
            if (CheckSectionEnd(line, context))
            {
                InsertSubSection(wr, subSections, context.CurrentSection);
                context.InSection = false;
                context.CurrentSection = null;
            }

            return line;
        }

        /// <summary>
        /// 检查段开始标记
        /// </summary>
        private static void CheckSectionStart(string line, SectionContext context)
        {
            if (Ismatchs(@"\$BOARD$", line))
            {
                context.InSection = true;
                context.InBoard = true;
                context.CurrentSection = @"\$BOARD$";
            }
            else if (Ismatchs(@"\$PADS$", line))
            {
                context.InSection = true;
                context.CurrentSection = @"\$PADS$";
            }
            else if (Ismatchs(@"\$PADSTACKS$", line))
            {
                context.InSection = true;
                context.CurrentSection = @"\$PADSTACKS$";
            }
            else if (Ismatchs(@"\$SHAPES$", line))
            {
                context.InSection = true;
                context.CurrentSection = @"\$SHAPES$";
            }
            else if (Ismatchs(@"\$DEVICES$", line))
            {
                context.InSection = true;
                context.CurrentSection = @"\$DEVICES$";
            }
            else if (Ismatchs(@"\$COMPONENTS$", line))
            {
                context.InSection = true;
                context.CurrentSection = @"\$COMPONENTS$";
            }
            else if (Ismatchs(@"\$SIGNALS$", line))
            {
                context.InSection = true;
                context.CurrentSection = @"\$SIGNALS$";
            }
            else if (Ismatchs(@"\$ROUTES$", line))
            {
                context.InSection = true;
                context.CurrentSection = @"\$ROUTES$";
            }
            else if (Ismatchs(@"\$TESTPINS$", line))
            {
                context.InSection = true;
                context.CurrentSection = @"\$TESTPINS$";
            }
            else if (Ismatchs(@"\$POWERPINS$", line))
            {
                context.InSection = true;
                context.CurrentSection = @"\$POWERPINS$";
            }
        }

        /// <summary>
        /// 检查段结束标记
        /// </summary>
        private static bool CheckSectionEnd(string line, SectionContext context)
        {
            if (context.CurrentSection == null) return false;

            if (context.CurrentSection == @"\$BOARD$" && Ismatchs(@"\$ENDBOARD$", line))
            {
                return true;
            }
            else if (context.CurrentSection == @"\$PADS$" && Ismatchs(@"\$ENDPADS$", line))
            {
                return true;
            }
            else if (context.CurrentSection == @"\$PADSTACKS$" && Ismatchs(@"\$ENDPADSTACKS$", line))
            {
                return true;
            }
            else if (context.CurrentSection == @"\$SHAPES$" && Ismatchs(@"\$ENDSHAPES$", line))
            {
                return true;
            }
            else if (context.CurrentSection == @"\$DEVICES$" && Ismatchs(@"\$ENDDEVICES$", line))
            {
                return true;
            }
            else if (context.CurrentSection == @"\$COMPONENTS$" && Ismatchs(@"\$ENDCOMPONENTS$", line))
            {
                return true;
            }
            else if (context.CurrentSection == @"\$SIGNALS$" && Ismatchs(@"\$ENDSIGNALS$", line))
            {
                return true;
            }
            else if (context.CurrentSection == @"\$ROUTES$" && Ismatchs(@"\$ENDROUTES$", line))
            {
                return true;
            }
            else if (context.CurrentSection == @"\$TESTPINS$" && Ismatchs(@"\$ENDTESTPINS$", line))
            {
                return true;
            }
            else if (context.CurrentSection == @"\$POWERPINS$" && Ismatchs(@"\$ENDPOWERPINS$", line))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 插入子文件的段内容
        /// </summary>
        private static void InsertSubSection(StreamWriter wr, Dictionary<string, List<string>> subSections, string sectionKey)
        {
            if (subSections.ContainsKey(sectionKey))
            {
                foreach (string subLine in subSections[sectionKey])
                {
                    // BOARD 段跳过 THICKNESS 行
                    if (sectionKey == @"\$BOARD$" && Ismatchs(@"THICKNESS.+", subLine))
                    {
                        continue;
                    }
                    wr.WriteLine(subLine);
                }
            }
        }

        #endregion

        #region 辅助类

        /// <summary>
        /// 段处理上下文
        /// </summary>
        private class SectionContext
        {
            public bool InSection = false;
            public bool InBoard = false;
            public string CurrentSection = null;
        }

        #endregion
    }
}