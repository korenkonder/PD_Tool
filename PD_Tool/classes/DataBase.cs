using System;
using System.IO;
using Aet = KKdMainLib.DB.Aet;
using Auth = KKdMainLib.DB.Auth;
using Spr = KKdMainLib.DB.Spr;

namespace PD_Tool
{
    public class DataBase
    {
        public static void Processor(bool json, char choose)
        {
            if (choose == '\0')
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
                string format = Console.ReadLine().ToUpper();
                if (format.Length > 0) choose = format[0];
            }
            if (choose == '1') AuthDBProcessor(json);
            if (choose == '2')  AETDBProcessor(json);
            if (choose == '3')  SPRDBProcessor(json);
        }

        public static void AuthDBProcessor(bool JSON)
        {
            Console.Title = "Auth DB Converter";
            Program.Choose(1, "bin", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            Auth auth;
            foreach (string file in fileNames)
                using (auth = new Auth())
                {
                    Console.Title = "Auth DB Converter: " + Path.GetFileNameWithoutExtension(file);
                    ext = Path.GetExtension(file).ToLower();
                    filepath = file.Replace(Path.GetExtension(file), "");

                    if (ext == ".bin")
                    {
                        auth.BINReader    (filepath);
                        auth.MsgPackWriter(filepath, JSON);
                    }
                    else if (ext == ".mp" || ext == ".json")
                    {
                        auth.MsgPackReader(filepath, ext == ".json");
                        auth.BINWriter    (filepath);
                    }
                }
        }

        public static void AETDBProcessor(bool json)
        {
            Console.Title = "AET DB Converter";
            Program.Choose(1, "bin", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            Aet aet;
            foreach (string file in fileNames)
                using (aet = new Aet())
                {
                    Console.Title = "AET DB Converter: " + Path.GetFileNameWithoutExtension(file);
                    ext      = Path.GetExtension(file).ToLower();
                    filepath = file.Replace(Path.GetExtension(file), "");

                    if (ext == ".bin")
                    {
                        aet.BINReader    (filepath);
                        aet.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".mp" || ext == ".json")
                    {
                        aet.MsgPackReader(filepath, ext == ".json");
                        aet.BINWriter    (filepath);
                    }
                }
        }

        public static void SPRDBProcessor(bool json)
        {
            Console.Title = "SPR DB Converter";
            Program.Choose(1, "bin", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            Spr spr;
            foreach (string file in fileNames)
                using (spr = new Spr())
                {
                    Console.Title = "SPR DB Converter: " + Path.GetFileNameWithoutExtension(file);
                    ext      = Path.GetExtension(file).ToLower();
                    filepath = file.Replace(Path.GetExtension(file), "");

                    if (ext == ".bin")
                    {
                        spr.BINReader    (filepath);
                        spr.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".mp" || ext == ".json")
                    {
                        spr.MsgPackReader(filepath, ext == ".json");
                        spr.BINWriter    (filepath);
                    }
                }
        }
    }
}
