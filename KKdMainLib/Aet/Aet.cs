//Original: AetSet.bt Version: 2.0 by samyuu

using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.MessagePack;

namespace KKdMainLib.Aet
{
    public class Aet
    {
        public Aet()
        { AET = new AetHeader(); i = i0 = 0; IO = null; }

        public AetHeader AET;

        private int i, i0;
        private Stream IO;

        private const string x00 = "\0";

        public void AETReader(string file)
        {
            IO = File.OpenReader(file + ".bin", true);

            AET.MAIN  = IO.ReadPointer<AetData>();
            AET.TOUCH = IO.ReadPointer<AetData>();
            if (AET.MAIN.Offset > 0)
            {
                AETReader(ref AET.MAIN);
                if (AET.TOUCH.Offset > 0)
                    AETReader(ref AET.TOUCH);
            }

            IO.Close();
        }

        public void AETWriter(string file)
        {
            if (AET.MAIN.Offset < 0) return;
            IO = File.OpenWriter(file + ".bin", true);

            IO.Position = 0x20;
            AETWriter(ref AET.MAIN );

            if (AET.TOUCH.Offset >= 0)
            {
                IO.Position = IO.Length + 0x20;
                AETWriter(ref AET.TOUCH);
            }

            IO.Position = 0;
            IO.Write(AET.MAIN.Offset);
            if (AET.TOUCH.Offset > 0) IO.Write(AET.TOUCH.Offset);

            IO.Close();
        }

        public void AETReader(ref Pointer<AetData> AET)
        {
            IO.Position = AET.Offset;
            ref AetData Aet = ref AET.Value;

            Aet.Name = IO.ReadPointerString();
            Aet.Unk       = IO.ReadSingle();
            Aet.Duration  = IO.ReadSingle();
            Aet.FrameRate = IO.ReadSingle();
            IO.ReadInt32();
            Aet.Width      = IO.ReadInt32();
            Aet.Height     = IO.ReadInt32();
            Aet.DontChange = IO.ReadInt32();
            if (Aet.DontChange != 0) { IO.Close(); return; }
            Aet.LayObjPointers = IO.ReadCountPointer<CountPointer<int>>();
            CountPointer<SpriteEntry> Sprites = IO.ReadCountPointer<SpriteEntry>();
            Aet.Unknown = IO.ReadCountPointer<int>();

            Aet.LayObjDict  = new Dictionary<int, int>();
            Aet.ObjectsDict = new Dictionary<int, AetObject>();
            Aet.SprEntsDict = new Dictionary<int, SpriteEntry>();
            Aet.SpritesDict = new Dictionary<int, Sprite>();

            IO.Position = Aet.LayObjPointers.Offset;
            for (i = 0; i < Aet.LayObjPointers.Count; i++)
            {
                Aet.LayObjDict.Add(i, IO.Position);
                Aet.LayObjPointers.Entries[i] = IO.ReadCountPointer<int>();
            }

            for (i = 0; i < Aet.LayObjPointers.Count; i++)
            {
                IO.Position = Aet.LayObjPointers.Entries[i].Offset;
                for (i0 = 0; i0 < Aet.LayObjPointers.Entries[i].Count; i0++)
                    Aet.LayObjPointers.Entries[i].Entries[i0] = IO.ReadAetObject(ref Aet.ObjectsDict);
            }

            IO.Position = Sprites.Offset;
            for (i = 0; i < Sprites.Count; i++)
            {
                SpriteEntry SprEnt = new SpriteEntry();
                int Offset = IO.Position;

                SprEnt.Unknown = IO.ReadInt32();
                SprEnt.Width   = IO.ReadInt16();
                SprEnt.Height  = IO.ReadInt16();
                SprEnt.Frames  = IO.ReadSingle();
                SprEnt.Sprites = IO.ReadCountPointer<int>();
                long Position = IO.LongPosition;
                IO.Position = SprEnt.Sprites.Offset;
                for (i0 = 0; i0 < SprEnt.Sprites.Count; i0++)
                {
                    Sprite Spr = new Sprite();
                    int OffsetLocal = IO.Position;
                    Spr.Name = IO.ReadPointerString();
                    Spr.ID   = IO.ReadInt32();
                    Aet.SpritesDict.Add(OffsetLocal, Spr);
                    SprEnt.Sprites.Entries[i0] = OffsetLocal;
                }
                IO.LongPosition = Position;
                Aet.SprEntsDict.Add(Offset, SprEnt);
            }
        }

