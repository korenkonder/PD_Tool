//Original: AetSet.bt Version: 3.4 by samyuu

using KKdBaseLib;
using KKdBaseLib.Aet;
using KKdMainLib.IO;

namespace KKdMainLib.Aet
{
    public class Aet
    {
        public Aet()
        { AET = new AetHeader(); IO = null; }

        public AetHeader AET;
        private Stream IO;
        private int i, i0, i1, i2;
        private const string x00 = "\0";

        public void AETReader(string file)
        {
            AET = new AetHeader();
            IO = File.OpenReader(file + ".bin", true);

            int i = 0;
            int Pos = -1;
            while (true) { Pos = IO.RI32(); if (Pos != 0 && Pos < IO.L) i++; else break; }

            IO.P = 0;
            AET.Data = new Pointer<AetData>[i];
            for (i = 0; i < AET.Data.Length; i++) AET.Data[i] = IO.RP<AetData>();
            for (i = 0; i < AET.Data.Length; i++) AETReader(ref AET.Data[i]);

            IO.C();
        }

        public void AETWriter(string file)
        {
            if (AET.Data == null || AET.Data.Length == 0) return;
            IO = File.OpenWriter();

            for (int i = 0; i < AET.Data.Length; i++)
            {
                IO.P = IO.L + 0x20;
                AETWriter(ref AET.Data[i]);
            }

            IO.P = 0;
            for (int i = 0; i < AET.Data.Length; i++)
                if (AET.Data[i].O > 0) IO.W(AET.Data[i].O);
            byte[] data = IO.ToArray();
            IO.Dispose();

            using (IO = File.OpenWriter(file + ".bin", true))
                IO.W(data);
            data = null;
        }

        private void AETReader(ref Pointer<AetData> AET)
        {
            IO.P = AET.O;
            ref AetData aet = ref AET.V;

            aet.Name = IO.RPS();
            aet.StartFrame    = IO.RF32();
            aet.FrameDuration = IO.RF32();
            aet.FrameRate     = IO.RF32();
            aet.BackColor = IO.RU32();
            aet.Width     = IO.RU32();
            aet.Height    = IO.RU32();
            aet.Position  = IO.RP<Vector2<CountPointer<KFT2>>>();
            aet.Layers    = IO.ReadCountPointer<AetLayer >();
            aet.Regions   = IO.ReadCountPointer<AetRegion>();
            aet.Sounds    = IO.ReadCountPointer<AetSound >();

            if (aet.Position.O > 0)
            {
                IO.P = aet.Position.O;
                aet.Position.V.X.O = IO.RI32();
                aet.Position.V.Y.O = IO.RI32();
                RKF(ref aet.Position.V.X);
                RKF(ref aet.Position.V.Y);
            }

            if (aet.Sounds.O > 0)
            {
                IO.P = aet.Sounds.O;
                for (i = 0; i < aet.Sounds.C; i++)
                {
                    ref AetSound aif = ref aet.Sounds.E[i];
                    aif.O = IO.P;
                    aif.Unk = IO.RU32();
                }
            }

            IO.P = aet.Layers.O;
            for (i = 0; i < aet.Layers.C; i++)
                aet.Layers[i] = new AetLayer() { P = IO.P, C = IO.RI32(), O = IO.RI32()};

            i1 = 0;
            RAL(ref aet.Layers.E[aet.Layers.C - 1]);
            for (i = 0; i < aet.Layers.C - 1; i++)
                RAL(ref aet.Layers.E[i]);

            IO.P = aet.Regions.O;
            for (i = 0; i < aet.Regions.C; i++)
                aet.Regions[i] = new AetRegion { O = IO.P, Color = IO.RU32(), Width = IO.RU16(),
                    Height = IO.RU16(), Frames = IO.RF32(), Sprites = IO.ReadCountPointer<Sprite>() };

            for (i = 0; i < aet.Regions.C; i++)
            {
                IO.P = aet.Regions[i].Sprites.O;
                for (i0 = 0; i0 < aet.Regions[i].Sprites.C; i0++)
                    aet.Regions.E[i].Sprites[i0] = new Sprite { Name = IO.RPS(), ID = IO.RU32() };
            }
            
            for (i = 0; i < aet.Layers.C; i++)
            {
                for (i0 = 0; i0 < aet.Layers[i].C; i0++)
                {
                    ref AetObject obj = ref aet.Layers[i].E[i0];
                    int DataID = obj.DataID;
                    if (DataID > 0)
                    {
                        if (obj.Type == AetObject.AetObjType.Pic)
                        {
                            for (i1 = 0; i1 < aet.Regions.C; i1++)
                                if (aet.Regions[i1].O == DataID) { obj.DataID = i1; break; }
                        }
                        else if (obj.Type == AetObject.AetObjType.Aif)
                        {
                            for (i1 = 0; i1 < aet.Sounds.C; i1++)
                                if (aet. Sounds[i1].O == DataID) { obj.DataID = i1; break; }
                        }
                        else if (obj.Type == AetObject.AetObjType.Eff)
                        {
                            for (i1 = 0; i1 < aet.Layers.C; i1++)
                                if (aet. Layers[i1].P == DataID) { obj.DataID = i1; break; }
                        }
                    }
                    if (DataID == obj.DataID) obj.DataID = -1;

                    int ParentObjectID = obj.ParentObjectID;
                    if (ParentObjectID > 0)
                        for (i1 = 0; i1 < aet.Layers.C; i1++)
                        {
                            for (i2 = 0; i2 < aet.Layers[i1].C; i2++)
                                if (aet.Layers[i1].E[i2].Offset == ParentObjectID)
                                { obj.ParentObjectID = aet.Layers[i1].E[i2].ID; break; }
                            if (ParentObjectID != obj.ParentObjectID) break;
                        }
                    if (ParentObjectID == obj.ParentObjectID) obj.ParentObjectID = -1;
                }
            }
        }

