using System;
using System.IO;
using KKdMainLib;
using KKdFARC = KKdMainLib.FARC;

namespace PD_Tool
{
    public class FARC
    {
        public static void Processor(bool Extract)
        {
            KKdFARC FARC = new KKdFARC();
            Console.Clear();
            if (Extract)
            {
                Console.Title = "FARC Extractor";
                Program.Choose(1, "farc", out string[] FileNames);
                foreach (string file in FileNames)
                    if (file != "" && File.Exists(file))
                    {
                        Console.Title = "FARC Extractor: " + Path.GetFileNameWithoutExtension(file);
                        new KKdFARC(file).UnPack();
                    }
            }
            else
            {
                string file = Program.Choose(2, "", out string[] FileNames);
                Console.Clear();
                Console.Title = "FARC Creator";
                if (file != "")
                {
                    Console.Title = "FARC Creator: " + Path.GetDirectoryName(file);
                    FARC = new KKdFARC();
                    Program.ConsoleDesign(true);
                    Program.ConsoleDesign("         Choose type of created FARC:");
                    Program.ConsoleDesign(false);
                    Program.ConsoleDesign("1. FArc [DT/DT2/DTex/F/F2/X]");
                    Program.ConsoleDesign("2. FArC [DT/DT2/DTex/F/F2/X] (Compressed)");
                    Program.ConsoleDesign("3. FARC [F/F2/X]");
                    Program.ConsoleDesign(false);
                    Program.ConsoleDesign("R. Return to Main Menu");
                    Program.ConsoleDesign(false);
                    Program.ConsoleDesign(true);
                    Console.WriteLine();
                    Console.WriteLine("Choosed folder: {0}", file);
                    Console.WriteLine();
                    string type = Console.ReadLine().ToUpper();
                         if (type == "1") FARC.Signature = KKdFARC.Farc.FArc;
                    else if (type == "3" || type == "4")
                    {
                        FARC.Signature = KKdFARC.Farc.FARC;

                        Console.WriteLine();
                        Program.ConsoleDesign(true);
                        Program.ConsoleDesign("             Choose type of FARC:");
                        Program.ConsoleDesign(false);
                        Program.ConsoleDesign("1. FARC");
                        Program.ConsoleDesign("2. FARC (Compressed)");
                        Program.ConsoleDesign("3. FARC (Encrypted)");
                        Program.ConsoleDesign("4. FARC (Compressed & Encrypted)");
                        Program.ConsoleDesign(false);
                        Program.ConsoleDesign(true);
                        Console.WriteLine();
                        type = Console.ReadLine();
                        if (type == "2" || type == "4") FARC.FARCType |= KKdFARC.Type.GZip;
                        if (type == "3" || type == "4") FARC.FARCType |= KKdFARC.Type.ECB ;
                    }
                    else if (type == "R") return;
                    else FARC.Signature = KKdFARC.Farc.FArC;
                    new KKdFARC(file, true).Pack(FARC.Signature);
                }
            }
        }
    }
}
