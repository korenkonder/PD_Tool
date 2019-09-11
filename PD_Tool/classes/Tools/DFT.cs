using System;
using KKdMainLib.IO;
using KKdMainLib.F2;

namespace PD_Tool.Tools
{
    public class DFT
    {
        public static void Processor()
        {
            Console.Title = "DOF Converter";
            DOF DOF;
            Program.Choose(1, "dft", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext = "";

            foreach (string file in FileNames)
            {
                DOF = new DOF();
                ext = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext = ext.ToLower();

                Console.Title = "DOF Converter: " + Path.GetFileNameWithoutExtension(file);
                     if (ext == ".dft") { DOF.DFTReader(filepath); DOF.TXTWriter(filepath); }
                //else if (ext == ".txt") { DOF.TXTReader(filepath); DOF.DFTWriter(filepath); }
                DOF = new DOF();
            }
        }
    }
}
