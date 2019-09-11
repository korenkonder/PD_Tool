using System;
using System.Runtime.InteropServices;
using KKdBaseLib;
using MSIO = System.IO;

namespace KKdMainLib.IO
{
    public unsafe class Stream : IDisposable
    {
        private MSIO.Stream stream;
        private int I, i, BitRead, BitWrite, TempBitRead, TempBitWrite, ValRead, ValWrite;
        private byte[] buf;
        private byte* ptr;

        private Format _format = Format.NULL;

        public Format Format
        {   get =>       _format;
            set {        _format = value;
                  IsBE = _format == Format.F2BE;
                  IsX  = _format == Format.X || _format == Format.XHD; } }

        public bool IsBE = false;
        public bool IsX  = false;

        public  int     Offset { get => ( int)LongOffset; set => LongOffset = value; }
        public uint UIntOffset { get => (uint)LongOffset; set => LongOffset = value; }
        public long LongOffset;

        public  int       Length { get => ( int)stream.Length -     Offset;
            set => stream.SetLength(value +     Offset); }
        public uint   UIntLength { get => (uint)stream.Length - UIntOffset;
            set => stream.SetLength(value + UIntOffset); }
        public long   LongLength { get =>       stream.Length - LongOffset;
            set => stream.SetLength(value + LongOffset); }

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

        public Stream(MSIO.Stream output = null, bool isBE = false)
        {
            if (output == null) output = MSIO.Stream.Null;
            LongOffset = 0;
            BitRead = 8;
            ValRead = ValRead = BitWrite = 0;
            stream = output;
            Format = Format.NULL;
            buf = new byte[128];
            ptr = buf.GetPtr();
            IsBE = isBE;
        }

        public void Close() => Dispose();

        public void Flush() => stream.Flush();

        public void SetLength(long length = 0) => stream.SetLength(length);

        public long Seek(long offset, SeekOrigin origin = 0) =>
            stream.Seek(offset, (MSIO.SeekOrigin)(int)origin);
        
        public long? Seek(long? offset, SeekOrigin origin)
        { if (offset == null) return null; return stream.Seek((long)offset, (MSIO.SeekOrigin)(int)origin); }

        public void Dispose()
        { CW(); if (stream != MSIO.Stream.Null) { stream.Flush(); stream.Dispose(); } }

        public MSIO.Stream BaseStream
        { get { stream.Flush(); return stream; } set { stream = value; } }

        public void Align(long Align)
        {
            long Al = Align - Position % Align;
            if (Position % Align != 0)
                stream.Seek(Position + Offset + Al, 0);
        }

        public void Align(long Align, bool SetLength)
        {
            if (SetLength) stream.SetLength(Position + Offset);
            long Al = Align - Position % Align;
            if (Position % Align != 0) stream.Seek(Position + Offset + Al, 0);
            if (SetLength) stream.SetLength(Position + Offset);
        }

        public void Align(long Align, bool SetLength0, bool SetLength1)
        {
            if (SetLength0) stream.SetLength(Position + Offset);
            long Al = Align - Position % Align;
            if (Position % Align != 0) stream.Seek(Position + Al, 0);
            if (SetLength1) stream.SetLength(Position + Offset);
        }

        public   bool ReadBoolean() =>         stream.ReadByte() != 0;
        public  sbyte   ReadSByte() => ( sbyte)stream.ReadByte();
        public   byte    ReadByte() => (  byte)stream.ReadByte();
        public  sbyte    ReadInt8() => ( sbyte)stream.ReadByte();
        public   byte   ReadUInt8() => (  byte)stream.ReadByte();
        public  short   ReadInt16() { CR(); stream.Read(buf, 0, 2); return *( short*)ptr; }
        public ushort  ReadUInt16() { CR(); stream.Read(buf, 0, 2); return *(ushort*)ptr; }
        public    int   ReadInt32() { CR(); stream.Read(buf, 0, 4); return *(   int*)ptr; }
        public   uint  ReadUInt32() { CR(); stream.Read(buf, 0, 4); return *(  uint*)ptr; }
        public   long   ReadInt64() { CR(); stream.Read(buf, 0, 8); return *(  long*)ptr; }
        public  ulong  ReadUInt64() { CR(); stream.Read(buf, 0, 8); return *( ulong*)ptr; }
        public  float  ReadSingle() { CR(); stream.Read(buf, 0, 4); return *( float*)ptr; }
        public double  ReadDouble() { CR(); stream.Read(buf, 0, 8); return *(double*)ptr; }
        
