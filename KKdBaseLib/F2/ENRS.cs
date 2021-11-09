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
                    if (ReadENRSValue(ref localPtr, out enrsEntry.Offset)) return;
                    if (ReadENRSValue(ref localPtr, out enrsEntry.Count )) return;
                    if (ReadENRSValue(ref localPtr, out enrsEntry.Size  )) return;
                    if (ReadENRSValue(ref localPtr, out enrsEntry.Repeat)) return;

                    if (enrsEntry.Repeat < 1) { enrsEntry.Sub = null; Array[i] = enrsEntry; continue; }

                    enrsEntry.Sub = new ENRSEntry.SubENRSEntry[enrsEntry.Count];
                    for (i0 = 0; i0 < enrsEntry.Count; i0++)
                    {
                        sub = default;
                        if (ReadENRSValue(ref localPtr, out sub.Skip, out sub.Type)) return;
                        if (ReadENRSValue(ref localPtr, out sub.Reverse           )) return;
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
                    if (WriteENRSValue(ref localPtr, enrsEntry.Offset)) goto End;
                    if (WriteENRSValue(ref localPtr, enrsEntry.Count )) goto End;
                    if (WriteENRSValue(ref localPtr, enrsEntry.Size  )) goto End;
                    if (WriteENRSValue(ref localPtr, enrsEntry.Repeat)) goto End;

                    if (enrsEntry.Repeat < 1) continue;

                    for (i0 = 0; i0 < enrsEntry.Count && i0 < enrsEntry.Sub.Length; i0++)
                    {
                        if (enrsEntry.Sub[i0].Type > ENRSEntry.Type.QWORD)
                            return data;

                        if (WriteENRSValue(ref localPtr, enrsEntry.Sub[i0].Skip, enrsEntry.Sub[i0].Type)) goto End;
                        if (WriteENRSValue(ref localPtr, enrsEntry.Sub[i0].Reverse                     )) goto End;
                    }
                }
            }
        End:
            return data;
        }

        private int length()
        {
            if (Array == null)
                return 0;

            int i, i0;
            int length = 0x10;
            for (i = 0; i < Array.Length; i++)
            {
                ENRSEntry enrs = Array[i];
                if (GetSize(enrs.Offset, ref length)) goto End;
                if (GetSize(enrs.Count , ref length)) goto End;
                if (GetSize(enrs.Size  , ref length)) goto End;
                if (GetSize(enrs.Repeat, ref length)) goto End;

                if (enrs.Repeat < 1) continue;

                for (i0 = 0; i0 < enrs.Count; i0++)
                {
                    if (enrs.Sub[i0].Type < ENRSEntry.Type. WORD ||
                        enrs.Sub[i0].Type > ENRSEntry.Type.QWORD) return length.A(0x10);

                    if (GetSizeType(enrs.Sub[i0].Skip   , ref length)) goto End;
                    if (GetSize    (enrs.Sub[i0].Reverse, ref length)) goto End;
                }
            }
        End:
            return length.A(0x10);

            static bool GetSizeType(int val, ref int length)
            {
                length += val <  0x00000010 ? 1 : val < 0x00001000 ? 2 : val < 0x10000000 ? 4 : 1;
                return    val >= 0x10000000;
            }

            static bool GetSize    (int val, ref int length)
            {
                length += val <  0x00000040 ? 1 : val < 0x00004000 ? 2 : val < 0x40000000 ? 4 : 1;
                return    val >= 0x40000000;
            }
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

        private unsafe static bool ReadENRSValue(ref byte* ptr, out int v, out ENRSEntry.Type type)
        {
            v = *ptr & 0xF;
             type = (ENRSEntry. Type)((*ptr & 0x30) >> 4);
            value = (ENRSEntry.Value)((*ptr & 0xC0) >> 6);
            ptr++;
                 if (value == ENRSEntry.Value.Int32  ) v = (((((v << 8) | *ptr++) << 8) | *ptr++) << 8) | *ptr++;
            else if (value == ENRSEntry.Value.Int16  ) v =     (v << 8) | *ptr++;
            else if (value == ENRSEntry.Value.Invalid) { v = 0; return true; }
            return false;
        }

        private unsafe static bool ReadENRSValue(ref byte* ptr, out int v)
        {
            v = *ptr & 0x3F;
            value = (ENRSEntry.Value)((*ptr & 0xC0) >> 6);
            ptr++;
                 if (value == ENRSEntry.Value.Int32  ) v = (((((v << 8) | *ptr++) << 8) | *ptr++) << 8) | *ptr++;
            else if (value == ENRSEntry.Value.Int16  ) v =     (v << 8) | *ptr++;
            else if (value == ENRSEntry.Value.Invalid) { v = 0; return true; }
            return false;
        }

        private unsafe static bool WriteENRSValue(ref byte* ptr, int val, ENRSEntry.Type type)
        {
            byte t = (byte)(((byte)type & 0x3) << 4);
            if (val < 0x00000010)
                *ptr++ = (byte)(((byte)ENRSEntry.Value.Int8  << 6) | t | ( val        & 0xF));
            else if (val < 0x00001000)
            {
                *ptr++ = (byte)(((byte)ENRSEntry.Value.Int16 << 6) | t | ((val >>  8) & 0xF));
                *ptr++ = (byte)(val & 0xFF);
            }
            else if (val < 0x10000000)
            {
                *ptr++ = (byte)(((byte)ENRSEntry.Value.Int32 << 6) | t | ((val >> 24) & 0xF));
                *ptr++ = (byte)((val >> 16) & 0xFF);
                *ptr++ = (byte)((val >>  8) & 0xFF);
                *ptr++ = (byte)( val        & 0xFF);
            }
            else
            {
                *ptr++ = (byte)ENRSEntry.Value.Invalid << 6;
                return true;
            }
            return false;
        }

        private unsafe static bool WriteENRSValue(ref byte* ptr, int val)
        {
            if (val < 0x00000040)
                *ptr++ = (byte)(((byte)ENRSEntry.Value.Int8  << 6) | ( val        & 0x3F));
            else if (val < 0x00004000)
            {
                *ptr++ = (byte)(((byte)ENRSEntry.Value.Int16 << 6) | ((val >>  8) & 0x3F));
                *ptr++ = (byte)(val & 0xFF);
            }
            else if (val < 0x40000000)
            {
                *ptr++ = (byte)(((byte)ENRSEntry.Value.Int32 << 6) | ((val >> 24) & 0x3F));
                *ptr++ = (byte)((val >> 16) & 0xFF);
                *ptr++ = (byte)((val >>  8) & 0xFF);
                *ptr++ = (byte)( val        & 0xFF);
            }
            else
            {
                *ptr++ = (byte)ENRSEntry.Value.Invalid << 6;
                return true;
            }
            return false;
        }

        public struct ENRSEntry
        {
            public int Offset;
            public int Count;
            public int Size;
            public int Repeat;
            public SubENRSEntry[] Sub;

            public ENRSEntry(int offset, int count, int size, int repeat)
            { Offset = offset; Count = count; Size = size; Repeat = repeat; Sub = new SubENRSEntry[count]; }

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

                public SubENRSEntry(int skip, int reverse, Type type)
                { Skip = skip; Reverse = reverse; Type = type; }

                public int SizeSkip => Skip + Reverse * (2 << (byte)Type);
                public int Size     =>        Reverse * (2 << (byte)Type);

                public override string ToString() =>
                    $"Skip: {Skip}; Reverse: {Reverse}; Type: {Type}";
            }

            public override string ToString() =>
                $"Offset: {Offset}; Count: {Count}; Size: {Size}; Repeat: {Repeat}";
        }

        public override string ToString() =>
            $"{(NotNull ? $"ENRS Count: {Array.Length}" : "No ENRS")}";
    }
}
