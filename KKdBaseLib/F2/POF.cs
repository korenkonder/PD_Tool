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
            Value Val = 0;
            Offsets = KKdList<long>.New;
            fixed (byte* ptr = data)
            {
                int i = 0, offset = 0, v = 0;
                byte bitShift = (byte)(shiftX ? 3 : 2);

                int length = *(int*)ptr - 4;
                byte* localPtr = ptr + 4;
                while (length > i)
                {
                    v = *ptr & 0x3F;
                    Val = (Value)(*ptr & 0xC0);
                    localPtr++; i++;
                         if (Val == Value.Int32  )
                    { v = (v << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; localPtr += 3; i += 3; }
                    else if (Val == Value.Int16  )
                    { v = (v <<  8) |  ptr[0];                                 localPtr += 1; i += 3; }
                    else if (Val == Value.Invalid) break;
                    offset += v;
                    Offsets.Add(offset << bitShift);
                }
            }
        }

        public unsafe byte[] Write(bool shiftX)
        {
            Offsets.Sort();
            long offset = 0;
            byte bitShift = (byte)(shiftX ? 3 : 2);
            int max1 = 0x00100 >> bitShift;
            int max2 = 0x10000 >> bitShift;

            byte[] data = new byte[length(shiftX)];
            fixed (byte* ptr = data)
            {
                byte Val = 0;
                *(int*)ptr = length(shiftX);
                byte* localPtr = ptr + 4;
                for (int i = 0; i < Offsets.Count; i++)
                {
                    offset = Offsets[i];
                    if (i > 0) { offset -= Offsets[i - 1]; if (offset == 0) continue; }

                    offset >>= bitShift;
                    Val = (byte)(offset > max2 ? Value.Int32 : offset > max1 ? Value.Int16 : Value.Int8);
                         if (offset < max1)   *localPtr = (byte)(Val |  offset       );
                    else if (offset < max2) { *localPtr = (byte)(Val | (offset >>  8)); localPtr++;
                                              *localPtr = (byte)        offset        ; }
                    else                    { *localPtr = (byte)(Val | (offset >> 24)); localPtr++;
                                              *localPtr = (byte)       (offset >> 16) ; localPtr++;
                                              *localPtr = (byte)       (offset >>  8) ; localPtr++;
                                              *localPtr = (byte)        offset        ; }
                    localPtr++;
                }
            }
            return data;
        }

        private int length(bool shiftX = false)
        {
            int length = 5;
            long offset = 0;
            byte bitShift = (byte)(shiftX ? 3 : 2);
            int max1 = 0x00100 >> bitShift;
            int max2 = 0x10000 >> bitShift;
            for (int i = 0; i < Offsets.Count; i++)
            {
                offset = Offsets[i];
                if (i > 0) { offset -= Offsets[i - 1]; if (offset == 0) continue; }

                offset >>= bitShift;
                     if (offset < max1) length += 1;
                else if (offset < max2) length += 2;
                else                    length += 4;
            }
            return length;
        }

        public enum Value : byte
        {
            Invalid = 0b00000000,
            Int8    = 0b01000000,
            Int16   = 0b10000000,
            Int32   = 0b11000000,
        }

        public override string ToString() =>
            $"{(NotNull ? $"Offsets Count: {Offsets.Count}" : "No POF")}";
    }
}
