using System;
using KKdMainLib;
using KKdMainLib.IO;
using KKdMainLib.F2;

namespace PD_Tool.Tools
{
    public class CCT
    {
        public static void Processor()
        {
            Console.Title = "Color Correction Converter";
            ColorCorrection ColorCorrection;
            Main.Choose(1, "cct", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext = "";

            foreach (string file in FileNames)
            {
                ColorCorrection = new ColorCorrection();
                ext = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext = ext.ToLower();

                Console.Title = "Color Correction Converter: " + Path.GetFileNameWithoutExtension(file);
                     if (ext == ".cct") { ColorCorrection.CCTReader(filepath); ColorCorrection.TXTWriter(filepath); }
                //else if (ext == ".txt") { ColorCorrection.TXTReader(filepath); ColorCorrection.BLTWriter(filepath); }
                ColorCorrection = new ColorCorrection();
            }
        }
    }
}
