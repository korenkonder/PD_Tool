using System;
using KKdMainLib.IO;
using KKdMainLib.F2;

namespace PD_Tool
{
    public class LIT
    {
        public static void Processor()
        {
            Console.Title = "Light Converter";
            Program.Choose(1, "lit", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            Light lit;
            foreach (string file in fileNames)
                using (lit = new Light())
                {
                    ext = Path.GetExtension(file);
                    filepath = file.Replace(ext, "");
                    ext = ext.ToLower();

                    Console.Title = "Light Converter: " + Path.GetFileNameWithoutExtension(file);
                         if (ext == ".lit") { lit.LITReader(filepath); lit.TXTWriter(filepath); }
                    //else if (ext == ".txt") { lit.TXTReader(filepath); lit.LITWriter(filepath); }
                }
        }
    }
}
