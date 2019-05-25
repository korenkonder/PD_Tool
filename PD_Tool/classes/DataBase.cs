using System;
using System.IO;
using KKdMainLib;
using KKdMainLib.DB;

namespace PD_Tool
{
    public class DataBase
    {
        public static void Processor()
        {
            Console.Title = "DB Converter";
            Console.Clear();
            Main.ConsoleDesign(true);
            Main.ConsoleDesign("         Choose type of DataBase file:");
            Main.ConsoleDesign(false);
            Main.ConsoleDesign("1. Auth DB Converter");
            Main.ConsoleDesign(false);
            Main.ConsoleDesign(true);
            Console.WriteLine();
            string format = Console.ReadLine();
            if (format == "1") AuthDBProcessor();
        }

        public static void AuthDBProcessor()
        {
            Console.Title = "Auth DB Converter";
            Auth Auth = new Auth();
            Main.Choose(1, "bin", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext      = "";

            foreach (string file in FileNames)
            {
                Console.Title = "Auth: DB Converter: " + Path.GetFileNameWithoutExtension(file);
                Auth = new Auth();
                ext      = Path.GetExtension(file).ToLower();
                filepath = file.Replace(Path.GetExtension(file), "");

                if (ext == ".bin")
                {
                    Auth.BINReader    (filepath);
                    Auth.MsgPackWriter(filepath);
                }
                else if (ext == ".mp")
                {
                    Auth.MsgPackReader(filepath);
                    Auth.BINWriter    (filepath);
                }
            }
        }
    }
}
