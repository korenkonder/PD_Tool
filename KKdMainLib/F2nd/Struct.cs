using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib.F2nd
{
    public struct Struct
    {
        public Header Header;
        public byte[] Data;
        public Struct[] SubStructs;
        public bool EOFC;
        public ENRS[] ENRS;
        public KKdList<long> POF;

        public long DataOffset;

        public override string ToString() => Header.ToString() + (SubStructs != null ?
            "; SubStructs: " + SubStructs.Length : "")+ (ENRS != null ? "; Has ENRS" : "") +
            (POF.NotNull ? "; Has POF" : "") + (EOFC ? "; Has EOFC" : "");

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
                { Struct.EOFC = true; break; }
                else if (Header.ID == 0 && (Header.Signature == 0x30464F50 ||
                    Header.Signature == 0x31464F50 || Header.Signature == 0x53524E45))
                    SubStructs.Add(new Struct { Header = Header, DataOffset =
                        stream.Position, Data = stream.ReadBytes(Header.SectionSize) });
                else if (Header.ID <= ID)
                { stream.LongPosition -= Header.Length; break; }
                else SubStructs.Add(ReadStruct(ref stream, Header));
            }

            for (int i = 0; i < SubStructs.Capacity; i++)
            {
                string Sig = SubStructs[i].Header.ToString();
                if (Sig == "ENRS" || Sig == "EOFC" || Sig == "POF0" || Sig == "POF1")
                {
                         if (Sig == "EOFC") Struct.EOFC = true;
                    else if (Sig == "ENRS") Struct.ENRS = F2nd.ENRS.Read(SubStructs[i].Data);
                    else                    Struct.POF  = F2nd.POF .Read(SubStructs[i].Data, Sig == "POF1");
                    SubStructs.RemoveAt(i); SubStructs.Capacity--; i--;
                }
            }

            if (SubStructs.Capacity > 0) Struct.SubStructs = SubStructs.ToArray();
            return Struct;
        }
    }
}
