using System;
using KKdMainLib;
using KKdMainLib.IO;
using KKdDEX = KKdMainLib.DEX;

namespace PD_Tool.Tools
{
    public class DEX
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "DEX Converter";
            KKdDEX DEX;
            Main.Choose(1, "dex", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext      = "";
            
            bool  MP   = true;
            bool _JSON = true;
            foreach (string file in FileNames)
                     if (file.EndsWith(".mp"  ))
                {  MP   = false; break; }
                else if (file.EndsWith(".json"))
                { _JSON = false; break; }

            Console.Clear();
            string format = "";
            Main.ConsoleDesign(true);
            Main.ConsoleDesign("        Choose type of exporting file:");
            Main.ConsoleDesign(false);
            Main.ConsoleDesign("1. F/FT PS3/PS4/PSVita");
            Main.ConsoleDesign("2. F2nd PS3/PSVita");
            Main.ConsoleDesign("3. X    PS4/PSVita");
            if ( MP   && !JSON)   Main.ConsoleDesign("9. MessagePack");
            if (_JSON &&  JSON)   Main.ConsoleDesign("9. JSON");
            Main.ConsoleDesign(false);
            Main.ConsoleDesign(true);
            Console.WriteLine();
            format = Console.ReadLine();
            
            Main.Format Format = Main.Format.NULL;
                 if (format == "1") Format = Main.Format.F   ;
            else if (format == "2") Format = Main.Format.F2LE;
            else if (format == "3") Format = Main.Format.X   ;
            else if (format == "9" && (MP && _JSON)) Format = Main.Format.NULL;
            else return;

            foreach (string file in FileNames)
            {
                DEX = new KKdDEX();
                ext      = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext = ext.ToLower();
                
                Console.Title = "DEX Converter: " + Path.GetFileNameWithoutExtension(file);
                if (ext == ".bin" || ext == ".dex")
                     DEX.    DEXReader(filepath, ext );
                else DEX.MsgPackReader(filepath, JSON);

                if (Format > Main.Format.NULL)
                     DEX.    DEXWriter(filepath, Format);
                else DEX.MsgPackWriter(filepath, JSON  );
                DEX = null;
            }
        }
    }
}
