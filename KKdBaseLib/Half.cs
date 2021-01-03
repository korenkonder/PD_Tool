using System;

namespace KKdBaseLib
{
    public struct Half : IFormattable
    {
        private ushort value;

        public static Half         NaN      => new Half { value = 0x7FFF };
        public static Half PositiveNaN      => new Half { value = 0x7FFF };
        public static Half NegativeNaN      => new Half { value = 0xFFFF };
        public static Half PositiveZero     => new Half { value = 0x0000 };
        public static Half NegativeZero     => new Half { value = 0x8000 };
        public static Half PositiveInfinity => new Half { value = 0x7C00 };
        public static Half NegativeInfinity => new Half { value = 0xFC00 };

        public static explicit operator Half(ushort bits) => new Half() { value = bits };

        public static explicit operator ushort(Half bits) => bits.value;

        public static unsafe implicit operator  float(Half h)
        {
            int si32;
            int sign     = (h.value >> 15) & 0x001;
            int exponent = (h.value >> 10) & 0x01F;
            int mantissa =  h.value        & 0x3FF;
            unchecked { si32 = sign != 0 ? (int)0x80000000 : 0x00000000; }

                 if (exponent == 0x1F) si32 |=                  0x7F800000;
            else if (exponent != 0x00) si32 |= (exponent - 15 + 127) << 23;
            si32 |= mantissa << 13;
            return *(float*)&si32;
        }

        public static unsafe explicit operator Half(float val)
        {
            int si32 = *(int*)&val;
                 if (      si32 == 0x00000000) return new Half { value = 0x0000 };
            else if ((uint)si32 == 0x80000000) return new Half { value = 0x8000 };
            else
            {
                ushort sign     = (ushort)((si32 >> 31) & 0x001);
                ushort exponent = (ushort)((si32 >> 23) & 0x0FF);
                ushort mantissa = (ushort)((si32 >> 13) & 0x3FF);

                if (exponent == 0xFF) exponent = 31;
                else if (exponent != 0x00)
                {
                    exponent -= 127 - 15;
                         if (exponent <  0) exponent = mantissa = 0;
                    else if (exponent > 30) exponent = 31;
                }
                return new Half { value = (ushort)((sign << 15) | (exponent << 10) | mantissa) };
            }
        }

        public static unsafe implicit operator double(Half h)
        {
            long si64;
            long sign     = (h.value >> 15) & 0x001;
            long exponent = (h.value >> 10) & 0x01F;
            long mantissa =  h.value        & 0x3FF;
            unchecked { si64 = sign != 0 ? (long)0x8000000000000000 : 0x0000000000000000; }

                 if (exponent == 0x1F) si64 |=           0x7FF0000000000000;
            else if (exponent != 0x00) si64 |= (exponent - 15 + 1023) << 52;
            si64 |= mantissa << 42;
            return *(double*)&si64;
        }

        public static unsafe explicit operator Half(double val)
        {
            long si64 = *(long*)&val;
                 if (       si64 == 0x0000000000000000) return new Half { value = 0x0000 };
            else if ((ulong)si64 == 0x8000000000000000) return new Half { value = 0x8000 };
            else
            {
                ushort sign     = (ushort)((si64 >> 63) & 0x001);
                ushort exponent = (ushort)((si64 >> 52) & 0x7FF);
                ushort mantissa = (ushort)((si64 >> 42) & 0x3FF);

                if (exponent == 0x7FF) exponent = 31;
                else if (exponent != 0x00)
                {
                    exponent -= 1023 - 15;
                         if (exponent <  0) exponent = mantissa = 0;
                    else if (exponent > 30) exponent = 31;
                }
                return new Half { value = (ushort)((sign << 15) | (exponent << 10) | mantissa) };
            }
        }

        public static Half operator - (Half a        ) => new Half() { value = (ushort)(a.value ^ 0x8000) };
        public static Half operator + (Half a, Half b) => (Half)((float)a + (float)b);
        public static Half operator - (Half a, Half b) => (Half)((float)a - (float)b);
        public static Half operator * (Half a, Half b) => (Half)((float)a * (float)b);
        public static Half operator / (Half a, Half b) => (Half)((float)a / (float)b);
        public static bool operator > (Half a, Half b) => (float)a >  (float)b;
        public static bool operator < (Half a, Half b) => (float)a <  (float)b;
        public static bool operator >=(Half a, Half b) => (float)a >= (float)b;
        public static bool operator <=(Half a, Half b) => (float)a <= (float)b;
        public static bool operator ==(Half a, Half b) => (float)a == (float)b;
        public static bool operator !=(Half a, Half b) => (float)a != (float)b;

        public int CompareTo(object obj) => CompareTo((Half)obj);
        public int CompareTo(Half h) => this == h ? 0 : (this > h ? 1 : -1);
        public bool Equals(Half other) => this == other;
        public override bool Equals(object obj) => base.Equals(obj);
        public override string ToString() => Extensions.ToS((double)this);
        public string ToString(string format, IFormatProvider formatProvider) =>
            ((float)this).ToString(format, formatProvider);
        public override int GetHashCode() => base.GetHashCode();
    }
}