        private void AETWriter(ref Pointer<AetData> AET)
        {
            ref AetData aet = ref AET.V;
            
            for (i = 0; i < aet.Regions.C; i++)
            {
                ref AetRegion region = ref aet.Regions.E[i];
                if (region.Sprites.C == 0) { region.Sprites.O = 0; continue; }
                if (region.Sprites.C > 1) IO.A(0x20);
                region.Sprites.O = IO.P;
                for (i0 = 0; i0 < region.Sprites.C; i0++) IO.W(0L);
            }

            IO.A(0x20);
            aet.Regions.O = IO.P;
            for (i = 0; i < aet.Regions.C; i++)
            {
                ref AetRegion region = ref aet.Regions.E[i];
                region.O = IO.P;
                IO.W(region.Color);
                IO.W(region.Width);
                IO.W(region.Height);
                IO.W(region.Frames);
                IO.W(region.Sprites);
            }

            IO.A(0x20);
            aet.Layers.O = IO.P;
            for (i = 0; i < aet.Layers.C; i++)
            {
                aet.Layers.E[i].O = IO.P;
                IO.W(0L);
            }

            for (i = 0; i < aet.Layers.C; i++)
            {
                if (aet.Layers.E[i].C < 1) continue;

                for (i0 = 0; i0 < aet.Layers.E[i].C; i0++)
                {
                    ref AetObject obj = ref aet.Layers.E[i].E[i0];
                    ref AnimationData data = ref obj.Data.V;
                    ref AnimationData.ThirdDimension _3D = ref data._3D.V;
                    ref AetObject.AetObjExtraData extraData = ref obj.ExtraData.V;
                    if (obj.Marker.C > 0)
                    {
                        if (obj.Marker.C > 3) IO.A(0x20);
                        obj.Marker.O = IO.P;
                        for (i1 = 0; i1 < obj.Marker.C; i1++)
                            IO.W(0L);
                    }

                    if (obj.Data.V._3D.O > 0)
                    {
                        W(ref _3D.Unk1      );
                        W(ref _3D.Unk2      );
                        W(ref _3D.RotReturnX);
                        W(ref _3D.RotReturnY);
                        W(ref _3D.RotReturnZ);
                        W(ref _3D. RotationX);
                        W(ref _3D. RotationY);
                        W(ref _3D.    ScaleZ);
                    }

                    if (obj.Data.O > 0)
                    {
                         W(ref data.  OriginX);
                         W(ref data.  OriginY);
                         W(ref data.PositionX);
                         W(ref data.PositionY);
                         W(ref data.Rotation );
                         W(ref data.   ScaleX);
                         W(ref data.   ScaleY);
                         W(ref data.Opacity  );
                    }
                    
                    if (obj.Data.V._3D.O > 0)
                    {
                        IO.A(0x20);
                        data._3D.O = IO.P;
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                    }
            
                    if (obj.ExtraData.O > 0)
                    {
                        W(ref extraData.Unk0);
                        W(ref extraData.Unk1);
                        W(ref extraData.Unk2);
                        W(ref extraData.Unk3);
                    }

                    if (obj.Data.O > 0)
                    {
                        IO.A(0x20);
                        obj.Data.O = IO.P;

                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                    }
            
                    if (obj.ExtraData.O > 0)
                    {
                        IO.A(0x20);
                        obj.ExtraData.O = IO.P;
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                        IO.W(0L);
                    }
                }

                IO.A(0x20);
                aet.Layers.E[i].E[0].Offset = IO.P;
                for (i0 = 0; i0 < aet.Layers.E[i].C; i0++)
                {
                    ref AetObject obj = ref aet.Layers.E[i].E[i0];
                    obj.Offset = IO.P;
                    IO.W(0L);
                    IO.W(0L);
                    IO.W(0L);
                    IO.W(0L);
                    IO.W(0L);
                    IO.W(0L);
                }
            }

            if (aet.Position.O > 0)
            {
                ref Vector2<CountPointer<KFT2>> pos = ref aet.Position.V;
                IO.A(0x10);
                W(ref pos.X);
                W(ref pos.Y);

                IO.A(0x20);
                aet.Position.O = IO.P;
                IO.W(pos.X);
                IO.W(pos.Y);

                W(ref pos.X);
                W(ref pos.Y);
            }

            IO.A(0x20);
            AET.O = IO.P;
            IO.W(0L);
            IO.W(0L);
            IO.W(0L);
            IO.W(0L);
            IO.W(0L);
            IO.W(0L);
            IO.W(0L);
            IO.W(0L);

            IO.A(0x10);
            {
                System.Collections.Generic.Dictionary<string, int> UsedValues =
                    new System.Collections.Generic.Dictionary<string, int>();

                for (i = 0; i < aet.Regions.C; i++)
                {
                    ref AetRegion region = ref aet.Regions.E[i];
                    for (i0 = 0; i0 < region.Sprites.C; i0++)
                        WPS(ref UsedValues, ref region.Sprites.E[i0].Name);
                }

                //IO.Align(0x4);
                for (i = 0; i < aet.Layers.C; i++)
                {
                    for (i0 = 0; i0 < aet.Layers.E[i].C; i0++)
                    {
                        ref AetObject obj = ref aet.Layers.E[i].E[i0];
                        for (i1 = 0; i1 < obj.Marker.C; i1++)
                            WPS(ref UsedValues, ref obj.Marker.E[i1].Name);
                    }

                    for (i0 = 0; i0 < aet.Layers.E[i].C; i0++)
                        WPS(ref UsedValues, ref aet.Layers.E[i].E[i0].Name);
                }

                //IO.Align(0x4);
                aet.Name.O = IO.P;
                IO.W(aet.Name.V + x00);
                IO.SL(IO.P);
            }

            aet.Sounds.O = 0;
            if (aet.Sounds.C > 0)
            {
                IO.A(0x10);
                aet.Sounds.O = IO.P;
                for (i = 0; i < aet.Sounds.C; i++)
                {
                    aet.Sounds.E[i].O = IO.P;
                    IO.W(aet.Sounds[i].Unk);
                }
            }

            IO.A(0x10);
            int ReturnPos = IO.P;
            int nvp = 0;

            IO.F();

            for (i = 0; i < aet.Layers.C; i++)
            {
                if (aet.Layers.E[i].C < 1) continue;

                for (i0 = 0; i0 < aet.Layers.E[i].C; i0++)
                {
                    ref AetObject obj = ref aet.Layers.E[i].E[i0];
                    ref AnimationData data = ref obj.Data.V;
                    ref AnimationData.ThirdDimension _3D = ref data._3D.V;
                    ref AetObject.AetObjExtraData extraData = ref obj.ExtraData.V;
                    
                    if (obj.Data.V._3D.O > 0)
                    {
                        IO.P = data._3D.O;
                        W(ref _3D.Unk1      );
                        W(ref _3D.Unk2      );
                        W(ref _3D.RotReturnX);
                        W(ref _3D.RotReturnY);
                        W(ref _3D.RotReturnZ);
                        W(ref _3D. RotationX);
                        W(ref _3D. RotationY);
                        W(ref _3D.    ScaleZ);
                    }

                    if (obj.Data.O > 0)
                    {
                        IO.P = obj.Data.O;
                        IO.W((byte) data.Mode);
                        IO.W((byte)0);
                        IO.W((byte)(data.UseTextureMask ? 1 : 0));
                        IO.W((byte)0);

                        W(ref data.  OriginX);
                        W(ref data.  OriginY);
                        W(ref data.PositionX);
                        W(ref data.PositionY);
                        W(ref data.Rotation );
                        W(ref data.   ScaleX);
                        W(ref data.   ScaleY);
                        W(ref data.Opacity  );
                        IO.W(data._3D.O);
                    }
            
                    if (obj.ExtraData.O > 0)
                    {
                        IO.P = obj.ExtraData.O;
                        W(ref extraData.Unk0);
                        W(ref extraData.Unk1);
                        W(ref extraData.Unk2);
                        W(ref extraData.Unk3);
                    }

                    void W(ref CountPointer<KFT2> keys)
                    {
                        if (keys.C < 1) IO.W(0L);
                        else { if (keys.C > 0 && keys.O == 0) { keys.O = ReturnPos + (nvp << 2); nvp++; } IO.W(keys); }
                        
                    }
                }
            }

            IO.P = ReturnPos;
            for (i = 0; i < nvp; i++)
                IO.W(0);
            IO.A(0x10, true);

            IO.F();

            for (i = 0; i < aet.Layers.C; i++)
                for (i0 = 0; i0 < aet.Layers.E[i].C; i0++)
                {
                    ref AetObject obj = ref aet.Layers.E[i].E[i0];

                    IO.P = obj.Offset;
                    IO.W(obj.Name.O);
                    IO.W(obj.LoopStart);
                    IO.W(obj.LoopEnd);
                    IO.W(obj.StartFrame);
                    IO.W(obj.PlaybackSpeed);
                    IO.W((ushort)obj.Flags);
                    IO.W(obj.Pad);
                    IO.W((byte)obj.Type);
                         if (obj.Type == AetObject.AetObjType.Pic) IO.W(aet.Regions[obj.DataID].O);
                    else if (obj.Type == AetObject.AetObjType.Aif) IO.W(aet.Sounds [obj.DataID].O);
                    else if (obj.Type == AetObject.AetObjType.Eff) IO.W(aet.Layers [obj.DataID].O);
                    else IO.W(0x00);

                    if (obj.ParentObjectID > -1)
                    {
                        bool Found = false;
                        for (i1 = 0; i1 < aet.Layers.C; i1++)
                            for (i2 = 0; i2 < aet.Layers.E[i1].C; i2++)
                                if (aet.Layers[i1].E[i2].ID == obj.ParentObjectID)
                                { Found = true; IO.W(aet.Layers.E[i1].E[i2].Offset); break; }
                        if (!Found) IO.W(0);
                    }
                    else IO.W(0);
                    IO.W(obj.Marker);
                    IO.W(obj.Data.O);
                    IO.W(obj.ExtraData.O);

                    ref CountPointer<Marker> Marker = ref obj.Marker;
                    IO.P = Marker.O;
                    for (i1 = 0; i1 < Marker.C; i1++)
                    {
                        IO.W(Marker[i1].Frame);
                        IO.W(Marker[i1].Name.O);
                    }
                }

            IO.F();

            for (i = 0; i < aet.Regions.C; i++)
            {
                ref AetRegion region = ref aet.Regions.E[i];
                if (region.Sprites.C == 0) continue;
                IO.P = region.Sprites.O;
                for (i0 = 0; i0 < region.Sprites.C; i0++)
                {
                    IO.W(region.Sprites[i0].Name.O);
                    IO.W(region.Sprites[i0].ID);
                }
            }

            IO.P = aet.Layers.O;
            for (i = 0; i < aet.Layers.C; i++)
            {
                if (aet.Layers[i].C > 0)
                {
                    IO.W(aet.Layers[i].C);
                    IO.W(aet.Layers[i].E[0].Offset);
                }
                else IO.W(0L);
            }

            IO.P = AET.O;
            IO.W(aet.Name.O);
            IO.W(aet.StartFrame);
            IO.W(aet.FrameDuration);
            IO.W(aet.FrameRate);
            IO.W(aet.BackColor);
            IO.W(aet.Width);
            IO.W(aet.Height);
            IO.W(aet.Position.O);
            IO.W(aet. Layers);
            IO.W(aet.Regions);
            IO.W(aet. Sounds);
            IO.W(0L);
        }

