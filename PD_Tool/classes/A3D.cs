using System;
using KKdBaseLib;
using KKdMainLib.IO;
using KKdA3DA = KKdMainLib.A3DA;
using KKdFARC = KKdMainLib.FARC;

namespace PD_Tool
{
    class A3D
    {
        public static void Processor(bool json)
        {
            Console.Title = "A3DA Converter";
            Program.Choose(1, "a3da", out string[] fileNames);
            if (fileNames.Length < 1) return;

            bool mp = false;
            foreach (string file in fileNames)
                if (file.EndsWith(".mp") || file.EndsWith(".json") || file.EndsWith(".farc")) { mp = true; break; }

            bool a3dcOpt = false;
            Format format = Format.NULL;
            string choose = "";
            Console.Clear();
            Program.ConsoleDesign(true);
            Program.ConsoleDesign("          Choose type of format to export:");
            Program.ConsoleDesign(false);
            Program.ConsoleDesign("        Tip: A3DC Opt - A3DC Optimization");
            Program.ConsoleDesign("     WARNING! CHECK RESULTS AFTER CONVERTING    ");
            Program.ConsoleDesign(false);
            Program.ConsoleDesign("1. A3DA [DT/AC/F]");
            Program.ConsoleDesign("2. A3DC [DT/AC/F]");
            Program.ConsoleDesign("3. A3DC [DT/AC/F] (A3DC Opt)");
            Program.ConsoleDesign("4. A3DA [AFT/FT/M39]");
            Program.ConsoleDesign("5. A3DC [AFT/FT/M39]");
            Program.ConsoleDesign("6. A3DC [AFT/FT/M39] (A3DC Opt)");
            Program.ConsoleDesign("7. A3DC [F2]");
            Program.ConsoleDesign("8. A3DC [F2] (A3DC Opt)");
            Program.ConsoleDesign("9. A3DC [MGF]");
            Program.ConsoleDesign("A. A3DC [MGF] (A3DC Opt)");
            Program.ConsoleDesign("B. A3DC [X]");
            Program.ConsoleDesign("C. A3DC [X] (A3DC Opt)");
            Program.ConsoleDesign("D. A3DC [XHD]");
            Program.ConsoleDesign("E. A3DC [XHD] (A3DC Opt)");
            if (!mp) Program.ConsoleDesign($"F. {(json ? "JSON" : "MsgPack")}");
            Program.ConsoleDesign(false);
            Program.ConsoleDesign(true);
            Console.WriteLine();
            choose = Console.ReadLine().ToUpper();
                 if (choose == "1")   format = Format.DT ;
            else if (choose == "2")   format = Format.F  ;
            else if (choose == "3") { format = Format.F  ; a3dcOpt = true; }
            else if (choose == "4")   format = Format.AFT;
            else if (choose == "5")   format = Format.AFT;
            else if (choose == "6") { format = Format.AFT; a3dcOpt = true; }
            else if (choose == "7")   format = Format.F2 ;
            else if (choose == "8") { format = Format.F2 ; a3dcOpt = true; }
            else if (choose == "9")   format = Format.MGF;
            else if (choose == "A") { format = Format.MGF; a3dcOpt = true; }
            else if (choose == "B")   format = Format.X  ;
            else if (choose == "C") { format = Format.X  ; a3dcOpt = true; }
            else if (choose == "D")   format = Format.XHD;
            else if (choose == "E") { format = Format.XHD; a3dcOpt = true; }
            else if (choose == "F")   format = Format.NULL;
            else return;

            int state;
            string filepath, ext;
            KKdA3DA a3da;
            KKdFARC farc;
            foreach (string file in fileNames)
            {
                filepath = Path.RemoveExtension(file);
                ext      = Path.GetExtension(file).ToLower();

                Console.Title = "A3DA Converter: " + Path.GetFileNameWithoutExtension(file);
                if (ext == ".farc")
                    using (farc = new KKdFARC(file))
                        FARCProcessor(farc, choose, format, a3dcOpt);
                else if (ext == ".a3da")
                    using (a3da = new KKdA3DA(a3dcOpt))
                    {
                        state = a3da.A3DAReader(filepath);
                        if (state == 1)
                            if (choose == "9") a3da.MsgPackWriter(filepath, json);
                            else
                            {
                                a3da.Head.Format = format;
                                File.WriteAllBytes(filepath + ".a3da", (choose == "1" ||
                                    choose == "4") ? a3da.A3DAWriter() : a3da.A3DCWriter());
                            }
                    }
                else if (ext == ".mp" || ext == ".json")
                    using (a3da = new KKdA3DA(a3dcOpt))
                    {
                        a3da.MsgPackReader(filepath, ext == ".json");
                        a3da.Head.Format = format;

                        File.WriteAllBytes(filepath + ".a3da", (choose == "1" ||
                            choose == "4") ? a3da.A3DAWriter() : a3da.A3DCWriter());
                    }
            }
        }

