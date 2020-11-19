using System;
using KKdMainLib.F2;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class BLT
    {
        public static void Processor()
        {
            Console.Title = "Bloom Converter";
            Program.Choose(1, "blt", out string[] fileNames);
            if (fileNames.Length < 1) return;

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
                        blt.TXTWriter(filepath);
                    }
                    /*else if (ext == ".txt")
                    {
                        blt.TXTReader(filepath);
                        blt.BLTWriter(filepath);
                    }*/
                }
        }
    }
}
