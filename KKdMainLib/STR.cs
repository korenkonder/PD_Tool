using KKdBaseLib;
using KKdMainLib.IO;
using KKdMainLib.F2nd;
using KKdMainLib.MessagePack;

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
            IO.Format = Main.Format.F;
            Header.Signature = IO.ReadInt32();
            if (Header.Signature == 0x41525453)
            {
                Header = IO.ReadHeader(true);
                POF = Header.AddPOF();
                IO.Position = Header.Length;
                
                long Count = IO.ReadInt32Endian();
                Offset = IO.ReadInt32Endian();
                
                if (Offset == 0)
                {
                    Offset = Count;
                    OffsetX = IO.ReadInt64();
                    Count   = IO.ReadInt64();
                    IO.Offset = Header.Length;
                    IO.Format = Main.Format.X;
                    IO.LongOffset += Offset;
                    IO.Position = 0;
                }
                else IO.Position = Header.Length + 0x40;

                STRs = new String[Count];
                for (int i = 0; i < Count; i++)
                    STRs[i].ID  = IO.ReadInt32Endian();

                if (IO.IsX)
                {
                    IO.Offset = Header.Length;
                    for (int i = 0; i < Count; i++)
                        STRs[i].Str.Value = IO.ReadStringAtOffset(STRs[i].Str.Offset + OffsetX);
                }
                else
                {
                    IO.Offset = 0;
                    for (int i = 0; i < Count; i++)
                        STRs[i].Str.Value = STRs[i].Str.Offset > 0 ?
                            IO.ReadStringAtOffset(STRs[i].Str.Offset) : null;
                }

                IO.Position = POF.Offset;
                IO.ReadPOF(ref POF);
            }
            else
            {
                IO.Position -= 4;
                int Count = 0;
                for (int a = 0, i = 0; IO.Position > 0 && IO.Position < IO.Length; i++, Count++)
                {
                    a = IO.ReadInt32();
                    if (a == 0) break;
                }
                STRs = new String[Count];

                for (int i = 0; i < Count; i++)
                {
                    IO.LongPosition = STRs[i].Str.Offset;
                    STRs[i].ID = i;
                    STRs[i].Str.Value = IO.NullTerminatedUTF8();
                }
            }

            IO.Close();
            return 1;
        }

        public void STRWriter(string filepath)
        {
            if (STRs == null || STRs.Length == 0 || Header.Format > Main.Format.F2BE) return;
            uint Offset = 0;
            uint CurrentOffset = 0;
            IO = File.OpenWriter(filepath + (Header.Format > Main.Format.FT ? ".str" : ".bin"), true);
            IO.Format = Header.Format;
            POF = new POF();
            IO.IsBE = IO.Format == Main.Format.F2BE;

            long Count = STRs.LongLength;
            if (IO.Format > Main.Format.FT)
            {
                IO.Position = 0x40;
                IO.WriteX(Count);
                IO.GetOffset(ref POF);
                IO.WriteX(0x80);
                IO.Position = 0x80;
                for (int i = 0; i < Count; i++) IO.Write(0x00L);
                IO.Align(0x10);
            }
            else
            {
                for (int i = 0; i < Count; i++) IO.Write(0x00);
                IO.Align(0x20);
            }

            KKdList<string> UsedSTR = KKdList<string>.New;
            KKdList<int> UsedSTRPos = KKdList<int>.New;
            int[] STRPos = new int[Count];
            for (int i = 0; i < Count; i++)
            {
                if (!UsedSTR.Contains(STRs[i].Str.Value))
                {
                    STRPos[i] = IO.Position;
                    UsedSTRPos.Add(STRPos[i]);
                    UsedSTR.Add(STRs[i].Str.Value);
                    IO.Write(STRs[i].Str.Value + "\0");
                }
                else
                    for (int i2 = 0; i2 < Count; i2++)
                        if (UsedSTR[i2] == STRs[i].Str.Value)
                        { STRPos[i] = UsedSTRPos[i2]; break; }
            }

            if (IO.Format > Main.Format.FT)
            {
                IO.Align(0x10);
                Offset = IO.UIntPosition;
                IO.Position = 0x80;
                for (int i = 0; i < Count; i++)
                {
                    IO.GetOffset(ref POF);
                    IO.WriteEndian(STRPos[i]);
                    IO.WriteEndian(STRs[i].ID);
                }

                IO.UIntPosition = Offset;
                IO.Write(ref POF, 1);
                CurrentOffset = IO.UIntPosition;
                IO.WriteEOFC(0);
                Header.DataSize = (int)(CurrentOffset - 0x40);
                Header.Signature = 0x41525453;
                Header.SectionSize = (int)(Offset - 0x40);
                IO.Position = 0;
                IO.Write(Header);
            }
            else
            {
                IO.Position = 0;
                for (int i = 0; i < Count; i++) IO.Write(STRPos[i]);
            }
            IO.Close();
        }

        public void MsgPackReader(string file, bool JSON)
        {
            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);
            if (!MsgPack.Element("STR", out MsgPack STR)) return;
            Header = new Header();
            System.Enum.TryParse(STR.ReadString("Format"), out Header.Format);

            if (!STR.ElementArray("Strings", out MsgPack Strings)) return;

            STRs = new String[Strings.Array.Length];
            for (int i = 0; i < STRs.Length; i++)
            {
                STRs[i].ID        = Strings[i].ReadInt32 ("ID" );
                STRs[i].Str.Value = Strings[i].ReadString("Str");
            }

            MsgPack = MsgPack.New;
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            if (STRs == null || STRs.Length == 0) return;
            MsgPack STR_ = new MsgPack("STR").Add("Format", Header.Format.ToString());
            MsgPack Strings = new MsgPack(STRs.Length, "Strings");
            for (int i = 0; i < STRs.Length; i++)
            {
                Strings[i] = MsgPack.New.Add("ID", STRs[i].ID);
                if (STRs[i].Str.Value != null || STRs[i].Str.Value != "")
                    Strings[i] = Strings[i].Add("Str", STRs[i].Str.Value);
            }
            STR_.Add(Strings);

            STR_.WriteAfterAll(true, file, JSON);
        }

        public struct String
        {
            public int ID;
            public Pointer<string> Str;

            public override string ToString() => "ID: " + ID + (Str.Value != null || 
                Str.Value != "" ? ("; Str: " + Str.Value) : "");
        }
    }
}