        public  short  ReadInt16Endian() { CR(); stream.Read(buf, 0, 2); buf.Endian(2, IsBE); return *( short*)ptr; }
        public ushort ReadUInt16Endian() { CR(); stream.Read(buf, 0, 2); buf.Endian(2, IsBE); return *(ushort*)ptr; }
        public    int  ReadInt32Endian() { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *(   int*)ptr; }
        public   uint ReadUInt32Endian() { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *(  uint*)ptr; }
        public   long  ReadInt64Endian() { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *(  long*)ptr; }
        public  ulong ReadUInt64Endian() { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *( ulong*)ptr; }
        public  float ReadSingleEndian() { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *( float*)ptr; }
        public double ReadDoubleEndian() { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *(double*)ptr; }

        public  short  ReadInt16Endian(bool IsBE) { CR(); stream.Read(buf, 0, 2); buf.Endian(2, IsBE); return *( short*)ptr; }
        public ushort ReadUInt16Endian(bool IsBE) { CR(); stream.Read(buf, 0, 2); buf.Endian(2, IsBE); return *(ushort*)ptr; }
        public    int  ReadInt32Endian(bool IsBE) { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *(   int*)ptr; }
        public   uint ReadUInt32Endian(bool IsBE) { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *(  uint*)ptr; }
        public   long  ReadInt64Endian(bool IsBE) { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *(  long*)ptr; }
        public  ulong ReadUInt64Endian(bool IsBE) { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *( ulong*)ptr; }
        public  float ReadSingleEndian(bool IsBE) { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *( float*)ptr; }
        public double ReadDoubleEndian(bool IsBE) { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *(double*)ptr; }

        public void Write(byte[] Val                        ) => stream.Write(Val,      0, Val.Length);
        public void Write(byte[] Val,             int Length) => stream.Write(Val,      0,     Length);
        public void Write(byte[] Val, int Offset, int Length) => stream.Write(Val, Offset,     Length);
        public void Write(char[] val, bool UTF8 = true) => Write(UTF8 ? val.ToUTF8() : val.ToASCII());

        public void WriteByte(byte val) => stream.WriteByte(val);

        public void Write(  bool val) => stream.WriteByte((byte)(val ? 1 : 0));
        public void Write( sbyte val) => stream.WriteByte((byte) val);
        public void Write(  byte val) => stream.WriteByte(       val);
        public void Write( short val) { CW(); *( short*)ptr = val; stream.Write(buf, 0, 2); }
        public void Write(ushort val) { CW(); *(ushort*)ptr = val; stream.Write(buf, 0, 2); }
        public void Write(   int val) { CW(); *(   int*)ptr = val; stream.Write(buf, 0, 4); }
        public void Write(  uint val) { CW(); *(  uint*)ptr = val; stream.Write(buf, 0, 4); }
        public void Write(  long val) { CW(); *(  long*)ptr = val; stream.Write(buf, 0, 8); }
        public void Write( ulong val) { CW(); *( ulong*)ptr = val; stream.Write(buf, 0, 8); }
        public void Write( float val) { CW(); *( float*)ptr = val; stream.Write(buf, 0, 4); }
        public void Write(double val) { CW(); *(double*)ptr = val; stream.Write(buf, 0, 8); }

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

        public void Write(  char val, bool UTF8 = true) =>
            Write(UTF8 ? val.ToString().ToUTF8() : val.ToString().ToASCII());
        public void Write(string val, bool UTF8 = true) =>
            Write(UTF8 ? val           .ToUTF8() : val           .ToASCII());
        
