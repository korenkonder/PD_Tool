using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public static class HeaderExtensions
    {
        public static Header ReadHeader(this Stream stream, bool seek)
        {
            if (seek)
                if (stream.PI64 > 4) stream.PI64 -= 4;
                else                 stream.PI64  = 0;
            return stream.ReadHeader();
        }

        public static Header ReadHeader(this Stream stream)
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
                    subStructs.Add(stream.RSt(header));
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
            { stream.W(@struct, 0, shiftX, useDepth); stream.WEOFC(); Data = stream.ToArray(); }
            return Data;
        }

        public static void W(this Stream stream, Struct @struct, uint depth = 0,
            bool shiftX = false, bool useDepth = true)
        {
            @struct.Update(shiftX);

            @struct.Header.Depth = depth;
            stream.W(@struct.Header, @struct.Header.Length == 0x40);
            if (@struct.Data != null) stream.W(@struct.Data);
            if (@struct.HasPOF ) stream.W(@struct.POF , shiftX, useDepth ? depth + 1 : 0u);
            if (@struct.HasENRS) stream.W(@struct.ENRS,         useDepth ? depth + 1 : 0u);
            if (@struct.HasSubStructs)
                for (int i = 0; i < @struct.SubStructs.Length; i++)
                    stream.W(@struct.SubStructs[i], depth + 1, shiftX, useDepth);
            if (@struct.HasPOF || @struct.HasENRS || @struct.HasSubStructs) 
                stream.WEOFC(depth + 1);
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
            if (json)
                using (JSON _IO = new JSON(File.OpenWriter(file + ".json", true)))
                    _IO.W(mp, ignoreNull, "\n", "  ");
            else
                using (  MP _IO = new   MP(File.OpenWriter(file + ".mp"  , true)))
                    _IO.W(mp, ignoreNull);
            return mp;
        }

        public static void WriteAfterAll(this MsgPack mp, bool ignoreNull, bool temp, string file, bool json = false)
        { if (temp) MsgPack.New.Add(mp).WriteAfterAll(file, ignoreNull, json).Dispose();
          else                      mp .WriteAfterAll(file, ignoreNull, json); }

        public static MsgPack WriteAfterAll(this MsgPack mp, string file, bool ignoreNull, bool json = false)
        {
            byte[] data = null;
            if (json)
                using (JSON _IO = new JSON(File.OpenWriter()))
                { _IO.W(mp, ignoreNull, true); data = _IO.ToArray(); }
            else
                using (  MP _IO = new   MP(File.OpenWriter()))
                { _IO.W(mp, ignoreNull      ); data = _IO.ToArray(); }
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

    public static class HashExt
    {
        public static ulong HashFNV1a64(this byte[] array, int length) // 0x1403B04D0 in SBZV_7.10; FNV 1a 64-bit
        {
            int i = 0;
            ulong hash = 0xCBF29CE484222325;
            while (i < length)
            { hash = 0x100000001B3 * (hash ^ (ulong)array[i]); i++; }
            return (hash >> 32) ^ hash;
        }

        public static uint HashMurmurHash(this byte[] array, int length, uint seed = 0,
            bool alreadyUpper = false, bool bigEndian = false) // 0x814D7A9C in PCSB00554; MurmurHash
        {
            uint a, b, hash;
            int i;
            
            const uint m = 0x7FD652AD;
            const int r = 16;

            hash = seed + 0xDEADBEEF;
            if (alreadyUpper)
            {
                if (bigEndian)
                    for (i = 0; length > 3; length -= 4, i += 4)
                    {
                        b = (uint)array[i + 3] | ((uint)array[i + 2] << 8)
                            | ((uint)array[i + 1] << 16) | ((uint)array[i + 0] << 24);
                        hash += b;
                        hash *= m;
                        hash ^= hash >> r;
                    }
                else
                    for (i = 0; length > 3; length -= 4, i += 4)
                    {
                        b = (uint)array[i + 0] | ((uint)array[i + 1] << 8)
                            | ((uint)array[i + 2] << 16) | ((uint)array[i + 3] << 24);
                        hash += b;
                        hash *= m;
                        hash ^= hash >> r;
                    }

                if (length > 0)
                {
                    if (length > 1)
                    {
                        if (length > 2)
                            hash += (uint)array[i + 2] << 16;
                        hash += (uint)array[i + 1] << 8;
                    }
                    hash += array[i];
                    hash *= m;
                    hash ^= hash >> r;
                }
            }
            else
            {
                if (bigEndian)
                    for (i = 0; length > 3; length -= 4, i += 4)
                    {
                        b = (uint)array[i + 3] | ((uint)array[i + 2] << 8)
                            | ((uint)array[i + 1] << 16) | ((uint)array[i + 0] << 24);
                        a =  b        & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFFFFFF00) | a        ;
                        a = (b >>  8) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFFFF00FF) | (a <<  8);
                        a = (b >> 16) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFF00FFFF) | (a << 16);
                        a = (b >> 24) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0x00FFFFFF) | (a << 24);
                        hash += b;
                        hash *= m;
                        hash ^= hash >> r;
                    }
                else
                    for (i = 0; length > 3; length -= 4, i += 4)
                    {
                        b = (uint)array[i + 0] | ((uint)array[i + 1] << 8)
                            | ((uint)array[i + 2] << 16) | ((uint)array[i + 3] << 24);
                        a =  b        & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFFFFFF00) | a        ;
                        a = (b >>  8) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFFFF00FF) | (a <<  8);
                        a = (b >> 16) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFF00FFFF) | (a << 16);
                        a = (b >> 24) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0x00FFFFFF) | (a << 24);
                        hash += b;
                        hash *= m;
                        hash ^= hash >> r;
                    }

                if (length > 0)
                {
                    if (length > 1)
                    {
                        if (length > 2)
                        {
                            b = array[i + 2]; if (b > 0x60 && b < 0x7B) b -= 0x20; hash += b << 16;
                        }
                        b = array[i + 1]; if (b > 0x60 && b < 0x7B) b -= 0x20; hash += b << 8;
                    }
                    b = array[i]; if (b > 0x60 && b < 0x7B) b -= 0x20; hash += b;
                    hash *= m;
                    hash ^= hash >> r;
                }
            }

            hash *= m;
            hash ^= hash >> 10;
            hash *= m;
            hash ^= hash >> 17;
            return hash;
        }
    }
}
