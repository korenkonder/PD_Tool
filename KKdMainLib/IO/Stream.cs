using System;
using KKdMainLib.Types;
using MSIO = System.IO;

namespace KKdMainLib.IO
{
    public unsafe class Stream : IDisposable
    {
        private MSIO.Stream stream;
        private   int I, i, i0, TempBitRead, TempBitWrite;
        private ushort ValRead;
        private  byte BitRead, BitWrite, ValWrite;
        private byte[] buf;
        private byte[] data;

        private Main.Format _format = Main.Format.NULL;

        public Main.Format Format
        {   get =>       _format;
            set {        _format = value;
                  IsBE = _format == Main.Format.F2BE;
                  IsX  = _format == Main.Format.X || _format == Main.Format.XHD; } }

        public bool IsBE = false;
        public bool IsX  = false;

        public  int     Offset { get => ( int)LongOffset; set => LongOffset = value; }
        public uint UIntOffset { get => (uint)LongOffset; set => LongOffset = value; }
        public long LongOffset;

        public  int       Length { get => ( int)stream.Length; set => stream.SetLength(value); }
        public uint   UIntLength { get => (uint)stream.Length; set => stream.SetLength(value); }
        public long   LongLength { get =>       stream.Length; set => stream.SetLength(value); }

        public  int     Position
        { get => ( int)stream.Position -     Offset; set => stream.Position = value +     Offset; }
        public uint UIntPosition
        { get => (uint)stream.Position - UIntOffset; set => stream.Position = value + UIntOffset; }
        public long LongPosition
        { get =>       stream.Position - LongOffset; set => stream.Position = value + LongOffset; }

        public bool CanRead    => stream.CanRead;
        public bool CanSeek    => stream.CanSeek;
        public bool CanTimeout => stream.CanTimeout;
        public bool CanWrite   => stream.CanWrite;

        public string File = null;

        public Stream(MSIO.Stream output = null, byte[] Data = null, bool isBE = false)
        {
            if (output == null) output = MSIO.Stream.Null;
            LongOffset = 0;
            BitRead = 8;
            ValRead = ValRead = BitWrite = 0;
            stream = output;
            Format = Main.Format.NULL;
            buf = new byte[128];
            IsBE = isBE;
            data = Data;
        }

        public void Close() => Dispose();

        public void Flush() => stream.Flush();

        public void SetLength(long length = 0) => stream.SetLength(length);

        public long Seek(long offset, SeekOrigin origin = 0) =>
            stream.Seek(offset, (MSIO.SeekOrigin)(int)origin);
        
        public long? Seek(long? offset, SeekOrigin origin)
        { if (offset == null) return null; return stream.Seek((long)offset, (MSIO.SeekOrigin)(int)origin); }

        public void Dispose()
        { CheckWrited(); Dispose(true); }

        private void Dispose(bool disposing)
        { if (disposing && stream != MSIO.Stream.Null) stream.Flush(); stream.Dispose(); data = null; }

        public MSIO.Stream BaseStream
        { get { stream.Flush(); return stream; } set { stream = value; } }

        public void Align(long Align)
        {
            long Al = Align - Position % Align;
            if (Position % Align != 0)
                stream.Seek(Position + Al, 0);
        }

        public void Align(long Align, bool SetLength)
        {
            if (SetLength) stream.SetLength(Position);
            long Al = Align - Position % Align;
            if (Position % Align != 0) stream.Seek(Position + Al, 0);
            if (SetLength) stream.SetLength(Position);
        }

        public void Align(long Align, bool SetLength0, bool SetLength1)
        {
            if (SetLength0) stream.SetLength(Position);
            long Al = Align - Position % Align;
            if (Position % Align != 0) stream.Seek(Position + Al, 0);
            if (SetLength1) stream.SetLength(Position);
        }

