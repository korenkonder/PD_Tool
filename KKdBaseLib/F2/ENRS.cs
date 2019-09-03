namespace KKdBaseLib.F2
{
    public unsafe struct ENRS
    {
        public int Offset;
        public int Count;
        public int Size;
        public int Repeat;
        public SubENRS[] Sub;

        public static ENRS[] Read(byte[] data)
        {
            byte* ptr = data.GetPtr();
            int i, i0;
            int ENRSCount = ((int*)ptr)[1];
            ENRS[] ENRSArr = new ENRS[ENRSCount];
            ptr += 0x10;

            for (i = 0; i < ENRSCount; i++)
            {
                ref ENRS ENR = ref ENRSArr[i];
                ENR.Offset = ReadENRSValue(ref ptr);
                ENR.Count  = ReadENRSValue(ref ptr);
                ENR.Size   = ReadENRSValue(ref ptr);
                ENR.Repeat = ReadENRSValue(ref ptr);

                if (i > 0) ENR.Offset += ENRSArr[i - 1].Offset;

                if (ENR.Repeat > 0)
                {
                    ENR.Sub = new SubENRS[ENR.Count];
                    for (i0 = 0; i0 < ENR.Count; i0++)
                    {
                        ENR.Sub[i0].Skip    = ReadENRSValue(ref ptr, out ENR.Sub[i0].Type);
                        ENR.Sub[i0].Reverse = ReadENRSValue(ref ptr);

                        if (ENR.Sub[i0].Type == Type.Invalid) return null;

                        if (i0 > 0) ENR.Sub[i0].Skip += ENR.Sub[i0 - 1].Skip + ENR.Sub[i0 - 1].Reverse *
                                (2 << ((byte)ENR.Sub[i0].Type >> 16));
                    }
                }
                else ENR.Sub = null;
            }
            return ENRSArr;
        }

        public static byte[] Write(ENRS[] ENRSArr)
        {
            int i, i0;
            byte[] data;
            byte* ptr;

            if (ENRSArr == null || ENRSArr.Length < 1)
                return new byte[0x10];

            int length = 0x10;
            for (i = 0; i < ENRSArr.Length; i++)
            {
                length += 0x10;
                ref ENRS ENR = ref ENRSArr[i];
                if (ENR.Repeat > 0 && ENR.Sub != null)
                {
                    ENR.Count = ENR.Sub.Length;
                    length += 0x8 * ENR.Count;
                }
                else ENR.Repeat = 0;
            }
            data = new byte[length];
            ptr  = data.GetPtr();
            ((int*)ptr)[1] = ENRSArr.Length;
            ptr += 0x10;

            for (i = 0; i < ENRSArr.Length; i++)
            {
                ref ENRS ENR = ref ENRSArr[i];
                WriteENRSValue(ref ptr, i > 0 ? ENR.Offset - ENRSArr[i - 1].Offset : ENR.Offset);
                WriteENRSValue(ref ptr, ENR.Count );
                WriteENRSValue(ref ptr, ENR.Size  );
                WriteENRSValue(ref ptr, ENR.Repeat);

                if (ENR.Repeat > 0)
                    for (i0 = 0; i0 < ENR.Count; i0++)
                    {
                        if (ENR.Sub[i0].Type < Type.WORD || ENR.Sub[i0].Type > Type.QWORD)
                            return GetFinalArray(data, ptr);

                        WriteENRSValue(ref ptr, i0 > 0 ? ENR.Sub[i0].Skip - ENR.Sub[i0 - 1].Skip - ENR.Sub[i0 - 1].Reverse *
                            (2 << (((byte)ENR.Sub[i0].Type & 0x30) >> 16)) : ENR.Sub[i0].Skip, ENR.Sub[i0].Type);
                        WriteENRSValue(ref ptr, ENR.Sub[i0].Reverse);
                    }
            }
            return GetFinalArray(data, ptr);
        }

        private static byte[] GetFinalArray(byte[] data, byte* ptr)
        {
            byte* Ptr = data.GetPtr();
            long length = (long)ptr - (long)Ptr;
            byte[] tempdata = new byte[length.Align(0x10)];
            for (int i = 0; i < length; i++) tempdata[i] = data[i];
            data = null;
            return tempdata;
        }

        private static int ReadENRSValue(ref byte* ptr, out Type Type)
        {
            int V = *ptr & 0xF;
                   Type = ( Type)(*ptr & 0x30);
            Value Value = (Value)(*ptr & 0xC0);
            ptr++;
                 if (Value == Value.Int32  )
            { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; }
            else if (Value == Value.Int16  )
            { V = (V <<  8) |  ptr[0];                                 ptr += 1; }
            else if (Value == Value.Invalid) V = 0;
            return V;
        }

        private static int ReadENRSValue(ref byte* ptr)
        {
            int V = *ptr & 0x3F;
            Value Value = (Value)(*ptr & 0xC0);
            ptr++;
                 if (Value == Value.Int32  )
            { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; }
            else if (Value == Value.Int16  )
            { V = (V <<  8) |  ptr[0];                                 ptr += 1; }
            else if (Value == Value.Invalid) V = 0;
            return V;
        }

        private static void WriteENRSValue(ref byte* ptr, int Val, Type Type)
        {
            Value Value = Value.Invalid;
                 if (Val < 0x00000040) Value = Value.Int8 ;
            else if (Val < 0x00004000) Value = Value.Int16;
            else if (Val < 0x40000000) Value = Value.Int32;
            *ptr = (byte)Value;
            *ptr |= (byte)(((byte)Type) & 0x30);

                 if (Val < 0x00000010)
            { *ptr |= (byte)( Val        & 0x0F); }
            else if (Val < 0x00001000)
            { *ptr |= (byte)((Val >>  8) & 0x0F); ptr++;
              *ptr  = (byte)( Val        & 0xFF); }
            else if (Val < 0x10000000)
            { *ptr |= (byte)((Val >> 24) & 0x0F); ptr++;
              *ptr  = (byte)((Val >> 16) & 0xFF); ptr++;
              *ptr  = (byte)((Val >>  8) & 0xFF); ptr++;
              *ptr  = (byte)( Val        & 0xFF); }
            ptr++;
        }

        private static void WriteENRSValue(ref byte* ptr, int Val)
        {
            Value Value = Value.Invalid;
                 if (Val < 0x00000040) Value = Value.Int8 ;
            else if (Val < 0x00004000) Value = Value.Int16;
            else if (Val < 0x40000000) Value = Value.Int32;
            *ptr = (byte)Value;

                 if (Val < 0x00000040)
            { *ptr |= (byte)( Val        & 0x3F); }
            else if (Val < 0x00004000)
            { *ptr |= (byte)((Val >>  8) & 0x3F); ptr++;
              *ptr  = (byte)( Val        & 0xFF); }
            else if (Val < 0x40000000)
            { *ptr |= (byte)((Val >> 24) & 0x3F); ptr++;
              *ptr  = (byte)((Val >> 16) & 0xFF); ptr++;
              *ptr  = (byte)((Val >>  8) & 0xFF); ptr++;
              *ptr  = (byte)( Val        & 0xFF); }
            ptr++;
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
