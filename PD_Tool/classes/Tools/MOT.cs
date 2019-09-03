using System;
using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool.Tools
{
    public class MOT
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "MOT Converter";
            Mot Mot;
            Program.Choose(1, "bin", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext = "";
            
            foreach (string file in FileNames)
            {
                Mot = new Mot();
                ext = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext = ext.ToLower();

                Console.Title = "MOT Converter: " + Path.GetFileNameWithoutExtension(file);
                if (ext == ".bin")
                {
                    Mot.    MOTReader(filepath);
                    Mot.MsgPackWriter(filepath, JSON);
                }
                else if (ext == ".mp" || ext == ".json")
                {
                    Mot.MsgPackReader(filepath, ext == ".json");
                    Mot.    MOTWriter(filepath);
                }
                Mot = new Mot();
            }
        }
    }
}
