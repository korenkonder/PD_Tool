using System;
using KKdMainLib.IO;
using KKdMainLib.Types;

namespace KKdMainLib.MessagePack
{
    public class IO
    {
        public Stream _IO;

        public IO(         ) => _IO = File.OpenWriter();
        public IO(Stream IO) => _IO = IO;

        public void Close() => _IO.Close();

        public MsgPack Read(bool Array = false)
        {
            MsgPack MsgPack = MsgPack.New;
            byte Unk = _IO.ReadByte();
            Types Type = (Types)Unk;
            if (!Array)
            {
                MsgPack.Name = ReadString(Type);
                if (MsgPack.Name != null) { Unk = _IO.ReadByte(); Type = (Types)Unk; }
            }

            bool FixArr = Type >= Types.FixArr && Type <= Types.FixArrMax;
            bool FixMap = Type >= Types.FixMap && Type <= Types.FixMapMax;
            bool FixStr = Type >= Types.FixStr && Type <= Types.FixStrMax;
            bool PosInt = Type >= Types.PosInt && Type <= Types.PosIntMax;
            bool NegInt = Type >= Types.NegInt && Type <= Types.NegIntMax;
            if (FixArr || FixMap || FixStr || PosInt || NegInt)
            {
                if (FixMap)
                {
                    MsgPack.Object = KKdList<object>.New;
                    for (int i = 0; i < Unk - (byte)Types.FixMap; i++) MsgPack.Add( Read(false));
                }
                else if (FixArr)
                {
                    MsgPack.Object = new object[Unk - (byte)Types.FixArr];
                    for (int i = 0; i < Unk - (byte)Types.FixArr; i++) MsgPack[i] = Read( true);
                }
                else if (FixStr) MsgPack.Object = ReadString( Type);
                else if (PosInt) MsgPack.Object =        Unk; 
                else if (NegInt) MsgPack.Object = (sbyte)Unk;
                return MsgPack;
            }
            
            while (true)
            {
                if (ReadNil    (ref MsgPack, ref Type)) break;
                if (ReadArr    (ref MsgPack, ref Type)) break;
                if (ReadMap    (ref MsgPack, ref Type)) break;
                if (ReadExt    (ref MsgPack, ref Type)) break;
                if (ReadString (ref MsgPack, ref Type)) break;
                if (ReadBoolean(ref MsgPack, ref Type)) break;
                if (ReadBytes  (ref MsgPack, ref Type)) break;
                if (ReadInt    (ref MsgPack, ref Type)) break;
                if (ReadUInt   (ref MsgPack, ref Type)) break;
                if (ReadFloat  (ref MsgPack, ref Type)) break;
                break;
            }
            return MsgPack;
        }

        private bool ReadInt    (ref MsgPack MsgPack, ref Types Type)
        {
                 if (Type == Types.Int8 ) MsgPack.Object = _IO.ReadSByte();
            else if (Type == Types.Int16) MsgPack.Object = _IO.ReadInt16Endian(true);
            else if (Type == Types.Int32) MsgPack.Object = _IO.ReadInt32Endian(true);
            else if (Type == Types.Int64) MsgPack.Object = _IO.ReadInt64Endian(true);
            else return false;
            return true;
        }

        private bool ReadUInt   (ref MsgPack MsgPack, ref Types Type)
        {
                 if (Type == Types.UInt8 ) MsgPack.Object = _IO.ReadByte();
            else if (Type == Types.UInt16) MsgPack.Object = _IO.ReadUInt16Endian(true);
            else if (Type == Types.UInt32) MsgPack.Object = _IO.ReadUInt32Endian(true);
            else if (Type == Types.UInt64) MsgPack.Object = _IO.ReadUInt64Endian(true);
            else return false;
            return true;
        }

        private bool ReadFloat  (ref MsgPack MsgPack, ref Types Type)
        {
                 if (Type == Types.Float32) MsgPack.Object = _IO.ReadSingleEndian(true);
            else if (Type == Types.Float64) MsgPack.Object = _IO.ReadDoubleEndian(true);
            else return false;
            return true;
        }

        private bool ReadBoolean(ref MsgPack MsgPack, ref Types Type)
        {
                 if (Type == Types.False) MsgPack.Object = false;
            else if (Type == Types.True ) MsgPack.Object = true ;
            else return false;
            return true;
        }

