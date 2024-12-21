using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;

namespace merge_cad.Function
{
    internal class Merge
    {

        public static void Getmatch(string patten, string matstr, string[] output)
        {
            Regex re = new Regex(patten);
            MatchCollection matches = re.Matches(matstr);

            foreach (Match match in matches)
            {

                GroupCollection groups = match.Groups;
                int index = 0;
                foreach (Group group in groups)
                {
                    output[index] = group.Value;
                    index++;
                }
            }

        }

        public static bool Ismatchs(string patten, string matstr)
        {
            Regex re = new Regex(patten);
            if (re.IsMatch(matstr))
            { return true; }
            else { return false; }

        }
        public static void Merge_cad(string sub_name,string mb_name, string new_name)
        {

            string l;
            List<string> sub_line = new List<string>();
            try 
            {
                using (StreamReader sb = new StreamReader(sub_name))
                {
                    while ((l = sb.ReadLine()) != null)
                    {
                        sub_line.Add(l);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            try
            {
                using (StreamWriter wr = new StreamWriter(new_name))
                {
                    using (StreamReader sr = new StreamReader(mb_name))
                    {
                        bool board_flag = false;
                        bool pad_flag = false;
                        bool padstack_flag = false;
                        bool shap_flag=false;
                        bool dev_flag=false;
                        bool comp_flag=false;
                        bool signal_flag=false;
                        bool rout_flag=false;
                        bool test_flag = false;
                        bool power_flag = false;
                        string line;
                        string Pstr;

                        while ((line = sr.ReadLine()) != null)
                        {

                            Pstr = @"\$BOARD";
                            if (Ismatchs(Pstr, line))
                            {
                                board_flag=true;

                            }

                            if (board_flag)
                            {
                                Pstr = @"ATTRIBUTE\s+\/GRA3\/.+";
                                if (Ismatchs(Pstr, line))
                                {
                                    continue;
                                }
                            }

                            Pstr = @"\$ENDBOARD";
                            if (Ismatchs(Pstr, line))
                            {
                                board_flag = false;
                                
                                foreach (string s in sub_line) 
                                    {
                                        Pstr = @"\$BOARD";
                                        if (Ismatchs(Pstr, s))
                                        {
                                            board_flag = true;
                                            continue;
                                        }

                                        Pstr = @"THICKNESS.+";
                                        if (Ismatchs(Pstr, s))
                                        {
                                            continue;
                                        }

                                        if(board_flag)
                                        { wr.WriteLine(s); }

                                        Pstr = @"\$ENDBOARD";
                                        if (Ismatchs(Pstr, s))
                                        {
                                            board_flag = false;
                                           continue;
                                    }

                                    }
                                continue;
                            }

                            Pstr = @"\$ENDPADS$";
                            if (Ismatchs(Pstr, line))
                            {
                                foreach (string s in sub_line)
                                {
                                        Pstr = @"\$PADS$";
                                        if (Ismatchs(Pstr, s))
                                        {
                                            pad_flag = true;
                                            continue;
                                        }
                 

                                        if (pad_flag)
                                        { wr.WriteLine(s); }

                                        Pstr = @"\$ENDPADS$";
                                        if (Ismatchs(Pstr, s))
                                        {
                                            pad_flag = false;
                                            continue;
                                        }

                                    }
                                continue;
                            }
                            
                            Pstr = @"\$ENDPADSTACKS$";
                            if (Ismatchs(Pstr, line))
                            {
                                foreach (string s in sub_line)
                                { 
                                    Pstr = @"\$PADSTACKS$";
                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        padstack_flag = true;
                                                                        continue;
                                                                    }


                                    if (padstack_flag)
                                    { wr.WriteLine(s); }

                                    Pstr = @"\$ENDPADSTACKS$";
                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        padstack_flag = false;
                                                                    }

                                 }
                                continue;
                           }

                            Pstr = @"\$ENDSHAPES$";
                            if (Ismatchs(Pstr, line))
                            {
                                foreach (string s in sub_line)
                                {
                                   Pstr = @"\$SHAPES$";
                                   if (Ismatchs(Pstr, s))
                                      {
                                         shap_flag = true;
                                         continue;
                                       }


                                    if (shap_flag)
                                    { wr.WriteLine(s); }

                                     Pstr = @"\$ENDSHAPES$";
                                     if (Ismatchs(Pstr, s))
                                         {
                                            shap_flag = false;
                                         }

                                 }
                                continue;
                            }

      

                            Pstr = @"\$ENDDEVICES$";
                            if (Ismatchs(Pstr, line))
                            {
                                foreach (string s in sub_line)
                                {
                                 Pstr = @"\$DEVICES$";
                                 if (Ismatchs(Pstr, s))
                                 {
                                   dev_flag = true;
                                   continue;
                                 }

                                 if (dev_flag)
                                 { wr.WriteLine(s); }

                                 Pstr = @"\$ENDDEVICES$";
                                 if (Ismatchs(Pstr, s))
                                 {
                                  dev_flag = false;
                                  }

                                                                }

                                continue;

                            }

                             Pstr = @"\$ENDCOMPONENTS$";
                             if (Ismatchs(Pstr, line))
                             {
                                foreach (string s in sub_line)
                                {
                                                                    Pstr = @"\$COMPONENTS$";
                                                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        comp_flag = true;
                                                                        continue;
                                                                    }


                                                                    if (comp_flag)
                                                                    { wr.WriteLine(s); }

                                                                    Pstr = @"\$ENDCOMPONENTS$";
                                                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        comp_flag = false;
                                                                    }

                                                                }


                                continue;
                            }


                            Pstr = @"\$ENDSIGNALS$";
                            if (Ismatchs(Pstr, line))
                            {
                                foreach (string s in sub_line)
                                {
                                                                    Pstr = @"\$SIGNALS$";
                                                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        signal_flag = true;
                                                                        continue;
                                                                    }


                                                                    if (signal_flag)
                                                                    { wr.WriteLine(s); }

                                                                    Pstr = @"\$ENDSIGNALS$";
                                                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        signal_flag = false;
                                                                    }

                                                                }

                                continue;

                            }


                            Pstr = @"\$ENDROUTES$";
                           if (Ismatchs(Pstr, line))
                           {
                                foreach (string s in sub_line)
                                {
                                                                    Pstr = @"\$ROUTES$";
                                                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        rout_flag = true;
                                                                        continue;
                                                                    }


                                                                    if (rout_flag)
                                                                    { wr.WriteLine(s); }

                                                                    Pstr = @"\$ENDROUTES$";
                                                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        rout_flag = false;
                                                                    }

                                                                }

                                continue;

                            }



                           Pstr = @"\$ENDTESTPINS$";
                           if (Ismatchs(Pstr, line))
                            {
                                foreach (string s in sub_line)
                                {
                                                                    Pstr = @"\$TESTPINS$";
                                                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        test_flag = true;
                                                                        continue;
                                                                    }


                                                                    if (test_flag)
                                                                    { wr.WriteLine(s); }

                                                                    Pstr = @"\$ENDTESTPINS$";
                                                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        test_flag = false;
                                                                    }

                                                                }
                                continue;

                            }

                           Pstr = @"\$ENDPOWERPINS$";
                            if (Ismatchs(Pstr, line))
                            {
                                foreach (string s in sub_line)
                                {
                                                                    Pstr = @"\$POWERPINS$";
                                                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        power_flag = true;
                                                                        continue;
                                                                    }


                                                                    if (power_flag)
                                                                    { wr.WriteLine(s); }

                                                                    Pstr = @"\$ENDPOWERPINS$";
                                                                    if (Ismatchs(Pstr, s))
                                                                    {
                                                                        power_flag = false;
                                                                    }

                                                                }
                                continue;

                            }
                         

                        wr.WriteLine(line);

                        }

                    }
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }


        }
    }
}
