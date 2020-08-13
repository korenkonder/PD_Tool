using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public static class HeaderExtensions
    {
        public static Header ReadHeader(this Stream stream, bool seek, bool readSectionSignature = true)
        {
            if (seek)
                if (stream.PI64 > 4) stream.PI64 -= 4;
                else                 stream.PI64  = 0;
            return stream.ReadHeader(readSectionSignature);
        }

        public static Header ReadHeader(this Stream stream, bool readSectionSignature = true)
        {
            Header header = new Header { Format = Format.F2 };
            header.Signature   = stream.RU32();
            header.DataSize    = stream.RU32();
            header.Length      = stream.RU32();
            header.Flags       = stream.RU32();
            header.Depth       = stream.RU32();
            header.SectionSize = stream.RU32();
            header.Version     = stream.RU32();
            stream.RI32();
            header.UseBigEndian   = (header.Flags & 0x08000000) != 0;
            header.UseSectionSize = (header.Flags & 0x10000000) != 0;
            if (header.Length == 0x40)
            {
                stream.RI64();
                stream.RI64();
                header.InnerSignature = stream.RU32();
                stream.RI32();
                stream.RI64();
            }
            stream.Format = header.Format;
            if (readSectionSignature) header.SectionSignature = stream.RU32E();
            return header;
        }

        public static void W(this Stream stream, Header header, bool extended = false)
        {
            header.Length = (header.Format < Format.X && extended) ? 0x40u : 0x20u;
            header.Flags = (header.UseSectionSize ? 0x10000000 : 0u) |
                           (header.UseBigEndian   ? 0x08000000 : 0u);

            stream.W(header.Signature);
            stream.W(header.DataSize);
            stream.W(header.Length);
            stream.W(header.Flags);
            stream.W(header.Depth);
            stream.W(header.SectionSize);
            stream.W(header.Version);
            stream.W(0x00);
            if (header.Length == 0x40)
            {
                stream.W(0x00L);
                stream.W(0x00L);
                stream.W(header.InnerSignature);
                stream.W(0x00);
                stream.W(0x00L);
            }
        }

        public static void WEOFC(this Stream stream, uint depth = 0) =>
            stream.W(new Header { Depth = depth, Length = 0x20, Signature = 0x43464F45, UseSectionSize = true });
    }

    public static class POFExtensions
    {
        public static void W(this Stream stream, POF pof, bool shiftX = false, uint depth = 0)
        {
            byte[] data = pof.Write(shiftX);
            Header header = new Header { Depth = depth, Format = Format.F2, Length = 0x20,
                Signature = shiftX ? 0x31464F50u : 0x30464F50u, UseSectionSize = true };
            header.DataSize = header.SectionSize = (uint)data.Length.A(0x10);
            stream.W(header);
            stream.W(data);
            for (int c = data.Length.A(0x10) - data.Length; c > 0; c--)
                stream.W((byte)0);
        }
    }

    public static class ENRSExtensions
    {
        public static void W(this Stream stream, ENRS enrs, uint depth = 0)
        {
            byte[] data = enrs.Write();
            Header header = new Header { Depth = depth, Format = Format.F2,
                Length = 0x20, Signature = 0x53524E45, UseSectionSize = true };
            header.DataSize = header.SectionSize = (uint)data.Length;
            stream.W(header);
            stream.W(data);
        }
    }

    public static class StructExtensions
    {
        public static Struct RSt(this byte[] data)
        {
            if (data == null || data.Length < 1) return default;
            Struct @struct;
            using (Stream stream = File.OpenReader(data))
                @struct = stream.RSt(stream.ReadHeader(false));
            return @struct;
        }

        public static Struct RSt(this Stream stream, Header header)
        {
            uint l = header.UseSectionSize ? header.SectionSize : header.DataSize;
            Struct @struct = new Struct { Header = header, DataOffset =
                stream.P, Data = stream.RBy(l) };
            uint depth = header.Depth;

            uint lastSig = 0, sig;
            long length = stream.L - stream.P;
            long position = 0;
            KKdList<Struct> subStructs = KKdList<Struct>.New;
            while (length > position)
            {
                header = stream.ReadHeader(false);
                sig = header.Signature;
                l = header.UseSectionSize ? header.SectionSize : header.DataSize;
                position += header.Length + l;
                if (sig == 0x43464F45 && header.Depth == depth + 1) break;
                else if (sig == 0x53524E45 || (sig & 0xF0FFFFFF) == 0x30464F50)
                {
                    byte[] Data = stream.RBy(l);
                    if (sig == 0x53524E45) @struct.ENRS.Read(Data);
                    else                   @struct.POF .Read(Data, sig == 0x31464F50);
                }
                else if (header.Depth == 0 && sig == 0x43505854)
                    subStructs.Add(new Struct { Header = header, DataOffset = stream.P, Data = stream.RBy(l) });
                else if (header.Depth > depth) subStructs.Add(stream.RSt(header));
                else { stream.PI64 -= header.Length; break; }
                lastSig = sig;
            }
            subStructs.Capacity = subStructs.Count;

            if (subStructs.Capacity > 0) @struct.SubStructs = subStructs.ToArray();
            return @struct;
        }

        public static byte[] W(this Struct @struct, bool shiftX = false, bool useDepth = true)
        {
            byte[] Data;
            using (Stream stream = File.OpenWriter())
            { stream.W(@struct, shiftX, useDepth); stream.WEOFC(); Data = stream.ToArray(); }
            return Data;
        }

        public static void W(this Stream stream, Struct @struct, bool shiftX = false, bool useDepth = true)
        {
            @struct.Update(shiftX);

            stream.W(@struct.Header);
            if (@struct.Data != null) stream.W(@struct.Data);
            if (@struct.HasPOF ) stream.W(@struct.POF , shiftX, useDepth ? @struct.Depth + 1 : 0u);
            if (@struct.HasENRS) stream.W(@struct.ENRS,         useDepth ? @struct.Depth + 1 : 0u);
            if (@struct.HasSubStructs)
            {
                for (int i = 0; i < @struct.SubStructs.Length; i++)
                    stream.W(@struct.SubStructs[i], shiftX);
                stream.WEOFC(@struct.Depth + 1);
            }
        }
    }

    public static class MPExt
    {
        public static MsgPack ReadMP(this byte[] array, bool json = false)
        {
            MsgPack MsgPack;
            if (json) using (JSON _IO = new JSON(File.OpenReader(array))) MsgPack = _IO.Read(    );
            else      using (  MP _IO = new   MP(File.OpenReader(array))) MsgPack = _IO.Read(true);
            return MsgPack;
        }

        public static MsgPack ReadMPAllAtOnce(this string file, bool json = false)
        {
            MsgPack MsgPack;
            if (json) using (JSON _IO = new JSON(File.OpenReader(file + ".json", true))) MsgPack = _IO.Read(    );
            else      using (  MP _IO = new   MP(File.OpenReader(file + ".mp"  , true))) MsgPack = _IO.Read(true);
            return MsgPack;
        }

        public static MsgPack ReadMP(this string file, bool json = false)
        {
            MsgPack MsgPack;
            if (json) using (JSON _IO = new JSON(File.OpenReader(file + ".json"))) MsgPack = _IO.Read(    );
            else      using (  MP _IO = new   MP(File.OpenReader(file + ".mp"  ))) MsgPack = _IO.Read(true);
            return MsgPack;
        }

        public static void Write(this MsgPack mp, bool ignoreNull, bool temp, string file, bool json = false)
        { if (temp) MsgPack.New.Add(mp).Write(file, ignoreNull, json).Dispose();
          else                      mp .Write(file, ignoreNull, json); }

        public static MsgPack Write(this MsgPack mp, string file, bool ignoreNull, bool json = false)
        {
            if (json) using (JSON _IO = new JSON(File.OpenWriter(file + ".json", true))) _IO.W(mp, ignoreNull, "\n", "  ");
            else      using (  MP _IO = new   MP(File.OpenWriter(file + ".mp"  , true))) _IO.W(mp, ignoreNull);
            return mp;
        }

        public static void WriteAfterAll(this MsgPack mp, bool ignoreNull, bool temp, string file, bool json = false)
        { if (temp) MsgPack.New.Add(mp).WriteAfterAll(file, ignoreNull, json).Dispose();
          else                      mp .WriteAfterAll(file, ignoreNull, json); }

        public static MsgPack WriteAfterAll(this MsgPack mp, string file, bool ignoreNull, bool json = false)
        {
            byte[] data = null;
            if (json) using (JSON _IO = new JSON(File.OpenWriter())) { _IO.W(mp, ignoreNull, true); data = _IO.ToArray(); }
            else      using (  MP _IO = new   MP(File.OpenWriter())) { _IO.W(mp, ignoreNull      ); data = _IO.ToArray(); }
            File.WriteAllBytes(file + (json ? ".json" : ".mp"), data);
            return mp;
        }

        public static void ToJSON   (this string file) =>
            file.ReadMP(    ).Write(file, false, true).Dispose();

        public static void ToMsgPack(this string file) =>
            file.ReadMP(true).Write(file, false      ).Dispose();
    }

    public static class IKFExt
    {
        public static IKF Round(this IKF kf, int d)
        {
                 if (kf is KFT0 kft0) { kft0.F  = kft0.F .Round(d);                             return kft0; }
            else if (kf is KFT1 kft1) { kft1.F  = kft1.F .Round(d); kft1.V  = kft1.V .Round(d); return kft1; }
            else if (kf is KFT2 kft2) { kft2.F  = kft2.F .Round(d); kft2.V  = kft2.V .Round(d);
                                        kft2.T  = kft2.T .Round(d);                             return kft2; }
            else if (kf is KFT3 kft3) { kft3.F  = kft3.F .Round(d); kft3.V  = kft3.V .Round(d);
                                        kft3.T1 = kft3.T1.Round(d); kft3.T2 = kft3.T2.Round(d); return kft3; }
            return   kf;
        }
    }
}
