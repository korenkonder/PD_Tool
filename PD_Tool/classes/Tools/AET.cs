using System;
using KKdMainLib;
using KKdMainLib.IO;
using KKdAet = KKdMainLib.Aet.Aet;

namespace PD_Tool.Tools
{
    public class AET
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "AET Converter";
            KKdAet Aet;
            Main.Choose(1, "bin", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext = "";
            
            foreach (string file in FileNames)
            {
                Aet = new KKdAet();
                ext = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext = ext.ToLower();

                Console.Title = "AET Converter: " + Path.GetFileNameWithoutExtension(file);
                if (ext == ".bin")
                {
                    Aet.    AETReader(filepath);
                    Aet.MsgPackWriter(filepath, JSON);
                }
                else if (ext == ".mp" || ext == ".json")
                {
                    Aet.MsgPackReader(filepath, ext == ".json");
                    Aet.    AETWriter(filepath);
                }
                Aet = null;
            }
        }
    }
}
