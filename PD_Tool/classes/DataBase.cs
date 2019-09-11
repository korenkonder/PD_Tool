using System;
using System.IO;
using DB = KKdMainLib.DB;

namespace PD_Tool
{
    public class DataBase
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "DB Converter";
            Console.Clear();
            Program.ConsoleDesign(true);
            Program.ConsoleDesign("         Choose type of DataBase file:");
            Program.ConsoleDesign(false);
            Program.ConsoleDesign("1. Auth DB Converter");
            Program.ConsoleDesign("2. AET DB Converter");
            Program.ConsoleDesign("3. SPR DB Converter");
            Program.ConsoleDesign(false);
            Program.ConsoleDesign("R. Return to Main Menu");
            Program.ConsoleDesign(false);
            Program.ConsoleDesign(true);
            Console.WriteLine();
            string format = Console.ReadLine();
            if (format == "1") AuthDBProcessor(JSON);
            if (format == "2")  AETDBProcessor(JSON);
            if (format == "3")  SPRDBProcessor(JSON);
        }

        public static void AuthDBProcessor(bool JSON)
        {
            Console.Title = "Auth DB Converter";
            DB.Auth Auth;
            Program.Choose(1, "bin", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext      = "";

            foreach (string file in FileNames)
            {
                Console.Title = "Auth DB Converter: " + Path.GetFileNameWithoutExtension(file);
                Auth = new DB.Auth();
                ext      = Path.GetExtension(file).ToLower();
                filepath = file.Replace(Path.GetExtension(file), "");

                if (ext == ".bin")
                {
                    Auth.BINReader    (filepath);
                    Auth.MsgPackWriter(filepath, JSON);
                }
                else if (ext == ".mp" || ext == ".json")
                {
                    Auth.MsgPackReader(filepath, ext == ".json");
                    Auth.BINWriter    (filepath);
                }
                Auth = null;
            }
        }

        public static void AETDBProcessor(bool JSON)
        {
            Console.Title = "AET DB Converter";
            DB.Aet Aet;
            Program.Choose(1, "bin", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext      = "";

            foreach (string file in FileNames)
            {
                Console.Title = "AET DB Converter: " + Path.GetFileNameWithoutExtension(file);
                Aet = new DB.Aet();
                ext      = Path.GetExtension(file).ToLower();
                filepath = file.Replace(Path.GetExtension(file), "");

                if (ext == ".bin")
                {
                    Aet.BINReader    (filepath);
                    Aet.MsgPackWriter(filepath, JSON);
                }
                else if (ext == ".mp" || ext == ".json")
                {
                    Aet.MsgPackReader(filepath, ext == ".json");
                    Aet.BINWriter    (filepath);
                }
                Aet = null;
            }
        }

        public static void SPRDBProcessor(bool JSON)
        {
            Console.Title = "SPR DB Converter";
            DB.Spr Spr;
            Program.Choose(1, "bin", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext      = "";

            foreach (string file in FileNames)
            {
                Console.Title = "SPR DB Converter: " + Path.GetFileNameWithoutExtension(file);
                Spr = new DB.Spr();
                ext      = Path.GetExtension(file).ToLower();
                filepath = file.Replace(Path.GetExtension(file), "");

                if (ext == ".bin")
                {
                    Spr.BINReader    (filepath);
                    Spr.MsgPackWriter(filepath, JSON);
                }
                else if (ext == ".mp" || ext == ".json")
                {
                    Spr.MsgPackReader(filepath, ext == ".json");
                    Spr.BINWriter    (filepath);
                }
                Spr = null;
            }
        }
    }
}