        public void AETWriter(ref Pointer<AetData> AET)
        {
            ref AetData Aet = ref AET.Value;

            if (Aet.SpritesArr != null)
            {
                List<int> WrittenSprites = new List<int>();
                for (i = 0; i < Aet.SprEntsArr.Length; i++)
                {
                    ref SpriteEntry SprEnt = ref Aet.SprEntsArr[i].Value;
                    if (SprEnt.Sprites.Count < 1) continue;
                    SprEnt.Sprites.Offset = IO.Position;
                    for (i0 = 0; i0 < SprEnt.Sprites.Count; i0++)
                    {
                        ref int I = ref SprEnt.Sprites.Entries[i0];
                        Aet.SpritesArr[Aet.SpritesIndDict[I]].Offset = IO.Position;
                        IO.Write(0);
                        IO.Write(Aet.SpritesArr[Aet.SpritesIndDict[I]].Value.ID);
                        WrittenSprites.Add(Aet.SpritesIndDict[I]);
                    }
                }
                for (i = 0; i < Aet.SpritesArr.Length; i++)
                    if (!WrittenSprites.Contains(i))
                    {
                        Aet.SpritesArr[i].Offset = IO.Position;
                        IO.Write(0);
                        IO.Write(Aet.SpritesArr[i].Value.ID);
                        WrittenSprites.Add(i);
                    }
            }

            IO.Align(0x20);
            for (i = 0; i < Aet.SprEntsArr.Length; i++)
            {
                Aet.SprEntsArr[i].Offset = IO.Position;
                ref SpriteEntry SprEnt = ref Aet.SprEntsArr[i].Value;
                IO.Write(SprEnt.Unknown);
                IO.Write(SprEnt.Width);
                IO.Write(SprEnt.Height);
                IO.Write(SprEnt.Frames);
                IO.Write(SprEnt.Sprites.Count);
                IO.Write(SprEnt.Sprites.Offset);
            }

            IO.Align(0x20);
            Aet.LayObjPointers.Offset = IO.Position;
            for (i = 0; i < Aet.LayObjPointers.Count; i++)
            {
                Aet.LayObjPointers.Entries[i].Offset = IO.Position;
                IO.Write(0L);
            }

            List<int> NullValPointers = new List<int>();
            for (i = 0; i < Aet.LayObjPointers.Count; i++)
            {
                for (i0 = 0; i0 < Aet.LayObjPointers.Entries[i].Count; i0++)
                {
                    ref int I = ref Aet.LayObjPointers.Entries[i].Entries[i0];
                    IO.Write(ref Aet.ObjectsArr[I].Value, ref NullValPointers);
                }

                IO.Align(0x20);
                for (i0 = 0; i0 < Aet.LayObjPointers.Entries[i].Count; i0++)
                {
                    ref int I = ref Aet.LayObjPointers.Entries[i].Entries[i0];
                    IO.Write(ref Aet.ObjectsArr[I].Value, ref Aet.ObjectsArr[I].Offset);
                }
            }
         
            IO.Align(0x20);
            AET.Offset = IO.Position;
            IO.Write(0L);
            IO.Write(Aet.Duration);
            IO.Write(Aet.FrameRate);
            IO.Write(0);
            IO.Write(Aet.Width);
            IO.Write(Aet.Height);
            IO.Write(0);
            IO.Write(0L);
            IO.Write(0L);
            IO.Write(0L);
            IO.Write(0L);

            IO.Align(0x10);
            {
                Dictionary<string, int> UsedValues = new Dictionary<string, int>();

                if (Aet.SpritesArr != null)
                    for (i = 0; i < Aet.SpritesArr.Length; i++)
                        IO.WritePointerString(ref UsedValues, ref Aet.SpritesArr[i].Value.Name);

                //IO.Align(0x4);
                for (i = 0; i < Aet.ObjectsArr.Length; i++)
                {
                    ref AetObject Obj = ref Aet.ObjectsArr[i].Value;
                    if (Obj.Type == ObjType.Pic)
                        for (i0 = 0; i0 < Obj.Pic.Value.Marker.Count; i0++)
                            IO.WritePointerString(ref UsedValues, ref Obj.Pic.Value.Marker.Entries[i0].Name);
                    else if (Obj.Type == ObjType.Eff)
                        for (i0 = 0; i0 < Obj.Eff.Value.Marker.Count; i0++)
                            IO.WritePointerString(ref UsedValues, ref Obj.Eff.Value.Marker.Entries[i0].Name);

                    IO.WritePointerString(ref UsedValues, ref Obj.Name);
                }

                //IO.Align(0x4);

                Aet.Name.Offset = IO.Position;
                IO.Write(Aet.Name.Value + x00);
            }

            if (Aet.Unknown.Count > 0)
            {
                IO.Align(0x10);
                Aet.Unknown.Offset = IO.Position;
                for (i = 0; i < Aet.Unknown.Count * 10; i++)
                    IO.Write(0L);
            }

            IO.Align(0x10);
            int ReturnPosition = IO.Position;
            for (i = 0; i < NullValPointers.Count; i++)
            {
                IO.Position = NullValPointers[i];
                IO.Write(ReturnPosition + i * 4);
            }
            IO.Position = ReturnPosition;
            for (i = 0; i < NullValPointers.Count; i++) IO.Write(0);

            IO.Align(1, true);

            for (i = 0; i < Aet.ObjectsArr.Length; i++)
            {
                ref AetObject Obj = ref Aet.ObjectsArr[i].Value;
                if (Obj.Type == ObjType.Pic)
                {
                    IO.Position = Obj.Pic.Offset;
                    IO.Write(Aet.SprEntsArr[Aet.SprEntsIndDict[Obj.Pic.Value.SpriteEntryID]].Offset);
                    IO.Write(Obj.Pic.Value.ParentObjectID > -1 ?
                        Aet.ObjectsArr[Obj.Pic.Value.ParentObjectID].Offset : 0);
                    IO.Write(Obj.Eff.Value.Marker.Count );
                    IO.Write(Obj.Eff.Value.Marker.Offset);
                    ref CountPointer<Marker> Marker = ref Obj.Pic.Value.Marker;
                    IO.Position = Marker.Offset;
                    for (i0 = 0; i0 < Marker.Count; i0++)
                    {
                        IO.Write(Marker.Entries[i0].Frame);
                        IO.Write(Marker.Entries[i0]. Name.Offset);
                    }
                }
                else if (Obj.Type == ObjType.Eff)
                {
                    IO.Position = Obj.Eff.Offset;
                    IO.Write(Aet.LayObjPointers.Entries[Obj.Eff.Value.LayerID].Offset);
                    IO.Write(Obj.Eff.Value.ParentObjectID > -1 ?
                        Aet.ObjectsArr[Obj.Eff.Value.ParentObjectID].Offset : 0);
                    IO.Write(Obj.Eff.Value.Marker.Count );
                    IO.Write(Obj.Eff.Value.Marker.Offset);
                    ref CountPointer<Marker> Marker = ref Obj.Eff.Value.Marker;
                    IO.Position = Marker.Offset;
                    for (i0 = 0; i0 < Marker.Count; i0++)
                    {
                        IO.Write(Marker.Entries[i0].Frame);
                        IO.Write(Marker.Entries[i0]. Name.Offset);
                    }
                }

                IO.Position = Aet.ObjectsArr[i].Offset;
                IO.Write(Obj.Name.Offset);
            }

            if (Aet.SpritesArr != null)
                for (i = 0; i < Aet.SpritesArr.Length; i++)
                {
                    IO.Position = Aet.SpritesArr[i].Offset;
                    IO.Write(Aet.SpritesArr[i].Value.Name.Offset);
                }

            for (i = 0; i < Aet.LayObjPointers.Count; i++)
            {
                IO.Position = Aet.LayObjPointers.Entries[i].Offset;
                IO.Write(Aet.LayObjPointers.Entries[i].Count);
                IO.Write(Aet.LayObjPointers.Entries[i].Count > 0 ?
                    Aet.ObjectsArr[Aet.ObjectsIndDict[Aet.LayObjPointers.Entries[i].Entries[0]]].Offset : 0);
            }

            IO.Position = AET.Offset;
            IO.Write(Aet.Name.Offset);
            IO.Position = AET.Offset + 0x20;
            IO.Write(Aet.LayObjPointers.Count );
            IO.Write(Aet.LayObjPointers.Offset);
            IO.Write(Aet.SprEntsArr.Length);
            IO.Write(Aet.SprEntsArr[0].Offset);
            if (Aet.Unknown.Count > 0)
            { IO.Write(Aet.Unknown.Count); IO.Write(Aet.Unknown.Offset); }
            else IO.Write(0L);
        }

