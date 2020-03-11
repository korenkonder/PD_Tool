using KKdBaseLib;

namespace KKdMainLib.IO
{
    public struct MP : System.IDisposable
    {
        public MP(Stream _IO) => this._IO = _IO;

        private Stream _IO;

        public void Close() => _IO.C();

        public byte[] ToArray(bool Close = false) => _IO.ToArray(Close);

        public MsgPack Read(bool array = false)
        {
            MsgPack msgPack = MsgPack.New;
            byte unk = _IO.RU8();
            if (!array) { msgPack.Name = RS((Types)unk); unk = _IO.RU8(); }
            Types type = (Types)unk;

            if (type >= Types.FixMap && type <= Types.FixMapMax)
            {
                msgPack.Object = KKdList<MsgPack>.New;
                for (int i = 0; i < unk - (byte)Types.FixMap; i++) msgPack.Add( Read(false));
            }
            else if (type >= Types.FixArr && type <= Types.FixArrMax)
            {
                msgPack.Object = new MsgPack[unk - (byte)Types.FixArr];
                for (int i = 0; i < unk - (byte)Types.FixArr; i++) msgPack[i] = Read( true);
            }
            else if (type >= Types.FixStr && type <= Types.FixStrMax) msgPack.Object = RS(type);
            else if (type >= Types.PosInt && type <= Types.PosIntMax) msgPack.Object =        unk;
            else if (type >= Types.NegInt && type <= Types.NegIntMax) msgPack.Object = (sbyte)unk;
            else
                while (true)
                {
                    if (RN (ref msgPack, ref type)) break;
                    if (RBy(ref msgPack, ref type)) break;
                    if (RM (ref msgPack, ref type)) break;
                    if (RE (ref msgPack, ref type)) break;
                    if (RS (ref msgPack, ref type)) break;
                    if (RBo(ref msgPack, ref type)) break;
                    if (RA (ref msgPack, ref type)) break;
                    if (RI (ref msgPack, ref type)) break;
                    if (RU (ref msgPack, ref type)) break;
                    if (RF (ref msgPack, ref type)) break;
                    break;
                }
            return msgPack;
        }

        private bool RI(ref MsgPack msgPack, ref Types type)
        {
                 if (type == Types.Int8 ) msgPack.Object = _IO.RI8();
            else if (type == Types.Int16) msgPack.Object = _IO.RI16E(true);
            else if (type == Types.Int32) msgPack.Object = _IO.RI32E(true);
            else if (type == Types.Int64) msgPack.Object = _IO.RI64E(true);
            else return false;
            return true;
        }

        private bool RU(ref MsgPack msgPack, ref Types type)
        {
                 if (type == Types.UInt8 ) msgPack.Object = _IO.RU8();
            else if (type == Types.UInt16) msgPack.Object = _IO.RU16E(true);
            else if (type == Types.UInt32) msgPack.Object = _IO.RU32E(true);
            else if (type == Types.UInt64) msgPack.Object = _IO.RU64E(true);
            else return false;
            return true;
        }

        private bool RF(ref MsgPack msgPack, ref Types type)
        {
                 if (type == Types.Float32) msgPack.Object = _IO.RF32E(true);
            else if (type == Types.Float64) msgPack.Object = _IO.RF64E(true);
            else return false;
            return true;
        }

        private bool RBo(ref MsgPack msgPack, ref Types type)
        {
                 if (type == Types.False) msgPack.Object = false;
            else if (type == Types.True ) msgPack.Object = true ;
            else return false;
            return true;
        }

        private bool RBy(ref MsgPack msgPack, ref Types type)
        {
            int Length = 0;
                 if (type == Types.Bin8 ) Length = _IO.RU8();
            else if (type == Types.Bin16) Length = _IO.RI16E(true);
            else if (type == Types.Bin32) Length = _IO.RI32E(true);
            else return false;
            msgPack.Object = _IO.RBy(Length);
            return true;
        }

        private bool RS(ref MsgPack msgPack, ref Types type)
        {
            string val = RS(type);
            if (val != null) msgPack.Object = val;
            else return false;
            return true;
        }

