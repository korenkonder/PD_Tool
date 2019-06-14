using System.Collections.Generic;
using System.Linq;
using KKdMainLib.IO;
using KKdMainLib.MessagePack;

namespace KKdMainLib.Aet
{
    public static class AetExt
    {
        private static System.Text.Encoding ShiftJIS = System.Text.Encoding.GetEncoding(932);

        public static Dictionary<TKey, int> GetIndDict<TKey, TVal>(this Dictionary<TKey, TVal> Dict)
        {
            List<TKey> Keys = Dict.Keys.ToList();
            Dictionary<TKey, int> IndDict = new Dictionary<TKey, int>();
            for (int i = 0; i < Keys.Count; i++)
                IndDict.Add(Keys[i], i);
            return IndDict;
        }

        public static Pointer<T>[] ToPointerArray<T>(this Dictionary<int, T> val)
        {
            Pointer<T>[] arr = new Pointer<T>[val.Count];
            for (int i = 0; i < val.Count; i++)
                arr[i] = val.ElementAt(i).ToPointer();
            return arr;
        }

        public static Pointer<T> ReadPointer<T>(this Stream IO) =>
            new Pointer<T> { Offset = IO.ReadInt32() };

        public static Pointer<string> ReadPointerString(this Stream IO)
        { Pointer<string> val = IO.ReadPointer<string>();
            val.Value = IO.ReadStringAtOffset(val.Offset); return val; }

        public static Pointer<string> ReadPointerStringShiftJIS(this Stream IO)
        { Pointer<string> val = IO.ReadPointer<string>();
            val.Value = ShiftJIS.GetString(IO.ReadAtOffset(val.Offset)); return val; }

        public static void WriteShiftJIS(this Stream IO, string String) =>
            IO.Write(ShiftJIS.GetBytes(String));

        public static CountPointer<T> ReadCountPointer<T>(this Stream IO)
        { CountPointer<T> val = new CountPointer<T> { Count = IO.ReadInt32(),
            Offset = IO.ReadInt32() }; val.Entries = new T[val.Count]; return val; }


        public static Pointer<T> ToPointer<T>(this KeyValuePair<int, T> val) =>
            new Pointer<T> { Offset = val.Key, Value = val.Value };
        
        public static void ReadMP(this MsgPack msg, ref CountPointer<float> val, string Name)
        {
            val.Offset = 0;
            val.Entries = null;
            if (msg.ContainsKey(Name))
            {
                val.Count = 1;
                val.Entries[0] = msg.ReadSingle(Name);
            }
            else if(msg.Element<MsgPack>(Name, out MsgPack Temp))
            {
                val.Count = Temp.Array.Length;
                for (int i = 0; i < val.Count; i++)
                    if (Temp[i] is MsgPack temp)
                        val.Entries[i] = temp.ReadSingle();

            }
        }

        public static MsgPack WriteMP(this CountPointer<float> val, string Name)
        {
            if (val.Count == 1) return new MsgPack(Name, MsgPack.Types.Float32, val.Entries[0]);
            else if (val.Count > 1)
            {
                MsgPack Val = new MsgPack(Name, val.Count);
                for (int i = 0; i < val.Count; i++) Val[i] = val.Entries[i];
                return Val;
            }
            return MsgPack.Null;
        }

        public static int ReadAetObject(this Stream IO, ref Dictionary<int, AetObject> ObjectsDict)
        {
            AetObject Anim = new AetObject();
            int AnimOffset = IO.Position;
            if (ObjectsDict.ContainsKey(AnimOffset)) return AnimOffset;
            Anim.Name = IO.ReadPointerString();
            Anim.StartTime0    = IO.ReadSingle();
            Anim.LoopDuration  = IO.ReadSingle();
            Anim.StartTime1    = IO.ReadSingle();
            Anim.PlaybackSpeed = IO.ReadSingle();
            Anim.Unk0 = IO.ReadByte();
            Anim.Unk1 = IO.ReadByte();
            Anim.Unk2 = IO.ReadByte();
            Anim.Type = (ObjType)IO.ReadByte();
                 if (Anim.Type == ObjType.Pic) Anim.Pic = IO.ReadPic(ref ObjectsDict);
            else if (Anim.Type == ObjType.Aif) Anim.Aif = IO.ReadAif();
            else if (Anim.Type == ObjType.Eff) Anim.Eff = IO.ReadEff(ref ObjectsDict);
            IO.Position = AnimOffset + 0x30;
            if (!ObjectsDict.ContainsKey(AnimOffset))
                ObjectsDict.Add(AnimOffset, Anim);
            return AnimOffset;
        }
        
