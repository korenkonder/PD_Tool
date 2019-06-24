using System;
using KKdMainLib.Types;

namespace KKdMainLib.MessagePack
{
    public struct MsgPack : IDisposable
    {
        public string Name;
        public object Object;
        
        public         object[] Array => Object is         object[] List ?
                List : null;
        public KKdList<object>   List => Object is KKdList<object>  List ?
                List : default(KKdList<object>);

        public static MsgPack New => new MsgPack { Object = KKdList<object>.New };
        public static MsgPack NewReserve(int Capacity) =>
            new MsgPack { Object = KKdList<object>.NewReserve(Capacity) };

        public MsgPack(            string Name = null)
        { Object = KKdList<object>.New; this.Name = Name; }

        public MsgPack(long Count, string Name = null)
        { if (Count > 0) { Object = new object[Count]; }
          else             Object = null; this.Name = Name; }

        public MsgPack(string Name, object Object)
        { this.Name = Name; this.Object = Object; }

        public static MsgPack Null => new MsgPack();

        public object this[int index]
        {   get { if (Object is object[] Array) return Array[index]; return null; }
            set { if (Object is object[] Array) {      Array[index] = value; Object = Array; }} }
        
        public MsgPack Add(object obj)
        { if (obj != null && Object is KKdList<object> List)
            { List.Add(obj); Object = List; } return this; }

        public void Dispose()
        { Name = null; Object = null; }

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
        { if (val != null) Add(Val, ( sbyte)val); return this; }
        public MsgPack Add(string Val,   byte? val)
        { if (val != null) Add(Val, (  byte)val); return this; }
        public MsgPack Add(string Val,  short? val)
        { if (val != null) Add(Val, ( short)val); return this; }
        public MsgPack Add(string Val, ushort? val)
        { if (val != null) Add(Val, (ushort)val); return this; }
        public MsgPack Add(string Val,    int? val)
        { if (val != null) Add(Val, (   int)val); return this; }
        public MsgPack Add(string Val,   uint? val)
        { if (val != null) Add(Val, (  uint)val); return this; }
        public MsgPack Add(string Val,   long? val)
        { if (val != null) Add(Val, (  long)val); return this; }
        public MsgPack Add(string Val,  ulong? val)
        { if (val != null) Add(Val, ( ulong)val); return this; }
        public MsgPack Add(string Val,  float? val)
        { if (val != null) Add(Val, ( float)val); return this; }
        public MsgPack Add(string Val, double? val)
        { if (val != null) Add(Val, (double)val); return this; }

