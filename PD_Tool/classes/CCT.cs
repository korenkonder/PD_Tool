using System;
using KKdMainLib.F2;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class CCT
    {
        public static void Processor()
        {
            Console.Title = "Color Correction Converter";
            Program.Choose(1, "cct", out string[] fileNames);
            if (fileNames.Length < 1) return;

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
                        cct.TXTWriter(filepath);
                    }
                    /*else if (ext == ".txt")
                    {
                        blt.TXTReader(filepath);
                        blt.CCTWriter(filepath);
                    }*/
                }
        }
    }
}
