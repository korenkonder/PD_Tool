using System;
using KKdMainLib.IO;
using KKdMainLib.F2;

namespace PD_Tool.Tools
{
    public class BLT
    {
        public static void Processor()
        {
            Console.Title = "Bloom Converter";
            Bloom Bloom;
            Program.Choose(1, "blt", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext = "";

            foreach (string file in FileNames)
            {
                Bloom = new Bloom();
                ext = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext = ext.ToLower();

                Console.Title = "Bloom Converter: " + Path.GetFileNameWithoutExtension(file);
                     if (ext == ".blt") { Bloom.BLTReader(filepath); Bloom.TXTWriter(filepath); }
                //else if (ext == ".txt") { Bloom.TXTReader(filepath); Bloom.BLTWriter(filepath); }
                Bloom = new Bloom();
            }
        }
    }
}
