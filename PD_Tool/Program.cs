using System;
using System.Windows.Forms;
using KKdMainLib;
using KKdMainLib.IO;
using KKdFARC = KKdMainLib.FARC;

namespace PD_Tool
{
    public static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [ThreadStatic] public static string choose = "";

        [STAThread]
        public static void Main(string[] args)
        {
            SetProcessDPIAware();

            Console.Title = "PD_Tool";
            if (args.Length == 0) { while (choose != "Q") MainMenu(); Exit(); }

            long header;
            Stream reader;
            foreach (string arg in args)
            {
                GC.Collect();
                     if (Directory.Exists(arg)) using (KKdFARC FARC = new KKdFARC(arg, true)) FARC.Pack();
                else if (File.Exists(arg) && Path.GetExtension(arg) == ".farc")
                    using (KKdFARC farc = new KKdFARC(arg)) farc.Unpack(true);
                else if (File.Exists(arg))
                {
                    using (reader = File.OpenReader(arg))
                        header = reader.RI64();
                    if (header == 0x454C494641564944) KKdMainLib.DIVAFILE.Decrypt(arg);
                }
            }
            Exit();
        }

        [ThreadStatic] private static bool json = true;

        private static void MainMenu()
        {
            Console. InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.Title = "PD_Tool";
            Console.Clear();

            ConsoleDesign(true);
            ConsoleDesign("                Choose action:");
            ConsoleDesign(false);
            ConsoleDesign("1. Extract FARC Archive");
            ConsoleDesign("2. Create  FARC Archive");
            ConsoleDesign("3. Decrypt from DIVAFILE");
            ConsoleDesign("4. Encrypt to   DIVAFILE");
            ConsoleDesign("5. DB_Tools");
            ConsoleDesign("6. AC/DT/F/AFT/FT Converting Tools");
            ConsoleDesign("7. F/F2/FT        Converting Tools");
            ConsoleDesign("8. X/XHD          Converting Tools");
            ConsoleDesign(json ? "9. MsgPack to JSON" : "9. JSON to MsgPack");
            ConsoleDesign(false);
            ConsoleDesign(json ? "M. MessagePack" : "J. JSON");
            ConsoleDesign("Q. Quit");
            ConsoleDesign(false);
            ConsoleDesign(true);
            Console.WriteLine();

            choose = Console.ReadLine().ToUpper();
            bool isNumber = int.TryParse(choose, out int result);
            if (isNumber) Functions();
                 if (choose == "M") json = false;
            else if (choose == "J") json = true ;
        }