        public static void Write(this Stream IO, ref AetObject Anim, ref List<int> NullValPointers)
        {
            if (Anim.Type == ObjType.Pic)
            {
                if (Anim.Pic.Value.Marker.Count > 0)
                {
                    Anim.Pic.Value.Marker.Offset = IO.Position;
                    for (int i = 0; i < Anim.Pic.Value.Marker.Count; i++) IO.Write(0L);
                }
                IO.Write(ref Anim.Pic.Value.Data, ref NullValPointers);
            }
            else if (Anim.Type == ObjType.Aif) IO.Write(ref Anim.Aif);
            else if (Anim.Type == ObjType.Eff)
            {
                if (Anim.Eff.Value.Marker.Count > 0)
                {
                    Anim.Eff.Value.Marker.Offset = IO.Position;
                    for (int i = 0; i < Anim.Eff.Value.Marker.Count; i++) IO.Write(0L);
                }
                IO.Write(ref Anim.Eff.Value.Data, ref NullValPointers);
            }

        }

        public static void Write(this Stream IO, ref AetObject Anim, ref int Offset)
        {
            Offset = IO.Position;
            IO.Write(0);
            IO.Write(Anim.StartTime0   );
            IO.Write(Anim.LoopDuration );
            IO.Write(Anim.StartTime1   );
            IO.Write(Anim.PlaybackSpeed);
            IO.Write(Anim.Unk0);
            IO.Write(Anim.Unk1);
            IO.Write(Anim.Unk2);
            IO.Write((byte)Anim.Type);
                 if (Anim.Type == ObjType.Pic) IO.Write(ref Anim.Pic);
            else if (Anim.Type == ObjType.Aif)
            {
                Anim.Aif.Offset = IO.Position;
                IO.Write(Anim.Aif.Value.Value.Offset);
                IO.Write(0L);
                IO.Write(0L);
                IO.Write(Anim.Aif.Value.Pointer);
            }
            else if (Anim.Type == ObjType.Eff) IO.Write(ref Anim.Eff);
            else { IO.Write(0L); IO.Write(0L); IO.Write(0L); }
        }

        public static Pointer<Pic> ReadPic(this Stream IO, ref Dictionary<int, AetObject> ObjectsDict)
        {
            Pointer<Pic> Pic = new Pointer<Pic> { Offset = IO.Position,
                Value = new Pic
                {
                    SpriteEntryID = IO.ReadInt32(),
                    ParentObjectID = IO.ReadInt32(),
                    Marker = IO.ReadCountPointer<Marker>()
                }
            };
            Pic.Value.Data.Offset = IO.ReadInt32();
            IO.ReadAnimData(ref Pic.Value.Data);

            IO.Position = Pic.Value.Marker.Offset;
            for (int i = 0; i < Pic.Value.Marker.Count; i++)
            {
                Pic.Value.Marker.Entries[i].Frame = IO.ReadSingle();
                Pic.Value.Marker.Entries[i]. Name = IO.ReadPointerStringShiftJIS();
            }

            return Pic;
        }

        public static void Write(this Stream IO, ref Pointer<Pic> Pic)
        {
            Pic.Offset = IO.Position;
            IO.Write(0L);
            if (Pic.Value.Marker.Count > 0)
            {
                IO.Write(Pic.Value.Marker.Count );
                IO.Write(Pic.Value.Marker.Offset);
            }
            else IO.Write(0L);
            IO.Write(Pic.Value.Data  .Offset);
            IO.Write(0);
        }

