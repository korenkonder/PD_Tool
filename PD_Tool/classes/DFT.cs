using System;
using KKdMainLib.F2;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class DFT
    {
        public static void Processor(bool json)
        {
            Console.Title = "DOF Converter";
            Program.Choose(1, "dft", out string[] fileNames);
            if (fileNames.Length < 1) return;

            bool cc = false;
            foreach (string file in fileNames)
                if (file.EndsWith(".dft")) { cc = true; break; }

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
                        if (choose == "2" && !dft.IsX)
                            dft.TXTWriter(filepath);
                        else
                            dft.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".json" || ext == ".mp")
                    {
                        dft.MsgPackReader(filepath, json);
                        dft.DFTWriter(filepath);
                    }
                }
        }
    }
}
