using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.MessagePack;

namespace KKdMainLib.A3DA
{
    public static class A3DAExt
    {
        private const string d = ".";
        private const string BO = "bin_offset";
        private const string MTBO = "model_transform" + d + BO;

        private static string value;
        private static string[] dataArray;
        private static int SOi;
        private static int[] SO;

        public static ModelTransform ReadMT(this Dictionary<string, object> Dict, string Temp)
        {
            ModelTransform MT = new ModelTransform();
            Dict.FindValue(out MT.BinOffset, Temp + MTBO);
            
            MT.Rot        = Dict.ReadVec3(Temp + "rot"        + d);
            MT.Scale      = Dict.ReadVec3(Temp + "scale"      + d);
            MT.Trans      = Dict.ReadVec3(Temp + "trans"      + d);
            MT.Visibility = Dict.ReadKey (Temp + "visibility" + d);
            return MT;
        }

        public static RGBAKey ReadRGBAKey(this Dictionary<string, object> Dict, string Temp) =>
            new RGBAKey { A = Dict.ReadKey(Temp + "a" + d), B = Dict.ReadKey(Temp + "b" + d),
                          G = Dict.ReadKey(Temp + "g" + d), R = Dict.ReadKey(Temp + "r" + d) };

        public static Vector3<Key> ReadVec3(this Dictionary<string, object> Dict, string Temp) =>
            new Vector3<Key> { X = Dict.ReadKey(Temp + "x" + d), Y =
                Dict.ReadKey(Temp + "y" + d), Z = Dict.ReadKey(Temp + "z" + d) };

        public static KeyUV ReadKeyUV(this Dictionary<string, object> Dict, string Temp) =>
            new KeyUV { U = Dict.ReadKey(Temp + "U" + d), V = Dict.ReadKey(Temp + "V" + d) };

        public static Key ReadKey(this Dictionary<string, object> Dict, string Temp)
        {
            Key Key = new Key();
            Dict.FindValue(out Key.BinOffset, Temp + BO    );
            Dict.FindValue(out Key.Type     , Temp + "type");

            if (Key.BinOffset == null && Key.Type == null) return null;
            if (Key.Type == null) return Key;
            if (Key.Type == 0x0000) return Key;

            if (Key.Type == 0x0001) { Dict.FindValue(out Key.Value, Temp + "value"); return Key; }

            int i = 0, i0 = 0;
            byte i1 = 0;
            Dict.FindValue(out Key.EPTypePost, Temp + "ep_type_post");
            Dict.FindValue(out Key.EPTypePre , Temp + "ep_type_pre" );
            Dict.FindValue(out Key.Length    , Temp + "key.length"  );
            Dict.FindValue(out Key.Max       , Temp + "max"         );
            if (Dict.StartsWith(Temp + "raw_data"))
                Dict.FindValue(out Key.RawData.KeyType, Temp + "raw_data_key_type");

            if (Key.Length != null)
            {
                Key.Trans = new Key.Transform[(int)Key.Length];
                for (i0 = 0; i0 < Key.Length; i0++)
                    if (Dict.FindValue(out value, Temp + "key" + d + i0 + d + "data"))
                    {
                        Key.Trans[i0] = new Key.Transform();
                        dataArray = value.Replace("(", "").Replace(")", "").Split(',');
                        Key.Trans[i0].Type = dataArray.Length - 1;
                        Key.Trans[i0].Frame = dataArray[0].ToDouble();
                        Key.Trans[i0].Value = new double[Key.Trans[i0].Type];
                        for (i1 = 1; i1 < dataArray.Length; i1++)
                            Key.Trans[i0].Value[i1 - 1] = dataArray[i1].ToDouble();
                    }
            }
            else if (Key.RawData.KeyType != null)
            {
                Key.RawData = new Key.RawD();
                Dict.FindValue(out Key.RawData.ValueType, Temp + "raw_data.value_type");
                if (Dict.FindValue(out value, Temp + "raw_data.value_list"))
                    Key.RawData.ValueList = value.Split(',');
                Dict.FindValue(out Key.RawData.ValueListSize, Temp + "raw_data.value_list_size");
                value = "";

                int DataSize = (int)Key.RawData.KeyType + 1;
                Key.Length = Key.RawData.ValueListSize / DataSize;
                Key.Trans = new Key.Transform[(int)Key.Length];
                for (i = 0; i < Key.Length; i++)
                {
                    Key.Trans[i].Type = (int)Key.RawData.KeyType;
                    Key.Trans[i].Frame = Key.RawData.ValueList[i * DataSize + 0].ToDouble();
                    Key.Trans[i].Value = new double[Key.Trans[i0].Type];
                    for (i1 = 1; i1 < Key.Trans[i].Type; i1++)
                        Key.Trans[i].Value[i1 - 1] = Key.RawData.ValueList[i * DataSize + i1].ToDouble();
                }
                Key.RawData.ValueList = null;
            }
            return Key;
        }

