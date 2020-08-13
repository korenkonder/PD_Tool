using KKdBaseLib;
using KKdBaseLib.F2;

namespace KKdMainLib.IO
{
    public static class Extensions
    {
        public static string NTASCII(this Stream stream, byte end = 0) =>
            stream.NT(end).ToASCII();
        public static string NTUTF8 (this Stream stream, byte end = 0) =>
            stream.NT(end).ToUTF8 ();
        public static byte[] NT     (this Stream stream, byte end = 0)
        {
            KKdList<byte> s = KKdList<byte>.New;
            while (stream.PI64 < stream.LI64)
            {
                byte a = stream.RU8();
                if (a == end) break;
                else s.Add(a);
            }
            s.Capacity = s.Count;
            return s.ToArray();
        }

        public static byte PB     (this Stream stream)
        { byte val = stream.RU8(); stream.PI64--; return       val; }
        public static char PCASCII(this Stream stream)
        { byte val = stream.RU8(); stream.PI64--; return (char)val; }
        public static char PCUTF8 (this Stream stream)
        {   long LongPosition = stream.PI64;   char val = stream.RCUTF8();
          stream.PI64 =        LongPosition; return val; }

        public static Stream SW(this Stream stream)
        {
            while (true)
                if (char.IsWhiteSpace(stream.PCUTF8())) stream.RCUTF8();
                else break;
            return stream;
        }

        public static bool As(this Stream stream, byte next)
        { if (stream.PB() == next) { stream.PB(); return true; } else return false; }
        public static bool As(this Stream stream, byte[] next)
        {
            for (var i = 0; i < next.Length; i++)
                if (!As(stream, next[i])) return false;
            return true;
        }

        public static bool AsASCII(this Stream stream, char next)
        { if (stream.PCUTF8() == next) { stream.RCUTF8(); return true; } else return false; }
        public static bool AsASCII(this Stream stream, string next)
        {
            for (var i = 0; i < next.Length; i++)
                if (!stream.AsASCII(next[i])) return false;
            return true;
        }

        public static bool As(this Stream stream, char next)
        { if (stream.PCUTF8() == next) { stream.RCUTF8(); return true; } else return false; }
        public static bool As(this Stream stream, string next)
        {
            for (var i = 0; i < next.Length; i++)
                if (!As(stream, next[i])) return false;
            return true;
        }

        public static long RIX(this Stream stream           ) =>
            stream.IsX ? stream.RI64() : stream.RI32E(  );
        public static long RIX(this Stream stream, bool isBE) =>
            stream.IsX ? stream.RI64() : stream.RI32E(isBE);

        public static void WX(this Stream stream, long val, ref POF pof)
        {   if (stream.IsX) stream.W (     val);
            else            stream.WE((int)val);       pof.Offsets.Add(stream.P); }
        public static void WX(this Stream stream, long val, ref POF pof, bool isBE)
        {   if (stream.IsX) stream.W (     val      );
            else            stream.WE((int)val, isBE); pof.Offsets.Add(stream.P); }

        public static void WX(this Stream stream, long val)
        {   if (stream.IsX) stream.W (     val);
            else            stream.WE((int)val);       }
        public static void WX(this Stream stream, long val, bool isBE)
        {   if (stream.IsX) stream.W (     val      );
            else            stream.WE((int)val, isBE); }

        public static byte[] RaO(this Stream stream, long offset = -1, long length = -1)
        {
            byte[] arr = null;
            long Position = stream.PI64;
            if (offset == -1) { Position += stream.IsX ? 8 : 4; offset = stream.RIX(); }
            stream.PI64 = offset;
            if (length == -1) arr = stream.NT();
            else              arr = stream.RBy(length);
            stream.PI64 = Position;
            return arr;
        }

        public static string RSaO(this Stream stream)
        {
            long Position = stream.PI64 + (stream.IsX ? 8 : 4);
            stream.PI64 = stream.RIX();
            string s = stream.NTUTF8();
            stream.PI64 = Position;
            return s;
        }

        public static string RSaO(this Stream stream, long offset)
        {
            long Position = stream.PI64;
            stream.PI64 = offset;
            string s = stream.NTUTF8();
            stream.PI64 = Position;
            return s;
        }

        public static string RSaO(this Stream stream, long offset, long length)
        {
            long Position = stream.PI64;
            stream.PI64 = offset;
            string s = stream.RSUTF8(length);
            stream.PI64 = Position;
            return s;
        }

        public static Pointer<string> RPSSJIS(this Stream stream)
        { Pointer<string> val = stream.RP<string>();
            val.V = stream.RPSSJIS(val.O); return val; }

        public static void WPSSJIS(this Stream stream, ref Pointer<string> val)
        { val.O = stream.P; stream.WPSSJIS(val.V); }

        public static string RPSSJIS(this Stream stream, long offset = -1, long length = -1) =>
            Text.ShiftJIS.GetString(stream.RaO(offset, length));

        public static void WPSSJIS(this Stream stream, string val) =>
            stream.W(val.ToSJIS());

        public static Pointer<string> RPS(this Stream stream)
        { Pointer<string> val = stream.RP<string>();
            val.V = stream.RSaO(val.O); return val; }

        public static void W(this Stream stream, ref Pointer<string> val)
        { val.O = stream.P; stream.W(val.V); }

        public static Pointer<T> RP<T>(this Stream stream) =>
            new Pointer<T> { O = stream.RI32() };

        public static void W<T>(this Stream stream, Pointer<T> val) =>
            stream.W(val.O);

        public static CountPointer<T> ReadCountPointer<T>(this Stream stream) =>
            new CountPointer<T> { C = stream.RI32(), O = stream.RI32() };

        public static void W<T>(this Stream stream, CountPointer<T> val)
        { stream.W(val.C); stream.W(val.O); }

        public static Pointer<T> RPE<T>(this Stream stream) =>
            new Pointer<T> { O = stream.RI32E() };

        public static void WE<T>(this Stream stream, Pointer<T> val) =>
            stream.WE(val.O);

        public static CountPointer<T> RCPE<T>(this Stream stream) =>
            new CountPointer<T> { C = stream.RI32E(), O = stream.RI32E() };

        public static void WE<T>(this Stream stream, CountPointer<T> val)
        { stream.WE(val.C); stream.WE(val.O); }

        public static Pointer<string> RPSE(this Stream stream)
        { Pointer<string> val = stream.RPE<string>();
            val.V = stream.RSaO(val.O); return val; }

        public static void WE(this Stream stream, ref Pointer<string> val)
        { val.O = stream.P; stream.W(val.V); }

        public static Pointer<T> RPX<T>(this Stream stream) =>
            new Pointer<T> { O = (int)stream.RIX() };

        public static void WE<T>(this Stream stream, ref Pointer<T> val) =>
            stream.WX(val.O);

        public static Pointer<string> RPSX(this Stream stream)
        { Pointer<string> val = stream.RPX<string>();
            val.V = stream.RSaO(val.O); return val; }

        public static void WX(this Stream stream, ref Pointer<string> val)
        { val.O = stream.P; stream.W(val.V); }

        public static CountPointer<T> RCPX<T>(this Stream stream) =>
            new CountPointer<T> { C = (int)stream.RIX(), O = (int)stream.RIX() };

        public static void WX<T>(this Stream stream, CountPointer<T> val)
        { stream.WX(val.C); stream.WX(val.O); }
    }
}