        public MsgPack Add(string Val, byte[] val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val, string val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val,   bool val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val,  sbyte val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val,   byte val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val,  short val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val, ushort val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val,    int val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val,   uint val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val,   long val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val,  ulong val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val,  float val) => Add(new MsgPack(Val, val));
        public MsgPack Add(string Val, double val) => Add(new MsgPack(Val, val));
        
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
        {        if (Object is   bool Boolean) return Boolean; return null; }
        public  sbyte?    ReadNInt8()
        {        if (Object is  sbyte   Int8 ) return         Int8;
            else if (Object is   byte  UInt8 ) return (sbyte)UInt8; return null; }
        public   byte?   ReadNUInt8()
        {        if (Object is  sbyte   Int8 ) return (byte) Int8;
            else if (Object is   byte  UInt8 ) return       UInt8; return null; }
        public  short?   ReadNInt16()
        {        if (Object is  sbyte   Int8 ) return         Int8 ;
            else if (Object is   byte  UInt8 ) return        UInt8 ;
            else if (Object is  short   Int16) return         Int16;
            else if (Object is ushort  UInt16) return (short)UInt16; return null; }
        public ushort?  ReadNUInt16()
        {        if (Object is  sbyte   Int8 ) return (ushort) Int8 ;
            else if (Object is   byte  UInt8 ) return         UInt8 ;
            else if (Object is  short   Int16) return (ushort) Int16;
            else if (Object is ushort  UInt16) return         UInt16; return null; }
        public    int?   ReadNInt32()
        {        if (Object is  sbyte   Int8 ) return       Int8 ;
            else if (Object is   byte  UInt8 ) return      UInt8 ;
            else if (Object is  short   Int16) return       Int16;
            else if (Object is ushort  UInt16) return      UInt16;
            else if (Object is    int   Int32) return       Int32;
            else if (Object is   uint  UInt32) return (int)UInt32; return null; }
        public   uint?  ReadNUInt32()
        {        if (Object is  sbyte   Int8 ) return (uint) Int8 ;
            else if (Object is   byte  UInt8 ) return       UInt8 ;
            else if (Object is  short   Int16) return (uint) Int16;
            else if (Object is ushort  UInt16) return       UInt16;
            else if (Object is    int   Int32) return (uint) Int32;
            else if (Object is   uint  UInt32) return       UInt32; return null; }
        public   long?   ReadNInt64()
        {        if (Object is  sbyte   Int8 ) return        Int8 ;
            else if (Object is   byte  UInt8 ) return       UInt8 ;
            else if (Object is  short   Int16) return        Int16;
            else if (Object is ushort  UInt16) return       UInt16;
            else if (Object is    int   Int32) return        Int32;
            else if (Object is   uint  UInt32) return       UInt32;
            else if (Object is   long   Int64) return        Int64;
            else if (Object is  ulong  UInt64) return (long)UInt64; return null; }
        public  ulong?  ReadNUInt64()
        {        if (Object is  sbyte   Int8 ) return (ulong) Int8 ;
            else if (Object is   byte  UInt8 ) return        UInt8 ;
            else if (Object is  short   Int16) return (ulong) Int16;
            else if (Object is ushort  UInt16) return        UInt16;
            else if (Object is    int   Int32) return (ulong) Int32;
            else if (Object is   uint  UInt32) return        UInt32;
            else if (Object is   long   Int64) return (ulong) Int64;
            else if (Object is  ulong  UInt64) return        UInt64; return null; }
        public  float?  ReadNSingle()
        {        if (Object is  sbyte   Int8 ) return           Int8 ;
            else if (Object is   byte  UInt8 ) return          UInt8 ;
            else if (Object is  short   Int16) return           Int16;
            else if (Object is ushort  UInt16) return          UInt16;
            else if (Object is    int   Int32) return           Int32;
            else if (Object is   uint  UInt32) return          UInt32;
            else if (Object is   long   Int64) return           Int64;
            else if (Object is  float Float32) return         Float32;
            else if (Object is double Float64) return ( float)Float64; return null; }
        public double?  ReadNDouble()
        {        if (Object is  sbyte   Int8 ) return   Int8 ;
            else if (Object is   byte  UInt8 ) return  UInt8 ;
            else if (Object is  short   Int16) return   Int16;
            else if (Object is ushort  UInt16) return  UInt16;
            else if (Object is    int   Int32) return   Int32;
            else if (Object is   uint  UInt32) return  UInt32;
            else if (Object is   long   Int64) return   Int64;
            else if (Object is  float Float32) return Float32;
            else if (Object is double Float64) return Float64; return null; }
        public string    ReadString()
        {        if (Object is string  String) return  String; return null; }

        public bool Element<T>(string Name, out MsgPack MsgPack)
        {
            if (Element(Name, out MsgPack))
            {
                if (MsgPack.Array == null) return false;

                for (int i = 0; i < MsgPack.Array.Length; i++)
                    if (!(MsgPack[i] is T)) return false;
                return true;
            }
            return false;
        }

        public bool Element(string Name, out MsgPack MsgPack)
        {
            MsgPack = New;
            if (List.IsNull) return false;

            for (int i = 0; i < List.Count; i++)
                if (List[i] is MsgPack msg) if (msg.Name == Name) { MsgPack = msg; return true; }
            return false;
        }

        public bool ContainsKey(string Name)
        {
            if (List.IsNull) return false;
            
            for (int i = 0; i < List.Count ; i++) 
                if (List[i] is MsgPack msg) if (msg.Name == Name) return true;
            return false;
        }

        public override string ToString() => Name ?? "" +
            ( List.NotNull ? ((Name != null ? " " : "") + "Elements Count: " + List .Count ) :
            (Array != null ? ((Name != null ? " " : "") + "Elements Count: " + Array.Length) : Object.ToString()));

        public struct Ext
        {
            public byte[] Data;
            public sbyte Type;
        }
    }
}
