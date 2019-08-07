using KKdBaseLib;

namespace KKdMainLib.IO
{
    public static class Extensions
    {
        public static string NullTerminatedASCII(this Stream stream, byte End = 0) =>
            stream.NullTerminated(End).ToASCII();
        public static string NullTerminatedUTF8 (this Stream stream, byte End = 0) =>
            stream.NullTerminated(End).ToUTF8 ();
        public static byte[] NullTerminated     (this Stream stream, byte End = 0)
        {
            KKdList<byte> s = KKdList<byte>.New;
            while (stream.LongPosition < stream.LongLength)
            {
                byte a = stream.ReadByte();
                if (a == End) break;
                else s.Add(a);
            }
            return s.ToArray();
        }
        public static char PeekCharUTF8(this Stream stream)
        {   long LongPosition = stream.LongPosition;   char val = stream.ReadCharUTF8();
          stream.LongPosition =        LongPosition; return val; }
        public static Stream SkipWhitespace(this Stream stream)
        {
            while (true)
                if (char.IsWhiteSpace(stream.PeekCharUTF8())) stream.ReadCharUTF8();
                else break;
            return stream;
        }
		public static bool Assert(this Stream stream, char next)
		{ if (stream.PeekCharUTF8() == next) { stream.ReadCharUTF8(); return true; } else return false; }
		public static bool Assert(this Stream stream, string next)
		{
            for (var i = 0; i < next.Length; i++)
                if (!stream.Assert(next[i])) return false;
            return true;
		}

        public static long ReadIntX(this Stream stream           ) => stream.IsX ?
            stream.ReadInt64() : stream.ReadUInt32Endian(    );
        public static long ReadIntX(this Stream stream, bool IsBE) => stream.IsX ?
            stream.ReadInt64() : stream.ReadUInt32Endian(IsBE);

        public static void WriteX(this Stream stream, long val, ref KKdList<long> POF)
        {   if (stream.IsX) stream.Write      (     val);
            else            stream.WriteEndian((int)val);       POF.Add(stream.Position); }
        public static void WriteX(this Stream stream, long val, ref KKdList<long> POF, bool IsBE)
        {   if (stream.IsX) stream.Write      (     val      );
            else            stream.WriteEndian((int)val, IsBE); POF.Add(stream.Position); }

        public static void WriteX(this Stream stream, long val)
        {   if (stream.IsX) stream.Write      (     val);
            else            stream.WriteEndian((int)val);       }
        public static void WriteX(this Stream stream, long val, bool IsBE)
        {   if (stream.IsX) stream.Write      (     val      );
            else            stream.WriteEndian((int)val, IsBE); }

        public static byte[] ReadAtOffset(this Stream stream, long Offset = -1, long Length = -1)
        {
            byte[] arr = null;
            long Position = stream.LongPosition;
            if (Offset == -1) { Position += stream.IsX ? 8 : 4; Offset = stream.ReadIntX(); }
            stream.LongPosition = Offset;
            if (Length == -1) arr = stream.NullTerminated();
            else              arr = stream.ReadBytes(Length);
            stream.LongPosition = Position;
            return arr;
        }

        public static string ReadStringAtOffset(this Stream stream, long Offset = -1, long Length = -1)
        {
            string s = null;
            long Position = stream.LongPosition;
            if (Offset == -1) { Position += stream.IsX ? 8 : 4; Offset = stream.ReadIntX(); }
            stream.LongPosition = Offset;
            if (Length == -1) s = stream.NullTerminatedUTF8();
            else              s = stream.ReadStringUTF8(Length);
            stream.LongPosition = Position;
            return s;
        }

        public static Pointer<string> ReadPointerStringShiftJIS(this Stream IO)
        { Pointer<string> val = IO.ReadPointer<string>();
            val.Value = IO.ReadStringShiftJISAtOffset(val.Offset); return val; }

        public static string ReadStringShiftJISAtOffset(this Stream IO, long Offset = 0, long Length = 0) =>
            Text.ShiftJIS.GetString(IO.ReadAtOffset(Offset, Length));

        public static void WriteShiftJIS(this Stream IO, string String) =>
            IO.Write(Text.ShiftJIS.GetBytes(String));
        
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
