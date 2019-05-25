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

            DIVA DIVA;
            foreach (string file in FileNames)
                try
                {
                    string filepath = file.Replace(Path.GetExtension(file), "");
                    string ext = Path.GetExtension(file);
                    Console.Title = "DIVA Converter: " + Path.GetFileNameWithoutExtension(file);
                    DIVA = new DIVA(filepath);
                    switch (ext.ToLower())
                    {
                        case ".diva":
                            DIVA.DIVAReader();
                            break;
                        case ".wav":
                            DIVA.DIVAWriter();
                            break;
                    }
                    GC.Collect();
                }
                catch (Exception e) { Console.WriteLine(e.Message); }

        }
    }
}