        public static void Write(this Stream IO, ModelTransform MT,
            string Temp, bool A3DC, bool IsX = false, byte Flags = 0b11111)
        {
            if (A3DC && !MT.Writed && (Flags & 0b10000) == 0b10000)
            { IO.Write(Temp + MTBO + "=", MT.BinOffset); MT.Writed = true; }

            if (A3DC) return;

            if ((Flags & 0b01000) == 0b01000) IO.Write(MT.Rot       , Temp + "rot"        + d, A3DC);
            if ((Flags & 0b00100) == 0b00100) IO.Write(MT.Scale     , Temp + "scale"      + d, A3DC);
            if ((Flags & 0b00010) == 0b00010) IO.Write(MT.Trans     , Temp + "trans"      + d, A3DC);
            if ((Flags & 0b00001) == 0b00001) IO.Write(MT.Visibility, Temp + "visibility" + d, A3DC);
        }

        public static void Write(this Stream IO, RGBAKey RGBA, string Temp, string Data, bool A3DC = false)
        {
            if (RGBA.R == null && RGBA.B == null && RGBA.G == null && RGBA.A == null) return;
            IO.Write(Temp + Data + "=", "true");
            IO.Write(RGBA.A, Temp + Data + d + "a" + d, A3DC); IO.Write(RGBA.B, Temp + Data + d + "b" + d, A3DC);
            IO.Write(RGBA.G, Temp + Data + d + "g" + d, A3DC); IO.Write(RGBA.R, Temp + Data + d + "r" + d, A3DC);
        }

        public static void Write(this Stream IO, Vector3<Key> Key, string Temp, bool A3DC = false)
        { IO.Write(Key.X, Temp + "x" + d, A3DC); IO.Write(Key.Y,
            Temp + "y" + d, A3DC); IO.Write(Key.Z, Temp + "z" + d, A3DC); }

        public static void Write(this Stream IO, KeyUV UV, string Temp, string Data, bool A3DC = false)
        { IO.Write(UV.U, Temp, Data + "U", A3DC); IO.Write(UV.V, Temp, Data + "V", A3DC); }

        public static void Write(this Stream IO, Key Key, string Temp, string Data, bool A3DC = false)
        { if (Key != null) { IO.Write(Temp + Data + "=", "true"); IO.Write(Key, Temp + Data + d, A3DC); } }

        public static void Write(this Stream IO, Key Key, string Temp, bool A3DC = false)
        {
            if (Key == null) return;

            if (A3DC) { IO.Write(Temp + BO + "=", Key.BinOffset); return; }

            int i = 0;
            if (Key.Trans != null)
                if (Key.Trans.Length == 0)
                {
                    IO.Write(Temp + "type=", Key.Type);
                    if (Key.Type > 0) IO.Write(Temp + "value=", Key.Value);
                    return;
                }

            if (Key.EPTypePost != null) IO.Write(Temp + "ep_type_post=", Key.EPTypePost);
            if (Key.EPTypePre  != null) IO.Write(Temp + "ep_type_pre=" , Key.EPTypePre );
            if (Key.RawData == null && Key.Trans != null)
            {
                SO = Key.Trans.Length.SortWriter();
                for (i = 0; i < Key.Trans.Length; i++)
                {
                    SOi = SO[i];
                    IO.Write(Temp + "key" + d + SOi + d + "data=", Key.Trans[SOi].ToString());
                    IO.Write(Temp + "key" + d + SOi + d + "type=", Key.Trans[SOi].Type      );
                }
                IO.Write(Temp + "key.length=", Key.Length);
                if (Key.Max != null) IO.Write(Temp + "max=", Key.Max);
            }
            else if (Key.Trans != null)
            {
                if (Key.Max != null) IO.Write(Temp + "max=", Key.Max);
                for (i = 0; i < Key.Trans.Length; i++)
                {
                    if (Key.RawData.KeyType < Key.Trans[i].Type || Key.RawData.KeyType == null)
                        Key.RawData.KeyType = Key.Trans[i].Type;
                    if (Key.RawData.KeyType == 3) break;
                }
                Key.RawData.ValueListSize = Key.Trans.Length * (Key.RawData.KeyType + 1);
                IO.Write(Temp + "raw_data.value_list=");
                for (i = 0; i < Key.Trans.Length; i++)
                    IO.Write(Key.Trans[i].ToString(false));
                IO.Position = IO.Position - 1;
                IO.Write('\n');
                IO.Write(Temp + "raw_data.value_list_size=", Key.RawData.ValueListSize);
                IO.Write(Temp + "raw_data.value_type="     , Key.RawData.ValueType    );
                IO.Write(Temp + "raw_data_key_type="       , Key.RawData.  KeyType    );
            }
            IO.Write(Temp + "type=", Key.Type & 0xFF);
            if (Key.RawData == null && Key.Trans == null && Key.Value != null)
                if (Key.Value != 0) IO.Write(Temp + "value=", Key.Value);
        }

