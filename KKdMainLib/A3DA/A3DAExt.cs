using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.Types;
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
            Dict.FindValue(out Key.EPTypePost, Temp + "ep_type_post");
            Dict.FindValue(out Key.EPTypePre , Temp + "ep_type_pre" );
            Dict.FindValue(out Key.Length    , Temp + "key.length"  );
            Dict.FindValue(out Key.Max       , Temp + "max"         );
            if (Dict.StartsWith(Temp + "raw_data"))
                Dict.FindValue(out Key.RawData.KeyType, Temp + "raw_data_key_type");

            if (Key.Length != null)
            {
                int Type;
                Key.Trans = new IKeyFrame<double, double>[(int)Key.Length];
                for (i0 = 0; i0 < Key.Length; i0++)
                    if (Dict.FindValue(out value, Temp + "key" + d + i0 + d + "data"))
                    {
                        dataArray = value.Replace("(", "").Replace(")", "").Split(',');
                        Type = dataArray.Length - 1;
                             if (Type == 0) Key.Trans[i0] = new KeyFrameT0<double, double>
                        { Frame = dataArray[0].ToDouble() };
                        else if (Type == 1) Key.Trans[i0] = new KeyFrameT1<double, double>
                        { Frame = dataArray[0].ToDouble(), Value = dataArray[1].ToDouble() };
                        else if (Type == 2) Key.Trans[i0] = new KeyFrameT2<double, double>
                        { Frame = dataArray[0].ToDouble(), Value = dataArray[1].ToDouble(),
                            Interpolation = dataArray[2].ToDouble() };
                        else if (Type == 3) Key.Trans[i0] = new KeyFrameT3<double, double>
                        { Frame = dataArray[0].ToDouble(), Value = dataArray[1].ToDouble(),
                            Interpolation1 = dataArray[2].ToDouble(),
                            Interpolation2 = dataArray[3].ToDouble() };
                        Key.Trans[i0] = Key.Trans[i0].Check();
                    }
            }
            else if (Key.RawData.KeyType != null)
            {
                Dict.FindValue(out Key.RawData.ValueType, Temp + "raw_data.value_type");
                if (Dict.FindValue(out value, Temp + "raw_data.value_list"))
                    Key.RawData.ValueList = value.Split(',');
                Dict.FindValue(out Key.RawData.ValueListSize, Temp + "raw_data.value_list_size");
                value = "";

                int DS = (int)Key.RawData.KeyType + 1;
                Key.Length = Key.RawData.ValueListSize / DS;
                Key.Trans = new IKeyFrame<double, double>[(int)Key.Length];
                     if (Key.RawData.KeyType == 0)
                    for (i = 0; i < Key.Length; i++)
                        Key.Trans[i] = new KeyFrameT0<double, double>
                        { Frame = Key.RawData.ValueList[i * DS + 0].ToDouble() }.Check();
                else if (Key.RawData.KeyType == 1)
                    for (i = 0; i < Key.Length; i++)
                        Key.Trans[i] = new KeyFrameT1<double, double>
                        { Frame = Key.RawData.ValueList[i * DS + 0].ToDouble(),
                            Value = Key.RawData.ValueList[i * DS + 1].ToDouble() }.Check();
                else if (Key.RawData.KeyType == 2)
                    for (i = 0; i < Key.Length; i++)
                        Key.Trans[i] = new KeyFrameT2<double, double>
                        { Frame = Key.RawData.ValueList[i * DS + 0].ToDouble(),
                            Value = Key.RawData.ValueList[i * DS + 1].ToDouble(),
                            Interpolation = Key.RawData.ValueList[i * DS + 2].ToDouble() }.Check();
                else if (Key.RawData.KeyType == 3)
                    for (i = 0; i < Key.Length; i++)
                        Key.Trans[i] = new KeyFrameT3<double, double>
                        { Frame = Key.RawData.ValueList[i * DS + 0].ToDouble(),
                            Value = Key.RawData.ValueList[i * DS + 1].ToDouble(),
                            Interpolation1 = Key.RawData.ValueList[i * DS + 2].ToDouble(),
                            Interpolation2 = Key.RawData.ValueList[i * DS + 3].ToDouble() }.Check();

                for (i = 0; i < Key.Length; i++) Key.Trans[i].Check();
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
            IO.Write(RGBA.A, Temp + Data + d + "a" + d, A3DC);
            IO.Write(RGBA.B, Temp + Data + d + "b" + d, A3DC);
            IO.Write(RGBA.G, Temp + Data + d + "g" + d, A3DC);
            IO.Write(RGBA.R, Temp + Data + d + "r" + d, A3DC);
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
            if (Key.RawData.KeyType == null && Key.Trans != null)
            {
                SO = Key.Trans.Length.SortWriter();
                for (i = 0; i < Key.Trans.Length; i++)
                {
                    SOi = SO[i];
                    IO.Write(Temp + "key" + d + SOi + d + "data=", Key.Trans[SOi].ToString());
                         if (Key.Trans[SOi] is KeyFrameT0<double, double>)
                        IO.Write(Temp + "key" + d + SOi + d + "type=", 0);
                    else if (Key.Trans[SOi] is KeyFrameT1<double, double>)
                        IO.Write(Temp + "key" + d + SOi + d + "type=", 1);
                    else if (Key.Trans[SOi] is KeyFrameT2<double, double>)
                        IO.Write(Temp + "key" + d + SOi + d + "type=", 2);
                    else if (Key.Trans[SOi] is KeyFrameT3<double, double>)
                        IO.Write(Temp + "key" + d + SOi + d + "type=", 3);
                }
                IO.Write(Temp + "key.length=", Key.Length);
                if (Key.Max != null) IO.Write(Temp + "max=", Key.Max);
            }
            else if (Key.Trans != null)
            {
                Key.RawData.KeyType = 0;
                if (Key.Max != null) IO.Write(Temp + "max=", Key.Max);
                for (i = 0; i < Key.Trans.Length; i++)
                {
                         if (Key.Trans[i] is KeyFrameT0<double, double> &&
                        Key.RawData.KeyType < 0) Key.RawData.KeyType = 0;
                    else if (Key.Trans[i] is KeyFrameT1<double, double> &&
                        Key.RawData.KeyType < 1) Key.RawData.KeyType = 1;
                    else if (Key.Trans[i] is KeyFrameT2<double, double> &&
                        Key.RawData.KeyType < 2) Key.RawData.KeyType = 2;
                    else if (Key.Trans[i] is KeyFrameT3<double, double> &&
                        Key.RawData.KeyType < 3) break;
                }
                Key.RawData.ValueListSize = Key.Trans.Length * (Key.RawData.KeyType + 1);
                IO.Write(Temp + "raw_data.value_list=");
                     if (Key.RawData.KeyType == 0) for (i = 0; i < Key.Trans.Length; i++)
                        IO.Write(Key.Trans[i].ToKeyFrameT0().ToString(false) +
                            ((i + 1 < Key.Trans.Length) ? "," : ""));
                else if (Key.RawData.KeyType == 1) for (i = 0; i < Key.Trans.Length; i++)
                        IO.Write(Key.Trans[i].ToKeyFrameT1().ToString(false) +
                            ((i + 1 < Key.Trans.Length) ? "," : ""));
                else if (Key.RawData.KeyType == 2) for (i = 0; i < Key.Trans.Length; i++)
                        IO.Write(Key.Trans[i].ToKeyFrameT2().ToString(false) +
                            ((i + 1 < Key.Trans.Length) ? "," : ""));
                else if (Key.RawData.KeyType == 3) for (i = 0; i < Key.Trans.Length; i++)
                        IO.Write(Key.Trans[i].ToKeyFrameT3().ToString(false) +
                            ((i + 1 < Key.Trans.Length) ? "," : ""));
                IO.Position = IO.Position - 1;
                IO.Write('\n');
                IO.Write(Temp + "raw_data.value_list_size=", Key.RawData.ValueListSize);
                IO.Write(Temp + "raw_data.value_type="     , Key.RawData.ValueType    );
                IO.Write(Temp + "raw_data_key_type="       , Key.RawData.  KeyType    );
            }
            IO.Write(Temp + "type=", Key.Type & 0xFF);
            if (Key.RawData.KeyType == null && Key.Trans == null && Key.Value != null)
                if (Key.Value != 0) IO.Write(Temp + "value=", Key.Value);
        }

        public static void ReadMT(this Stream IO, ref ModelTransform MT, int C_F16)
        {
            if (MT.BinOffset == null) return;

            IO.Position = (int)MT.BinOffset;

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
            
            IO.Position = (int)Key.BinOffset;
            Key.Type = IO.ReadInt32();

            Key.Value = IO.ReadSingle();
            if (Key.Type == 0x0000 || Key.Type == 0x0001) return;

            Key.Max    = IO.ReadSingle();
            Key.Length = IO.ReadInt32 ();
            Key.Trans = new IKeyFrame<double, double>[(int)Key.Length];
            KeyFrameT3<double, double> Temp;
            for (int i = 0; i < Key.Length; i++)
            {
                Temp = new KeyFrameT3<double, double>();
                if (F16 && C_F16 > 0)
                { Temp.Frame = IO.ReadUInt16(); Temp.Value = (double)IO.ReadHalf  (); }
                else
                { Temp.Frame = IO.ReadSingle(); Temp.Value =         IO.ReadSingle(); }

                if (F16 && C_F16 == 2)
                { Temp.Interpolation1 = (double)IO.ReadHalf  ();
                  Temp.Interpolation2 = (double)IO.ReadHalf  (); }
                else
                { Temp.Interpolation1 =         IO.ReadSingle();
                  Temp.Interpolation2 =         IO.ReadSingle(); }

                Key.Trans[i] = Temp.Check();
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
                IO.Length   += 0x30;
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
            if (k.Object == null) return null;
            
            Key Key = new Key { EPTypePost = k.ReadNDouble("Post"),
                EPTypePre = k.ReadNDouble("Pre"), Max = k.ReadNDouble("M"),
                Type = k.ReadNInt32("T"), Value = k.ReadNDouble("V") };
            if (k.ReadBoolean("RD")) Key.RawData = new Key.RawD() { KeyType = -1, ValueType = "float" };
            if (Key.Type == 0) Key.Value = 0.0;

            if (Key.Type < 2) return Key;

            if (!k.ElementArray("Trans", out MsgPack Trans)) return Key;

            Key.Length = Trans.Array.Length;
            Key.Trans = new IKeyFrame<double, double>[Key.Length.Value];
            for (int i = 0; i < Key.Length; i++)
            {
                if (Trans[i].Array == null) continue;
                else if (Trans[i].Array.Length == 0) continue;
                else if (Trans[i].Array.Length == 1)
                    Key.Trans[i] = new KeyFrameT0<double, double>
                    {   Frame          = Trans[i][0].ReadDouble() };
                else if (Trans[i].Array.Length == 2)
                    Key.Trans[i] = new KeyFrameT1<double, double>
                    {
                        Frame          = Trans[i][0].ReadDouble(),
                        Value          = Trans[i][1].ReadDouble(),
                    };
                else if (Trans[i].Array.Length == 3)
                    Key.Trans[i] = new KeyFrameT2<double, double>
                    {
                        Frame          = Trans[i][0].ReadDouble(),
                        Value          = Trans[i][1].ReadDouble(),
                        Interpolation  = Trans[i][2].ReadDouble(),
                    };
                else if (Trans[i].Array.Length == 4)
                    Key.Trans[i] = new KeyFrameT3<double, double>
                    {
                        Frame          = Trans[i][0].ReadDouble(),
                        Value          = Trans[i][1].ReadDouble(),
                        Interpolation1 = Trans[i][2].ReadDouble(),
                        Interpolation2 = Trans[i][3].ReadDouble(),
                    };
                Key.Trans[i] = Key.Trans[i].Check();
            }
            return Key;
        }

        public static MsgPack Add(this MsgPack MsgPack, string name, ModelTransform MT) =>
            name == null ? 
            MsgPack.Add("Rot"       , MT.Rot       )
                   .Add("Scale"     , MT.Scale     )
                   .Add("Trans"     , MT.Trans     )
                   .Add("Visibility", MT.Visibility):
            MsgPack.Add(new MsgPack(name).Add("Rot"       , MT.Rot       )
                                         .Add("Scale"     , MT.Scale     )
                                         .Add("Trans"     , MT.Trans     )
                                         .Add("Visibility", MT.Visibility));

        public static MsgPack Add(this MsgPack MsgPack, string name, RGBAKey RGBA) =>
            (RGBA.R == null && RGBA.G == null && RGBA.B == null && RGBA.A == null) ? MsgPack :
           MsgPack.Add(new MsgPack(name).Add("R", RGBA.R).Add("G", RGBA.G)
                                        .Add("B", RGBA.B).Add("A", RGBA.A));

        public static MsgPack Add(this MsgPack MsgPack, string name, Vector3<Key> Key) =>
            MsgPack.Add(new MsgPack(name).Add("X", Key.X).Add("Y", Key.Y).Add("Z", Key.Z));

        public static MsgPack Add(this MsgPack MsgPack, string name, KeyUV UV) =>
            (UV.U == null && UV.V == null) ? MsgPack :
            MsgPack.Add(new MsgPack(name).Add("U", UV.U).Add("V", UV.V));

        public static MsgPack Add(this MsgPack MsgPack, string name, Key Key)
        {
            if (Key == null) return MsgPack;
            if (Key.Type == null) return MsgPack;

            MsgPack Keys = new MsgPack(name).Add("T", Key.Type);
            if (Key.Trans != null)
            {
                Keys.Add("M", Key.Max).Add("Post", Key.EPTypePost).Add("Pre", Key.EPTypePre);

                if (Key.RawData.KeyType != null) Keys.Add("RD", true);
                
                MsgPack Trans = new MsgPack(Key.Trans.Length, "Trans");
                MsgPack K;
                for (int i = 0; i < Key.Trans.Length; i++)
                         if (Key.Trans[i] is KeyFrameT0<double, double> KeyFrameT0)
                    {
                        K = new MsgPack(1);
                        K[0] = (MsgPack)KeyFrameT0.Frame;
                        Trans[i] = K;
                    }
                    else if (Key.Trans[i] is KeyFrameT1<double, double> KeyFrameT1)
                    {
                        K = new MsgPack(2);
                        K[0] = (MsgPack)KeyFrameT1.Frame;
                        K[1] = (MsgPack)KeyFrameT1.Value;
                        Trans[i] = K;
                    }
                    else if (Key.Trans[i] is KeyFrameT2<double, double> KeyFrameT2)
                    {
                        K = new MsgPack(3);
                        K[0] = (MsgPack)KeyFrameT2.Frame;
                        K[1] = (MsgPack)KeyFrameT2.Value;
                        K[2] = (MsgPack)KeyFrameT2.Interpolation;
                        Trans[i] = K;
                    }
                    else if (Key.Trans[i] is KeyFrameT3<double, double> KeyFrameT3)
                    {
                        K = new MsgPack(4);
                        K[0] = (MsgPack)KeyFrameT3.Frame;
                        K[1] = (MsgPack)KeyFrameT3.Value;
                        K[2] = (MsgPack)KeyFrameT3.Interpolation1;
                        K[3] = (MsgPack)KeyFrameT3.Interpolation2;
                        Trans[i] = K;
                    }
                Keys.Add(Trans);
            }
            else if (Key.Value != 0) Keys.Add("V", Key.Value);
            return MsgPack.Add(Keys);
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
