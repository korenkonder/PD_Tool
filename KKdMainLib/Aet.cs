//Original: AetSet.bt Version: 4.3 by samyuu

using KKdBaseLib;
using KKdBaseLib.Auth2D;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct Aet : System.IDisposable
    {
        private Stream _IO;
        private int i, i0, i1, i2;
        private const string x00 = "\0";

        public Header AET;

        public void AETReader(string file)
        {
            AET = default;
            _IO = File.OpenReader(file + ".bin", true);

            int i = 0;
            int Pos = -1;
            while (true) { Pos = _IO.RI32(); if (Pos != 0 && Pos < _IO.L) i++; else break; }

            _IO.P = 0;
            AET.Data = new Pointer<Data>[i];
            for (i = 0; i < AET.Data.Length; i++) AET.Data[i] = _IO.RP<Data>();
            for (i = 0; i < AET.Data.Length; i++) AETReader(ref AET.Data[i]);

            _IO.C();
        }

        public void AETWriter(string file)
        {
            if (AET.Data == null || AET.Data.Length == 0) return;
            _IO = File.OpenWriter();

            for (int i = 0; i < AET.Data.Length; i++)
            {
                _IO.P = _IO.L + 0x20;
                AETWriter(ref AET.Data[i]);
            }

            _IO.P = 0;
            for (int i = 0; i < AET.Data.Length; i++)
                if (AET.Data[i].O > 0) _IO.W(AET.Data[i].O);
            byte[] data = _IO.ToArray();
            _IO.Dispose();

            using (_IO = File.OpenWriter(file + ".bin", true))
                _IO.W(data);
            data = null;
        }

        private void AETReader(ref Pointer<Data> aetData)
        {
            _IO.P = aetData.O;
            ref Data aet = ref aetData.V;

            aet.Name = _IO.RPS();
            aet.StartFrame = _IO.RF32();
            aet.EndFrame   = _IO.RF32();
            aet.FrameRate  = _IO.RF32();
            aet.BackColor  = _IO.RU32();
            aet.Width      = _IO.RU32();
            aet.Height     = _IO.RU32();
            aet.Camera       = _IO.RP<Vector2<CountPointer<KFT2>>>();
            aet.Compositions = _IO.ReadCountPointer<Composition>();
            aet.Surfaces     = _IO.ReadCountPointer<Surface    >();
            aet.SoundEffects = _IO.ReadCountPointer<AetSoundEffect>();

            if (aet.Camera.O > 0)
            {
                _IO.P = aet.Camera.O;
                aet.Camera.V.X.O = _IO.RI32();
                aet.Camera.V.Y.O = _IO.RI32();
                RKF(ref aet.Camera.V.X);
                RKF(ref aet.Camera.V.Y);
            }

            if (aet.SoundEffects.O > 0)
            {
                _IO.P = aet.SoundEffects.O;
                for (i = 0; i < aet.SoundEffects.C; i++)
                {
                    ref AetSoundEffect aif = ref aet.SoundEffects.E[i];
                    aif.O = _IO.P;
                    aif.Unk = _IO.RU32();
                }
            }

            _IO.P = aet.Compositions.O;
            for (i = 0; i < aet.Compositions.C; i++)
                aet.Compositions[i] = new Composition() { P = _IO.P, C = _IO.RI32(), O = _IO.RI32()};

            i1 = 0;
            RAL(ref aet.Compositions.E[aet.Compositions.C - 1]);
            for (i = 0; i < aet.Compositions.C - 1; i++)
                RAL(ref aet.Compositions.E[i]);

            _IO.P = aet.Surfaces.O;
            for (i = 0; i < aet.Surfaces.C; i++)
                aet.Surfaces[i] = new Surface { O = _IO.P, Color = _IO.RU32(), Width = _IO.RU16(),
                    Height = _IO.RU16(), Frames = _IO.RF32(), Sprites = _IO.ReadCountPointer<SpriteIdentifier>() };

            for (i = 0; i < aet.Surfaces.C; i++)
            {
                _IO.P = aet.Surfaces[i].Sprites.O;
                for (i0 = 0; i0 < aet.Surfaces[i].Sprites.C; i0++)
                    aet.Surfaces.E[i].Sprites[i0] = new SpriteIdentifier { Name = _IO.RPS(), ID = _IO.RU32() };
            }

            for (i = 0; i < aet.Compositions.C; i++)
            {
                for (i0 = 0; i0 < aet.Compositions[i].C; i0++)
                {
                    ref Layer obj = ref aet.Compositions[i].E[i0];
                    obj.DataID = -1;
                    int dataOffset = obj.DataOffset;
                    if (dataOffset > 0)
                        if (obj.Type == Layer.AetLayerType.Pic)
                            for (i1 = 0; i1 < aet.Surfaces.C; i1++)
                            { if (aet.    Surfaces[i1].O == dataOffset) { obj.DataID = i1; break; } }
                        else if (obj.Type == Layer.AetLayerType.Aif)
                            for (i1 = 0; i1 < aet.SoundEffects.C; i1++)
                            { if (aet.SoundEffects[i1].O == dataOffset) { obj.DataID = i1; break; } }
                        else if (obj.Type == Layer.AetLayerType.Eff)
                            for (i1 = 0; i1 < aet.Compositions.C; i1++)
                            { if (aet.Compositions[i1].P == dataOffset) { obj.DataID = i1; break; } }

                    ref int parentLayer = ref obj.ParentLayer;
                    if (parentLayer > 0)
                        for (i1 = 0; i1 < aet.Compositions.C; i1++)
                        {
                            for (i2 = 0; i2 < aet.Compositions[i1].C; i2++)
                                if (aet.Compositions[i1].E[i2].Offset == parentLayer)
                                { obj.ParentLayer = aet.Compositions[i1].E[i2].ID; break; }
                            if (parentLayer != obj.ParentLayer) break;
                        }
                    if (parentLayer == obj.ParentLayer) obj.ParentLayer = -1;
                }
            }
        }

        private void AETWriter(ref Pointer<Data> aetData)
        {
            ref Data aet = ref aetData.V;

            for (i = 0; i < aet.Surfaces.C; i++)
            {
                ref Surface region = ref aet.Surfaces.E[i];
                if (region.Sprites.C == 0) { region.Sprites.O = 0; continue; }
                if (region.Sprites.C > 1) _IO.A(0x20);
                region.Sprites.O = _IO.P;
                for (i0 = 0; i0 < region.Sprites.C; i0++) _IO.W(0L);
            }

            _IO.A(0x20);
            aet.Surfaces.O = _IO.P;
            for (i = 0; i < aet.Surfaces.C; i++)
            {
                ref Surface region = ref aet.Surfaces.E[i];
                region.O = _IO.P;
                _IO.W(region.Color);
                _IO.W(region.Width);
                _IO.W(region.Height);
                _IO.W(region.Frames);
                _IO.W(region.Sprites);
            }

            _IO.A(0x20);
            aet.Compositions.O = _IO.P;
            for (i = 0; i < aet.Compositions.C; i++)
            {
                aet.Compositions.E[i].O = _IO.P;
                _IO.W(0L);
            }

            for (i = 0; i < aet.Compositions.C; i++)
            {
                if (aet.Compositions.E[i].C < 1) continue;

                for (i0 = 0; i0 < aet.Compositions.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Compositions.E[i].E[i0];
                    ref AnimationData data = ref obj.Data.V;
                    ref AnimationData.Perspective persp = ref data.Persp.V;
                    ref AudioData extraData = ref obj.ExtraData.V;
                    if (obj.Marker.C > 0)
                    {
                        if (obj.Marker.C > 3) _IO.A(0x20);
                        obj.Marker.O = _IO.P;
                        for (i1 = 0; i1 < obj.Marker.C; i1++)
                            _IO.W(0L);
                    }

                    if (obj.Data.V.Persp.O > 0)
                    {
                        W(ref persp.Unk1      );
                        W(ref persp.Unk2      );
                        W(ref persp.RotReturnX);
                        W(ref persp.RotReturnY);
                        W(ref persp.RotReturnZ);
                        W(ref persp. RotationX);
                        W(ref persp. RotationY);
                        W(ref persp.    ScaleZ);
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

                    if (obj.Data.V.Persp.O > 0)
                    {
                        _IO.A(0x20);
                        data.Persp.O = _IO.P;
                        _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
                        _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
                    }

                    if (obj.ExtraData.O > 0)
                    {
                        W(ref extraData.Data0);
                        W(ref extraData.Data1);
                        W(ref extraData.Data2);
                        W(ref extraData.Data3);
                    }

                    if (obj.Data.O > 0)
                    {
                        _IO.A(0x20);
                        obj.Data.O = _IO.P;
                        _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
                        _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
                    }

                    if (obj.ExtraData.O > 0)
                    {
                        _IO.A(0x20);
                        obj.ExtraData.O = _IO.P;
                        _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
                    }
                }

                _IO.A(0x20);
                aet.Compositions.E[i].E[0].Offset = _IO.P;
                for (i0 = 0; i0 < aet.Compositions.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Compositions.E[i].E[i0];
                    obj.Offset = _IO.P;
                    _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
                }
            }

            if (aet.Camera.O > 0)
            {
                ref Vector2<CountPointer<KFT2>> pos = ref aet.Camera.V;
                _IO.A(0x10);
                W(ref pos.X);
                W(ref pos.Y);

                _IO.A(0x20);
                aet.Camera.O = _IO.P;
                _IO.W(pos.X);
                _IO.W(pos.Y);

                W(ref pos.X);
                W(ref pos.Y);
            }

            _IO.A(0x20);
            aetData.O = _IO.P;
            _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
            _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);

            _IO.A(0x10);
            {
                System.Collections.Generic.Dictionary<string, int> usedValues =
                    new System.Collections.Generic.Dictionary<string, int>();

                for (i = 0; i < aet.Surfaces.C; i++)
                {
                    ref Surface region = ref aet.Surfaces.E[i];
                    for (i0 = 0; i0 < region.Sprites.C; i0++)
                        WPS(ref usedValues, ref region.Sprites.E[i0].Name);
                }

                //_IO.Align(0x4);
                for (i = 0; i < aet.Compositions.C; i++)
                {
                    for (i0 = 0; i0 < aet.Compositions.E[i].C; i0++)
                    {
                        ref Layer obj = ref aet.Compositions.E[i].E[i0];
                        for (i1 = 0; i1 < obj.Marker.C; i1++)
                            WPS(ref usedValues, ref obj.Marker.E[i1].Name);
                    }

                    for (i0 = 0; i0 < aet.Compositions.E[i].C; i0++)
                        WPS(ref usedValues, ref aet.Compositions.E[i].E[i0].Name);
                }

                //_IO.Align(0x4);
                aet.Name.O = _IO.P;
                _IO.W(aet.Name.V + x00);
                _IO.SL(_IO.P);
            }

            aet.SoundEffects.O = 0;
            if (aet.SoundEffects.C > 0)
            {
                _IO.A(0x10);
                aet.SoundEffects.O = _IO.P;
                for (i = 0; i < aet.SoundEffects.C; i++)
                {
                    aet.SoundEffects.E[i].O = _IO.P;
                    _IO.W(aet.SoundEffects[i].Unk);
                }
            }

            for (i = 0; i < aet.Compositions.C; i++)
                for (i0 = 0; i0 < aet.Compositions.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Compositions.E[i].E[i0];
                         if (obj.Type == Layer.AetLayerType.Pic) obj.DataOffset = aet.Surfaces    [obj.DataID].O;
                    else if (obj.Type == Layer.AetLayerType.Aif) obj.DataOffset = aet.SoundEffects[obj.DataID].O;
                    else if (obj.Type == Layer.AetLayerType.Eff) obj.DataOffset = aet.Compositions[obj.DataID].O;
                    else obj.DataOffset = 0;
                    if (obj.DataOffset == 0)
                    {

                    }
                }

            _IO.A(0x4);
            int returnPos = _IO.P;
            int nvp = 0;

            _IO.F();

            for (i = 0; i < aet.Compositions.C; i++)
            {
                if (aet.Compositions.E[i].C < 1) continue;

                for (i0 = 0; i0 < aet.Compositions.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Compositions.E[i].E[i0];
                    ref AnimationData data = ref obj.Data.V;
                    ref AnimationData.Perspective persp = ref data.Persp.V;
                    ref AudioData extraData = ref obj.ExtraData.V;

                    if (obj.Data.V.Persp.O > 0)
                    {
                        _IO.P = data.Persp.O;
                        W(ref _IO, ref persp.Unk1      );
                        W(ref _IO, ref persp.Unk2      );
                        W(ref _IO, ref persp.RotReturnX);
                        W(ref _IO, ref persp.RotReturnY);
                        W(ref _IO, ref persp.RotReturnZ);
                        W(ref _IO, ref persp. RotationX);
                        W(ref _IO, ref persp. RotationY);
                        W(ref _IO, ref persp.    ScaleZ);
                    }

                    if (obj.Data.O > 0)
                    {
                        _IO.P = obj.Data.O;
                        _IO.W((byte) data.Mode);
                        _IO.W((byte)0);
                        _IO.W((byte)(data.UseTextureMask ? 1 : 0));
                        _IO.W((byte)0);

                        W(ref _IO, ref data.  OriginX);
                        W(ref _IO, ref data.  OriginY);
                        W(ref _IO, ref data.PositionX);
                        W(ref _IO, ref data.PositionY);
                        W(ref _IO, ref data.Rotation );
                        W(ref _IO, ref data.   ScaleX);
                        W(ref _IO, ref data.   ScaleY);
                        W(ref _IO, ref data.Opacity  );
                        _IO.W(data.Persp.O);
                    }

                    if (obj.ExtraData.O > 0)
                    {
                        _IO.P = obj.ExtraData.O;
                        W(ref _IO, ref extraData.Data0);
                        W(ref _IO, ref extraData.Data1);
                        W(ref _IO, ref extraData.Data2);
                        W(ref _IO, ref extraData.Data3);
                    }

                    void W(ref Stream _IO, ref CountPointer<KFT2> keys)
                    {
                        if (keys.C < 1) _IO.W(0L);
                        else { if (keys.C > 0 && keys.O == 0) { keys.O = returnPos + (nvp << 2); nvp++; } _IO.W(keys); }

                    }
                }
            }

            _IO.P = returnPos;
            for (i = 0; i < nvp; i++)
                _IO.W(0);
            _IO.A(0x10, true);

            _IO.F();

            for (i = 0; i < aet.Compositions.C; i++)
                for (i0 = 0; i0 < aet.Compositions.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Compositions.E[i].E[i0];

                    _IO.P = obj.Offset;
                    _IO.W(obj.Name.O);
                    _IO.W(obj.StartFrame);
                    _IO.W(obj.EndFrame);
                    _IO.W(obj.StartOffset);
                    _IO.W(obj.PlaybackSpeed);
                    _IO.W((ushort)obj.Flags);
                    _IO.W(obj.Pad);
                    _IO.W((byte)obj.Type);
                    _IO.W(obj.DataOffset);

                    if (obj.ParentLayer > -1)
                    {
                        bool Found = false;
                        for (i1 = 0; i1 < aet.Compositions.C; i1++)
                            for (i2 = 0; i2 < aet.Compositions.E[i1].C; i2++)
                                if (aet.Compositions[i1].E[i2].ID == obj.ParentLayer)
                                { Found = true; _IO.W(aet.Compositions.E[i1].E[i2].Offset); break; }
                        if (!Found) _IO.W(0);
                    }
                    else _IO.W(0);
                    _IO.W(obj.Marker);
                    _IO.W(obj.Data.O);
                    _IO.W(obj.ExtraData.O);

                    ref CountPointer<Marker> Marker = ref obj.Marker;
                    _IO.P = Marker.O;
                    for (i1 = 0; i1 < Marker.C; i1++)
                    {
                        _IO.W(Marker[i1].Frame);
                        _IO.W(Marker[i1].Name.O);
                    }
                }

            _IO.F();

            for (i = 0; i < aet.Surfaces.C; i++)
            {
                ref Surface region = ref aet.Surfaces.E[i];
                if (region.Sprites.C == 0) continue;
                _IO.P = region.Sprites.O;
                for (i0 = 0; i0 < region.Sprites.C; i0++)
                {
                    _IO.W(region.Sprites[i0].Name.O);
                    _IO.W(region.Sprites[i0].ID);
                }
            }

            _IO.P = aet.Compositions.O;
            for (i = 0; i < aet.Compositions.C; i++)
            {
                if (aet.Compositions[i].C > 0)
                {
                    _IO.W(aet.Compositions[i].C);
                    _IO.W(aet.Compositions[i].E[0].Offset);
                }
                else _IO.W(0L);
            }

            _IO.P = aetData.O;
            _IO.W(aet.Name.O);
            _IO.W(aet.StartFrame);
            _IO.W(aet.EndFrame);
            _IO.W(aet.FrameRate);
            _IO.W(aet.BackColor);
            _IO.W(aet.Width);
            _IO.W(aet.Height);
            _IO.W(aet.Camera.O);
            _IO.W(aet. Compositions);
            _IO.W(aet.Surfaces);
            _IO.W(aet. SoundEffects);
            _IO.W(0L);
        }

        private void RAL(ref Composition layer)
        {
            for (i0 = 0; i0 < layer.C; i0++, i1++)
            {
                layer.E[i0].ID = i1;
                ref Layer obj = ref layer.E[i0];
                _IO.P = obj.Offset = layer.O + i0 * 0x30;
                obj.Name = _IO.RPS();
                obj.StartFrame    = _IO.RF32();
                obj.  EndFrame    = _IO.RF32();
                obj.StartOffset   = _IO.RF32();
                obj.PlaybackSpeed = _IO.RF32();
                obj.Flags = (Layer.AetLayerFlags)_IO.RU16();
                obj.Pad   = _IO.RU8();
                obj.Type  = (Layer.AetLayerType)_IO.RU8();
                obj.DataOffset  = _IO.RI32();
                obj.ParentLayer = _IO.RI32();
                obj.Marker = _IO.ReadCountPointer<Marker>();
                obj.Data = _IO.RP<AnimationData>();
                obj.ExtraData = _IO.RP<AudioData>();

                if (obj.Data.O > 0)
                {
                    ref AnimationData data = ref obj.Data.V;
                    _IO.P = obj.Data.O;
                    data.Mode = (AnimationData.BlendMode)_IO.RU8();
                    _IO.RU8();
                    data.UseTextureMask = _IO.RBo();
                    _IO.RU8();
                    data.  OriginX = _IO.ReadCountPointer<KFT2>();
                    data.  OriginY = _IO.ReadCountPointer<KFT2>();
                    data.PositionX = _IO.ReadCountPointer<KFT2>();
                    data.PositionY = _IO.ReadCountPointer<KFT2>();
                    data.Rotation  = _IO.ReadCountPointer<KFT2>();
                    data.   ScaleX = _IO.ReadCountPointer<KFT2>();
                    data.   ScaleY = _IO.ReadCountPointer<KFT2>();
                    data.Opacity   = _IO.ReadCountPointer<KFT2>();
                    data.Persp = _IO.RP<AnimationData.Perspective>();

                    RKF(ref data.  OriginX);
                    RKF(ref data.  OriginY);
                    RKF(ref data.PositionX);
                    RKF(ref data.PositionY);
                    RKF(ref data.Rotation );
                    RKF(ref data.   ScaleX);
                    RKF(ref data.   ScaleY);
                    RKF(ref data.Opacity  );

                    if (data.Persp.O > 0)
                    {
                        ref AnimationData.Perspective Persp = ref data.Persp.V;
                        _IO.P = data.Persp.O;
                        Persp.Unk1       = _IO.ReadCountPointer<KFT2>();
                        Persp.Unk2       = _IO.ReadCountPointer<KFT2>();
                        Persp.RotReturnX = _IO.ReadCountPointer<KFT2>();
                        Persp.RotReturnY = _IO.ReadCountPointer<KFT2>();
                        Persp.RotReturnZ = _IO.ReadCountPointer<KFT2>();
                        Persp. RotationX = _IO.ReadCountPointer<KFT2>();
                        Persp. RotationY = _IO.ReadCountPointer<KFT2>();
                        Persp.    ScaleZ = _IO.ReadCountPointer<KFT2>();

                        RKF(ref Persp.Unk1      );
                        RKF(ref Persp.Unk2      );
                        RKF(ref Persp.RotReturnX);
                        RKF(ref Persp.RotReturnY);
                        RKF(ref Persp.RotReturnZ);
                        RKF(ref Persp. RotationX);
                        RKF(ref Persp. RotationY);
                        RKF(ref Persp.    ScaleZ);
                    }
                }

                if (obj.ExtraData.O > 0)
                {
                    ref AudioData extraData = ref obj.ExtraData.V;
                    _IO.P = obj.ExtraData.O;
                    extraData.Data0 = _IO.ReadCountPointer<KFT2>();
                    extraData.Data1 = _IO.ReadCountPointer<KFT2>();
                    extraData.Data2 = _IO.ReadCountPointer<KFT2>();
                    extraData.Data3 = _IO.ReadCountPointer<KFT2>();

                    RKF(ref extraData.Data0);
                    RKF(ref extraData.Data1);
                    RKF(ref extraData.Data2);
                    RKF(ref extraData.Data3);
                }

                _IO.P = obj.Marker.O;
                for (i2 = 0; i2 < obj.Marker.C; i2++)
                    obj.Marker[i2] = new Marker() { Frame = _IO.RF32(), Name = _IO.RPSSJIS() };
            }
        }

        private void RKF(ref CountPointer<KFT2> keys)
        {
            if (keys.C  < 1) return;
            _IO.P = keys.O;
            if (keys.C == 1) { keys.E[0].V = _IO.RF32(); return; }

            keys.E = new KFT2[keys.C];
            for (uint i = 0; i < keys.C; i++) keys.E[i].F = _IO.RF32();
            for (uint i = 0; i < keys.C; i++)
            { keys.E[i].V = _IO.RF32();       keys.E[i].T = _IO.RF32(); }
        }

        private void W(ref CountPointer<KFT2> keys)
        {
            keys.O = 0;
            if (keys.C < 1) return;
            if (keys.C == 1)
            {
                if (keys.E[0].V != 0) { keys.O = _IO.P; _IO.W(keys.E[0].V); }
                return;
            }

            if (keys.C > 2) _IO.A(0x20);
            keys.O = _IO.P;

            for (uint i = 0; i < keys.C; i++) _IO.W(keys.E[i].F);
            for (uint i = 0; i < keys.C; i++)
            { _IO.W(keys.E[i].V);              _IO.W(keys.E[i].T); }
        }

        private void WPS(ref System.Collections.Generic.Dictionary<string, int> dict, ref Pointer<string> str)
        {
            if (dict.ContainsKey(str.V)) str.O = dict[str.V];
            else { str.O = _IO.P; dict.Add(str.V, str.O); _IO.WPSSJIS(str.V + "\0"); }
        }

        public void MsgPackReader(string file, bool json)
        {
            AET = default;

            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            MsgPack aet;
            if ((aet = msgPack["Aet", true]).NotNull)
                if (aet.Array != null && aet.Array.Length > 0)
                {
                    AET.Data = new Pointer<Data>[aet.Array.Length];
                    for (int i = 0; i < AET.Data.Length; i++)
                        MsgPackReader(aet.Array[i], ref AET.Data[i]);
                }
            aet.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (AET.Data == null || AET.Data.Length == 0) return;

            MsgPack aet = new MsgPack(AET.Data.Length, "Aet");
            for (int i = 0; i < AET.Data.Length; i++)
                aet[i] = MsgPackWriter(ref AET.Data[i]);
            aet.WriteAfterAll(true, file, json);
        }

        public void MsgPackReader(MsgPack msgPack, ref Pointer<Data> aetData)
        {
            int i, i0;
            aetData = default;
            ref Data aet = ref aetData.V;

            aet.Name.V     = msgPack.RS  ("Name"      );
            aet.StartFrame = msgPack.RF32("StartFrame");
            aet.  EndFrame = msgPack.RF32(  "EndFrame");
            aet.FrameRate  = msgPack.RF32("FrameRate" );
            aet.BackColor  = msgPack.RU32("BackColor" );
            aet.Width      = msgPack.RU32("Width"     );
            aet.Height     = msgPack.RU32("Height"    );

            MsgPack temp;
            if ((temp = msgPack["Camera"]).NotNull)
            {
                aet.Camera.O = 1;
                aet.Camera.V.X = RKF(temp, "X");
                aet.Camera.V.Y = RKF(temp, "Y");
            }

            if ((temp = msgPack["SoundEffect", true]).NotNull)
            {
                aet.SoundEffects.C = temp.Array.Length;
                for (i = 0; i < aet.SoundEffects.C; i++)
                    aet.SoundEffects.E[i] = new AetSoundEffect { Unk = temp[i].RU32("Unk") };
            }

            if ((temp = msgPack["Surface", true]).NotNull)
            {
                aet.Surfaces.C = temp.Array.Length;
                for (i = 0; i < aet.Surfaces.C; i++)
                {
                    ref Surface region = ref aet.Surfaces.E[i];
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
                            region.Sprites[i0] = new SpriteIdentifier() { Name = new Pointer<string>()
                                { V = sprite[i0].RS("Name") }, ID = sprite[i0].RU32("ID") };
                    }
                    sprite.Dispose();
                }
            }

            if ((temp = msgPack["Composition", true]).NotNull)
            {
                aet.Compositions.C = temp.Array.Length + 1;
                for (i = 0; i < aet.Compositions.C - 1; i++)
                {
                    ref Composition layer = ref aet.Compositions.E[i];
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
            else aet.Compositions.C = 1;

            if ((temp = msgPack["Root", true]).NotNull)
            {
                i = aet.Compositions.C - 1;
                aet.Compositions.E[i].C = temp.Array.Length;
                for (i0 = 0; i0 < aet.Compositions[i].C; i0++)
                    RAO(ref aet.Compositions.E[i].E[i0], temp[i0]);
            }
            temp.Dispose();
        }

        public MsgPack MsgPackWriter(ref Pointer<Data> aetData)
        {
            int i, i0;
            if (aetData.O <= 0) return MsgPack.Null;
            ref Data aet = ref aetData.V;

            MsgPack msgPack = MsgPack.New.Add("Name", aet.Name.V).Add("StartFrame", aet.StartFrame)
                .Add("EndFrame", aet.EndFrame).Add("FrameRate", aet.FrameRate)
                .Add("BackColor", aet.BackColor).Add("Width", aet.Width).Add("Height", aet.Height);


            if (aet.Camera.O > 0)
                msgPack.Add(new MsgPack("Camera").Add(WMP(ref aet.Camera.V.X, "X"))
                                               .Add(WMP(ref aet.Camera.V.Y, "Y")));


            i = aet.Compositions.C - 1;
            MsgPack rootLayer = new MsgPack(aet.Compositions[i].C, "Root");
            for (i0 = 0; i0 < aet.Compositions[i].C; i0++)
                rootLayer[i0] = WMP(ref aet.Compositions[i].E[i0]);
            msgPack.Add(rootLayer);

            if (aet.Compositions[i].C > 1)
            {
                MsgPack layers = new MsgPack(aet.Compositions.C - 1, "Composition");
                for (i = 0; i < aet.Compositions.C - 1; i++)
                {
                    ref Composition layer = ref aet.Compositions.E[i];
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
                msgPack.Add(layers);
            }

            MsgPack regions = new MsgPack(aet.Surfaces.C, "Surface");
            for (i = 0; i < aet.Surfaces.C; i++)
            {
                ref Surface region = ref aet.Surfaces.E[i];
                MsgPack entry = MsgPack.New.Add("ID", i).Add("Width", region.Width)
                    .Add("Height", region.Height).Add("Color", region.Color);

                if (region.Sprites.C > 0)
                {
                    entry.Add("Frames", region.Frames);
                    MsgPack sprite = new MsgPack(region.Sprites.C, "Sprite");
                    for (i0 = 0; i0 < region.Sprites.C; i0++)
                        sprite[i0] = MsgPack.New.Add("Name", region.Sprites[i0]
                            .Name.V).Add("ID", region.Sprites[i0].ID);
                    entry.Add(sprite);
                }
                regions[i] = entry;
            }
            msgPack.Add(regions);

            MsgPack sounds = new MsgPack(aet.SoundEffects.C, "SoundEffect");
            for (i = 0; i < aet.SoundEffects.C; i++)
                sounds[i] = MsgPack.New.Add("ID", i).Add("Unk", aet.SoundEffects[i].Unk);
            msgPack.Add(sounds);

            return msgPack;
        }

        private Layer RAO(ref Layer layer, MsgPack msg)
        {
            uint i;
            layer = default;
            layer.ID            = msg.RI32("ID"           );
            layer.Name.V        = msg.RS  ("Name"         );
            layer.StartFrame    = msg.RF32("StartFrame"   );
            layer.  EndFrame    = msg.RF32(  "EndFrame"   );
            layer.StartOffset   = msg.RF32("StartOffset"  );
            layer.PlaybackSpeed = msg.RF32("PlaybackSpeed");
            layer.Flags = (Layer.AetLayerFlags)msg.RU16("Flags");
            layer.Pad = msg.RU8("Pad");
            System.Enum.TryParse(msg.RS("Type"), out layer.Type);
            layer.DataID      = msg.RnI32("DataID"     ) ?? -1;
            layer.ParentLayer = msg.RnI32("ParentLayer") ?? -1;

            MsgPack temp;
            if ((temp = msg["Markers", true]).NotNull)
            {
                layer.Marker.C = temp.Array.Length;
                for (i = 0; i < layer.Marker.C; i++)
                {
                    layer.Marker.E[i].Frame   = temp[i].RF32("Frame");
                    layer.Marker.E[i]. Name.V = temp[i].RS  ( "Name");
                }
            }

            if ((temp = msg["AnimationData"]).NotNull)
            {
                layer.Data.O = 1;
                ref AnimationData data = ref layer.Data.V;
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

                MsgPack persp;
                if ((persp = temp["Persp"]).NotNull)
                {
                    data.Persp.O = 1;
                    ref AnimationData.Perspective dataPersp = ref data.Persp.V;
                    dataPersp.Unk1       = RKF(persp, "Unk1"      );
                    dataPersp.Unk2       = RKF(persp, "Unk2"      );
                    dataPersp.RotReturnX = RKF(persp, "RotReturnX");
                    dataPersp.RotReturnY = RKF(persp, "RotReturnY");
                    dataPersp.RotReturnZ = RKF(persp, "RotReturnZ");
                    dataPersp. RotationX = RKF(persp,  "RotationX");
                    dataPersp. RotationY = RKF(persp,  "RotationY");
                    dataPersp.    ScaleZ = RKF(persp,     "ScaleZ");
                }
                persp.Dispose();
            }

            if ((temp = msg["AudioData"]).NotNull)
            {
                layer.ExtraData.O = 1;
                ref AudioData data = ref layer.ExtraData.V;
                data.Data0 = RKF(temp, "Data0");
                data.Data1 = RKF(temp, "Data1");
                data.Data2 = RKF(temp, "Data2");
                data.Data3 = RKF(temp, "Data3");
            }
            temp.Dispose();
            return layer;
        }

        private CountPointer<KFT2> RKF(MsgPack msg, string name)
        {
            CountPointer<KFT2> kf = default;
            MsgPack temp;
            float? value = msg.RnF32(name);
            if (value != null) { kf.C = 1; kf.E[0].V = value.Value; }
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

        private MsgPack WMP(ref Layer layer, string name = null)
        {
            int i;
            MsgPack @object = new MsgPack(name).Add("ID", layer.ID).Add("Name", layer.Name.V)
                .Add("StartFrame", layer.StartFrame).Add("EndFrame", layer.EndFrame)
                .Add("StartOffset", layer.StartOffset).Add("PlaybackSpeed", layer.PlaybackSpeed)
                .Add("Flags", (ushort)layer.Flags).Add("Pad", layer.Pad).Add("Type", layer.Type.ToString());

            if (layer.DataID      > -1) @object.Add("DataID"     , layer.DataID     );
            if (layer.ParentLayer > -1) @object.Add("ParentLayer", layer.ParentLayer);

            if (layer.Marker.C > 0)
            {
                MsgPack markers = new MsgPack(layer.Marker.C, "Markers");
                for (i = 0; i < layer.Marker.C; i++)
                    markers[i] = MsgPack.New.Add("Frame", layer.Marker[i].Frame  )
                                            .Add( "Name", layer.Marker[i]. Name.V);
                @object.Add(markers);
            }

            if (layer.Data.O > 0)
            {
                ref AnimationData data = ref layer.Data.V;
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
                if (data.Persp.O > 0)
                {
                    ref AnimationData.Perspective dataPersp = ref data.Persp.V;
                    MsgPack persp = new MsgPack("Persp");
                    persp.Add(WMP(ref dataPersp.Unk1      , "Unk1"      ));
                    persp.Add(WMP(ref dataPersp.Unk2      , "Unk2"      ));
                    persp.Add(WMP(ref dataPersp.RotReturnX, "RotReturnX"));
                    persp.Add(WMP(ref dataPersp.RotReturnY, "RotReturnY"));
                    persp.Add(WMP(ref dataPersp.RotReturnZ, "RotReturnZ"));
                    persp.Add(WMP(ref dataPersp. RotationX,  "RotationX"));
                    persp.Add(WMP(ref dataPersp. RotationY,  "RotationY"));
                    persp.Add(WMP(ref dataPersp.    ScaleZ,     "ScaleZ"));
                    animationData.Add(persp);
                }
                @object.Add(animationData);
            }
            if (layer.ExtraData.O > 0)
            {
                ref AudioData data = ref layer.ExtraData.V;
                MsgPack extraData = new MsgPack("AudioData");
                extraData.Add(WMP(ref data.Data0, "Data0"));
                extraData.Add(WMP(ref data.Data1, "Data1"));
                extraData.Add(WMP(ref data.Data2, "Data2"));
                extraData.Add(WMP(ref data.Data3, "Data3"));
                @object.Add(extraData);
            }
            return @object;
        }

        private MsgPack WMP(ref CountPointer<KFT2> kfe, string name)
        {
            if (kfe.C <  1) return     MsgPack.Null;
            if (kfe.C == 1) return new MsgPack(name, kfe.E[0].V);
            MsgPack msg = new MsgPack(kfe.C, name);
            for (int i = 0; i < kfe.E.Length; i++)
            {
                IKF kf = kfe.E[i].Check();
                     if (kf is KFT0 kdt0) msg[i] = new MsgPack(null, new MsgPack[] { kdt0.F });
                else if (kf is KFT1 kft1) msg[i] = new MsgPack(null, new MsgPack[] { kft1.F, kft1.V });
                else if (kf is KFT2 kft2) msg[i] = new MsgPack(null, new MsgPack[] { kft2.F, kft2.V, kft2.T });
            }
            return msg;
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (_IO != null) _IO.Dispose(); AET = default; disposed = true; } }
    }
}
