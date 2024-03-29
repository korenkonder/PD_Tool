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

            Console. InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            Console.Title = "PD_Tool";
            if (args.Length == 0) { while (choose != "Q") MainMenu(); Exit(); }

            long header;
            Stream reader;
            foreach (string arg in args)
            {
                GC.Collect();
                if (Directory.Exists(arg))
                    using (KKdFARC FARC = new KKdFARC(arg, true) { CompressionLevel = 9 }) FARC.Pack();
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
            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.Title = $"PD_Tool v{ver}";
            Console.Clear();

            ConsoleDesign(true);
            ConsoleDesign("                Choose action:");
            ConsoleDesign(false);
            ConsoleDesign("1. Extract FARC Archive");
            ConsoleDesign("2. Create FARC Archive");
            ConsoleDesign("3. Decrypt from DIVAFILE");
            ConsoleDesign("4. Encrypt to DIVAFILE");
            ConsoleDesign("5. DB_Tools");
            ConsoleDesign("6. AC/DT/F/AFT Converting Tools");
            ConsoleDesign("7. F/F2/FT     Converting Tools");
            ConsoleDesign("8. X/XHD/VRFL  Converting Tools");
            ConsoleDesign("9. FT/M39      Converting Tools");
            ConsoleDesign(json ? "A. MsgPack to JSON" : "A. JSON to MsgPack");
            ConsoleDesign(false);
            ConsoleDesign(json ? "M. MessagePack" : "J. JSON");
            ConsoleDesign("Q. Quit");
            ConsoleDesign(false);
            ConsoleDesign(true);
            Console.WriteLine();

            choose = Console.ReadLine().ToUpper();
            bool isNumber = int.TryParse(choose, out int result);
            if (isNumber) Functions();

            if (choose == null || choose == "") return;
                 if (choose[0] == 'M') json = false;
            else if (choose[0] == 'J') json = true ;
            else if (choose[0] >= 'A' && choose[0] <= 'A') Functions();
        }

        private static void Functions()
        {
            Console.Clear();
            if (choose == null || choose == "") return;

                 if (choose[0] == '1' || choose[0] == '2') FARC.Processor(choose == "1");
            else if (choose[0] == '3')
            {
                Choose(1, "", out string[] FileNames);
                foreach (string FileName in FileNames) DIVAFILE.Decrypt(FileName);
            }
            else if (choose[0] == '4')
            {
                Choose(1, "", out string[] FileNames);
                foreach (string FileName in FileNames) DIVAFILE.Encrypt(FileName);
            }
            else if (choose[0] == '5')
                DataBase.Processor(json, choose.Length > 1 ? choose[1] : '\0');
            else if (choose[0] == '6')
            {
                string localChoose = "";
                if (choose.Length == 1)
                {
                    Console.Clear();
                    Console.Title = "AC/DT/F/AFT Converting Tools";
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
                    ConsoleDesign("8. STR"     );
                    ConsoleDesign("9. Table"   );
                    ConsoleDesign(false);
                    ConsoleDesign("R. Return to Main Menu");
                    ConsoleDesign(false);
                    ConsoleDesign(true);
                    Console.WriteLine();
                    localChoose = Console.ReadLine().ToUpper();
                }
                else localChoose = choose[1].ToString();
                     if (localChoose == "1") A3D.Processor(json);
                else if (localChoose == "2") AET.Processor(json);
                else if (localChoose == "3") DB .Processor(json);
                else if (localChoose == "4") DEX.Processor(json);
                else if (localChoose == "5") DIV.Processor();
                else if (localChoose == "6") MHD.Processor(json);
                else if (localChoose == "7") MOT.Processor(json);
                else if (localChoose == "8") STR.Processor(json);
                else if (localChoose == "9") TBL.Processor(json);
                else choose = localChoose;
            }
            else if (choose[0] == '7')
            {
                string localChoose = "";
                if (choose.Length == 1)
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
                    localChoose = Console.ReadLine().ToUpper();
                }
                else localChoose = choose[1].ToString();
                     if (localChoose == "1") A3D.Processor(json);
                else if (localChoose == "2") BLT.Processor(json);
                else if (localChoose == "3") CCT.Processor(json);
                else if (localChoose == "4") DEX.Processor(json);
                else if (localChoose == "5") DFT.Processor(json);
                else if (localChoose == "6") LIT.Processor(json);
                else if (localChoose == "7") STR.Processor(json);
                else if (localChoose == "8") VAG.Processor();
                else choose = localChoose;
            }
            else if (choose[0] == '8')
            {
                string localChoose = "";
                if (choose.Length == 1)
                {
                    Console.Clear();
                    Console.Title = "X/XHD/VRFL Converting Tools";
                    ConsoleDesign(true);
                    ConsoleDesign("               Choose converter:");
                    ConsoleDesign(false);
                    ConsoleDesign("1. A3DA");
                    ConsoleDesign("2. Bloom"           );
                    ConsoleDesign("3. Color Correction");
                    ConsoleDesign("4. DEX" );
                    ConsoleDesign("5. DOF"             );
                    ConsoleDesign("6. Light"           );
                    ConsoleDesign("7. VAG" );
                    ConsoleDesign(false);
                    ConsoleDesign("R. Return to Main Menu");
                    ConsoleDesign(false);
                    ConsoleDesign(true);
                    Console.WriteLine();
                    localChoose = Console.ReadLine().ToUpper();
                }
                else localChoose = choose[1].ToString();
                     if (localChoose == "1") A3D.Processor(json);
                else if (localChoose == "2") BLT.Processor(json);
                else if (localChoose == "3") CCT.Processor(json);
                else if (localChoose == "4") DEX.Processor(json);
                else if (localChoose == "5") DFT.Processor(json);
                else if (localChoose == "6") LIT.Processor(json);
                else if (localChoose == "7") VAG.Processor();
                else choose = localChoose;
            }
            else if (choose[0] == '9')
            {
                string localChoose = "";
                if (choose.Length == 1)
                {
                    Console.Clear();
                    Console.Title = "FT/M39 Converting Tools";
                    ConsoleDesign(true);
                    ConsoleDesign("               Choose converter:");
                    ConsoleDesign(false);
                    ConsoleDesign("1. A3DA"    );
                    ConsoleDesign("2. AET"     );
                    ConsoleDesign("3. DEX"     );
                    ConsoleDesign("4. DIVA"    );
                    ConsoleDesign("5. MotHead" );
                    ConsoleDesign("6. MOT"     );
                    ConsoleDesign("7. STR"     );
                    ConsoleDesign("8. Table"   );
                    ConsoleDesign(false);
                    ConsoleDesign("R. Return to Main Menu");
                    ConsoleDesign(false);
                    ConsoleDesign(true);
                    Console.WriteLine();
                    localChoose = Console.ReadLine().ToUpper();
                }
                else localChoose = choose[1].ToString();
                     if (localChoose == "1") A3D.Processor(json);
                else if (localChoose == "2") AET.Processor(json);
                else if (localChoose == "3") DEX.Processor(json);
                else if (localChoose == "4") DIV.Processor();
                else if (localChoose == "5") MHD.Processor(json);
                else if (localChoose == "6") MOT.Processor(json);
                else if (localChoose == "7") STR.Processor(json);
                else if (localChoose == "8") TBL.Processor(json);
                else choose = localChoose;
            }
            else if (choose[0] == 'A')
            {
                Console.Title = json ? "MsgPack to JSON" : "JSON to MsgPack";
                Choose(1, json ? "mp" : "json", out string[] fileNames);
                foreach (string file in fileNames)
                    if (json)
                    {
                        Console.Title = "MsgPack to JSON: " + Path.GetFileNameWithoutExtension(file);
                        Path.RemoveExtension(file).ToJSON   ();
                    }
                    else
                    {
                        Console.Title = "JSON to MsgPack: " + Path.GetFileNameWithoutExtension(file);
                        Path.RemoveExtension(file).ToMsgPack();
                    }
            }
        }

        public static void Exit() => Environment.Exit(0);

        public static void ConsoleDesign(string text, params string[] args)
        {
            text = string.Format(text, args);
            string Text = "X                                                  X";
            Text = Text.Remove(3) + text + Text.Remove(0, text.Length + 3);
            Console.WriteLine(Text);
        }

        public static void ConsoleDesign(bool Fill)
        {
            if (Fill) Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            else      Console.WriteLine("X                                                  X");
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
            FileNames = new string[0];
            if (code == 1)
            {
                string mp   = GetArgs("MessagePack", true, "mp"  );
                string json = GetArgs("JSON"       , true, "json");
                string bin  = GetArgs("BIN"        , true, "bin" );
                string wav  = GetArgs("WAV"        , true, "wav" );

                string Filter = GetArgs("All", false, "*");
                     if (filetype == "a3da") Filter = GetArgs("A3DA", "a3da", "farc", "json", "mp") +
                        GetArgs("A3DA", true, "a3da") +   GetArgs("FARC", true, "farc") + json + mp;
                else if (filetype == "bin" ) Filter = GetArgs("BIN" , "bin", "json", "mp") +
                        bin + json + mp;
                else if (filetype == "blt" ) Filter = GetArgs("BLT" , "blt", "json", "mp") +
                        GetArgs("BLT", true, "blt") + json + mp;
                else if (filetype == "bon" ) Filter = GetArgs("BON" , "bon", "bin", "json", "mp") +
                        GetArgs("BON", true, "bon") + bin + json + mp;
                else if (filetype == "cct" ) Filter = GetArgs("CCT" , "cct", "json", "mp") +
                        GetArgs("CCT", true, "cct") + json + mp;
                else if (filetype == "databank") Filter = GetArgs("DAT", "dat", "json", "mp") +
                        GetArgs("DAT", true, "dat") + json + mp;
                else if (filetype == "dex" ) Filter = GetArgs("DEX" , "dex", "bin", "json", "mp") +
                        GetArgs("DEX", true, "dex") + bin + json + mp;
                else if (filetype == "dft" ) Filter = GetArgs("DFT" , "dft", "json", "mp") +
                        GetArgs("DFT", true, "dft") + json + mp;
                else if (filetype == "diva") Filter = GetArgs("DIVA", "diva", "wav") +
                        GetArgs("DIVA", true, "diva") + wav;
                else if (filetype == "dsc" ) Filter = GetArgs("DSC" , "dsc", "json", "mp") +
                        GetArgs("DSC", true, "dsc") + json + mp;
                else if (filetype == "dve" ) Filter = GetArgs("Particle", "dve", "farc", "json", "mp") +
                        GetArgs("Particle", true, "dve") + GetArgs("Particle", true, "farc") + json + mp;
                else if (filetype == "farc") Filter = GetArgs("FARC", "farc");
                else if (filetype == "json") Filter = GetArgs("JSON", "json");
                else if (filetype == "igb" ) Filter = GetArgs("IGB", "igb");
                else if (filetype == "mgftxt") Filter = GetArgs("MGF2AFT txt", "txt");
                else if (filetype == "mhd" ) Filter = GetArgs("MotHead", "mhd", "bin", "json", "mp") +
                        GetArgs("MotHead", true, "mhd") + bin + json + mp;
                else if (filetype == "mp"  ) Filter = GetArgs("MessagePack", "mp");
                else if (filetype == "lit")  Filter = GetArgs("LIT" , "lit", "json", "mp") +
                        GetArgs("LIT", true, "lit") + json + mp;
                else if (filetype == "pvdb") Filter = GetArgs("PV DB / PV Field", "txt", "json", "mp") +
                        GetArgs("PV DB / PV Field", true, "txt") + json + mp;
                else if (filetype == "str" ) Filter = GetArgs("STR" , "str", "bin", "json", "mp") +
                        GetArgs("STR", true, "str") + bin + json + mp;
                else if (filetype == "tbl" ) Filter = GetArgs("Table" , "bin", "farc", "json", "mp") +
                        bin + GetArgs("FARC Archives", true, "farc") + json + mp;
                else if (filetype == "vag" ) Filter = GetArgs("VAG" , "vag", "wav") +
                        GetArgs("VAG", true, "vag") + wav;

                using OpenFileDialog ofd = new OpenFileDialog { Filter = Filter,
                    Multiselect = true, Title = "Choose file(s) to open:" };
                if (ofd.ShowDialog() == DialogResult.OK) FileNames = ofd.FileNames;
            }
            else if (code == 2)
            {
                string Return = "";
                using (OpenFileDialog ofd = new OpenFileDialog { ValidateNames = false,
                    CheckFileExists = false, Filter = " | ", CheckPathExists = true,
                    Title = "Choose any file in folder:", FileName = "Folder Selection." })
                    if (ofd.ShowDialog() == DialogResult.OK) Return = Path.GetDirectoryName(ofd.FileName);
                return Return;
            }
            return "";
        }
    }
}