        public void MsgPackReader(string file, bool JSON)
        {
            AET = new AetHeader();
            AET.MAIN .Offset = -1;
            AET.TOUCH.Offset = -1;

            MsgPack MsgPack = file.ReadMP(JSON);
            if (!MsgPack.Element<MsgPack>("Aet", out MsgPack Aet)) { MsgPack = null; return; }

            if (Aet.Array != null)
                if (Aet.Array.Length == 1)
                {
                    if (Aet.Array[0] is MsgPack MAIN ) MsgPackReader(MAIN , ref AET.MAIN );
                }
                else if (Aet.Array.Length == 2)
                {
                    if (Aet.Array[0] is MsgPack MAIN ) MsgPackReader(MAIN , ref AET.MAIN );
                    if (Aet.Array[1] is MsgPack TOUCH) MsgPackReader(TOUCH, ref AET.TOUCH);
                }

            MsgPack = null;
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            if (this.AET.MAIN .Offset <= 0) return;
            MsgPack AET = new MsgPack("Aet", this.AET.TOUCH.Offset > 0 ? 2 : 1);

            AET[0] = MsgPackWriter(ref this.AET.MAIN);
            if (this.AET.TOUCH.Offset > 0) AET[1] = MsgPackWriter(ref this.AET.TOUCH);

            AET.WriteAfterAll(true, file, JSON);
        }

