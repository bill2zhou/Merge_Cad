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
    internal class Overturn_y
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
           //         Console.WriteLine(group.Value);
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
        public static void Overturn(string old_name, string new_name) {
            int flp=0;
            List<string> nail_oc=new List<string>();
            List<string>  via_pad= new List<string>();  
            try
            {
                using (StreamReader sr = new StreamReader(old_name))
                {                
                    string line;
                    string Pstr;
                    
                    
                    while ((line = sr.ReadLine()) != null)
                    {
                        Pstr = @"SHAPE\s.+\sMIRRORY\s.+";
 
                        if (Ismatchs(Pstr, line))
                        { flp = 1; }

                         Pstr = @"SHAPE\s.+\sMIRRORX\s.+";
                        if (Ismatchs(Pstr, line))
                        { flp = 2; }

                        Pstr = @"NAILLOC\s(\w+)\s\d+\s\S+\s(\S+)\s.*\s\w+";
                        if (Ismatchs(Pstr, line))
                        {
                            string[] output = new string[20];
                            Getmatch(Pstr, line, output);
                     //      Console.WriteLine(output[1]);
                            nail_oc.Add(output[1]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
            foreach (string nail in nail_oc)
            {
                string v= nail;
               
                try
                {
                    using (StreamReader sr = new StreamReader(old_name))
                    {
                        string line;
                        string Pstr;
                        while ((line = sr.ReadLine()) != null)
                        {
                            Pstr = @"VIA\s(\S+)\s.*\s"+v+@"$";
                            if (Ismatchs(Pstr, line))
                            {
                              //  Console.WriteLine(line);
                                string[] output = new string[20];
                                Getmatch(Pstr, line, output);
                                Console.WriteLine(output[1]);
                                via_pad.Add(output[1]);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }


            }

            try
            {
                using (StreamWriter wr = new StreamWriter(new_name))
                {
                    using (StreamReader sr = new StreamReader(old_name))
                    {
                        int lay_flag = 0;
                        bool board_flag=false;
                        bool pad_flag=false;
                        bool via_flag=false;
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
                                   line="LINE" + " " +(0-Convert.ToDecimal(output[1])).ToString()+" "+ output[2] +" "+ (0 - Convert.ToDecimal(output[3])).ToString()+" "+output[4] ;

                                }
                            }

                            Pstr = @"\$ENDBOARD";
                            if (Ismatchs(Pstr, line))
                            {
                                board_flag = false;
                            }


                            Pstr = @"\$PADSTACKS";
                            if (Ismatchs(Pstr, line))
                            {
                                pad_flag = true;
                            }
                            if (pad_flag)
                            {
                                Pstr = @"PADSTACK\s(\S+)\s.*";
                                if (Ismatchs(Pstr, line))
                                {
                                    via_flag = false;
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    foreach (string va in via_pad)
                                    {
                                        if (va == output[1])
                                        { 
                                        via_flag = true;
                                        }
                                    
                                    }
                                }
                                if (via_flag)
                                {
                                    Pstr = @"PAD (PAD\d+)\s(\w+)\s(.+)";
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    if (output[2] == "SOLDERMASK_TOP")
                                    {
                                        line = "PAD" + " " + output[1] + " " + "SOLDERMASK_BOTTOM" + " " + output[3];
                                    }
                                    if (output[2] == "SOLDERMASK_BOTTOM")
                                    {
                                        line = "PAD" + " " + output[1] + " " + "SOLDERMASK_TOP" + " " + output[3];
                                    }

                                    if (output[2] == "TOP")
                                    {
                                        line = "PAD" + " " + output[1] + " " + "BOTTOM" + " " + output[3];
                                    }
                                    if (output[2] == "BOTTOM")
                                    {
                                        line = "PAD" + " " + output[1] + " " + "TOP" + " " + output[3];
                                    }


                                    if (output[2] == "FM_TOP")
                                    {
                                        line = "PAD" + " " + output[1] + " " + "FM_BOTTOM" + " " + output[3];
                                    }
                                    if (output[2] == "FM_BOTTOM")
                                    {
                                        line = "PAD" + " " + output[1] + " " + "FM_TOP" + " " + output[3];
                                    }

                                }
                            
                            }

                            Pstr = @"\$ENDPADSTACKS";
                            if (Ismatchs(Pstr, line))
                            {
                                pad_flag = false;
                            }

                            Pstr = @"\$COMPONENTS";
                            if (Ismatchs(Pstr, line))
                            {
                                comp_flag = true;
                            }

                            if (comp_flag)
                            {


                                Pstr = @"LAYER\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    if (output[1] == "TOP")
                                    { 
                                        line= "LAYER BOTTOM";
                                        lay_flag = 1;
                                    }

                                    if (output[1] == "BOTTOM")
                                    {
                                        line = "LAYER TOP";
                                        lay_flag = 2;
                                    }

                                }

                                Pstr = @"ROTATION\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    decimal a = Convert.ToDecimal(output[1]);
                                    decimal b=0;

                                    if (flp == 1)
                                    {
                                        b = 360 - a;
                                        line = "ROTATION" + " " + b.ToString();
                                    }
                                    if (flp == 2)
                                    {
                                        
                                        if (lay_flag == 1)
                                        {
                                            line = "ROTATION" + " " +  (360-a).ToString();
                                            lay_flag = 0;
                                        }
                                        if (lay_flag == 2)
                                        {
                                            if (a <= 180)
                                            {
                                                b = 180 - a;
                                            }
                                            if (a > 180)
                                            {
                                                b = 540-a;
                                            }

                                            line = "ROTATION" + " " + b.ToString();
                                            lay_flag=0;
                                        }
                                    }

                                
                                }

                                Pstr = @"PLACE\s(\S+)\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "PLACE" + " " + (0 - Convert.ToDecimal(output[1])).ToString() + " " + output[2];

                                }

                                Pstr = @"SHAPE\s(.+)\s(\S+)\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    string a = Convert.ToString(output[2]);
                                    string b = Convert.ToString(output[3]);
                                    string c = "";
                                    string d = "";

                                    if (a == "0" && b == "0")
                                    {
                                        if (flp == 1)
                                        {
                                            c = "MIRRORY";
                                        }
                                        if(flp == 2)
                                        {
                                            c = "MIRRORX";
                                        }
                                        d = "FLIP";

                                    }
                                    if (b== "FLIP")
                                    {
                                        c = "0";
                                        d = "0";

                                    }
                                    line =  "SHAPE" + " " + Convert.ToString(output[1]) + " " + c + " " + d;


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
                                Pstr = @"LAYER\s+(TOP|BOTTOM).*";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    string a = Convert.ToString(output[1]);
                                    string b = "";

                                    if(a == "TOP")
                                    { b="BOTTOM"; }
                                    if (a == "BOTTOM")
                                    { b = "TOP"; }
                                    line= "LAYER"+" "+b;
                                }

                                Pstr = @"LAYER\s+(SOLDERMASK_TOP|SOLDERMASK_BOTTOM)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    string a = Convert.ToString(output[1]);
                                    string b = "";
                                    if (a == "SOLDERMASK_TOP")
                                    { b = "SOLDERMASK_BOTTOM"; }
                                    if(a == "SOLDERMASK_BOTTOM")
                                    { a = "SOLDERMASK_TOP"; }
                                    line = "LAYER" + " " + b;
                                }

                                Pstr = @"LINE\s(\S+)\s(\S+)\s(\S+)\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "LINE"  + " " + (0 - Convert.ToDecimal(output[1])).ToString() + " " + output[2]  + " " + (0 - Convert.ToDecimal(output[3])).ToString() + " " + output[4];

                                }

                                Pstr = @"ARC\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)\s(\S+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = "ARC"  + " " + (0 - Convert.ToDecimal(output[1])).ToString() + " " + output[2] + " " + (0 - Convert.ToDecimal(output[3])).ToString() + " " + output[4]  + " " + (0 - Convert.ToDecimal(output[5])).ToString() + " " + output[6];
                                }

                                Pstr = @"(VIA\s\S+)\s(\S+)\s(\S+)\s(.+)\s(VIA\d+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    line = output[1]  + " " + (0 - Convert.ToDecimal(output[2])).ToString() + " " + output[3] + " " + output[4]+" "+output[5];

                                }

                                Pstr = @"NAILLOC\s(\w+)\s(\d+)\s(\S+)\s(\S+)\s(.*)\s(\w+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    string a = Convert.ToString(output[6]);
                                    string b = "";

                                    if (a == "TOP")
                                    { b = "BOTTOM"; }
                                    if (a == "BOTTOM")
                                    { b = "TOP"; }

                                    line = "NAILLOC"+" "+output[1] + " " + output[2]  + " " + (0 - Convert.ToDecimal(output[3])).ToString() + " " + output[4] + " " + output[5]+" "+b;

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
                                Pstr = @"(TESTPIN\s)(\d+)\s(\S+\.\S+)\s(\S+\.\S+)\s(.*)\s(\w+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    string a= Convert.ToString(output[6]);
                                    string b = "";
                                    if (a=="TOP")
                                    { b = "BOTTOM"; }
                                    if (a == "BOTTOM")
                                    { b = "TOP"; }

                                    line = output[1] + output[2] +" " + (0 - Convert.ToDecimal(output[3])).ToString() + " " + output[4] + " "+output[5]+" "+b;

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
                                Pstr = @"POWERPIN\s(\S+)\s(\S+)\s(\S+)\s(.+)\s(\w+)";
                                if (Ismatchs(Pstr, line))
                                {
                                    string[] output = new string[20];
                                    Getmatch(Pstr, line, output);
                                    string a = Convert.ToString(output[5]);
                                    string b = "";
                                    if (a == "TOP")
                                    { b = "BOTTOM"; }
                                    if (a == "BOTTOM")
                                    { b = "TOP"; }

                                    line ="POWERPIN"+" "+ output[1] + " " + (0 - Convert.ToDecimal(output[2])).ToString() + " " + output[3] + " " + output[4] + " " + b;

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
