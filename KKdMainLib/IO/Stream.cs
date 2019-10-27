using System;
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

        public  int    O { get => ( int)I64O; set => I64O = value; }
        public uint U32O { get => (uint)I64O; set => I64O = value; }
        public long I64O;

        public  int    L { get => ( int)stream.Length -    O; set => stream.SetLength(value +    O); }
        public uint U32L { get => (uint)stream.Length - U32O; set => stream.SetLength(value + U32O); }
        public long I64L { get =>       stream.Length - I64O; set => stream.SetLength(value + I64O); }

        public  int    P
        { get => ( int)stream.Position -    O; set => stream.Position = value +     O; }
        public uint U32P
        { get => (uint)stream.Position - U32O; set => stream.Position = value + U32O; }
        public long I64P
        { get =>       stream.Position - I64O; set => stream.Position = value + I64O; }

        public bool CanRead    => stream.CanRead;
        public bool CanSeek    => stream.CanSeek;
        public bool CanTimeout => stream.CanTimeout;
        public bool CanWrite   => stream.CanWrite;

        public string File = null;

        public Stream(MSIO.Stream output = null, bool isBE = false)
        {
            if (output == null) output = MSIO.Stream.Null;
            I64O = 0;
            BitRead = 8;
            ValRead = ValRead = BitWrite = 0;
            stream = output;
            Format = Format.NULL;
            buf = new byte[0x100];
            ptr = buf.GetPtr();
            IsBE = isBE;
        }

        public void C() => D(true);

        public void F() => stream.Flush();

        public void SL(long length = 0) => stream.SetLength(length);

        public long S(long offset, SeekOrigin origin = 0) =>
            stream.Seek(offset, (MSIO.SeekOrigin)(int)origin);
        
        public long? S(long? offset, SeekOrigin origin)
        { if (offset == null) return null; return stream.Seek((long)offset, (MSIO.SeekOrigin)(int)origin); }

        private bool disposed = false;
        public void D() => D(true);
        public void Dispose() => D(true);

        protected virtual void D(bool dispose)
        { CW(); if (stream != MSIO.Stream.Null && !disposed) { disposed = true; stream.Flush();
                stream.Dispose(); } if (dispose) GC.SuppressFinalize(this); }

        public MSIO.Stream BS
        { get { stream.Flush(); return stream; } set { stream = value; } }

        public void A(long Align)
        {
            long Al = Align - P % Align;
            if (P % Align != 0) stream.Seek(P + O + Al, 0);
        }

        public void A(long Align, bool SetLength)
        {
            if (SetLength) stream.SetLength(P + O);
            long Al = Align - P % Align;
            if (P % Align != 0) stream.Seek(P + O + Al, 0);
            if (SetLength) stream.SetLength(P + O);
        }

        public void A(long Align, bool SetLength0, bool SetLength1)
        {
            if (SetLength0) stream.SetLength(P + O);
            long Al = Align - P % Align;
            if (P % Align != 0) stream.Seek(P + Al, 0);
            if (SetLength1) stream.SetLength(P + O);
        }

        public   bool RBo () =>        stream.ReadByte() != 0;
        public  sbyte RI8 () => (sbyte)stream.ReadByte();
        public   byte RU8 () => ( byte)stream.ReadByte();
        public  short RI16() { CR(); stream.Read(buf, 0, 2); return *( short*)ptr; }
        public ushort RU16() { CR(); stream.Read(buf, 0, 2); return *(ushort*)ptr; }
        public    int RI32() { CR(); stream.Read(buf, 0, 4); return *(   int*)ptr; }
        public   uint RU32() { CR(); stream.Read(buf, 0, 4); return *(  uint*)ptr; }
        public   long RI64() { CR(); stream.Read(buf, 0, 8); return *(  long*)ptr; }
        public  ulong RU64() { CR(); stream.Read(buf, 0, 8); return *( ulong*)ptr; }
        public  float RF32() { CR(); stream.Read(buf, 0, 4); return *( float*)ptr; }
        public double RF64() { CR(); stream.Read(buf, 0, 8); return *(double*)ptr; }
        
        public  short RI16E() { CR(); stream.Read(buf, 0, 2); buf.Endian(2, IsBE); return *( short*)ptr; }
        public ushort RU16E() { CR(); stream.Read(buf, 0, 2); buf.Endian(2, IsBE); return *(ushort*)ptr; }
        public    int RI32E() { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *(   int*)ptr; }
        public   uint RU32E() { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *(  uint*)ptr; }
        public   long RI64E() { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *(  long*)ptr; }
        public  ulong RU64E() { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *( ulong*)ptr; }
        public  float RF32E() { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *( float*)ptr; }
        public double RF64E() { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *(double*)ptr; }

        public  short RI16E(bool IsBE) { CR(); stream.Read(buf, 0, 2); buf.Endian(2, IsBE); return *( short*)ptr; }
        public ushort RU16E(bool IsBE) { CR(); stream.Read(buf, 0, 2); buf.Endian(2, IsBE); return *(ushort*)ptr; }
        public    int RI32E(bool IsBE) { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *(   int*)ptr; }
        public   uint RU32E(bool IsBE) { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *(  uint*)ptr; }
        public   long RI64E(bool IsBE) { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *(  long*)ptr; }
        public  ulong RU64E(bool IsBE) { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *( ulong*)ptr; }
        public  float RF32E(bool IsBE) { CR(); stream.Read(buf, 0, 4); buf.Endian(4, IsBE); return *( float*)ptr; }
        public double RF64E(bool IsBE) { CR(); stream.Read(buf, 0, 8); buf.Endian(8, IsBE); return *(double*)ptr; }

        public void W(byte[] Val                        ) => stream.Write(Val ?? new byte[0],      0, Val.Length);
        public void W(byte[] Val,             int Length) => stream.Write(Val ?? new byte[0],      0,     Length);
        public void W(byte[] Val, int Offset, int Length) => stream.Write(Val ?? new byte[0], Offset,     Length);
        public void W(char[] val, bool UTF8 = true) => W(UTF8 ? val.ToUTF8() : val.ToASCII());

        public void W(  bool val) => stream.WriteByte((byte)(val ? 1 : 0));
        public void W( sbyte val) => stream.WriteByte((byte) val);
        public void W(  byte val) => stream.WriteByte(       val);
        public void W( short val) { CW(); *( short*)ptr = val; stream.Write(buf, 0, 2); }
        public void W(ushort val) { CW(); *(ushort*)ptr = val; stream.Write(buf, 0, 2); }
        public void W(   int val) { CW(); *(   int*)ptr = val; stream.Write(buf, 0, 4); }
        public void W(  uint val) { CW(); *(  uint*)ptr = val; stream.Write(buf, 0, 4); }
        public void W(  long val) { CW(); *(  long*)ptr = val; stream.Write(buf, 0, 8); }
        public void W( ulong val) { CW(); *( ulong*)ptr = val; stream.Write(buf, 0, 8); }
        public void W( float val) { CW(); *( float*)ptr = val; stream.Write(buf, 0, 4); }
        public void W(double val) { CW(); *(double*)ptr = val; stream.Write(buf, 0, 8); }

        public void W( sbyte? val) => W(val ?? default);
        public void W(  byte? val) => W(val ?? default);
        public void W( short? val) => W(val ?? default);
        public void W(ushort? val) => W(val ?? default);
        public void W(   int? val) => W(val ?? default);
        public void W(  uint? val) => W(val ?? default);
        public void W(  long? val) => W(val ?? default);
        public void W( ulong? val) => W(val ?? default);
        public void W( float? val) => W(val ?? default);
        public void W(double? val) => W(val ?? default);

        public void W(  char val, bool UTF8 = true) => W(UTF8 ? val.ToString().ToUTF8() : val.ToString().ToASCII());
        public void W(string val, bool UTF8 = true) => W(UTF8 ? val           .ToUTF8() : val           .ToASCII());
        
        public void WE( short val) { CW(); *( short*)ptr = val; buf.Endian(2, IsBE); stream.Write(buf, 0, 2); }
        public void WE(ushort val) { CW(); *(ushort*)ptr = val; buf.Endian(2, IsBE); stream.Write(buf, 0, 2); }
        public void WE(   int val) { CW(); *(   int*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WE(  uint val) { CW(); *(  uint*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WE(  long val) { CW(); *(  long*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }
        public void WE( ulong val) { CW(); *( ulong*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }
        public void WE( float val) { CW(); *( float*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WE(double val) { CW(); *(double*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }

        public void WE( short val, bool IsBE) { CW(); *( short*)ptr = val; buf.Endian(2, IsBE); stream.Write(buf, 0, 2); }
        public void WE(ushort val, bool IsBE) { CW(); *(ushort*)ptr = val; buf.Endian(2, IsBE); stream.Write(buf, 0, 2); }
        public void WE(   int val, bool IsBE) { CW(); *(   int*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WE(  uint val, bool IsBE) { CW(); *(  uint*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WE(  long val, bool IsBE) { CW(); *(  long*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }
        public void WE( ulong val, bool IsBE) { CW(); *( ulong*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }
        public void WE( float val, bool IsBE) { CW(); *( float*)ptr = val; buf.Endian(4, IsBE); stream.Write(buf, 0, 4); }
        public void WE(double val, bool IsBE) { CW(); *(double*)ptr = val; buf.Endian(8, IsBE); stream.Write(buf, 0, 8); }
        

        public Half RF16 (         ) { ushort a = RU16 (    ); return (Half)a; }
        public Half RF16E(         ) { ushort a = RU16E(    ); return (Half)a; }
        public Half RF16E(bool IsBE) { ushort a = RU16E(IsBE); return (Half)a; }

        public void W (Half val           ) => W ((ushort)val      );
        public void WE(Half val           ) => WE((ushort)val      );
        public void WE(Half val, bool IsBE) => WE((ushort)val, IsBE);
        
        public char RC(bool UTF8 = true) => UTF8 ? RCUTF8() : (char)stream.ReadByte();

        public char RCUTF8()
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

        public string RS(long Length, bool UTF8 = true) =>
            UTF8 ? RSUTF8(Length) : RSASCII(Length);

        public string RSUTF8 (long Length) => RBy(Length).ToUTF8 ();
        public string RSASCII(long Length) => RBy(Length).ToASCII();
        
        
        public string RS(long? Length, bool UTF8 = true) =>
            UTF8 ? RSUTF8(Length) : RSASCII(Length);

        public string RSUTF8 (long? Length) => RBy(Length).ToUTF8 ();
        public string RSASCII(long? Length) => RBy(Length).ToASCII();
        
        public byte[] RBy(long Length, int Offset = -1)
        { byte[] Buf = new byte[Length]; if (Offset > -1) stream.Position = Offset;
            stream.Read(Buf, 0, (int)Length); return Buf; }
        
        public void RBy(long Length, byte[] Buf, long Offset = -1)
        { if (Offset > -1) stream.Position = Offset; stream.Read(Buf, 0, (int)Length); }

        public byte[] RBy(long? Length, int Offset = -1)
        { if (Length == null) return new byte[0]; else return RBy((long)Length, Offset); }

        public void RBy(long  Length, byte Bits, byte[] Buf, long Offset = -1)
        { if (Offset > -1) stream.Seek(Offset, 0);
                 if (Bits > 0 && Bits < 8) for (i = 0; i < Length; i++) Buf[i] = RBi(Bits); }
           
        public byte RBi(byte Bits)
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

        public byte RHB() => RBi(4);
        
        public void W(int val, byte Bits)
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
        { byte[] Data = ToArray(); if (Close) Dispose(); return Data; }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        [System.Security.SecuritySafeCritical]
        private void CFUTRM() //CheckForUnableToReadMemory
        { try { if (buf[0] != ptr[0] || buf[1] != ptr[1] || buf[2] != ptr[2] || buf[3] != ptr[3] ||
                    buf[4] != ptr[4] || buf[5] != ptr[5] || buf[6] != ptr[6] || buf[7] != ptr[7])
                    ptr = buf.GetPtr(); } catch (AccessViolationException) { ptr = buf.GetPtr(); } }

        public byte[] ToArray()
        {
            long Position = stream.Position;
            byte[] Data = RBy(stream.Length, 0);
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