        private string RS(Types val)
        {
                 if (val >= Types.FixStr  && val <= Types.FixStrMax)
                return _IO.RS(val - Types.FixStr);
            else if (val >= Types.   Str8 && val <= Types.   Str32 )
            {
                System.Enum.TryParse(val.ToString(), out Types type);
                int Length = 0;
                     if (type == Types.Str8 ) Length = _IO.RU8();
                else if (type == Types.Str16) Length = _IO.RI16E(true);
                else                          Length = _IO.RI32E(true);
                return _IO.RS(Length);
            }
            return null;
        }

        private bool RN(ref MsgPack msgPack, ref Types type)
        {
            if (type == Types.Nil) msgPack.Object = null;
            else return false;
            return true;
        }

        private bool RA(ref MsgPack msgPack, ref Types type)
        {
            int Length = 0;
                 if (type == Types.Arr16) Length = _IO.RI16E(true);
            else if (type == Types.Arr32) Length = _IO.RI32E(true);
            else return false;
            msgPack.Object = new MsgPack[Length];
            for (int i = 0; i < Length; i++) msgPack[i] = Read(true);
            return true;
        }

        private bool RM(ref MsgPack msgPack, ref Types type)
        {
            int Length = 0;
                 if (type == Types.Map16) Length = _IO.RI16E(true);
            else if (type == Types.Map32) Length = _IO.RI32E(true);
            else return false;
            msgPack.Object = KKdList<MsgPack>.New;
            for (int i = 0; i < Length; i++) msgPack.Add(Read());
            return true;
        }

        private bool RE(ref MsgPack MsgPack, ref Types type)
        {
            int Length = 0;
                 if (type == Types.FixExt1 ) Length = 1 ;
            else if (type == Types.FixExt2 ) Length = 2 ;
            else if (type == Types.FixExt4 ) Length = 4 ;
            else if (type == Types.FixExt8 ) Length = 8 ;
            else if (type == Types.FixExt16) Length = 16;
            else if (type == Types.   Ext8 ) Length = _IO.RU8();
            else if (type == Types.   Ext16) Length = _IO.RI16E(true);
            else if (type == Types.   Ext32) Length = _IO.RI32E(true);
            else return false;
            MsgPack.Object = new MsgPack.Ext { Type = _IO.RI8(), Data = _IO.RBy(Length) };
            return true;
        }

        public MP W(MsgPack msgPack, bool IsArray = false)
        {
            if   (msgPack.Name != null && !IsArray) W(msgPack.Name);
            Write(msgPack.Object);
            return this;
        }

        private void Write(object obj)
        {
            if (obj == null) { WN(); return; }
            switch (obj)
            {
                case KKdList<MsgPack>  val: WM(val.Count );
                    for (int i = 0; i < val.Count ; i++) W(val[i]); break;
                case         MsgPack[] val: WA(val.Length);
                    for (int i = 0; i < val.Length; i++) W(val[i]); break;
                case     MsgPack val: W(val); break;
                case      byte[] val: W(val); break;
                case        bool val: W(val); break;
                case       sbyte val: W(val); break;
                case        byte val: W(val); break;
                case       short val: W(val); break;
                case      ushort val: W(val); break;
                case         int val: W(val); break;
                case        uint val: W(val); break;
                case        long val: W(val); break;
                case       ulong val: W(val); break;
                case       float val: W(val); break;
                case      double val: W(val); break;
                case      string val: W(val); break;
                case MsgPack.Ext val: W(val); break;
            }
        }