        private void RAL(ref AetLayer layer)
        {
            for (i0 = 0; i0 < layer.C; i0++, i1++)
            {
                layer.E[i0].ID = i1;
                ref AetObject obj = ref layer.E[i0];
                IO.P = obj.Offset = layer.O + i0 * 0x30;
                obj.Name = IO.RPS();
                obj.LoopStart     = IO.RF32();
                obj.LoopEnd       = IO.RF32();
                obj.StartFrame    = IO.RF32();
                obj.PlaybackSpeed = IO.RF32();
                obj.Flags = (AetObject.AetObjFlags)IO.RU16();
                obj.Pad  = IO.RU8();
                obj.Type = (AetObject.AetObjType)IO.RU8();
                obj.        DataID = IO.RI32();
                obj.ParentObjectID = IO.RI32();
                obj.Marker = IO.ReadCountPointer<Marker>();
                obj.Data = IO.RP<AnimationData>();
                obj.ExtraData = IO.RP<AetObject.AetObjExtraData>();
                
                if (obj.Data.O > 0)
                {
                    ref AnimationData data = ref obj.Data.V;
                    IO.P = obj.Data.O;
                    data.Mode = (AnimationData.BlendMode)IO.RU8();
                    IO.RU8();
                    data.UseTextureMask = IO.RBo();
                    IO.RU8();
                    data.  OriginX = IO.ReadCountPointer<KFT2>();
                    data.  OriginY = IO.ReadCountPointer<KFT2>();
                    data.PositionX = IO.ReadCountPointer<KFT2>();
                    data.PositionY = IO.ReadCountPointer<KFT2>();
                    data.Rotation  = IO.ReadCountPointer<KFT2>();
                    data.   ScaleX = IO.ReadCountPointer<KFT2>();
                    data.   ScaleY = IO.ReadCountPointer<KFT2>();
                    data.Opacity   = IO.ReadCountPointer<KFT2>();
                    data._3D = IO.RP<AnimationData.ThirdDimension>();
                    
                    RKF(ref data.  OriginX);
                    RKF(ref data.  OriginY);
                    RKF(ref data.PositionX);
                    RKF(ref data.PositionY);
                    RKF(ref data.Rotation );
                    RKF(ref data.   ScaleX);
                    RKF(ref data.   ScaleY);
                    RKF(ref data.Opacity  );
                    
                    if (data._3D.O > 0)
                    {
                        ref AnimationData.ThirdDimension _3D = ref data._3D.V;
                        IO.P = data._3D.O;
                        _3D.Unk1       = IO.ReadCountPointer<KFT2>();
                        _3D.Unk2       = IO.ReadCountPointer<KFT2>();
                        _3D.RotReturnX = IO.ReadCountPointer<KFT2>();
                        _3D.RotReturnY = IO.ReadCountPointer<KFT2>();
                        _3D.RotReturnZ = IO.ReadCountPointer<KFT2>();
                        _3D. RotationX = IO.ReadCountPointer<KFT2>();
                        _3D. RotationY = IO.ReadCountPointer<KFT2>();
                        _3D.    ScaleZ = IO.ReadCountPointer<KFT2>();
                        
                        RKF(ref _3D.Unk1      );
                        RKF(ref _3D.Unk2      );
                        RKF(ref _3D.RotReturnX);
                        RKF(ref _3D.RotReturnY);
                        RKF(ref _3D.RotReturnZ);
                        RKF(ref _3D. RotationX);
                        RKF(ref _3D. RotationY);
                        RKF(ref _3D.    ScaleZ);
                    }
                }
                    
                if (obj.ExtraData.O > 0)
                {
                    ref AetObject.AetObjExtraData extraData = ref obj.ExtraData.V;
                    IO.P = obj.ExtraData.O;
                    extraData.Unk0 = IO.ReadCountPointer<KFT2>();
                    extraData.Unk1 = IO.ReadCountPointer<KFT2>();
                    extraData.Unk2 = IO.ReadCountPointer<KFT2>();
                    extraData.Unk3 = IO.ReadCountPointer<KFT2>();
                    
                    RKF(ref extraData.Unk0);
                    RKF(ref extraData.Unk1);
                    RKF(ref extraData.Unk2);
                    RKF(ref extraData.Unk3);
                }
                    
                IO.P = obj.Marker.O;
                for (i2 = 0; i2 < obj.Marker.C; i2++)
                    obj.Marker[i2] = new Marker() { Frame = IO.RF32(), Name = IO.RPSSJIS() };
            }
        }

