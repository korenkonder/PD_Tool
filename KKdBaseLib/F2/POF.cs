namespace KKdBaseLib.F2
{
    public unsafe struct POF : INull
    {
        public int Depth;
        public KKdList<long> Offsets;

        public bool  IsNull => Offsets. IsNull;
        public bool NotNull => Offsets.NotNull;

        public int Length  => length(false).Align(0x10);
        public int LengthX => length( true).Align(0x10);

        public static POF Read(byte[] data, bool ShiftX, int Depth = 0)
        {
            Value Val = 0;
            KKdList<long> Offsets = KKdList<long>.New;
            byte* ptr = data.GetPtr();
            int i = 0, Offset = 0, V = 0;
            byte BitShift = (byte)(ShiftX ? 3 : 2);

            int Length = *(int*)ptr - 4; ptr += 4;
            while (Length > i)
            {
                V = *ptr & 0x3F;
                Val = (Value)(*ptr & 0xC0);
                ptr++; i++;
                     if (Val == Value.Int32  )
                { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; i += 3; }
                else if (Val == Value.Int16  )
                { V = (V <<  8) |  ptr[0];                                 ptr += 1; i += 3; }
                else if (Val == Value.Invalid) break;
                Offset += V;
                Offsets.Add(Offset << BitShift);
            }
            return new POF { Depth = Depth, Offsets = Offsets };
        }

        public static byte[] Write(POF POF, bool ShiftX)
        {
            POF.Offsets.Sort();
            long Offset = 0;
            byte BitShift = (byte)(ShiftX ? 3 : 2);
            int Max1 = 0x00100 >> BitShift;
            int Max2 = 0x10000 >> BitShift;

            byte[] data = new byte[POF.Length];
            byte* ptr = data.GetPtr();

            byte Val = 0;
            *(int*)ptr = POF.length(ShiftX); ptr += 4;
            for (int i = 0; i < POF.Offsets.Count; i++)
            {
                Offset = POF.Offsets[i];
                if (i > 0) { Offset -= POF.Offsets[i - 1]; if (Offset == 0) continue; }

                Offset >>= BitShift;
                Val = (byte)(Offset > Max2 ? Value.Int32 : Offset > Max1 ? Value.Int16 : Value.Int8);
                     if (Offset < Max1)   *ptr = (byte)(Val |  Offset       );
                else if (Offset < Max2) { *ptr = (byte)(Val | (Offset >>  8)); ptr++;
                                          *ptr = (byte)        Offset        ; }
                else                    { *ptr = (byte)(Val | (Offset >> 24)); ptr++;
                                          *ptr = (byte)       (Offset >> 16) ; ptr++;
                                          *ptr = (byte)       (Offset >>  8) ; ptr++; 
                                          *ptr = (byte)        Offset        ; }
                ptr++;
            }
            return data;
        }

        private int length(bool ShiftX = false)
        {
            int length = 5;
            long Offset = 0;
            byte BitShift = (byte)(ShiftX ? 3 : 2);
            int Max1 = 0x00100 >> BitShift;
            int Max2 = 0x10000 >> BitShift;
            for (int i = 0; i < Offsets.Count; i++)
            {
                Offset = Offsets[i];
                if (i > 0) { Offset -= Offsets[i - 1]; if (Offset == 0) continue; }

                Offset >>= BitShift;
                     if (Offset < Max1) length += 1;
                else if (Offset < Max2) length += 2;
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
            $"Depth: {Depth}{(NotNull ? $"; Offsets Count: {Offsets.Count}" : "")}";
    }
}
