using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib.F2nd
{
    public struct Struct
    {
        public Header Header;
        public byte[] Data;
        public Struct[] SubStructs;
        public bool HasEOFC;
        public Mini POF;
        public Mini ENRS;

        public long DataOffset;

        public override string ToString() => Header.ToString() + (SubStructs != null ? "; SubStructs: " +
            SubStructs.Length : "") + (POF != null ? "; Has " + POF.Header.ToString() : "") +
            (ENRS != null ? "; Has ENRS" : "") + (HasEOFC ? "; Has EOFC" : "");

        public static Struct ReadStruct(byte[] Data)
        {
            Stream stream = File.OpenReader(Data);
            Struct Struct = ReadStruct(ref stream, stream.ReadHeader(false));
            stream.Close();
            return Struct;
        }

        private static Struct ReadStruct(ref Stream stream, Header Header)
        {
            Struct Struct = new Struct { Header = Header, DataOffset =
                stream.Position, Data = stream.ReadBytes(Header.SectionSize) };
            int ID = Header.ID;

            KKdList<Struct> SubStructs = KKdList<Struct>.New;
            long Length = stream.Length - stream.Position;
            long Position = 0;
            while (Length > Position)
            {
                Header = stream.ReadHeader(false);
                Position += Header.Length + Header.DataSize;
                if (Header.ID == ID && Header.Signature == 0x43464F45)
                { Struct.HasEOFC = true; break; }
                else if (Header.ID == 0 && Header.Signature == 0x53524E45)
                    SubStructs.Add(new Struct { Header = Header, DataOffset =
                        stream.Position, Data = stream.ReadBytes(Header.SectionSize) });
                else if (Header.ID <= ID)
                { stream.LongPosition -= Header.Length; break; }
                else SubStructs.Add(ReadStruct(ref stream, Header));
                //(Header.Signature == 0x30464F50 || Header.Signature ==
                //    0x31464F50 || Header.Signature == 0x53524E45)
            }

            for (int i = 0; i < SubStructs.Capacity; i++)
            {
                string Sig = SubStructs[i].Header.ToString();
                if (Sig == "ENRS" || Sig == "POF0" || Sig == "POF1")
                {
                    if (Sig == "ENRS") Struct.ENRS = (Mini)SubStructs[i];
                    else               Struct.POF  = (Mini)SubStructs[i];
                    SubStructs.RemoveAt(i); SubStructs.Capacity--; i--;
                }
            }

            if (SubStructs.Capacity > 0)
            {
                Struct.SubStructs = SubStructs.ToArray();
            }
            return Struct;
        }

        public class Mini
        {
            public Header Header;
            public byte[] Data;
            public bool HasEOFC;

            public static explicit operator   Mini(Struct Struct) =>
                new   Mini() { Header = Struct.Header, Data = Struct.Data, HasEOFC = Struct.HasEOFC };

            public static explicit operator Struct(  Mini Struct) =>
                new Struct() { Header = Struct.Header, Data = Struct.Data, HasEOFC = Struct.HasEOFC };

            public override string ToString() => Header.ToString() + (HasEOFC ? "; Has EOFC" : "");
        }
    }
}
