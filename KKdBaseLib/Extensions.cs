using System;

namespace KKdBaseLib
{
    public static unsafe class Extensions
    {
        public static double Acos   (this double d  ) =>     Math.Acos   (d  );
        public static double Asin   (this double d  ) =>     Math.Asin   (d  );
        public static double Atan   (this double d  ) =>     Math.Atan   (d  );
        public static double Actg   (this double d  ) => 1 / Math.Atan   (d  );
        public static double  Cos   (this double d  ) =>     Math.Cos    (d  );
        public static double  Cosh  (this double val) =>     Math.Cosh   (val);
        public static double  Sin   (this double a  ) =>     Math.Sin    (a  );
        public static double  Sinh  (this double val) =>     Math.Sinh   (val);
        public static double  Tan   (this double a  ) =>     Math.Tan    (a  );
        public static double  Tanh  (this double val) =>     Math.Tanh   (val);
        public static double  Ctg   (this double a  ) => 1 / Math.Tan    (a  );
        public static double  Ctgh  (this double val) => 1 / Math.Tanh   (val);

        public static double Abs    (this double val) =>     Math.Abs    (val);
        public static double Ceiling(this double a  ) =>     Math.Ceiling(a  );
        public static double Exp    (this double d  ) =>     Math.Exp    (d  );
        public static double Log    (this double d  ) =>     Math.Log    (d  );
        public static double Log10  (this double d  ) =>     Math.Log10  (d  );
        public static double Round  (this double d  ) =>     Math.Round  (d  );
        public static    int Sign   (this double val) =>     Math.Sign   (val);
        public static double Sqrt   (this double d  ) =>     Math.Sqrt   (d  );

        public static double Atan2(this double y   , double x      ) => Math.Atan2(y   , x      );
        public static double Log  (this double val , double newBase) => Math.Log  (val , newBase);
        public static double Max  (this double val1, double val2   ) => Math.Max  (val1, val2   );
        public static double Min  (this double val1, double val2   ) => Math.Min  (val1, val2   );
        public static double Pow  (this double x   , double y      ) => Math.Pow  (x   , y      );
        public static double Round(this double val ,    int d      ) => Math.Round(val , d      );

        public static float Acos   (this float d  ) => (float)     Math.Acos   (d  ) ;
        public static float Asin   (this float d  ) => (float)     Math.Asin   (d  ) ;
        public static float Atan   (this float d  ) => (float)     Math.Atan   (d  ) ;
        public static float Actg   (this float d  ) => (float)(1 / Math.Atan   (d  ));
        public static float  Cos   (this float d  ) => (float)     Math.Cos    (d  ) ;
        public static float  Cosh  (this float val) => (float)     Math.Cosh   (val) ;
        public static float  Sin   (this float a  ) => (float)     Math.Sin    (a  ) ;
        public static float  Sinh  (this float val) => (float)     Math.Sinh   (val) ;
        public static float  Tan   (this float a  ) => (float)     Math.Tan    (a  ) ;
        public static float  Tanh  (this float val) => (float)     Math.Tanh   (val) ;
        public static float  Ctg   (this float a  ) => (float)(1 / Math.Tan    (a  ));
        public static float  Ctgh  (this float val) => (float)(1 / Math.Tanh   (val));

        public static float Abs    (this float val) =>             Math.Abs    (val) ;
        public static float Ceiling(this float a  ) => (float)     Math.Ceiling(a  ) ;
        public static float Exp    (this float d  ) => (float)     Math.Exp    (d  ) ;
        public static float Log    (this float d  ) => (float)     Math.Log    (d  ) ;
        public static float Log10  (this float d  ) => (float)     Math.Log10  (d  ) ;
        public static float Round  (this float d  ) => (float)     Math.Round  (d  ) ;
        public static float Sqrt   (this float d  ) => (float)     Math.Sqrt   (d  ) ;

        public static float Atan2(this float y   , float x      ) => (float)Math.Atan2(y   , x      );
        public static float Log  (this float val , float newBase) => (float)Math.Log  (val , newBase);
        public static float Max  (this float val1, float val2   ) =>        Math.Max  (val1, val2   );
        public static float Min  (this float val1, float val2   ) =>        Math.Min  (val1, val2   );
        public static float Pow  (this float x   , float y      ) => (float)Math.Pow  (x   , y      );
        public static float Round(this float val ,   int d      ) => (float)Math.Round(val , d      );

        public static void FC( ref double Value) =>
            Value = Value % 1 >= 0.5 ? (long)(Value + 0.5) : (long)Value;

