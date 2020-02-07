using System;

namespace KKdBaseLib
{
    public struct MsgPack : IDisposable, IEquatable<MsgPack>, INull
    {
        public string Name;
        public object Object;

        public         MsgPack[] Array => Object is         MsgPack[] Array ? Array :    null;
        public KKdList<MsgPack>   List => Object is KKdList<MsgPack>   List ?  List : default;

        public static MsgPack New => new MsgPack { Object = KKdList<MsgPack>.New };
        public static MsgPack NewReserve(int Capacity) =>
            new MsgPack { Object = KKdList<MsgPack>.NewReserve(Capacity) };

        public bool  IsNull => Array == null && List. IsNull;
        public bool NotNull => Array != null || List.NotNull;

        public MsgPack(            string Name = null)
        { Object = KKdList<MsgPack>.New; this.Name = Name; }

        public MsgPack(long Count, string Name = null)
        { Object = Count > -1 ? new MsgPack[Count] : null; this.Name = Name; }

        public MsgPack(string Name, object Object)
        { this.Name = Name; this.Object = Object; }

        public static MsgPack Null => new MsgPack();

        public MsgPack this[ int index]
        {   get =>    Object is MsgPack[] Array  ? Array[index] : default;
            set { if (Object is MsgPack[] Array) { Array[index] =   value; Object = Array; } } }

        public MsgPack this[uint index]
        {   get =>    Object is MsgPack[] Array  ? Array[index] : default;
            set { if (Object is MsgPack[] Array) { Array[index] =   value; Object = Array; } } }

        public MsgPack this[string key]
        {   get =>    Object is KKdList<MsgPack> List  ? List[ElementIndex(key)] : default; }

        public MsgPack this[string key, bool array]
        {   get { if (!array) return this[key];
                if (Object is KKdList<MsgPack> List) { MsgPack MsgPack = List[ElementIndex(key)];
                    return MsgPack.Object is MsgPack[] ? MsgPack : default; } return default; } }

        public MsgPack Add(MsgPack obj)
        { if (Object is KKdList<MsgPack> List && obj.Object != null) { List.Add(obj); Object = List; } return this; }

        public void Dispose()
        { Name = null; Object = null; }

        public bool Equals(MsgPack msg) =>
            Name == msg.Name && Object == msg.Object;

        public override string ToString() => Name ?? "" + (Object != null ?
            (List. NotNull ? $"{(Name != null ? " " : "")}Elements Count: {List .Count }" :
            (Array != null ? $"{(Name != null ? " " : "")}Elements Count: {Array.Length}" : Object)) : "");

        public static implicit operator MsgPack(byte[] val) => new MsgPack(null, val);
        public static implicit operator MsgPack(string val) => new MsgPack(null, val);
        public static implicit operator MsgPack( sbyte val) => new MsgPack(null, val);
        public static implicit operator MsgPack(  byte val) => new MsgPack(null, val);
        public static implicit operator MsgPack( short val) => new MsgPack(null, val);
        public static implicit operator MsgPack(ushort val) => new MsgPack(null, val);
        public static implicit operator MsgPack(   int val) => new MsgPack(null, val);
        public static implicit operator MsgPack(  uint val) => new MsgPack(null, val);
        public static implicit operator MsgPack(  long val) => new MsgPack(null, val);
        public static implicit operator MsgPack( ulong val) => new MsgPack(null, val);
        public static implicit operator MsgPack( float val) => new MsgPack(null, val);
        public static implicit operator MsgPack(double val) => new MsgPack(null, val);