        public void MsgPackReader(MsgPack AET, ref Pointer<AetData> AetData)
        {
            MsgPack Temp = new MsgPack();

            AetData.Offset = -1;
            ref AetData Aet = ref AetData.Value;

            Aet.Name.Value = AET.ReadString("Name"     );
            Aet.Unk        = AET.ReadSingle("Unk"      );
            Aet.Duration   = AET.ReadSingle("Duration" );
            Aet.FrameRate  = AET.ReadSingle("FrameRate");
            Aet.Width  = AET.ReadInt32("Width" );
            Aet.Height = AET.ReadInt32("Height");

            if (AET.Element<MsgPack>("Layers", out MsgPack Layers))
            {
                Aet.LayObjPointers.Count = Layers.Array.Length;
                for (i = 0; i < Aet.LayObjPointers.Count; i++)
                    if (Layers[i] is MsgPack ObjectID)
                    {
                        Aet.LayObjPointers.Entries[i].Count =
                            ObjectID.Array == null ? 0 : ObjectID.Array.Length;
                        for (i0 = 0; i0 < Aet.LayObjPointers.Entries[i].Count; i0++)
                            if (ObjectID.Array[i0] is MsgPack msg)
                                Aet.LayObjPointers.Entries[i].Entries[i0] = msg.ReadInt32();
                    }
            }
            else return;

            Aet.ObjectsIndDict = new Dictionary<int, int>();
            Aet.SprEntsIndDict = new Dictionary<int, int>();
            Aet.SpritesIndDict = new Dictionary<int, int>();

            int? ID = null;
            if (AET.Element("SpriteEntry", out Temp))
            {
                Aet.SprEntsArr = new Pointer<SpriteEntry>[1];
                ID = Aet.SprEntsArr[0].Value.ReadMP(Temp);
                if (ID != null) Aet.SprEntsIndDict.Add(ID.Value, 0);
            }
            else if (AET.Element<MsgPack>("SpriteEntries", out Temp))
            {
                Aet.SprEntsArr = new Pointer<SpriteEntry>[Temp.Array.Length];
                for (i = 0; i < Aet.SprEntsArr.Length; i++)
                    if (Temp[i] is MsgPack SpriteEntry)
                    {
                        ID = Aet.SprEntsArr[i].Value.ReadMP(SpriteEntry);
                        if (ID != null) Aet.SprEntsIndDict.Add(ID.Value, i);
                    }
            }
            else return;

            if (AET.Element("Object", out Temp))
            {
                Aet.ObjectsArr = new Pointer<AetObject>[1];
                ID = Aet.ObjectsArr[0].Value.ReadMP(Temp, 0, ref Aet);
                if (ID != null) Aet.ObjectsIndDict.Add(ID.Value, 0);
            }
            else if (AET.Element<MsgPack>("Objects", out Temp))
            {
                Aet.ObjectsArr = new Pointer<AetObject>[Temp.Array.Length];
                for (i = 0; i < Aet.ObjectsArr.Length; i++)
                    if (Temp[i] is MsgPack Object)
                    {
                        ID = Aet.ObjectsArr[i].Value.ReadMP(Object, i, ref Aet);
                        if (ID != null) Aet.ObjectsIndDict.Add(ID.Value, i);
                    }
            }
            else return;

            if (AET.Element("Sprite", out Temp))
            {
                Aet.SpritesArr = new Pointer<Sprite>[1];
                ID = Aet.SpritesArr[0].Value.ReadMP(Temp);
                if (ID != null) Aet.SpritesIndDict.Add(ID.Value, 0);
            }
            else if (AET.Element<MsgPack>("Sprites", out Temp))
            {
                Aet.SpritesArr = new Pointer<Sprite>[Temp.Array.Length];
                for (i = 0; i < Aet.SpritesArr.Length; i++)
                    if (Temp[i] is MsgPack Sprite)
                    {
                        ID = Aet.SpritesArr[i].Value.ReadMP(Sprite);
                        if (ID != null) Aet.SpritesIndDict.Add(ID.Value, i);
                    }
            }
            if (AET.Element("Unknown", out Temp))
                Aet.Unknown.Count = Temp.ReadInt32();

            AetData.Offset = 0;
        }