        private void RKF(ref CountPointer<KFT2> keys)
        {
            if (keys.C  < 1) return;
            IO.P = keys.O;
            if (keys.C == 1) { keys.E[0].V = IO.RF32(); return; }

            keys.E = new KFT2[keys.C];
            for (uint i = 0; i < keys.C; i++) keys.E[i].F = IO.RF32();
            for (uint i = 0; i < keys.C; i++)
            { keys.E[i].V = IO.RF32();        keys.E[i].T = IO.RF32(); }
        }

        private void W(ref CountPointer<KFT2> keys)
        {
            keys.O = 0;
            if (keys.C < 1) return;
            if (keys.C == 1)
            {
                if (keys.E[0].V != 0) { keys.O = IO.P; IO.W(keys.E[0].V); }
                return;
            }

            if (keys.C > 2) IO.A(0x20);
            keys.O = IO.P;

            for (uint i = 0; i < keys.C; i++) IO.W(keys.E[i].F);
            for (uint i = 0; i < keys.C; i++)
            { IO.W(keys.E[i].V);              IO.W(keys.E[i].T); }
        }

        private void WPS(ref System.Collections.Generic.Dictionary<string, int> Dict, ref Pointer<string> Str)
        {
            if (Dict.ContainsKey(Str.V)) Str.O = Dict[Str.V];
            else { Str.O = IO.P; Dict.Add(Str.V, Str.O); IO.WPSSJIS(Str.V + "\0"); }
        }

