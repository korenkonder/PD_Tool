using System;
using KKdMainLib.F2;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class DFT
    {
        public static void Processor()
        {
            Console.Title = "DOF Converter";
            Program.Choose(1, "dft", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            DOF dft;
            foreach (string file in fileNames)
                using (dft = new DOF())
                {
                    filepath = Path.RemoveExtension(file);
                    ext      = Path.GetExtension(file).ToLower();

                    Console.Title = "DOF Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".dft")
                    {
                        dft.DFTReader(filepath);
                        dft.TXTWriter(filepath);
                    }
                    /*else if (ext == ".txt")
                    {
                        dft.TXTReader(filepath);
                        dft.DFTWriter(filepath);
                    }*/
                }
        }
    }
}