        public MsgPack MsgPackWriter(ref Pointer<AetData> AetData)
        {
            if (AetData.Offset <= 0) return null;
            ref AetData Aet = ref AetData.Value;

            Aet.ObjectsIndDict = Aet.ObjectsDict.GetIndDict();
            Aet.SpritesIndDict = Aet.SpritesDict.GetIndDict();
            Aet.SprEntsIndDict = Aet.SprEntsDict.GetIndDict();
            Aet.ObjectsArr = Aet.ObjectsDict.ToPointerArray();
            Aet.SpritesArr = Aet.SpritesDict.ToPointerArray();
            Aet.SprEntsArr = Aet.SprEntsDict.ToPointerArray();
            Aet.ObjectsDict = null;
            Aet.SpritesDict = null;
            Aet.SprEntsDict = null;

            MsgPack AET = new MsgPack().Add("Name", Aet.Name.Value).Add("Duration",
                Aet.Duration).Add("FrameRate", Aet.FrameRate).Add("Width",
                Aet.Width).Add("Height", Aet.Height).Add("Unk", Aet.Unk);

            if (Aet.LayObjPointers.Count > 0)
            {
                MsgPack Layers = new MsgPack("Layers", Aet.LayObjPointers.Count);
                for (i = 0; i < Aet.LayObjPointers.Count; i++)
                    if (Aet.LayObjPointers.Entries[i].Count > 0)
                    {
                        MsgPack Objects = new MsgPack(Aet.LayObjPointers.Entries[i].Count);
                        for (i0 = 0; i0 < Aet.LayObjPointers.Entries[i].Count; i0++)
                            Objects[i0] = Aet.ObjectsIndDict[Aet.LayObjPointers.Entries[i].Entries[i0]];
                        Layers[i] = Objects;
                    }
                AET.Add(Layers);
            }

            if (Aet.SprEntsArr.Length == 1)
                AET.Add(Aet.SprEntsArr[0].Value.WriteMP(0, ref Aet.SpritesIndDict, "SpriteEntry"));
            else if (Aet.SprEntsArr.Length > 0)
            {
                MsgPack SpriteEntries = new MsgPack("SpriteEntries", Aet.SprEntsArr.Length);
                for (i = 0; i < Aet.SprEntsArr.Length; i++)
                    SpriteEntries[i] = Aet.SprEntsArr[i].Value.WriteMP(i, ref Aet.SpritesIndDict);
                AET.Add(SpriteEntries);
            }

            if (Aet.ObjectsArr.Length == 1)
                AET.Add(Aet.ObjectsArr[0].Value.WriteMP(0, ref Aet, "Object"));
            else if (Aet.ObjectsArr.Length > 1)
            {
                MsgPack Objects = new MsgPack("Objects", Aet.ObjectsArr.Length);
                for (i = 0; i < Aet.ObjectsArr.Length; i++)
                    Objects[i] = Aet.ObjectsArr[i].Value.WriteMP(i, ref Aet);
                AET.Add(Objects);
            }

            if (Aet.SpritesArr.Length == 1)
                AET.Add(Aet.SpritesArr[0].Value.WriteMP(0, "Sprite"));
            else if (Aet.SpritesArr.Length > 1)
            {
                MsgPack Sprites = new MsgPack("Sprites", Aet.SpritesArr.Length);
                for (i = 0; i < Aet.SpritesArr.Length; i++)
                    Sprites[i] = Aet.SpritesArr[i].Value.WriteMP(i);
                AET.Add(Sprites);
            }

            if (Aet.Unknown.Count > 0)
                AET.Add("Unknown", Aet.Unknown.Count);

            return AET;
        }
    }

    public struct AetHeader
    {
        public Pointer<AetData> MAIN ;
        public Pointer<AetData> TOUCH;
    }

    public struct AetData
    {
        public Pointer<string> Name;
        public float Unk;
        public float Duration;
        public float FrameRate;
        public int Width;
        public int Height;
        public int DontChange;
        public CountPointer<CountPointer<int>> LayObjPointers;
        public CountPointer<int> Unknown;

        public Dictionary<int, int> LayObjDict;
        public Dictionary<int, int> ObjectsIndDict;
        public Dictionary<int, int> SpritesIndDict;
        public Dictionary<int, int> SprEntsIndDict;
        public Pointer<  AetObject>[] ObjectsArr;
        public Pointer<SpriteEntry>[] SprEntsArr;
        public Pointer<     Sprite>[] SpritesArr;
        public Dictionary<int,   AetObject> ObjectsDict;
        public Dictionary<int, SpriteEntry> SprEntsDict;
        public Dictionary<int,      Sprite> SpritesDict;
    }

    public struct AetObject
    {
        public int Offset;
        public Pointer<string> Name;
        public float StartTime0;
        public float LoopDuration;
        public float StartTime1;
        public float PlaybackSpeed;
        public byte Unk0;
        public byte Unk1;
        public byte Unk2;
        public ObjType Type;
        public Pointer<Pic> Pic;
        public Pointer<Aif> Aif;
        public Pointer<Eff> Eff;

        public int? ReadMP(MsgPack msg, int i, ref AetData Aet)
        {
            Name.Value    = msg.ReadString("Name"         );
            StartTime0    = msg.ReadSingle("StartTime0"   );
            LoopDuration  = msg.ReadSingle("LoopDuration" );
            StartTime1    = msg.ReadSingle("StartTime1"   );
            PlaybackSpeed = msg.ReadSingle("PlaybackSpeed");
            Unk0 = msg.ReadUInt8("Unk0");
            Unk1 = msg.ReadUInt8("Unk1");
            Unk2 = msg.ReadUInt8("Unk2");
                 if (msg.Element("Pic", out MsgPack Pic))
            { Type = ObjType.Pic; this.Pic = new Pointer<Pic>() { Value = new Pic().ReadMP(Pic, i, ref Aet) }; }
            else if (msg.Element("Aif", out MsgPack Aif))
            { Type = ObjType.Aif; this.Aif = new Pointer<Aif>() { Value = new Aif().ReadMP(Aif)             }; }
            else if (msg.Element("Eff", out MsgPack Eff))
            { Type = ObjType.Eff; this.Eff = new Pointer<Eff>() { Value = new Eff().ReadMP(Eff, i, ref Aet) }; }
            else Type = ObjType.Nop;

            return msg.ReadNInt32("ID");
        }

