namespace KKdBaseLib.F2
{
    public struct ENRSList : INull
    {
        public KKdList<ENRS> List;

        public int Length => length();

        public bool  IsNull => List. IsNull;
        public bool NotNull => List.NotNull;

        public unsafe static ENRSList Read(byte[] data)
        {
            if (data == null || data.Length < 0x10) return default;
            byte* ptr = data.GetPtr();
            int i, i0;
            int ENRSCount = ((int*)ptr)[1];
            KKdList<ENRS> list = KKdList<ENRS>.New;
            ptr += 0x10;
            ENRS enrs;
            ENRS.SubENRS sub;

            for (i = 0; i < ENRSCount; i++)
            {
                enrs = default;
                enrs.Offset = ReadENRSValue(ref ptr);
                enrs.Count  = ReadENRSValue(ref ptr);
                enrs.Size   = ReadENRSValue(ref ptr);
                enrs.Repeat = ReadENRSValue(ref ptr);

                if (i > 0) enrs.Offset += list[list.Count - 1].Offset;

                if (enrs.Repeat < 1) { enrs.Sub = null; list.Add(enrs); continue; }

                enrs.Sub = new KKdList<ENRS.SubENRS> { Capacity = enrs.Count };
                for (i0 = 0; i0 < enrs.Count; i0++)
                {
                    sub = default;
                    sub.Skip    = ReadENRSValue(ref ptr, out sub.Type);
                    sub.Reverse = ReadENRSValue(ref ptr);
                    if (i0 > 0) sub.Skip += enrs.Sub[i0 - 1].SizeSkip;
                    enrs.Sub.Add(sub);

                    if (enrs.Sub[i0].Type == ENRS.Type.Invalid) return default;
                }
                list.Add(enrs);
            }
            return new ENRSList { List = list };
        }

        public unsafe static byte[] Write(ENRSList enrsList)
        {
            int i, i0;
            byte[] data;
            byte* ptr;

            KKdList<ENRS> list = (System.Collections.Generic.List<ENRS>)enrsList.List;
            if (enrsList.IsNull || enrsList.List.Count < 1) return new byte[0x20];

            data = new byte[enrsList.Length];
            ptr  = data.GetPtr();
            ((int*)ptr)[1] = enrsList.List.Count;
            ptr += 0x10;

            for (i = 0; i < enrsList.List.Count; i++)
            {
                ENRS enrs = enrsList.List[i];
                WriteENRSValue(ref ptr, i > 0 ? enrs.Offset - enrsList.List[i - 1].Offset : enrs.Offset);
                WriteENRSValue(ref ptr, enrs.Count );
                WriteENRSValue(ref ptr, enrs.Size  );
                WriteENRSValue(ref ptr, enrs.Repeat);

                if (enrs.Repeat < 1) continue;

                for (i0 = 0; i0 < enrs.Count; i0++)
                {
                    if (enrs.Sub[i0].Type < ENRS.Type. WORD ||
                        enrs.Sub[i0].Type > ENRS.Type.QWORD)
                        return data;

                    WriteENRSValue(ref ptr, i0 > 0 ? enrs.Sub[i0].Skip -
                        enrs.Sub[i0 - 1].SizeSkip : enrs.Sub[i0].Skip, enrs.Sub[i0].Type);
                    WriteENRSValue(ref ptr, enrs.Sub[i0].Reverse);
                }
                
            }
            return data;
        }

        private int length()
        {
            int i, i0;
            int length = 0x10;
            for (i = 0; i < List.Count; i++)
            {
                ENRS enrs = List[i];
                length += GetSize(i > 0 ? enrs.Offset - List[i - 1].Offset : enrs.Offset);
                length += GetSize(enrs.Count );
                length += GetSize(enrs.Size  );
                length += GetSize(enrs.Repeat);

                if (enrs.Repeat < 1) continue;

                for (i0 = 0; i0 < enrs.Count; i0++)
                {
                    if (enrs.Sub[i0].Type < ENRS.Type. WORD ||
                        enrs.Sub[i0].Type > ENRS.Type.QWORD) return length.A(0x10);

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

        [System.ThreadStatic] private static ENRS.Value value;

        private unsafe static int ReadENRSValue(ref byte* ptr, out ENRS.Type type)
        {
            int V = *ptr & 0xF;
             type = (ENRS. Type)((*ptr & 0x30) >> 4);
            value = (ENRS.Value)((*ptr & 0xC0) >> 6);
            ptr++;
                 if (value == ENRS.Value.Int32  )
            { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; }
            else if (value == ENRS.Value.Int16  )
            { V = (V <<  8) |  ptr[0];                                 ptr += 1; }
            else if (value == ENRS.Value.Invalid) V = 0;
            return V;
        }

        private unsafe static int ReadENRSValue(ref byte* ptr)
        {
            int V = *ptr & 0x3F;
            value = (ENRS.Value)((*ptr & 0xC0) >> 6);
            ptr++;
                 if (value == ENRS.Value.Int32  )
            { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; }
            else if (value == ENRS.Value.Int16  )
            { V = (V <<  8) |  ptr[0];                                 ptr += 1; }
            else if (value == ENRS.Value.Invalid) V = 0;
            return V;
        }

        private unsafe static void WriteENRSValue(ref byte* ptr, int val, ENRS.Type type)
        {
            value = ENRS.Value.Invalid;
                 if (val < 0x00000040) value = ENRS.Value.Int8 ;
            else if (val < 0x00004000) value = ENRS.Value.Int16;
            else if (val < 0x40000000) value = ENRS.Value.Int32;
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
            value = ENRS.Value.Invalid;
                 if (val < 0x00000040) value = ENRS.Value.Int8 ;
            else if (val < 0x00004000) value = ENRS.Value.Int16;
            else if (val < 0x40000000) value = ENRS.Value.Int32;
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

        public struct ENRS
        {
            public int Offset;
            public int Count;
            public int Size;
            public int Repeat;
            public KKdList<SubENRS> Sub;

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

            public struct SubENRS
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
            $"{(NotNull ? $"ENRS Count: {List.Count}" : "No ENRS")}";
    }
}