        public static void ReadMT(this Stream IO, ref ModelTransform MT, int C_F16)
        {
            if (MT.BinOffset == null) return;

            IO.Position = IO.Offset + (int)MT.BinOffset;

            IO.ReadOffset(out MT.Scale);
            IO.ReadOffset(out MT.Rot  );
            IO.ReadOffset(out MT.Trans);
            MT.Visibility = new Key { BinOffset = IO.ReadInt32() };

            IO.ReadVec3(ref MT.Scale     , C_F16);
            IO.ReadVec3(ref MT.Rot       , C_F16,  true);
            IO.ReadVec3(ref MT.Trans     , C_F16);
            IO.ReadKey (ref MT.Visibility, C_F16);
        }

        public static void ReadRGBAKey(this Stream IO, ref RGBAKey RGBA, int C_F16)
        { IO.ReadKey(ref RGBA.R, C_F16); IO.ReadKey(ref RGBA.G, C_F16);
          IO.ReadKey(ref RGBA.B, C_F16); IO.ReadKey(ref RGBA.A, C_F16); }

        public static void ReadVec3(this Stream IO, ref Vector3<Key> Key, int C_F16, bool F16 = false)
        { IO.ReadKey(ref Key.X, C_F16, F16); IO.ReadKey(ref Key.Y,
            C_F16, F16); IO.ReadKey(ref Key.Z, C_F16, F16); }

        public static void ReadKeyUV(this Stream IO, ref KeyUV UV, int C_F16)
        { IO.ReadKey(ref UV.U, C_F16); IO.ReadKey(ref UV.V, C_F16); }

        public static void ReadKey(this Stream IO, ref Key Key, int C_F16, bool F16 = false)
        {
            if (Key == null) return;
            if (Key.BinOffset == null || Key.BinOffset < 0) return;
            
            IO.Position = IO.Offset + (int)Key.BinOffset;
            Key.Type = IO.ReadInt32();

            Key.Value = IO.ReadSingle();
            if (Key.Type == 0x0000 || Key.Type == 0x0001) return;

            Key.Max    = IO.ReadSingle();
            Key.Length = IO.ReadInt32 ();
            Key.Trans = new Key.Transform[(int)Key.Length];
            int Ke = (int)Key.Length;
            for (int i = 0; i < Key.Length; i++)
            {
                Key.Trans[i] = new Key.Transform { Type = 3, Value = new double[3] };
                if (F16 && C_F16 > 0)
                { Key.Trans[i].Frame = IO.ReadUInt16(); Key.Trans[i].Value[0] = (double)IO.ReadHalf  (); }
                else
                { Key.Trans[i].Frame = IO.ReadSingle(); Key.Trans[i].Value[0] =         IO.ReadSingle(); }

                if (F16 && C_F16 == 2)
                { Key.Trans[i].Value[1] = (double)IO.ReadHalf  ();
                  Key.Trans[i].Value[2] = (double)IO.ReadHalf  (); }
                else
                { Key.Trans[i].Value[1] =         IO.ReadSingle();
                  Key.Trans[i].Value[2] =         IO.ReadSingle(); }
            }
        }