        public static Pointer<Aif> ReadAif(this Stream IO)
        {
            Pointer<Aif> Aif = new Pointer<Aif> { Offset = IO.Position,
                Value = new Aif
                {
                    Value = IO.ReadPointer<int>(),
                    Zero0 = IO.ReadInt32(),
                    Zero1 = IO.ReadInt32(),
                    Zero2 = IO.ReadInt32(),
                    Zero3 = IO.ReadInt32(),
                    Pointer = IO.ReadInt32(),
                }
            };

            IO.Position = Aif.Value.Value.Offset;
            Aif.Value.Value.Value = IO.ReadInt32();

            IO.Position = Aif.Value.Pointer;
            Aif.Value.Unk0 = IO.ReadCountPointer<float>();
            Aif.Value.Unk1 = IO.ReadCountPointer<float>();
            Aif.Value.Unk2 = IO.ReadCountPointer<float>();
            Aif.Value.Unk3 = IO.ReadCountPointer<float>();

            IO.Position = Aif.Value.Unk0.Offset;
            for (int i = 0; i < Aif.Value.Unk0.Count; i++) Aif.Value.Unk0.Entries[i] = IO.ReadSingle();
            IO.Position = Aif.Value.Unk1.Offset;
            for (int i = 0; i < Aif.Value.Unk1.Count; i++) Aif.Value.Unk1.Entries[i] = IO.ReadSingle();
            IO.Position = Aif.Value.Unk2.Offset;
            for (int i = 0; i < Aif.Value.Unk2.Count; i++) Aif.Value.Unk2.Entries[i] = IO.ReadSingle();
            IO.Position = Aif.Value.Unk3.Offset;
            for (int i = 0; i < Aif.Value.Unk3.Count; i++) Aif.Value.Unk3.Entries[i] = IO.ReadSingle();

            return Aif;
        }

        public static void Write(this Stream IO, ref Pointer<Aif> Aif)
        {
            if (Aif.Value.Unk0.Count > 0) { Aif.Value.Unk0.Offset = IO.Position;
                for (int i = 0; i < Aif.Value.Unk0.Count; i++) IO.Write(Aif.Value.Unk0.Entries[i]); }
            if (Aif.Value.Unk1.Count > 0) { Aif.Value.Unk1.Offset = IO.Position;
                for (int i = 0; i < Aif.Value.Unk1.Count; i++) IO.Write(Aif.Value.Unk1.Entries[i]); }
            if (Aif.Value.Unk2.Count > 0) { Aif.Value.Unk2.Offset = IO.Position;
                for (int i = 0; i < Aif.Value.Unk2.Count; i++) IO.Write(Aif.Value.Unk2.Entries[i]); }
            if (Aif.Value.Unk3.Count > 0) { Aif.Value.Unk3.Offset = IO.Position;
                for (int i = 0; i < Aif.Value.Unk3.Count; i++) IO.Write(Aif.Value.Unk3.Entries[i]); }

            IO.Align(0x10);
            Aif.Value.Value.Offset = IO.Position;
            IO.Write(Aif.Value.Value.Value);

            IO.Align(0x10);
            Aif.Value.Pointer = IO.Position;
            if (Aif.Value.Unk0.Count > 0) { IO.Write(Aif.Value.Unk0.Count); IO.Write(Aif.Value.Unk0.Offset); }
            else IO.Write(0L);
            if (Aif.Value.Unk1.Count > 0) { IO.Write(Aif.Value.Unk1.Count); IO.Write(Aif.Value.Unk1.Offset); }
            else IO.Write(0L);
            if (Aif.Value.Unk2.Count > 0) { IO.Write(Aif.Value.Unk2.Count); IO.Write(Aif.Value.Unk2.Offset); }
            else IO.Write(0L);
            if (Aif.Value.Unk3.Count > 0) { IO.Write(Aif.Value.Unk3.Count); IO.Write(Aif.Value.Unk3.Offset); }
            else IO.Write(0L);
        }

        public static Pointer<Eff> ReadEff(this Stream IO, ref Dictionary<int, AetObject> ObjectsDict)
        {
            Pointer<Eff> Eff = new Pointer<Eff> { Offset = IO.Position,
                Value = new Eff
                {
                    LayerID = IO.ReadInt32(),
                    ParentObjectID = IO.ReadInt32(),
                    Marker = IO.ReadCountPointer<Marker>()
                }
            };
            Eff.Value.Data.Offset = IO.ReadInt32();
            IO.Position = Eff.Value.LayerID;
            CountPointer<AetObject> AetObjEntry = IO.ReadCountPointer<AetObject>();
            IO.Position = AetObjEntry.Offset;
            for (int i = 0; i < AetObjEntry.Count; i++) IO.ReadAetObject(ref ObjectsDict);
            IO.Position = Eff.Value.Marker.Offset;
            for (int i = 0; i < Eff.Value.Marker.Count; i++)
            {
                Eff.Value.Marker.Entries[i].Frame = IO.ReadSingle();
                Eff.Value.Marker.Entries[i]. Name = IO.ReadPointerStringShiftJIS();
            }
            IO.ReadAnimData(ref Eff.Value.Data);

            return Eff;
        }