        public   bool ReadBoolean() =>         stream.ReadByte() == 1;
        public  sbyte   ReadSByte() => ( sbyte)stream.ReadByte();
        public   byte    ReadByte() => (  byte)stream.ReadByte();
        public  sbyte    ReadInt8() => ( sbyte) IntFromArray(1);
        public   byte   ReadUInt8() => (  byte)UIntFromArray(1);
        public  short   ReadInt16() => ( short) IntFromArray(2);
        public ushort  ReadUInt16() => (ushort)UIntFromArray(2);
        public    int   ReadInt24() => (   int) IntFromArray(3);
        public    int   ReadInt32() => (   int) IntFromArray(4);
        public   uint  ReadUInt32() => (  uint)UIntFromArray(4);
        public   long   ReadInt64() =>          IntFromArray(8);
        public  ulong  ReadUInt64() =>         UIntFromArray(8);
        public   Half   ReadHalf() { ushort a = ReadUInt16(); return  (  Half ) a; }
        public  float ReadSingle() {   uint a = ReadUInt32(); return *( float*)&a; }
        public double ReadDouble() {  ulong a = ReadUInt64(); return *(double*)&a; }
        
        public  short  ReadInt16Endian() => ( short) IntFromArray(2, IsBE);
        public ushort ReadUInt16Endian() => (ushort)UIntFromArray(2, IsBE);
        public    int  ReadInt24Endian() => (   int) IntFromArray(3, IsBE);
        public    int  ReadInt32Endian() => (   int) IntFromArray(4, IsBE);
        public   uint ReadUInt32Endian() => (  uint)UIntFromArray(4, IsBE);
        public   long  ReadInt64Endian() =>          IntFromArray(8, IsBE);
        public  ulong ReadUInt64Endian() =>         UIntFromArray(8, IsBE);
        public   Half   ReadHalfEndian() { ushort a = ReadUInt16Endian(); return  (  Half ) a; }
        public  float ReadSingleEndian() {   uint a = ReadUInt32Endian(); return *( float*)&a; }
        public double ReadDoubleEndian() {  ulong a = ReadUInt64Endian(); return *(double*)&a; }

        public  short  ReadInt16Endian(bool IsBE) => ( short) IntFromArray(2, IsBE);
        public ushort ReadUInt16Endian(bool IsBE) => (ushort)UIntFromArray(2, IsBE);
        public    int  ReadInt24Endian(bool IsBE) => (   int) IntFromArray(3, IsBE);
        public    int  ReadInt32Endian(bool IsBE) => (   int) IntFromArray(4, IsBE);
        public   uint ReadUInt32Endian(bool IsBE) => (  uint)UIntFromArray(4, IsBE);
        public   long  ReadInt64Endian(bool IsBE) =>          IntFromArray(8, IsBE);
        public  ulong ReadUInt64Endian(bool IsBE) =>         UIntFromArray(8, IsBE);
        public   Half   ReadHalfEndian(bool IsBE)
        { ushort a = ReadUInt16Endian(IsBE); return  (  Half ) a; }
        public  float ReadSingleEndian(bool IsBE)
        {   uint a = ReadUInt32Endian(IsBE); return *( float*)&a; }
        public double ReadDoubleEndian(bool IsBE)
        {  ulong a = ReadUInt64Endian(IsBE); return *(double*)&a; }

        public void Write(byte[] Val)                         => stream.Write(Val, 0, Val. Length);
        public void Write(byte[] Val,             int Length) => stream.Write(Val, 0     , Length);
        public void Write(byte[] Val, int Offset, int Length) => stream.Write(Val, Offset, Length);
        public void Write(char[] val, bool UTF8 = true)
        { if (UTF8) Write(val.ToUTF8()); else Write(val.ToASCII()); }

        public void WriteByte(byte val) => stream.WriteByte(val);

        public void Write(  bool val) => stream.WriteByte((byte)(val ? 1 : 0));
        public void Write( sbyte val) => stream.WriteByte((byte) val);
        public void Write(  byte val) => stream.WriteByte(       val);
        public void Write( short val) => ToArray(2,           val);
        public void Write(ushort val) => ToArray(2,           val);
        public void Write(   int val) => ToArray(4,           val);
        public void Write(  uint val) => ToArray(4,           val);
        public void Write(  long val) => ToArray(8,           val);
        public void Write( ulong val) => ToArray(8,           val);
        public void Write(  Half val) => ToArray(2,  (ushort) val);
        public void Write( float val) => ToArray(4, *( uint*)&val);
        public void Write(double val) => ToArray(8, *(ulong*)&val);

