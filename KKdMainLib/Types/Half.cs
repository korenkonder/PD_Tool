using System;

namespace KKdMainLib.Types
{
    public struct Half : IFormattable
    {
        private ushort _value;

        public static explicit operator Half(ushort bits) => new Half() { _value = bits };

        public static explicit operator ushort(Half bits) => bits._value;

        public static explicit operator double(Half h)
        {
                 if (h._value       == 0x0000) return +0;
            else if (h._value       == 0x8000) return -0;
            else if (h._value       == 0x7C00) return  double.PositiveInfinity;
            else if (h._value       == 0xFC00) return  double.NegativeInfinity;
            else if (h._value >> 10 == 0x001F) return  double.NaN;
            else if (h._value >> 10 == 0x003F) return -double.NaN;

            long exponent = ((h._value >> 10) & 0x1F);
            long mantissa = (h._value & 0x3FF);
            sbyte n = (sbyte)(((h._value >> 15 & 0x01) == 0) ? 1 : -1);

            double m = (((long)1 << 10) | mantissa) / Math.Pow(2, 10);
            double x = Math.Pow(2, exponent - (0x1F >> 1));
            double d = n * m * x;
            return d;
        }

        public static explicit operator Half(double val)
        {
            Half h = new Half();
                 if (val == +0                      ) h._value = 0x0000;
            else if (val == -0                      ) h._value = 0x8000;
            else if (val ==  double.NaN             ) h._value = 0x7FFF;
            else if (val == -double.NaN             ) h._value = 0xFFFF;
            else if (val ==  double.PositiveInfinity) h._value = 0x7C00;
            else if (val ==  double.NegativeInfinity) h._value = 0xFC00;
            else                                      h._value = ToDouble(val);
            return h;
        }

        public static ushort ToDouble(double val)
        {
            ushort Sign = 0;
            if (val < 0)
                Sign = 0x8000;
            val = Math.Abs(val);
            double Pow1 = 1;
            double Pow2 = 1 << 10;
            double x = 0;

            int MaxPow = (1 << 4);

            int i = 0;
            while (i < MaxPow && i > -MaxPow + 1)
            {
                Pow1 = Math.Pow(2, i);
                x = val / Pow1;
                if (x >= 1 && x < 2)
                {
                    ushort exponent_max = (ushort)Math.Ceiling(x * Pow2);
                    ushort exponent_min = (ushort)Math.Floor  (x * Pow2);
                    ushort exponent = Math.Abs(x - exponent_max / Pow2) >
                        Math.Abs(x - exponent_min / Pow2) ? exponent_max : exponent_min;
                    ushort mantissa = (ushort)(i + MaxPow - 1);
                    ushort d = (ushort)(Sign | ((mantissa & 0x001F) << 10) | (exponent & 0x03FF));
                    return d;
                }
                else if (val < 1) i--;
                else              i++;
            }

            return i >= +0 ? (ushort)0x7C00 : (ushort)0xFC00;
        }

        public override string ToString() => ((double)this).ToString();
        public string ToString(string format, IFormatProvider formatProvider) =>
            ((double)this).ToString(format, formatProvider);
        public override int GetHashCode() => base.GetHashCode();
    }
}
