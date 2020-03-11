using System;
using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class MOT
    {
        public static void Processor(bool json)
        {
            Console.Title = "MOT Converter";
            Program.Choose(1, "bin", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            Mot mot;
            foreach (string file in fileNames)
                using (mot = new Mot())
                {
                    ext = Path.GetExtension(file);
                    filepath = file.Replace(ext, "");
                    ext = ext.ToLower();

                    Console.Title = "MOT Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".bin")
                    {
                        mot.    MOTReader(filepath);
                        mot.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".mp" || ext == ".json")
                    {
                        mot.MsgPackReader(filepath, ext == ".json");
                        mot.    MOTWriter(filepath);
                    }
                }
        }
    }
}
