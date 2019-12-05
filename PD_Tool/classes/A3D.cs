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

            Format format = Format.NULL;
            string choose = "";
            if (mp)
            {
                Console.Clear();
                Program.ConsoleDesign(true);
                Program.ConsoleDesign("          Choose type of format to export:");
                Program.ConsoleDesign(false);
                Program.ConsoleDesign("1. A3DA [DT/AC/F]");
                Program.ConsoleDesign("2. A3DC [DT/AC/F]");
                Program.ConsoleDesign("3. A3DA [AFT/FT] ");
                Program.ConsoleDesign("4. A3DC [AFT/FT] ");
                Program.ConsoleDesign("5. A3DC [F2]     ");
                Program.ConsoleDesign("6. A3DC [MGF]    ");
                Program.ConsoleDesign("7. A3DC [X]      ");
                Program.ConsoleDesign(false);
                Program.ConsoleDesign(true);
                Console.WriteLine();
                choose = Console.ReadLine();
                     if (choose == "1") format = Format.DT  ;
                else if (choose == "2") format = Format.F   ;
                else if (choose == "3") format = Format.AFT ;
                else if (choose == "4") format = Format.AFT ;
                else if (choose == "5") format = Format.F2LE;
                else if (choose == "6") format = Format.MGF ;
                else if (choose == "7") format = Format.X   ;
                else return;
            }

            int state;
            string filepath, ext;
            KKdA3DA a3da;
            foreach (string file in fileNames)
            {
                ext      = Path.GetExtension(file);
                filepath = file.Replace(ext, "");
                ext      = ext.ToLower();

                Console.Title = "A3DA Converter: " + Path.GetFileNameWithoutExtension(file);
                if (ext == ".farc")
                    using (KKdFARC farc = new KKdFARC(file))
                        FARCProcessor(farc, choose, format);
                else if (ext == ".a3da")
                    using (a3da = new KKdA3DA(true))
                    {
                        state = a3da.A3DAReader(filepath);
                        if (state == 1) a3da.MsgPackWriter(filepath, json);
                    }
                else if (ext == ".mp" || ext == ".json")
                    using (a3da = new KKdA3DA(true))
                    {
                        a3da.MsgPackReader(filepath, ext == ".json");
                        a3da.Data._.CompressF16 = format > Format.AFT && format <
                            Format.FT ? format == Format.MGF ? 2 : 1 : 0;
                        a3da.Head.Format = format;

                        File.WriteAllBytes(filepath + ".a3da", (choose != "1" &&
                            choose != "3") ? a3da.A3DCWriter() : a3da.A3DAWriter());
                    }
            }
        }

        private static void FARCProcessor(KKdFARC farc, string choose, Format format)
        {
            if (!farc.HeaderReader()) return;
            if (!farc.HasFiles) return;

            KKdList<string> list = KKdList<string>.New;
            for (int i = 0; i < farc.Files.Count; i++)
            {
                string file = farc.Files[i].Name.ToLower();
                bool div = false;
                for (int i0 = 0; i0 < 159 && !div; i0++)
                    if (file.Contains("div_" + i0)) div = true;

                if (!div && file.EndsWith(".a3da")) list.Add(file);
            }

            KKdList<string> A3DAlist = KKdList<string>.New;
            for (int i = 0; i < farc.Files.Count; i++)
                if (farc.Files[i].Name.ToLower().EndsWith(".a3da"))
                    A3DAlist.Add(farc.Files[i].Name);

            byte[] data = null;
            if (list.Count == A3DAlist.Count || (format > Format.AFT && format < Format.FT))
            {
                KKdA3DA a3da;
                for (int i = 0; i < A3DAlist.Count; i++)
                    using (a3da = new KKdA3DA(true))
                    {
                        data = farc.FileReader(A3DAlist[i]);
                        int state = a3da.A3DAReader(data);
                        if (state == 1)
                        {
                            KKdFARC.FARCFile file = farc.Files[i];
                            a3da.Data._.CompressF16 = format > Format.AFT && format <
                                Format.FT ? format == Format.MGF ? 2 : 1 : 0;
                            a3da.Head.Format = format;
                            file.Data = (choose != "1" && choose != "3") ? a3da.A3DCWriter() : a3da.A3DAWriter();
                            farc.Files[i] = file;
                        }
                    }
                farc.Save();
                return;
            }

            KKdA3DA[] a3daArray;
            using (KKdList<KKdA3DA> a3daList = KKdList<KKdA3DA>.New)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    KKdA3DA a3da;
                    using (a3da = new KKdA3DA(true))
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
                float Div = a3daArray[i].Data.PlayControl.Div.Value;
                KKdA3DA a3da;
                for (int i1 = 1; i1 < Div; i1++)
                    using (a3da = new KKdA3DA(true))
                    {
                        string file = Path.GetFileNameWithoutExtension(list[i]) +
                            "_div_" + i1 + Path.GetExtension(list[i]);
                        data = farc.FileReader(file);
                        int state = a3da.A3DAReader(data);
                        if (state == 1) a3daArray[i].A3DAMerger(ref a3da.Data);
                    }
                a3daArray[i].Data.PlayControl.Div = null;
            }

            farc.Files.Capacity = list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                KKdFARC.FARCFile file = default;
                file.Name = list[i];
                a3daArray[i].Data._.CompressF16 = format > Format.AFT && format <
                    Format.FT ? format == Format.MGF ? 2 : 1 : 0;
                a3daArray[i].Head.Format = format;
                file.Data = (choose != "1" && choose != "3") ? a3daArray[i].A3DCWriter() : a3daArray[i].A3DAWriter();
                farc.Files.Add(file);
            }
            farc.Save();
            return;
        }
    }
}
