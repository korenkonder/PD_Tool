using System;
using KKdMainLib.Types;

namespace KKdMainLib.MessagePack
{
    public struct MsgPack : IDisposable, IEquatable<MsgPack>
    {
        public string Name;
        public object Object;
        
        public         MsgPack[] Array => Object is         MsgPack[] List ?
                List : null;
        public KKdList<MsgPack>   List => Object is KKdList<MsgPack>  List ?
                List : default(KKdList<MsgPack>);

        public static MsgPack New => new MsgPack { Object = KKdList<MsgPack>.New };
        public static MsgPack NewReserve(int Capacity) =>
            new MsgPack { Object = KKdList<MsgPack>.NewReserve(Capacity) };

        public MsgPack(            string Name = null)
        { Object = KKdList<MsgPack>.New; this.Name = Name; }

        public MsgPack(long Count, string Name = null)
        { if (Count > 0) { Object = new MsgPack[Count]; }
          else             Object = null; this.Name = Name; }

        public MsgPack(string Name, object Object)
        { this.Name = Name; this.Object = Object; }

        public static MsgPack Null => new MsgPack();

        public MsgPack this[int index]
        {   get { if (Object is MsgPack[] Array) return Array[index]; return default(MsgPack); }
            set { if (Object is MsgPack[] Array) {      Array[index] = value;  Object = Array; } } }
        
        public MsgPack Add(MsgPack obj)
        { if (Object is KKdList<MsgPack> List) { List.Add(obj); Object = List; } return this; }

        public void Dispose()
        { Name = null; Object = null; }

        public bool Equals(MsgPack msg) =>
            Name == msg.Name && Object == msg.Object;

        public override string ToString() => Name ?? "" +
            (List. NotNull ? ((Name != null ? " " : "") + "Elements Count: " + List .Count ) :
            (Array != null ? ((Name != null ? " " : "") + "Elements Count: " + Array.Length) : Object.ToString()));

        public static explicit operator MsgPack(byte[] val) => new MsgPack(null, val);
        public static explicit operator MsgPack(string val) => new MsgPack(null, val);
        public static explicit operator MsgPack( sbyte val) => new MsgPack(null, val);
        public static explicit operator MsgPack(  byte val) => new MsgPack(null, val);
        public static explicit operator MsgPack( short val) => new MsgPack(null, val);
        public static explicit operator MsgPack(ushort val) => new MsgPack(null, val);
        public static explicit operator MsgPack(   int val) => new MsgPack(null, val);
        public static explicit operator MsgPack(  uint val) => new MsgPack(null, val);
        public static explicit operator MsgPack(  long val) => new MsgPack(null, val);
        public static explicit operator MsgPack( ulong val) => new MsgPack(null, val);
        public static explicit operator MsgPack( float val) => new MsgPack(null, val);
        public static explicit operator MsgPack(double val) => new MsgPack(null, val);

        public MsgPack Add( sbyte? val) => val != null ? Add(new MsgPack(null, ( sbyte)val)) : this;
        public MsgPack Add(  byte? val) => val != null ? Add(new MsgPack(null, (  byte)val)) : this;
        public MsgPack Add( short? val) => val != null ? Add(new MsgPack(null, ( short)val)) : this;
        public MsgPack Add(ushort? val) => val != null ? Add(new MsgPack(null, (ushort)val)) : this;
        public MsgPack Add(   int? val) => val != null ? Add(new MsgPack(null, (   int)val)) : this;
        public MsgPack Add(  uint? val) => val != null ? Add(new MsgPack(null, (  uint)val)) : this;
        public MsgPack Add(  long? val) => val != null ? Add(new MsgPack(null, (  long)val)) : this;
        public MsgPack Add( ulong? val) => val != null ? Add(new MsgPack(null, ( ulong)val)) : this;
        public MsgPack Add( float? val) => val != null ? Add(new MsgPack(null, ( float)val)) : this;
        public MsgPack Add(double? val) => val != null ? Add(new MsgPack(null, (double)val)) : this;
        
