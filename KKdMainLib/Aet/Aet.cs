//Original: AetSet.bt Version: 2.0 by samyuu

//using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.Types;
using KKdMainLib.MessagePack;

namespace KKdMainLib.Aet
{
    public class Aet
    {
        public Aet()
        { AET = new AetHeader(); i = i0 = 0; IO = null; }

        public AetHeader AET;

        private int i, i0, i1;
        private Stream IO;

        private const string x00 = "\0";

        public void AETReader(string file)
        {
            AET = new AetHeader();
            IO = File.OpenReader(file + ".bin", true);

            i = 0;
            int Pos = -1;
            while (true) { Pos = IO.ReadInt32(); if (Pos != 0 && Pos < IO.Length) i++; else break; }

            IO.Position = 0;
            AET.Data = new Pointer<AetData>[i];
            for (int i = 0; i < AET.Data.Length; i++) AET.Data[i] = IO.ReadPointer<AetData>();
            for (int i = 0; i < AET.Data.Length; i++) AETReader(ref AET.Data[i]);

            IO.Close();
        }

        public void AETWriter(string file)
        {
            if (AET.Data == null) return;
            if (AET.Data.Length == 0) return;
            IO = File.OpenWriter(file + ".bin", true);

            for (int i = 0; i < AET.Data.Length; i++)
            {
                IO.Position = IO.Length + 0x20;
                AETWriter(ref AET.Data[i]);
            }

            IO.Position = 0;
            for (int i = 0; i < AET.Data.Length; i++) IO.Write(AET.Data[i].Offset);
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
            Aet.Layers        = IO.ReadCountPointer<   AetLayer>();
            Aet.SpriteEntries = IO.ReadCountPointer<SpriteEntry>();
            Aet.Unknown = IO.ReadCountPointer<int>();

            IO.Position = Aet.Layers.Offset;
            for (i = 0; i < Aet.Layers.Count; i++)
                Aet.Layers[i] = new AetLayer() { Pointer = IO.Position,
                    Count = IO.ReadInt32(), Offset = IO.ReadInt32()};

            for (i = 0, i1 = 0; i < Aet.Layers.Count; i++)
            {
                IO.Position = Aet.Layers[i].Offset;
                for (i0 = 0; i0 < Aet.Layers[i].Count; i0++, i1++)
                {
                    Aet.Layers[i].Objects[i0] = IO.ReadAetObject();
                    Aet.Layers[i].ObjectIDs[i0] = i1;
                }
            }

            IO.Position = Aet.SpriteEntries.Offset;
            for (i = 0; i < Aet.SpriteEntries.Count; i++)
                Aet.SpriteEntries[i] = new SpriteEntry
                { Offset = IO.Position, Unknown = IO.ReadInt32(), Width = IO.ReadInt16(),
                    Height = IO.ReadInt16(), Frames = IO.ReadSingle(), Sprites = IO.ReadCountPointer<Sprite>() };

            for (i = 0; i < Aet.SpriteEntries.Count; i++)
            {
                IO.Position = Aet.SpriteEntries[i].Sprites.Offset;
                for (i0 = 0; i0 < Aet.SpriteEntries[i].Sprites.Count; i0++)
                    Aet.SpriteEntries.Entries[i].Sprites[i0] =
                        new Sprite { Name = IO.ReadPointerString(), ID = IO.ReadInt32() };
            }
        }

