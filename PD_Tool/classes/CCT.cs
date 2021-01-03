using System;
using KKdMainLib.F2;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class CCT
    {
        public static void Processor(bool json)
        {
            Console.Title = "Color Correction Converter";
            Program.Choose(1, "cct", out string[] fileNames);
            if (fileNames.Length < 1) return;

            bool cc = false;
            foreach (string file in fileNames)
                if (file.EndsWith(".cct")) { cc = true; break; }

            string choose = "";
            if (cc)
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
            ColorCorrection cct;
            foreach (string file in fileNames)
                using (cct = new ColorCorrection())
                {
                    filepath = Path.RemoveExtension(file);
                    ext      = Path.GetExtension(file).ToLower();

                    Console.Title = "Color Correction Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".cct")
                    {
                        cct.CCTReader(filepath);
                        if (choose == "2" && !cct.IsX)
                            cct.TXTWriter(filepath);
                        else
                            cct.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".json" || ext == ".mp")
                    {
                        cct.MsgPackReader(filepath, json);
                        cct.CCTWriter(filepath);
                    }
                }
        }
    }
}