        public MsgPack WriteMP(int Id, ref AetData Aet, string name = null)
        {
            MsgPack AetObject = new MsgPack(name).Add("ID", Id).Add("Name", Name.Value)
                .Add("StartTime0", StartTime0).Add("LoopDuration", LoopDuration)
                .Add("StartTime1", StartTime1).Add("PlaybackSpeed", PlaybackSpeed)
                .Add("Unk0", Unk0).Add("Unk1", Unk1).Add("Unk2", Unk2);

                 if (Type == ObjType.Pic) AetObject.Add(Pic.Value.WriteMP(ref Aet));
            else if (Type == ObjType.Aif) AetObject.Add(Aif.Value.WriteMP());
            else if (Type == ObjType.Eff) AetObject.Add(Eff.Value.WriteMP(ref Aet));
            return AetObject;
        }
    }

    public struct Pic
    {
        public int SpriteEntryID;
        public int ParentObjectID;
        public CountPointer<Marker> Marker;
        public Pointer<AnimationData> Data;

        public Pic ReadMP(MsgPack msg, int i, ref AetData Aet)
        {
            int SpriteEntryID = msg.ReadInt32("SpriteEntryID");
            if (Aet.SprEntsIndDict.ContainsValue(SpriteEntryID))
                this.SpriteEntryID = Aet.SprEntsIndDict.GetKey(SpriteEntryID);

            this.ParentObjectID = -1;
            int? ParentObjectID = msg.ReadNInt32("ParentObjectID");
            if (ParentObjectID != null)
                this.ParentObjectID = ParentObjectID.Value;

            if (msg.Element("Marker", out MsgPack Marker))
            {
                this.Marker.Count = 1;
                this.Marker.Entries[0].ReadMP(Marker);
            }
            else if (msg.Element<MsgPack>("Markers", out MsgPack Markers))
            {
                this.Marker.Count = Markers.Array.Length;
                for (i = 0; i < Markers.Array.Length; i++)
                    if (Markers[i] is MsgPack Object)
                        this.Marker.Entries[i].ReadMP(Object);
            }
            else this.Marker.Entries = null;

            Data.Value.ReadMP(msg);

            return this;
        }

        public MsgPack WriteMP(ref AetData Aet)
        {
            MsgPack Pic = new MsgPack("Pic");
            if (Aet.SprEntsIndDict.ContainsKey(SpriteEntryID))
                Pic.Add("SpriteEntryID", Aet.SprEntsIndDict[SpriteEntryID]);
            if (ParentObjectID > 0)
                if (Aet.ObjectsIndDict.ContainsKey(ParentObjectID))
                    Pic.Add("ParentObjectID", Aet.ObjectsIndDict[ParentObjectID]);

            if (Marker.Count == 1)
                Pic.Add(Marker.Entries[0].WriteMP("Marker"));
            else if (Marker.Count > 1)
            {
                MsgPack Markers = new MsgPack("Markers", Marker.Count);
                for (int i = 0; i < Marker.Count; i++)
                    Markers[i] = Marker.Entries[i].WriteMP();
                Pic.Add(Markers);
            }

            Pic.Add(Data.Value.WriteMP());
            return Pic;
        }
    }

    public struct Aif
    {
        public Pointer<int> Value;
        public int Zero0;
        public int Zero1;
        public int Zero2;
        public int Zero3;
        public int Pointer;
        public CountPointer<float> Unk0;
        public CountPointer<float> Unk1;
        public CountPointer<float> Unk2;
        public CountPointer<float> Unk3;

        public Aif ReadMP(MsgPack msg)
        {
            Value.Value = msg.ReadInt32("Value");

            msg.ReadMP(ref Unk0, "Unk0");
            msg.ReadMP(ref Unk1, "Unk1");
            msg.ReadMP(ref Unk2, "Unk2");
            msg.ReadMP(ref Unk3, "Unk3");

            return this;
        }

        public MsgPack WriteMP() =>
            new MsgPack("Aif").Add("Value", Value.Value).Add(Unk0.WriteMP("Unk0")).Add(Unk1
                .WriteMP("Unk1")).Add(Unk2.WriteMP("Unk2")).Add(Unk3.WriteMP("Unk3"));
    }

    public struct Eff
    {
        public int LayerID;
        public int ParentObjectID;
        public CountPointer<Marker> Marker;
        public Pointer<AnimationData> Data;

