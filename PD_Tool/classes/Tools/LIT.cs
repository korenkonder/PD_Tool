using System;
using KKdMainLib;
using KKdMainLib.IO;
using KKdMainLib.F2;

namespace PD_Tool.Tools
{
    public class LIT
    {
        public static void Processor()
        {
            Console.Title = "Light Converter";
            Light LIT;
            Program.Choose(1, "lit", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext = "";

            foreach (string file in FileNames)
            {
                LIT = new Light();
                ext = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext = ext.ToLower();

                Console.Title = "Light Converter: " + Path.GetFileNameWithoutExtension(file);
                     if (ext == ".lit") { LIT.LITReader(filepath); LIT.TXTWriter(filepath); }
                //else if (ext == ".txt") { LIT.TXTReader(filepath); LIT.LITWriter(filepath); }
                LIT = new Light();
            }
        }
    }
}
