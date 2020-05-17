using System;

namespace KKdBaseLib
{
    public struct Half : IFormattable
    {
        private ushort _value;

        public static Half         NaN => new Half { _value = 0x7FFF };
        public static Half PositiveNaN => new Half { _value = 0x7FFF };
        public static Half NegativeNaN => new Half { _value = 0xFFFF };
        public static Half PositiveZero => new Half { _value = 0x0000 };
        public static Half NegativeZero => new Half { _value = 0x8000 };
        public static Half PositiveInfinity => new Half { _value = 0x7C00 };
        public static Half NegativeInfinity => new Half { _value = 0xFC00 };

        public static explicit operator Half(ushort bits) => new Half() { _value = bits };

        public static explicit operator ushort(Half bits) => bits._value;

        public static unsafe implicit operator  float(Half h)
        {
            int sign     =  (h._value >> 15) & 0x001;
            int exponent = ((h._value >> 10) & 0x01F) + 127 - 15;
            int mantissa =   h._value        & 0x3FF;

            int si32 = (sign << 31) | (exponent << 23) | (mantissa << 13);
            return *(float*)&si32;
        }

        public static unsafe explicit operator Half(float val)
        {
            int si32 = *(int*)&val;
            ushort sign     = (ushort)( (si32 >> 16) & 0x8000);
             short exponent = ( short)(((si32 >> 23) & 0x00FF) - 127 + 15);
            ushort mantissa = (ushort)( (si32 >> 13) & 0x03FF);

                 if (exponent <  0) { exponent =  0; mantissa = 0; }
            else if (exponent > 30)   exponent = 31;

            return new Half { _value = (ushort)(sign | (exponent << 10) | mantissa) };
        }

        public static unsafe implicit operator double(Half h)
        {
            int sign     =  (h._value >> 15) & 0x001;
            int exponent = ((h._value >> 10) & 0x01F) + 1023 - 15;
            int mantissa =   h._value        & 0x3FF;

            long si64 = ((long)sign << 63) | ((long)exponent << 52) | ((long)mantissa << 42);
            return *(double*)&si64;
        }

        public static unsafe explicit operator Half(double val)
        {
            long si64 = *(long*)&val;
            ushort sign     = (ushort) ((si64 >> 48) & 0x8000);
             short exponent = ( short)(((si64 >> 52) & 0x07FF) - 1023 + 15);
            ushort mantissa = (ushort) ((si64 >> 42) & 0x03FF);

                 if (exponent <  0) { exponent =  0; mantissa = 0; }
            else if (exponent > 30)   exponent = 31;

            return new Half { _value = (ushort)(sign | (exponent << 10) | mantissa) };
        }

        public static Half operator - (Half a        ) => new Half() { _value = (ushort)(a._value ^ 0x8000) };
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
