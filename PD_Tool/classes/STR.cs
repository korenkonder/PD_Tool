using System;
using KKdMainLib.IO;
using KKdSTR = KKdMainLib.STR;

namespace PD_Tool
{
    public class STR
    {
        public static void Processor(bool json)
        {
            Console.Title = "STR Converter";
            Program.Choose(1, "str", out string[] fileNames);

            string filepath, ext;
            KKdSTR str;
            foreach (string file in fileNames)
                using (str = new KKdSTR())
                {
                    filepath = Path.RemoveExtension(file);
                    ext      = Path.GetExtension(file).ToLower();

                    Console.Title = "STR Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".str" || ext == ".bin")
                    {
                        str.STRReader    (filepath,  ext);
                        str.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".json" || ext == ".mp")
                    {
                        str.MsgPackReader(filepath, ext == ".json");
                        str.STRWriter    (filepath);
                    }
                }
        }
    }
}