        public static long FC(this double Value) =>
                    Value % 1 >= 0.5 ? (long)(Value + 0.5) : (long)Value;

        public static   int A(this   int value,   int alignement,   int divide = 1) =>
            ((value % alignement == 0) ? value : (value + alignement - value % alignement)) / divide;

        public static  uint A(this  uint value,  uint alignement,  uint divide = 1) =>
            ((value % alignement == 0) ? value : (value + alignement - value % alignement)) / divide;

        public static  long A(this  long value,  long alignement,  long divide = 1) =>
            ((value % alignement == 0) ? value : (value + alignement - value % alignement)) / divide;

        public static ulong A(this ulong value, ulong alignement, ulong divide = 1) =>
            ((value % alignement == 0) ? value : (value + alignement - value % alignement)) / divide;

        private static byte[] buf = new  byte[8];

        public static byte[] E(this byte[] le, byte len)
        {             for (byte i = 0; i < len; i++) buf[i] = le[i];
            for (byte i = 0; i < len; i++) le[len - i - 1] = buf[i]; return le; }
        public static byte[] E(this byte[] le, byte len, bool isBE)
        { if (isBE) { for (byte i = 0; i < len; i++) buf[i] = le[i];
                for (byte i = 0; i < len; i++) le[len - i - 1] = buf[i]; } return le; }

        public static  short E(this  short le)
        { for (byte i = 0; i < 2; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 2; i++) { le = (short)((int)le | buf[i]); if (i < 1) le <<= 8; } return le; }
        public static ushort E(this ushort le)
        { for (byte i = 0; i < 2; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 2; i++) { le |= buf[i]; if (i < 1) le <<= 8; } return le; }
        public static    int E(this    int le)
        { for (byte i = 0; i < 4; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 4; i++) { le |= buf[i]; if (i < 3) le <<= 8; } return le; }
        public static   uint E(this   uint le)
        { for (byte i = 0; i < 4; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 4; i++) { le |= buf[i]; if (i < 3) le <<= 8; } return le; }
        public static   long E(this   long le)
        { for (byte i = 0; i < 8; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 8; i++) { le |= buf[i]; if (i < 7) le <<= 8; } return le; }
        public static  ulong E(this  ulong le)
        { for (byte i = 0; i < 8; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 8; i++) { le |= buf[i]; if (i < 7) le <<= 8; } return le; }

        public static  short E(this  short le, bool isBE)
        { if (isBE) { for (byte i = 0; i < 2; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 2; i++) { le = (short)((int)le | buf[i]); if (i < 1) le <<= 8; } } return le; }
        public static ushort E(this ushort le, bool isBE)
        { if (isBE) { for (byte i = 0; i < 2; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 2; i++) { le |= buf[i]; if (i < 1) le <<= 8; } } return le; }
        public static    int E(this    int le, bool isBE)
        { if (isBE) { for (byte i = 0; i < 4; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 4; i++) { le |= buf[i]; if (i < 3) le <<= 8; } } return le; }
        public static   uint E(this   uint le, bool isBE)
        { if (isBE) { for (byte i = 0; i < 4; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 4; i++) { le |= buf[i]; if (i < 3) le <<= 8; } } return le; }
        public static   long E(this   long le, bool isBE)
        { if (isBE) { for (byte i = 0; i < 8; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 8; i++) { le |= buf[i]; if (i < 7) le <<= 8; } } return le; }
        public static  ulong E(this  ulong le, bool isBE)
        { if (isBE) { for (byte i = 0; i < 8; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 8; i++) { le |= buf[i]; if (i < 7) le <<= 8; } } return le; }

