using System;
using System.Collections.Generic;
using KKdMainLib.IO;
using MSIO = System.IO;
using KKdMain = KKdMainLib.Main;
using KKdFARC = KKdMainLib.FARC;

namespace PD_Tool
{
    public static class Program
    {
        public static string function = "";
        public static List<string> ProcessedFiles = new List<string>();

        [STAThread]
        public static void Main(string[] args)
        {
            Console.Title = "PD_Tool";
            
            if (args.Length == 0)
            {
                while (function != "Q") MainMenu();
                Exit();
            }

            string header;
            Stream reader;
            KKdFARC Farc;

            foreach (string arg in args)
            {
                Farc = new KKdFARC();
                     if (MSIO.Directory.Exists(arg)) Farc.Pack(arg);
                else if (MSIO.File.Exists(arg) && MSIO.Path.GetExtension(arg) == ".farc") Farc.UnPack(arg, true);
                else if (MSIO.File.Exists(arg))
                {
                    reader = File.OpenReader(arg);
                    header = reader.ReadString(8);
                    reader.Close();
                    if (header.ToUpper() == "DIVAFILE") DIVAFILE.Decrypt(arg);
                }
            }
            Exit();
        }

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
            KKdMain.ConsoleDesign(false);
            KKdMain.ConsoleDesign("Q. Quit");
            KKdMain.ConsoleDesign(false);
            KKdMain.ConsoleDesign(true);
            Console.WriteLine();

            function = Console.ReadLine().ToUpper();
            bool isNumber = int.TryParse(function, out int result);
            if (isNumber) Functions();
        }

        private static void Functions()
        {
            Console.Clear();
            if (function == "1" || function == "2")
                FARC.Processor(function == "1");
            else if (function == "3" || function == "4")
            {
                KKdMain.Choose(1, "", out string[] FileNames);
                foreach (string FileName in FileNames) DIVAFILE.Decrypt(FileName);
            }
            else if (function == "5") DataBase.Processor();
            else if (function == "6")
            {
                Console.Clear();
                Console.Title = "Converter Tools";
                KKdMain.ConsoleDesign(true);
                KKdMain.ConsoleDesign("                 Choose tool:");
                KKdMain.ConsoleDesign(false);
                KKdMain.ConsoleDesign("1. A3DA Converter");
                KKdMain.ConsoleDesign("2. DEX  Converter");
                KKdMain.ConsoleDesign("3. DIVA Converter");
                KKdMain.ConsoleDesign("4. STR  Converter");
                KKdMain.ConsoleDesign("5. VAG  Converter");
                KKdMain.ConsoleDesign(false);
                KKdMain.ConsoleDesign("R. Return to Main Menu");
                KKdMain.ConsoleDesign("Q. Quit");
                KKdMain.ConsoleDesign(false);
                KKdMain.ConsoleDesign(true);
                Console.WriteLine();
                string Function = Console.ReadLine();
                Console.Clear();
                     if (Function == "1") Tools.A3D.Processor();
                else if (Function == "2") Tools.DEX.Processor();
                else if (Function == "3") Tools.DIV.Processor();
                else if (Function == "4") Tools.STR.Processor();
                else if (Function == "5") Tools.VAG.Processor();
                else     function = Function;
            }
        }

        public static void Exit() => Environment.Exit(0);
    }
}