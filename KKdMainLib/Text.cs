using System;
using System.Text;

namespace KKdMainLib
{
    public static class Text
    {
        public static string    ToASCII(this byte[] Array) => Encoding.ASCII.GetString(Array);
        public static string    ToUTF8 (this byte[] Array) => Encoding.UTF8 .GetString(Array);
        public static byte[]    ToASCII(this string Data ) => Encoding.ASCII.GetBytes (Data );
        public static byte[]    ToUTF8 (this string Data ) => Encoding.UTF8 .GetBytes (Data );
        public static byte[]    ToASCII(this char[] Data ) => Encoding.ASCII.GetBytes (Data );
        public static byte[]    ToUTF8 (this char[] Data ) => Encoding.UTF8 .GetBytes (Data );
        public static string   ToBase64(this byte[] Array) => Convert.  ToBase64String(Array);
        public static byte[] FromBase64(this string Data ) => Convert.FromBase64String(Data );
    }
}
