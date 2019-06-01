using System;
using System.Linq;
using System.Collections.Generic;

namespace KKdMainLib.MessagePack
{
    public class MsgPack : IDisposable
    {
        public Types Type;
        public string Name;
        public object Object;
        
        public MsgPack(             Types Type = Types.Map32) => NewMsgPack(null, Type);
        public MsgPack(string Name, Types Type = Types.Map32) => NewMsgPack(Name, Type);
        public MsgPack(             long Count) => NewMsgPack(null, Count);
        public MsgPack(string Name, long Count) => NewMsgPack(Name, Count);
        public MsgPack(string Name, Types Type, object Object)
        { this.Name = Name; this.Type = Type; this.Object = Object; }
        public MsgPack(string Name, object Object)
        {
            switch (Object)
            {
                case List<object> val: Type = Types.  Map32; break;
                case    object[]  val: Type = Types.  Arr32; break;
                case     MsgPack  val: Type = Types.  Map32; break;
                case      byte[]  val: Type = Types.  Bin32; break;
                case        bool  val: Type = val ? Types.True : Types.False; break;
                case       sbyte  val: Type = Types.  Int8 ; break;
                case        byte  val: Type = Types. UInt8 ; break;
                case       short  val: Type = Types.  Int16; break;
                case      ushort  val: Type = Types. UInt16; break;
                case         int  val: Type = Types.  Int32; break;
                case        uint  val: Type = Types. UInt32; break;
                case        long  val: Type = Types.  Int64; break;
                case       ulong  val: Type = Types. UInt64; break;
                case       float  val: Type = Types.Float32; break;
                case      double  val: Type = Types.Float64; break;
                case      string  val: Type = Types.  Str32; break;
                case         Ext  val: Type = Types.  Ext32; break;
            }
            this.Name = Name; this.Object = Object;
        }

        public static MsgPack Null => null;

        public object this[int index]
        {   get { return ((object[])Object)[index]; }
            set { object[] Data = (object[])Object; Data[index] = value; Object = Data; } }

        public MsgPack(List<object> Object, string Name, Types Type)
        { this.Object = Object; this.Name = Name; this.Type = Type; }

        private void NewMsgPack(string Name, Types Type)
        { Object = new List<object>(); this.Name = Name; this.Type = Type; }

        private void NewMsgPack(string Name, long Count)
        { if (Count > 0) Object = new object[Count]; else Object = null; this.Name = Name; Type = Types.Arr32; }
        
        public MsgPack Add(object obj)
        { if (obj != null) if (typeof(List<object>) == Object.GetType())
                { List<object> Obj = (List<object>)Object; Obj.Add(obj); Object = Obj; } return this; }

        public MsgPack ToArray()
        { if (typeof(List<object> ) == Object.GetType())
        { Object = ((List<object> )    Object)                 .ToArray(); Type = Types.Arr32; } return this; }

        public MsgPack ToList()
        { if (typeof(     object[]) == Object.GetType())
        { Object = ((     object[])    Object).OfType<object>().ToList (); Type = Types.Map32; } return this; }

        public void Dispose()
        { Type = 0; Name = null; Object = null; }

        public MsgPack Add( sbyte? val) => Add(null, val);
        public MsgPack Add(  byte? val) => Add(null, val);
        public MsgPack Add( short? val) => Add(null, val);
        public MsgPack Add(ushort? val) => Add(null, val);
        public MsgPack Add(   int? val) => Add(null, val);
        public MsgPack Add(  uint? val) => Add(null, val);
        public MsgPack Add(  long? val) => Add(null, val);
        public MsgPack Add( ulong? val) => Add(null, val);
        public MsgPack Add( float? val) => Add(null, val);
        public MsgPack Add(double? val) => Add(null, val);
        
        public MsgPack Add(byte[]  val) => Add(null, val);
        public MsgPack Add(string  val) => Add(null, val);
        public MsgPack Add(  bool  val) => Add(null, val);
        public MsgPack Add( sbyte  val) => Add(null, val);
        public MsgPack Add(  byte  val) => Add(null, val);
        public MsgPack Add( short  val) => Add(null, val);
        public MsgPack Add(ushort  val) => Add(null, val);
        public MsgPack Add(   int  val) => Add(null, val);
        public MsgPack Add(  uint  val) => Add(null, val);
        public MsgPack Add(  long  val) => Add(null, val);
        public MsgPack Add( ulong  val) => Add(null, val);
        public MsgPack Add( float  val) => Add(null, val);
        public MsgPack Add(double  val) => Add(null, val);