        public void Write( sbyte? val) => Write(val.GetValueOrDefault());
        public void Write(  byte? val) => Write(val.GetValueOrDefault());
        public void Write( short? val) => Write(val.GetValueOrDefault());
        public void Write(ushort? val) => Write(val.GetValueOrDefault());
        public void Write(   int? val) => Write(val.GetValueOrDefault());
        public void Write(  uint? val) => Write(val.GetValueOrDefault());
        public void Write(  long? val) => Write(val.GetValueOrDefault());
        public void Write( ulong? val) => Write(val.GetValueOrDefault());
        public void Write( float? val) => Write(val.GetValueOrDefault());
        public void Write(double? val) => Write(val.GetValueOrDefault());
        
        public void Write(  bool* val, int Length)
        { for (i = 0; i < Length; i++) stream.WriteByte((byte)(val[i] ? 1 : 0)); }
        public void Write( sbyte* val, int Length)
        { for (i = 0; i < Length; i++) stream.WriteByte((byte) val[i]); }
        public void Write(  byte* val, int Length)
        { for (i = 0; i < Length; i++) stream.WriteByte(       val[i]); }
        public void Write( short* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(2,           val[i]); }
        public void Write(ushort* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(2,           val[i]); }
        public void Write(   int* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(4,           val[i]); }
        public void Write(  uint* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(4,           val[i]); }
        public void Write(  long* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(8,           val[i]); }
        public void Write( ulong* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(8,           val[i]); }
        public void Write( float* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(4, *( uint*)&val[i]); }
        public void Write(double* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(4, *(ulong*)&val[i]); }
        
        public void WriteEndian( short* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(2, Endian(val[i], 2, IsBE)); }
        public void WriteEndian(ushort* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(2, Endian(val[i], 2, IsBE)); }
        public void WriteEndian(   int* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(4, Endian(val[i], 4, IsBE)); }
        public void WriteEndian(  uint* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(4, Endian(val[i], 4, IsBE)); }
        public void WriteEndian(  long* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(8, Endian(val[i], 8, IsBE)); }
        public void WriteEndian( ulong* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(8, Endian(val[i], 8, IsBE)); }
        public void WriteEndian( float* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(4, Endian(*( uint*)&val[i], 4, IsBE)); }
        public void WriteEndian(double* val, int Length)
        { for (i = 0; i < Length; i++) ToArray(8, Endian(*(ulong*)&val[i], 8, IsBE)); }

        public void WriteEndian( short* val, int Length, bool IsBE)
        { for (i = 0; i < Length; i++) ToArray(2, Endian(val[i], 2, IsBE)); }
        public void WriteEndian(ushort* val, int Length, bool IsBE)
        { for (i = 0; i < Length; i++) ToArray(2, Endian(val[i], 2, IsBE)); }
        public void WriteEndian(   int* val, int Length, bool IsBE)
        { for (i = 0; i < Length; i++) ToArray(4, Endian(val[i], 4, IsBE)); }
        public void WriteEndian(  uint* val, int Length, bool IsBE)
        { for (i = 0; i < Length; i++) ToArray(4, Endian(val[i], 4, IsBE)); }
        public void WriteEndian(  long* val, int Length, bool IsBE)
        { for (i = 0; i < Length; i++) ToArray(8, Endian(val[i], 8, IsBE)); }
        public void WriteEndian( ulong* val, int Length, bool IsBE)
        { for (i = 0; i < Length; i++) ToArray(8, Endian(val[i], 8, IsBE)); }
        public void WriteEndian( float* val, int Length, bool IsBE)
        { for (i = 0; i < Length; i++) ToArray(4, Endian(*( uint*)&val[i], 4, IsBE)); }
        public void WriteEndian(double* val, int Length, bool IsBE)
        { for (i = 0; i < Length; i++) ToArray(8, Endian(*(ulong*)&val[i], 8, IsBE)); }

        public void Write(  char val, bool UTF8 = true)
        { if (UTF8) Write(val.ToString().ToUTF8()); else Write(val.ToString().ToASCII()); }
        public void Write(string val, bool UTF8 = true)
        { if (UTF8) Write(val           .ToUTF8()); else Write(val           .ToASCII()); }
        
