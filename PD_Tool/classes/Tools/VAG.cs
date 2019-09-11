using System;
using System.IO;
using KKdVAG = KKdSoundLib.VAG;

namespace PD_Tool.Tools
{
    public class VAG
    {
        public static void Processor()
        {
            Console.Title = "VAG Converter";
            Program.Choose(1, "vag", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext = "";

            bool InputWAV = false;
            foreach (string file in FileNames)
                if (Path.GetExtension(file) == ".wav")
                    InputWAV = true;

            bool HE_VAG = true;
            if (InputWAV)
            {
                Console.Clear();
                Program.ConsoleDesign(true);
                Program.ConsoleDesign("          Choose type of format to export:");
                Program.ConsoleDesign(false);
                Program.ConsoleDesign("1. VAG (Downmix to 1 ch)");
                Program.ConsoleDesign("2. HEVAG");
                Program.ConsoleDesign(false);
                Program.ConsoleDesign(true);
                Console.WriteLine();
                string format = Console.ReadLine();
                HE_VAG = format == "2";
            }

            KKdVAG VAG;
            foreach (string file in FileNames)
            {
                VAG = new KKdVAG();
                ext      = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext      = ext.ToLower();

                Console.Title = "VAG Converter: " + Path.GetFileNameWithoutExtension(file);
                     if (ext == ".vag") { VAG.VAGReader(filepath); VAG.WAVWriter(filepath        ); }
                else if (ext == ".wav") { VAG.WAVReader(filepath); VAG.VAGWriter(filepath, HE_VAG); }
                VAG = null;
            }
        }
    }
}