        private static void FARCProcessor(KKdFARC farc, string choose, Format format, bool a3dcOpt)
        {
            if (!farc.HeaderReader() || !farc.HasFiles) return;

            KKdList<string> list = KKdList<string>.New;
            for (int i = 0; i < farc.Files.Count; i++)
            {
                string file = farc.Files[i].Name.ToLower();
                if (!file.Contains("_div_") && file.EndsWith(".a3da")) list.Add(file);
            }

            KKdList<string> A3DAlist = KKdList<string>.New;
            for (int i = 0; i < farc.Files.Count; i++)
                if (farc.Files[i].Name.ToLower().EndsWith(".a3da"))
                    A3DAlist.Add(farc.Files[i].Name);

            byte[] data = null;
            if (list.Count == A3DAlist.Count)
            {
                KKdA3DA a3da;
                for (int i = 0; i < A3DAlist.Count; i++)
                    using (a3da = new KKdA3DA(a3dcOpt))
                    {
                        data = farc.FileReader(A3DAlist[i]);
                        int state = a3da.A3DAReader(data);
                        if (state == 1)
                        {
                            KKdFARC.FARCFile file = farc.Files[i];
                            a3da.Head.Format = format;
                            file.Data = (choose == "1"|| choose == "4")
                                ? a3da.A3DAWriter() : a3da.A3DCWriter();
                            farc.Files[i] = file;
                        }
                    }
                farc.Save();
                return;
            }

            KKdA3DA[] a3daArray;
            using (KKdList<KKdA3DA> a3daList = KKdList<KKdA3DA>.NewReserve(list.Count))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    KKdA3DA a3da;
                    using (a3da = new KKdA3DA(a3dcOpt))
                    {
                        data = farc.FileReader(list[i]);
                        int state = a3da.A3DAReader(data);
                        if (state == 1) a3daList.Add(a3da);
                    }
                }
                a3daArray = a3daList.ToArray();
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (a3daArray[i].Data.PlayControl.Div == null) continue;
                int div = a3daArray[i].Data.PlayControl.Div.Value;
                string filename = Path.RemoveExtension(list[i]);
                string ext      = Path.GetExtension(list[i]).ToLower();
                KKdA3DA a3da;
                for (int i1 = 1; i1 < div; i1++)
                    using (a3da = new KKdA3DA(a3dcOpt))
                    {
                        string file = filename + "_div_" + i1 + ext;
                        data = farc.FileReader(file);
                        int state = a3da.A3DAReader(data);
                        if (state == 1) a3daArray[i].A3DAMerger(ref a3da.Data);
                    }
                a3daArray[i].Data.PlayControl.Div = null;
            }

            farc.Files.Capacity = 0;
            farc.Files.Capacity = list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                KKdFARC.FARCFile file = default;
                file.Name = list[i];
                a3daArray[i].Head.Format = format;
                file.Data = (choose == "1" || choose == "4")
                    ? a3daArray[i].A3DAWriter() : a3daArray[i].A3DCWriter();
                farc.Files.Add(file);
            }
            farc.Save();
            return;
        }
    }
}
