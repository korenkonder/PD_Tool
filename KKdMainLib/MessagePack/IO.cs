using System;
using System.Collections.Generic;
using KKdMainLib.IO;

namespace KKdMainLib.MessagePack
{
    public class IO
    {
        public Stream _IO;

        public IO(         ) => _IO = File.OpenWriter();
        public IO(Stream IO) => _IO = IO;

        public void Close() => _IO.Close();

        public MsgPack Read(bool NotArray = true)
        {
            MsgPack MsgPack = new MsgPack();
            byte Unk = _IO.ReadByte();
            MsgPack.Type = (MsgPack.Types)Unk;
            if (NotArray)
            {
                MsgPack.Name = ReadString(MsgPack.Type);
                if (MsgPack.Name != null) { Unk = _IO.ReadByte(); MsgPack.Type = (MsgPack.Types)Unk; }
            }

            bool FixArr = MsgPack.Type >= MsgPack.Types.FixArr && MsgPack.Type <= MsgPack.Types.FixArrMax;
            bool FixMap = MsgPack.Type >= MsgPack.Types.FixMap && MsgPack.Type <= MsgPack.Types.FixMapMax;
            bool FixStr = MsgPack.Type >= MsgPack.Types.FixStr && MsgPack.Type <= MsgPack.Types.FixStrMax;
            bool PosInt = MsgPack.Type >= MsgPack.Types.PosInt && MsgPack.Type <= MsgPack.Types.PosIntMax;
            bool NegInt = MsgPack.Type >= MsgPack.Types.NegInt && MsgPack.Type <= MsgPack.Types.NegIntMax;
            if (FixArr || FixMap || FixStr || PosInt || NegInt)
            {
                if (FixArr || FixMap)
                {
                    MsgPack.Type = FixMap ? MsgPack.Types.FixMap : MsgPack.Types.FixArr;
                    if (FixMap)
                    {
                        MsgPack.Object = new List<object>();
                        for (int i = 0; i < Unk - (byte)MsgPack.Type; i++)
                            MsgPack.Add(Read());
                    }
                    else
                    {
                        MsgPack.Object = new object[Unk - (byte)MsgPack.Type];
                        for (int i = 0; i < Unk - (byte)MsgPack.Type; i++)
                            MsgPack[i] = Read(false);
                    }
                }
                else if (FixStr)
                { MsgPack.Object = ReadString(MsgPack.Type); MsgPack.Type = MsgPack.Types.FixStr; }
                else if (PosInt)
                { MsgPack.Object = (ulong)       Unk;        MsgPack.Type = MsgPack.Types.PosInt; }
                else if (NegInt)
                { MsgPack.Object = ( long)(sbyte)Unk;        MsgPack.Type = MsgPack.Types.NegInt; }
                return MsgPack;
            }
            
            while (true)
            {
                if (ReadNil    (ref MsgPack)) break;
                if (ReadArr    (ref MsgPack)) break;
                if (ReadMap    (ref MsgPack)) break;
                if (ReadExt    (ref MsgPack)) break;
                if (ReadString (ref MsgPack)) break;
                if (ReadBoolean(ref MsgPack)) break;
                if (ReadBytes  (ref MsgPack)) break;
                if (ReadInt    (ref MsgPack)) break;
                if (ReadUInt   (ref MsgPack)) break;
                if (ReadFloat  (ref MsgPack)) break;
                break;
            }
            return MsgPack;
        }

        private bool ReadInt(ref MsgPack MsgPack)
        {
                 if (MsgPack.Type == MsgPack.Types.Int8 ) MsgPack.Object = (long)_IO.ReadSByte();
            else if (MsgPack.Type == MsgPack.Types.Int16) MsgPack.Object = (long)_IO.ReadInt16Endian(true);
            else if (MsgPack.Type == MsgPack.Types.Int32) MsgPack.Object = (long)_IO.ReadInt32Endian(true);
            else if (MsgPack.Type == MsgPack.Types.Int64) MsgPack.Object = (long)_IO.ReadInt64Endian(true);
            else return false;
            return true;
        }

        private bool ReadUInt(ref MsgPack MsgPack)
        {
                 if (MsgPack.Type == MsgPack.Types.UInt8 ) MsgPack.Object = (ulong)_IO.ReadByte();
            else if (MsgPack.Type == MsgPack.Types.UInt16) MsgPack.Object = (ulong)_IO.ReadUInt16Endian(true);
            else if (MsgPack.Type == MsgPack.Types.UInt32) MsgPack.Object = (ulong)_IO.ReadUInt32Endian(true);
            else if (MsgPack.Type == MsgPack.Types.UInt64) MsgPack.Object = (ulong)_IO.ReadUInt64Endian(true);
            else return false;
            return true;
        }

