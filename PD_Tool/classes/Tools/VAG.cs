using System;
using System.IO;
using KKdMainLib;
using KKdVAG = KKdSoundLib.VAG;

namespace PD_Tool.Tools
{
    public class VAG
    {
        public static void Processor()
        {
            Console.Title = "VAG Converter";
            Main.Choose(1, "vag", out string[] FileNames);
            bool InputWAV = false;
            foreach (string file in FileNames)
                if (Path.GetExtension(file) == ".wav")
                    InputWAV = true;

            bool HE_VAG = true;
            if (InputWAV)
            {
                Console.Clear();
                Main.ConsoleDesign(true);
                Main.ConsoleDesign("          Choose type of format to export:");
                Main.ConsoleDesign(false);
                Main.ConsoleDesign("1. VAG (Downmix to 1 ch)");
                Main.ConsoleDesign("2. HEVAG");
                Main.ConsoleDesign(false);
                Main.ConsoleDesign(true);
                Console.WriteLine();
                string format = Console.ReadLine();
                HE_VAG = format != "2";
            }

            KKdVAG VAG;
            foreach (string file in FileNames)
                try
                {
                    string ext = Path.GetExtension(file);
                    string filepath = file.Remove(file.Length - ext.Length);
                    Console.Title = "VAG Converter: " +
                        Path.GetFileNameWithoutExtension(file);
                    VAG = new KKdVAG() { file = filepath };
                    switch (ext.ToLower())
                    {
                        case ".vag":
                            VAG.VAGReader();
                            VAG.WAVWriter();
                            break;
                        case ".wav":
                            VAG.WAVReader();
                            VAG.VAGWriter(HE_VAG);
                            break;
                    }
                }
                catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }
}