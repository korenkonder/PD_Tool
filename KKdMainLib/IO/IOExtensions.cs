using System.Collections.Generic;

namespace KKdMainLib.IO
{
    public static class IOExtensions
    {
        public static string NullTerminatedASCII(this Stream stream, byte End = 0) => stream.NullTerminated(End).ToASCII();
        public static string NullTerminatedUTF8 (this Stream stream, byte End = 0) => stream.NullTerminated(End).ToUTF8 ();
        public static byte[] NullTerminated     (this Stream stream, byte End = 0)
        {
            List<byte> s = new List<byte>();
            while (true && stream.LongPosition > 0 && stream.LongPosition < stream.LongLength)
            {
                byte a = stream.ReadByte();
                if (a == End) break;
                else s.Add(a);
            }
            return s.ToArray();
        }
    }
}
