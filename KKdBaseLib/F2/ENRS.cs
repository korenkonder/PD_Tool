namespace KKdBaseLib.F2
{
    public unsafe struct ENRSList
    {
        public int ID;
        public bool EOFC;
        public KKdList<ENRS> List;

        public bool  IsNull => List. IsNull;
        public bool NotNull => List.NotNull;

        public static ENRSList Read(byte[] data, int ID = 0, bool EOFC = false)
        {
            byte* ptr = data.GetPtr();
            int i, i0;
            int ENRSCount = ((int*)ptr)[1];
            KKdList<ENRS> List = KKdList<ENRS>.New;
            ptr += 0x10;

            for (i = 0; i < ENRSCount; i++)
            {
                ENRS ENR;
                ENR.Offset = ReadENRSValue(ref ptr);
                ENR.Count  = ReadENRSValue(ref ptr);
                ENR.Size   = ReadENRSValue(ref ptr);
                ENR.Repeat = ReadENRSValue(ref ptr);

                if (i > 0) ENR.Offset += List[List.Count - 1].Offset;

                if (ENR.Repeat < 1) { ENR.Sub = null; List.Add(ENR); continue; }

                ENR.Sub = new KKdList<ENRS.SubENRS> { Capacity = ENR.Count };
                for (i0 = 0; i0 < ENR.Count; i0++)
                {
                    ENRS.SubENRS Sub = ENR.Sub[0];
                    Sub.Skip    = ReadENRSValue(ref ptr, out Sub.Type) + i0 > 0 ? ENR.Sub[i0 - 1].SizeSkip : 0;
                    Sub.Reverse = ReadENRSValue(ref ptr);
                    ENR.Sub.Add(Sub);

                    if (ENR.Sub[i0].Type == ENRS.Type.Invalid) return default;
                }
                List.Add(ENR);
            }
            return new ENRSList { EOFC = EOFC, ID = ID, List = List };
        }

        public static byte[] Write(ENRSList ENRS)
        {
            int i, i0;
            byte[] data;
            byte* ptr;

            KKdList<ENRS> List = (System.Collections.Generic.List<ENRS>)ENRS.List;
            if (ENRS.IsNull || ENRS.List.Count < 1) return new byte[0x20];

            int length = 0x10;
            for (i = 0; i < ENRS.List.Count; i++)
            {
                length += 0x10;
                ENRS ENR = ENRS.List[i];
                if (ENR.Repeat > 0 && ENR.Sub.NotNull)
                {
                    ENR.Count = ENR.Sub.Count;
                    length += 0x8 * ENR.Count;
                }
                else ENR.Repeat = 0;
                ENRS.List[i] = ENR;
            }
            data = new byte[length];
            ptr  = data.GetPtr();
            ((int*)ptr)[1] = ENRS.List.Count;
            ptr += 0x10;

            for (i = 0; i < ENRS.List.Count; i++)
            {
                ENRS ENR = ENRS.List[i];
                WriteENRSValue(ref ptr, i > 0 ? ENR.Offset - ENRS.List[i - 1].Offset : ENR.Offset);
                WriteENRSValue(ref ptr, ENR.Count );
                WriteENRSValue(ref ptr, ENR.Size  );
                WriteENRSValue(ref ptr, ENR.Repeat);

                if (ENR.Repeat < 1) continue;

                for (i0 = 0; i0 < ENR.Count; i0++)
                {
                    if (ENR.Sub[i0].Type < ENRSList.ENRS.Type. WORD ||
                        ENR.Sub[i0].Type > ENRSList.ENRS.Type.QWORD)
                        return GetFinalArray(data, ptr);

                    WriteENRSValue(ref ptr, i0 > 0 ? ENR.Sub[i0].Skip - ENR.Sub[i0 - 1].SizeSkip : ENR.Sub[i0].Skip, ENR.Sub[i0].Type);
                    WriteENRSValue(ref ptr, ENR.Sub[i0].Reverse);
                }
                
            }
            return GetFinalArray(data, ptr);
        }

        [System.ThreadStatic] private static ENRS.Value Value;

        private static byte[] GetFinalArray(byte[] data, byte* ptr)
        {
            byte* Ptr = data.GetPtr();
            long length = (long)ptr - (long)Ptr;
            byte[] tempdata = new byte[length.Align(0x10)];
            for (int i = 0; i < length; i++) tempdata[i] = data[i];
            data = null;
            return tempdata;
        }

        private static int ReadENRSValue(ref byte* ptr, out ENRS.Type Type)
        {
            int V = *ptr & 0xF;
             Type = (ENRS. Type)((*ptr & 0x30) >> 4);
            Value = (ENRS.Value)((*ptr & 0xC0) >> 6);
            ptr++;
                 if (Value == ENRS.Value.Int32  )
            { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; }
            else if (Value == ENRS.Value.Int16  )
            { V = (V <<  8) |  ptr[0];                                 ptr += 1; }
            else if (Value == ENRS.Value.Invalid) V = 0;
            return V;
        }

        private static int ReadENRSValue(ref byte* ptr)
        {
            int V = *ptr & 0x3F;
            Value = (ENRS.Value)((*ptr & 0xC0) >> 6);
            ptr++;
                 if (Value == ENRS.Value.Int32  )
            { V = (V << 24) | (ptr[0] << 16) | (ptr[1] << 8) | ptr[2]; ptr += 3; }
            else if (Value == ENRS.Value.Int16  )
            { V = (V <<  8) |  ptr[0];                                 ptr += 1; }
            else if (Value == ENRS.Value.Invalid) V = 0;
            return V;
        }

        private static void WriteENRSValue(ref byte* ptr, int Val, ENRS.Type Type)
        {
            Value = ENRS.Value.Invalid;
                 if (Val < 0x00000040) Value = ENRS.Value.Int8 ;
            else if (Val < 0x00004000) Value = ENRS.Value.Int16;
            else if (Val < 0x40000000) Value = ENRS.Value.Int32;
            *ptr = (byte)((((byte)Value << 6) & 0xC0) | (((byte)Type << 4) & 0x30));

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
            Value = ENRS.Value.Invalid;
                 if (Val < 0x00000040) Value = ENRS.Value.Int8 ;
            else if (Val < 0x00004000) Value = ENRS.Value.Int16;
            else if (Val < 0x40000000) Value = ENRS.Value.Int32;
            *ptr = (byte)(((byte)Value << 6) & 0xC0);

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

                public override string ToString() => "Skip: " + Skip + "; Reverse: " + Reverse + "; Type: " + Type;
            }

            public override string ToString() =>
                "Offset: " + Offset + "; Count: " + Count + "; " + "Size: " + Size + "; Repeat: " + Repeat;
        }

        public override string ToString() =>
            $"ID: {ID}{(NotNull ? $"; ENRS Count: {List.Count}" : "")}{(EOFC ? "; Has EOFC" : "")}";
    }
}