        public void MsgPackReader(string file, bool JSON)
        {
            AET = new AetHeader();

            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);
            MsgPack Aet;
            if ((Aet = MsgPack["Aet", true]).NotNull)
                if (Aet.Array != null && Aet.Array.Length > 0)
                {
                    AET.Data = new Pointer<AetData>[Aet.Array.Length];
                    for (int i = 0; i < AET.Data.Length; i++)
                        MsgPackReader(Aet.Array[i], ref AET.Data[i]);
                }
            Aet.Dispose();
            MsgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            if (this.AET.Data == null || this.AET.Data.Length == 0) return;

            MsgPack AET = new MsgPack(this.AET.Data.Length, "Aet");
            for (int i = 0; i < this.AET.Data.Length; i++)
                AET[i] = MsgPackWriter(ref this.AET.Data[i]);
            AET.WriteAfterAll(true, file, JSON);
        }

        public void MsgPackReader(MsgPack AET, ref Pointer<AetData> AetData)
        {
            int i, i0;
            AetData = default;
            ref AetData aet = ref AetData.V;

            aet.Name.V        = AET.RS  ("Name"         );
            aet.StartFrame    = AET.RF32("StartFrame"   );
            aet.FrameDuration = AET.RF32("FrameDuration");
            aet.FrameRate     = AET.RF32("FrameRate"    );
            aet.BackColor     = AET.RU32("BackColor"    );
            aet.Width         = AET.RU32("Width"        );
            aet.Height        = AET.RU32("Height"       );

            MsgPack temp;
            if ((temp = AET["Position"]).NotNull)
            {
                aet.Position.O = 1;
                aet.Position.V.X = RKF(temp, "X");
                aet.Position.V.Y = RKF(temp, "Y");
            }
            
            if ((temp = AET["Sound", true]).NotNull)
            {
                aet.Sounds.C = temp.Array.Length;
                for (i = 0; i < aet.Sounds.C; i++)
                    aet.Sounds.E[i] = new AetSound { Unk = temp[i].RU32("Unk") };
            }
            
            if ((temp = AET["Region", true]).NotNull)
            {
                aet.Regions.C = temp.Array.Length;
                for (i = 0; i < aet.Regions.C; i++)
                {
                    ref AetRegion region = ref aet.Regions.E[i];
                    region = default;
                    region.Color  = temp[i].RU32("Color" );
                    region.Width  = temp[i].RU16("Width" );
                    region.Height = temp[i].RU16("Height");
                    region.Frames = temp[i].RF32("Frames");

                    MsgPack sprite;
                    if ((sprite = temp[i]["Sprite", true]).NotNull)
                    {
                        region.Sprites.C = sprite.Array.Length;
                        for (i0 = 0; i0 < region.Sprites.C; i0++)
                            region.Sprites[i0] = new Sprite() { Name = new Pointer<string>()
                                { V = sprite[i0].RS("Name") }, ID = sprite[i0].RU32("ID") };
                    }
                    sprite.Dispose();
                }
            }
            
            if ((temp = AET["Layer", true]).NotNull)
            {
                aet.Layers.C = temp.Array.Length + 1;
                for (i = 0; i < aet.Layers.C - 1; i++)
                {
                    ref AetLayer layer = ref aet.Layers.E[i];
                    layer = default;
                    MsgPack @object;
                    if ((@object = temp[i]["Object", true]).NotNull)
                    {
                        layer.C = @object.Array.Length;
                        for (i0 = 0; i0 < layer.C; i0++)
                            RAO(ref layer.E[i0], @object[i0]);
                    }
                    @object.Dispose();
                }
            }
            else aet.Layers.C = 1;

            if ((temp = AET["RootLayer", true]).NotNull)
            {
                i = aet.Layers.C - 1;
                aet.Layers.E[i].C = temp.Array.Length;
                for (i0 = 0; i0 < aet.Layers[i].C; i0++)
                    RAO(ref aet.Layers.E[i].E[i0], temp[i0]);
            }
            temp.Dispose();
        }
        
