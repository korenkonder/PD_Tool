using System;
using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class MHD
    {
        public static void Processor(bool json)
        {
            Console.Title = "MotHead Converter";
            Program.Choose(1, "bin", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            MotHead mhd;
            foreach (string file in fileNames)
                using (mhd = new MotHead())
                {
                    filepath = Path.RemoveExtension(file);
                    ext      = Path.GetExtension(file).ToLower();

                    Console.Title = "MotHead Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".bin")
                    {
                        mhd.MotHeadReader(filepath);
                        mhd.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".mp" || ext == ".json")
                    {
                        mhd.MsgPackReader(filepath, ext == ".json");
                        mhd.MotHeadWriter(filepath);
                    }
                }
        }
    }
}
