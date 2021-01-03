using System;
using KKdMainLib.F2;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class BLT
    {
        public static void Processor(bool json)
        {
            Console.Title = "Bloom Converter";
            Program.Choose(1, "blt", out string[] fileNames);
            if (fileNames.Length < 1) return;

            bool bloom = false;
            foreach (string file in fileNames)
                if (file.EndsWith(".blt")) { bloom = true; break; }

            string choose = "";
            if (bloom)
            {
                Console.Clear();
                Program.ConsoleDesign(true);
                Program.ConsoleDesign("          Choose type of format to export:");
                Program.ConsoleDesign(false);
                Program.ConsoleDesign("1. JSON");
                Program.ConsoleDesign("2. TXT");
                Program.ConsoleDesign(false);
                Program.ConsoleDesign(true);
                Console.WriteLine();
                choose = Console.ReadLine().ToUpper();
            }

            string filepath, ext;
            Bloom blt;
            foreach (string file in fileNames)
                using (blt = new Bloom())
                {
                    filepath = Path.RemoveExtension(file);
                    ext      = Path.GetExtension(file).ToLower();

                    Console.Title = "Bloom Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".blt")
                    {
                        blt.BLTReader(filepath);
                        if (choose == "2" && !blt.IsX)
                            blt.TXTWriter(filepath);
                        else
                            blt.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".json" || ext == ".mp")
                    {
                        blt.MsgPackReader(filepath, json);
                        blt.BLTWriter(filepath);
                    }
                }
        }
    }
}
