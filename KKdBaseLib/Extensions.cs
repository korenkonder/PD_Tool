using System;

namespace KKdBaseLib
{
    public static unsafe class Extensions
    {
        private const double RadPi = 180 / Math.PI;
        public static double ToDegrees(this double val) => val * RadPi;
        public static double ToRadians(this double val) => val / RadPi;

        public static double Acos   (this double d  ) =>     Math.Acos   (d  );
        public static double Asin   (this double d  ) =>     Math.Asin   (d  );
        public static double Atan   (this double d  ) =>     Math.Atan   (d  );
        public static double Aсtg   (this double d  ) => 1 / Math.Atan   (d  );
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

        public static float ToDegrees(this float val) => (float)(val * RadPi);
        public static float ToRadians(this float val) => (float)(val / RadPi);

        public static float Acos   (this float d  ) => (float)     Math.Acos   (d  ) ;
        public static float Asin   (this float d  ) => (float)     Math.Asin   (d  ) ;
        public static float Atan   (this float d  ) => (float)     Math.Atan   (d  ) ;
        public static float Aсtg   (this float d  ) => (float)(1 / Math.Atan   (d  ));
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
        public static  short E(this  short le, bool isBE)
        { if (isBE) { for (byte i = 0; i < 2; i++) { buf[i] = (byte)le; le >>= 8; } le = 0;
                for (byte i = 0; i < 2; i++) { le = (short)((int)le |
                        buf[i]); if (i < 1) le <<= 8; } } return le; }
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

        public static  short TI16(this byte[] arr)
        {  short val; fixed (byte* ptr = arr) val = *( short*)ptr; return val; }
        public static ushort TU16(this byte[] arr)
        { ushort val; fixed (byte* ptr = arr) val = *(ushort*)ptr; return val; }
        public static    int TI32(this byte[] arr)
        {    int val; fixed (byte* ptr = arr) val = *(   int*)ptr; return val; }
        public static   uint TU32(this byte[] arr)
        {   uint val; fixed (byte* ptr = arr) val = *(  uint*)ptr; return val; }
        public static   long TI64(this byte[] arr)
        {   long val; fixed (byte* ptr = arr) val = *(  long*)ptr; return val; }
        public static  ulong TU64(this byte[] arr)
        {  ulong val; fixed (byte* ptr = arr) val = *( ulong*)ptr; return val; }
        public static  float TF32(this byte[] arr)
        {  float val; fixed (byte* ptr = arr) val = *( float*)ptr; return val; }
        public static double TF64(this byte[] arr)
        { double val; fixed (byte* ptr = arr) val = *(double*)ptr; return val; }

        public static void GBy(this byte[] arr,  short val)
        { fixed (byte* ptr = arr) *( short*)ptr = val; }
        public static void GBy(this byte[] arr, ushort val)
        { fixed (byte* ptr = arr) *(ushort*)ptr = val; }
        public static void GBy(this byte[] arr,    int val)
        { fixed (byte* ptr = arr) *(   int*)ptr = val; }
        public static void GBy(this byte[] arr,   uint val)
        { fixed (byte* ptr = arr) *(  uint*)ptr = val; }
        public static void GBy(this byte[] arr,   long val)
        { fixed (byte* ptr = arr) *(  long*)ptr = val; }
        public static void GBy(this byte[] arr,  ulong val)
        { fixed (byte* ptr = arr) *( ulong*)ptr = val; }
        public static void GBy(this byte[] arr,  float val)
        { fixed (byte* ptr = arr) *( float*)ptr = val; }
        public static void GBy(this byte[] arr, double val)
        { fixed (byte* ptr = arr) *(double*)ptr = val; }

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

        public static string ToString(this     int d, bool BE) =>
            BitConverter.GetBytes(d.E(BE)).ToASCII();

