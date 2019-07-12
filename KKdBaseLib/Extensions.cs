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

        public static void FloorCeiling( ref double Value) => 
            Value = Value % 1 >= 0.5 ? (long)(Value + 0.5) : (long) Value;

        public static long FloorCeiling(this double Value) =>
                    Value % 1 >= 0.5 ? (long)(Value + 0.5) : (long) Value;

        public static   int Align(this   int value,   int alignement,   int divide = 1) =>
            ((value % alignement == 0) ? value : (value + alignement - value % alignement)) / divide;

        public static  uint Align(this  uint value,  uint alignement,  uint divide = 1) =>
            ((value % alignement == 0) ? value : (value + alignement - value % alignement)) / divide;

        public static  long Align(this  long value,  long alignement,  long divide = 1) =>
            ((value % alignement == 0) ? value : (value + alignement - value % alignement)) / divide;

        public static ulong Align(this ulong value, ulong alignement, ulong divide = 1) =>
            ((value % alignement == 0) ? value : (value + alignement - value % alignement)) / divide;

        public static byte[] buf    = new  byte[8];
        public static byte*  bufPtr = buf.GetPtr();

        public static  long Endian(this   long LE, byte Len, bool IsBE)
        { if (IsBE) { for (byte i = 0; i < Len; i++) { bufPtr[i] = (byte)LE; LE >>= 8; } LE = 0;
                for (byte i = 0; i < Len; i++) { LE |= bufPtr[i]; if (i < Len - 1) LE <<= 8; } } return LE; }

        public static ulong Endian(this  ulong LE, byte Len, bool IsBE)
        { if (IsBE) { for (byte i = 0; i < Len; i++) { bufPtr[i] = (byte)LE; LE >>= 8; } LE = 0;
                for (byte i = 0; i < Len; i++) { LE |= bufPtr[i]; if (i < Len - 1) LE <<= 8; } } return LE; }
        
        
        public static  sbyte CITSB(this    int c)
        {                if (c >  0x0000007F) c =  0x0000007F;
                    else if (c < -0x00000080) c = -0x00000080; return ( sbyte)c; }

        public static   byte CITB (this    int c)
        {                if (c >  0x000000FF) c =  0x000000FF;
                    else if (c <  0x00000000) c =  0x00000000; return (  byte)c; }

        public static  short CITS (this    int c)
        {                if (c >  0x00007FFF) c =  0x00007FFF;
                    else if (c < -0x00008000) c = -0x00008000; return ( short)c; }

        public static ushort CITUS(this    int c)
        {                if (c >  0x0000FFFF) c =  0x0000FFFF;
                    else if (c <  0x00000000) c =  0x00000000; return (ushort)c; }

        public static  sbyte CFTSB(this  float c)
        { c = c.Round(); if (c >  0x0000007F) c =  0x0000007F;
                    else if (c < -0x00000080) c = -0x00000080; return ( sbyte)c; }

        public static   byte CFTB (this  float c)
        { c = c.Round(); if (c >  0x000000FF) c =  0x000000FF;
                    else if (c <  0x00000000) c =  0x00000000; return (  byte)c; }

        public static  short CFTS (this  float c)
        { c = c.Round(); if (c >  0x00007FFF) c =  0x00007FFF;
                    else if (c < -0x00008000) c = -0x00008000; return ( short)c; }

        public static ushort CFTUS(this  float c)
        { c = c.Round(); if (c >  0x0000FFFF) c =  0x0000FFFF;
                    else if (c <  0x00000000) c =  0x00000000; return (ushort)c; }

        public static    int CFTI (this  float c)
        { c = c.Round();                                       return (   int)c; }

        public static   uint CFTUI(this  float c)
        { c = c.Round();                                       return (  uint)c; }

        public static  sbyte CFTSB(this double c)
        { c = c.Round(); if (c >  0x0000007F) c =  0x0000007F;
                    else if (c < -0x00000080) c = -0x00000080; return ( sbyte)c; }

        public static   byte CFTB (this double c)
        { c = c.Round(); if (c >  0x000000FF) c =  0x000000FF;
                    else if (c <  0x00000000) c =  0x00000000; return (  byte)c; }

        public static  short CFTS (this double c)
        { c = c.Round(); if (c >  0x00007FFF) c =  0x00007FFF;
                    else if (c < -0x00008000) c = -0x00008000; return ( short)c; }

        public static ushort CFTUS(this double c)
        { c = c.Round(); if (c >  0x0000FFFF) c =  0x0000FFFF;
                    else if (c <  0x00000000) c =  0x00000000; return (ushort)c; }

        public static    int CFTI (this double c)
        { c = c.Round(); if (c >  0x7FFFFFFF) c =  0x7FFFFFFF;
                    else if (c < -0x80000000) c = -0x80000000; return (   int)c; }

        public static   uint CFTUI(this double c)
        { c = c.Round(); if (c >  0xFFFFFFFF) c =  0xFFFFFFFF;
                    else if (c <  0x00000000) c =  0x00000000; return (  uint)c; }

        public static  sbyte* GetPtr(this  sbyte[] array) { fixed ( sbyte* tempPtr = array) return tempPtr; }
        public static   byte* GetPtr(this   byte[] array) { fixed (  byte* tempPtr = array) return tempPtr; }
        public static  short* GetPtr(this  short[] array) { fixed ( short* tempPtr = array) return tempPtr; }
        public static ushort* GetPtr(this ushort[] array) { fixed (ushort* tempPtr = array) return tempPtr; }
        public static    int* GetPtr(this    int[] array) { fixed (   int* tempPtr = array) return tempPtr; }
        public static   uint* GetPtr(this   uint[] array) { fixed (  uint* tempPtr = array) return tempPtr; }
        public static   long* GetPtr(this   long[] array) { fixed (  long* tempPtr = array) return tempPtr; }
        public static  ulong* GetPtr(this  ulong[] array) { fixed ( ulong* tempPtr = array) return tempPtr; }
        public static  float* GetPtr(this  float[] array) { fixed ( float* tempPtr = array) return tempPtr; }
        public static double* GetPtr(this double[] array) { fixed (double* tempPtr = array) return tempPtr; }

        public static string ToString(this     int d, bool IsBE) =>
            BitConverter.GetBytes((int)((long)d).Endian(4, IsBE)).ToASCII();
        
        public static string ToString(this  object d)
        {
            if (d == null) return "Null";
            else if (d is  float F32) return ToString(F32);
            else if (d is double F64) return ToString(F64);
            return d.ToString();
        }

        private static readonly string NumberDecimalSeparator =
             System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

        public static string ToString(this   bool? d) => d.GetValueOrDefault().ToString();
        public static string ToString(this   bool  d) => d.ToString().ToLower();
        public static string ToString(this  sbyte? d) => d.GetValueOrDefault().ToString();
        public static string ToString(this  sbyte  d) => d.ToString();
        public static string ToString(this   byte? d) => d.GetValueOrDefault().ToString();
        public static string ToString(this   byte  d) => d.ToString();
        public static string ToString(this  short? d) => d.GetValueOrDefault().ToString();
        public static string ToString(this  short  d) => d.ToString();
        public static string ToString(this ushort? d) => d.GetValueOrDefault().ToString();
        public static string ToString(this ushort  d) => d.ToString();
        public static string ToString(this    int? d) => d.GetValueOrDefault().ToString();
        public static string ToString(this    int  d) => d.ToString();
        public static string ToString(this   uint? d) => d.GetValueOrDefault().ToString();
        public static string ToString(this   uint  d) => d.ToString();
        public static string ToString(this   long? d) => d.GetValueOrDefault().ToString();
        public static string ToString(this   long  d) => d.ToString();
        public static string ToString(this  ulong? d) => d.GetValueOrDefault().ToString();
        public static string ToString(this  ulong  d) => d.ToString();
        public static string ToString(this  float? d, byte round) => d.GetValueOrDefault().ToString(round);
        public static string ToString(this  float? d)             => d.GetValueOrDefault().ToString();
        public static string ToString(this  float  d, byte round) =>
            Math.Round(d, round).ToString().ToLower().Replace(NumberDecimalSeparator, ".");
        public static string ToString(this  float  d) =>
                       d        .ToString().ToLower().Replace(NumberDecimalSeparator, ".");
        public static string ToString(this double? d, byte round) => d.GetValueOrDefault().ToString(round);
        public static string ToString(this double? d)             => d.GetValueOrDefault().ToString();
        public static string ToString(this double  d, byte round) => 
            Math.Round(d, round).ToString().ToLower().Replace(NumberDecimalSeparator, ".");
        public static string ToString(this double  d) =>
                       d        .ToString().ToLower().Replace(NumberDecimalSeparator, ".");
        public static  float ToSingle(this string s) =>
             float.   Parse(s.Replace(".", NumberDecimalSeparator));
        public static   bool ToSingle(this string s, out float value) =>
             float.TryParse(s.Replace(".", NumberDecimalSeparator), out value);
        public static double ToDouble(this string s) =>
            double.   Parse(s.Replace(".", NumberDecimalSeparator));
        public static   bool ToDouble(this string s, out double value) =>
            double.TryParse(s.Replace(".", NumberDecimalSeparator), out value);
        public static   bool ToSingle(this string s, out float? value)
        { bool Val = ToSingle(s, out  float val); value = val; return Val; }
        public static   bool ToDouble(this string s, out double? value)
        { bool Val = ToDouble(s, out double val); value = val; return Val; }
    }
}
