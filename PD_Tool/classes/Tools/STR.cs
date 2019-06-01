using System;
using KKdMainLib;
using KKdMainLib.IO;
using KKdSTR = KKdMainLib.STR;

namespace PD_Tool.Tools
{
    public class STR
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "STR Converter";
            Main.Choose(1, "str", out string[] FileNames);

            KKdSTR Data;
            string filepath = "";
            string ext      = "";
            foreach (string file in FileNames)
            {
                filepath = file.Replace(Path.GetExtension(file), "");
                ext      = Path.GetExtension(file).ToLower();
                Data = new KKdSTR();

                Console.Title = "PD_Tool: Converter Tools: STR Reader: " +
                    Path.GetFileNameWithoutExtension(file);
                if (ext == ".str" || ext == ".bin")
                {
                    Data.STRReader    (filepath, ext);
                    Data.MsgPackWriter(filepath, JSON);
                }
                else if (ext == ".mp")
                {
                    Data.MsgPackReader(filepath, JSON);
                    Data.STRWriter    (filepath);
                }
            }
        }
    }
}