        public void WriteEndian( short val) => ToArray(2, Endian(val, 2, IsBE));
        public void WriteEndian(ushort val) => ToArray(2, Endian(val, 2, IsBE));
        public void WriteEndian(   int val) => ToArray(4, Endian(val, 4, IsBE));
        public void WriteEndian(  uint val) => ToArray(4, Endian(val, 4, IsBE));
        public void WriteEndian(  long val) => ToArray(8, Endian(val, 8, IsBE));
        public void WriteEndian( ulong val) => ToArray(8, Endian(val, 8, IsBE));
        public void WriteEndian( float val) => ToArray(4, Endian(*( uint*)&val, 4, IsBE));
        public void WriteEndian(double val) => ToArray(8, Endian(*(ulong*)&val, 8, IsBE));

        public void WriteEndian( short val, bool IsBE) => ToArray(2, Endian(val, 2, IsBE));
        public void WriteEndian(ushort val, bool IsBE) => ToArray(2, Endian(val, 2, IsBE));
        public void WriteEndian(   int val, bool IsBE) => ToArray(4, Endian(val, 4, IsBE));
        public void WriteEndian(  uint val, bool IsBE) => ToArray(4, Endian(val, 4, IsBE));
        public void WriteEndian(  long val, bool IsBE) => ToArray(8, Endian(val, 8, IsBE));
        public void WriteEndian( ulong val, bool IsBE) => ToArray(8, Endian(val, 8, IsBE));
        public void WriteEndian( float val, bool IsBE) => ToArray(4, Endian(*( uint*)&val, 4, IsBE));
        public void WriteEndian(double val, bool IsBE) => ToArray(8, Endian(*(ulong*)&val, 8, IsBE));

        public   long Endian(  long BE, byte Length, bool IsBE)
        { if (IsBE) { for (I = 0; I < Length; I++) { buf[I] = (byte)BE; BE >>= 8; } BE = 0;
                      for (I = 0; I < Length; I++) { BE |= buf[I]; if (I < Length - 1) BE <<= 8; } } return BE; }

        public  ulong Endian( ulong BE, byte Length, bool IsBE)
        { if (IsBE) { for (I = 0; I < Length; I++) { buf[I] = (byte)BE; BE >>= 8; } BE = 0;
                      for (I = 0; I < Length; I++) { BE |= buf[I]; if (I < Length - 1) BE <<= 8; } } return BE; }

        private void ToArray(byte L,  long val)
        { CheckWrited(); for (I = 0; I < L; I++) { buf[I] = (byte)val; val >>= 8; } stream.Write(buf, 0, L); }

        private void ToArray(byte L, ulong val)
        { CheckWrited(); for (I = 0; I < L; I++) { buf[I] = (byte)val; val >>= 8; } stream.Write(buf, 0, L); }

        private  long  IntFromArray(byte L) { stream.Read(buf, 0, L);  long val = 0;
                         for (I = L; I > 0; I--) { val <<= 8; val |= buf[I - 1]; } return val; }

        private ulong UIntFromArray(byte L) { stream.Read(buf, 0, L); ulong val = 0;
                         for (I = L; I > 0; I--) { val <<= 8; val |= buf[I - 1]; } return val; }

        private  long  IntFromArray(byte L, bool IsBE) { stream.Read(buf, 0, L);  long val = 0; if (IsBE)
                         for (I = 0; I < L; I++) { val <<= 8; val |= buf[I    ]; } else
                         for (I = L; I > 0; I--) { val <<= 8; val |= buf[I - 1]; } return val; }

        private ulong UIntFromArray(byte L, bool IsBE) { stream.Read(buf, 0, L); ulong val = 0; if (IsBE)
                         for (I = 0; I < L; I++) { val <<= 8; val |= buf[I    ]; } else
                         for (I = L; I > 0; I--) { val <<= 8; val |= buf[I - 1]; } return val; }
        
        public char ReadChar(bool UTF8 = true)
        { if (UTF8) return ReadCharUTF8();
          else      return (char)stream.ReadByte(); }