        public void AETWriter(ref Pointer<AetData> AET)
        {
            ref AetData Aet = ref AET.Value;

            for (i = 0; i < Aet.SpriteEntries.Count; i++)
            {
                ref SpriteEntry SprEnt = ref Aet.SpriteEntries.Entries[i];
                if (SprEnt.Sprites.Count == 0) { SprEnt.Sprites.Offset = 0; continue; }
                if (SprEnt.Sprites.Count > 1) IO.Align(0x20);
                SprEnt.Sprites.Offset = IO.Position;
                for (i0 = 0; i0 < SprEnt.Sprites.Count; i0++)
                    IO.Write(0L);
            }

            IO.Align(0x20);
            Aet.SpriteEntries.Offset = IO.Position;
            for (i = 0; i < Aet.SpriteEntries.Count; i++)
            {
                ref SpriteEntry SprEnt = ref Aet.SpriteEntries.Entries[i];
                SprEnt.Offset = IO.Position;
                IO.Write(SprEnt.Unknown);
                IO.Write(SprEnt.Width);
                IO.Write(SprEnt.Height);
                IO.Write(SprEnt.Frames);
                IO.Write(SprEnt.Sprites.Count);
                IO.Write(SprEnt.Sprites.Offset);
            }

            IO.Align(0x20);
            Aet.Layers.Offset = IO.Position;
            for (i = 0; i < Aet.Layers.Count; i++)
            {
                Aet.Layers.Entries[i].Offset = IO.Position;
                IO.Write(0L);
            }

            KKdList<int> NullValPointers = KKdList<int>.New;
            for (i = 0; i < Aet.Layers.Count; i++)
            {
                if (Aet.Layers.Entries[i].Count < 1) continue;

                for (i0 = 0; i0 < Aet.Layers.Entries[i].Count; i0++)
                    IO.Write(ref Aet.Layers.Entries[i].Objects[i0], ref NullValPointers);

                IO.Align(0x20);
                Aet.Layers.Entries[i].Objects[0].Offset = IO.Position;
                for (i0 = 0; i0 < Aet.Layers.Entries[i].Count; i0++)
                    IO.Write(ref Aet.Layers.Entries[i].Objects[i0]);
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
                System.Collections.Generic.Dictionary<string, int> UsedValues =
                    new System.Collections.Generic.Dictionary<string, int>();

                for (i = 0; i < Aet.SpriteEntries.Count; i++)
                {
                    ref SpriteEntry SprEnt = ref Aet.SpriteEntries.Entries[i];
                    for (i0 = 0; i0 < SprEnt.Sprites.Count; i0++)
                        IO.WritePointerString(ref UsedValues, ref SprEnt.Sprites.Entries[i0].Name);
                }

                //IO.Align(0x4);
                for (i = 0; i < Aet.Layers.Count; i++)
                    for (i0 = 0; i0 < Aet.Layers.Entries[i].Count; i0++)
                    {
                        ref AetObject Obj = ref Aet.Layers.Entries[i].Objects[i0];
                             if (Obj.Type == AetObject.ObjType.Pic)
                            for (i1 = 0; i1 < Obj.Pic.Value.Marker.Count; i1++)
                                IO.WritePointerString(ref UsedValues, ref Obj.Pic.Value.Marker.Entries[i1].Name);
                        else if (Obj.Type == AetObject.ObjType.Eff)
                            for (i1 = 0; i1 < Obj.Eff.Value.Marker.Count; i1++)
                                IO.WritePointerString(ref UsedValues, ref Obj.Eff.Value.Marker.Entries[i1].Name);

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
            for (i = 0; i < NullValPointers.Count; i++) IO.Write(0);

            IO.Align(0x10, true);

            for (i = 0; i < NullValPointers.Count; i++)
            {
                IO.Position = NullValPointers[i];
                IO.Write(ReturnPosition + i * 4);
            }

            for (i = 0; i < Aet.Layers.Count; i++)
                for (i0 = 0; i0 < Aet.Layers.Entries[i].Count; i0++)
                {
                    ref AetObject Obj = ref Aet.Layers.Entries[i].Objects[i0];
                    if (Obj.Type == AetObject.ObjType.Pic)
                    {
                        IO.Position = Obj.Pic.Offset;
                        IO.Write(Aet.SpriteEntries[Obj.Pic.Value.SpriteEntryID].Offset);
                        if (Obj.Pic.Value.ParentObjectID > -1)
                        {
                            bool Found = false;
                            for (int I = 0; I < Aet.Layers.Count; I++)
                                for (int I0 = 0; I0 < Aet.Layers.Entries[I].Count; I0++)
                                    if (Aet.Layers.Entries[I].ObjectIDs[I0] == Obj.Pic.Value.ParentObjectID)
                                    {
                                        Found = true;
                                        IO.Write(Aet.Layers.Entries[I].Objects[I0].Offset);
                                        break;
                                    }
                            if (!Found) IO.Write(0);
                        }
                        else IO.Write(0);
                        IO.Write(Obj.Eff.Value.Marker.Count);
                        IO.Write(Obj.Eff.Value.Marker.Offset);
                        ref CountPointer<Marker> Marker = ref Obj.Pic.Value.Marker;
                        IO.Position = Marker.Offset;
                        for (i1 = 0; i1 < Marker.Count; i1++)
                        {
                            IO.Write(Marker[i1].Frame);
                            IO.Write(Marker[i1].Name.Offset);
                        }
                    }
                    else if (Obj.Type == AetObject.ObjType.Eff)
                    {
                        IO.Position = Obj.Eff.Offset;
                        IO.Write(Aet.Layers[Obj.Eff.Value.LayerID].Offset);
                        if (Obj.Eff.Value.ParentObjectID > -1)
                        {
                            bool Found = false;
                            for (int I = 0; I < Aet.Layers.Count; I++)
                                for (int I0 = 0; I0 < Aet.Layers.Entries[I].Count; I0++)
                                    if (Aet.Layers.Entries[I].ObjectIDs[I0] == Obj.Eff.Value.ParentObjectID)
                                    {
                                        Found = true;
                                        IO.Write(Aet.Layers.Entries[I].Objects[I0].Offset);
                                        break;
                                    }
                            if (!Found) IO.Write(0);
                        }
                        else IO.Write(0);
                        IO.Write(Obj.Eff.Value.Marker.Count);
                        IO.Write(Obj.Eff.Value.Marker.Offset);
                        ref CountPointer<Marker> Marker = ref Obj.Eff.Value.Marker;
                        IO.Position = Marker.Offset;
                        for (i1 = 0; i1 < Marker.Count; i1++)
                        {
                            IO.Write(Marker[i1].Frame);
                            IO.Write(Marker[i1].Name.Offset);
                        }
                    }

                    IO.Position = Obj.Offset;
                    IO.Write(Obj.Name.Offset);
                }
            

            for (i = 0; i < Aet.SpriteEntries.Count; i++)
            {
                ref SpriteEntry SprEnt = ref Aet.SpriteEntries.Entries[i];
                if (SprEnt.Sprites.Count == 0) continue;
                IO.Position = SprEnt.Sprites.Offset;
                for (i0 = 0; i0 < SprEnt.Sprites.Count; i0++)
                {
                    IO.Write(SprEnt.Sprites[i0].Name.Offset);
                    IO.Write(SprEnt.Sprites[i0].ID);
                }
            }

            IO.Position = Aet.Layers.Offset;
            for (i = 0; i < Aet.Layers.Count; i++)
            {
                if (Aet.Layers[i].Count > 0)
                {
                    IO.Write(Aet.Layers[i].Count);
                    IO.Write(Aet.Layers[i].Objects[0].Offset);
                }
                else IO.Write(0L);
            }

            IO.Position = AET.Offset;
            IO.Write(Aet.Name.Offset);
            IO.Position = AET.Offset + 0x20;
            IO.Write(Aet.Layers.Count );
            IO.Write(Aet.Layers.Offset);
            IO.Write(Aet.SpriteEntries.Count );
            IO.Write(Aet.SpriteEntries.Offset);
            if (Aet.Unknown.Count > 0)
            { IO.Write(Aet.Unknown.Count); IO.Write(Aet.Unknown.Offset); }
            else IO.Write(0L);
        }

        public void MsgPackReader(string file, bool JSON)
        {
            AET = new AetHeader();

            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);
            if (!MsgPack.ElementArray("Aet", out MsgPack Aet)) { MsgPack = MsgPack.New; return; }

            if (Aet.Array != null)
                if (Aet.Array.Length > 0)
                {
                    AET.Data = new Pointer<AetData>[Aet.Array.Length];
                    for (int i = 0; i < AET.Data.Length; i++)
                        MsgPackReader(Aet.Array[i], ref AET.Data[i] );
                }

            MsgPack = MsgPack.New;
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            if (this.AET.Data == null) return;
            if (this.AET.Data.Length == 0) return;

            MsgPack AET = new MsgPack(this.AET.Data.Length, "Aet");
            for (int i = 0; i < this.AET.Data.Length; i++) AET[i] = MsgPackWriter(ref this.AET.Data[i]);
            AET.WriteAfterAll(true, file, JSON);
        }

        public void MsgPackReader(MsgPack AET, ref Pointer<AetData> AetData)
        {
            MsgPack Temp = MsgPack.New;

            AetData.Offset = -1;
            ref AetData Aet = ref AetData.Value;

            Aet.Name.Value = AET.ReadString("Name"     );
            Aet.Unk        = AET.ReadSingle("Unk"      );
            Aet.Duration   = AET.ReadSingle("Duration" );
            Aet.FrameRate  = AET.ReadSingle("FrameRate");
            Aet.Width  = AET.ReadInt32("Width" );
            Aet.Height = AET.ReadInt32("Height");

            if (AET.Element("Unknown", out Temp))
                Aet.Unknown.Count = Temp.ReadInt32();


            if (AET.ElementArray("SpriteEntries", out Temp))
            {
                Aet.SpriteEntries.Count = Temp.Array.Length;
                for (i = 0; i < Aet.SpriteEntries.Count; i++)
                    Aet.SpriteEntries.Entries[i].ReadMP(Temp[i]);
            }
            else return;


            if (AET.ElementArray("Layers", out Temp))
            {
                Aet.Layers.Count = Temp.Array.Length + 1;
                for (i = 0; i < Aet.Layers.Count - 1; i++)
                    Aet.Layers.Entries[i].ReadMP(Temp[i]);
            }
            else return;


            i = Aet.Layers.Count - 1;
            if (AET.ElementArray("RootLayer", out Temp))
            {
                Aet.Layers.Entries[i].Objects = new AetObject[Temp.Array.Length];
                for (i0 = 0; i0 < Aet.Layers[i].Count; i0++)
                    Aet.Layers.Entries[i].Objects[i0].ReadMP(Temp[i0]);
            }
            else if (AET.Element("RootLayer", out Temp))
                Aet.Layers.Entries[i].Objects[0].ReadMP(Temp);
            else return;

            AetData.Offset = 0;
        }
        
        public MsgPack MsgPackWriter(ref Pointer<AetData> AetData)
        {
            if (AetData.Offset <= 0) return MsgPack.New;
            ref AetData Aet = ref AetData.Value;

            MsgPack AET = MsgPack.New.Add("Name", Aet.Name.Value).Add("Duration",
                Aet.Duration).Add("FrameRate", Aet.FrameRate).Add("Width",
                Aet.Width).Add("Height", Aet.Height).Add("Unk", Aet.Unk);
            if (Aet.Unknown.Count > 0)
                AET.Add("Unknown", Aet.Unknown.Count);

            i = Aet.Layers.Count - 1;
            if (Aet.Layers[i].Count == 1)
                AET.Add(Aet.Layers[i][0].WriteMP(ref Aet, Aet.Layers[i].ObjectIDs[0], "RootLayer"));
            if (Aet.Layers[i].Count > 1)
            {
                MsgPack RootLayer = new MsgPack(Aet.Layers[i].Count, "RootLayer");
                for (i0 = 0; i0 < Aet.Layers[i].Count; i0++)
                    RootLayer[i0] = Aet.Layers[i][i0].WriteMP(ref Aet, Aet.Layers[i].ObjectIDs[i0]);
                AET.Add(RootLayer);
            }

            MsgPack Layers = new MsgPack(Aet.Layers.Count - 1, "Layers");
            for (i = 0; i < Aet.Layers.Count - 1; i++)
                Layers[i] = Aet.Layers[i].WriteMP(i, ref Aet);
            AET.Add(Layers);

            MsgPack SpriteEntries = new MsgPack(Aet.SpriteEntries.Count, "SpriteEntries");
            for (i = 0; i < Aet.SpriteEntries.Count; i++)
                SpriteEntries[i] = Aet.SpriteEntries[i].WriteMP(i);
            AET.Add(SpriteEntries);

            return AET;
        }
    }

