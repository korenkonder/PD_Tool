using System;
using System.Collections.Generic;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public class POF
    {
        public byte Type;
        public int Lenght;
        public int Offset;
        public int LastOffset;
        public List<long> Offsets;
        public List<long> POFOffsets;
        public PDHead Header;

        public POF()
        { Type = 0; Lenght = 0; Offset = 0; LastOffset = 0; Offsets = new List<long>();
            POFOffsets = new List<long>(); Header = new PDHead(); }
    }

    public static class POFExtensions
    {
        public static POF AddPOF(this PDHead Header)
        {
            POF POF = new POF { Offsets = new List<long>(), POFOffsets =
                new List<long>(), Offset = Header.DataSize + Header.Lenght };
            return POF;
        }
        public static Stream GetOffset(this Stream stream, ref POF POF)
        {
            if (POF != null) if (stream.Format > Main.Format.F)
                    POF.POFOffsets.Add(stream.Position - (stream.IsX ? stream.Offset : 0x00));
            return stream;
        }

        public static void ReadPOF(this Stream stream, ref POF POF)
        {
            if (stream.ReadString(3) == "POF")
            {
                POF.POFOffsets.Sort();
                POF.Type = byte.Parse(stream.ReadString(1));
                int IsX = POF.Type + 2;
                stream.Seek(-4, SeekOrigin.Current);
                POF.Header = stream.ReadHeader();
                stream.Seek(POF.Offset + POF.Header.Lenght, 0);
                POF.Lenght = stream.ReadInt32();
                while (POF.Lenght + POF.Offset + POF.Header.Lenght > stream.Position)
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
                    POF.Offsets.Add(POF.LastOffset);
                }

                for (int i = 0; i < POF.Offsets.Count && i < POF.POFOffsets.Count; i++)
                    if (POF.Offsets[i] != POF.POFOffsets[i])
                        Console.WriteLine("Not right POF{0} offset table.\n" +
                            "  Expected: {1}\n  Got: {2}", POF.Type,
                            POF.Offsets[i].ToString("X8"), POF.POFOffsets[i].ToString("X8"));
            }
        }
        
        public static void Write(this Stream stream, ref POF POF, int ID)
        {
            POF.POFOffsets.Sort();
            long CurrentPOFOffset = 0;
            long POFOffset = 0;
            byte BitShift = (byte)(2 + POF.Type);
            int Max1 = (0x00FF >> BitShift) << BitShift;
            int Max2 = (0xFFFF >> BitShift) << BitShift;
            POF.Lenght = 5 + ID;
            for (int i = 0; i < POF.POFOffsets.Count; i++)
            {
                POFOffset = POF.POFOffsets[i] - CurrentPOFOffset;
                CurrentPOFOffset = POF.POFOffsets[i];
                     if (POFOffset <= Max1) POF.Lenght += 1;
                else if (POFOffset <= Max2) POF.Lenght += 2;
                else                        POF.Lenght += 4;
                POF.POFOffsets[i] = POFOffset;
            }

            long POFLenghtAling = POF.Lenght.Align(16);
            POF.Header = new PDHead { DataSize = (int)POFLenghtAling, ID = ID, Format = Main.Format.F2LE,
                Lenght = 0x20, SectionSize = (int)POFLenghtAling, Signature = 0x30464F50 };
            POF.Header.Signature += POF.Type << 24;
            stream.Write(POF.Header);

            stream.Write(POF.Lenght);
            for (int i = 0; i < POF.POFOffsets.Count; i++)
            {
                POFOffset = POF.POFOffsets[i];
                     if (POFOffset <= Max1) stream.Write      ((  byte)((1 <<  6) | (POFOffset >> BitShift)));
                else if (POFOffset <= Max2) stream.WriteEndian((ushort)((2 << 14) | (POFOffset >> BitShift)), true);
                else                        stream.WriteEndian((  uint)((3 << 30) | (POFOffset >> BitShift)), true);
            }
            stream.Write(0x00);
            stream.Align(16, true);
            stream.WriteEOFC(ID);
        }

        public static long ReadUInt32Endian(this Stream IO, ref POF POF) =>
            IO.GetOffset(ref POF).ReadUInt32Endian();
        public static long ReadUInt32Endian(this Stream IO, ref POF POF, bool IsBE) =>
            IO.GetOffset(ref POF).ReadUInt32Endian(IsBE);
        public static long ReadInt64(this Stream IO, ref POF POF) =>
            IO.GetOffset(ref POF).ReadInt64();

        public static long ReadIntX(this Stream IO, ref POF POF           ) =>
            IO.IsX ? IO.ReadInt64() : IO.ReadUInt32Endian(    );
        public static long ReadIntX(this Stream IO, ref POF POF, bool IsBE) =>
            IO.IsX ? IO.ReadInt64() : IO.ReadUInt32Endian(IsBE);

        public static string ReadStringAtOffset(this Stream IO, ref POF POF, long Offset = 0, long Length = 0) =>
            IO.GetOffset(ref POF).ReadStringAtOffset(Offset, Length);
    }
}