        public static void ReadOffset(this Stream IO, out Vector3<Key> Key)
        { Key = new Vector3<Key> { X = new Key { BinOffset = IO.ReadInt32() },
                                   Y = new Key { BinOffset = IO.ReadInt32() },
                                   Z = new Key { BinOffset = IO.ReadInt32() }, }; }

        public static void WriteOffset(this Stream IO, ref ModelTransform MT, bool ReturnToOffset)
        {
            if (ReturnToOffset)
            {
                IO.Position = (int)MT.BinOffset;
                IO.WriteOffset(MT.Scale);
                IO.WriteOffset(MT.Rot  );
                IO.WriteOffset(MT.Trans);
                IO.Write(MT.Visibility.BinOffset);
            }
            else
            {
                MT.BinOffset = IO.Position;
                IO.Position += 0x30;
                IO.Length += 0x30;
            }
        }

        public static void WriteOffset(this Stream IO, Vector3<Key> Key)
        {
            IO.Write(Key.X.BinOffset);
            IO.Write(Key.Y.BinOffset);
            IO.Write(Key.Z.BinOffset);
        }

        public static ModelTransform ReadMT(this MsgPack k, string name)
        { if (k.Element(name, out MsgPack Name)) return Name.ReadMT(); return new ModelTransform(); }

        public static ModelTransform ReadMT(this MsgPack k) =>
            new ModelTransform { Rot   = k.ReadVec3("Rot"  ), Scale      = k.ReadVec3("Scale"     ),
                                 Trans = k.ReadVec3("Trans"), Visibility = k.ReadKey ("Visibility") };

        public static RGBAKey ReadRGBAKey(this MsgPack k, string name)
        { if (k.Element(name, out MsgPack Name)) return Name.ReadRGBAKey(); return new RGBAKey(); }

        public static RGBAKey ReadRGBAKey(this MsgPack k) =>
            new RGBAKey { R = k.ReadKey("R"), G = k.ReadKey("G"), B = k.ReadKey("B"), A = k.ReadKey("A") };

        public static Vector3<Key> ReadVec3(this MsgPack k, string name)
        { if (k.Element(name, out MsgPack Name)) return Name.ReadVec3(); return new Vector3<Key>(); }

        public static Vector3<Key> ReadVec3(this MsgPack k) =>
            new Vector3<Key> { X = k.ReadKey("X"), Y = k.ReadKey("Y"), Z = k.ReadKey("Z") };

        public static KeyUV ReadKeyUV(this MsgPack k, string name)
        { if (k.Element(name, out MsgPack Name)) return Name.ReadKeyUV(); return new KeyUV(); }

        public static KeyUV ReadKeyUV(this MsgPack k) =>
            new KeyUV { U = k.ReadKey("U"), V = k.ReadKey("V") };

        public static Key ReadKey(this MsgPack k, string name)
        { if (k.Element(name, out MsgPack Name)) return Name.ReadKey(); return null; }

        public static Key ReadKey(this MsgPack k)
        {
            if (k == null) return null;
            
            Key Key = new Key { EPTypePost = k.ReadNDouble("Post"),
                EPTypePre = k.ReadNDouble("Pre"), Max = k.ReadNDouble("M"),
                Type = k.ReadNInt32("T"), Value = k.ReadNDouble("V") };
            if (k.ReadBoolean("RD")) Key.RawData = new Key.RawD();
            if (Key.Type == 0) Key.Value = 0.0;

            if (Key.Type < 2) return Key;

            if (!k.Element("Trans", out MsgPack Trans, typeof(object[]))) return Key;

            Key.Length = ((object[])Trans.Object).Length;
            Key.Trans = new Key.Transform[Key.Length.Value];
            MsgPack _Trans = new MsgPack();
            byte i1 = 0;
            for (int i = 0; i < Key.Length; i++)
            {
                Key.Trans[i] = new Key.Transform();
                if (Trans[i].GetType() != typeof(MsgPack)) continue;

                _Trans = (MsgPack)Trans[i];
                if (_Trans.Object.GetType() != typeof(object[])) continue;
                Key.Trans[i].Type = ((object[])_Trans.Object).Length - 1;
                Key.Trans[i].Value = new double[Key.Trans[i].Type];

                if (_Trans[0].GetType() != typeof(MsgPack)) continue;
                Key.Trans[i].Frame = ((MsgPack)_Trans[0]).ReadDouble();

                for (i1 = 0; i1 < Key.Trans[i].Type; i1++)
                    Key.Trans[i].Value[i1] = ((MsgPack)_Trans[i1 + 1]).ReadDouble();
            }
            return Key;
        }