        public static void Write(this Stream IO, ref Pointer<Eff> Eff)
        {
            Eff.Offset = IO.Position;
            IO.Write(0L);
            if (Eff.Value.Marker.Count > 0)
            {
                IO.Write(Eff.Value.Marker.Count );
                IO.Write(Eff.Value.Marker.Offset);
            }
            else IO.Write(0L);
            IO.Write(Eff.Value.Data.Offset);
            IO.Write(0);
        }

        public static void ReadAnimData(this Stream IO, ref Pointer<AnimationData> Anim)
        {
            ref AnimationData Data = ref Anim.Value;
            IO.Position = Anim.Offset;
            Data.BlendMode = (BlendMode)IO.ReadByte();
            Data.Unk0           = IO.ReadByte();
            Data.UseTextureMask = IO.ReadBoolean();
            Data.Unk1           = IO.ReadByte();
            Data.Anim = IO.ReadSubAnimData();
            Data.SubAnim = IO.ReadPointer<SubAnimationData>();

            IO.ReadSubAnimData(ref Data.Anim);

            if (Data.SubAnim.Offset > 0)
            {
                IO.Position = Data.SubAnim.Offset;
                Data.SubAnim.Value = IO.ReadSubAnimData();
                IO.ReadSubAnimData(ref Data.SubAnim.Value);
            }
        }

        public static SubAnimationData ReadSubAnimData(this Stream IO)
        {
            SubAnimationData Data = new SubAnimationData() { };
            Data.  OriginX = IO.ReadKeyFrameEntry();
            Data.  OriginY = IO.ReadKeyFrameEntry();
            Data.PositionX = IO.ReadKeyFrameEntry();
            Data.PositionY = IO.ReadKeyFrameEntry();
            Data.Rotation  = IO.ReadKeyFrameEntry();
            Data.   ScaleX = IO.ReadKeyFrameEntry();
            Data.   ScaleY = IO.ReadKeyFrameEntry();
            Data.Opacity   = IO.ReadKeyFrameEntry();
            return Data;
        }

        public static void ReadSubAnimData(this Stream IO, ref SubAnimationData Data)
        {
            IO.ReadKeyFrameEntry(ref Data.  OriginX);
            IO.ReadKeyFrameEntry(ref Data.  OriginY);
            IO.ReadKeyFrameEntry(ref Data.PositionX);
            IO.ReadKeyFrameEntry(ref Data.PositionY);
            IO.ReadKeyFrameEntry(ref Data.Rotation );
            IO.ReadKeyFrameEntry(ref Data.   ScaleX);
            IO.ReadKeyFrameEntry(ref Data.   ScaleY);
            IO.ReadKeyFrameEntry(ref Data.Opacity  );
        }

        public static void Write(this Stream IO, ref Pointer<AnimationData> Anim, ref List<int> NullValPointers)
        {
            ref AnimationData Data = ref Anim.Value;

            byte SubFlags = 0;
            if (Data.SubAnim.Offset > -1)
                SubFlags = IO.Write(ref Data.SubAnim.Value);

            byte Flags = IO.Write(ref Data.Anim);

            if (Data.SubAnim.Offset > -1)
            {
                IO.Align(0x20);
                Data.SubAnim.Offset = IO.Position;
                IO.Write(ref Data.SubAnim.Value, ref NullValPointers, Flags);
            }

            IO.Align(0x20);
            Anim.Offset = IO.Position;
            IO.WriteByte((byte) Data.BlendMode);
            IO.WriteByte(       Data.Unk0);
            IO.WriteByte((byte)(Data.UseTextureMask ? 1 : 0));
            IO.WriteByte(       Data.Unk1);

            IO.Write(ref Data.Anim, ref NullValPointers, Flags);
            IO.Write(Data.SubAnim.Offset > -1 ? Data.SubAnim.Offset : 0);
        }

        public static byte Write(this Stream IO, ref SubAnimationData Data)
        {
            int Flags = 0;
            Flags |= IO.Write(ref Data.  OriginX) ? 0b10000000 : 0;
            Flags |= IO.Write(ref Data.  OriginY) ? 0b01000000 : 0;
            Flags |= IO.Write(ref Data.PositionX) ? 0b00100000 : 0;
            Flags |= IO.Write(ref Data.PositionY) ? 0b00010000 : 0;
            Flags |= IO.Write(ref Data.Rotation ) ? 0b00001000 : 0;
            Flags |= IO.Write(ref Data.   ScaleX) ? 0b00000100 : 0;
            Flags |= IO.Write(ref Data.   ScaleY) ? 0b00000010 : 0;
            Flags |= IO.Write(ref Data.Opacity  ) ? 0b00000001 : 0;
            return (byte)Flags;
        }

