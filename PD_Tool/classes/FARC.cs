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
                        FARC.UnPack(FileName);
            }
            else
            {
                string file = Main.Choose(2, "", out string[] FileNames);
                Console.Clear();
                Console.Title = "FARC Creator";
                if (file != "")
                {
                    Main.ConsoleDesign(true);
                    Main.ConsoleDesign("         Choose type of created FARC:");
                    Main.ConsoleDesign(false);
                    Main.ConsoleDesign("1. FArc [DT/DT2nd/DTex/F/F2nd/X]");
                    Main.ConsoleDesign("2. FArC [DT/DT2nd/DTex/F/F2nd/X] (Compressed)");
                    Main.ConsoleDesign("3. FARC [F/F2nd/X] (Compressed)");
                    Main.ConsoleDesign("4. FARC [FT] (Compressed)");
                    Main.ConsoleDesign(false);
                    Main.ConsoleDesign("Note: Creating FT FARCs currently not supported.");
                    Main.ConsoleDesign(false);
                    Main.ConsoleDesign(true);
                    Console.WriteLine();
                    Console.WriteLine("Choosed folder: {0}", file);
                    Console.WriteLine();
                    int.TryParse(Console.ReadLine(), out int type);
                         if (type == 1) FARC.Signature = KKdFARC.Farc.FArc;
                    else if (type == 3) FARC.Signature = KKdFARC.Farc.FARC;
                    else                FARC.Signature = KKdFARC.Farc.FArC;
                    Console.Clear();
                    Console.Title = "FARC Creator - Directory: " + Path.GetDirectoryName(file);
                    FARC.Pack(file);
                }
            }
        }
    }
}
