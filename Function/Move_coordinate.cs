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
    internal class Move_coordinate
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
        public static void move(string x,string y,string pin ,string stuf,string old_name, string new_name) {
 

            try
            {
                using (StreamWriter wr = new StreamWriter(new_name))
                {
                    using (StreamReader sr = new StreamReader(old_name))
                    {
                        bool board_flag=false;
                        bool pad_flag=false;
                        bool padstack_flag=false;
                        bool signal_flag=false;
                        bool shape_flag=false;
                        bool comp_flag=false;
                        bool rout_flag=false;
                        bool test_flag = false;
                        bool power_flag = false;
                        string line;
                        string Pstr;

                      //  string newline;
                        while ((line = sr.ReadLine()) != null)
                        {

                            Pstr = @"ORIGIN\s(\S+)\s(\S+)$";
                            if (Ismatchs(Pstr, line))
                            {
                                line = "ORIGIN" + " " + x + " " + y + "\r\n";
                            }

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
                                   line="LINE"  + " " +(Convert.ToDecimal(output[1])+ Convert.ToDecimal(x)).ToString() + " " + (Convert.ToDecimal(output[2]) + Convert.ToDecimal(y)).ToString() + " "+ ( Convert.ToDecimal(output[3])+ Convert.ToDecimal(x)).ToString() + " " + (Convert.ToDecimal(output[4]) + Convert.ToDecimal(y)).ToString();

                                }
                            }

                            Pstr = @"\$ENDBOARD";
                            if (Ismatchs(Pstr, line))
                            {
                                board_flag = false;
                            }


                            Pstr = @"\$PADS$";
                            if (Ismatchs(Pstr, line))
                            {
                                pad_flag = true;
                            }

                            if (pad_flag)
                            {
                                Pstr = @"PAD\s(\S+)(\s.+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line ="PAD" + " " + output[1] + stuf + output[2];

                                }
                            }

                            Pstr = @"\$ENDPADS$";
                            if (Ismatchs(Pstr, line))
                            {
                                pad_flag = false;
                            }



                            Pstr = @"\$PADSTACKS";
                            if (Ismatchs(Pstr, line))
                            {
                                padstack_flag = true;
                            }

                            if (padstack_flag)
                            {
                                Pstr = @"PAD\s(\S+)(\s.+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "PAD" + " " + output[1] + stuf+output[2];

                                }

                                Pstr = @"PADSTACK\s(\S+)(\s.+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "PADSTACK" + " " + output[1] + stuf+output[2];

                                }


                            }

                            Pstr = @"\$ENDPADSTACKS";
                            if (Ismatchs(Pstr, line))
                            {
                                padstack_flag = false;
                            }


                            Pstr = @"\$SHAPES";
                            if (Ismatchs(Pstr, line))
                            {
                                shape_flag = true;
                            }

                            if (shape_flag)
                            {
                                Pstr = @"(PIN\s\d+\s)(\S+)(\s.+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = output[1] + output[2] + stuf+output[3];

                                }
                            }

                            Pstr = @"\$ENDSHAPES";
                            if (Ismatchs(Pstr, line))
                            {
                                shape_flag = false;
                            }



                            Pstr = @"\$COMPONENTS";
                            if (Ismatchs(Pstr, line))
                            {
                                comp_flag = true;
                            }

                            if (comp_flag)
                            {
                                Pstr = @"COMPONENT\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "COMPONENT" + " " + output[1]+stuf;
                                }

                                Pstr = @"PLACE\s(\S+)\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "PLACE" + " " + ( Convert.ToDecimal(output[1])+ Convert.ToDecimal(x)).ToString() + " " + (Convert.ToDecimal(output[2]) + Convert.ToDecimal(y)).ToString();
                                }            
                            }


                            Pstr = @"\$ENDCOMPONENTS";
                            if (Ismatchs(Pstr, line))
                            {
                                comp_flag = false;
                            }


                            Pstr = @"\$SIGNALS$";
                            if (Ismatchs(Pstr, line))
                            {
                                signal_flag = true;
                            }

                            if (signal_flag)
                            {
                                Pstr = @"SIGNAL\s(\S+)$";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "SIGNAL" + " " + output[1] + stuf;
                                }

                                Pstr = @"NODE\s(\S+)(\s.+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "NODE" + " " + output[1] + stuf +output[2];
                                }
                            }


                            Pstr = @"\$ENDSIGNALS$";
                            if (Ismatchs(Pstr, line))
                            {
                                signal_flag = false;
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
                                    line = "LINE" + " " + (Convert.ToDecimal(output[1]) + Convert.ToDecimal(x)).ToString() + " " + (Convert.ToDecimal(output[2]) + Convert.ToDecimal(y)).ToString() + " " + (Convert.ToDecimal(output[3]) + Convert.ToDecimal(x)).ToString() + " " + (Convert.ToDecimal(output[4]) + Convert.ToDecimal(y)).ToString();

                                }

                                Pstr = @"ARC\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "ARC" + " " + (Convert.ToDecimal(output[1]) + Convert.ToDecimal(x)).ToString() + " " + (Convert.ToDecimal(output[2]) + Convert.ToDecimal(y)).ToString() + " " + (Convert.ToDecimal(output[3]) + Convert.ToDecimal(x)).ToString() + " " + (Convert.ToDecimal(output[4]) + Convert.ToDecimal(y)).ToString() + " " + (Convert.ToDecimal(output[5]) + Convert.ToDecimal(x)).ToString() + " " + (Convert.ToDecimal(output[6]) + Convert.ToDecimal(y)).ToString();
                                }

                                Pstr = @"ROUTE\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "ROUTE" + " " + output[1] + stuf;
                                }


                                Pstr = @"(VIA\s\S+)\s(\S+)\s(\S+)\s(.+)\s(VIA\d+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = output[1]+stuf  + " " + (Convert.ToDecimal(output[2])+ Convert.ToDecimal(x)).ToString() + " " + (Convert.ToDecimal(output[3]) + Convert.ToDecimal(y)).ToString() + " " + output[4]+" "+output[5]+stuf;

                                }

                                Pstr = @"NAILLOC\s(\w+)\s(\d+)\s(\S+)\s(\S+)\s(.*)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);              
                                    line = "NAILLOC"+" "+output[1]+stuf + " " +output[2]+(Convert.ToDecimal(output[3])+ Convert.ToDecimal(x)).ToString() + " " + (Convert.ToDecimal(output[4]) + Convert.ToDecimal(y)).ToString() + " " + output[5];

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
                                Pstr = @"(TESTPIN\s)(\d+)\s(\S+)\s(\S+)\s(\S+)\s(.*)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    string tp = (Convert.ToDecimal(pin) + Convert.ToDecimal(output[2])).ToString();
                                    string a= (Convert.ToDecimal(x) + Convert.ToDecimal(output[3])).ToString();
                                    string b = (Convert.ToDecimal(y) + Convert.ToDecimal(output[4])).ToString();
                                    if (output[3] == "-32767")
                                    {
                                        a = "-32767";
                                        b = "-32767";
                                    }

                                    line = output[1] + tp +" " + a + " " + b + " "+output[5]+stuf+" "+ output[6];

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
                                    string a = (Convert.ToDecimal(x) + Convert.ToDecimal(output[2])).ToString();
                                    string b = (Convert.ToDecimal(y) + Convert.ToDecimal(output[3])).ToString();
                                    if (output[3] == "-32767")
                                    {
                                        a = "-32767";
                                        b = "-32767";
                                    }

                                    line = "POWERPIN"+" "+output[1]+stuf + " " + a + " " + b + " " + output[4]+stuf + " " + output[5];

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
