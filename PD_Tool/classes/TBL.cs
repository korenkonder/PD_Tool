using System;
using KKdBaseLib;
using KKdMainLib;
using KKdMainLib.IO;
using KKdFARC = KKdMainLib.FARC;

namespace PD_Tool
{
    public class TBL
    {
        public static void Processor(bool json)
        {
            Console.Title = "Table Converter";
            Program.Choose(1, "tbl", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            Tables tbl;
            foreach (string file in fileNames)
                using (tbl = new Tables())
                {
                    filepath = Path.RemoveExtension(file);
                    ext      = Path.GetExtension(file).ToLower();

                    Console.Title = "Table Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".bin")
                    {
                        tbl.BINReader    (filepath);
                        tbl.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".farc")
                        using (KKdFARC farc = new KKdFARC(file))
                            FARCProcessor(ref tbl, farc, filepath, json);
                    else if (ext == ".json" || ext == ".mp")
                    {
                        tbl.MsgPackReader(filepath, ext == ".json");
                        tbl.BINWriter    (filepath);
                    }
                }
        }

        private static void FARCProcessor(ref Tables tbl, KKdFARC farc, string filepath, bool json)
        {
            if (!farc.HeaderReader()) return;
            if (!farc.HasFiles) return;

            int file = -1;
            KKdList<string> list = KKdList<string>.New;
            for (int i = 0; i < farc.Files.Count; i++)
                if (farc.Files[i].Name.ToLower().EndsWith(".bin")) { file = i; break; }
            if (file == -1) return;

            farc.Unpack(false);
            tbl.BINReader    (farc.Files[file].Data);
            tbl.MsgPackWriter(filepath.Replace(Path.GetFileName(filepath),
                Path.GetFileNameWithoutExtension(farc.Files[file].Name)), json);
        }
    }
}