        public MsgPack Add(byte[]  val) => Add(new MsgPack(null, val));
        public MsgPack Add(string  val) => Add(new MsgPack(null, val));
        public MsgPack Add(  bool  val) => Add(new MsgPack(null, val));
        public MsgPack Add( sbyte  val) => Add(new MsgPack(null, val));
        public MsgPack Add(  byte  val) => Add(new MsgPack(null, val));
        public MsgPack Add( short  val) => Add(new MsgPack(null, val));
        public MsgPack Add(ushort  val) => Add(new MsgPack(null, val));
        public MsgPack Add(   int  val) => Add(new MsgPack(null, val));
        public MsgPack Add(  uint  val) => Add(new MsgPack(null, val));
        public MsgPack Add(  long  val) => Add(new MsgPack(null, val));
        public MsgPack Add( ulong  val) => Add(new MsgPack(null, val));
        public MsgPack Add( float  val) => Add(new MsgPack(null, val));
        public MsgPack Add(double  val) => Add(new MsgPack(null, val));
        

        public MsgPack Add(string Val,  byte[] val) => val != null ? Add(new MsgPack(Val, val)) : this;
        public MsgPack Add(string Val,  string val) => val != null ? Add(new MsgPack(Val, val)) : this;
        public MsgPack Add(string Val,  sbyte? val) => val != null ? Add(Val, ( sbyte)val) : this;
        public MsgPack Add(string Val,   byte? val) => val != null ? Add(Val, (  byte)val) : this;
        public MsgPack Add(string Val,  short? val) => val != null ? Add(Val, ( short)val) : this;
        public MsgPack Add(string Val, ushort? val) => val != null ? Add(Val, (ushort)val) : this;
        public MsgPack Add(string Val,    int? val) => val != null ? Add(Val, (   int)val) : this;
        public MsgPack Add(string Val,   uint? val) => val != null ? Add(Val, (  uint)val) : this;
        public MsgPack Add(string Val,   long? val) => val != null ? Add(Val, (  long)val) : this;
        public MsgPack Add(string Val,  ulong? val) => val != null ? Add(Val, ( ulong)val) : this;
        public MsgPack Add(string Val,  float? val) => val != null ? Add(Val, ( float)val) : this;
        public MsgPack Add(string Val, double? val) => val != null ? Add(Val, (double)val) : this;

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

