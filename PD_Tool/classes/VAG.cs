using System;
using System.IO;
using KKdVAG = KKdSoundLib.VAG;

namespace PD_Tool
{
    public class VAG
    {
        public static void Processor()
        {
            Console.Title = "VAG Converter";
            Program.Choose(1, "vag", out string[] fileNames);
            if (fileNames.Length < 1) return;

            bool wav = false;
            foreach (string file in fileNames)
                if (Path.GetExtension(file) == ".wav")
                    wav = true;

            string choose = "";
            if (wav)
            {
                Console.Clear();
                Program.ConsoleDesign(true);
                Program.ConsoleDesign("          Choose type of format to export:");
                Program.ConsoleDesign(false);
                Program.ConsoleDesign("1. VAG (Downmix to 1 ch)");
                Program.ConsoleDesign("2. HEVAG [Fastest] (Multichannel VAG)");
                Program.ConsoleDesign("3. HEVAG [Fast]");
                Program.ConsoleDesign("4. HEVAG [Medium]");
                Program.ConsoleDesign("5. HEVAG [Slow]");
                Program.ConsoleDesign("6. HEVAG [Slowest]");
                Program.ConsoleDesign("7. HEVAG [Slow as hell] (Full HEVAG)");
                Program.ConsoleDesign(false);
                Program.ConsoleDesign(true);
                Console.WriteLine();
                choose = Console.ReadLine().ToUpper();
            }

            string filepath, ext;
            KKdVAG VAG;
            foreach (string file in fileNames)
                using (VAG = new KKdVAG())
                {
                    ext      = Path.GetExtension(file);
                    filepath = file.Replace(ext, "");
                    ext      = ext.ToLower();

                    Console.Title = "VAG Converter: " + Path.GetFileNameWithoutExtension(file);
                         if (ext == ".vag") { VAG.VAGReader(filepath); VAG.WAVWriter(filepath        ); }
                    else if (ext == ".wav") { VAG.WAVReader(filepath); VAG.VAGWriter(filepath, choose); }
                }
        }
    }
}
