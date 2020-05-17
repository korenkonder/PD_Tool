using System;
using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class AET
    {
        public static void Processor(bool json)
        {
            Console.Title = "AET Converter";
            Program.Choose(1, "bin", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            Aet aet;
            foreach (string file in fileNames)
                using (aet = new Aet())
                {
                    ext = Path.GetExtension(file);
                    filepath = file.Replace(ext, "");
                    ext = ext.ToLower();

                    Console.Title = "AET Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".bin")
                    {
                        aet.    AETReader(filepath);
                        aet.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".mp" || ext == ".json")
                    {
                        aet.MsgPackReader(filepath, ext == ".json");
                        aet.    AETWriter(filepath);
                    }
                }
        }
    }
}
