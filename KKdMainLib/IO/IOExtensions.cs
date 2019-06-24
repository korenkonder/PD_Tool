using System.Collections.Generic;

namespace KKdMainLib.IO
{
    public static class IOExtensions
    {
        public static string NullTerminatedASCII(this Stream stream, byte End = 0) =>
            stream.NullTerminated(End).ToASCII();
        public static string NullTerminatedUTF8 (this Stream stream, byte End = 0) =>
            stream.NullTerminated(End).ToUTF8 ();
        public static byte[] NullTerminated     (this Stream stream, byte End = 0)
        {
            List<byte> s = new List<byte>();
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

        public static byte[] ReadAtOffset(this Stream stream, long Offset = 0, long Length = 0)
        {
            byte[] arr = null;
            long Position = stream.LongPosition;
            if (Offset == 0) { Position += stream.IsX ? 8 : 4; Offset = stream.ReadIntX(); }
            stream.LongPosition = Offset;
            if (Length == 0) arr = stream.NullTerminated();
            else             arr = stream.ReadBytes(Length);
            stream.LongPosition = Position;
            return arr;
        }

        public static string ReadStringAtOffset(this Stream stream, long Offset = 0, long Length = 0)
        {
            string s = null;
            long Position = stream.LongPosition;
            if (Offset == 0) { Position += stream.IsX ? 8 : 4; Offset = stream.ReadIntX(); }
            stream.LongPosition = Offset;
            if (Length == 0) s = stream.NullTerminatedUTF8();
            else             s = stream.ReadStringUTF8(Length);
            stream.LongPosition = Position;
            return s;
        }
    }
}
