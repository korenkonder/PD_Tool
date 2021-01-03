using System;
using KKdMainLib.IO;
using KKdMainLib.F2;

namespace PD_Tool
{
    public class LIT
    {
        public static void Processor(bool json)
        {
            Console.Title = "Light Converter";
            Program.Choose(1, "lit", out string[] fileNames);
            if (fileNames.Length < 1) return;

            bool bloom = false;
            foreach (string file in fileNames)
                if (file.EndsWith(".lit")) { bloom = true; break; }

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
            Light lit;
            foreach (string file in fileNames)
                using (lit = new Light())
                {
                    filepath = Path.RemoveExtension(file);
                    ext      = Path.GetExtension(file).ToLower();

                    Console.Title = "Light Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".lit")
                    {
                        lit.LITReader(filepath);
                        if (choose == "2" && !lit.IsX)
                            lit.TXTWriter(filepath);
                        else
                            lit.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".json" || ext == ".mp")
                    {
                        lit.MsgPackReader(filepath, json);
                        lit.LITWriter(filepath);
                    }
                }
        }
    }
}
