using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct STR : System.IDisposable
    {        
        private long offset;
        private long offsetX;
        private POF pof;
        private Header header;
        private Stream _IO;

        public String[] STRs;

        public int STRReader(string filepath, string ext)
        {
            _IO =  File.OpenReader(filepath + ext);

            header = new Header();
            _IO.Format = Format.F;
            header.Signature = _IO.RI32();
            if (header.Signature == 0x41525453)
            {
                header = _IO.ReadHeader(true, false);
                pof.Offsets = KKdList<long>.New;
                
                long count = _IO.RI32E();
                offset = _IO.RI32E();
                
                if (offset == 0)
                {
                    offset = count;
                    offsetX = _IO.RI64();
                    count   = _IO.RI64();
                    _IO.O = header.Length;
                    _IO.Format = Format.X;
                    _IO.I64O += offset;
                    _IO.P = 0;
                }
                else _IO.P = header.Length + 0x40;

                STRs = new String[count];
                for (int i = 0; i < count; i++)
                {
                    STRs[i].Str.O = _IO.RI32E();
                    STRs[i].ID    = _IO.RI32E();
                }

                if (_IO.IsX)
                {
                    _IO.I64O = header.Length + offsetX;
                    for (int i = 0; i < count; i++)
                        STRs[i].Str.V = _IO.RSaO(STRs[i].Str.O);
                }
                else
                {
                    _IO.O = 0;
                    for (int i = 0; i < count; i++)
                        STRs[i].Str.V = STRs[i].Str.O > 0 ?
                            _IO.RSaO(STRs[i].Str.O) : null;
                }
            }
            else
            {
                _IO.P -= 4;
                int count = 0;
                for (int a = 0, i = 0; _IO.P > 0 && _IO.P < _IO.L; i++, count++)
                {
                    a = _IO.RI32();
                    if (a == 0) break;
                }
                STRs = new String[count];

                for (int i = 0; i < count; i++)
                {
                    _IO.I64P = STRs[i].Str.O;
                    STRs[i].ID = i;
                    STRs[i].Str.V = _IO.NTUTF8();
                }
            }

            _IO.C();
            return 1;
        }

        public void STRWriter(string filepath)
        {
            if (STRs == null || STRs.Length == 0 || header.Format > Format.F2BE) return;
            uint offset = 0;
            uint currentOffset = 0;
            _IO = File.OpenWriter(filepath + (header.Format > Format.AFT &&
                header.Format < Format.FT ? ".str" : ".bin"), true);
            _IO.Format = header.Format;
            pof.Offsets = KKdList<long>.New;

            long count = STRs.LongLength;
            if (_IO.Format > Format.AFT && _IO.Format < Format.FT)
            {
                _IO.P = 0x40;
                _IO.WX(count, ref pof);
                _IO.WX(0x80);
                _IO.P = 0x80;
                for (int i = 0; i < count; i++) _IO.W(0x00L);
                _IO.A(0x10);
            }
            else
            {
                for (int i = 0; i < count; i++) _IO.W(0x00);
                _IO.A(0x20);
            }

            KKdList<string> usedSTR = KKdList<string>.New;
            KKdList<int> usedSTRPos = KKdList<int>.New;
            int[] STRPos = new int[count];

            usedSTRPos.Add(_IO.P);
            usedSTR.Add("");
            _IO.W(0);
            for (int i = 0; i < count; i++)
            {
                if (!usedSTR.Contains(STRs[i].Str.V))
                {
                    STRPos[i] = _IO.P;
                    usedSTRPos.Add(STRPos[i]);
                    usedSTR.Add(STRs[i].Str.V);
                    _IO.W(STRs[i].Str.V);
                    _IO.W(0);
                }
                else
                    for (int i2 = 0; i2 < count; i2++)
                        if (usedSTR[i2] == STRs[i].Str.V) { STRPos[i] = usedSTRPos[i2]; break; }
            }

            if (_IO.Format > Format.AFT)
            {
                _IO.A(0x10);
                offset = _IO.U32P;
                _IO.P = 0x80;
                for (int i = 0; i < count; i++)
                {
                    pof.Offsets.Add(_IO.P);
                    _IO.WE(STRPos[i]);
                    _IO.WE(STRs[i].ID);
                }

                _IO.U32P = offset;
                _IO.W(pof, false, 1);
                currentOffset = _IO.U32P;
                _IO.WEOFC();
                header.DataSize = (int)(currentOffset - 0x40);
                header.Signature = 0x41525453;
                header.SectionSize = (int)(offset - 0x40);
                _IO.P = 0;
                _IO.W(header, true);
            }
            else
            {
                _IO.P = 0;
                for (int i = 0; i < count; i++) _IO.W(STRPos[i]);
            }
            _IO.C();
        }

        public void MsgPackReader(string file, bool json)
        {
            MsgPack temp;
            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            if ((temp = msgPack["STR"]).NotNull) return;

            header = new Header();
            System.Enum.TryParse(temp.RS("Format"), out header.Format);

            if ((temp = msgPack["Strings", true]).IsNull) return;

            STRs = new String[temp.Array.Length];
            for (int i = 0; i < STRs.Length; i++)
            {
                STRs[i].ID    = temp[i].RI32("ID" );
                STRs[i].Str.V = temp[i].RS  ("Str");
                if (STRs[i].Str.V == null) STRs[i].Str.V = "";
            }

            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (STRs == null || STRs.Length == 0) return;
            MsgPack str = new MsgPack("STR").Add("Format", header.Format.ToString());
            MsgPack strings = new MsgPack(STRs.Length, "Strings");
            for (int i = 0; i < STRs.Length; i++)
            {
                strings[i] = MsgPack.New.Add("ID", STRs[i].ID);
                if (STRs[i].Str.V != null)
                    if (STRs[i].Str.V != "")
                        strings[i] = strings[i].Add("Str", STRs[i].Str.V);
            }
            str.Add(strings);

            str.WriteAfterAll(true, file, json);
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (_IO != null) _IO.Dispose(); STRs = default;
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