        private static void Functions()
        {
            Console.Clear();
                 if (choose == "1" || choose == "2") FARC.Processor(choose == "1");
            else if (choose == "3" || choose == "4")
            {
                Choose(1, "", out string[] FileNames);
                foreach (string FileName in FileNames) DIVAFILE.Decrypt(FileName);
            }
            else if (choose == "5") DataBase.Processor(json);
            else if (choose == "6")
            {
                Console.Clear();
                Console.Title = "AC/DT/F/AFT/FT Converting Tools";
                ConsoleDesign(true);
                ConsoleDesign("               Choose converter:");
                ConsoleDesign(false);
                ConsoleDesign("1. A3DA"    );
                ConsoleDesign("2. AET"     );
                ConsoleDesign("3. DataBank");
                ConsoleDesign("4. DEX"     );
                ConsoleDesign("5. DIVA"    );
                ConsoleDesign("6. MotHead" );
                ConsoleDesign("7. MOT"     );
                ConsoleDesign("8. Table"   );
                ConsoleDesign("9. STR"     );
                ConsoleDesign(false);
                ConsoleDesign("R. Return to Main Menu");
                ConsoleDesign(false);
                ConsoleDesign(true);
                Console.WriteLine();
                string localChoose = Console.ReadLine().ToUpper();
                     if (localChoose == "1") A3D.Processor(json);
                else if (localChoose == "2") AET.Processor(json);
                else if (localChoose == "3") DB .Processor(json);
                else if (localChoose == "4") DEX.Processor(json);
                else if (localChoose == "5") DIV.Processor();
                else if (localChoose == "6") MHD.Processor(json);
                else if (localChoose == "7") MOT.Processor(json);
                else if (localChoose == "8") TBL.Processor(json);
                else if (localChoose == "9") STR.Processor(json);
                else choose = localChoose;
            }
            else if (choose == "7")
            {
                Console.Clear();
                Console.Title = "F/F2/FT Converting Tools";
                ConsoleDesign(true);
                ConsoleDesign("               Choose converter:");
                ConsoleDesign(false);
                ConsoleDesign("1. A3DA"            );
                ConsoleDesign("2. Bloom"           );
                ConsoleDesign("3. Color Correction");
                ConsoleDesign("4. DEX"             );
                ConsoleDesign("5. DOF"             );
                ConsoleDesign("6. Light"           );
                ConsoleDesign("7. STR"             );
                ConsoleDesign("8. VAG"             );
                ConsoleDesign(false);
                ConsoleDesign("R. Return to Main Menu");
                ConsoleDesign(false);
                ConsoleDesign(true);
                Console.WriteLine();
                string localChoose = Console.ReadLine().ToUpper();
                     if (localChoose == "1") A3D.Processor(json);
                else if (localChoose == "2") BLT.Processor();
                else if (localChoose == "3") CCT.Processor();
                else if (localChoose == "4") DEX.Processor(json);
                else if (localChoose == "5") DFT.Processor();
                else if (localChoose == "6") LIT.Processor();
                else if (localChoose == "7") STR.Processor(json);
                else if (localChoose == "8") VAG.Processor();
                else choose = localChoose;
            }
            else if (choose == "8")
            {
                Console.Clear();
                Console.Title = "X Converting Tools";
                ConsoleDesign(true);
                ConsoleDesign("               Choose converter:");
                ConsoleDesign(false);
                ConsoleDesign("1. A3DA"          );
                ConsoleDesign("2. DEX"           );
                ConsoleDesign("3. VAG"           );
                ConsoleDesign(false);
                ConsoleDesign("R. Return to Main Menu");
                ConsoleDesign(false);
                ConsoleDesign(true);
                Console.WriteLine();
                string localChoose = Console.ReadLine().ToUpper();
                     if (localChoose == "1") A3D.Processor(json);
                else if (localChoose == "2") DEX.Processor(json);
                else if (localChoose == "3") VAG.Processor();
                else choose = localChoose;
            }
            else if (choose == "9")
            {
                Console.Title = json ? "MsgPack to JSON" : "JSON to MsgPack";
                Choose(1, json ? "mp" : "json", out string[] fileNames);
                foreach (string file in fileNames)
                    if (json)
                    {
                        Console.Title = "MsgPack to JSON: " + Path.GetFileNameWithoutExtension(file);
                        file.Replace(Path.GetExtension(file), "").ToJSON   ();
                    }
                    else
                    {
                        Console.Title = "JSON to MsgPack: " + Path.GetFileNameWithoutExtension(file);
                        file.Replace(Path.GetExtension(file), "").ToMsgPack();
                    }
            }
        }

        public static void Exit() => Environment.Exit(0);
        
        public static void ConsoleDesign(string text, params string[] args)
        {
            text = string.Format(text, args);
            string Text = "█                                                  █";
            Text = Text.Remove(3) + text + Text.Remove(0, text.Length + 3);
            Console.WriteLine(Text);
        }

        public static void ConsoleDesign(bool Fill)
        {
            if (Fill) Console.WriteLine("████████████████████████████████████████████████████");
            else      Console.WriteLine("█                                                  █");
        }

        private static string GetArgs(string name, bool And, params string[] ext)
        {
            int L = ext.Length;
            string Out = (And ? "|" : "") + name + " files (";
            for (int i = 0; i < L; i++) { Out += "*." + ext[i]; if (i + 1 < L) Out += ", "; }
            Out += ")|";
            for (int i = 0; i < L; i++) { Out += "*." + ext[i]; if (i + 1 < L) Out += ";" ; }
            return Out;
        }

