using System;
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
                KKdMain.Choose(1, "", out string[] FileNames);
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
                KKdMain.Choose(1, JSON ? "mp" : "json", out string[] FileNames);
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
    }
}