        public Eff ReadMP(MsgPack msg, int i, ref AetData Aet)
        {
            LayerID = msg.ReadInt32("LayerID");

            this.ParentObjectID = -1;
            int? ParentObjectID = msg.ReadNInt32("ParentObjectID");
            if (ParentObjectID != null)
                this.ParentObjectID = ParentObjectID.Value;
            
            if (msg.Element("Marker", out MsgPack Marker))
            {
                this.Marker.Count = 1;
                this.Marker.Entries[0].ReadMP(Marker);
            }
            else if (msg.Element<MsgPack>("Markers", out MsgPack Markers))
            {
                this.Marker.Count = Markers.Array.Length;
                for (i = 0; i < Markers.Array.Length; i++)
                    if (Markers[i] is MsgPack Object)
                        this.Marker.Entries[i].ReadMP(Object);
            }
            else this.Marker.Entries = null;

            Data.Value.ReadMP(msg);

            return this;
        }

        public MsgPack WriteMP(ref AetData Aet)
        {
            MsgPack Eff = new MsgPack("Eff");
            if (Aet.LayObjDict.ContainsValue(LayerID))
                Eff.Add("LayerID", Aet.LayObjDict.GetKey(LayerID));
            if (ParentObjectID > 0)
                if (Aet.ObjectsIndDict.ContainsKey(ParentObjectID))
                    Eff.Add("ParentObjectID", Aet.ObjectsIndDict[ParentObjectID]);

            if (Marker.Count == 1)
                Eff.Add(Marker.Entries[0].WriteMP("Marker"));
            else if (Marker.Count > 1)
            {
                MsgPack Markers = new MsgPack("Markers", Marker.Count);
                for (int i = 0; i < Marker.Count; i++)
                    Markers[i] = Marker.Entries[i].WriteMP();
                Eff.Add(Markers);
            }

            Eff.Add(Data.Value.WriteMP());

            return Eff;
        }
    }
    
    public struct Marker
    {
        public float Frame;
        public Pointer<string> Name;

        public void ReadMP(MsgPack mp)
        { Frame = mp.ReadSingle("Frame"); Name.Value = mp.ReadString("Name"); }

        public MsgPack WriteMP(string name = null) =>
            new MsgPack(name).Add("Frame", Frame).Add("Name", Name.Value);
    }

    public struct SubAnimationData
    {
        public KeyFrameEntry   OriginX;
        public KeyFrameEntry   OriginY;
        public KeyFrameEntry PositionX;
        public KeyFrameEntry PositionY;
        public KeyFrameEntry Rotation;
        public KeyFrameEntry    ScaleX;
        public KeyFrameEntry    ScaleY;
        public KeyFrameEntry Opacity;

        public void ReadMP(MsgPack msg)
        {
              OriginX.ReadMP(msg,   "OriginX");
              OriginY.ReadMP(msg,   "OriginY");
            PositionX.ReadMP(msg, "PositionX");
            PositionY.ReadMP(msg, "PositionY");
            Rotation .ReadMP(msg, "Rotation" );
               ScaleX.ReadMP(msg,    "ScaleX");
               ScaleY.ReadMP(msg,    "ScaleY");
            Opacity  .ReadMP(msg, "Opacity"  );
        }

        public MsgPack WriteMP()
        {
            MsgPack AnimationData = new MsgPack("SubAnimationData");
            WriteMP(ref AnimationData);
            return AnimationData;
        }

        public void WriteMP(ref MsgPack msg)
        {
            msg.Add(  OriginX.WriteMP(  "OriginX"));
            msg.Add(  OriginY.WriteMP(  "OriginY"));
            msg.Add(PositionX.WriteMP("PositionX"));
            msg.Add(PositionY.WriteMP("PositionY"));
            msg.Add(Rotation .WriteMP("Rotation" ));
            msg.Add(   ScaleX.WriteMP(   "ScaleX"));
            msg.Add(   ScaleY.WriteMP(   "ScaleY"));
            msg.Add(Opacity  .WriteMP("Opacity"  ));
        }
    }

    public struct AnimationData
    {
        public BlendMode BlendMode;
        public byte Unk0;
        public bool UseTextureMask;
        public byte Unk1;
        public SubAnimationData Anim;
        public Pointer<SubAnimationData> SubAnim;

        public void ReadMP(MsgPack msg)
        {
            if (!msg.Element("AnimationData", out MsgPack Data)) return;

            System.Enum.TryParse(Data.ReadString("BlendMode"), out BlendMode);
            Unk0           = Data.ReadUInt8  ("Unk0");
            UseTextureMask = Data.ReadBoolean("UseTextureMask");
            Unk1           = Data.ReadUInt8  ("Unk1");

            Anim.ReadMP(Data);

            SubAnim.Offset = -1;
            if (Data.Element("SubAnimationData", out MsgPack SubAnimationData))
            { SubAnim.Offset = 0; SubAnim.Value.ReadMP(SubAnimationData); }
        }

