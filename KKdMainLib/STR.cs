using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct STR : System.IDisposable
    {
        private POF pof;
        private Header header;
        private Stream _IO;

        public String[] Strings;

        public int STRReader(string filepath, string ext)
        {
            _IO =  File.OpenReader(filepath + ext);

            header = new Header();
            _IO.Format = Format.F;
            header.Signature = _IO.RI32();
            if (header.Signature == 0x41525453)
            {
                header = _IO.ReadHeader(true, false);
                _IO.IsBE = header.UseBigEndian;
                _IO.Format = header.Format;

                int count = _IO.RI32E();
                int offset = _IO.RI32E();

                _IO.P = offset;
                Strings = new String[count];
                for (int i = 0; i < count; i++)
                {
                    Strings[i].Str.O = _IO.RI32E();
                    Strings[i].ID    = _IO.RI32E();
                }

                _IO.O = 0;
                for (int i = 0; i < count; i++)
                    Strings[i].Str.V = Strings[i].Str.O > 0 ?
                        _IO.RSaO(Strings[i].Str.O) : null;
            }
            else
            {
                if ((header.Signature >> 24) > 0) { header.Format = Format.DT; _IO.IsBE = true; }
                else header.Format = Format.F;
                int count = 0;
                for (int i = header.Signature; i != 0 && _IO.P >= 0 && _IO.P < _IO.L; count++)
                    i = _IO.RI32();
                Strings = new String[count];

                _IO.P = 0;
                for (int i = 0; i < count; i++)
                {
                    Strings[i].ID = i;
                    Strings[i].Str.V = _IO.RSaO();
                }
            }

            _IO.C();
            return 1;
        }

        public void STRWriter(string filepath)
        {
            if (Strings == null || Strings.Length == 0 || header.Format > Format.F2) return;
            uint offset = 0;
            uint currentOffset = 0;
            Format format = header.Format;
            _IO = File.OpenWriter(filepath + (header.Format > Format.AFT &&
                header.Format < Format.FT ? ".str" : ".bin"), true);
            _IO.Format = header.Format;
            pof.Offsets = KKdList<long>.New;

            long count = Strings.LongLength;
            if (_IO.Format > Format.AFT && _IO.Format < Format.FT)
            {
                _IO.P = 0x40;
                _IO.WX(count, ref pof);
                _IO.WX(0x80);
                _IO.P = 0x80;
                for (int i = 0; i < count; i++) _IO.W(0x00L);
            }
            else
                for (int i = 0; i < count; i++) _IO.W(0x00);
            _IO.A(0x10);

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
                    usedSTRPos.Add(STRPos[i] = _IO.P);
                    usedSTR.Add(Strings[i].Str.V);
                    _IO.W(Strings[i].Str.V);
                    _IO.W((byte)0);
                    if (format < Format.F) _IO.A(0x8);
                }
            }
            _IO.A(0x4);
            _IO.L = _IO.P;

            if (_IO.Format > Format.AFT)
            {
                _IO.A(0x10);
                offset = _IO.PU32;
                _IO.P = 0x80;
                for (int i = 0; i < count; i++)
                {
                    pof.Offsets.Add(_IO.P);
                    _IO.WE(STRPos[i]);
                    _IO.WE(Strings[i].ID);
                }

                _IO.PU32 = offset;
                _IO.W(pof, false, 1);
                _IO.WEOFC(1);
                currentOffset = _IO.PU32;
                _IO.WEOFC();
                header.DataSize = (int)(currentOffset - 0x40);
                header.Signature = 0x41525453;
                header.SectionSize = (int)(offset - 0x40);
                header.UseSectionSize = true;
                _IO.P = 0;
                _IO.W(header, true);
            }
            else
            {
                _IO.P = 0;
                _IO.IsBE = header.Format < Format.F;
                for (int i = 0; i < count; i++) _IO.WE(STRPos[i]);
            }
            _IO.C();
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
        { if (!disposed) { if (_IO != null) _IO.D(); _IO = null; Strings = default;
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
