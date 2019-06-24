using System;
using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.Types;
using KKdMain = KKdMainLib.Main;
using KKdFARC = KKdMainLib.FARC;

namespace PD_Tool
{
    public static class Program
    {
        public static string function = "";

        [STAThread]
        public static void Main(string[] args)
        {
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

        private static bool JSON = true;

        private static void MainMenu()
        {
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
            KKdMain.ConsoleDesign("6. Converting Tools");
            KKdMain.ConsoleDesign(JSON ? "7. MsgPack to JSON" : "7. JSON to MsgPack");
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
                Console.Title = "Converter Tools";
                KKdMain.ConsoleDesign(true);
                KKdMain.ConsoleDesign("                 Choose tool:");
                KKdMain.ConsoleDesign(false);
                KKdMain.ConsoleDesign("1. A3DA     Converter");
                KKdMain.ConsoleDesign("2. AET      Converter");
                KKdMain.ConsoleDesign("3. DataBank Converter");
                KKdMain.ConsoleDesign("4. DEX      Converter");
                KKdMain.ConsoleDesign("5. DIVA     Converter");
                KKdMain.ConsoleDesign("6. STR      Converter");
                KKdMain.ConsoleDesign("7. VAG      Converter");
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
                else if (Function == "6") Tools.STR.Processor(JSON);
                else if (Function == "7") Tools.VAG.Processor();
                else     function = Function;
            }
            else if (function == "7")
            {
                KKdMain.Choose(1, JSON ? "mp" : "json", out string[] FileNames);
                if (JSON) foreach (string file in FileNames)
                        KKdMainLib.MessagePack.MPExt.ToJSON   (file.Replace(Path.GetExtension(file), ""));
                else      foreach (string file in FileNames)
                        KKdMainLib.MessagePack.MPExt.ToMsgPack(file.Replace(Path.GetExtension(file), ""));
            }
        }

        public static void Exit() => Environment.Exit(0);
    }
}