        public static void Write(this Stream IO, ref SubAnimationData Data, ref List<int> NullValPointers, byte Flags)
        {
            IO.Write(Data.  OriginX.Count );
            if ((Flags & 0b10000000) == 0b10000000) NullValPointers.Add(IO.Position);
            IO.Write(Data.  OriginX.Offset);
            IO.Write(Data.  OriginY.Count );
            if ((Flags & 0b01000000) == 0b01000000) NullValPointers.Add(IO.Position);
            IO.Write(Data.  OriginY.Offset);
            IO.Write(Data.PositionX.Count );
            if ((Flags & 0b00100000) == 0b00100000) NullValPointers.Add(IO.Position);
            IO.Write(Data.PositionX.Offset);
            IO.Write(Data.PositionY.Count );
            if ((Flags & 0b00010000) == 0b00010000) NullValPointers.Add(IO.Position);
            IO.Write(Data.PositionY.Offset);
            IO.Write(Data.Rotation .Count );
            if ((Flags & 0b00001000) == 0b00001000) NullValPointers.Add(IO.Position);
            IO.Write(Data.Rotation .Offset);
            IO.Write(Data.   ScaleX.Count );
            if ((Flags & 0b00000100) == 0b00000100) NullValPointers.Add(IO.Position);
            IO.Write(Data.   ScaleX.Offset);
            IO.Write(Data.   ScaleY.Count );
            if ((Flags & 0b00000010) == 0b00000010) NullValPointers.Add(IO.Position);
            IO.Write(Data.   ScaleY.Offset);
            IO.Write(Data.Opacity  .Count );
            if ((Flags & 0b00000001) == 0b00000001) NullValPointers.Add(IO.Position);
            IO.Write(Data.Opacity  .Offset);
        }

        public static KeyFrameEntry ReadKeyFrameEntry(this Stream IO) =>
            new KeyFrameEntry { Count = IO.ReadInt32(), Offset = IO.ReadInt32() };

        public static void ReadKeyFrameEntry(this Stream IO, ref KeyFrameEntry entry)
        {
            if (entry.Count  < 1) return;
            IO.Position = entry.Offset;
            if (entry.Count == 1) { entry.Value = IO.ReadSingle(); return; }

            entry.Interpolation = new float[entry.Count];
            entry.KeyFrame      = new float[entry.Count];
            entry.ArrValue      = new float[entry.Count];
            for (int i = 0; i < entry.Count; i++)  entry.KeyFrame     [i] = IO.ReadSingle();
            for (int i = 0; i < entry.Count; i++)
            { entry.ArrValue[i] = IO.ReadSingle(); entry.Interpolation[i] = IO.ReadSingle(); }
        }

        public static bool Write(this Stream IO, ref KeyFrameEntry entry)
        {
            if (entry.Count == 0) return false;

            if (entry.Count > 2) IO.Align(0x20);
            entry.Offset = IO.Position;
            if (entry.Count == 1) if (entry.Value == 0) return true;
                else { IO.Write(entry.Value); return false; }  

            for (int i = 0; i < entry.Count; i++) IO.Write(entry.KeyFrame     [i]);
            for (int i = 0; i < entry.Count; i++)
            { IO.Write(entry.ArrValue[i]);        IO.Write(entry.Interpolation[i]); }
            return false;
        }

        public static void ReadMP(this CountPointer<Marker> mark, MsgPack msg)
        {
            if (msg.Element("Marker", out MsgPack Marker))
            {
                mark.Count = 1;
                mark.Entries[0].ReadMP(Marker);
            }
            else if (msg.Element<MsgPack>("Markers", out MsgPack Markers))
            {
                mark.Count = Markers.Array.Length;
                for (int i = 0; i < mark.Count; i++)
                    mark.Entries[i].ReadMP(Markers[i] as MsgPack);
            }
        }

        public static void WritePointerString(this Stream IO,
            ref Dictionary<string, int> Dict, ref Pointer<string> Str)
        {
            if (Dict.ContainsKey(Str.Value)) Str.Offset = Dict[Str.Value];
            else
            { Str.Offset = IO.Position; Dict.Add(Str.Value, Str.Offset); IO.WriteShiftJIS(Str.Value + "\0"); }
        }
    }
}
