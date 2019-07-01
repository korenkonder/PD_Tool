using System;

namespace KKdMainLib
{
    public static class MathExtensions
    {
        public static void FloorCeiling(ref double Value)
        {   if (Value % 1 >= 0.5) Value = (long)(Value + 0.5);
            else                  Value = (long) Value; }

        public static long FloorCeiling(this double Value)
        {   if (Value % 1 >= 0.5) return (long)(Value + 0.5);
            else                  return (long) Value; }

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
        
        public static double Round(this double c, int d = 0) => Math.Round(c, d);

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
