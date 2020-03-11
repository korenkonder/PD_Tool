﻿using System;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class DB
    {
        public static void Processor(bool json)
        {
            Console.Title = "DataBank Converter";
            Program.Choose(1, "databank", out string[] fileNames);
            if (fileNames.Length < 1) return;

            bool mp = true;
            foreach (string file in fileNames)
                     if (file.EndsWith(".mp"  )) { mp = false; break; }
                else if (file.EndsWith(".json")) { mp = false; break; }

            uint num2 = (uint)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string[] file_split;
            string filepath, ext;
            KKdMainLib.DataBank db;
            foreach (string file in fileNames)
            {
                ext      = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext      = ext.ToLower();

                string filename = Path.GetFileNameWithoutExtension(file);
                file_split = filename.Split('_');
                using (db = new KKdMainLib.DataBank())
                {
                    if (file_split.Length == 5 && ext == ".dat" && mp)
                    {
                        filepath = file.Replace(filename + ".dat", "");
                        Console.Title = "DataBank Converter: " + filename;
                        db.     DBReader(file);
                        db.MsgPackWriter(filepath + file_split[0] + "_" +
                            file_split[1] + "_" + file_split[2], json);
                    }
                    else if (ext == ".bin" && mp)
                    {
                        filepath = file.Replace(".bin", "");
                        Console.Title = "DataBank Converter: " + filename;
                        db.     DBReader(file);
                        db.MsgPackWriter(filepath, json);
                    }
                    else if ((ext == ".mp" || ext == ".json") && !mp)
                    {
                        Console.Title = "DataBank Converter: " + filename;
                        db.MsgPackReader(filepath, json);
                        db.     DBWriter(filepath, num2);
                    }
                }
            }
        }
    }
}
