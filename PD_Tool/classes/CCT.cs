using System;
using KKdMainLib.IO;
using KKdMainLib.F2;

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
                    ext = Path.GetExtension(file);
                    filepath = file.Replace(ext, "");
                    ext = ext.ToLower();

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
