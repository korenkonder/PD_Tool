//Original: AetSet.bt Version: 2.0 by samyuu

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
                    Aet.Layers[i].Objects[i0].ID = i1;
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
                                    if (Aet.Layers[I][I0].ID == Obj.Pic.Value.ParentObjectID)
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
                                    if (Aet.Layers[I][I0].ID == Obj.Eff.Value.ParentObjectID)
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
            if (!MsgPack.ElementArray("Aet", out MsgPack Aet)) { MsgPack.Dispose(); return; }

            if (Aet.Array != null)
                if (Aet.Array.Length > 0)
                {
                    AET.Data = new Pointer<AetData>[Aet.Array.Length];
                    for (int i = 0; i < AET.Data.Length; i++)
                        MsgPackReader(Aet.Array[i], ref AET.Data[i]);
                }

            MsgPack.Dispose();
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
                Aet.Layers.Entries[i].Count = Temp.Array.Length;
                for (i0 = 0; i0 < Aet.Layers[i].Count; i0++)
                    Aet.Layers.Entries[i].Objects[i0].ReadMP(Temp[i0]);
            }
            else if (AET.Element("RootLayer", out Temp))
            {
                Aet.Layers.Entries[i].Count = 1;
                Aet.Layers.Entries[i].Objects[0].ReadMP(Temp);
            }
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
                AET.Add(Aet.Layers[i][0].WriteMP(ref Aet, "RootLayer"));
            if (Aet.Layers[i].Count > 1)
            {
                MsgPack RootLayer = new MsgPack(Aet.Layers[i].Count, "RootLayer");
                for (i0 = 0; i0 < Aet.Layers[i].Count; i0++)
                    RootLayer[i0] = Aet.Layers[i][i0].WriteMP(ref Aet);
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
    
    public static class AetExt
    {
        private readonly static System.Text.Encoding ShiftJIS = System.Text.Encoding.GetEncoding(932);

        public static Pointer<string> ReadPointerStringShiftJIS(this Stream IO)
        { Pointer<string> val = IO.ReadPointer<string>();
            val.Value = ShiftJIS.GetString(IO.ReadAtOffset(val.Offset)); return val; }

        public static void WriteShiftJIS(this Stream IO, string String) =>
            IO.Write(ShiftJIS.GetBytes(String));
        
        public static MsgPack ReadMP(this MsgPack msg, ref CountPointer<float> val, string Name)
        {
            val.Offset = 0;
            val.Entries = null;
            if (msg.ContainsKey(Name))
            {
                val.Count = 1;
                val[0] = msg.ReadSingle(Name);
            }
            else if(msg.ElementArray(Name, out MsgPack Temp))
            {
                val.Count = Temp.Array.Length;
                for (int i = 0; i < val.Count; i++)
                    val[i] = Temp[i].ReadSingle();
            }
            return msg;
        }

        public static MsgPack Add(this MsgPack MsgPack, string Name, CountPointer<float> val)
        {
            if (val.Count == 1) return MsgPack.Add(new MsgPack(Name, val[0]));
            else if (val.Count > 1)
            {
                MsgPack Val = new MsgPack(val.Count, Name);
                for (int i = 0; i < val.Count; i++) Val[i] = (MsgPack)val[i];
                return MsgPack.Add(Val);
            }
            return MsgPack;
        }

        public static AetObject ReadAetObject(this Stream IO)
        {
            AetObject Anim = new AetObject() { Offset = IO.Position };
            Anim.Name = IO.ReadPointerString();
            Anim.StartTime0    = IO.ReadSingle();
            Anim.LoopDuration  = IO.ReadSingle();
            Anim.StartTime1    = IO.ReadSingle();
            Anim.PlaybackSpeed = IO.ReadSingle();
            Anim.Unk0 = IO.ReadByte();
            Anim.Unk1 = IO.ReadByte();
            Anim.Unk2 = IO.ReadByte();
            Anim.Type = (AetObject.ObjType)IO.ReadByte();
                 if (Anim.Type == AetObject.ObjType.Pic)
            {
                Anim.Pic = new Pointer<AetObject.Picture> { Offset = IO.Position,
                    Value = new AetObject.Picture
                    {
                        SpriteEntryID = IO.ReadInt32(),
                        ParentObjectID = IO.ReadInt32(),
                        Marker = IO.ReadCountPointer<Marker>()
                    }
                };
                ref AetObject.Picture Pic = ref Anim.Pic.Value;
                Pic.Data.Offset = IO.ReadInt32();
                IO.ReadAnimData(ref Pic.Data);

                IO.Position = Pic.Marker.Offset;
                for (int i = 0; i < Pic.Marker.Count; i++)
                    Pic.Marker[i] = new Marker()
                    { Frame = IO.ReadSingle(), Name = IO.ReadPointerStringShiftJIS() };
            }
            else if (Anim.Type == AetObject.ObjType.Aif)
            {
                Anim.Aif = new Pointer<AetObject.Audio> { Offset = IO.Position,
                    Value = new AetObject.Audio
                    {
                        Value = IO.ReadPointer<int>(),
                        Zero0 = IO.ReadInt32(),
                        Zero1 = IO.ReadInt32(),
                        Zero2 = IO.ReadInt32(),
                        Zero3 = IO.ReadInt32(),
                        Pointer = IO.ReadInt32(),
                    }
                };

                ref AetObject.Audio Aif = ref Anim.Aif.Value; 

                IO.Position = Aif.Value.Offset;
                Aif.Value.Value = IO.ReadInt32();

                IO.Position = Aif.Pointer;
                Aif.Unk0 = IO.ReadCountPointer<float>();
                Aif.Unk1 = IO.ReadCountPointer<float>();
                Aif.Unk2 = IO.ReadCountPointer<float>();
                Aif.Unk3 = IO.ReadCountPointer<float>();

                IO.Position = Aif.Unk0.Offset;
                for (int i = 0; i < Aif.Unk0.Count; i++) Aif.Unk0[i] = IO.ReadSingle();
                IO.Position = Aif.Unk1.Offset;
                for (int i = 0; i < Aif.Unk1.Count; i++) Aif.Unk1[i] = IO.ReadSingle();
                IO.Position = Aif.Unk2.Offset;
                for (int i = 0; i < Aif.Unk2.Count; i++) Aif.Unk2[i] = IO.ReadSingle();
                IO.Position = Aif.Unk3.Offset;
                for (int i = 0; i < Aif.Unk3.Count; i++) Aif.Unk3[i] = IO.ReadSingle();
            }
            else if (Anim.Type == AetObject.ObjType.Eff)
            {
                Anim.Eff = new Pointer<AetObject.Effect> { Offset = IO.Position,
                    Value = new AetObject.Effect
                    {
                        LayerID = IO.ReadInt32(),
                        ParentObjectID = IO.ReadInt32(),
                        Marker = IO.ReadCountPointer<Marker>()
                    }
                };
                ref AetObject.Effect Eff = ref Anim.Eff.Value;
                Eff.Data.Offset = IO.ReadInt32();
                IO.Position = Eff.Marker.Offset;
                for (int i = 0; i < Eff.Marker.Count; i++)
                    Eff.Marker[i] = new Marker()
                    { Frame = IO.ReadSingle(), Name = IO.ReadPointerStringShiftJIS() };
                IO.ReadAnimData(ref Eff.Data);
            }
            IO.Position = Anim.Offset + 0x30;
            return Anim;
        }
        
        public static void Write(this Stream IO, ref AetObject Anim, ref KKdList<int> NullValPointers)
        {
                 if (Anim.Type == AetObject.ObjType.Pic)
            {
                ref AetObject.Picture Pic = ref Anim.Pic.Value;
                if (Pic.Marker.Count > 0)
                {
                    Pic.Marker.Offset = IO.Position;
                    for (int i = 0; i < Pic.Marker.Count; i++) IO.Write(0L);
                }
                IO.Write(ref Pic.Data, ref NullValPointers);
            }
            else if (Anim.Type == AetObject.ObjType.Aif)
            {
                ref AetObject.Audio Aif = ref Anim.Aif.Value;
                if (Aif.Unk0.Count > 0) { Aif.Unk0.Offset = IO.Position;
                    for (int i = 0; i < Aif.Unk0.Count; i++) IO.Write(Aif.Unk0[i]); }
                if (Aif.Unk1.Count > 0) { Aif.Unk1.Offset = IO.Position;
                    for (int i = 0; i < Aif.Unk1.Count; i++) IO.Write(Aif.Unk1[i]); }
                if (Aif.Unk2.Count > 0) { Aif.Unk2.Offset = IO.Position;
                    for (int i = 0; i < Aif.Unk2.Count; i++) IO.Write(Aif.Unk2[i]); }
                if (Aif.Unk3.Count > 0) { Aif.Unk3.Offset = IO.Position;
                    for (int i = 0; i < Aif.Unk3.Count; i++) IO.Write(Aif.Unk3[i]); }

                IO.Align(0x10);
                Aif.Value.Offset = IO.Position;
                IO.Write(Aif.Value.Value);

                IO.Align(0x10);
                Aif.Pointer = IO.Position;
                if (Aif.Unk0.Count > 0) { IO.Write(Aif.Unk0.Count); IO.Write(Aif.Unk0.Offset); } else IO.Write(0L);
                if (Aif.Unk1.Count > 0) { IO.Write(Aif.Unk1.Count); IO.Write(Aif.Unk1.Offset); } else IO.Write(0L);
                if (Aif.Unk2.Count > 0) { IO.Write(Aif.Unk2.Count); IO.Write(Aif.Unk2.Offset); } else IO.Write(0L);
                if (Aif.Unk3.Count > 0) { IO.Write(Aif.Unk3.Count); IO.Write(Aif.Unk3.Offset); } else IO.Write(0L);
            }
            else if (Anim.Type == AetObject.ObjType.Eff)
            {
                ref AetObject.Effect Eff = ref Anim.Eff.Value;
                if (Eff.Marker.Count > 0)
                {
                    Eff.Marker.Offset = IO.Position;
                    for (int i = 0; i < Eff.Marker.Count; i++) IO.Write(0L);
                }
                IO.Write(ref Eff.Data, ref NullValPointers);
            }
        }

        public static void Write(this Stream IO, ref AetObject Anim)
        {
            Anim.Offset = IO.Position;
            IO.Write(0);
            IO.Write(Anim.StartTime0   );
            IO.Write(Anim.LoopDuration );
            IO.Write(Anim.StartTime1   );
            IO.Write(Anim.PlaybackSpeed);
            IO.Write(Anim.Unk0);
            IO.Write(Anim.Unk1);
            IO.Write(Anim.Unk2);
            IO.Write((byte)Anim.Type);
                 if (Anim.Type == AetObject.ObjType.Pic)
            {
                Anim.Pic.Offset = IO.Position;
                IO.Write(0L);
                if (Anim.Pic.Value.Marker.Count > 0)
                {
                    IO.Write(Anim.Pic.Value.Marker.Count );
                    IO.Write(Anim.Pic.Value.Marker.Offset);
                }
                else IO.Write(0L);
                IO.Write(Anim.Pic.Value.Data  .Offset);
                IO.Write(0);
            }
            else if (Anim.Type == AetObject.ObjType.Aif)
            {
                Anim.Aif.Offset = IO.Position;
                IO.Write(Anim.Aif.Value.Value.Offset);
                IO.Write(0L);
                IO.Write(0L);
                IO.Write(Anim.Aif.Value.Pointer);
            }
            else if (Anim.Type == AetObject.ObjType.Eff)
            {
                Anim.Eff.Offset = IO.Position;
                IO.Write(0L);
                if (Anim.Eff.Value.Marker.Count > 0)
                {
                    IO.Write(Anim.Eff.Value.Marker.Count );
                    IO.Write(Anim.Eff.Value.Marker.Offset);
                }
                else IO.Write(0L);
                IO.Write(Anim.Eff.Value.Data.Offset);
                IO.Write(0);
            }
            else { IO.Write(0L); IO.Write(0L); IO.Write(0L); }
        }

        public static void ReadAnimData(this Stream IO, ref Pointer<AnimationData> Anim)
        {
            ref AnimationData Data = ref Anim.Value;
            IO.Position = Anim.Offset;
            Data.Mode = (AnimationData.BlendMode)IO.ReadByte();
            Data.Unk0           = IO.ReadByte();
            Data.UseTextureMask = IO.ReadBoolean();
            Data.Unk1           = IO.ReadByte();
            Data.  OriginX = IO.ReadKeyFrameEntry();
            Data.  OriginY = IO.ReadKeyFrameEntry();
            Data.PositionX = IO.ReadKeyFrameEntry();
            Data.PositionY = IO.ReadKeyFrameEntry();
            Data.Rotation  = IO.ReadKeyFrameEntry();
            Data.   ScaleX = IO.ReadKeyFrameEntry();
            Data.   ScaleY = IO.ReadKeyFrameEntry();
            Data.Opacity   = IO.ReadKeyFrameEntry();
            Data._3D = IO.ReadPointer<AnimationData3D>();

            IO.ReadKeyFrameEntry(ref Data.  OriginX);
            IO.ReadKeyFrameEntry(ref Data.  OriginY);
            IO.ReadKeyFrameEntry(ref Data.PositionX);
            IO.ReadKeyFrameEntry(ref Data.PositionY);
            IO.ReadKeyFrameEntry(ref Data.Rotation );
            IO.ReadKeyFrameEntry(ref Data.   ScaleX);
            IO.ReadKeyFrameEntry(ref Data.   ScaleY);
            IO.ReadKeyFrameEntry(ref Data.Opacity  );

            if (Data._3D.Offset > 0)
            {
                IO.Position = Data._3D.Offset;
                Data._3D.Value.Unk1       = IO.ReadKeyFrameEntry();
                Data._3D.Value.Unk2       = IO.ReadKeyFrameEntry();
                Data._3D.Value.RotReturnX = IO.ReadKeyFrameEntry();
                Data._3D.Value.RotReturnY = IO.ReadKeyFrameEntry();
                Data._3D.Value.RotReturnZ = IO.ReadKeyFrameEntry();
                Data._3D.Value. RotationX = IO.ReadKeyFrameEntry();
                Data._3D.Value. RotationY = IO.ReadKeyFrameEntry();
                Data._3D.Value.    ScaleZ = IO.ReadKeyFrameEntry();

                IO.ReadKeyFrameEntry(ref Data._3D.Value.Unk1      );
                IO.ReadKeyFrameEntry(ref Data._3D.Value.Unk2      );
                IO.ReadKeyFrameEntry(ref Data._3D.Value.RotReturnX);
                IO.ReadKeyFrameEntry(ref Data._3D.Value.RotReturnY);
                IO.ReadKeyFrameEntry(ref Data._3D.Value.RotReturnZ);
                IO.ReadKeyFrameEntry(ref Data._3D.Value. RotationX);
                IO.ReadKeyFrameEntry(ref Data._3D.Value. RotationY);
                IO.ReadKeyFrameEntry(ref Data._3D.Value.    ScaleZ);
            }
        }

        public static void Write(this Stream IO, ref Pointer<AnimationData> Anim, ref KKdList<int> NullValPointers)
        {
            ref AnimationData Data = ref Anim.Value;

            int SubFlags = 0;
            if (Data._3D.Offset > -1)
            {
                SubFlags |= IO.Write(ref Data._3D.Value.Unk1      ) ? 0b10000000 : 0;
                SubFlags |= IO.Write(ref Data._3D.Value.Unk2      ) ? 0b01000000 : 0;
                SubFlags |= IO.Write(ref Data._3D.Value.RotReturnX) ? 0b00100000 : 0;
                SubFlags |= IO.Write(ref Data._3D.Value.RotReturnY) ? 0b00010000 : 0;
                SubFlags |= IO.Write(ref Data._3D.Value.RotReturnZ) ? 0b00001000 : 0;
                SubFlags |= IO.Write(ref Data._3D.Value. RotationX) ? 0b00000100 : 0;
                SubFlags |= IO.Write(ref Data._3D.Value. RotationY) ? 0b00000010 : 0;
                SubFlags |= IO.Write(ref Data._3D.Value.    ScaleZ) ? 0b00000001 : 0;
            }

            int Flags = 0;
            Flags |= IO.Write(ref Data.  OriginX) ? 0b10000000 : 0;
            Flags |= IO.Write(ref Data.  OriginY) ? 0b01000000 : 0;
            Flags |= IO.Write(ref Data.PositionX) ? 0b00100000 : 0;
            Flags |= IO.Write(ref Data.PositionY) ? 0b00010000 : 0;
            Flags |= IO.Write(ref Data.Rotation ) ? 0b00001000 : 0;
            Flags |= IO.Write(ref Data.   ScaleX) ? 0b00000100 : 0;
            Flags |= IO.Write(ref Data.   ScaleY) ? 0b00000010 : 0;
            Flags |= IO.Write(ref Data.Opacity  ) ? 0b00000001 : 0;

            if (Data._3D.Offset > -1)
            {
                IO.Align(0x20);
                Data._3D.Offset = IO.Position;
                IO.Write(ref Data._3D.Value, ref NullValPointers, SubFlags);
            }

            IO.Align(0x20);
            Anim.Offset = IO.Position;
            IO.WriteByte((byte) Data.Mode);
            IO.WriteByte(       Data.Unk0);
            IO.WriteByte((byte)(Data.UseTextureMask ? 1 : 0));
            IO.WriteByte(       Data.Unk1);

            IO.Write(ref Data, ref NullValPointers, Flags);
            IO.Write(Data._3D.Offset > -1 ? Data._3D.Offset : 0);
        }

        public static void Write(this Stream IO, ref AnimationData Data,
            ref KKdList<int> NullValPointers, int Flags)
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

        public static void Write(this Stream IO, ref AnimationData3D Data,
            ref KKdList<int> NullValPointers, int Flags)
        {
            IO.Write(Data.Unk1      .Count );
            if ((Flags & 0b10000000) == 0b10000000) NullValPointers.Add(IO.Position);
            IO.Write(Data.Unk1      .Offset);
            IO.Write(Data.Unk2      .Count );
            if ((Flags & 0b01000000) == 0b01000000) NullValPointers.Add(IO.Position);
            IO.Write(Data.Unk2      .Offset);
            IO.Write(Data.RotReturnX.Count );
            if ((Flags & 0b00100000) == 0b00100000) NullValPointers.Add(IO.Position);
            IO.Write(Data.RotReturnX.Offset);
            IO.Write(Data.RotReturnY.Count );
            if ((Flags & 0b00010000) == 0b00010000) NullValPointers.Add(IO.Position);
            IO.Write(Data.RotReturnY.Offset);
            IO.Write(Data.RotReturnZ.Count );
            if ((Flags & 0b00001000) == 0b00001000) NullValPointers.Add(IO.Position);
            IO.Write(Data.RotReturnZ.Offset);
            IO.Write(Data. RotationX.Count );
            if ((Flags & 0b00000100) == 0b00000100) NullValPointers.Add(IO.Position);
            IO.Write(Data. RotationX.Offset);
            IO.Write(Data. RotationY.Count );
            if ((Flags & 0b00000010) == 0b00000010) NullValPointers.Add(IO.Position);
            IO.Write(Data. RotationY.Offset);
            IO.Write(Data.    ScaleZ.Count );
            if ((Flags & 0b00000001) == 0b00000001) NullValPointers.Add(IO.Position);
            IO.Write(Data.    ScaleZ.Offset);
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
            if (entry.Count == 1) if (entry.Value == 0) return  true;
                      else { IO.Write(entry.Value);     return false; }  

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
            else if (msg.ElementArray("Markers", out MsgPack Markers))
            {
                mark.Count = Markers.Array.Length;
                for (int i = 0; i < mark.Count; i++)
                    mark.Entries[i].ReadMP(Markers[i]);
            }
        }

        public static void WritePointerString(this Stream IO,
            ref System.Collections.Generic.Dictionary<string, int> Dict, ref Pointer<string> Str)
        {
            if (Dict.ContainsKey(Str.Value)) Str.Offset = Dict[Str.Value];
            else
            { Str.Offset = IO.Position; Dict.Add(Str.Value, Str.Offset); IO.WriteShiftJIS(Str.Value + "\0"); }
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
            set => Objects = new AetObject[value];
        }
        public int Offset;
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
                return MsgPack.New.Add("ID", Id).Add(this.Objects[0].WriteMP(ref Aet, "Object"));
            MsgPack Objects = new MsgPack(Count, "Objects");
            for (int i = 0; i < Count; i++)
                Objects[i] = this.Objects[i].WriteMP(ref Aet);
            return MsgPack.New.Add("ID", Id).Add(Objects);
        }

        public AetObject this[int index]
        {
            get =>    Count > 0  ? Objects[index] : default;
            set { if (Count > 0) { Objects[index] =   value; } }
        }
        
        public override string ToString() => "Count: " + Count;
    }

    public struct AetObject
    {
        public int ID;
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

        public void ReadMP(MsgPack msg)
        {
            ID            = msg.ReadInt32 ("ID"           );
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
        }

        public MsgPack WriteMP(ref AetData Aet, string name = null)
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
                            { Pic.Add("ParentObjectID", Aet.Layers[i][i1].ID); break; }

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
                            { Eff.Add("ParentObjectID", Aet.Layers[i][i1].ID); break; }

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
