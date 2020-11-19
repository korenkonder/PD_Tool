using System;
using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class ADP
    {
        public static void Processor(bool json)
        {
            Console.Title = "Add Param Converter";
            Program.Choose(1, "adp", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            AddParam adp;
            foreach (string file in fileNames)
                using (adp = new AddParam())
                {
                    filepath = Path.RemoveExtension(file);
                    ext      = Path.GetExtension(file).ToLower();

                    Console.Title = "Add Param Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".adp")
                    {
                        adp.AddParamReader(filepath);
                        adp. MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".mp" || ext == ".json")
                    {
                        adp. MsgPackReader(filepath, ext == ".json");
                        adp.AddParamWriter(filepath);
                    }
                }
        }
    }
}