        public void WriteEndian( short val) { CW(); *( short*)ptr = val; buf.Endian(2, IsBE); stream.Write(buf, 0, 2); }
        public void WriteEndian(ushort val) { CW(); *(ushort*)ptr = val; buf.Endian(2, IsBE); stream.Write(buf, 0, 2); }
        public void WriteEndian(   int val) { CW(); *(   int*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WriteEndian(  uint val) { CW(); *(  uint*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WriteEndian(  long val) { CW(); *(  long*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }
        public void WriteEndian( ulong val) { CW(); *( ulong*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }
        public void WriteEndian( float val) { CW(); *( float*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WriteEndian(double val) { CW(); *(double*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }

        public void WriteEndian( short val, bool IsBE)
        { CW(); *( short*)ptr = val; buf.Endian(2, IsBE); stream.Write(buf, 0, 2); }
        public void WriteEndian(ushort val, bool IsBE)
        { CW(); *(ushort*)ptr = val; buf.Endian(2, IsBE); stream.Write(buf, 0, 2); }
        public void WriteEndian(   int val, bool IsBE)
        { CW(); *(   int*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WriteEndian(  uint val, bool IsBE)
        { CW(); *(  uint*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WriteEndian(  long val, bool IsBE)
        { CW(); *(  long*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }
        public void WriteEndian( ulong val, bool IsBE)
        { CW(); *( ulong*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }
        public void WriteEndian( float val, bool IsBE)
        { CW(); *( float*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WriteEndian(double val, bool IsBE)
        { CW(); *(double*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }
        

        public Half ReadHalf      (         ) { ushort a = ReadUInt16      (    ); return (Half)a; }
        public Half ReadHalfEndian(         ) { ushort a = ReadUInt16Endian(    ); return (Half)a; }
        public Half ReadHalfEndian(bool IsBE) { ushort a = ReadUInt16Endian(IsBE); return (Half)a; }

        public void Write      (  Half val           ) => Write      ( (ushort ) val      );
        public void WriteEndian(  Half val           ) => WriteEndian( (ushort ) val      );
        public void WriteEndian(  Half val, bool IsBE) => WriteEndian( (ushort ) val, IsBE);
        
        public char ReadChar(bool UTF8 = true) => UTF8 ? ReadCharUTF8() : (char)stream.ReadByte();

        public char ReadCharUTF8()
        {
            byte t;
            int T;
            int val = 0;
            for (I = 0, i = 4; I < i; I++)
            {
                T = stream.ReadByte();
                if (T == -1) return '\uFFFF';
                t = (byte)T;

                     if ((t & 0xC0) == 0x80 && I >  0)   val = (val << 6) | (t & 0x3F);
                else if ((t & 0x80) == 0x00 && I == 0)   return (char)t;
                else if ((t & 0xE0) == 0xC0 && I == 0) { val = t & 0x1F; i = 2; }
                else if ((t & 0xF0) == 0xE0 && I == 0) { val = t & 0x0F; i = 3; }
                else if ((t & 0xF8) == 0xF0 && I == 0) { val = t & 0x07; i = 4; }
                else return '\uFFFF';
            }
            return (char)val;
        }

        public string ReadString(long Length, bool UTF8 = true) =>
            UTF8 ? ReadStringUTF8(Length) : ReadStringASCII(Length);

        public string ReadStringUTF8 (long Length) => ReadBytes(Length).ToUTF8 ();
        public string ReadStringASCII(long Length) => ReadBytes(Length).ToASCII();
        
        
        public string ReadString(long? Length, bool UTF8 = true) =>
            UTF8 ? ReadStringUTF8(Length) : ReadStringASCII(Length);

        public string ReadStringUTF8 (long? Length) => ReadBytes(Length).ToUTF8 ();
        public string ReadStringASCII(long? Length) => ReadBytes(Length).ToASCII();
        
        public byte[] ReadBytes(long Length, int Offset = -1)
        { byte[] Buf = new byte[Length]; if (Offset > -1) stream.Position = Offset;
            stream.Read(Buf, 0, (int)Length); return Buf; }
        
        public void ReadBytes(long Length, byte[] Buf, long Offset = -1)
        { if (Offset > -1) stream.Position = Offset; stream.Read(Buf, 0, (int)Length); }

        public byte[] ReadBytes(long? Length, int Offset = -1)
        { if (Length == null) return new byte[0]; else return ReadBytes((long)Length, Offset); }

        public void ReadBytes(long  Length, byte Bits, byte[] Buf, long Offset = -1)
        { if (Offset > -1) stream.Seek(Offset, 0);
                 if (Bits > 0 && Bits < 8) for (i = 0; i < Length; i++) Buf[i] = ReadBits(Bits); }
           
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
        
        public void Write(int val, byte Bits)
        {
            val &= (1 << Bits) - 1;
            BitWrite += Bits;
            TempBitWrite = 8 - BitWrite;
            if (TempBitWrite < 0)
            {
                BitWrite = (byte)-TempBitWrite;
                TempBitWrite = 8 + TempBitWrite;
                stream.WriteByte((byte)(ValWrite | (val >> BitWrite)));
                ValWrite = 0;
            }
            ValWrite |= val << TempBitWrite;
            ValWrite &= 0xFF;
        }

        public void CR() //CheckRead
        { CFUTRM(); if (BitRead  > 0)                                     ValRead  = 0; BitRead  = 8;   }
        public void CW() //CheckWrite
        { CFUTRM(); if (BitWrite > 0) { stream.WriteByte((byte)ValWrite); ValWrite = 0; BitWrite = 0; } }

        public byte[] ToArray(bool Close)
        { byte[] Data = ToArray(); if (Close) this.Close(); return Data; }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        [System.Security.SecurityCritical]
        private void CFUTRM() //CheckForUnableToReadMemory
        { ptr = buf.GetPtr(); }

        public byte[] ToArray()
        {
            long Position = stream.Position;
            byte[] Data = ReadBytes(stream.Length, 0);
            stream.Position = Position;
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
