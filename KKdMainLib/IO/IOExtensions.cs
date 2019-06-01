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
            while (true && stream.LongPosition >= 0 && stream.LongPosition < stream.LongLength)
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
            long LongPosition = stream.LongPosition;
            while (true)
                if (char.IsWhiteSpace(stream.ReadCharUTF8())) LongPosition = stream.LongPosition;
                else                { stream.LongPosition   = LongPosition; break; }
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
    }
}
