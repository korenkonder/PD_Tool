using KKdBaseLib;

namespace KKdMainLib.F2nd
{
    public struct POF
    {
        public static unsafe KKdList<long> Read(byte[] data, bool ShiftX)
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
            return Offsets;
        }

        public static unsafe byte[] Write(KKdList<long> Offsets, bool ShiftX)
        {
            Offsets.Sort();
            int Length = 5;
            long Offset = 0;
            byte BitShift = (byte)(ShiftX ? 3 : 2);
            int Max1 = 0x00FF >> BitShift;
            int Max2 = 0xFFFF >> BitShift;
            for (int i = 0; i < Offsets.Count; i++)
            {
                Offset = Offsets[i];
                if (i > 0) { Offset -= Offsets[i - 1]; if (Offset == 0) continue; }

                Offset >>= BitShift;
                     if (Offset <= Max1) Length += 1;
                else if (Offset <= Max2) Length += 2;
                else                     Length += 4;
            }

            byte[] data = new byte[Length.Align(0x10)];
            byte* ptr = data.GetPtr();

            Value Val = 0;
            *(int*)ptr = Length; ptr += 4;
            for (int i = 0; i < Offsets.Count; i++)
            {
                Offset = Offsets[i];
                if (i > 0) { Offset -= Offsets[i - 1]; if (Offset == 0) continue; }

                Offset >>= BitShift;
                Val = Offset > Max2 ? Value.Int32 : Offset > Max1 ? Value.Int16 : Value.Int8;
                     if (Offset <= Max1)   *ptr = (byte)((byte)Val |  Offset       );
                else if (Offset <= Max2) { *ptr = (byte)((byte)Val | (Offset >>  8)); ptr++;
                                           *ptr = (byte)              Offset        ; }
                else                     { *ptr = (byte)((byte)Val | (Offset >> 24)); ptr++;
                                           *ptr = (byte)             (Offset >> 16) ; ptr++;
                                           *ptr = (byte)             (Offset >>  8) ; ptr++; 
                                           *ptr = (byte)              Offset        ; }
                ptr++;
            }
            return data;
        }

        public enum Value : byte
        {
            Invalid = 0b00000000,
            Int8    = 0b01000000,
            Int16   = 0b10000000,
            Int32   = 0b11000000,
        }
    }
}
