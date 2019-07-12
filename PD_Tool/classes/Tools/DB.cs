using System;
using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool.Tools
{
    public class DB
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "DataBank Converter";
            Main.Choose(1, "databank", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext = "";

            bool MP = true;
            foreach (string file in FileNames)
                     if (file.EndsWith(".mp"  )) { MP = false; break; }
                else if (file.EndsWith(".json")) { MP = false; break; }

            string format = "1";
            if (MP)
            {
                Console.Clear();
                Main.ConsoleDesign(true);
                Main.ConsoleDesign("        Choose type of exporting file:");
                Main.ConsoleDesign(false);
                Main.ConsoleDesign("1. Compact");
                Main.ConsoleDesign("2. Normal");
                Main.ConsoleDesign(false);
                Main.ConsoleDesign(true);
                Console.WriteLine();
                format = Console.ReadLine();
            }

            DataBank DB;
            string[] file_split;
            foreach (string file in FileNames)
            {
                ext      = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext      = ext.ToLower();

                string filename = Path.GetFileNameWithoutExtension(file);
                file_split = filename.Split('_');
                DB = new DataBank();
                if (file_split.Length == 5 && ext == ".dat" && MP)
                {
                    filepath = file.Replace(filename + ".dat", "");
                    Console.Title = "DataBank Converter: " + filename;
                    DB.     DBReader(file);
                    DB.MsgPackWriter(filepath + file_split[0] + "_" + 
                        file_split[1] + "_" + file_split[2], JSON, format != "2");
                }
                else if (ext == ".mp" || ext == ".json" && !MP)
                {
                    Console.Title = "DataBank Converter: " + filename;
                    DB.MsgPackReader(filepath, JSON);
                    DB.     DBWriter(filepath);
                }
                DB = null;
            }
        }
    }
}
