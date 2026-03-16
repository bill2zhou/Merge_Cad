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

namespace merge_cad.Function
{
    internal class Rotate
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
        public static void Rotate_90(string old_name, string new_name) {
       //     List<string> nail_oc=new List<string>();
       //     List<string>  via_pad= new List<string>();  

            try
            {
                using (StreamWriter wr = new StreamWriter(new_name))
                {
                    using (StreamReader sr = new StreamReader(old_name))
                    {
                        bool board_flag=false;
                        bool comp_flag=false;
                        bool rout_flag=false;
                        bool test_flag = false;
                        bool power_flag = false;
                        string line;
                        string Pstr;

                      //  string newline;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Pstr = @"\$BOARD";
                            if (Ismatchs(Pstr, line))
                            { 
                            board_flag=true;
                            }
                            if (board_flag)
                            {
                                Pstr = @"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                   line="LINE"  + " " +(0-Convert.ToDecimal(output[2])).ToString() + " " + output[1]  + " "+ (0 - Convert.ToDecimal(output[4])).ToString() + " " + output[3];

                                }
                            }

                            Pstr = @"\$ENDBOARD";
                            if (Ismatchs(Pstr, line))
                            {
                                board_flag = false;
                            }

                            Pstr = @"\$COMPONENTS";
                            if (Ismatchs(Pstr, line))
                            {
                                comp_flag = true;
                            }

                            if (comp_flag)
                            {
                                Pstr = @"ROTATION\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    decimal a = Convert.ToDecimal(output[1]);
                                    decimal b=a+90;
                                    line = "ROTATION" + " " + b.ToString();
                                }

                                Pstr = @"PLACE\s(\S+)\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "PLACE" + " " + (0 - Convert.ToDecimal(output[2])).ToString() + " " + output[1];
                                }            
                            }


                            Pstr = @"\$ENDCOMPONENTS";
                            if (Ismatchs(Pstr, line))
                            {
                                comp_flag = false;
                            }

                            Pstr = @"\$ROUTES";
                            if (Ismatchs(Pstr, line))
                            {
                                rout_flag = true;
                            }

                            if (rout_flag)
                            {



                                Pstr = @"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "LINE"  + " " + (0 - Convert.ToDecimal(output[2])).ToString() + " " + output[1]  + " " + (0 - Convert.ToDecimal(output[4])).ToString() + " " + output[3];

                                }

                                Pstr = @"ARC\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "ARC"  + " " + (0 - Convert.ToDecimal(output[2])).ToString() + " " + output[1] + " " + (0 - Convert.ToDecimal(output[4])).ToString() + " " + output[3]  + " " + (0 - Convert.ToDecimal(output[6])).ToString() + " " + output[5];
                                }

                                Pstr = @"(VIA\s\S+)\s(\S+)\s(\S+)\s(.+)\s(VIA\d+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = output[1]  + " " + (0 - Convert.ToDecimal(output[3])).ToString() + " " + output[2] + " " + output[4]+" "+output[5];

                                }

                                Pstr = @"(NAILLOC\s\w+\s\d+)\s(\S+)\s(\S+)\s(.*)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);              
                                    line = output[1]  + " " + (0 - Convert.ToDecimal(output[3])).ToString() + " " + output[2] + " " + output[4];

                                }
                            }



                            Pstr = @"\$ENDROUTES";
                            if (Ismatchs(Pstr, line))
                            {
                                rout_flag = false;
                            }

                            Pstr = @"\$TESTPINS";
                            if (Ismatchs(Pstr, line))
                            {
                                test_flag = true;
                            }
                            if (test_flag)
                            {
                                Pstr = @"(TESTPIN\s)(\d+)\s(\S+\.\S+)\s(\S+\.\S+)\s(\S+)\s(.*)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = output[1] + output[2] +" " + (0 - Convert.ToDecimal(output[4])).ToString() + " " + output[3] + " "+output[5]+" "+ output[6];

                                }

                            }


                            Pstr = @"\$ENDTESTPINS";
                            if (Ismatchs(Pstr, line))
                            {
                                test_flag = false;
                            }

                            Pstr = @"\$POWERPINS";
                            if (Ismatchs(Pstr, line))
                            {
                                power_flag = true;
                            }

                            if (power_flag)
                            {
                                Pstr = @"POWERPIN\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(.+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);

                                    line ="POWERPIN"+" "+output[1] + " " + (0 - Convert.ToDecimal(output[3])).ToString() + " " + output[2] + " " + output[4] + " " + output[5];

                                }
                            }

                            Pstr = @"\$ENDPOWERPINS";
                            if (Ismatchs(Pstr, line))
                            {
                                power_flag = false;
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
