using System;
using System.IO;
using KKdMainLib;
using KKdSoundLib;

namespace PD_Tool.Tools
{
    public class DIV
    {
        public static void Processor()
        {
            Console.Title = "DIVA Converter";
            Main.Choose(1, "diva", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext = "";

            DIVA DIVA;
            foreach (string file in FileNames)
            {
                DIVA = new DIVA();
                ext      = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext      = ext.ToLower();

                Console.Title = "DIVA Converter: " + Path.GetFileNameWithoutExtension(file);
                     if (ext == ".diva") DIVA.DIVAReader(filepath);
                else if (ext == ".wav" ) DIVA.DIVAWriter(filepath);
                DIVA = null;
            }
        }
    }
}