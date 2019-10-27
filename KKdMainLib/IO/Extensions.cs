using KKdBaseLib;
using KKdBaseLib.F2;

namespace KKdMainLib.IO
{
    public static class Extensions
    {
        public static string NTASCII(this Stream stream, byte End = 0) =>
            stream.NT(End).ToASCII();
        public static string NTUTF8 (this Stream stream, byte End = 0) =>
            stream.NT(End).ToUTF8 ();
        public static byte[] NT     (this Stream stream, byte End = 0)
        {
            KKdList<byte> s = KKdList<byte>.New;
            while (stream.I64P < stream.I64L)
            {
                byte a = stream.RU8();
                if (a == End) break;
                else s.Add(a);
            }
            return s.ToArray();
        }
        
        public static byte PB     (this Stream stream)
        { byte val = stream.RU8(); stream.I64P--; return       val; }
        public static char PCASCII(this Stream stream)
        { byte val = stream.RU8(); stream.I64P--; return (char)val; }
        public static char PCUTF8 (this Stream stream)
        {   long LongPosition = stream.I64P;   char val = stream.RCUTF8();
          stream.I64P =        LongPosition; return val; }

        public static Stream SW(this Stream stream)
        {
            while (true)
                if (char.IsWhiteSpace(stream.PCUTF8())) stream.RCUTF8();
                else break;
            return stream;
        }

		public static bool A(this Stream stream, byte next)
		{ if (stream.PB() == next) { stream.PB(); return true; } else return false; }
		public static bool A(this Stream stream, byte[] next)
		{
            for (var i = 0; i < next.Length; i++)
                if (!A(stream, next[i])) return false;
            return true;
		}
        
		public static bool AASCII(this Stream stream, char next)
		{ if (stream.PCUTF8() == next) { stream.RCUTF8(); return true; } else return false; }
		public static bool AASCII(this Stream stream, string next)
		{
            for (var i = 0; i < next.Length; i++)
                if (!stream.AASCII(next[i])) return false;
            return true;
		}

		public static bool A(this Stream stream, char next)
		{ if (stream.PCUTF8() == next) { stream.RCUTF8(); return true; } else return false; }
		public static bool a(this Stream stream, string next)
		{
            for (var i = 0; i < next.Length; i++)
                if (!A(stream, next[i])) return false;
            return true;
		}

        public static long RIX(this Stream stream           ) => stream.IsX ?
            stream.RI64() : stream.RU32E(    );
        public static long RIX(this Stream stream, bool IsBE) => stream.IsX ?
            stream.RI64() : stream.RU32E(IsBE);

        public static void WX(this Stream stream, long val, ref POF POF)
        {   if (stream.IsX) stream.W (     val);
            else            stream.WE((int)val);       POF.Offsets.Add(stream.P); }
        public static void WX(this Stream stream, long val, ref POF POF, bool IsBE)
        {   if (stream.IsX) stream.W (     val      );
            else            stream.WE((int)val, IsBE); POF.Offsets.Add(stream.P); }

        public static void WX(this Stream stream, long val)
        {   if (stream.IsX) stream.W (     val);
            else            stream.WE((int)val);       }
        public static void WX(this Stream stream, long val, bool IsBE)
        {   if (stream.IsX) stream.W (     val      );
            else            stream.WE((int)val, IsBE); }

        public static byte[] RaO(this Stream stream, long Offset = -1, long Length = -1)
        {
            byte[] arr = null;
            long Position = stream.I64P;
            if (Offset == -1) { Position += stream.IsX ? 8 : 4; Offset = stream.RIX(); }
            stream.I64P = Offset;
            if (Length == -1) arr = stream.NT();
            else              arr = stream.RBy(Length);
            stream.I64P = Position;
            return arr;
        }

        public static string RSaO(this Stream stream, long Offset = -1, long Length = -1)
        {
            string s = null;
            long Position = stream.I64P;
            if (Offset == -1) { Position += stream.IsX ? 8 : 4; Offset = stream.RIX(); }
            stream.I64P = Offset;
            if (Length == -1) s = stream.NTUTF8();
            else              s = stream.RSUTF8(Length);
            stream.I64P = Position;
            return s;
        }

        public static Pointer<string> RPSSJIS(this Stream stream)
        { Pointer<string> val = stream.RP<string>();
            val.V = stream.RPSSJIS(val.O); return val; }

        public static void WPSSJIS(this Stream stream, ref Pointer<string> val)
        { val.O = stream.P; stream.WPSSJIS(val.V); }

        public static string RPSSJIS(this Stream stream, long Offset = 0, long Length = 0) =>
            Text.ShiftJIS.GetString(stream.RaO(Offset, Length));

        public static void WPSSJIS(this Stream stream, string String) =>
            stream.W(Text.ShiftJIS.GetBytes(String));

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
