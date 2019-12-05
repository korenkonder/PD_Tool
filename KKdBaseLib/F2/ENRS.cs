namespace KKdBaseLib.F2
{
    public struct ENRS : INull
    {
        public ENRSEntry[] Array;

        public int Length => length();

        public bool  IsNull => Array == null;
        public bool NotNull => Array != null;

        public unsafe void Read(byte[] data)
        {
            if (data == null || data.Length < 0x10) return;
            fixed (byte* ptr = data)
            {
                int i, i0;
                int ENRSCount = ((int*)ptr)[1];
                ENRSEntry enrsEntry;
                ENRSEntry.SubENRSEntry sub;
                Array = new ENRSEntry[ENRSCount];

                byte* localPtr = ptr + 0x10;
                for (i = 0; i < ENRSCount; i++)
                {
                    enrsEntry = default;
                    enrsEntry.Offset = ReadENRSValue(ref localPtr);
                    enrsEntry.Count  = ReadENRSValue(ref localPtr);
                    enrsEntry.Size   = ReadENRSValue(ref localPtr);
                    enrsEntry.Repeat = ReadENRSValue(ref localPtr);

                    if (i > 0) enrsEntry.Offset += Array[i - 1].Offset;

                    if (enrsEntry.Repeat < 1) { enrsEntry.Sub = null; Array[i] = enrsEntry; continue; }

                    enrsEntry.Sub = new ENRSEntry.SubENRSEntry[enrsEntry.Count];
                    for (i0 = 0; i0 < enrsEntry.Count; i0++)
                    {
                        sub = default;
                        sub.Skip    = ReadENRSValue(ref localPtr, out sub.Type);
                        sub.Reverse = ReadENRSValue(ref localPtr);
                        if (i0 > 0) sub.Skip += enrsEntry.Sub[i0 - 1].SizeSkip;
                        enrsEntry.Sub[i0] = sub;

                        if (enrsEntry.Sub[i0].Type == ENRSEntry.Type.Invalid) { Array = null; return; }
                    }
                    Array[i] = enrsEntry;
                }
            }
        }

        public unsafe byte[] Write()
        {
            int i, i0;
            byte[] data;

            if (IsNull || Array.Length < 1) return new byte[0x20];

            data = new byte[Length];
            fixed (byte* ptr = data)
            {
                ((int*)ptr)[1] = Array.Length;

                byte* localPtr = ptr + 0x10;
                for (i = 0; i < Array.Length; i++)
                {
                    ENRSEntry enrsEntry = Array[i];
                    WriteENRSValue(ref localPtr, i > 0 ? enrsEntry.Offset -
                        Array[i - 1].Offset : enrsEntry.Offset);
                    WriteENRSValue(ref localPtr, enrsEntry.Count > enrsEntry.Sub.Length ?
                        enrsEntry.Sub.Length : enrsEntry.Count);
                    WriteENRSValue(ref localPtr, enrsEntry.Size  );
                    WriteENRSValue(ref localPtr, enrsEntry.Repeat);

                    if (enrsEntry.Repeat < 1) continue;

                    for (i0 = 0; i0 < enrsEntry.Count && i0 < enrsEntry.Sub.Length; i0++)
                    {
                        if (enrsEntry.Sub[i0].Type < ENRSEntry.Type. WORD ||
                            enrsEntry.Sub[i0].Type > ENRSEntry.Type.QWORD)
                            return data;

                        WriteENRSValue(ref localPtr, i0 > 0 ? enrsEntry.Sub[i0].Skip -
                            enrsEntry.Sub[i0 - 1].SizeSkip : enrsEntry.Sub[i0].Skip, enrsEntry.Sub[i0].Type);
                        WriteENRSValue(ref localPtr, enrsEntry.Sub[i0].Reverse);
                    }
                }
            }
            return data;
        }

        private int length()
        {
            int i, i0;
            int length = 0x10;
            for (i = 0; i < Array.Length; i++)
            {
                ENRSEntry enrs = Array[i];
                length += GetSize(i > 0 ? enrs.Offset - Array[i - 1].Offset : enrs.Offset);
                length += GetSize(enrs.Count );
                length += GetSize(enrs.Size  );
                length += GetSize(enrs.Repeat);

                if (enrs.Repeat < 1) continue;

                for (i0 = 0; i0 < enrs.Count; i0++)
                {
                    if (enrs.Sub[i0].Type < ENRSEntry.Type. WORD ||
                        enrs.Sub[i0].Type > ENRSEntry.Type.QWORD) return length.A(0x10);

                    length += GetSizeType(i0 > 0 ? enrs.Sub[i0].Skip -
                        enrs.Sub[i0 - 1].SizeSkip : enrs.Sub[i0].Skip);
                    length += GetSize(enrs.Sub[i0].Reverse);
                }
            }
            return length.A(0x10);

            static int GetSizeType(int val) => val < 0x00000010 ? 1 : val < 0x00001000 ? 2 : 4;
            static int GetSize    (int val) => val < 0x00000040 ? 1 : val < 0x00004000 ? 2 : 4;
        }

        /*private static KKdList<ENRS.SubENRS> Optimize(KKdList<ENRS.SubENRS> Sub)
        {
            if (Sub.IsNull || Sub.Count < 2) return Sub;

            for (int i = 1; i < Sub.Capacity; i++)
                if (Sub[i - 1].Skip == Sub[i].Skip - Sub[i - 1].Size && Sub[i - 1].Type == Sub[i].Type)
                {
                    ENRS.SubENRS SubENRS = Sub[i - 1];
                    SubENRS.Reverse++;
                    Sub[i - 1] = SubENRS;
                    Sub.RemoveAt(i);
                    Sub.Capacity--;
                    i--;
                }
            return Sub;
        }*/

        [System.ThreadStatic] private static ENRSEntry.Value value;

        private unsafe static int ReadENRSValue(ref byte* ptr, out ENRSEntry.Type type)
        {
            int V = *ptr & 0xF;
             type = (ENRSEntry. Type)((*ptr & 0x30) >> 4);
            value = (ENRSEntry.Value)((*ptr & 0xC0) >> 6);
            ptr++;
                 if (value == ENRSEntry.Value.Int32  )
            { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; }
            else if (value == ENRSEntry.Value.Int16  )
            { V = (V <<  8) |  ptr[0];                                 ptr += 1; }
            else if (value == ENRSEntry.Value.Invalid) V = 0;
            return V;
        }

        private unsafe static int ReadENRSValue(ref byte* ptr)
        {
            int V = *ptr & 0x3F;
            value = (ENRSEntry.Value)((*ptr & 0xC0) >> 6);
            ptr++;
                 if (value == ENRSEntry.Value.Int32  )
            { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; }
            else if (value == ENRSEntry.Value.Int16  )
            { V = (V <<  8) |  ptr[0];                                 ptr += 1; }
            else if (value == ENRSEntry.Value.Invalid) V = 0;
            return V;
        }

        private unsafe static void WriteENRSValue(ref byte* ptr, int val, ENRSEntry.Type type)
        {
            value = ENRSEntry.Value.Invalid;
                 if (val < 0x00000040) value = ENRSEntry.Value.Int8 ;
            else if (val < 0x00004000) value = ENRSEntry.Value.Int16;
            else if (val < 0x40000000) value = ENRSEntry.Value.Int32;
            *ptr = (byte)((((byte)value << 6) & 0xC0) | (((byte)type << 4) & 0x30));

                 if (val < 0x00000010)
            { *ptr |= (byte)( val        & 0x0F); }
            else if (val < 0x00001000)
            { *ptr |= (byte)((val >>  8) & 0x0F); ptr++;
              *ptr  = (byte)( val        & 0xFF); }
            else if (val < 0x10000000)
            { *ptr |= (byte)((val >> 24) & 0x0F); ptr++;
              *ptr  = (byte)((val >> 16) & 0xFF); ptr++;
              *ptr  = (byte)((val >>  8) & 0xFF); ptr++;
              *ptr  = (byte)( val        & 0xFF); }
            ptr++;
        }

        private unsafe static void WriteENRSValue(ref byte* ptr, int val)
        {
            value = ENRSEntry.Value.Invalid;
                 if (val < 0x00000040) value = ENRSEntry.Value.Int8 ;
            else if (val < 0x00004000) value = ENRSEntry.Value.Int16;
            else if (val < 0x40000000) value = ENRSEntry.Value.Int32;
            *ptr = (byte)(((byte)value << 6) & 0xC0);

                 if (val < 0x00000040)
            { *ptr |= (byte)( val        & 0x3F); }
            else if (val < 0x00004000)
            { *ptr |= (byte)((val >>  8) & 0x3F); ptr++;
              *ptr  = (byte)( val        & 0xFF); }
            else if (val < 0x40000000)
            { *ptr |= (byte)((val >> 24) & 0x3F); ptr++;
              *ptr  = (byte)((val >> 16) & 0xFF); ptr++;
              *ptr  = (byte)((val >>  8) & 0xFF); ptr++;
              *ptr  = (byte)( val        & 0xFF); }
            ptr++;
        }

        public struct ENRSEntry
        {
            public int Offset;
            public int Count;
            public int Size;
            public int Repeat;
            public SubENRSEntry[] Sub;

            public enum Type : byte
            {
                 WORD   = 0b00,
                DWORD   = 0b01,
                QWORD   = 0b10,
                Invalid = 0b11,
            }

            public enum Value : byte
            {
                Int8    = 0b00,
                Int16   = 0b01,
                Int32   = 0b10,
                Invalid = 0b11,
            }

            public struct SubENRSEntry
            {
                public int Skip;
                public int Reverse;
                public Type Type;

                public int SizeSkip => Skip + Reverse * (2 << (byte)Type);
                public int Size     =>        Reverse * (2 << (byte)Type);

                public override string ToString() => "Skip: " + Skip +
                    "; Reverse: " + Reverse + "; Type: " + Type;
            }

            public override string ToString() =>
                "Offset: " + Offset + "; Count: " + Count + "; " +
                "Size: " + Size + "; Repeat: " + Repeat;
        }

        public override string ToString() =>
            $"{(NotNull ? $"ENRS Count: {Array.Length}" : "No ENRS")}";
    }
}