        public static  sbyte* GetPtr(this  sbyte[] array)
        { fixed ( sbyte* tempPtr = array) return tempPtr; }
        public static   byte* GetPtr(this   byte[] array)
        { fixed (  byte* tempPtr = array) return tempPtr; }
        public static  short* GetPtr(this  short[] array)
        { fixed ( short* tempPtr = array) return tempPtr; }
        public static ushort* GetPtr(this ushort[] array)
        { fixed (ushort* tempPtr = array) return tempPtr; }
        public static    int* GetPtr(this    int[] array)
        { fixed (   int* tempPtr = array) return tempPtr; }
        public static   uint* GetPtr(this   uint[] array)
        { fixed (  uint* tempPtr = array) return tempPtr; }
        public static   long* GetPtr(this   long[] array)
        { fixed (  long* tempPtr = array) return tempPtr; }
        public static  ulong* GetPtr(this  ulong[] array)
        { fixed ( ulong* tempPtr = array) return tempPtr; }
        public static  float* GetPtr(this  float[] array)
        { fixed ( float* tempPtr = array) return tempPtr; }
        public static double* GetPtr(this double[] array)
        { fixed (double* tempPtr = array) return tempPtr; }

        public static IntPtr GetIntPtr(this  sbyte[] array)
        { fixed ( sbyte* tempPtr = array) return (IntPtr)tempPtr; }
        public static IntPtr GetIntPtr(this   byte[] array)
        { fixed (  byte* tempPtr = array) return (IntPtr)tempPtr; }
        public static IntPtr GetIntPtr(this  short[] array)
        { fixed ( short* tempPtr = array) return (IntPtr)tempPtr; }
        public static IntPtr GetIntPtr(this ushort[] array)
        { fixed (ushort* tempPtr = array) return (IntPtr)tempPtr; }
        public static IntPtr GetIntPtr(this    int[] array)
        { fixed (   int* tempPtr = array) return (IntPtr)tempPtr; }
        public static IntPtr GetIntPtr(this   uint[] array)
        { fixed (  uint* tempPtr = array) return (IntPtr)tempPtr; }
        public static IntPtr GetIntPtr(this   long[] array)
        { fixed (  long* tempPtr = array) return (IntPtr)tempPtr; }
        public static IntPtr GetIntPtr(this  ulong[] array)
        { fixed ( ulong* tempPtr = array) return (IntPtr)tempPtr; }
        public static IntPtr GetIntPtr(this  float[] array)
        { fixed ( float* tempPtr = array) return (IntPtr)tempPtr; }
        public static IntPtr GetIntPtr(this double[] array)
        { fixed (double* tempPtr = array) return (IntPtr)tempPtr; }
        
        public static string ToS(this  object d)
        {
                 if (d ==   null        ) return "Null";
            else if (d is   bool boolean) return boolean ? "true" : "false";
            else if (d is  float f32    ) return ToS(f32);
            else if (d is double f64    ) return ToS(f64);
            return d.ToString();
        }

        private static readonly string NumberDecimalSeparator =
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
        public static string ToS(this  float? d)            => (d ?? default).ToString();
        public static string ToS(this  float  d, int round) =>
            Math.Round(d, round).ToString().ToLower().Replace(NumberDecimalSeparator, ".");
        public static string ToS(this  float  d) =>
            Math.Round(d,    15).ToString().ToLower().Replace(NumberDecimalSeparator, ".");
        public static string ToS(this double? d, int round) => (d ?? default).ToS(round);
        public static string ToS(this double? d)            => (d ?? default).ToString();
        public static string ToS(this double  d, int round) => 
            Math.Round(d, round).ToString().ToLower().Replace(NumberDecimalSeparator, ".");
        public static string ToS(this double  d) =>
            Math.Round(d,    15).ToString().ToLower().Replace(NumberDecimalSeparator, ".");
        public static  float ToF32(this string s) =>
             float.   Parse(s.Replace(".", NumberDecimalSeparator));
        public static   bool ToF32(this string s, out float value) =>
             float.TryParse(s.Replace(".", NumberDecimalSeparator), out value);
        public static double ToF64(this string s) =>
            double.   Parse(s.Replace(".", NumberDecimalSeparator));
        public static   bool ToF64(this string s, out double value) =>
            double.TryParse(s.Replace(".", NumberDecimalSeparator), out value);
        public static   bool ToF32(this string s, out float? value)
        { bool Val = ToF32(s, out  float val); value = val; return Val; }
        public static   bool ToF64(this string s, out double? value)
        { bool Val = ToF64(s, out double val); value = val; return Val; }
    }
}
