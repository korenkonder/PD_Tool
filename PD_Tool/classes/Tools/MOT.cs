using System;
using KKdMainLib;
using KKdMainLib.IO;
using KKdMot = KKdMainLib.Mot;

namespace PD_Tool.Tools
{
    public class MOT
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "MOT Converter";
            KKdMot Mot;
            Main.Choose(1, "bin", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext = "";
            
            foreach (string file in FileNames)
            {
                Mot = new KKdMot();
                ext = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext = ext.ToLower();

                Console.Title = "MOT Converter: " + Path.GetFileNameWithoutExtension(file);
                if (ext == ".bin") Mot.MOTReader(filepath, JSON);
                else if (ext == ".mp" || ext == ".json")
                {
                    //Mot.MsgPackReader(filepath, ext == ".json");
                    //Mot.    AETWriter(filepath);
                }
                Mot = new KKdMot();
            }
        }
    }
}