        public MsgPack WriteMP()
        {
            MsgPack AnimationData = new MsgPack("AnimationData")
                .Add("BlendMode", BlendMode.ToString()).Add("Unk0", Unk0)
                .Add("UseTextureMask", UseTextureMask) .Add("Unk1", Unk1);

            Anim.WriteMP(ref AnimationData);
            if (SubAnim.Offset > 0)
                AnimationData.Add(SubAnim.Value.WriteMP());
            return AnimationData;
        }
    }

    public struct KeyFrameEntry
    {
        public int Count;
        public int Offset;
        public float Value;
        public float[] ArrValue;
        public float[] KeyFrame;
        public float[] Interpolation;

        public void ReadMP(MsgPack msg, string name)
        {
            float? Value = msg.ReadNSingle(name);
            if (Value != null) { Count = 1; this.Value = Value.Value; return; }
            if (!msg.Element<MsgPack>(name, out MsgPack Temp)) return;

            MsgPack temp;
            Count = Temp.Array.Length;
            if (Count == 0) return;
            ArrValue      = new float[Count];
            KeyFrame      = new float[Count];
            Interpolation = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                temp = Temp[i] as MsgPack;
                KeyFrame     [i] = temp.ReadSingle("KeyFrame"     );
                ArrValue     [i] = temp.ReadSingle("Value"        );
                Interpolation[i] = temp.ReadSingle("Interpolation");
            }
        }

        public MsgPack WriteMP(string name)
        {
            if (Count  < 1) return MsgPack.Null;
            if (Count == 1) return new MsgPack(name, MsgPack.Types.Float32, Value);
            MsgPack KFE = new MsgPack(name, Count);
            for (int i = 0; i < Count; i++) KFE[i] = new MsgPack().Add("KeyFrame", KeyFrame[i])
                    .Add("Value", ArrValue[i]).Add("Interpolation", Interpolation[i]);
            return KFE;
        }
    }

    public struct SpriteEntry
    {
        public int Unknown;
        public short Width;
        public short Height;
        public float Frames;
        public CountPointer<int> Sprites;

        public int? ReadMP(MsgPack msg)
        {
            Unknown = msg.ReadInt32 ("Unknown");
            Width   = msg.ReadInt16 ("Width"  );
            Height  = msg.ReadInt16 ("Height" );
            Frames  = msg.ReadSingle("Frames" );

            if (msg.Element("SpriteID", out MsgPack SpriteID))
            {
                Sprites = new CountPointer<int> { Count = 1 };
                Sprites.Entries[0] = SpriteID.ReadInt32();
            }
            else if (msg.Element<MsgPack>("SpriteIDs", out MsgPack SpriteIDs))
            {
                Sprites = new CountPointer<int> { Count = SpriteIDs.Array.Length };
                for (int i0 = 0; i0 < Sprites.Count; i0++)
                    Sprites.Entries[i0] = (SpriteIDs[i0] as MsgPack).ReadInt32();
            }

            return msg.ReadNInt32("ID");
        }

        public MsgPack WriteMP(int Id, ref Dictionary<int, int> SpritesIndDict, string Name = null)
        {
            MsgPack SpriteEntry =  new MsgPack(Name).Add("ID", Id)
                .Add("Width", Width).Add("Height", Height).Add("Unknown", Unknown);

            if (Sprites.Count > 0) SpriteEntry.Add("Frames", Frames);

            if (Sprites.Count == 1)
                SpriteEntry.Add("SpriteID", SpritesIndDict[Sprites.Entries[0]]);
            else if (Sprites.Count > 1)
            {
                MsgPack SpriteIDs = new MsgPack("SpriteIDs", Sprites.Count);
                for (int i0 = 0; i0 < Sprites.Count; i0++)
                    SpriteIDs[i0] = SpritesIndDict[Sprites.Entries[i0]];
                SpriteEntry.Add(SpriteIDs);
            }

            return SpriteEntry;
        }
    }

    public struct Sprite
    {
        public Pointer<string> Name;
        public int ID;

        public MsgPack WriteMP(int Id, string name = null) =>
            new MsgPack(name).Add("ID", Id).Add("Name", Name.Value).Add("SprDB_ID", ID);

        public int? ReadMP(MsgPack mp)
        { ID = mp.ReadInt32("SprDB_ID"); Name.Value = mp.ReadString("Name"); return mp.ReadNInt32("ID"); }
    }

    public struct CountPointer<T>
    {
        public int Count { get => Entries == null ? 0 : Entries.Length; set => Entries = new T[value]; }
        public int Offset;

        public T[] Entries;

        public override string ToString() => Entries.ToString();
    }

    public struct Pointer<T>
    {
        public int Offset;
        public T Value;

        public override string ToString() => Value.ToString();
    }

    public enum ObjType : byte
    {
        Nop = 0,
        Pic = 1,
        Aif = 2,
        Eff = 3,
    }

    public enum BlendMode : byte
    {
        Alpha                    = 3,
        Additive                 = 5,
        DstColorZero             = 6,
        SrcAlphaOneMinusSrcColor = 7,
        Transparent              = 8,
    }
}
