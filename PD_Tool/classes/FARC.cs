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
                Main.Choose(1, "farc", out string[] FileNames);
                foreach (string FileName in FileNames)
                    if (FileName != "" && File.Exists(FileName))
                        new KKdFARC(FileName).UnPack();
            }
            else
            {
                string file = Main.Choose(2, "", out string[] FileNames);
                Console.Clear();
                Console.Title = "FARC Creator";
                if (file != "")
                {
                    FARC = new KKdFARC();
                    Main.ConsoleDesign(true);
                    Main.ConsoleDesign("         Choose type of created FARC:");
                    Main.ConsoleDesign(false);
                    Main.ConsoleDesign("1. FArc [DT/DT2nd/DTex/F/F2nd/X]");
                    Main.ConsoleDesign("2. FArC [DT/DT2nd/DTex/F/F2nd/X] (Compressed)");
                    Main.ConsoleDesign("3. FARC [F/F2nd/X]");
                    Main.ConsoleDesign(false);
                    Main.ConsoleDesign("R. Return to Main Menu");
                    Main.ConsoleDesign(false);
                    Main.ConsoleDesign(true);
                    Console.WriteLine();
                    Console.WriteLine("Choosed folder: {0}", file);
                    Console.WriteLine();
                    string type = Console.ReadLine().ToUpper();
                         if (type == "1") FARC.Signature = KKdFARC.Farc.FArc;
                    else if (type == "3" || type == "4")
                    {
                        FARC.Signature = KKdFARC.Farc.FARC;

                        Console.WriteLine();
                        Main.ConsoleDesign(true);
                        Main.ConsoleDesign("             Choose type of FARC:");
                        Main.ConsoleDesign(false);
                        Main.ConsoleDesign("1. FARC");
                        Main.ConsoleDesign("2. FARC (Compressed)");
                        Main.ConsoleDesign("3. FARC (Encrypted)");
                        Main.ConsoleDesign("4. FARC (Compressed & Encrypted)");
                        Main.ConsoleDesign(false);
                        Main.ConsoleDesign(true);
                        Console.WriteLine();
                        type = Console.ReadLine();
                        if (type == "2" || type == "4") FARC.FARCType |= KKdFARC.Type.GZip;
                        if (type == "3" || type == "4") FARC.FARCType |= KKdFARC.Type.ECB ;
                    }
                    else if (type == "R") return;
                    else FARC.Signature = KKdFARC.Farc.FArC;
                    Console.Title = "FARC Creator - Directory: " + Path.GetDirectoryName(file);
                    new KKdFARC(file, true).Pack(FARC.Signature);
                }
            }
        }
    }
}