        public static  sbyte TI8(this byte[] arr, int offset = 0)
        {  sbyte val; fixed (byte* ptr = arr) val = *( sbyte*)(ptr + offset); return val; }
        public static   byte TU8(this byte[] arr, int offset = 0)
        {   byte val; fixed (byte* ptr = arr) val = *(  byte*)(ptr + offset); return val; }
        public static  short TI16(this byte[] arr, int offset = 0)
        {  short val; fixed (byte* ptr = arr) val = *( short*)(ptr + offset); return val; }
        public static ushort TU16(this byte[] arr, int offset = 0)
        { ushort val; fixed (byte* ptr = arr) val = *(ushort*)(ptr + offset); return val; }
        public static    int TI24(this byte[] arr, int offset = 0)
        {    int val; fixed (byte* ptr = arr) val = (*(ushort*)(ptr + offset))
                    | ((( short)*(sbyte*)(ptr + offset + 2)) << 16); return val; }
        public static   uint TU24(this byte[] arr, int offset = 0)
        {   uint val; fixed (byte* ptr = arr) val = (*(ushort*)(ptr + offset))
                    | (((  uint)*( byte*)(ptr + offset + 2)) << 16); return val; }
        public static    int TI32(this byte[] arr, int offset = 0)
        {    int val; fixed (byte* ptr = arr) val = *(   int*)(ptr + offset); return val; }
        public static   uint TU32(this byte[] arr, int offset = 0)
        {   uint val; fixed (byte* ptr = arr) val = *(  uint*)(ptr + offset); return val; }
        public static   long TI64(this byte[] arr, int offset = 0)
        {   long val; fixed (byte* ptr = arr) val = *(  long*)(ptr + offset); return val; }
        public static  ulong TU64(this byte[] arr, int offset = 0)
        {  ulong val; fixed (byte* ptr = arr) val = *( ulong*)(ptr + offset); return val; }
        public static   Half TF16(this byte[] arr, int offset = 0)
        {   Half val; fixed (byte* ptr = arr) val = *(  Half*)(ptr + offset); return val; }
        public static  float TF32(this byte[] arr, int offset = 0)
        {  float val; fixed (byte* ptr = arr) val = *( float*)(ptr + offset); return val; }
        public static double TF64(this byte[] arr, int offset = 0)
        { double val; fixed (byte* ptr = arr) val = *(double*)(ptr + offset); return val; }
        public static   Vec2 TV2 (this byte[] arr, int offset = 0)
        {   Vec2 val; fixed (byte* ptr = arr) val = *( Vec2*)(ptr + offset); return val; }
        public static   Vec3 TV3 (this byte[] arr, int offset = 0)
        {   Vec3 val; fixed (byte* ptr = arr) val = *( Vec3*)(ptr + offset); return val; }
        public static   Vec4 TV4 (this byte[] arr, int offset = 0)
        {   Vec4 val; fixed (byte* ptr = arr) val = *( Vec4*)(ptr + offset); return val; }
        public static   Quat TQ (this byte[] arr, int offset = 0)
        {   Quat val; fixed (byte* ptr = arr) val = *( Quat*)(ptr + offset); return val; }

