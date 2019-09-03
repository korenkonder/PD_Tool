using System;
using System.Windows.Forms;
using KKdMainLib;
using KKdMainLib.IO;
using KKdMain = KKdMainLib.Main;
using KKdFARC = KKdMainLib.FARC;

namespace PD_Tool
{
    public static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern bool SetProcessDPIAware();

        [ThreadStatic] public static string function = "";

        [STAThread]
        public static void Main(string[] args)
        {
            SetProcessDPIAware();
            
            Console.Title = "PD_Tool";
            if (args.Length == 0)
            {
                GC.Collect();
                while (function != "Q") MainMenu();
                Exit();
            }

            long header;
            Stream reader;
            foreach (string arg in args)
            {
                     if (Directory.Exists(arg)) new KKdFARC(arg, true).Pack();
                else if (File.Exists(arg) && Path.GetExtension(arg) == ".farc") new KKdFARC(arg).UnPack(true);
                else if (File.Exists(arg))
                {
                    reader = File.OpenReader(arg);
                    header = reader.ReadInt64();
                    reader.Close();
                    if (header == 0x454C494641564944) KKdMainLib.DIVAFILE.Decrypt(arg);
                }
            }
            Exit();
        }

        [ThreadStatic] private static bool JSON = true;

        private static void MainMenu()
        {
            GC.Collect();
            Console. InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.Title = "PD_Tool";
            Console.Clear();

            KKdMain.ConsoleDesign(true);
            KKdMain.ConsoleDesign("                Choose action:");
            KKdMain.ConsoleDesign(false);
            KKdMain.ConsoleDesign("1. Extract FARC Archive");
            KKdMain.ConsoleDesign("2. Create  FARC Archive");
            KKdMain.ConsoleDesign("3. Decrypt from DIVAFILE");
            KKdMain.ConsoleDesign("4. Encrypt to   DIVAFILE");
            KKdMain.ConsoleDesign("5. DB_Tools");
            KKdMain.ConsoleDesign("6. AC/DT/F/AFT/FT Converting Tools");
            KKdMain.ConsoleDesign("7. F/F2/X/FT      Converting Tools");
            KKdMain.ConsoleDesign(JSON ? "8. MsgPack to JSON" : "9. JSON to MsgPack");
            KKdMain.ConsoleDesign(false);
            KKdMain.ConsoleDesign(JSON ? "M. MessagePack" : "J. JSON");
            KKdMain.ConsoleDesign("Q. Quit");
            KKdMain.ConsoleDesign(false);
            KKdMain.ConsoleDesign(true);
            Console.WriteLine();

            function = Console.ReadLine().ToUpper();
            bool isNumber = int.TryParse(function, out int result);
            if (isNumber) Functions();
                 if (function == "M") JSON = false;
            else if (function == "J") JSON = true ;
        }

        private static void Functions()
        {
            Console.Clear();
                 if (function == "1" || function == "2") FARC.Processor(function == "1");
            else if (function == "3" || function == "4")
            {
                Program.Choose(1, "", out string[] FileNames);
                foreach (string FileName in FileNames) DIVAFILE.Decrypt(FileName);
            }
            else if (function == "5") DataBase.Processor(JSON);
            else if (function == "6")
            {
                Console.Clear();
                Console.Title = "AC/DT/F/AFT/FT Converting Tools";
                KKdMain.ConsoleDesign(true);
                KKdMain.ConsoleDesign("               Choose converter:");
                KKdMain.ConsoleDesign(false);
                KKdMain.ConsoleDesign("1. A3DA"    );
                KKdMain.ConsoleDesign("2. AET"     );
                KKdMain.ConsoleDesign("3. DataBank");
                KKdMain.ConsoleDesign("4. DEX"     );
                KKdMain.ConsoleDesign("5. DIVA"    );
                KKdMain.ConsoleDesign("6. MOT"     );
                KKdMain.ConsoleDesign("7. STR"     );
                KKdMain.ConsoleDesign(false);
                KKdMain.ConsoleDesign("R. Return to Main Menu");
                KKdMain.ConsoleDesign(false);
                KKdMain.ConsoleDesign(true);
                Console.WriteLine();
                string Function = Console.ReadLine();
                Console.Clear();
                     if (Function == "1") Tools.A3D.Processor(JSON);
                else if (Function == "2") Tools.AET.Processor(JSON);
                else if (Function == "3") Tools.DB .Processor(JSON);
                else if (Function == "4") Tools.DEX.Processor(JSON);
                else if (Function == "5") Tools.DIV.Processor();
                else if (Function == "6") Tools.MOT.Processor(JSON);
                else if (Function == "7") Tools.STR.Processor(JSON);
                else     function = Function;
            }
            else if (function == "7")
            {
                Console.Clear();
                Console.Title = "F/F2/X/FT Converting Tools";
                KKdMain.ConsoleDesign(true);
                KKdMain.ConsoleDesign("               Choose converter:");
                KKdMain.ConsoleDesign(false);
                KKdMain.ConsoleDesign("1. A3DA"            );
                KKdMain.ConsoleDesign("2. Bloom"           );
                KKdMain.ConsoleDesign("3. Color Correction");
                KKdMain.ConsoleDesign("4. DEX"             );
                KKdMain.ConsoleDesign("5. DOF"             );
                KKdMain.ConsoleDesign("6. Light"           );
                KKdMain.ConsoleDesign("7. STR"             );
                KKdMain.ConsoleDesign("8. VAG"             );
                KKdMain.ConsoleDesign(false);
                KKdMain.ConsoleDesign("R. Return to Main Menu");
                KKdMain.ConsoleDesign(false);
                KKdMain.ConsoleDesign(true);
                Console.WriteLine();
                string Function = Console.ReadLine();
                Console.Clear();
                     if (Function == "1") Tools.A3D.Processor(JSON);
                else if (Function == "2") Tools.BLT.Processor();
                else if (Function == "3") Tools.CCT.Processor();
                else if (Function == "4") Tools.DEX.Processor(JSON);
                else if (Function == "5") Tools.DFT.Processor();
                else if (Function == "6") Tools.LIT.Processor();
                else if (Function == "7") Tools.STR.Processor(JSON);
                else if (Function == "8") Tools.VAG.Processor();
                else     function = Function;
            }
            else if (function == "8")
            {
                Program.Choose(1, JSON ? "mp" : "json", out string[] FileNames);
                foreach (string file in FileNames)
                    if (JSON)
                    {
                        Console.Title = "MsgPack to JSON: " + Path.GetFileNameWithoutExtension(file);
                        MPExt.ToJSON   (file.Replace(Path.GetExtension(file), ""));
                    }
                    else
                    {
                        Console.Title = "JSON to MsgPack: " + Path.GetFileNameWithoutExtension(file);
                        MPExt.ToMsgPack(file.Replace(Path.GetExtension(file), ""));
                    }
            }
        }

