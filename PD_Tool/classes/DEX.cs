using System;
using KKdBaseLib;
using KKdMainLib.IO;
using KKdDEX = KKdMainLib.DEX;

namespace PD_Tool
{
    public class DEX
    {
        public static void Processor(bool json)
        {
            Console.Title = "DEX Converter";
            Program.Choose(1, "dex", out string[] fileNames);
            if (fileNames.Length < 1) return;

            bool  mp   = true;
            bool _json = true;
            foreach (string file in fileNames)
                     if (file.EndsWith(".mp"  )) {  mp   = false; break; }
                else if (file.EndsWith(".json")) { _json = false; break; }

            Console.Clear();
            string choose = "";
            Program.ConsoleDesign(true);
            Program.ConsoleDesign("        Choose type of exporting file:");
            Program.ConsoleDesign(false);
            Program.ConsoleDesign("1. F/AFT/FT/M39 PS3/PS4/PSV/AC/NSW");
            Program.ConsoleDesign("2. F2           PS3/PSV");
            Program.ConsoleDesign("3. X            PS4/PSV");
            if ( mp   && !json) Program.ConsoleDesign("9. MessagePack");
            if (_json &&  json) Program.ConsoleDesign("9. JSON");
            Program.ConsoleDesign(false);
            Program.ConsoleDesign(true);
            Console.WriteLine();
            choose = Console.ReadLine().ToUpper();

            Format format = Format.NULL;
                 if (choose == "1") format = Format.F ;
            else if (choose == "2") format = Format.F2;
            else if (choose == "3") format = Format.X ;
            else if (choose == "9" && mp && _json) format = Format.NULL;
            else return;

            string filepath, ext;
            KKdDEX dex;
            foreach (string file in fileNames)
                using (dex = new KKdDEX())
                {
                    filepath = Path.RemoveExtension(file);
                    ext      = Path.GetExtension(file).ToLower();

                    Console.Title = "DEX Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".bin" || ext == ".dex") dex.    DEXReader(filepath, ext );
                    else                                dex.MsgPackReader(filepath, json);

                    if (format > Format.NULL) dex.    DEXWriter(filepath, format);
                    else                      dex.MsgPackWriter(filepath, json);
                }
        }
    }
}
