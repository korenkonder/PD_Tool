using System;
using System.IO;
using KKdFARC = KKdMainLib.FARC;

namespace PD_Tool
{
    public class FARC
    {
        public static void Processor(bool extract)
        {
            Console.Clear();
            if (extract)
            {
                Console.Title = "FARC Extractor";
                Program.Choose(1, "farc", out string[] fileNamesExtract);
                foreach (string fileExtract in fileNamesExtract)
                    if (fileExtract != "" && File.Exists(fileExtract))
                    {
                        Console.Title = "FARC Extractor: " + Path.GetFileNameWithoutExtension(fileExtract);
                        using (KKdFARC farc = new KKdFARC(fileExtract))
                            farc.UnPack();
                    }
                return;
            }
            
            string file = Program.Choose(2, "", out string[] fileNames);
            Console.Clear();
            if (file == null || file == "") return;
            Console.Title = "FARC Creator";
                
            using (KKdFARC farc = new KKdFARC(file, true))
            {
                Console.Title = "FARC Creator: " + Path.GetDirectoryName(file);
                
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
                string choose = Console.ReadLine().ToUpper();
                     if (choose == "1") farc.Signature = KKdFARC.Farc.FArc;
                else if (choose == "3" || choose == "4")
                {
                    farc.Signature = KKdFARC.Farc.FARC;
                    
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
                    choose = Console.ReadLine();
                    if (choose == "2" || choose == "4") farc.FARCType |= KKdFARC.Type.GZip;
                    if (choose == "3" || choose == "4") farc.FARCType |= KKdFARC.Type.ECB ;
                }
                else if (choose == "R") return;
                else farc.Signature = KKdFARC.Farc.FArC;
                farc.Pack(farc.Signature);
            }
        }
    }
}