        public MsgPack MsgPackWriter(ref Pointer<AetData> AetData)
        {
            int i, i0;
            if (AetData.O <= 0) return MsgPack.Null;
            ref AetData aet = ref AetData.V;

            MsgPack AET = MsgPack.New.Add("Name", aet.Name.V).Add("StartFrame", aet.StartFrame)
                .Add("FrameDuration", aet.FrameDuration).Add("FrameRate", aet.FrameRate)
                .Add("BackColor", aet.BackColor).Add("Width", aet.Width).Add("Height", aet.Height);


            if (aet.Position.O > 0)
                AET.Add(new MsgPack("Position").Add(WMP(ref aet.Position.V.X, "X"))
                                               .Add(WMP(ref aet.Position.V.Y, "Y")));


            i = aet.Layers.C - 1;
            MsgPack rootLayer = new MsgPack(aet.Layers[i].C, "RootLayer");
            for (i0 = 0; i0 < aet.Layers[i].C; i0++)
                rootLayer[i0] = WMP(ref aet.Layers[i].E[i0]);
            AET.Add(rootLayer);

            if (aet.Layers[i].C > 1)
            {
                MsgPack layers = new MsgPack(aet.Layers.C - 1, "Layer");
                for (i = 0; i < aet.Layers.C - 1; i++)
                {
                    ref AetLayer layer = ref aet.Layers.E[i];
                    MsgPack Object = MsgPack.New.Add("ID", i);
                    if (layer.C > 0) 
                    {
                        MsgPack objects = new MsgPack(layer.C, "Object");
                        for (i0 = 0; i0 < layer.C; i0++)
                            objects[i0] = WMP(ref layer.E[i0]);
                        Object.Add(objects);
                    }
                    layers[i] = Object;
                }
                AET.Add(layers);
            }

            MsgPack regions = new MsgPack(aet.Regions.C, "Region");
            for (i = 0; i < aet.Regions.C; i++)
            {
                ref AetRegion region = ref aet.Regions.E[i];
                MsgPack entry = MsgPack.New.Add("ID", i).Add("Width", region.Width)
                    .Add("Height", region.Height).Add("Color", region.Color);

                if (region.Sprites.C > 0)
                {
                    entry.Add("Frames", region.Frames);
                    MsgPack Sprites = new MsgPack(region.Sprites.C, "Sprite");
                    for (i0 = 0; i0 < region.Sprites.C; i0++)
                        Sprites[i0] = MsgPack.New.Add("Name",
                            region.Sprites[i0].Name.V).Add("ID", region.Sprites[i0].ID);
                    entry.Add(Sprites);
                }
                regions[i] = entry;
            }
            AET.Add(regions);

            MsgPack sounds = new MsgPack(aet.Sounds.C, "Sound");
            for (i = 0; i < aet.Sounds.C; i++)
                sounds[i] = MsgPack.New.Add("ID", i).Add("Unk", aet.Sounds[i].Unk);
            AET.Add(sounds);

            return AET;
        }

