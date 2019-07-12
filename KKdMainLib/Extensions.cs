using System.Collections.Generic;
using KKdBaseLib;
using KKdMainLib.IO;
using KKdMainLib.F2nd;

namespace KKdMainLib
{
    public static class HeaderExtensions
    {
        public static Header ReadHeader(this Stream stream, bool Seek, bool ReadSectionSignature = true)
        {
            if (Seek)
                if (stream.Position > 4) stream.LongPosition -= 4;
                else                     stream.LongPosition  = 0;
            return stream.ReadHeader(ReadSectionSignature);
        }

        public static Header ReadHeader(this Stream stream, bool ReadSectionSignature = true)
        {
            Header Header = new Header
            { Format = Main.Format.F2LE, Signature = stream.ReadInt32(),
                DataSize = stream.ReadInt32(),
                Length = stream.ReadInt32() };
            if (stream.ReadUInt32() == 0x18000000)
            { Header.Format = Main.Format.F2BE; }
            Header.ID = stream.ReadInt32();
            Header.SectionSize = stream.ReadInt32();
            Header.Count = stream.ReadInt32();
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
            stream.Write((Header.Format < Main.Format.X && Extended) ? 0x40 : 0x20);
            stream.Write(Header.Format == Main.Format.F2BE ? 0x18000000 : 0x10000000);
            stream.Write(Header.ID);
            stream.Write(Header.SectionSize);
            stream.Write(0x00);
            stream.Write(0x00);
            if (Header.Format < Main.Format.X && Extended)
            {
                stream.Write(Header.Format < Main.Format.MGF ? (int)((Header.SectionSignature ^
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
        public static POF AddPOF(this Header Header) =>
            new POF { Offsets = new List<long>(), Offset = Header.DataSize + Header.Length };

        public static void GetOffset(this Stream stream, ref POF POF)
        { if (stream.Format > Main.Format.F) POF.Offsets.Add(stream.Position + (stream.IsX ? stream.Offset : 0x00)); }

        public static void ReadPOF(this Stream stream, ref POF POF)
        {
            if (stream.ReadString(3) == "POF")
            {
                POF.Type = byte.Parse(stream.ReadString(1));
                int IsX = POF.Type + 2;
                stream.Seek(-4, SeekOrigin.Current);
                POF.Header = stream.ReadHeader();
                stream.Seek(POF.Offset + POF.Header.Length, 0);
                POF.Length = stream.ReadInt32();
                while (POF.Length + POF.Offset + POF.Header.Length > stream.Position)
                {
                    int a = stream.ReadByte();
                         if (a >> 6 == 0) break;
                    else if (a >> 6 == 1) a = a & 0x3F;
                    else if (a >> 6 == 2)
                    {
                        a = a & 0x3F;
                        a = (a << 8) | stream.ReadByte();
                    }
                    else if (a >> 6 == 3)
                    {
                        a = a & 0x3F;
                        a = (a << 8) | stream.ReadByte();
                        a = (a << 8) | stream.ReadByte();
                        a = (a << 8) | stream.ReadByte();
                    }
                    a <<= IsX;
                    POF.LastOffset += a;
                }
            }
        }
        
        public static void Write(this Stream stream, ref POF POF, int ID)
        {
            POF.Offsets.Sort();
            long CurrentPOFOffset = 0;
            long POFOffset = 0;
            byte BitShift = (byte)(2 + POF.Type);
            int Max1 = (0x00FF >> BitShift) << BitShift;
            int Max2 = (0xFFFF >> BitShift) << BitShift;
            POF.Length = 5 + ID;
            for (int i = 0; i < POF.Offsets.Count; i++)
            {
                POFOffset = POF.Offsets[i] - CurrentPOFOffset;
                CurrentPOFOffset = POF.Offsets[i];
                     if (POFOffset <= Max1) POF.Length += 1;
                else if (POFOffset <= Max2) POF.Length += 2;
                else                        POF.Length += 4;
                POF.Offsets[i] = POFOffset;
            }

            long POFLengthAling = POF.Length.Align(16);
            POF.Header = new Header { DataSize = (int)POFLengthAling, ID = ID, Format = Main.Format.F2LE,
                Length = 0x20, SectionSize = (int)POFLengthAling, Signature = 0x30464F50 };
            POF.Header.Signature += POF.Type << 24;
            stream.Write(POF.Header);

            stream.Write(POF.Length);
            for (int i = 0; i < POF.Offsets.Count; i++)
            {
                POFOffset = POF.Offsets[i];
                     if (POFOffset <= Max1) stream.Write      ((  byte)((1 <<  6) | (POFOffset >> BitShift)));
                else if (POFOffset <= Max2) stream.WriteEndian((ushort)((2 << 14) | (POFOffset >> BitShift)), true);
                else                        stream.WriteEndian((  uint)((3 << 30) | (POFOffset >> BitShift)), true);
            }
            stream.Write(0x00);
            stream.Align(16, true);
            stream.WriteEOFC(ID);
        }
    }

    public static class PointerExt
    {
        public static Pointer<T> ReadPointer<T>(this Stream IO) =>
            new Pointer<T> { Offset = IO.ReadInt32() };

        public static Pointer<string> ReadPointerString(this Stream IO)
        { Pointer<string> val = IO.ReadPointer<string>();
            val.Value = IO.ReadStringAtOffset(val.Offset); return val; }

        public static CountPointer<T> ReadCountPointer<T>(this Stream IO) =>
            new CountPointer<T> { Count = IO.ReadInt32(), Offset = IO.ReadInt32() };

        public static Pointer<T> ReadPointerEndian<T>(this Stream IO) =>
            new Pointer<T> { Offset = IO.ReadInt32Endian() };

        public static Pointer<string> ReadPointerStringEndian(this Stream IO)
        { Pointer<string> val = IO.ReadPointerEndian<string>();
            val.Value = IO.ReadStringAtOffset(val.Offset); return val; }

        public static CountPointer<T> ReadCountPointerEndian<T>(this Stream IO) =>
            new CountPointer<T> { Count = IO.ReadInt32Endian(), Offset = IO.ReadInt32Endian() };

        public static Pointer<T> ReadPointerX<T>(this Stream IO) =>
            new Pointer<T> { Offset = (int)IO.ReadIntX() };

        public static Pointer<string> ReadPointerStringX(this Stream IO)
        { Pointer<string> val = IO.ReadPointerX<string>();
            val.Value = IO.ReadStringAtOffset(val.Offset); return val; }

        public static CountPointer<T> ReadCountPointerX<T>(this Stream IO) =>
            new CountPointer<T> { Count = (int)IO.ReadIntX(), Offset = (int)IO.ReadIntX() };
    }
}
