using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct STR : System.IDisposable
    {
        private POF pof;
        private Header header;
        private Stream s;

        public String[] Strings;

        public int STRReader(string filepath, string ext)
        {
            s =  File.OpenReader(filepath + ext);

            header = new Header();
            s.Format = Format.F;
            header.Signature = s.RU32();
            if (header.Signature == 0x41525453)
            {
                header = s.ReadHeader(true, false);
                s.IsBE = header.UseBigEndian;
                s.Format = header.Format;

                int count = s.RI32E();
                int offset = s.RI32E();

                s.P = offset;
                Strings = new String[count];
                for (int i = 0; i < count; i++)
                {
                    Strings[i].Str.O = s.RI32E();
                    Strings[i].ID    = s.RI32E();
                }

                s.O = 0;
                for (int i = 0; i < count; i++)
                    Strings[i].Str.V = Strings[i].Str.O > 0 ?
                        s.RSaO(Strings[i].Str.O) : null;
            }
            else
            {
                if ((header.Signature >> 24) > 0) { header.Format = Format.DT; s.IsBE = true; }
                else header.Format = Format.F;
                int count = 0;
                for (uint i = header.Signature; i != 0 && s.P >= 0 && s.P < s.L; count++)
                    i = s.RU32();
                Strings = new String[count];

                s.P = 0;
                for (int i = 0; i < count; i++)
                {
                    Strings[i].ID = i;
                    Strings[i].Str.V = s.RSaO();
                }
            }

            s.C();
            return 1;
        }

        public void STRWriter(string filepath)
        {
            if (Strings == null || Strings.Length == 0 || header.Format > Format.F2) return;
            uint offset = 0;
            uint currentOffset = 0;
            Format format = header.Format;
            s = File.OpenWriter(filepath + (header.Format > Format.AFT &&
                header.Format < Format.FT ? ".str" : ".bin"), true);
            s.Format = header.Format;
            s.IsBE = header.UseBigEndian;
            pof.Offsets = KKdList<long>.New;

            long count = Strings.LongLength;
            if (s.Format > Format.AFT && s.Format < Format.FT)
            {
                s.P = 0x40;
                s.WX(count, ref pof);
                s.WX(0x80);
                s.P = 0x80;
                for (int i = 0; i < count; i++) s.W(0x00L);
            }
            else
                for (int i = 0; i < count; i++) s.W(0x00);
            s.A(0x10);

            KKdList<string> usedSTR = KKdList<string>.New;
            KKdList<int> usedSTRPos = KKdList<int>.New;
            int[] STRPos = new int[count];

            for (int i = 0; i < count; i++)
            {
                if (usedSTR.Contains(Strings[i].Str.V))
                {
                    for (int i2 = 0; i2 < count; i2++)
                        if (usedSTR[i2] == Strings[i].Str.V)
                        { STRPos[i] = usedSTRPos[i2]; break; }
                }
                else
                {
                    usedSTRPos.Add(STRPos[i] = s.P);
                    usedSTR.Add(Strings[i].Str.V);
                    s.W(Strings[i].Str.V);
                    s.W((byte)0);
                    if (format < Format.F) s.A(0x8);
                }
            }
            s.A(0x4);
            s.L = s.P;

            if (s.Format > Format.AFT)
            {
                s.A(0x10);
                offset = s.PU32;
                s.P = 0x80;
                for (int i = 0; i < count; i++)
                {
                    pof.Offsets.Add(s.P);
                    s.WE(STRPos[i]);
                    s.WE(Strings[i].ID);
                }

                s.PU32 = offset;
                s.W(pof, false, 1);
                s.WEOFC(1);
                currentOffset = s.PU32;
                s.WEOFC();
                header.DataSize = currentOffset - 0x40;
                header.Signature = 0x41525453;
                header.SectionSize = offset - 0x40;
                header.UseSectionSize = true;
                s.P = 0;
                s.W(header, true);
            }
            else
            {
                s.P = 0;
                s.IsBE = header.Format < Format.F;
                for (int i = 0; i < count; i++) s.WE(STRPos[i]);
            }
            s.C();
        }

        public void MsgPackReader(string file, bool json)
        {
            header = default;
            Strings = default;
            MsgPack temp;
            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            if ((temp = msgPack["STR"]).IsNull) return;

            header = new Header();
            if (!System.Enum.TryParse(temp.RS("Format"), out header.Format)) return;
            bool? isBE = temp.RnB("UseBigEndian");
            if (isBE.HasValue) header.UseBigEndian = isBE.Value;

            if ((temp = temp["Strings", true]).IsNull) return;

            Strings = new String[temp.Array.Length];
            for (int i = 0; i < Strings.Length; i++)
            {
                Strings[i].ID    = temp[i].RI32("ID" );
                Strings[i].Str.V = temp[i].RS  ("Str");
                if (Strings[i].Str.V == null) Strings[i].Str.V = "";
            }

            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (Strings == null || Strings.Length == 0) return;
            MsgPack str = new MsgPack("STR").Add("Format", header.Format.ToString());
            if (header.UseBigEndian) str.Add("UseBigEndian", header.UseBigEndian);
            MsgPack strings = new MsgPack(Strings.Length, "Strings");
            for (int i = 0; i < Strings.Length; i++)
            {
                strings[i] = MsgPack.New.Add("ID", Strings[i].ID);
                if (Strings[i].Str.V != null)
                    if (Strings[i].Str.V != "")
                        strings[i] = strings[i].Add("Str", Strings[i].Str.V);
            }
            str.Add(strings);

            str.WriteAfterAll(false, true, file, json);
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; Strings = default;
                pof = default; header = default; disposed = true; } }

        public struct String
        {
            public int ID;
            public Pointer<string> Str;

            public override string ToString() => "ID: " + ID + (Str.V != null ||
                Str.V != "" ? ("; Str: " + Str.V) : "");
        }
    }
}
