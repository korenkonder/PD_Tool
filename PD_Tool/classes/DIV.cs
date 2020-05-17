using System;
using System.IO;
using KKdSoundLib;

namespace PD_Tool
{
    public class DIV
    {
        public static void Processor()
        {
            Console.Title = "DIVA Converter";
            Program.Choose(1, "diva", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            DIVA diva;
            foreach (string file in fileNames)
            {
                diva = new DIVA();
                ext      = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext      = ext.ToLower();

                Console.Title = "DIVA Converter: " + Path.GetFileNameWithoutExtension(file);
                     if (ext == ".diva") diva.DIVAReader(filepath);
                else if (ext == ".wav" ) diva.DIVAWriter(filepath);
                diva = null;
            }
        }
    }
}
