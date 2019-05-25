using System;
using KKdMainLib;
using MSIO = System.IO;
using KKdSTR = KKdMainLib.STR;

namespace PD_Tool.Tools
{
    public class STR
    {
        public static void Processor()
        {
            Console.Title = "STR Converter";
            Main.Choose(1, "str", out string[] FileNames);

            KKdSTR Data;
            string filepath = "";
            string ext      = "";
            foreach (string file in FileNames)
            {
                filepath = file.Replace(MSIO.Path.GetExtension(file), "");
                ext      = MSIO.Path.GetExtension(file).ToLower();
                Data = new KKdSTR();

                Console.Title = "PD_Tool: Converter Tools: STR Reader: " +
                    MSIO.Path.GetFileNameWithoutExtension(file);
                if (ext == ".str" || ext == ".bin")
                {
                    Data.STRReader    (filepath, ext);
                    Data.MsgPackWriter(filepath);
                }
                else if (ext == ".mp")
                {
                    Data.MsgPackReader(filepath);
                    Data.STRWriter    (filepath);
                }
            }
        }
    }
}
