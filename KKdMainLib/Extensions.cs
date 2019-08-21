using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public static class HeaderExtensions
    {
        public static Header ReadHeader(this Stream stream, bool Seek, bool ReadSectionSignature = true)
        {
            if (Seek)
                if (stream.LongPosition > 4) stream.LongPosition -= 4;
                else                         stream.LongPosition  = 0;
            return stream.ReadHeader(ReadSectionSignature);
        }

        public static Header ReadHeader(this Stream stream, bool ReadSectionSignature = true)
        {
            Header Header = new Header { Format = Format.F2LE, Signature = stream.ReadInt32(),
                DataSize = stream.ReadInt32(), Length = stream.ReadInt32() };
            if (stream.ReadUInt32() == 0x18000000)
            { Header.Format = Format.F2BE; }
            Header.ID = stream.ReadInt32();
            Header.SectionSize = stream.ReadInt32();
            Header.SubID = stream.ReadInt32();
            stream.ReadInt32();
            if (Header.Length == 0x40)
            {
                stream.ReadInt64();
                stream.ReadInt64();
                Header.InnerSignature = stream.ReadInt32();
                stream.ReadInt32();
                stream.ReadInt64();
            }
            stream.Format = Header.Format;
            if (ReadSectionSignature) Header.SectionSignature = stream.ReadInt32Endian();
            return Header;
        }

        public static void Write(this Stream stream, Header Header, bool Extended = false)
        {
            stream.Write(Header.Signature);
            stream.Write(Header.DataSize);
            stream.Write((Header.Format < Format.X && Extended) ? 0x40 : 0x20);
            stream.Write(Header.Format == Format.F2BE ? 0x18000000 : 0x10000000);
            stream.Write(Header.ID);
            stream.Write(Header.SectionSize);
            stream.Write(Header.SubID);
            stream.Write(0x00);
            if (Header.Format < Format.X && Extended)
            {
                stream.Write(Header.Format < Format.MGF ? (int)((Header.SectionSignature ^
                    (Header.DataSize * (long)Header.Signature)) - Header.ID + Header.SectionSize) : 0);
                stream.Write(0x00);
                stream.Write(0x00L);
                stream.Write(Header.InnerSignature);
                stream.Write(0x00);
                stream.Write(0x00L);
            }
        }

        public static void WriteEOFC(this Stream stream, int ID = 0) =>
            stream.Write(new Header { ID = ID, Length = 0x20, Signature = 0x43464F45 });
    }

    public static class POFExtensions
    {
        public static void Write(this Stream stream, ref KKdList<long> Offsets, int ID, bool ShiftX)
        {
            byte[] data = POF.Write(Offsets, ShiftX);
            Header Header = new Header { ID = ID, Format = Format.F2LE,
                Length = 0x20, Signature = ShiftX ? 0x31464F50 : 0x30464F50 };
            Header.DataSize = Header.SectionSize = data.Length;
            stream.Write(Header);
            stream.Write(data);
            stream.WriteEOFC(ID);
        }
    }

    public static class StructExtensions
    {
        public static Struct ReadStruct(byte[] Data)
        {
            Stream stream = File.OpenReader(Data);
            Struct Struct = stream.ReadStruct(stream.ReadHeader(false));
            stream.Close();
            return Struct;
        }

        public static Struct ReadStruct(this Stream stream, Header Header)
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
                else SubStructs.Add(stream.ReadStruct(Header));
            }

            for (int i = 0; i < SubStructs.Capacity; i++)
            {
                string Sig = SubStructs[i].Header.ToString();
                if (Sig == "ENRS" || Sig == "EOFC" || Sig == "POF0" || Sig == "POF1")
                {
                         if (Sig == "EOFC") Struct.EOFC = true;
                    else if (Sig == "ENRS") Struct.ENRS = ENRS.Read(SubStructs[i].Data);
                    else                    Struct.POF  = POF .Read(SubStructs[i].Data, Sig == "POF1");
                    SubStructs.RemoveAt(i); SubStructs.Capacity--; i--;
                }
            }

            if (SubStructs.Capacity > 0) Struct.SubStructs = SubStructs.ToArray();
            return Struct;
        }
    }
    
    public static class MPExt
    {
        public static MsgPack ReadMP(this byte[] array, bool JSON = false)
        {
            MsgPack MsgPack;
            if (JSON)
            { JSON IO = new JSON(File.OpenReader(array));
                MsgPack = IO.Read(    ); IO.Close(); }
            else
            {   MP IO = new   MP(File.OpenReader(array));
                MsgPack = IO.Read(true); IO.Close(); }
            return MsgPack;
        }

        public static MsgPack ReadMPAllAtOnce(this string file, bool JSON = false)
        {
            MsgPack MsgPack;
            if (JSON)
            { JSON IO = new JSON(File.OpenReader(file + ".json", true));
                MsgPack = IO.Read(    ); IO.Close(); }
            else
            {   MP IO = new   MP(File.OpenReader(file + ".mp"  , true));
                MsgPack = IO.Read(true); IO.Close(); }
            return MsgPack;
        }

        public static MsgPack ReadMP(this string file, bool JSON = false)
        {
            MsgPack MsgPack;
            if (JSON)
            { JSON IO = new JSON(File.OpenReader(file + ".json"));
                MsgPack = IO.Read(    ); IO.Close(); }
            else
            {   MP IO = new   MP(File.OpenReader(file + ".mp"  ));
                MsgPack = IO.Read(true); IO.Close(); }
            return MsgPack;
        }
        
        public static void Write(this MsgPack mp, bool Temp, string file, bool JSON = false)
        { if (Temp) MsgPack.New.Add(mp).Write(file, JSON).Dispose();
          else                      mp .Write(file, JSON); }

        public static MsgPack Write(this MsgPack mp, string file, bool JSON = false)
        {
            if (JSON)
            { JSON IO = new JSON(File.OpenWriter(file + ".json", true));
                IO.Write(mp, "\n", "  ").Close(); }
            else
            {   MP IO = new   MP(File.OpenWriter(file + ".mp"  , true));
                IO.Write(mp            ).Close(); }
            return mp;
        }

        public static void WriteAfterAll(this MsgPack mp, bool Temp, string file, bool JSON = false)
        { if (Temp) MsgPack.New.Add(mp).WriteAfterAll(file, JSON).Dispose();
          else                      mp .WriteAfterAll(file, JSON); }

        public static MsgPack WriteAfterAll(this MsgPack mp, string file, bool JSON = false)
        {
            byte[] data = null;
            if (JSON)
            { JSON IO = new JSON(File.OpenWriter());
                IO.Write(mp, true); data = IO.ToArray(true); }
            else
            {   MP IO = new   MP(File.OpenWriter());
                IO.Write(mp      ); data = IO.ToArray(true); }
            File.WriteAllBytes(file + (JSON ? ".json" : ".mp"), data);
            return mp;
        }

        public static void ToJSON   (this string file) =>
            file.ReadMP(    ).Write(file, true).Dispose();

        public static void ToMsgPack(this string file) =>
            file.ReadMP(true).Write(file      ).Dispose();
    }
}