    public struct AetHeader
    {
        public Pointer<AetData>[] Data;
        public POF POF;
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
        public CountPointer<   AetLayer> Layers;
        public CountPointer<SpriteEntry> SpriteEntries;
        public CountPointer<int> Unknown;
    }

    public struct AetLayer
    {
        public int Pointer;
        public int Count
        {
            get => Objects != null ? Objects.Length : 0;
            set { ObjectIDs = new int[value]; Objects = new AetObject[value]; }
        }
        public int Offset;
        public int[] ObjectIDs;
        public AetObject[] Objects;

        public void ReadMP(MsgPack msg)
        {
            if (msg.Element("Object", out MsgPack Object))
            {
                Count = 1;
                Objects[0].ReadMP(Object);
            }
            else if (msg.ElementArray("Objects", out MsgPack Objects))
            {
                Count = Objects.Array.Length;
                for (int i0 = 0; i0 < Count; i0++)
                    this.Objects[i0].ReadMP(Objects[i0]);
            }
        }

        public MsgPack WriteMP(int Id, ref AetData Aet)
        {
            if (Count < 1)
                return MsgPack.New.Add("ID", Id);
            else if (Count == 1)
                return MsgPack.New.Add("ID", Id).Add(this.Objects[0].WriteMP(ref Aet, ObjectIDs[0], "Object"));
            MsgPack Objects = new MsgPack(Count, "Objects");
            for (int i = 0; i < Count; i++)
                Objects[i] = this.Objects[i].WriteMP(ref Aet, ObjectIDs[i]);
            return MsgPack.New.Add("ID", Id).Add(Objects);
        }