        public MsgPack Add(string Val,  sbyte? val)
        { if (val == null) Add(Null); else Add(Val, ( sbyte)val); return this; }
        public MsgPack Add(string Val,   byte? val)
        { if (val == null) Add(Null); else Add(Val, (  byte)val); return this; }
        public MsgPack Add(string Val,  short? val)
        { if (val == null) Add(Null); else Add(Val, ( short)val); return this; }
        public MsgPack Add(string Val, ushort? val)
        { if (val == null) Add(Null); else Add(Val, (ushort)val); return this; }
        public MsgPack Add(string Val,    int? val)
        { if (val == null) Add(Null); else Add(Val, (   int)val); return this; }
        public MsgPack Add(string Val,   uint? val)
        { if (val == null) Add(Null); else Add(Val, (  uint)val); return this; }
        public MsgPack Add(string Val,   long? val)
        { if (val == null) Add(Null); else Add(Val, (  long)val); return this; }
        public MsgPack Add(string Val,  ulong? val)
        { if (val == null) Add(Null); else Add(Val, ( ulong)val); return this; }
        public MsgPack Add(string Val,  float? val)
        { if (val == null) Add(Null); else Add(Val, ( float)val); return this; }
        public MsgPack Add(string Val, double? val)
        { if (val == null) Add(Null); else Add(Val, (double)val); return this; }

        public MsgPack Add(string Val, byte[] val)
        { if (val == null) Add(Null); else
            Add(new MsgPack(Val, Types.  Bin32, val)); return this; }
        public MsgPack Add(string Val, string val)
        { if (val == null) Add(Null); else
            Add(new MsgPack(Val, Types.  Str32, val)); return this; }
        public MsgPack Add(string Val,   bool val)
        {   Add(new MsgPack(Val, val ? 
                    Types.True : Types.  False, val)); return this; }
        public MsgPack Add(string Val,  sbyte val)
        {   Add(new MsgPack(Val, Types.   Int8, val)); return this; }
        public MsgPack Add(string Val,   byte val)
        {   Add(new MsgPack(Val, Types.  UInt8, val)); return this; }
        public MsgPack Add(string Val,  short val)
        {   Add(new MsgPack(Val, Types.  Int16, val)); return this; }
        public MsgPack Add(string Val, ushort val)
        {   Add(new MsgPack(Val, Types. UInt16, val)); return this; }
        public MsgPack Add(string Val,    int val)
        {   Add(new MsgPack(Val, Types.  Int32, val)); return this; }
        public MsgPack Add(string Val,   uint val)
        {   Add(new MsgPack(Val, Types. UInt32, val)); return this; }
        public MsgPack Add(string Val,   long val)
        {   Add(new MsgPack(Val, Types.  Int64, val)); return this; }
        public MsgPack Add(string Val,  ulong val)
        {   Add(new MsgPack(Val, Types. UInt64, val)); return this; }
        public MsgPack Add(string Val,  float val)
        {   Add(new MsgPack(Val, Types.Float32, val)); return this; }
        public MsgPack Add(string Val, double val)
        {   Add(new MsgPack(Val, Types.Float64, val)); return this; }
        
        public   bool ReadBoolean(string Name) => ReadNBoolean(Name).GetValueOrDefault();
        public  sbyte    ReadInt8(string Name) =>    ReadNInt8(Name).GetValueOrDefault();
        public   byte   ReadUInt8(string Name) =>   ReadNUInt8(Name).GetValueOrDefault();
        public  short   ReadInt16(string Name) =>   ReadNInt16(Name).GetValueOrDefault();
        public ushort  ReadUInt16(string Name) =>  ReadNUInt16(Name).GetValueOrDefault();
        public    int   ReadInt32(string Name) =>   ReadNInt32(Name).GetValueOrDefault();
        public   uint  ReadUInt32(string Name) =>  ReadNUInt32(Name).GetValueOrDefault();
        public   long   ReadInt64(string Name) =>   ReadNInt64(Name).GetValueOrDefault();
        public  ulong  ReadUInt64(string Name) =>  ReadNUInt64(Name).GetValueOrDefault();
        public  float  ReadSingle(string Name) =>  ReadNSingle(Name).GetValueOrDefault();
        public double  ReadDouble(string Name) =>  ReadNDouble(Name).GetValueOrDefault();

        public   bool? ReadNBoolean(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack.ReadNBoolean(); return null; }
        public  sbyte?    ReadNInt8(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack.   ReadNInt8(); return null; }
        public   byte?   ReadNUInt8(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack.  ReadNUInt8(); return null; }
        public  short?   ReadNInt16(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack.  ReadNInt16(); return null; }
        public ushort?  ReadNUInt16(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack. ReadNUInt16(); return null; }
        public    int?   ReadNInt32(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack.  ReadNInt32(); return null; }
        public   uint?  ReadNUInt32(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack. ReadNUInt32(); return null; }
        public   long?   ReadNInt64(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack.  ReadNInt64(); return null; }
        public  ulong?  ReadNUInt64(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack. ReadNUInt64(); return null; }
        public  float?  ReadNSingle(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack. ReadNSingle(); return null; }
        public double?  ReadNDouble(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack. ReadNDouble(); return null; }
        public string    ReadString(string Name)
        { if (Element(Name, out MsgPack MsgPack)) return MsgPack.  ReadString(); return null; }

