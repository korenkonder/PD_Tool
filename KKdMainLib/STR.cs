using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public class STR
    {
        public STR()
        { Offset = 0; OffsetX = 0; STRs = null; Header = new Header(); }
        
        private long Offset;
        private long OffsetX;
        private POF POF;
        private Header Header;
        private Stream IO;

        public String[] STRs;

        public int STRReader(string filepath, string ext)
        {
            IO =  File.OpenReader(filepath + ext);

            Header = new Header();
            IO.Format = Format.F;
            Header.Signature = IO.RI32();
            if (Header.Signature == 0x41525453)
            {
                Header = IO.ReadHeader(true, false);
                POF.Offsets = KKdList<long>.New;
                
                long Count = IO.RI32E();
                Offset = IO.RI32E();
                
                if (Offset == 0)
                {
                    Offset = Count;
                    OffsetX = IO.RI64();
                    Count   = IO.RI64();
                    IO.O = Header.Length;
                    IO.Format = Format.X;
                    IO.I64O += Offset;
                    IO.P = 0;
                }
                else IO.P = Header.Length + 0x40;

                STRs = new String[Count];
                for (int i = 0; i < Count; i++)
                {
                    STRs[i].Str.O = IO.RI32E();
                    STRs[i].ID         = IO.RI32E();
                }

                if (IO.IsX)
                {
                    IO.I64O = Header.Length + OffsetX;
                    for (int i = 0; i < Count; i++)
                        STRs[i].Str.V = IO.RSaO(STRs[i].Str.O);
                }
                else
                {
                    IO.O = 0;
                    for (int i = 0; i < Count; i++)
                        STRs[i].Str.V = STRs[i].Str.O > 0 ?
                            IO.RSaO(STRs[i].Str.O) : null;
                }
            }
            else
            {
                IO.P -= 4;
                int Count = 0;
                for (int a = 0, i = 0; IO.P > 0 && IO.P < IO.L; i++, Count++)
                {
                    a = IO.RI32();
                    if (a == 0) break;
                }
                STRs = new String[Count];

                for (int i = 0; i < Count; i++)
                {
                    IO.I64P = STRs[i].Str.O;
                    STRs[i].ID = i;
                    STRs[i].Str.V = IO.NTUTF8();
                }
            }

            IO.C();
            return 1;
        }

        public void STRWriter(string filepath)
        {
            if (STRs == null || STRs.Length == 0 || Header.Format > Format.F2BE) return;
            uint Offset = 0;
            uint CurrentOffset = 0;
            IO = File.OpenWriter(filepath + (Header.Format > Format.FT ? ".str" : ".bin"), true);
            IO.Format = Header.Format;
            POF.Offsets = KKdList<long>.New;
            IO.IsBE = IO.Format == Format.F2BE;

            long Count = STRs.LongLength;
            if (IO.Format > Format.FT)
            {
                IO.P = 0x40;
                IO.WX(Count, ref POF);
                IO.WX(0x80);
                IO.P = 0x80;
                for (int i = 0; i < Count; i++) IO.W(0x00L);
                IO.A(0x10);
            }
            else
            {
                for (int i = 0; i < Count; i++) IO.W(0x00);
                IO.A(0x20);
            }

            KKdList<string> UsedSTR = KKdList<string>.New;
            KKdList<int> UsedSTRPos = KKdList<int>.New;
            int[] STRPos = new int[Count];

            UsedSTRPos.Add(IO.P);
            UsedSTR.Add("");
            IO.W(0);
            for (int i = 0; i < Count; i++)
            {
                if (!UsedSTR.Contains(STRs[i].Str.V))
                {
                    STRPos[i] = IO.P;
                    UsedSTRPos.Add(STRPos[i]);
                    UsedSTR.Add(STRs[i].Str.V);
                    IO.W(STRs[i].Str.V);
                    IO.W(0);
                }
                else
                    for (int i2 = 0; i2 < Count; i2++)
                        if (UsedSTR[i2] == STRs[i].Str.V) { STRPos[i] = UsedSTRPos[i2]; break; }
            }

            if (IO.Format > Format.FT)
            {
                IO.A(0x10);
                Offset = IO.U32P;
                IO.P = 0x80;
                for (int i = 0; i < Count; i++)
                {
                    POF.Offsets.Add(IO.P);
                    IO.WE(STRPos[i]);
                    IO.WE(STRs[i].ID);
                }

                IO.U32P = Offset;
                POF.Depth = 1;
                IO.W(POF);
                CurrentOffset = IO.U32P;
                IO.WEOFC();
                Header.DataSize = (int)(CurrentOffset - 0x40);
                Header.Signature = 0x41525453;
                Header.SectionSize = (int)(Offset - 0x40);
                IO.P = 0;
                IO.W(Header, true);
            }
            else
            {
                IO.P = 0;
                for (int i = 0; i < Count; i++) IO.W(STRPos[i]);
            }
            IO.C();
        }

        public void MsgPackReader(string file, bool JSON)
        {
            MsgPack Temp;
            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);
            if ((Temp = MsgPack["STR"]).NotNull) return;

            Header = new Header();
            System.Enum.TryParse(Temp.RS("Format"), out Header.Format);

            if ((Temp = MsgPack["Strings", true]).IsNull) return;

            STRs = new String[Temp.Array.Length];
            for (int i = 0; i < STRs.Length; i++)
            {
                STRs[i].ID        = Temp[i].RI32 ("ID" );
                STRs[i].Str.V = Temp[i].RS("Str");
                if (STRs[i].Str.V == null) STRs[i].Str.V = "";
            }

            MsgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            if (STRs == null || STRs.Length == 0) return;
            MsgPack STR_ = new MsgPack("STR").Add("Format", Header.Format.ToString());
            MsgPack Strings = new MsgPack(STRs.Length, "Strings");
            for (int i = 0; i < STRs.Length; i++)
            {
                Strings[i] = MsgPack.New.Add("ID", STRs[i].ID);
                if (STRs[i].Str.V != null)
                    if (STRs[i].Str.V != "")
                        Strings[i] = Strings[i].Add("Str", STRs[i].Str.V);
            }
            STR_.Add(Strings);

            STR_.WriteAfterAll(true, file, JSON);
        }

        public struct String
        {
            public int ID;
            public Pointer<string> Str;

            public override string ToString() => "ID: " + ID + (Str.V != null || 
                Str.V != "" ? ("; Str: " + Str.V) : "");
        }
    }
}