        public AetObject this[int index]
        {
            get { if (Count > 0) return Objects[index]; return default(AetObject); }
            set { if (Count > 0) { Objects[index] = value; } }
        }
        
        public override string ToString() => "Count: " + Count;
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
        public Pointer<Picture> Pic;
        public Pointer<Audio  > Aif;
        public Pointer<Effect > Eff;

        public int ReadMP(MsgPack msg)
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
            { Type = ObjType.Pic; this.Pic.Value.ReadMP(Pic); }
            else if (msg.Element("Aif", out MsgPack Aif))
            { Type = ObjType.Aif; this.Aif.Value.ReadMP(Aif); }
            else if (msg.Element("Eff", out MsgPack Eff))
            { Type = ObjType.Eff; this.Eff.Value.ReadMP(Eff); }
            else Type = ObjType.Nop;
            return msg.ReadInt32("ID");
        }

        public MsgPack WriteMP(ref AetData Aet, int ID, string name = null)
        {
            MsgPack AetObject = new MsgPack(name).Add("ID", ID).Add("Name", Name.Value)
                .Add("StartTime0", StartTime0).Add("LoopDuration", LoopDuration)
                .Add("StartTime1", StartTime1).Add("PlaybackSpeed", PlaybackSpeed)
                .Add("Unk0", Unk0).Add("Unk1", Unk1).Add("Unk2", Unk2);

                 if (Type == ObjType.Pic) AetObject.Add(Pic.Value.WriteMP(ref Aet));
            else if (Type == ObjType.Aif) AetObject.Add(Aif.Value.WriteMP());
            else if (Type == ObjType.Eff) AetObject.Add(Eff.Value.WriteMP(ref Aet));
            return AetObject;
        }