        private AetObject RAO(ref AetObject Obj, MsgPack msg)
        {
            uint i;
            Obj = default;
            Obj.ID            = msg.RI32("ID"          );
            Obj.Name.V        = msg.RS  ("Name"        );
            Obj.LoopStart     = msg.RF32("LoopStart"   );
            Obj.LoopEnd       = msg.RF32("LoopEnd"     );
            Obj.StartFrame    = msg.RF32("StartFrame"  );
            Obj.PlaybackSpeed = msg.RF32("PlaybackSpeed");
            Obj.Flags = (AetObject.AetObjFlags)msg.RU16("Flags");
            Obj.Pad = msg.RU8("Pad");
            System.Enum.TryParse(msg.RS("Type"), out Obj.Type);
            Obj.        DataID = msg.RnI32(        "DataID") ?? -1;
            Obj.ParentObjectID = msg.RnI32("ParentObjectID") ?? -1;
                
            MsgPack temp;
            if ((temp = msg["Markers", true]).NotNull)
            {
                Obj.Marker.C = temp.Array.Length;
                for (i = 0; i < Obj.Marker.C; i++)
                {
                    Obj.Marker.E[i].Frame   = temp[i].RF32("Frame");
                    Obj.Marker.E[i]. Name.V = temp[i].RS  ( "Name");
                }
            }

            if ((temp = msg["AnimationData"]).NotNull)
            {
                Obj.Data.O = 1;
                ref AnimationData data = ref Obj.Data.V;
                System.Enum.TryParse(temp.RS("BlendMode"), out data.Mode);
                data.UseTextureMask = temp.RB("UseTextureMask");
                
                data.  OriginX = RKF(temp,   "OriginX");
                data.  OriginY = RKF(temp,   "OriginY");
                data.PositionX = RKF(temp, "PositionX");
                data.PositionY = RKF(temp, "PositionY");
                data.Rotation  = RKF(temp, "Rotation" );
                data.   ScaleX = RKF(temp,    "ScaleX");
                data.   ScaleY = RKF(temp,    "ScaleY");
                data.Opacity   = RKF(temp, "Opacity"  );

                MsgPack _3D;
                if ((_3D = temp["3D"]).NotNull)
                {
                    data._3D.O = 1;
                    ref AnimationData.ThirdDimension data3D = ref data._3D.V;
                    data3D.Unk1       = RKF(_3D, "Unk1"      );
                    data3D.Unk2       = RKF(_3D, "Unk2"      );
                    data3D.RotReturnX = RKF(_3D, "RotReturnX");
                    data3D.RotReturnY = RKF(_3D, "RotReturnY");
                    data3D.RotReturnZ = RKF(_3D, "RotReturnZ");
                    data3D. RotationX = RKF(_3D,  "RotationX");
                    data3D. RotationY = RKF(_3D,  "RotationY");
                    data3D.    ScaleZ = RKF(_3D,     "ScaleZ");
                }
                _3D.Dispose();
            }

            if ((temp = msg["ExtraData"]).NotNull)
            {
                Obj.ExtraData.O = 1;
                ref AetObject.AetObjExtraData data = ref Obj.ExtraData.V;
                data.Unk0 = RKF(temp, "Unk0");
                data.Unk1 = RKF(temp, "Unk1");
                data.Unk2 = RKF(temp, "Unk2");
                data.Unk3 = RKF(temp, "Unk3");
            }
            temp.Dispose();
            return Obj;
        }

        private CountPointer<KFT2> RKF(MsgPack msg, string name)
        {
            CountPointer<KFT2> kf = default;
            MsgPack temp;
            float? Value = msg.RnF32(name);
            if (Value != null) { kf.C = 1; kf.E[0].V = Value.Value; }
            if ((temp = msg[name, true]).NotNull && temp.Array.Length > 1)
            {
                kf.E = new KFT2[temp.Array.Length];
                for (int i = 0; i < kf.E.Length; i++)
                {
                         if (temp[i].Array == null ||
                             temp[i].Array.Length == 0) continue;
                    else if (temp[i].Array.Length == 1)
                        kf.E[i] = new KFT2(temp[i][0].RF32());
                    else if (temp[i].Array.Length == 2)
                        kf.E[i] = new KFT2(temp[i][0].RF32(), temp[i][1].RF32());
                    else if (temp[i].Array.Length == 3)
                        kf.E[i] = new KFT2(temp[i][0].RF32(), temp[i][1].RF32(), temp[i][2].RF32());
                }
            }
            temp.Dispose();
            return kf;
        }