        public MsgPack Add(  bool? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;
        public MsgPack Add( sbyte? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;
        public MsgPack Add(  byte? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;
        public MsgPack Add( short? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;
        public MsgPack Add(ushort? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;
        public MsgPack Add(   int? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;
        public MsgPack Add(  uint? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;
        public MsgPack Add(  long? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;
        public MsgPack Add( ulong? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;
        public MsgPack Add( float? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;
        public MsgPack Add(double? val) => val.HasValue ? Add(new MsgPack(null, val.Value)) : this;

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

        public MsgPack Add(string Val,   bool? val) => val.HasValue ? Add(Val, val.Value) : this;
        public MsgPack Add(string Val,  sbyte? val) => val.HasValue ? Add(Val, val.Value) : this;
        public MsgPack Add(string Val,   byte? val) => val.HasValue ? Add(Val, val.Value) : this;
        public MsgPack Add(string Val,  short? val) => val.HasValue ? Add(Val, val.Value) : this;
        public MsgPack Add(string Val, ushort? val) => val.HasValue ? Add(Val, val.Value) : this;
        public MsgPack Add(string Val,    int? val) => val.HasValue ? Add(Val, val.Value) : this;
        public MsgPack Add(string Val,   uint? val) => val.HasValue ? Add(Val, val.Value) : this;
        public MsgPack Add(string Val,   long? val) => val.HasValue ? Add(Val, val.Value) : this;
        public MsgPack Add(string Val,  ulong? val) => val.HasValue ? Add(Val, val.Value) : this;
        public MsgPack Add(string Val,  float? val) => val.HasValue ? Add(Val, val.Value) : this;
        public MsgPack Add(string Val, double? val) => val.HasValue ? Add(Val, val.Value) : this;

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

        public   bool RB  (string Name) => RnB  (Name) ?? default;
        public  sbyte RI8 (string Name) => RnI8 (Name) ?? default;
        public   byte RU8 (string Name) => RnU8 (Name) ?? default;
        public  short RI16(string Name) => RnI16(Name) ?? default;
        public ushort RU16(string Name) => RnU16(Name) ?? default;
        public    int RI32(string Name) => RnI32(Name) ?? default;
        public   uint RU32(string Name) => RnU32(Name) ?? default;
        public   long RI64(string Name) => RnI64(Name) ?? default;
        public  ulong RU64(string Name) => RnU64(Name) ?? default;
        public  float RF32(string Name) => RnF32(Name) ?? default;
        public double RF64(string Name) => RnF64(Name) ?? default;

        public   bool? RnB  (string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is   bool B  ) return         B  ; return null;
        }
        public  sbyte? RnI8 (string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is  sbyte I8 ) return         I8 ;
            else if (MsgPack.Object is   byte U8 ) return ( sbyte)U8 ; return null;
        }
        public   byte? RnU8 (string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is  sbyte I8 ) return (  byte)I8 ;
            else if (MsgPack.Object is   byte U8 ) return         U8 ; return null;
        }
        public  short? RnI16(string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is  sbyte I8 ) return         I8 ;
            else if (MsgPack.Object is   byte U8 ) return         U8 ;
            else if (MsgPack.Object is  short I16) return         I16;
            else if (MsgPack.Object is ushort U16) return ( short)U16; return null;
        }
        public ushort? RnU16(string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is  sbyte I8 ) return (ushort)I8 ;
            else if (MsgPack.Object is   byte U8 ) return         U8 ;
            else if (MsgPack.Object is  short I16) return (ushort)I16;
            else if (MsgPack.Object is ushort U16) return         U16; return null;
        }
        public    int? RnI32(string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is  sbyte I8 ) return         I8 ;
            else if (MsgPack.Object is   byte U8 ) return         U8 ;
            else if (MsgPack.Object is  short I16) return         I16;
            else if (MsgPack.Object is ushort U16) return         U16;
            else if (MsgPack.Object is    int I32) return         I32;
            else if (MsgPack.Object is   uint U32) return (   int)U32; return null;
        }
        public   uint? RnU32(string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is  sbyte I8 ) return (  uint)I8 ;
            else if (MsgPack.Object is   byte U8 ) return         U8 ;
            else if (MsgPack.Object is  short I16) return (  uint)I16;
            else if (MsgPack.Object is ushort U16) return         U16;
            else if (MsgPack.Object is    int I32) return (  uint)I32;
            else if (MsgPack.Object is   uint U32) return         U32; return null;
        }
        public   long? RnI64(string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is  sbyte I8 ) return         I8 ;
            else if (MsgPack.Object is   byte U8 ) return         U8 ;
            else if (MsgPack.Object is  short I16) return         I16;
            else if (MsgPack.Object is ushort U16) return         U16;
            else if (MsgPack.Object is    int I32) return         I32;
            else if (MsgPack.Object is   uint U32) return         U32;
            else if (MsgPack.Object is   long I64) return         I64;
            else if (MsgPack.Object is  ulong U64) return (  long)U64; return null;
        }
        public  ulong? RnU64(string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is  sbyte I8 ) return ( ulong)I8 ;
            else if (MsgPack.Object is   byte U8 ) return         U8 ;
            else if (MsgPack.Object is  short I16) return ( ulong)I16;
            else if (MsgPack.Object is ushort U16) return         U16;
            else if (MsgPack.Object is    int I32) return ( ulong)I32;
            else if (MsgPack.Object is   uint U32) return         U32;
            else if (MsgPack.Object is   long I64) return ( ulong)I64; return null;
        }
        public  float? RnF32(string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is  sbyte I8 ) return         I8 ;
            else if (MsgPack.Object is   byte U8 ) return         U8 ;
            else if (MsgPack.Object is  short I16) return         I16;
            else if (MsgPack.Object is ushort U16) return         U16;
            else if (MsgPack.Object is    int I32) return         I32;
            else if (MsgPack.Object is   uint U32) return         U32;
            else if (MsgPack.Object is   long I64) return         I64;
            else if (MsgPack.Object is  float F32) return         F32;
            else if (MsgPack.Object is double F64) return ( float)F64; return null;
        }
        public double? RnF64(string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is  sbyte I8 ) return         I8 ;
            else if (MsgPack.Object is   byte U8 ) return         U8 ;
            else if (MsgPack.Object is  short I16) return         I16;
            else if (MsgPack.Object is ushort U16) return         U16;
            else if (MsgPack.Object is    int I32) return         I32;
            else if (MsgPack.Object is   uint U32) return         U32;
            else if (MsgPack.Object is   long I64) return         I64;
            else if (MsgPack.Object is  float F32) return         F32;
            else if (MsgPack.Object is double F64) return         F64; return null;
        }
        public string    RS(string Name)
        {
            MsgPack MsgPack = this[Name];
                 if (MsgPack.Object is string S  ) return         S  ; return null;
        }

