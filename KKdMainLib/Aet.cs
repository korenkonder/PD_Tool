//Original: AetSet.bt Version: 5.0 by samyuu

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
            AET.Scenes = new Pointer<Scene>[i];
            for (i = 0; i < AET.Scenes.Length; i++) AET.Scenes[i] = _IO.RP<Scene>();
            for (i = 0; i < AET.Scenes.Length; i++) AETReader(ref AET.Scenes[i]);

            _IO.C();
        }

        public void AETWriter(string file)
        {
            if (AET.Scenes == null || AET.Scenes.Length == 0) return;
            _IO = File.OpenWriter();

            for (int i = 0; i < AET.Scenes.Length; i++)
            {
                _IO.P = _IO.L + 0x20;
                AETWriter(ref AET.Scenes[i]);
            }

            _IO.P = 0;
            for (int i = 0; i < AET.Scenes.Length; i++)
                if (AET.Scenes[i].O > 0) _IO.W(AET.Scenes[i].O);
            byte[] data = _IO.ToArray();
            _IO.Dispose();

            using (_IO = File.OpenWriter(file + ".bin", true))
                _IO.W(data);
            data = null;
        }

        private void AETReader(ref Pointer<Scene> aetData)
        {
            _IO.P = aetData.O;
            ref Scene aet = ref aetData.V;

            aet.Name = _IO.RPS();
            aet.StartFrame = _IO.RF32();
            aet.EndFrame   = _IO.RF32();
            aet.FrameRate  = _IO.RF32();
            aet.BackColor  = _IO.RU32();
            aet.Width      = _IO.RU32();
            aet.Height     = _IO.RU32();
            aet.Camera       = _IO.RP<Vector2<CountPointer<KFT2>>>();
            aet.Compositions = _IO.ReadCountPointer<Composition>();
            aet.Videos       = _IO.ReadCountPointer<Video      >();
            aet.Audios       = _IO.ReadCountPointer<Audio      >();

            if (aet.Camera.O > 0)
            {
                _IO.P = aet.Camera.O;
                aet.Camera.V.X.O = _IO.RI32();
                aet.Camera.V.Y.O = _IO.RI32();
                RKF(ref aet.Camera.V.X);
                RKF(ref aet.Camera.V.Y);
            }

            if (aet.Audios.O > 0)
            {
                _IO.P = aet.Audios.O;
                for (i = 0; i < aet.Audios.C; i++)
                {
                    ref Audio aif = ref aet.Audios.E[i];
                    aif.O = _IO.P;
                    aif.SoundID = _IO.RU32();
                }
            }

            _IO.P = aet.Compositions.O;
            for (i = 0; i < aet.Compositions.C; i++)
                aet.Compositions[i] = new Composition() { P = _IO.P, C = _IO.RI32(), O = _IO.RI32()};

            i1 = 0;
            RAL(ref aet.Compositions.E[aet.Compositions.C - 1]);
            for (i = 0; i < aet.Compositions.C - 1; i++)
                RAL(ref aet.Compositions.E[i]);

            _IO.P = aet.Videos.O;
            for (i = 0; i < aet.Videos.C; i++)
                aet.Videos[i] = new Video { O = _IO.P, Color = _IO.RU32(), Width = _IO.RU16(),
                    Height = _IO.RU16(), Frames = _IO.RF32(), Sources = _IO.ReadCountPointer<Video.Source>() };

            for (i = 0; i < aet.Videos.C; i++)
            {
                _IO.P = aet.Videos[i].Sources.O;
                for (i0 = 0; i0 < aet.Videos[i].Sources.C; i0++)
                    aet.Videos.E[i].Sources[i0] = new Video.Source { Name = _IO.RPS(), ID = _IO.RU32() };
            }

            for (i = 0; i < aet.Compositions.C; i++)
            {
                for (i0 = 0; i0 < aet.Compositions[i].C; i0++)
                {
                    ref Layer obj = ref aet.Compositions[i].E[i0];
                    obj.DataID = -1;
                    int dataOffset = obj.VideoItemOffset;
                    if (dataOffset > 0)
                             if (obj.Type == Layer.AetLayerType.Video      )
                            for (i1 = 0; i1 < aet.Videos      .C; i1++)
                            { if (aet.Videos      [i1].O == dataOffset) { obj.DataID = i1; break; } }
                        else if (obj.Type == Layer.AetLayerType.Audio      )
                            for (i1 = 0; i1 < aet.Audios      .C; i1++)
                            { if (aet.Audios      [i1].O == dataOffset) { obj.DataID = i1; break; } }
                        else if (obj.Type == Layer.AetLayerType.Composition)
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

        private void AETWriter(ref Pointer<Scene> aetData)
        {
            ref Scene aet = ref aetData.V;

            for (i = 0; i < aet.Videos.C; i++)
            {
                ref Video region = ref aet.Videos.E[i];
                if (region.Sources.C == 0) { region.Sources.O = 0; continue; }
                if (region.Sources.C > 1) _IO.A(0x20);
                region.Sources.O = _IO.P;
                for (i0 = 0; i0 < region.Sources.C; i0++) _IO.W(0L);
            }

            _IO.A(0x20);
            aet.Videos.O = _IO.P;
            for (i = 0; i < aet.Videos.C; i++)
            {
                ref Video region = ref aet.Videos.E[i];
                region.O = _IO.P;
                _IO.W(region.Color  );
                _IO.W(region.Width  );
                _IO.W(region.Height );
                _IO.W(region.Frames );
                _IO.W(region.Sources);
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
                    ref VideoData data = ref obj.Video.V;
                    ref VideoData.Video3DData persp = ref data.Video3D.V;
                    ref AudioData extraData = ref obj.Audio.V;
                    if (obj.Markers.C > 0)
                    {
                        if (obj.Markers.C > 3) _IO.A(0x20);
                        obj.Markers.O = _IO.P;
                        for (i1 = 0; i1 < obj.Markers.C; i1++)
                            _IO.W(0L);
                    }

                    if (obj.Video.V.Video3D.O > 0)
                    {
                        W(ref persp.   AnchorZ);
                        W(ref persp. PositionZ);
                        W(ref persp.DirectionX);
                        W(ref persp.DirectionY);
                        W(ref persp.DirectionZ);
                        W(ref persp. RotationX);
                        W(ref persp. RotationY);
                        W(ref persp.    ScaleZ);
                    }

                    if (obj.Video.O > 0)
                    {
                         W(ref data.  AnchorX);
                         W(ref data.  AnchorY);
                         W(ref data.PositionX);
                         W(ref data.PositionY);
                         W(ref data.Rotation );
                         W(ref data.   ScaleX);
                         W(ref data.   ScaleY);
                         W(ref data. Opacity );
                    }

                    if (obj.Video.V.Video3D.O > 0)
                    {
                        _IO.A(0x20);
                        data.Video3D.O = _IO.P;
                        _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
                        _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
                    }

                    if (obj.Audio.O > 0)
                    {
                        W(ref extraData.VolumeL);
                        W(ref extraData.VolumeR);
                        W(ref extraData.   PanL);
                        W(ref extraData.   PanR);
                    }

                    if (obj.Video.O > 0)
                    {
                        _IO.A(0x20);
                        obj.Video.O = _IO.P;
                        _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
                        _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L); _IO.W(0L);
                    }

                    if (obj.Audio.O > 0)
                    {
                        _IO.A(0x20);
                        obj.Audio.O = _IO.P;
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

                for (i = 0; i < aet.Videos.C; i++)
                {
                    ref Video region = ref aet.Videos.E[i];
                    for (i0 = 0; i0 < region.Sources.C; i0++)
                        WPS(ref usedValues, ref region.Sources.E[i0].Name);
                }

                //_IO.Align(0x4);
                for (i = 0; i < aet.Compositions.C; i++)
                {
                    for (i0 = 0; i0 < aet.Compositions.E[i].C; i0++)
                    {
                        ref Layer obj = ref aet.Compositions.E[i].E[i0];
                        for (i1 = 0; i1 < obj.Markers.C; i1++)
                            WPS(ref usedValues, ref obj.Markers.E[i1].Name);
                    }

                    for (i0 = 0; i0 < aet.Compositions.E[i].C; i0++)
                        WPS(ref usedValues, ref aet.Compositions.E[i].E[i0].Name);
                }

                //_IO.Align(0x4);
                aet.Name.O = _IO.P;
                _IO.W(aet.Name.V + x00);
                _IO.SL(_IO.P);
            }

            aet.Audios.O = 0;
            if (aet.Audios.C > 0)
            {
                _IO.A(0x10);
                aet.Audios.O = _IO.P;
                for (i = 0; i < aet.Audios.C; i++)
                {
                    aet.Audios.E[i].O = _IO.P;
                    _IO.W(aet.Audios[i].SoundID);
                }
            }

            for (i = 0; i < aet.Compositions.C; i++)
                for (i0 = 0; i0 < aet.Compositions.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Compositions.E[i].E[i0];
                         if (obj.Type == Layer.AetLayerType.Video      ) obj.VideoItemOffset = aet.Videos      [obj.DataID].O;
                    else if (obj.Type == Layer.AetLayerType.Audio      ) obj.VideoItemOffset = aet.Audios      [obj.DataID].O;
                    else if (obj.Type == Layer.AetLayerType.Composition) obj.VideoItemOffset = aet.Compositions[obj.DataID].O;
                    else obj.VideoItemOffset = 0;
                    if (obj.VideoItemOffset == 0)
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
                    ref VideoData data = ref obj.Video.V;
                    ref VideoData.Video3DData persp = ref data.Video3D.V;
                    ref AudioData extraData = ref obj.Audio.V;

                    if (obj.Video.V.Video3D.O > 0)
                    {
                        _IO.P = data.Video3D.O;
                        W(ref _IO, ref persp.   AnchorZ);
                        W(ref _IO, ref persp. PositionZ);
                        W(ref _IO, ref persp.DirectionX);
                        W(ref _IO, ref persp.DirectionY);
                        W(ref _IO, ref persp.DirectionZ);
                        W(ref _IO, ref persp. RotationX);
                        W(ref _IO, ref persp. RotationY);
                        W(ref _IO, ref persp.    ScaleZ);
                    }

                    if (obj.Video.O > 0)
                    {
                        _IO.P = obj.Video.O;
                        _IO.W((byte)data.TransferMode.BlendMode );
                        _IO.W((byte)data.TransferMode.Flags     );
                        _IO.W((byte)data.TransferMode.TrackMatte);
                        _IO.W((byte)0);

                        W(ref _IO, ref data.  AnchorX);
                        W(ref _IO, ref data.  AnchorY);
                        W(ref _IO, ref data.PositionX);
                        W(ref _IO, ref data.PositionY);
                        W(ref _IO, ref data.Rotation );
                        W(ref _IO, ref data.   ScaleX);
                        W(ref _IO, ref data.   ScaleY);
                        W(ref _IO, ref data. Opacity );
                        _IO.W(data.Video3D.O);
                    }

                    if (obj.Audio.O > 0)
                    {
                        _IO.P = obj.Audio.O;
                        W(ref _IO, ref extraData.VolumeL);
                        W(ref _IO, ref extraData.VolumeR);
                        W(ref _IO, ref extraData.   PanL);
                        W(ref _IO, ref extraData.   PanR);
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
                    _IO.W(obj.OffsetFrame);
                    _IO.W(obj.TimeScale);
                    _IO.W((ushort)obj.Flags  );
                    _IO.W((  byte)obj.Quality);
                    _IO.W((  byte)obj.Type   );
                    _IO.W(obj.VideoItemOffset);

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
                    _IO.W(obj.Markers);
                    _IO.W(obj.Video.O);
                    _IO.W(obj.Audio.O);

                    ref CountPointer<Marker> Marker = ref obj.Markers;
                    _IO.P = Marker.O;
                    for (i1 = 0; i1 < Marker.C; i1++)
                    {
                        _IO.W(Marker[i1].Frame);
                        _IO.W(Marker[i1].Name.O);
                    }
                }

            _IO.F();

            for (i = 0; i < aet.Videos.C; i++)
            {
                ref Video region = ref aet.Videos.E[i];
                if (region.Sources.C == 0) continue;
                _IO.P = region.Sources.O;
                for (i0 = 0; i0 < region.Sources.C; i0++)
                {
                    _IO.W(region.Sources[i0].Name.O);
                    _IO.W(region.Sources[i0].ID);
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
            _IO.W(aet.Compositions);
            _IO.W(aet.Videos);
            _IO.W(aet.Audios);
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
                obj.OffsetFrame   = _IO.RF32();
                obj.TimeScale = _IO.RF32();
                obj.Flags   = (Layer.AetLayerFlags  )_IO.RU16();
                obj.Quality = (Layer.AetLayerQuality)_IO.RU8 ();
                obj.Type    = (Layer.AetLayerType   )_IO.RU8 ();
                obj.VideoItemOffset  = _IO.RI32();
                obj.ParentLayer = _IO.RI32();
                obj.Markers = _IO.ReadCountPointer<Marker>();
                obj.Video = _IO.RP<VideoData>();
                obj.Audio = _IO.RP<AudioData>();

                if (obj.Video.O > 0)
                {
                    ref VideoData data = ref obj.Video.V;
                    _IO.P = obj.Video.O;
                    data.TransferMode.BlendMode  = (VideoData.VideoTransferMode.TransferBlendMode )_IO.RU8();
                    data.TransferMode.Flags      = (VideoData.VideoTransferMode.TransferFlags     )_IO.RU8();
                    data.TransferMode.TrackMatte = (VideoData.VideoTransferMode.TransferTrackMatte)_IO.RU8();
                    data.Padding = _IO.RU8();
                    data.  AnchorX = _IO.ReadCountPointer<KFT2>();
                    data.  AnchorY = _IO.ReadCountPointer<KFT2>();
                    data.PositionX = _IO.ReadCountPointer<KFT2>();
                    data.PositionY = _IO.ReadCountPointer<KFT2>();
                    data.Rotation  = _IO.ReadCountPointer<KFT2>();
                    data.   ScaleX = _IO.ReadCountPointer<KFT2>();
                    data.   ScaleY = _IO.ReadCountPointer<KFT2>();
                    data.Opacity   = _IO.ReadCountPointer<KFT2>();
                    data.Video3D = _IO.RP<VideoData.Video3DData>();

                    RKF(ref data.  AnchorX);
                    RKF(ref data.  AnchorY);
                    RKF(ref data.PositionX);
                    RKF(ref data.PositionY);
                    RKF(ref data.Rotation );
                    RKF(ref data.   ScaleX);
                    RKF(ref data.   ScaleY);
                    RKF(ref data. Opacity );

                    if (data.Video3D.O > 0)
                    {
                        ref VideoData.Video3DData Persp = ref data.Video3D.V;
                        _IO.P = data.Video3D.O;
                        Persp.AnchorZ       = _IO.ReadCountPointer<KFT2>();
                        Persp.PositionZ       = _IO.ReadCountPointer<KFT2>();
                        Persp.DirectionX = _IO.ReadCountPointer<KFT2>();
                        Persp.DirectionY = _IO.ReadCountPointer<KFT2>();
                        Persp.DirectionZ = _IO.ReadCountPointer<KFT2>();
                        Persp. RotationX = _IO.ReadCountPointer<KFT2>();
                        Persp. RotationY = _IO.ReadCountPointer<KFT2>();
                        Persp.    ScaleZ = _IO.ReadCountPointer<KFT2>();

                        RKF(ref Persp.   AnchorZ);
                        RKF(ref Persp. PositionZ);
                        RKF(ref Persp.DirectionX);
                        RKF(ref Persp.DirectionY);
                        RKF(ref Persp.DirectionZ);
                        RKF(ref Persp. RotationX);
                        RKF(ref Persp. RotationY);
                        RKF(ref Persp.    ScaleZ);
                    }
                }

                if (obj.Audio.O > 0)
                {
                    ref AudioData extraData = ref obj.Audio.V;
                    _IO.P = obj.Audio.O;
                    extraData.VolumeL = _IO.ReadCountPointer<KFT2>();
                    extraData.VolumeR = _IO.ReadCountPointer<KFT2>();
                    extraData.   PanL = _IO.ReadCountPointer<KFT2>();
                    extraData.   PanR = _IO.ReadCountPointer<KFT2>();

                    RKF(ref extraData.VolumeL);
                    RKF(ref extraData.VolumeR);
                    RKF(ref extraData.   PanL);
                    RKF(ref extraData.   PanR);
                }

                _IO.P = obj.Markers.O;
                for (i2 = 0; i2 < obj.Markers.C; i2++)
                    obj.Markers[i2] = new Marker() { Frame = _IO.RF32(), Name = _IO.RPSSJIS() };
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
                    AET.Scenes = new Pointer<Scene>[aet.Array.Length];
                    for (int i = 0; i < AET.Scenes.Length; i++)
                        MsgPackReader(aet.Array[i], ref AET.Scenes[i]);
                }
            aet.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (AET.Scenes == null || AET.Scenes.Length == 0) return;

            MsgPack aet = new MsgPack(AET.Scenes.Length, "Aet");
            for (int i = 0; i < AET.Scenes.Length; i++)
                aet[i] = MsgPackWriter(ref AET.Scenes[i]);
            aet.WriteAfterAll(true, file, json);
        }

        public void MsgPackReader(MsgPack msgPack, ref Pointer<Scene> aetData)
        {
            int i, i0;
            aetData = default;
            ref Scene aet = ref aetData.V;

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
                aet.Audios.C = temp.Array.Length;
                for (i = 0; i < aet.Audios.C; i++)
                    aet.Audios.E[i] = new Audio { SoundID = temp[i].RU32("Unk") };
            }

            if ((temp = msgPack["Surface", true]).NotNull)
            {
                aet.Videos.C = temp.Array.Length;
                for (i = 0; i < aet.Videos.C; i++)
                {
                    ref Video region = ref aet.Videos.E[i];
                    region = default;
                    region.Color  = temp[i].RU32("Color" );
                    region.Width  = temp[i].RU16("Width" );
                    region.Height = temp[i].RU16("Height");
                    region.Frames = temp[i].RF32("Frames");

                    MsgPack sprite;
                    if ((sprite = temp[i]["Sprite", true]).NotNull)
                    {
                        region.Sources.C = sprite.Array.Length;
                        for (i0 = 0; i0 < region.Sources.C; i0++)
                            region.Sources[i0] = new Video.Source() { Name = new Pointer<string>()
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

        public MsgPack MsgPackWriter(ref Pointer<Scene> aetData)
        {
            int i, i0;
            if (aetData.O <= 0) return MsgPack.Null;
            ref Scene aet = ref aetData.V;

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
                MsgPack compositions = new MsgPack(aet.Compositions.C - 1, "Composition");
                for (i = 0; i < aet.Compositions.C - 1; i++)
                {
                    ref Composition composition = ref aet.Compositions.E[i];
                    MsgPack compos = MsgPack.New.Add("ID", i);
                    if (composition.C > 0)
                    {
                        MsgPack objects = new MsgPack(composition.C, "Layer");
                        for (i0 = 0; i0 < composition.C; i0++)
                            objects[i0] = WMP(ref composition.E[i0]);
                        compos.Add(objects);
                    }
                    compositions[i] = compos;
                }
                msgPack.Add(compositions);
            }

            MsgPack videos = new MsgPack(aet.Videos.C, "Video");
            for (i = 0; i < aet.Videos.C; i++)
            {
                ref Video video = ref aet.Videos.E[i];
                MsgPack entry = MsgPack.New.Add("ID", i).Add("Width", video.Width)
                    .Add("Height", video.Height).Add("Color", video.Color);

                if (video.Sources.C > 0)
                {
                    entry.Add("Frames", video.Frames);
                    MsgPack source = new MsgPack(video.Sources.C, "Source");
                    for (i0 = 0; i0 < video.Sources.C; i0++)
                        source[i0] = MsgPack.New.Add("Name", video.Sources[i0]
                            .Name.V).Add("ID", video.Sources[i0].ID);
                    entry.Add(source);
                }
                videos[i] = entry;
            }
            msgPack.Add(videos);

            MsgPack audios = new MsgPack(aet.Audios.C, "Audios");
            for (i = 0; i < aet.Audios.C; i++)
                audios[i] = MsgPack.New.Add("ID", i).Add("SoundID", aet.Audios[i].SoundID);
            msgPack.Add(audios);

            return msgPack;
        }

        private Layer RAO(ref Layer layer, MsgPack msg)
        {
            uint i;
            layer = default;
            layer.ID     = msg.RI32("ID"  );
            layer.Name.V = msg.RS  ("Name");
            layer. StartFrame = msg.RF32( "StartFrame");
            layer.   EndFrame = msg.RF32(   "EndFrame");
            layer.OffsetFrame = msg.RF32("OffsetFrame");
            layer.  TimeScale = msg.RF32(  "TimeScale");
            layer.Flags = (Layer.AetLayerFlags)msg.RU16("Flags");
            System.Enum.TryParse(msg.RS("Quality"), out layer.Quality);
            System.Enum.TryParse(msg.RS("Type"   ), out layer.Type   );
            layer.DataID      = msg.RnI32("DataID"     ) ?? -1;
            layer.ParentLayer = msg.RnI32("ParentLayer") ?? -1;

            MsgPack temp;
            if ((temp = msg["Markers", true]).NotNull)
            {
                layer.Markers.C = temp.Array.Length;
                for (i = 0; i < layer.Markers.C; i++)
                {
                    layer.Markers.E[i].Frame   = temp[i].RF32("Frame");
                    layer.Markers.E[i]. Name.V = temp[i].RS  ( "Name");
                }
            }

            if ((temp = msg["VideoData"]).NotNull)
            {
                layer.Video.O = 1;
                ref VideoData video = ref layer.Video.V;
                System.Enum.TryParse(temp.RS("BlendMode"), out video.TransferMode.BlendMode);
                video.TransferMode.Flags = (VideoData.VideoTransferMode.TransferFlags)temp.RU8("Flags");
                System.Enum.TryParse(temp.RS("TrackMatte"), out video.TransferMode.TrackMatte);

                video.  AnchorX = RKF(temp,   "AnchorX");
                video.  AnchorY = RKF(temp,   "AnchorY");
                video.PositionX = RKF(temp, "PositionX");
                video.PositionY = RKF(temp, "PositionY");
                video.Rotation  = RKF(temp, "Rotation" );
                video.   ScaleX = RKF(temp,    "ScaleX");
                video.   ScaleY = RKF(temp,    "ScaleY");
                video. Opacity  = RKF(temp,  "Opacity" );

                MsgPack video3D;
                if ((video3D = temp["Video3DData"]).NotNull)
                {
                    video.Video3D.O = 1;
                    ref VideoData.Video3DData video3DData = ref video.Video3D.V;
                    video3DData.   AnchorZ = RKF(video3D,    "AnchorZ");
                    video3DData. PositionZ = RKF(video3D,  "PositionZ");
                    video3DData.DirectionX = RKF(video3D, "DirectionX");
                    video3DData.DirectionY = RKF(video3D, "DirectionY");
                    video3DData.DirectionZ = RKF(video3D, "DirectionZ");
                    video3DData. RotationX = RKF(video3D,  "RotationX");
                    video3DData. RotationY = RKF(video3D,  "RotationY");
                    video3DData.    ScaleZ = RKF(video3D,     "ScaleZ");
                }
                video3D.Dispose();
            }

            if ((temp = msg["Audio"]).NotNull)
            {
                layer.Audio.O = 1;
                ref AudioData data = ref layer.Audio.V;
                data.VolumeL = RKF(temp, "VolumeL");
                data.VolumeR = RKF(temp, "VolumeR");
                data.   PanL = RKF(temp,    "PanL");
                data.   PanR = RKF(temp,    "PanR");
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
            MsgPack layerData = new MsgPack(name).Add("ID", layer.ID).Add("Name", layer.Name.V)
                .Add("StartFrame", layer.StartFrame).Add("EndFrame", layer.EndFrame)
                .Add("OffsetFrame", layer.OffsetFrame).Add("TimeScale", layer.TimeScale)
                .Add("Flags", (ushort)layer.Flags).Add("Quality", layer.Quality.ToString())
                .Add("Type", layer.Type.ToString());

            if (layer.DataID      > -1) layerData.Add("DataID"     , layer.DataID     );
            if (layer.ParentLayer > -1) layerData.Add("ParentLayer", layer.ParentLayer);

            if (layer.Markers.C > 0)
            {
                MsgPack markers = new MsgPack(layer.Markers.C, "Markers");
                for (i = 0; i < layer.Markers.C; i++)
                    markers[i] = MsgPack.New.Add("Frame", layer.Markers[i].Frame  )
                                            .Add( "Name", layer.Markers[i]. Name.V);
                layerData.Add(markers);
            }

            if (layer.Video.O > 0)
            {
                ref VideoData video = ref layer.Video.V;
                MsgPack VideoData = new MsgPack("VideoData")
                    .Add("BlendMode" , video.TransferMode.BlendMode .ToString())
                    .Add("Flags"     , (byte)video.TransferMode.Flags)
                    .Add("TrackMatte", video.TransferMode.TrackMatte.ToString());

                VideoData.Add(WMP(ref video.AnchorX  ,   "OriginX"));
                VideoData.Add(WMP(ref video.AnchorY  ,   "OriginY"));
                VideoData.Add(WMP(ref video.PositionX, "PositionX"));
                VideoData.Add(WMP(ref video.PositionY, "PositionY"));
                VideoData.Add(WMP(ref video.Rotation , "Rotation" ));
                VideoData.Add(WMP(ref video.   ScaleX,    "ScaleX"));
                VideoData.Add(WMP(ref video.   ScaleY,    "ScaleY"));
                VideoData.Add(WMP(ref video. Opacity ,  "Opacity" ));
                if (video.Video3D.O > 0)
                {
                    ref VideoData.Video3DData video3D = ref video.Video3D.V;
                    MsgPack video3DData = new MsgPack("Persp");
                    video3DData.Add(WMP(ref video3D.   AnchorZ,    "AnchorZ"));
                    video3DData.Add(WMP(ref video3D. PositionZ,  "PositionZ"));
                    video3DData.Add(WMP(ref video3D.DirectionX, "DirectionX"));
                    video3DData.Add(WMP(ref video3D.DirectionY, "DirectionY"));
                    video3DData.Add(WMP(ref video3D.DirectionZ, "DirectionZ"));
                    video3DData.Add(WMP(ref video3D. RotationX,  "RotationX"));
                    video3DData.Add(WMP(ref video3D. RotationY,  "RotationY"));
                    video3DData.Add(WMP(ref video3D.    ScaleZ,     "ScaleZ"));
                    VideoData.Add(video3DData);
                }
                layerData.Add(VideoData);
            }
            if (layer.Audio.O > 0)
            {
                ref AudioData data = ref layer.Audio.V;
                MsgPack audioData = new MsgPack("AudioData");
                audioData.Add(WMP(ref data.VolumeL, "VolumeL"));
                audioData.Add(WMP(ref data.VolumeR, "VolumeR"));
                audioData.Add(WMP(ref data.   PanL,    "PanL"));
                audioData.Add(WMP(ref data.   PanR,    "PanR"));
                layerData.Add(audioData);
            }
            return layerData;
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
