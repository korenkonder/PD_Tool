using System;

namespace KKdMainLib
{
    public static class MathExtensions
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

        public static        byte[] buf    = new  byte[8];
        public static unsafe byte*  bufPtr = buf.GetPtr();

        public static unsafe  long Endian(this   long LE, byte Len, bool IsBE)
        { if (IsBE) { for (byte i = 0; i < Len; i++) { bufPtr[i] = (byte)LE; LE >>= 8; } LE = 0;
                for (byte i = 0; i < Len; i++) { LE |= bufPtr[i]; if (i < Len - 1) LE <<= 8; } } return LE; }

        public static unsafe ulong Endian(this  ulong LE, byte Len, bool IsBE)
        { if (IsBE) { for (byte i = 0; i < Len; i++) { bufPtr[i] = (byte)LE; LE >>= 8; } LE = 0;
                for (byte i = 0; i < Len; i++) { LE |= bufPtr[i]; if (i < Len - 1) LE <<= 8; } } return LE; }
        
        public static sbyte CITSB(this int c)
        {
                 if (c >  0x7F) c =  0x7F;
            else if (c < -0x80) c = -0x80;
            return (sbyte)c;
        }

        public static byte CITB(this int c)
        {
                 if (c > 0xFF) c = 0xFF;
            else if (c < 0x00) c = 0x00;
            return (byte)c;
        }

        public static short CITS(this int c)
        {
                 if (c >  0x7FFF) c =  0x7FFF;
            else if (c < -0x8000) c = -0x8000;
            return (short)c;
        }

        public static ushort CITUS(this int c)
        {
                 if (c > 0xFFFF) c = 0xFFFF;
            else if (c < 0x0000) c = 0x0000;
            return (ushort)c;
        }

        public static sbyte CFTSB(this float c)
        {
            c = c.Round();
                 if (c >  0x7F) c =  0x7F;
            else if (c < -0x80) c = -0x80;
            return (sbyte)c;
        }

        public static byte CFTB(this float c)
        {
            c = c.Round();
                 if (c > 0xFF) c = 0xFF;
            else if (c < 0x00) c = 0x00;
            return (byte)c;
        }

        public static short CFTS(this float c)
        {
            c = c.Round();
                 if (c >  0x7FFF) c =  0x7FFF;
            else if (c < -0x8000) c = -0x8000;
            return (short)c;
        }

        public static ushort CFTUS(this float c)
        {
            c = c.Round();
                 if (c > 0xFFFF) c = 0xFFFF;
            else if (c < 0x0000) c = 0x0000;
            return (ushort)c;
        }

        public static int CFTI(this float c)
        {
            c = c.Round();
                 if (c >  0x7FFFFFFF) c =  0x7FFFFFFF;
            else if (c < -0x80000000) c = -0x80000000;
            return (int)c;
        }

        public static uint CFTUI(this float c)
        {
            c = c.Round();
                 if (c > 0xFFFFFFFF) c = 0xFFFFFFFF;
            else if (c < 0x00000000) c = 0x00000000;
            return (uint)c;
        }

        public static float Round(this float c) => (float)Math.Round(c);

        public static sbyte CFTSB(this double c)
        {
            c = Math.Round(c);
                 if (c >  0x7F) c =  0x7F;
            else if (c < -0x80) c = -0x80;
            return (sbyte)c;
        }

        public static byte CFTB(this double c)
        {
            c = Math.Round(c);
                 if (c > 0xFF) c = 0xFF;
            else if (c < 0x00) c = 0x00;
            return (byte)c;
        }

        public static short CFTS(this double c)
        {
            c = Math.Round(c);
                 if (c >  0x7FFF) c =  0x7FFF;
            else if (c < -0x8000) c = -0x8000;
            return (short)c;
        }

        public static ushort CFTUS(this double c)
        {
            c = Math.Round(c);
                 if (c > 0xFFFF) c = 0xFFFF;
            else if (c < 0x0000) c = 0x0000;
            return (ushort)c;
        }

        public static int CFTI(this double c)
        {
            c = Math.Round(c);
                 if (c >  0x7FFFFFFF) c =  0x7FFFFFFF;
            else if (c < -0x80000000) c = -0x80000000;
            return (int)c;
        }

        public static uint CFTUI(this double c)
        {
            c = Math.Round(c);
                 if (c > 0xFFFFFFFF) c = 0xFFFFFFFF;
            else if (c < 0x00000000) c = 0x00000000;
            return (uint)c;
        }

        public static unsafe  sbyte* GetPtr(this sbyte[] array)
        {  sbyte* Ptr; fixed ( sbyte* tempPtr = array) Ptr = tempPtr; return Ptr; }

        public static unsafe   byte* GetPtr(this  byte[] array)
        {   byte* Ptr; fixed (  byte* tempPtr = array) Ptr = tempPtr; return Ptr; }

        public static unsafe  short* GetPtr(this short[] array)
        {  short* Ptr; fixed ( short* tempPtr = array) Ptr = tempPtr; return Ptr; }

        public static unsafe ushort* GetPtr(this ushort[] array)
        { ushort* Ptr; fixed (ushort* tempPtr = array) Ptr = tempPtr; return Ptr; }

        public static unsafe    int* GetPtr(this    int[] array)
        {    int* Ptr; fixed (   int* tempPtr = array) Ptr = tempPtr; return Ptr; }

        public static unsafe   uint* GetPtr(this   uint[] array)
        {   uint* Ptr; fixed (  uint* tempPtr = array) Ptr = tempPtr; return Ptr; }

        public static unsafe   long* GetPtr(this   long[] array)
        {   long* Ptr; fixed (  long* tempPtr = array) Ptr = tempPtr; return Ptr; }

        public static unsafe  ulong* GetPtr(this  ulong[] array)
        {  ulong* Ptr; fixed ( ulong* tempPtr = array) Ptr = tempPtr; return Ptr; }

        public static unsafe  float* GetPtr(this  float[] array)
        {  float* Ptr; fixed ( float* tempPtr = array) Ptr = tempPtr; return Ptr; }

        public static unsafe double* GetPtr(this double[] array)
        { double* Ptr; fixed (double* tempPtr = array) Ptr = tempPtr; return Ptr; }
    }
}