        public   bool RB  () => RnB  () ?? default;
        public  sbyte RI8 () => RnI8 () ?? default;
        public   byte RU8 () => RnU8 () ?? default;
        public  short RI16() => RnI16() ?? default;
        public ushort RU16() => RnU16() ?? default;
        public    int RI32() => RnI32() ?? default;
        public   uint RU32() => RnU32() ?? default;
        public   long RI64() => RnI64() ?? default;
        public  ulong RU64() => RnU64() ?? default;
        public  float RF32() => RnF32() ?? default;
        public double RF64() => RnF64() ?? default;

        public   bool? RnB  ()
        {        if (Object is   bool B  ) return         B  ; return null; }
        public  sbyte? RnI8 ()
        {        if (Object is  sbyte I8 ) return         I8 ;
            else if (Object is   byte U8 ) return ( sbyte)U8 ; return null; }
        public   byte? RnU8 ()
        {        if (Object is  sbyte I8 ) return (  byte)I8 ;
            else if (Object is   byte U8 ) return         U8 ; return null; }
        public  short? RnI16()
        {        if (Object is  sbyte I8 ) return         I8 ;
            else if (Object is   byte U8 ) return         U8 ;
            else if (Object is  short I16) return         I16;
            else if (Object is ushort U16) return ( short)U16; return null; }
        public ushort? RnU16()
        {        if (Object is  sbyte I8 ) return (ushort)I8 ;
            else if (Object is   byte U8 ) return         U8 ;
            else if (Object is  short I16) return (ushort)I16;
            else if (Object is ushort U16) return         U16; return null; }
        public    int? RnI32()
        {        if (Object is  sbyte I8 ) return         I8 ;
            else if (Object is   byte U8 ) return         U8 ;
            else if (Object is  short I16) return         I16;
            else if (Object is ushort U16) return         U16;
            else if (Object is    int I32) return         I32;
            else if (Object is   uint U32) return (   int)U32; return null; }
        public   uint? RnU32()
        {        if (Object is  sbyte I8 ) return (  uint)I8 ;
            else if (Object is   byte U8 ) return         U8 ;
            else if (Object is  short I16) return (  uint)I16;
            else if (Object is ushort U16) return         U16;
            else if (Object is    int I32) return (  uint)I32;
            else if (Object is   uint U32) return         U32; return null; }
        public   long? RnI64()
        {        if (Object is  sbyte I8 ) return         I8 ;
            else if (Object is   byte U8 ) return         U8 ;
            else if (Object is  short I16) return         I16;
            else if (Object is ushort U16) return         U16;
            else if (Object is    int I32) return         I32;
            else if (Object is   uint U32) return         U32;
            else if (Object is   long I64) return         I64;
            else if (Object is  ulong U64) return (  long)U64; return null; }
        public  ulong? RnU64()
        {        if (Object is  sbyte I8 ) return ( ulong)I8 ;
            else if (Object is   byte U8 ) return         U8 ;
            else if (Object is  short I16) return ( ulong)I16;
            else if (Object is ushort U16) return         U16;
            else if (Object is    int I32) return ( ulong)I32;
            else if (Object is   uint U32) return         U32;
            else if (Object is   long I64) return ( ulong)I64; return null; }
        public  float? RnF32()
        {        if (Object is  sbyte I8 ) return         I8 ;
            else if (Object is   byte U8 ) return         U8 ;
            else if (Object is  short I16) return         I16;
            else if (Object is ushort U16) return         U16;
            else if (Object is    int I32) return         I32;
            else if (Object is   uint U32) return         U32;
            else if (Object is   long I64) return         I64;
            else if (Object is  float F32) return         F32;
            else if (Object is double F64) return ( float)F64; return null; }
        public double? RnF64()
        {        if (Object is  sbyte I8 ) return         I8 ;
            else if (Object is   byte U8 ) return         U8 ;
            else if (Object is  short I16) return         I16;
            else if (Object is ushort U16) return         U16;
            else if (Object is    int I32) return         I32;
            else if (Object is   uint U32) return         U32;
            else if (Object is   long I64) return         I64;
            else if (Object is  float F32) return         F32;
            else if (Object is double F64) return         F64; return null; }
        public string  RS   ()
        {        if (Object is string S  ) return         S  ; return null; }

        public MsgPack Element(string Name)
        {
            if (List.IsNull) return default;

            for (int i = 0; i < List.Count; i++)
                if (List[i].Name == Name) return List[i];
            return default;
        }

        public bool ContainsKey(string Name) => ElementIndex(Name) > -1;

        public int ElementIndex(string Name)
        {
            if (List.IsNull) return -1;

            for (int i = 0; i < List.Count; i++)
                if (List[i].Name == Name) return i;
            return -1;
        }


        public struct Ext
        {
            public sbyte   Type;
            public  byte[] Data;
        }
    }
}
