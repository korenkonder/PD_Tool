namespace KKdBaseLib.F2
{
    public struct POF : INull
    {
        public KKdList<long> Offsets;

        public bool  IsNull => Offsets. IsNull;
        public bool NotNull => Offsets.NotNull;

        public int Length  => length(false).A(0x10);
        public int LengthX => length( true).A(0x10);

        public unsafe void Read(byte[] data, bool shiftX)
        {
            Offsets = KKdList<long>.New;
            fixed (byte* ptr = data)
            {
                long offset = 0;
                int i = 0, j = 0, v = 0;
                byte bitShift = (byte)(shiftX ? 3 : 2);

                int length = *(int*)ptr - 4;
                byte* l = ptr + 4;
                Offsets.Capacity = length;
                while (length > i)
                {
                    if (ReadPOFValue(ref l, out v)) break;
                    j++;
                    offset += v;
                    Offsets.Add(offset << bitShift);
                }
                Offsets.Capacity = j;
            }
        }

        public unsafe byte[] Write(bool shiftX)
        {
            Offsets.Sort();
            long j, k, o;
            byte bitShift, v;
            bitShift = (byte)(shiftX ? 3 : 2);
            v = (byte)((1 << bitShift) - 1);

            j = 0;
            byte[] data = new byte[length(shiftX)];
            fixed (byte* ptr = data)
            {
                *(int*)ptr = length(shiftX);
                byte* l = ptr + 4;
                for (int i = 0; i < Offsets.Count; i++)
                {
                    o = Offsets[i];
                    if ((o & v) != 0)
                    {
                        WritePOFValue(ref l, 0x7FFFFFFF);
                        break;
                    }
                    
                    k = o - j;
                    if (i > 0 & k == 0)
                        continue;
                    j = o;
                    o = k;
                    WritePOFValue(ref l, o >> bitShift);
                }
            }
            return data;
        }

        [System.ThreadStatic] private static Value value;

        private unsafe static bool ReadPOFValue(ref byte* ptr, out int v)
        {
            v = *ptr & 0x3F;
            value = (Value)((*ptr & 0xC0) >> 6);
            ptr++;
                 if (value == Value.Int32  ) v = (((((v << 8) | *ptr++) << 8) | *ptr++) << 8) | *ptr++;
            else if (value == Value.Int16  ) v =     (v << 8) | *ptr++; 
            else if (value == Value.Invalid) { v = 0; return true; }
            return false;
        }

        private unsafe static bool WritePOFValue(ref byte* ptr, long val)
        {
            if (val < 0x00000040)
                *ptr++ = (byte)(((byte)Value.Int8  << 6) | ( val        & 0x3F));
            else if (val < 0x00004000)
            {
                *ptr++ = (byte)(((byte)Value.Int16 << 6) | ((val >>  8) & 0x3F));
                *ptr++ = (byte)(val & 0xFF);
            }
            else if (val < 0x40000000)
            {
                *ptr++ = (byte)(((byte)Value.Int32 << 6) | ((val >> 24) & 0x3F));
                *ptr++ = (byte)((val >> 16) & 0xFF);
                *ptr++ = (byte)((val >>  8) & 0xFF);
                *ptr++ = (byte)( val        & 0xFF);
            }
            else
            {
                *ptr++ = (byte)Value.Invalid << 6;
                return true;
            }
            return false;
        }

        private int length(bool shiftX = false)
        {
            int length = 4;
            long offset = 0;
            byte bitShift = (byte)(shiftX ? 3 : 2);
            int max1 = 0x00000040;
            int max2 = 0x00004000;
            int max3 = 0x40000000;
            for (int i = 0; i < Offsets.Count; i++)
            {
                offset = Offsets[i];
                if (i > 0) { offset -= Offsets[i - 1]; if (offset == 0) continue; }

                offset >>= bitShift;
                     if (offset < max1) length += 1;
                else if (offset < max2) length += 2;
                else if (offset < max3) length += 4;
                else                    length += 1;
            }
            return length;
        }

        public enum Value : byte
        {
            Invalid = 0b00,
            Int8    = 0b01,
            Int16   = 0b10,
            Int32   = 0b11,
        }

        public override string ToString() =>
            $"{(NotNull ? $"Offsets Count: {Offsets.Count}" : "No POF")}";
    }
}