        public   bool ReadBoolean() => ReadNBoolean().GetValueOrDefault();
        public  sbyte    ReadInt8() =>    ReadNInt8().GetValueOrDefault();
        public   byte   ReadUInt8() =>   ReadNUInt8().GetValueOrDefault();
        public  short   ReadInt16() =>   ReadNInt16().GetValueOrDefault();
        public ushort  ReadUInt16() =>  ReadNUInt16().GetValueOrDefault();
        public    int   ReadInt32() =>   ReadNInt32().GetValueOrDefault();
        public   uint  ReadUInt32() =>  ReadNUInt32().GetValueOrDefault();
        public   long   ReadInt64() =>   ReadNInt64().GetValueOrDefault();
        public  ulong  ReadUInt64() =>  ReadNUInt64().GetValueOrDefault();
        public  float  ReadSingle() =>  ReadNSingle().GetValueOrDefault();
        public double  ReadDouble() =>  ReadNDouble().GetValueOrDefault();
        
        public   bool? ReadNBoolean()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(  bool)) return (  bool)       Object; return null; ; }
        public  sbyte?    ReadNInt8()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(  long)) return ( sbyte)( long)Object;
            else if (Object.GetType() == typeof( ulong)) return ( sbyte)(ulong)Object; return null; }
        public   byte?   ReadNUInt8()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(  long)) return (  byte)( long)Object;
            else if (Object.GetType() == typeof( ulong)) return (  byte)(ulong)Object; return null; }
        public  short?   ReadNInt16()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(  long)) return ( short)( long)Object;
            else if (Object.GetType() == typeof( ulong)) return ( short)(ulong)Object; return null; }
        public ushort?  ReadNUInt16()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(  long)) return (ushort)( long)Object;
            else if (Object.GetType() == typeof( ulong)) return (ushort)(ulong)Object; return null; }
        public    int?   ReadNInt32()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(  long)) return (   int)( long)Object;
            else if (Object.GetType() == typeof( ulong)) return (   int)(ulong)Object; return null; }
        public   uint?  ReadNUInt32()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(  long)) return (  uint)( long)Object;
            else if (Object.GetType() == typeof( ulong)) return (  uint)(ulong)Object; return null; }
        public   long?   ReadNInt64()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(  long)) return (  long)       Object;
            else if (Object.GetType() == typeof( ulong)) return (  long)(ulong)Object; return null; }
        public  ulong?  ReadNUInt64()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof( ulong)) return ( ulong)       Object;
            else if (Object.GetType() == typeof(  long)) return ( ulong)( long)Object; return null; }
        public  float?  ReadNSingle()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(double)) return (float)(double)Object;
            else if (Object.GetType() == typeof( float)) return (float)        Object;
            else if (Object.GetType() == typeof(  long)) return (float)(  long)Object; 
            else if (Object.GetType() == typeof( ulong)) return (float)( ulong)Object; return null; }
        public double?  ReadNDouble()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(double)) return (double)       Object;
            else if (Object.GetType() == typeof( float)) return (double)(float)Object;
            else if (Object.GetType() == typeof(  long)) return (double)( long)Object; 
            else if (Object.GetType() == typeof( ulong)) return (double)(ulong)Object; return null; }
        public string    ReadString()
        { if (Object == null) return null;
                 if (Object.GetType() == typeof(string)) return (string)       Object; return null; }
        
        public bool Element(string Name, out MsgPack MsgPack, Type Type)
        { if (Element(out MsgPack, Name)) return MsgPack.Object.GetType() == Type; return false; }

        public bool Element(string Name, out MsgPack MsgPack)
        { if (Element(out MsgPack, Name)) return MsgPack                  != null; return false; }

        public bool Element(out MsgPack MsgPack, string Name)
        {
            MsgPack = null;
            if (Object == null) return false;

            Type type = Object.GetType();
            if (type == typeof(List<object>))
            {
                List<object> Obj = (List<object>)Object;
                foreach (object obj in Obj)
                {
                    if (obj == null) continue; type = obj.GetType();
                    if (type == typeof(MsgPack)) if (((MsgPack)obj).Name == Name)
                        { MsgPack = (MsgPack)obj; return true; }
                }
            }
            else if (type == typeof(object[]))
            {
                object[] Obj = (object[])Object;
                foreach (object obj in Obj)
                {
                    if (obj == null) continue; type = obj.GetType();
                    if (type == typeof(MsgPack)) if (((MsgPack)obj).Name == Name)
                        { MsgPack = (MsgPack)obj; return true; }
                }
            }
            return false;
        }

        public bool ContainsKey(string Name)
        {
            if (Object == null) return false;

            Type type = Object.GetType();
            if (type == typeof(List<object>))
            {
                List<object> Obj = (List<object>)Object;
                foreach (object obj in Obj)
                {
                    if (obj == null) continue; type = obj.GetType();
                    if (type == typeof(MsgPack)) if (((MsgPack)obj).Name == Name) return true;
                }
            }
            return false;
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

        public struct Ext
        {
            public byte[] Data;
            public sbyte Type;
        }
    }
}