        public enum ObjType : byte
        {
            Nop = 0,
            Pic = 1,
            Aif = 2,
            Eff = 3,
        }

        public struct Picture
        {
            public int SpriteEntryID;
            public int ParentObjectID;
            public CountPointer<Marker> Marker;
            public Pointer<AnimationData> Data;

            public Picture ReadMP(MsgPack msg)
            {
                SpriteEntryID = msg.ReadInt32("SpriteEntryID");

                this.ParentObjectID = -1;
                int? ParentObjectID = msg.ReadNInt32("ParentObjectID");
                if (ParentObjectID != null)
                    this.ParentObjectID = ParentObjectID.Value;

                if (msg.Element("Marker", out MsgPack Marker))
                {
                    this.Marker.Count = 1;
                    this.Marker.Entries[0].ReadMP(Marker);
                }
                else if (msg.ElementArray("Markers", out MsgPack Markers))
                {
                    this.Marker.Count = Markers.Array.Length;
                    for (int i = 0; i < Markers.Array.Length; i++)
                        this.Marker.Entries[i].ReadMP(Markers[i]);
                }
                else this.Marker.Entries = null;

                Data.Value.ReadMP(msg);

                return this;
            }

            public MsgPack WriteMP(ref AetData Aet)
            {
                MsgPack Pic = new MsgPack("Pic");
                for (int i = 0; i < Aet.SpriteEntries.Count; i++)
                    if (Aet.SpriteEntries[i].Offset == SpriteEntryID)
                    { Pic.Add("SpriteEntryID", i); break; }

                if (ParentObjectID > 0)
                    for (int i = 0; i < Aet.Layers.Count; i++)
                        for (int i1 = 0; i1 < Aet.Layers[i].Count; i1++)
                            if (Aet.Layers[i][i1].Offset == ParentObjectID)
                            { Pic.Add("ParentObjectID", Aet.Layers[i].ObjectIDs[i1]); break; }

                if (Marker.Count == 1)
                    Pic.Add(Marker.Entries[0].WriteMP("Marker"));
                else if (Marker.Count > 1)
                {
                    MsgPack Markers = new MsgPack(Marker.Count, "Markers");
                    for (int i = 0; i < Marker.Count; i++)
                        Markers[i] = Marker.Entries[i].WriteMP();
                    Pic.Add(Markers);
                }

                Pic.Add(Data.Value.WriteMP());
                return Pic;
            }
        }