        private bool ReadFloat(ref MsgPack MsgPack)
        {
                 if (MsgPack.Type == MsgPack.Types.Float32) MsgPack.Object = _IO.ReadSingleEndian(true);
            else if (MsgPack.Type == MsgPack.Types.Float64) MsgPack.Object = _IO.ReadDoubleEndian(true);
            else return false;
            return true;
        }

        private bool ReadBoolean(ref MsgPack MsgPack)
        {
                 if (MsgPack.Type == MsgPack.Types.False) MsgPack.Object = false;
            else if (MsgPack.Type == MsgPack.Types.True ) MsgPack.Object = true ;
            else return false;
            return true;
        }

        private bool ReadBytes(ref MsgPack MsgPack)
        {
            int Length = 0;
                 if (MsgPack.Type == MsgPack.Types.Bin8 ) Length = _IO.ReadByte();
            else if (MsgPack.Type == MsgPack.Types.Bin16) Length = _IO.ReadInt16Endian(true);
            else if (MsgPack.Type == MsgPack.Types.Bin32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = _IO.ReadBytes(Length);
            return true;
        }
        
        private bool ReadString(ref MsgPack MsgPack)
        {
            string val = ReadString(MsgPack.Type);
            if (val != null) MsgPack.Object = val;
            else return false;
            return true;
        }

        private string ReadString(MsgPack.Types Val)
        {
            if (Val >= MsgPack.Types.FixStr && Val <= MsgPack.Types.FixStrMax)
                return _IO.ReadString(Val - MsgPack.Types.FixStr);
            else if (Val >= MsgPack.Types.Str8 && Val <= MsgPack.Types.Str32)
            {
                Enum.TryParse(Val.ToString(), out MsgPack.Types Type);
                int Length = 0;
                     if (Type == MsgPack.Types.Str8 ) Length = _IO.ReadByte();
                else if (Type == MsgPack.Types.Str16) Length = _IO.ReadInt16Endian(true);
                else                                  Length = _IO.ReadInt32Endian(true);
                return _IO.ReadString(Length);
            }
            return null;
        }

        private bool ReadNil(ref MsgPack MsgPack)
        {
            if (MsgPack.Type == MsgPack.Types.Nil)
                MsgPack.Object = null;
            else return false;
            return true;
        }

        private bool ReadArr(ref MsgPack MsgPack)
        {
            int Length = 0;
                 if (MsgPack.Type == MsgPack.Types.Arr16) Length = _IO.ReadInt16Endian(true);
            else if (MsgPack.Type == MsgPack.Types.Arr32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = new object[Length];
            for (int i = 0; i < Length; i++) MsgPack[i] = Read(false);
            return true;
        }

        private bool ReadMap(ref MsgPack MsgPack)
        {
            int Length = 0;
                 if (MsgPack.Type == MsgPack.Types.Map16) Length = _IO.ReadInt16Endian(true);
            else if (MsgPack.Type == MsgPack.Types.Map32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = new List<object>();
            for (int i = 0; i < Length; i++) MsgPack.Add(Read());
            return true;
        }

        private bool ReadExt(ref MsgPack MsgPack)
        {
            int Length = 0;
                 if (MsgPack.Type == MsgPack.Types.FixExt1 ) Length = 1 ; 
            else if (MsgPack.Type == MsgPack.Types.FixExt2 ) Length = 2 ;
            else if (MsgPack.Type == MsgPack.Types.FixExt4 ) Length = 4 ;
            else if (MsgPack.Type == MsgPack.Types.FixExt8 ) Length = 8 ;
            else if (MsgPack.Type == MsgPack.Types.FixExt16) Length = 16;
            else if (MsgPack.Type == MsgPack.Types.   Ext8 ) Length = _IO.ReadByte();
            else if (MsgPack.Type == MsgPack.Types.   Ext16) Length = _IO.ReadInt16Endian(true);
            else if (MsgPack.Type == MsgPack.Types.   Ext32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = new MsgPack.Ext { Type = _IO.ReadSByte(), Data = _IO.ReadBytes(Length) };
            return true;
        }

        public IO Write(MsgPack MsgPack, bool Close)
        { Write(MsgPack); if (Close) this.Close(); return this; }
        
        public IO Write(MsgPack MsgPack)
        {
            if (MsgPack.Name   != null) Write(MsgPack.Name);
            Write(MsgPack.Object);
            return this;
        }

        private void Write(object obj)
        {
            if (obj == null) { WriteNil(); return; }
            switch (obj)
            {
                case List<object>  val: WriteMap(val.Count );
                    foreach (object Val in val) Write(Val); break;
                case      object[] val: WriteArr(val.Length);
                    foreach (object Val in val) Write(Val); break;
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

        private void Write( sbyte val) { if (val < -0x20) _IO.Write((byte)0xD0); _IO.Write(val); }
        private void Write(  byte val) { if (val >= 0x80) _IO.Write((byte)0xCC); _IO.Write(val); }
        private void Write( short val) { if (( sbyte)val == val) Write(( sbyte)val);
                                    else if ((  byte)val == val) Write((  byte)val);
                                    else { _IO.Write((byte)0xD1); _IO.WriteEndian(val, true); } }
        private void Write(ushort val) { if ((  byte)val == val) Write((  byte)val);
                                    else { _IO.Write((byte)0xCD); _IO.WriteEndian(val, true); } }
        private void Write(   int val) { if (( short)val == val) Write(( short)val);
                                    else if ((ushort)val == val) Write((ushort)val);
                                    else { _IO.Write((byte)0xD2); _IO.WriteEndian(val, true); } }
        private void Write(  uint val) { if ((ushort)val == val) Write((ushort)val);
                                    else { _IO.Write((byte)0xCE); _IO.WriteEndian(val, true); } }
        private void Write(  long val) { if ((   int)val == val) Write((   int)val);
                                    else if ((  uint)val == val) Write((  uint)val);
                                    else { _IO.Write((byte)0xD3); _IO.WriteEndian(val, true); } }
        private void Write( ulong val) { if ((  uint)val == val) Write((  uint)val);
                                    else { _IO.Write((byte)0xCF); _IO.WriteEndian(val, true); } }
        private void Write( float val) { if ((  long)val == val) Write((  long)val);
                                    else { _IO.Write((byte)0xCA); _IO.WriteEndian(val, true); } }
        private void Write(double val) { if ((  long)val == val) Write((  long)val);
                                    else if (( float)val == val) Write(( float)val);
                                    else { _IO.Write((byte)0xCB); _IO.WriteEndian(val, true); } }

        private void Write(  bool val)
        { _IO.Write((byte)(val ? 0xC3 : 0xC2)); }

        private void Write(byte[] val)
        {
            if (val == null) { WriteNil(); return; }

                 if (val.Length <   0x100)
            { _IO.Write((byte)0xC4); _IO.Write      ((  byte)val.Length      ); }
            else if (val.Length < 0x10000)
            { _IO.Write((byte)0xC5); _IO.WriteEndian((ushort)val.Length, true); }
            else
            { _IO.Write((byte)0xC6); _IO.WriteEndian(        val.Length, true); }
            _IO.Write(val);
        }
        
        private void Write(string val)
        {
            if (val == null) { WriteNil(); return; }

            byte[] array = Text.ToUTF8(val);
                 if (array.Length <    0x20)
              _IO.Write((byte)(0xA0 | (array.Length & 0x1F)));
            else if (array.Length <   0x100)
            { _IO.Write((byte) 0xD9); _IO.Write      ((  byte)array.Length); }
            else if (array.Length < 0x10000)
            { _IO.Write((byte )0xDA); _IO.WriteEndian((ushort)array.Length, true); }
            else
            { _IO.Write((byte) 0xDB); _IO.WriteEndian(        array.Length, true); }
            _IO.Write(array);
        }

        private void WriteNil() => _IO.Write((byte)0xC0);

        private void WriteArr(int val)
        {
                 if (val ==      0) { WriteNil(); return; }
            else if (val <    0x10)   _IO.Write((byte)(0x90 | (val & 0x0F)));
            else if (val < 0x10000) { _IO.Write((byte) 0xDC); _IO.WriteEndian((ushort)val, true); }
            else                    { _IO.Write((byte) 0xDD); _IO.WriteEndian(        val, true); }
        }
                
        private void WriteMap(int val)
        {
                 if (val ==      0) { WriteNil(); return; }
            else if (val <    0x10)   _IO.Write((byte)(0x80 | (val & 0x0F)));
            else if (val < 0x10000) { _IO.Write((byte) 0xDE); _IO.WriteEndian((ushort)val, true); }
            else                    { _IO.Write((byte) 0xDF); _IO.WriteEndian(        val, true); }
        }

        private void Write(MsgPack.Ext val)
        {
            if (val.Data == null) { WriteNil(); return; }

                 if (val.Data.Length <  1 ) { WriteNil(); return; }
            else if (val.Data.Length == 1 ) _IO.Write((byte)0xD4);
            else if (val.Data.Length == 2 ) _IO.Write((byte)0xD5);
            else if (val.Data.Length == 4 ) _IO.Write((byte)0xD6);
            else if (val.Data.Length == 8 ) _IO.Write((byte)0xD7);
            else if (val.Data.Length == 16) _IO.Write((byte)0xD8);
            else
            {
                     if (val.Data.Length <   0x100)
                { _IO.Write((byte)0xC7); _IO.Write      ((  byte)val.Data.Length); }
                else if (val.Data.Length < 0x10000)
                { _IO.Write((byte)0xC8); _IO.WriteEndian((ushort)val.Data.Length, true); }
                else
                { _IO.Write((byte)0xC9); _IO.WriteEndian(        val.Data.Length, true); }
            }
            _IO.Write(val.Type);
            _IO.Write(val.Data);
        }
    }
}