        private static string GetArgs(string name, params string[] ext)
        {
            int L = ext.Length;
            string Out = name + " files (";
            for (int i = 0; i < L; i++) { Out += "*." + ext[i]; if (i + 1 < L) Out += ", "; }
            Out += ")|";
            for (int i = 0; i < L; i++) { Out += "*." + ext[i]; if (i + 1 < L) Out += ";" ; }
            return Out;
        }

        public static string Choose(int code, string filetype, out string[] FileNames)
        {
            string mp   = GetArgs("MessagePack", true, "mp"  );
            string json = GetArgs("JSON"       , true, "json");
            string bin  = GetArgs("BIN"        , true, "bin" );
            string wav  = GetArgs("WAV"        , true, "wav" );

            FileNames = new string[0];
            if (code == 1)
            {
                string Filter = GetArgs("All;", false, "*");
                     if (filetype == "a3da") Filter = GetArgs("A3DA", "a3da", "farc", "json", "mp") +
                        GetArgs("A3DA", true, "a3da") +   GetArgs("FARC", true, "farc") + json + mp;
                else if (filetype == "bin" ) Filter = GetArgs("BIN" , "bin", "json", "mp") +
                        bin + json + mp;
                else if (filetype == "blt" ) Filter = GetArgs("BLT" , "blt");
                else if (filetype == "bon" ) Filter = GetArgs("BON" , "bon", "bin", "json", "mp") +
                        GetArgs("BON", true, "bon") + bin + json + mp;
                else if (filetype == "cct" ) Filter = GetArgs("CCT" , "cct");
                else if (filetype == "databank") Filter = GetArgs("DAT", "dat", "json", "mp") +
                        GetArgs("DAT", true, "dat") + json + mp;
                else if (filetype == "dex" ) Filter = GetArgs("DEX" , "dex", "bin", "json", "mp") +
                        GetArgs("DEX", true, "dex") + bin + json + mp;
                else if (filetype == "dft" ) Filter = GetArgs("DFT" , "dft");
                else if (filetype == "diva") Filter = GetArgs("DIVA", "diva", "wav") +
                        GetArgs("DIVA", true, "diva") + wav;
                else if (filetype == "farc") Filter = GetArgs("FARC", "farc");
                else if (filetype == "json") Filter = GetArgs("JSON", "json");
                else if (filetype == "mp"  ) Filter = GetArgs("MessagePack", "mp");
                else if (filetype == "lit")  Filter = GetArgs("LIT" , "lit");
                else if (filetype == "str" ) Filter = GetArgs("STR" , "str", "bin", "json", "mp") +
                        GetArgs("STR", true, "str") + bin + json + mp;
                else if (filetype == "tbl" ) Filter = GetArgs("Table" , "bin", "farc", "json", "mp") +
                        bin + GetArgs("FARC Archives", true, "farc") + json + mp;
                else if (filetype == "vag" ) Filter = GetArgs("VAG" , "vag", "wav") +
                        GetArgs("VAG", true, "vag") + wav;
                
                using OpenFileDialog ofd = new OpenFileDialog { //InitialDirectory = Application.StartupPath,
                    Filter = Filter, Multiselect = true, Title = "Choose file(s) to open:" };
                if (ofd.ShowDialog() == DialogResult.OK) FileNames = ofd.FileNames;
            }
            else if (code == 2)
            {
                string Return = "";
                using (OpenFileDialog ofd = new OpenFileDialog { //InitialDirectory = Application.StartupPath,
                    ValidateNames = false, CheckFileExists = false, Filter = " | ", CheckPathExists = true,
                    Title = "Choose any file in folder:", FileName = "Folder Selection." })
                    if (ofd.ShowDialog() == DialogResult.OK) Return = Path.GetDirectoryName(ofd.FileName);
                return Return;
            }
            return "";
        }
    }
}