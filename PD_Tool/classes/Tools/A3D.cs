using System;
using KKdMainLib;
using KKdMainLib.IO;
using KKdA3DA = KKdMainLib.A3DA.A3DA;

namespace PD_Tool.Tools
{
    class A3D
    {
        public static void Processor(bool JSON)
        {
            Console.Title = "A3DA Converter";
            Main.Choose(1, "a3da", out string[] FileNames);
            if (FileNames.Length < 1) return;
            string filepath = "";
            string ext      = "";

            bool MP = true;
            foreach (string file in FileNames)
                if (file.EndsWith(".mp"  )) { MP = false; break; }

            Main.Format Format = Main.Format.NULL;
            if (!MP)
            {
                Console.Clear();
                Main.ConsoleDesign(true);
                Main.ConsoleDesign("          Choose type of format to export:");
                Main.ConsoleDesign(false);
                Main.ConsoleDesign("1. DT   PS3");
                Main.ConsoleDesign("2. F    PS3/PSV");
                Main.ConsoleDesign("3. FT   PS4");
                Main.ConsoleDesign("4. F2nd PS3/PSV");
                Main.ConsoleDesign("5. MGF      PSV");
                Main.ConsoleDesign("6. X    PS4/PSV");
                Main.ConsoleDesign(false);
                Main.ConsoleDesign(true);
                Console.WriteLine();
                string format = Console.ReadLine();
                     if (format == "1") Format = Main.Format.DT  ;
                else if (format == "2") Format = Main.Format.F   ;
                else if (format == "3") Format = Main.Format.FT  ;
                else if (format == "4") Format = Main.Format.F2LE;
                else if (format == "5") Format = Main.Format.MGF ;
                else if (format == "6") Format = Main.Format.X   ;
                else return;
            }

            KKdA3DA A;
            foreach (string file in FileNames)
                try
                {
                    ext      = Path.GetExtension(file);
                    filepath = file.Replace(ext, "");
                    ext      = ext.ToLower();
                    Console.Title = "A3DA Converter: " + Path.GetFileNameWithoutExtension(file);
                    A = new KKdA3DA();
                         if (ext == ".a3da")
                    {
                        A.A3DAReader   (filepath);
                        A.MsgPackWriter(filepath, JSON);
                    }
                    else if (ext == ".mp"  )
                    {
                        A.MsgPackReader(filepath, JSON);
                        A.IO = File.OpenWriter(filepath + ".a3da", true);
                        if (A.Data.Header.Format < Main.Format.F2LE)
                            A.Data._.CompressF16 = Format == Main.Format.MGF ? 2 : 1;
                        A.Data.Header.Format = Format;

                        if (A.Data.Header.Format > Main.Format.DT && A.Data.Header.Format != Main.Format.FT)
                            A.A3DCWriter(filepath);
                        else
                            A.A3DAWriter();
                    }
                }
                catch (Exception e)
                { Console.WriteLine(e); }
        }
    }
}