        private bool ReadBytes  (ref MsgPack MsgPack, ref Types Type)
        {
            int Length = 0;
                 if (Type == Types.Bin8 ) Length = _IO.ReadByte();
            else if (Type == Types.Bin16) Length = _IO.ReadInt16Endian(true);
            else if (Type == Types.Bin32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = _IO.ReadBytes(Length);
            return true;
        }
        
        private bool ReadString  (ref MsgPack MsgPack, ref Types Type)
        {
            string val = ReadString(Type);
            if (val != null) MsgPack.Object = val;
            else return false;
            return true;
        }

        private string ReadString(Types Val)
        {
                 if (Val >= Types.FixStr  && Val <= Types.FixStrMax)
                return _IO.ReadString(Val - Types.FixStr);
            else if (Val >= Types.   Str8 && Val <= Types.   Str32 )
            {
                Enum.TryParse(Val.ToString(), out Types Type);
                int Length = 0;
                     if (Type == Types.Str8 ) Length = _IO.ReadByte();
                else if (Type == Types.Str16) Length = _IO.ReadInt16Endian(true);
                else                                  Length = _IO.ReadInt32Endian(true);
                return _IO.ReadString(Length);
            }
            return null;
        }

        private bool ReadNil(ref MsgPack MsgPack, ref Types Type)
        {
            if (Type == Types.Nil) MsgPack.Object = null;
            else return false;
            return true;
        }

        private bool ReadArr(ref MsgPack MsgPack, ref Types Type)
        {
            int Length = 0;
                 if (Type == Types.Arr16) Length = _IO.ReadInt16Endian(true);
            else if (Type == Types.Arr32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = new object[Length];
            for (int i = 0; i < Length; i++) MsgPack[i] = Read(true);
            return true;
        }

        private bool ReadMap(ref MsgPack MsgPack, ref Types Type)
        {
            int Length = 0;
                 if (Type == Types.Map16) Length = _IO.ReadInt16Endian(true);
            else if (Type == Types.Map32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = KKdList<object>.New;
            for (int i = 0; i < Length; i++) MsgPack.Add(Read());
            return true;
        }

        private bool ReadExt(ref MsgPack MsgPack, ref Types Type)
        {
            int Length = 0;
                 if (Type == Types.FixExt1 ) Length = 1 ; 
            else if (Type == Types.FixExt2 ) Length = 2 ;
            else if (Type == Types.FixExt4 ) Length = 4 ;
            else if (Type == Types.FixExt8 ) Length = 8 ;
            else if (Type == Types.FixExt16) Length = 16;
            else if (Type == Types.   Ext8 ) Length = _IO.ReadByte();
            else if (Type == Types.   Ext16) Length = _IO.ReadInt16Endian(true);
            else if (Type == Types.   Ext32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = new MsgPack.Ext { Type = _IO.ReadSByte(), Data = _IO.ReadBytes(Length) };
            return true;
        }

        public IO Write(MsgPack MsgPack, bool Close)
        { Write(MsgPack); if (Close) this.Close(); return this; }
        
        public IO Write(MsgPack MsgPack)
        {
            if (MsgPack.Name != null) Write(MsgPack.Name);
            Write(MsgPack.Object);
            return this;
        }

        private void Write(object obj)
        {
            if (obj == null) { WriteNil(); return; }
            switch (obj)
            {
                case KKdList<object>  val: WriteMap(val.Count );
                    for (int i = 0; i < val.Count ; i++) Write(val[i]); break;
                case object[] val: WriteArr(val.Length);
                    for (int i = 0; i < val.Length; i++) Write(val[i]); break;
                case     MsgPack val: Write(val); break;
                case      byte[] val: Write(val); break;
                case        bool val: Write(val); break;
                case       sbyte val: Write(val); break;
                case        byte val: Write(val); break;
                case       short val: Write(val); break;
                case      ushort val: Write(val); break;
                case         int val: Write(val); break;
                case        uint val: Write(val); break;
                case        long val: Write(val); break;
                case       ulong val: Write(val); break;
                case       float val: Write(val); break;
                case      double val: Write(val); break;
                case      string val: Write(val); break;
                case MsgPack.Ext val: Write(val); break;
            }
        }

        private void Write( sbyte val) { if (val < -0x20) _IO.WriteByte(0xD0); _IO.Write(val); }
        private void Write(  byte val) { if (val >= 0x80) _IO.WriteByte(0xCC); _IO.Write(val); }
        private void Write( short val) { if (( sbyte)val == val) Write(( sbyte)val);
                                    else if ((  byte)val == val) Write((  byte)val);
                                    else { _IO.WriteByte(0xD1); _IO.WriteEndian(val, true); } }
        private void Write(ushort val) { if ((  byte)val == val) Write((  byte)val);
                                    else { _IO.WriteByte(0xCD); _IO.WriteEndian(val, true); } }
        private void Write(   int val) { if (( short)val == val) Write(( short)val);
                                    else if ((ushort)val == val) Write((ushort)val);
                                    else { _IO.WriteByte(0xD2); _IO.WriteEndian(val, true); } }
        private void Write(  uint val) { if ((ushort)val == val) Write((ushort)val);
                                    else { _IO.WriteByte(0xCE); _IO.WriteEndian(val, true); } }
        private void Write(  long val) { if ((   int)val == val) Write((   int)val);
                                    else if ((  uint)val == val) Write((  uint)val);
                                    else { _IO.WriteByte(0xD3); _IO.WriteEndian(val, true); } }
        private void Write( ulong val) { if ((  uint)val == val) Write((  uint)val);
                                    else { _IO.WriteByte(0xCF); _IO.WriteEndian(val, true); } }
        private void Write( float val) { if ((  long)val == val) Write((  long)val);
                                    else { _IO.WriteByte(0xCA); _IO.WriteEndian(val, true); } }
        private void Write(double val) { if ((  long)val == val) Write((  long)val);
                                    else if (( float)val == val) Write(( float)val);
                                    else { _IO.WriteByte(0xCB); _IO.WriteEndian(val, true); } }

        private void Write(  bool val) =>
            _IO.WriteByte((byte)(val ? 0xC3 : 0xC2));

        private void Write(byte[] val)
        {
            if (val == null) { WriteNil(); return; }

                 if (val.Length <   0x100)
            { _IO.WriteByte(0xC4); _IO.WriteByte  ((  byte)val.Length      ); }
            else if (val.Length < 0x10000)
            { _IO.WriteByte(0xC5); _IO.WriteEndian((ushort)val.Length, true); }
            else
            { _IO.WriteByte(0xC6); _IO.WriteEndian(        val.Length, true); }
            _IO.Write(val);
        }
        
        private void Write(string val)
        {
            if (val == null) { WriteNil(); return; }

            byte[] array = Text.ToUTF8(val);
                 if (array.Length <    0x20)
              _IO.WriteByte((byte)(0xA0 | (array.Length & 0x1F)));
            else if (array.Length <   0x100)
            { _IO.WriteByte(0xD9); _IO.WriteByte  ((  byte)array.Length); }
            else if (array.Length < 0x10000)
            { _IO.WriteByte(0xDA); _IO.WriteEndian((ushort)array.Length, true); }
            else
            { _IO.WriteByte(0xDB); _IO.WriteEndian(        array.Length, true); }
            _IO.Write(array);
        }

        private void WriteNil() => _IO.WriteByte(0xC0);

        private void WriteArr(int val)
        {
                 if (val ==      0) { WriteNil(); return; }
            else if (val <    0x10)   _IO.WriteByte((byte)(0x90 | (val & 0x0F)));
            else if (val < 0x10000) { _IO.WriteByte(0xDC); _IO.WriteEndian((ushort)val, true); }
            else                    { _IO.WriteByte(0xDD); _IO.WriteEndian(        val, true); }
        }
                
        private void WriteMap(int val)
        {
                 if (val ==      0) { WriteNil(); return; }
            else if (val <    0x10)   _IO.WriteByte((byte)(0x80 | (val & 0x0F)));
            else if (val < 0x10000) { _IO.WriteByte(0xDE); _IO.WriteEndian((ushort)val, true); }
            else                    { _IO.WriteByte(0xDF); _IO.WriteEndian(        val, true); }
        }

        private void Write(MsgPack.Ext val)
        {
            if (val.Data == null) { WriteNil(); return; }

                 if (val.Data.Length <  1 ) { WriteNil(); return; }
            else if (val.Data.Length == 1 ) _IO.WriteByte(0xD4);
            else if (val.Data.Length == 2 ) _IO.WriteByte(0xD5);
            else if (val.Data.Length == 4 ) _IO.WriteByte(0xD6);
            else if (val.Data.Length == 8 ) _IO.WriteByte(0xD7);
            else if (val.Data.Length == 16) _IO.WriteByte(0xD8);
            else
            {
                     if (val.Data.Length <   0x100)
                { _IO.WriteByte(0xC7); _IO.WriteByte  ((  byte)val.Data.Length); }
                else if (val.Data.Length < 0x10000)
                { _IO.WriteByte(0xC8); _IO.WriteEndian((ushort)val.Data.Length, true); }
                else
                { _IO.WriteByte(0xC9); _IO.WriteEndian(        val.Data.Length, true); }
            }
            _IO.Write(val.Type);
            _IO.Write(val.Data);
        }
    }

    public enum Types : byte
    {
        PosInt    = 0b00000000,
        FixMap    = 0b10000000,
        FixArr    = 0b10010000,
        FixStr    = 0b10100000,
        Nil       = 0b11000000,
        NeverUsed = 0b11000001,
        False     = 0b11000010,
        True      = 0b11000011,
        Bin8      = 0b11000100,
        Bin16     = 0b11000101,
        Bin32     = 0b11000110,
        Ext8      = 0b11000111,
        Ext16     = 0b11001000,
        Ext32     = 0b11001001,
        Float32   = 0b11001010,
        Float64   = 0b11001011,
        UInt8     = 0b11001100,
        UInt16    = 0b11001101,
        UInt32    = 0b11001110,
        UInt64    = 0b11001111,
        Int8      = 0b11010000,
        Int16     = 0b11010001,
        Int32     = 0b11010010,
        Int64     = 0b11010011,
        FixExt1   = 0b11010100,
        FixExt2   = 0b11010101,
        FixExt4   = 0b11010110,
        FixExt8   = 0b11010111,
        FixExt16  = 0b11011000,
        Str8      = 0b11011001,
        Str16     = 0b11011010,
        Str32     = 0b11011011,
        Arr16     = 0b11011100,
        Arr32     = 0b11011101,
        Map16     = 0b11011110,
        Map32     = 0b11011111,
        NegInt    = 0b11100000,
        PosIntMax = 0b01111111,
        FixMapMax = 0b10001111,
        FixArrMax = 0b10011111,
        FixStrMax = 0b10111111,
        NegIntMax = 0b11111111,
    }
}