        public char ReadCharUTF8()
        {
            byte t;
            int T;
            int val = 0;
            for (I = 0, i0 = 4; I < i0; I++)
            {
                T = stream.ReadByte();
                if (T == -1) return '\uFFFF';
                t = (byte)T;

                     if ((t & 0xC0) == 0x80 && I >  0)   val = (val << 6) | (t & 0x3F);
                else if ((t & 0x80) == 0x00 && I == 0)   return (char)t;
                else if ((t & 0xE0) == 0xC0 && I == 0) { val = t & 0x1F; i0 = 2; }
                else if ((t & 0xF0) == 0xE0 && I == 0) { val = t & 0x0F; i0 = 3; }
                else if ((t & 0xF8) == 0xF0 && I == 0) { val = t & 0x07; i0 = 4; }
                else return '\uFFFF';
            }
            return (char)val;
        }
        
        public string ReadString(long Length, bool UTF8 = true)
        { if (UTF8) return ReadStringUTF8 (Length);
          else      return ReadStringASCII(Length); }

        public string ReadStringUTF8 (long Length) => ReadBytes(Length).ToUTF8 ();
        public string ReadStringASCII(long Length) => ReadBytes(Length).ToASCII();
        
        
        public string ReadString(long? Length, bool UTF8 = true)
        { if (UTF8) return ReadStringUTF8 (Length);
          else      return ReadStringASCII(Length); }

        public string ReadStringUTF8 (long? Length) => ReadBytes(Length).ToUTF8 ();
        public string ReadStringASCII(long? Length) => ReadBytes(Length).ToASCII();
        
        public byte[] ReadBytes(long  Length, int Offset = 0)
        { byte[] Buf = new byte[Length]; if (Offset > 0) stream.Position = Offset;
            stream.Read(Buf, 0, (int)Length); return Buf; }
        
        public void ReadBytes(long  Length, byte[] Buf, long Offset = 0)
        { if (Offset > 0) stream.Position = Offset; stream.Read(Buf, 0, (int)Length); }

        public byte[] ReadBytes(long? Length, int Offset = 0)
        { if (Length == null) return new byte[0]; else return ReadBytes((long)Length, Offset); }

        public void ReadBytes(long Length, byte Bits, byte[] Buf, long Offset = 0)
        { if (Offset > 0) stream.Seek(Offset, 0);
                 if (Bits > 0 && Bits < 8) for (i0 = 0; i0 < Length; i0++) Buf[i0] = ReadBits(Bits); }
           
        public byte ReadBits(byte Bits)
        {
            BitRead += Bits;
            TempBitRead = 8 - BitRead;
            if (TempBitRead < 0)
            {
                BitRead = (byte)-TempBitRead;
                TempBitRead = 8 + TempBitRead;
                ValRead = (ushort)((ValRead << 8) | (byte)stream.ReadByte());
            }
            return (byte)((ValRead >> TempBitRead) & ((1 << Bits) - 1));
        }

        public byte ReadHalfByte() => ReadBits(4);
        
        public void Write(byte val, byte Bits)
        {
            BitWrite += Bits;
            TempBitWrite = 8 - BitWrite;
            if (TempBitWrite < 0)
            {
                BitWrite = (byte)-TempBitWrite;
                TempBitWrite = 8 + TempBitWrite;
                stream.WriteByte((byte)(ValWrite | (val >> BitWrite)));
                ValWrite = 0;
            }
            ValWrite |= (byte)(val << TempBitWrite);
        }

        public void CheckRead  () { if (BitRead  > 0)                    ValRead  = 0; BitRead  = 8; }
        public void CheckWrited() { if (BitWrite > 0) { Write(ValWrite); ValWrite =    BitWrite = 0; } }

        public byte[] ToArray(bool Close)
        { byte[] Data = ToArray(); if (Close) this.Close(); return Data; }

        public byte[] ToArray()
        {
            long Offset = stream.Position;
            LongPosition = 0;
            byte[] Data = ReadBytes(stream.Length);
            LongPosition = Offset;
            return Data;
        }
    }

    public enum SeekOrigin
    {
        Begin   = 0,
        Current = 1,
        End     = 2,
    }
}
