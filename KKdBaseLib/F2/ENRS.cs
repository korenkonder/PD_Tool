namespace KKdBaseLib.F2
{
    public struct ENRS
    {
        public int Offset;
        public int Count;
        public int Size;
        public int Repeat;
        public SubENRS[] Sub;

        public static unsafe ENRS[] Read(byte[] data)
        {
            byte* ptr = data.GetPtr();
            int i, i0;
            int ENRSCount = ((int*)ptr)[1];
            ENRS[] ENRSArr = new ENRS[ENRSCount];
            ptr += 0x10;

            ENRS ENR = new ENRS();
            for (i = 0; i < ENRSCount; i++)
            {
                ENR.Offset = ReadENRSValue(ref ptr) + ENR.Offset;
                ENR.Count  = ReadENRSValue(ref ptr);
                ENR.Size   = ReadENRSValue(ref ptr);
                ENR.Repeat = ReadENRSValue(ref ptr);

                if (ENR.Repeat > 0)
                {
                    ENR.Sub = new SubENRS[ENR.Count];
                    for (i0 = 0; i0 < ENR.Count; i0++)
                    {
                        ENR.Sub[i0].Skip    = ReadENRSValue(ref ptr, out ENR.Sub[i0].Type);
                        ENR.Sub[i0].Reverse = ReadENRSValue(ref ptr);

                        if (ENR.Sub[i0].Type == Type.Invalid) return null;

                        if (i0 > 0) ENR.Sub[i0].Skip += ENR.Sub[i0 - 1].Skip + ENR.Sub[i0 - 1].Reverse *
                                ((ENR.Sub[i0].Type == Type.WORD) ? 2 : (ENR.Sub[i0].Type == Type.DWORD) ? 4 : 8);
                    }
                }
                else ENR.Sub = null;
                ENRSArr[i] = ENR;
            }
            return ENRSArr;
        }

        private static unsafe int ReadENRSValue(ref byte* ptr, out Type Rev)
        {
            int V = *ptr & 0xF;
            Rev = (Type)(*ptr & 0x30);
            Value Val = (Value)(*ptr & 0xC0);
            ptr++;
                 if (Val == Value.Int32  )
            { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; }
            else if (Val == Value.Int16  )
            { V = (V <<  8) |  ptr[0];                                 ptr += 1; }
            else if (Val == Value.Invalid) V = 0;
            return V;
        }

        private static unsafe int ReadENRSValue(ref byte* ptr)
        {
            int V = *ptr & 0x3F;
            Value Val = (Value)(*ptr & 0xC0);
            ptr++;
                 if (Val == Value.Int32  )
            { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; }
            else if (Val == Value.Int16  )
            { V = (V <<  8) |  ptr[0];                                 ptr += 1; }
            else if (Val == Value.Invalid) V = 0;
            return V;
        }

        public enum Type : byte
        {
             WORD   = 0b00000000,
            DWORD   = 0b00010000,
            QWORD   = 0b00100000,
            Invalid = 0b00110000,
        }

        public enum Value : byte
        {
            Int8    = 0b00000000,
            Int16   = 0b01000000,
            Int32   = 0b10000000,
            Invalid = 0b11000000,
        }

        public struct SubENRS
        {
            public int Skip;
            public int Reverse;
            public Type Type;

            public override string ToString() => "Skip: " + Skip + "; Reverse: " + Reverse + "; Type: " + Type;
        }

        public override string ToString() =>
            "Offset: " + Offset + "; Count: " + Count + "; " + "Size: " + Size + "; Repeat: " + Repeat;
    }
}