        private MsgPack WMP(ref AetObject obj, string name = null)
        {
            int i;
            MsgPack @object = new MsgPack(name).Add("ID", obj.ID).Add("Name", obj.Name.V)
                .Add("LoopStart", obj.LoopStart).Add("LoopEnd", obj.LoopEnd)
                .Add("StartFrame", obj.StartFrame).Add("PlaybackSpeed", obj.PlaybackSpeed)
                .Add("Flags", (ushort)obj.Flags).Add("Pad", obj.Pad).Add("Type", obj.Type.ToString());

            if (obj.        DataID > -1) @object.Add(        "DataID", obj.        DataID);
            if (obj.ParentObjectID > -1) @object.Add("ParentObjectID", obj.ParentObjectID);

            if (obj.Marker.C > 0)
            {
                MsgPack markers = new MsgPack(obj.Marker.C, "Markers");
                for (i = 0; i < obj.Marker.C; i++)
                    markers[i] = MsgPack.New.Add("Frame", obj.Marker[i].Frame  )
                                            .Add( "Name", obj.Marker[i]. Name.V);
                @object.Add(markers);
            }

            if (obj.Data.O > 0)
            {
                ref AnimationData data = ref obj.Data.V;
                MsgPack animationData = new MsgPack("AnimationData").Add("BlendMode",
                    data.Mode.ToString()).Add("UseTextureMask", data.UseTextureMask);

                animationData.Add(WMP(ref data.OriginX  ,   "OriginX"));
                animationData.Add(WMP(ref data.OriginY  ,   "OriginY"));
                animationData.Add(WMP(ref data.PositionX, "PositionX"));
                animationData.Add(WMP(ref data.PositionY, "PositionY"));
                animationData.Add(WMP(ref data.Rotation , "Rotation" ));
                animationData.Add(WMP(ref data.   ScaleX,    "ScaleX"));
                animationData.Add(WMP(ref data.   ScaleY,    "ScaleY"));
                animationData.Add(WMP(ref data.Opacity  , "Opacity"  ));
                if (data._3D.O > 0)
                {
                    ref AnimationData.ThirdDimension data3D = ref data._3D.V;
                    MsgPack _3D = new MsgPack("3D");
                    _3D.Add(WMP(ref data3D.Unk1      , "Unk1"      ));
                    _3D.Add(WMP(ref data3D.Unk2      , "Unk2"      ));
                    _3D.Add(WMP(ref data3D.RotReturnX, "RotReturnX"));
                    _3D.Add(WMP(ref data3D.RotReturnY, "RotReturnY"));
                    _3D.Add(WMP(ref data3D.RotReturnZ, "RotReturnZ"));
                    _3D.Add(WMP(ref data3D. RotationX,  "RotationX"));
                    _3D.Add(WMP(ref data3D. RotationY,  "RotationY"));
                    _3D.Add(WMP(ref data3D.    ScaleZ,     "ScaleZ"));
                    animationData.Add(_3D);
                }
                @object.Add(animationData);
            }
            if (obj.ExtraData.O > 0)
            {
                ref AetObject.AetObjExtraData data = ref obj.ExtraData.V;
                MsgPack extraData = new MsgPack("ExtraData");
                extraData.Add(WMP(ref data.Unk0, "Unk0"));
                extraData.Add(WMP(ref data.Unk1, "Unk1"));
                extraData.Add(WMP(ref data.Unk2, "Unk2"));
                extraData.Add(WMP(ref data.Unk3, "Unk3"));
                @object.Add(extraData);
            }
            return @object;
        }

        private MsgPack WMP(ref CountPointer<KFT2> kf, string name)
        {
            if (kf.C <  1) return     MsgPack.Null;
            if (kf.C == 1) return new MsgPack(name, kf.E[0].V);
            MsgPack KFE = new MsgPack(kf.C, name);
            for (int i = 0; i < kf.E.Length; i++)
            {
                IKF KF = kf.E[i].Check();
                     if (KF is KFT0 KFT0) KFE[i] = new MsgPack(null, new MsgPack[] { KFT0.F });
                else if (KF is KFT1 KFT1) KFE[i] = new MsgPack(null, new MsgPack[] { KFT1.F, KFT1.V });
                else if (KF is KFT2 KFT2) KFE[i] = new MsgPack(null, new MsgPack[] { KFT2.F, KFT2.V, KFT2.T });
            }
            return KFE;
        }
    }
}