        public static MsgPack WriteMP(this ModelTransform MT, string name) =>
            MT.WriteMP(new MsgPack(name));

        public static MsgPack WriteMP(this ModelTransform MT)              =>
            MT.WriteMP(new MsgPack(    ));

        public static MsgPack WriteMP(this ModelTransform MT, MsgPack MTs) =>
            MTs.Add(MT.Rot       .WriteMP("Rot"       ))
               .Add(MT.Scale     .WriteMP("Scale"     ))
               .Add(MT.Trans     .WriteMP("Trans"     ))
               .Add(MT.Visibility.WriteMP("Visibility"));

        public static MsgPack WriteMP(this RGBAKey RGBA, string name)
        {
            if (RGBA.R == null && RGBA.G == null && RGBA.B == null && RGBA.A == null) return MsgPack.Null;
            return new MsgPack(name).Add(RGBA.R.WriteMP("R")).Add(RGBA.G.WriteMP("G"))
                                    .Add(RGBA.B.WriteMP("B")).Add(RGBA.A.WriteMP("A"));
        }

        public static MsgPack WriteMP(this Vector3<Key> Key, string name) =>
            new MsgPack(name).Add(Key.X.WriteMP("X")).Add(Key.Y.WriteMP("Y")).Add(Key.Z.WriteMP("Z"));

        public static MsgPack WriteMP(this KeyUV UV, string name)
        {
            if (UV.U == null && UV.V == null) return MsgPack.Null;
            return new MsgPack(name).Add(UV.U.WriteMP("U")).Add(UV.V.WriteMP("V"));
        }

        public static MsgPack WriteMP(this Key Key, string name)
        {
            if (Key == null) return MsgPack.Null;
            if (Key.Type == null) return MsgPack.Null;

            MsgPack Keys = new MsgPack(name).Add("T", Key.Type);
            if (Key.Trans != null)
            {
                Keys.Add("Post", Key.EPTypePost).Add("Pre", Key.EPTypePre).Add("M", Key.Max);

                if (Key.RawData != null) Keys.Add("RD", true);

                byte i0 = 0;
                MsgPack Trans = new MsgPack("Trans", Key.Trans.Length);
                for (int i = 0; i < Key.Trans.Length; i++)
                {
                    MsgPack K = new MsgPack(Key.Trans[i].Type + 1);
                    K[0] = Key.Trans[i].Frame;
                    for (i0 = 1; i0 < Key.Trans[i].Type + 1; i0++)
                        K[i0] = Key.Trans[i].Value[i0 - 1];
                    Trans[i] = K;
                }
                Keys.Add(Trans);
            }
            else if (Key.Value != 0) Keys.Add("V", Key.Value);
            return Keys;
        }

        public static void Write(this Stream IO, string Data, ref bool? val)
        { if (val != null) IO.Write(Data, (  bool)val   ); }
        public static void Write(this Stream IO, string Data,     long? val)
        { if (val != null) IO.Write(Data, (  long)val   ); }
        public static void Write(this Stream IO, string Data,    ulong? val)
        { if (val != null) IO.Write(Data, ( ulong)val   ); }
        public static void Write(this Stream IO, string Data,   double? val)
        { if (val != null) IO.Write(Data, (double)val   ); }
        public static void Write(this Stream IO, string Data,   double? val, byte r)
        { if (val != null) IO.Write(Data, (double)val, r); }
        public static void Write(this Stream IO, string Data, ref bool  val)         =>
            IO.Write(Data, Main.ToString(val));
        public static void Write(this Stream IO, string Data,     long  val)         =>
            IO.Write(Data,  val.ToString(   ));
        public static void Write(this Stream IO, string Data,    ulong  val)         =>
            IO.Write(Data,  val.ToString(   ));
        public static void Write(this Stream IO, string Data,   double  val)         =>
            IO.Write(Data,  val.ToString(   ));
        public static void Write(this Stream IO, string Data,   double  val, byte r) =>
            IO.Write(Data,  val.ToString(r  ));
        public static void Write(this Stream IO, string Data,   string  val)
        { if (val != null) IO.Write((Data + val + "\n").ToUTF8()); }
    }
}