        public struct Audio
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

            public void ReadMP(MsgPack msg) =>
                Value.Value = msg.ReadMP(ref Unk0, "Unk0").ReadMP(ref Unk1, "Unk1")
                   .ReadMP(ref Unk2, "Unk2").ReadMP(ref Unk3, "Unk3").ReadInt32("Value");

            public MsgPack WriteMP() =>
                new MsgPack("Aif").Add("Value", Value.Value).Add("Unk0", Unk0)
                .Add("Unk1", Unk0).Add("Unk2", Unk2).Add("Unk3", Unk3);
        }

        public struct Effect
        {
            public int LayerID;
            public int ParentObjectID;
            public CountPointer<Marker> Marker;
            public Pointer<AnimationData> Data;

            public void ReadMP(MsgPack msg)
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
                else if (msg.ElementArray("Markers", out MsgPack Markers))
                {
                    this.Marker.Count = Markers.Array.Length;
                    for (int i = 0; i < Markers.Array.Length; i++)
                        this.Marker.Entries[i].ReadMP(Markers[i]);
                }
                else this.Marker.Entries = null;

                Data.Value.ReadMP(msg);
            }

            public MsgPack WriteMP(ref AetData Aet)
            {
                MsgPack Eff = new MsgPack("Eff");
                for (int i = 0; i < Aet.Layers.Count; i++)
                    if (Aet.Layers[i].Pointer == LayerID)
                    { Eff.Add("LayerID", i); break; }
                if (ParentObjectID > 0)
                    for (int i = 0; i < Aet.Layers.Count; i++)
                        for (int i1 = 0; i1 < Aet.Layers[i].Count; i1++)
                            if (Aet.Layers[i][i1].Offset == ParentObjectID)
                            { Eff.Add("ParentObjectID", Aet.Layers[i].ObjectIDs[i1]); break; }

                if (Marker.Count == 1)
                    Eff.Add(Marker[0].WriteMP("Marker"));
                else if (Marker.Count > 1)
                {
                    MsgPack Markers = new MsgPack(Marker.Count, "Markers");
                    for (int i = 0; i < Marker.Count; i++)
                        Markers[i] = Marker[i].WriteMP();
                    Eff.Add(Markers);
                }

                Eff.Add(Data.Value.WriteMP());

                return Eff;
            }
        }

        public override string ToString() => Name.Value;
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

    public struct AnimationData3D
    {
        public KeyFrameEntry Unk1      ;
        public KeyFrameEntry Unk2      ;
        public KeyFrameEntry RotReturnX;
        public KeyFrameEntry RotReturnY;
        public KeyFrameEntry RotReturnZ;
        public KeyFrameEntry  RotationX;
        public KeyFrameEntry  RotationY;
        public KeyFrameEntry     ScaleZ;

        public void ReadMP(MsgPack msg)
        {
            Unk1      .ReadMP(msg, "Unk1"      );
            Unk2      .ReadMP(msg, "Unk2"      );
            RotReturnX.ReadMP(msg, "RotReturnX");
            RotReturnY.ReadMP(msg, "RotReturnY");
            RotReturnZ.ReadMP(msg, "RotReturnZ");
             RotationX.ReadMP(msg,  "RotationX");
             RotationY.ReadMP(msg,  "RotationY");
                ScaleZ.ReadMP(msg,     "ScaleZ");
        }

        public MsgPack WriteMP()
        {
            MsgPack _3D = new MsgPack("3D");
            _3D.Add(Unk1      .WriteMP("Unk1"      ));
            _3D.Add(Unk2      .WriteMP("Unk2"      ));
            _3D.Add(RotReturnX.WriteMP("RotReturnX"));
            _3D.Add(RotReturnY.WriteMP("RotReturnY"));
            _3D.Add(RotReturnZ.WriteMP("RotReturnZ"));
            _3D.Add( RotationX.WriteMP( "RotationX"));
            _3D.Add( RotationY.WriteMP( "RotationY"));
            _3D.Add(    ScaleZ.WriteMP(    "ScaleZ"));
            return _3D;
        }
    }

    public struct AnimationData
    {
        public BlendMode Mode;
        public byte Unk0;
        public bool UseTextureMask;
        public byte Unk1;
        public KeyFrameEntry   OriginX;
        public KeyFrameEntry   OriginY;
        public KeyFrameEntry PositionX;
        public KeyFrameEntry PositionY;
        public KeyFrameEntry Rotation;
        public KeyFrameEntry    ScaleX;
        public KeyFrameEntry    ScaleY;
        public KeyFrameEntry Opacity;
        public Pointer<AnimationData3D> _3D;

        public void ReadMP(MsgPack msg)
        {
            if (!msg.Element("AnimationData", out MsgPack Data)) return;

            System.Enum.TryParse(Data.ReadString("BlendMode"), out Mode);
            Unk0           = Data.ReadUInt8  ("Unk0");
            UseTextureMask = Data.ReadBoolean("UseTextureMask");
            Unk1           = Data.ReadUInt8  ("Unk1");
            
              OriginX.ReadMP(Data,   "OriginX");
              OriginY.ReadMP(Data,   "OriginY");
            PositionX.ReadMP(Data, "PositionX");
            PositionY.ReadMP(Data, "PositionY");
            Rotation .ReadMP(Data, "Rotation" );
               ScaleX.ReadMP(Data,    "ScaleX");
               ScaleY.ReadMP(Data,    "ScaleY");
            Opacity  .ReadMP(Data, "Opacity"  );

            this._3D.Offset = -1;
            if (Data.Element("3D", out MsgPack _3D))
            { this._3D.Offset = 0; this._3D.Value.ReadMP(_3D); }
        }

        public MsgPack WriteMP()
        {
            MsgPack AnimationData = new MsgPack("AnimationData")
                .Add("BlendMode", Mode.ToString()).Add("Unk0", Unk0)
                .Add("UseTextureMask", UseTextureMask) .Add("Unk1", Unk1);

            AnimationData.Add(  OriginX.WriteMP(  "OriginX"));
            AnimationData.Add(  OriginY.WriteMP(  "OriginY"));
            AnimationData.Add(PositionX.WriteMP("PositionX"));
            AnimationData.Add(PositionY.WriteMP("PositionY"));
            AnimationData.Add(Rotation .WriteMP("Rotation" ));
            AnimationData.Add(   ScaleX.WriteMP(   "ScaleX"));
            AnimationData.Add(   ScaleY.WriteMP(   "ScaleY"));
            AnimationData.Add(Opacity  .WriteMP("Opacity"  ));
            if (_3D.Offset > 0)
                AnimationData.Add(_3D.Value.WriteMP());
            return AnimationData;
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
            if (!msg.ElementArray(name, out MsgPack Temp)) return;
            
            Count = Temp.Array.Length;
            if (Count == 0) return;
            ArrValue      = new float[Count];
            KeyFrame      = new float[Count];
            Interpolation = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                KeyFrame     [i] = Temp[i].ReadSingle("KeyFrame"     );
                ArrValue     [i] = Temp[i].ReadSingle("Value"        );
                Interpolation[i] = Temp[i].ReadSingle("Interpolation");
            }
        }

        public MsgPack WriteMP(string name)
        {
            if (Count  < 1) return MsgPack.Null;
            if (Count == 1) return new MsgPack(name, Value);
            MsgPack KFE = new MsgPack(Count, name);
            for (int i = 0; i < Count; i++) KFE[i] = MsgPack.New.Add("KeyFrame", KeyFrame[i])
                    .Add("Value", ArrValue[i]).Add("Interpolation", Interpolation[i]);
            return KFE;
        }
    }

    public struct SpriteEntry
    {
        public int Offset;
        public int Unknown;
        public short Width;
        public short Height;
        public float Frames;
        public CountPointer<Sprite> Sprites;

        public void ReadMP(MsgPack msg)
        {
            Unknown = msg.ReadInt32 ("Unknown");
            Width   = msg.ReadInt16 ("Width"  );
            Height  = msg.ReadInt16 ("Height" );
            Frames  = msg.ReadSingle("Frames" );

            if (msg.Element("Sprite", out MsgPack Sprite))
            {
                Sprites.Count = 1;
                Sprites[0] = new Sprite() { Name = new Pointer<string>()
                { Value = Sprite.ReadString("Name") }, ID = Sprite.ReadInt32("ID") };
            }
            else if (msg.ElementArray("Sprites", out MsgPack Sprites))
            {
                this.Sprites.Count = Sprites.Array.Length;
                for (int i0 = 0; i0 < this.Sprites.Count; i0++)
                    this.Sprites[i0] = new Sprite() { Name = new Pointer<string>()
                        { Value = Sprites[i0].ReadString("Name") }, ID = Sprites[i0].ReadInt32("ID") };
            }
        }

        public MsgPack WriteMP(int Id)
        {
            MsgPack SpriteEntry = MsgPack.New.Add("ID", Id)
                .Add("Width", Width).Add("Height", Height).Add("Unknown", Unknown);

            if (Sprites.Count > 0) SpriteEntry.Add("Frames", Frames);

            if (Sprites.Count == 1)
                SpriteEntry.Add(new MsgPack("Sprite").Add("Name",
                    Sprites[0].Name.Value).Add("ID", Sprites[0].ID));
            else if (Sprites.Count > 1)
            {
                MsgPack Sprites = new MsgPack(this.Sprites.Count, "Sprites");
                for (int i0 = 0; i0 < this.Sprites.Count; i0++)
                    Sprites[i0] = MsgPack.New.Add("Name", 
                        this.Sprites[i0].Name.Value).Add("ID", this.Sprites[i0].ID);
                SpriteEntry.Add(Sprites);
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

        public override string ToString() => "ID: " + ID + "; Name: " + Name;
    }
}