        public static void Exit() => Environment.Exit(0);

        private static string GetArgs(string name, bool And, params string[] ext)
        {
            string Out = "";
            if (And) Out = "|";
            Out += name + " files (";
            for (int i = 0; i < ext.Length; i++)
            { Out += "*." + ext[i]; if (i + 1 < ext.Length) Out += ", "; }
            Out += ")|";
            for (int i = 0; i < ext.Length; i++)
            { Out += "*." + ext[i]; if (i + 1 < ext.Length) Out += ";"; }

            return Out;
        }

        private static string GetArgs(string name, params string[] ext)
        {
            string Out = name + " files (";
            for (int i = 0; i < ext.Length; i++)
            { Out += "*." + ext[i]; if (i + 1 < ext.Length) Out += ", "; }
            Out += ")|";
            for (int i = 0; i < ext.Length; i++)
            { Out += "*." + ext[i]; if (i + 1 < ext.Length) Out += ";"; }

            return Out;
        }

        public static string Choose(int code, string filetype, out string[] FileNames)
        {
            string MsgPack = GetArgs("MessagePack", true, "mp");
            string JSON = GetArgs("JSON", true, "json");
            string BIN = GetArgs("BIN", true, "bin");
            string WAV = GetArgs("WAV", true, "wav");

            FileNames = new string[0];
            if (code == 1)
            {
                OpenFileDialog ofd = new OpenFileDialog { InitialDirectory = Application.StartupPath,
                    Multiselect = true, Title = "Choose file(s) to open:" };

                ofd.Filter = GetArgs("All;", false, "*");
                     if (filetype == "a3da") ofd.Filter = GetArgs("A3DA", "a3da", "farc", "json", "mp") +
                        GetArgs("A3DA", true, "a3da") +   GetArgs("FARC", true, "farc") + JSON + MsgPack;
                else if (filetype == "bin" ) ofd.Filter = GetArgs("BIN" , "bin", "json", "mp") +
                        BIN + JSON + MsgPack;
                else if (filetype == "blt" ) ofd.Filter = GetArgs("BLT" , "blt");
                else if (filetype == "cct" ) ofd.Filter = GetArgs("CCT" , "cct");
                else if (filetype == "databank") ofd.Filter = GetArgs("DAT", "dat", "json", "mp") +
                        GetArgs("DAT", true, "dat") + JSON + MsgPack;
                else if (filetype == "dex" ) ofd.Filter = GetArgs("DEX" , "dex", "bin", "json", "mp") +
                        GetArgs("DEX", true, "dex") + BIN + JSON + MsgPack;
                else if (filetype == "dft")  ofd.Filter = GetArgs("DFT" , "dft");
                else if (filetype == "diva") ofd.Filter = GetArgs("DIVA", "diva", "wav") +
                        GetArgs("DIVA", true, "diva") + GetArgs("WAV", true, "wav");
                else if (filetype == "farc") ofd.Filter = "FARC Archives (*.farc)|*.farc";
                else if (filetype == "json") ofd.Filter = "JSON (*.json)|*.json";
                else if (filetype == "mp"  ) ofd.Filter = GetArgs("MessagePack", "mp");
                else if (filetype == "lit")  ofd.Filter = GetArgs("LIT" , "lit");
                else if (filetype == "str" ) ofd.Filter = GetArgs("STR" , "str", "bin", "json", "mp") +
                        GetArgs("STR", true, "str") + BIN + JSON + MsgPack;
                else if (filetype == "vag" ) ofd.Filter = GetArgs("VAG" , "vag", "wav") +
                        GetArgs("VAG", true, "vag") + GetArgs("WAV", true, "wav");

                if (ofd.ShowDialog() == DialogResult.OK) FileNames = ofd.FileNames;
                ofd.Dispose();
            }
            else if (code == 2)
            {
                OpenFileDialog ofd = new OpenFileDialog { InitialDirectory = Application.StartupPath,
                    ValidateNames = false, CheckFileExists = false, Filter = " | ", CheckPathExists = true,
                    Title = "Choose any file in folder:", FileName = "Folder Selection." };
                string Return = "";
                if (ofd.ShowDialog() == DialogResult.OK)
                    Return = Path.GetDirectoryName(ofd.FileName);
                ofd.Dispose();
                return Return;
            }
            return "";
        }
    }
}