        private void W( sbyte val) { if (val < -0x20) _IO.W((byte)0xD0); _IO.W(val); }
        private void W(  byte val) { if (val >= 0x80) _IO.W((byte)0xCC); _IO.W(val); }
        private void W( short val) { if (( sbyte)val == val) W(( sbyte)val);
                                else if ((  byte)val == val) W((  byte)val);
                                else { _IO.W((byte)0xD1); _IO.WE(val, true); } }
        private void W(ushort val) { if ((  byte)val == val) W((  byte)val);
                                else { _IO.W((byte)0xCD); _IO.WE(val, true); } }
        private void W(   int val) { if (( short)val == val) W(( short)val);
                                else if ((ushort)val == val) W((ushort)val);
                                else { _IO.W((byte)0xD2); _IO.WE(val, true); } }
        private void W(  uint val) { if ((ushort)val == val) W((ushort)val);
                                else { _IO.W((byte)0xCE); _IO.WE(val, true); } }
        private void W(  long val) { if ((   int)val == val) W((   int)val);
                                else if ((  uint)val == val) W((  uint)val);
                                else { _IO.W((byte)0xD3); _IO.WE(val, true); } }
        private void W( ulong val) { if ((  uint)val == val) W((  uint)val);
                                else { _IO.W((byte)0xCF); _IO.WE(val, true); } }
        private void W( float val) { if ((  long)val == val) W((  long)val);
                                else { _IO.W((byte)0xCA); _IO.WE(val, true); } }
        private void W(double val) { if ((  long)val == val) W((  long)val);
                                else if (( float)val == val) W(( float)val);
                                else { _IO.W((byte)0xCB); _IO.WE(val, true); } }

        private void W(  bool val) =>
            _IO.W((byte)(val ? 0xC3 : 0xC2));

        private void W(byte[] val)
        {
            if (val == null) { WN(); return; }

                 if (val.Length <   0x100) { _IO.W((byte)0xC4); _IO.W ((  byte)val.Length      ); }
            else if (val.Length < 0x10000) { _IO.W((byte)0xC5); _IO.WE((ushort)val.Length, true); }
            else                           { _IO.W((byte)0xC6); _IO.WE(        val.Length, true); }
            _IO.W(val);
        }

        private void W(string val)
        {
            if (val == null) { WN(); return; }

            byte[] array = Text.ToUTF8(val);
                 if (array.Length <    0x20)   _IO.W((byte)(0xA0 | (array.Length & 0x1F)));
            else if (array.Length <   0x100) { _IO.W((byte)0xD9); _IO.W ((  byte)array.Length); }
            else if (array.Length < 0x10000) { _IO.W((byte)0xDA); _IO.WE((ushort)array.Length, true); }
            else                             { _IO.W((byte)0xDB); _IO.WE(        array.Length, true); }
            _IO.W(array);
        }

        private void WN() => _IO.W((byte)0xC0);

        private void WA(int val)
        {
                 if (val ==      0) { WN(); return; }
            else if (val <    0x10)   _IO.W((byte)(0x90 | (val & 0x0F)));
            else if (val < 0x10000) { _IO.W((byte)0xDC); _IO.WE((ushort)val, true); }
            else                    { _IO.W((byte)0xDD); _IO.WE(        val, true); }
        }

        private void WM(int val)
        {
                 if (val ==      0) { WN(); return; }
            else if (val <    0x10)   _IO.W((byte)(0x80 | (val & 0x0F)));
            else if (val < 0x10000) { _IO.W((byte)0xDE); _IO.WE((ushort)val, true); }
            else                    { _IO.W((byte)0xDF); _IO.WE(        val, true); }
        }

        private void W(MsgPack.Ext val) => WE(val);

        private void WE(MsgPack.Ext val)
        {
            if (val.Data == null) { WN(); return; }

                 if (val.Data.Length <  1 ) { WN(); return; }
            else if (val.Data.Length == 1 ) _IO.W((byte)0xD4);
            else if (val.Data.Length == 2 ) _IO.W((byte)0xD5);
            else if (val.Data.Length == 4 ) _IO.W((byte)0xD6);
            else if (val.Data.Length == 8 ) _IO.W((byte)0xD7);
            else if (val.Data.Length == 16) _IO.W((byte)0xD8);
            else
            {
                     if (val.Data.Length <   0x100)
                { _IO.W((byte)0xC7); _IO.W ((  byte)val.Data.Length); }
                else if (val.Data.Length < 0x10000)
                { _IO.W((byte)0xC8); _IO.WE((ushort)val.Data.Length, true); }
                else
                { _IO.W((byte)0xC9); _IO.WE(        val.Data.Length, true); }
            }
            _IO.W(val.Type);
            _IO.W(val.Data);
        }

        public void Dispose() => _IO.C();
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