        public   bool? ReadNBoolean(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack.ReadNBoolean() : null;
        public  sbyte?    ReadNInt8(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack.  ReadNInt8 () : null;
        public   byte?   ReadNUInt8(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack. ReadNUInt8 () : null;
        public  short?   ReadNInt16(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack.  ReadNInt16() : null;
        public ushort?  ReadNUInt16(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack. ReadNUInt16() : null;
        public    int?   ReadNInt32(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack.  ReadNInt32() : null;
        public   uint?  ReadNUInt32(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack. ReadNUInt32() : null;
        public   long?   ReadNInt64(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack.  ReadNInt64() : null;
        public  ulong?  ReadNUInt64(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack. ReadNUInt64() : null;
        public  float?  ReadNSingle(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack. ReadNSingle() : null;
        public double?  ReadNDouble(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack. ReadNDouble() : null;
        public string    ReadString(string Name) =>
            Element(Name, out MsgPack MsgPack) ? MsgPack.  ReadString() : null;

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
        {        if (Object is   bool Boolean) return         Boolean; return null; }
        public  sbyte?    ReadNInt8()
        {        if (Object is  sbyte   Int8 ) return           Int8 ;
            else if (Object is   byte  UInt8 ) return ( sbyte) UInt8 ; return null; }
        public   byte?   ReadNUInt8()
        {        if (Object is  sbyte   Int8 ) return (  byte)  Int8 ;
            else if (Object is   byte  UInt8 ) return          UInt8 ; return null; }
        public  short?   ReadNInt16()
        {        if (Object is  sbyte   Int8 ) return           Int8 ;
            else if (Object is   byte  UInt8 ) return          UInt8 ;
            else if (Object is  short   Int16) return           Int16;
            else if (Object is ushort  UInt16) return ( short) UInt16; return null; }
        public ushort?  ReadNUInt16()
        {        if (Object is  sbyte   Int8 ) return (ushort)  Int8 ;
            else if (Object is   byte  UInt8 ) return          UInt8 ;
            else if (Object is  short   Int16) return (ushort)  Int16;
            else if (Object is ushort  UInt16) return          UInt16; return null; }
        public    int?   ReadNInt32()
        {        if (Object is  sbyte   Int8 ) return           Int8 ;
            else if (Object is   byte  UInt8 ) return          UInt8 ;
            else if (Object is  short   Int16) return           Int16;
            else if (Object is ushort  UInt16) return          UInt16;
            else if (Object is    int   Int32) return           Int32;
            else if (Object is   uint  UInt32) return (   int) UInt32; return null; }
        public   uint?  ReadNUInt32()
        {        if (Object is  sbyte   Int8 ) return (  uint)  Int8 ;
            else if (Object is   byte  UInt8 ) return          UInt8 ;
            else if (Object is  short   Int16) return (  uint)  Int16;
            else if (Object is ushort  UInt16) return          UInt16;
            else if (Object is    int   Int32) return (  uint)  Int32;
            else if (Object is   uint  UInt32) return          UInt32; return null; }
        public   long?   ReadNInt64()
        {        if (Object is  sbyte   Int8 ) return           Int8 ;
            else if (Object is   byte  UInt8 ) return          UInt8 ;
            else if (Object is  short   Int16) return           Int16;
            else if (Object is ushort  UInt16) return          UInt16;
            else if (Object is    int   Int32) return           Int32;
            else if (Object is   uint  UInt32) return          UInt32;
            else if (Object is   long   Int64) return           Int64;
            else if (Object is  ulong  UInt64) return (  long) UInt64; return null; }
        public  ulong?  ReadNUInt64()
        {        if (Object is  sbyte   Int8 ) return ( ulong)  Int8 ;
            else if (Object is   byte  UInt8 ) return          UInt8 ;
            else if (Object is  short   Int16) return ( ulong)  Int16;
            else if (Object is ushort  UInt16) return          UInt16;
            else if (Object is    int   Int32) return ( ulong)  Int32;
            else if (Object is   uint  UInt32) return          UInt32;
            else if (Object is   long   Int64) return ( ulong)  Int64;
            else if (Object is  ulong  UInt64) return          UInt64; return null; }
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
        {        if (Object is  sbyte   Int8 ) return           Int8 ;
            else if (Object is   byte  UInt8 ) return          UInt8 ;
            else if (Object is  short   Int16) return           Int16;
            else if (Object is ushort  UInt16) return          UInt16;
            else if (Object is    int   Int32) return           Int32;
            else if (Object is   uint  UInt32) return          UInt32;
            else if (Object is   long   Int64) return           Int64;
            else if (Object is  float Float32) return         Float32;
            else if (Object is double Float64) return         Float64; return null; }
        public string    ReadString()
        {        if (Object is string  String) return          String; return null; }

        public bool ElementArray(string Name, out MsgPack MsgPack)
        {
            if (Element(Name, out MsgPack))
            {
                if (MsgPack.Array == null) return false;
                return true;
            }
            return false;
        }

        public bool Element(string Name, out MsgPack MsgPack)
        {
            MsgPack = New;
            if (List.IsNull) return false;

            for (int i = 0; i < List.Count; i++)
                if (List[i].Name == Name) { MsgPack = List[i]; return true; }
            return false;
        }

        public bool ContainsKey(string Name)
        {
            if (List.IsNull) return false;
            
            for (int i = 0; i < List.Count ; i++)
                if (List[i].Name == Name) return true;
            return false;
        }
        
        public struct Ext
        {
            public sbyte   Type;
            public  byte[] Data;
        }
    }
}