        public static void GBy(this byte[] arr,  sbyte val, int offset = 0)
        { fixed (byte* ptr = arr) *( sbyte*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,   byte val, int offset = 0)
        { fixed (byte* ptr = arr) *(  byte*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,  short val, int offset = 0)
        { fixed (byte* ptr = arr) *( short*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr, ushort val, int offset = 0)
        { fixed (byte* ptr = arr) *(ushort*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,    int val, int offset = 0)
        { fixed (byte* ptr = arr) *(   int*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,   uint val, int offset = 0)
        { fixed (byte* ptr = arr) *(  uint*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,   long val, int offset = 0)
        { fixed (byte* ptr = arr) *(  long*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,  ulong val, int offset = 0)
        { fixed (byte* ptr = arr) *( ulong*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,   Half val, int offset = 0)
        { fixed (byte* ptr = arr) *(  Half*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,  float val, int offset = 0)
        { fixed (byte* ptr = arr) *( float*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr, double val, int offset = 0)
        { fixed (byte* ptr = arr) *(double*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,   Vec2 val, int offset = 0)
        { fixed (byte* ptr = arr) *(  Vec2*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,   Vec3 val, int offset = 0)
        { fixed (byte* ptr = arr) *(  Vec3*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,   Vec4 val, int offset = 0)
        { fixed (byte* ptr = arr) *(  Vec4*)(ptr + offset) = val; }
        public static void GBy(this byte[] arr,   Quat val, int offset = 0)
        { fixed (byte* ptr = arr) *(  Quat*)(ptr + offset) = val; }

        public static  sbyte CITSB(this    int c)
        {                return ( sbyte)(c > 0x0000007F ?
                0x0000007F : c < -0x00000080 ? -0x00000080 : c); }
        public static   byte CITB (this    int c)
        {                return (  byte)(c > 0x000000FF ?
                0x000000FF : c <  0x00000000 ?  0x00000000 : c); }
        public static  short CITS (this    int c)
        {                return ( short)(c > 0x00007FFF ?
                0x00007FFF : c < -0x00008000 ? -0x00008000 : c); }
        public static ushort CITUS(this    int c)
        {                return (ushort)(c > 0x0000FFFF ?
                0x0000FFFF : c <  0x00000000 ?  0x00000000 : c); }
        public static  sbyte CFTSB(this  float c)
        { c = c.Round(); return ( sbyte)(c > 0x0000007F ?
                0x0000007F : c < -0x00000080 ? -0x00000080 : c); }
        public static   byte CFTB (this  float c)
        { c = c.Round(); return (  byte)(c > 0x000000FF ?
                0x000000FF : c <  0x00000000 ?  0x00000000 : c); }
        public static  short CFTS (this  float c)
        { c = c.Round(); return ( short)(c > 0x00007FFF ?
                0x00007FFF : c < -0x00008000 ? -0x00008000 : c); }
        public static ushort CFTUS(this  float c)
        { c = c.Round(); return (ushort)(c > 0x0000FFFF ?
                0x0000FFFF : c <  0x00000000 ?  0x00000000 : c); }
        public static  sbyte CFTSB(this double c)
        { c = c.Round(); return ( sbyte)(c > 0x0000007F ?
                0x0000007F : c < -0x00000080 ? -0x00000080 : c); }
        public static   byte CFTB (this double c)
        { c = c.Round(); return (  byte)(c > 0x000000FF ?
                0x000000FF : c <  0x00000000 ?  0x00000000 : c); }
        public static  short CFTS (this double c)
        { c = c.Round(); return ( short)(c > 0x00007FFF ?
                0x00007FFF : c < -0x00008000 ? -0x00008000 : c); }
        public static ushort CFTUS(this double c)
        { c = c.Round(); return (ushort)(c > 0x0000FFFF ?
                0x0000FFFF : c <  0x00000000 ?  0x00000000 : c); }
        public static    int CFTI (this double c)
        { c = c.Round(); return (   int)(c > 0x7FFFFFFF ?
                0x7FFFFFFF : c < -0x80000000 ? -0x80000000 : c); }
        public static   uint CFTUI(this double c)
        { c = c.Round(); return (  uint)(c > 0xFFFFFFFF ?
                0xFFFFFFFF : c <  0xFFFFFFFF ?  0x00000000 : c); }

        public static    int ToI32(this  float f) => *(  int*)&f;
        public static   uint ToU32(this  float f) => *( uint*)&f;
        public static   long ToI64(this double f) => *( long*)&f;
        public static  ulong ToU64(this double f) => *(ulong*)&f;

        public static  float ToF32(this   int i) => *( float*)&i;
        public static  float ToF32(this  uint i) => *( float*)&i;
        public static double ToF64(this  long i) => *(double*)&i;
        public static double ToF64(this ulong i) => *(double*)&i;

        public static string ToS(this  int d, bool BE) =>
            BitConverter.GetBytes(d.E(BE)).ToASCII();

        public static string ToS(this uint d, bool BE) =>
            BitConverter.GetBytes(d.E(BE)).ToASCII();

        public static string ToS(this  object d)
        {
                 if (d ==   null        ) return "Null";
            else if (d is   bool boolean) return boolean ? "true" : "false";
            else if (d is  float f32    ) return ToS(f32);
            else if (d is double f64    ) return ToS(f64);
            return d.ToString();
        }

        private static readonly string NDS =
             System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

        public static string ToS(this   bool? d) => (d ?? default).ToString();
        public static string ToS(this   bool  d) => d.ToString().ToLower();
        public static string ToS(this  sbyte? d) => (d ?? default).ToString();
        public static string ToS(this  sbyte  d) => d.ToString();
        public static string ToS(this   byte? d) => (d ?? default).ToString();
        public static string ToS(this   byte  d) => d.ToString();
        public static string ToS(this  short? d) => (d ?? default).ToString();
        public static string ToS(this  short  d) => d.ToString();
        public static string ToS(this ushort? d) => (d ?? default).ToString();
        public static string ToS(this ushort  d) => d.ToString();
        public static string ToS(this    int? d) => (d ?? default).ToString();
        public static string ToS(this    int  d) => d.ToString();
        public static string ToS(this   uint? d) => (d ?? default).ToString();
        public static string ToS(this   uint  d) => d.ToString();
        public static string ToS(this   long? d) => (d ?? default).ToString();
        public static string ToS(this   long  d) => d.ToString();
        public static string ToS(this  ulong? d) => (d ?? default).ToString();
        public static string ToS(this  ulong  d) => d.ToString();
        public static string ToS(this  float? d, int round) => (d ?? default).ToS(round);
        public static string ToS(this  float? d)            => (d ?? default).ToS(15);
        public static string ToS(this  float  d, int round)
        { string s = (d.ToU32() & 0x80000000) != 0 ? "-" : ""; d = (d.ToU32() & 0x7FFFFFFF).ToF32();
          return s + Math.Round(d, round).ToString().ToLower().Replace(NDS, "."); }
        public static string ToS(this  float  d)
        { string s = (d.ToU32() & 0x80000000) != 0 ? "-" : ""; d = (d.ToU32() & 0x7FFFFFFF).ToF32();
          return s + Math.Round(d,    15).ToString().ToLower().Replace(NDS, "."); }
        public static string ToS(this double? d, int round) => (d ?? default).ToS(round);
        public static string ToS(this double? d)            => (d ?? default).ToS();
        public static string ToS(this double  d, int round)
        { string s = (d.ToU64() & 0x8000000000000000) != 0 ? "-" : ""; d = (d.ToU64() & 0x7FFFFFFFFFFFFFFF).ToF64();
          return s + Math.Round(d, round).ToString().ToLower().Replace(NDS, "."); }
        public static string ToS(this double  d)
        { string s = (d.ToU64() & 0x8000000000000000) != 0 ? "-" : ""; d = (d.ToU64() & 0x7FFFFFFFFFFFFFFF).ToF64();
          return s + Math.Round(d,    15).ToString().ToLower().Replace(NDS, "."); }
        public static  float ToF32(this string s)
        { if (s.Length < 1) return 0;  uint d = 0; if (s[0] == '-') d = 0x80000000;
          d |=  float.Parse(s.Replace(".", NDS)).ToU32(); return d.ToF32(); }
        public static double ToF64(this string s)
        { if (s.Length < 1) return 0; ulong d = 0; if (s[0] == '-') d = 0x8000000000000000;
          d |= double.Parse(s.Replace(".", NDS)).ToU64(); return d.ToF64(); }
        public static bool ToF32(this string s, out  float  val)
        { val = 0; if (s.Length < 1) return false;  uint d = 0; if (s[0] == '-') d = 0x80000000;
          bool b =  float.TryParse(s.Replace(".", NDS), out val); if (b) val = (d | val.ToU32()).ToF32(); return b; }
        public static bool ToF64(this string s, out double  val)
        { val = 0; if (s.Length < 1) return false; ulong d = 0; if (s[0] == '-') d = 0x8000000000000000;
          bool b = double.TryParse(s.Replace(".", NDS), out val); if (b) val = (d | val.ToU64()).ToF64(); return b; }
        public static bool ToF32(this string s, out  float? val)
        { val = null; if (s.Length < 1) return false;  uint d = 0; if (s[0] == '-') d = 0x80000000;
          bool b =  float.TryParse(s.Replace(".", NDS), out  float t); if (b) val = (d | t.ToU32()).ToF32(); return b; }
        public static bool ToF64(this string s, out double? val)
        { val = null; if (s.Length < 1) return false; ulong d = 0; if (s[0] == '-') d = 0x8000000000000000;
          bool b = double.TryParse(s.Replace(".", NDS), out double t); if (b) val = (d | t.ToU64()).ToF64(); return b; }

        public static T[] ReverseEndian<T>(this T[] a)
        {
            if (a == null || a.Length < 1) return a;

            int length = a.Length;
            T[] b = new T[length];
            for (int i = 0; i < length; i++)
                b[i] = (T)ReverseEndian(a[i]);
            return b;
        }

        public static object ReverseEndian(this object a)
        {
            object c = default;
            System.Reflection.FieldInfo[] d = a.GetType().GetFields();
            foreach (System.Reflection.FieldInfo field in d)
            {
                object e = field.GetValue(a);
                Type t = field.FieldType;
                     if (t == typeof( short)) e = (( short)e).E();
                else if (t == typeof(ushort)) e = ((ushort)e).E();
                else if (t == typeof(   int)) e = ((   int)e).E();
                else if (t == typeof(  uint)) e = ((  uint)e).E();
                else if (t == typeof(  long)) e = ((  long)e).E();
                else if (t == typeof( ulong)) e = (( ulong)e).E();
                else if (t == typeof( float)) e = (( float)e).ToI32().E().ToF32();
                else if (t == typeof(double)) e = ((double)e).ToI64().E().ToF64();
                else if (t == typeof(  Half)) e = (Half)((ushort)(Half)e).E();
                else e = ReverseEndian(e);
                field.SetValue(a, e);
            }
            return c;
        }
    }
}
