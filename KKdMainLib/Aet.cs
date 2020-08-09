//Original: AetSet.bt Version: 5.0 by samyuu

using KKdBaseLib;
using KKdBaseLib.Auth2D;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct Aet : System.IDisposable
    {
        private int i, i0, i1, i2;
        private Stream s;
        private const string x00 = "\0";

        public Header AET;

        public void AETReader(string file)
        {
            AET = default;
            s = File.OpenReader(file + ".bin", true);

            int i = 0;
            int Pos = -1;
            while (true) { Pos = s.RI32(); if (Pos != 0 && Pos < s.L) i++; else break; }

            s.P = 0;
            AET.Scenes = new Pointer<Scene>[i];
            for (i = 0; i < AET.Scenes.Length; i++) AET.Scenes[i] = s.RP<Scene>();
            for (i = 0; i < AET.Scenes.Length; i++) AETReader(ref AET.Scenes[i]);

            s.C();
        }

        public void AETWriter(string file)
        {
            if (AET.Scenes == null || AET.Scenes.Length == 0) return;
            s = File.OpenWriter();

            for (int i = 0; i < AET.Scenes.Length; i++)
            {
                s.P = s.L + 0x20;
                AETWriter(ref AET.Scenes[i]);
            }

            s.P = 0;
            for (int i = 0; i < AET.Scenes.Length; i++)
                if (AET.Scenes[i].O > 0) s.W(AET.Scenes[i].O);
            byte[] data = s.ToArray();
            s.Dispose();

            using (s = File.OpenWriter(file + ".bin", true))
                s.W(data);
            data = null;
        }

        private void AETReader(ref Pointer<Scene> aetData)
        {
            s.P = aetData.O;
            ref Scene aet = ref aetData.V;

            aet.Name = s.RPS();
            aet.StartFrame  = s.RF32();
            aet.EndFrame    = s.RF32();
            aet.FrameRate   = s.RF32();
            aet.BackColor   = s.RU32();
            aet.Width       = s.RU32();
            aet.Height      = s.RU32();
            aet.Camera      = s.RP<Vec2<CountPointer<KFT2>>>();
            aet.Composition = s.ReadCountPointer<Composition>();
            aet.Video       = s.ReadCountPointer<Video      >();
            aet.Audio       = s.ReadCountPointer<Audio      >();

            if (aet.Camera.O > 0)
            {
                s.P = aet.Camera.O;
                aet.Camera.V.X.O = s.RI32();
                aet.Camera.V.Y.O = s.RI32();
                RKF(ref aet.Camera.V.X);
                RKF(ref aet.Camera.V.Y);
            }

            if (aet.Audio.O > 0)
            {
                s.P = aet.Audio.O;
                for (i = 0; i < aet.Audio.C; i++)
                {
                    ref Audio aif = ref aet.Audio.E[i];
                    aif.O = s.P;
                    aif.SoundID = s.RU32();
                }
            }

            s.P = aet.Composition.O;
            for (i = 0; i < aet.Composition.C; i++)
                aet.Composition[i] = new Composition() { P = s.P, C = s.RI32(), O = s.RI32()};

            i1 = 0;
            RAL(ref aet.Composition.E[aet.Composition.C - 1]);
            for (i = 0; i < aet.Composition.C - 1; i++)
                RAL(ref aet.Composition.E[i]);

            s.P = aet.Video.O;
            for (i = 0; i < aet.Video.C; i++)
                aet.Video[i] = new Video { O = s.P, Color = s.RU32(), Width = s.RU16(),
                    Height = s.RU16(), Frames = s.RF32(), Identifiers = s.ReadCountPointer<Video.Identifier>() };

            for (i = 0; i < aet.Video.C; i++)
            {
                s.P = aet.Video[i].Identifiers.O;
                for (i0 = 0; i0 < aet.Video[i].Identifiers.C; i0++)
                    aet.Video.E[i].Identifiers[i0] = new Video.Identifier { Name = s.RPS(), ID = s.RU32() };
            }

            for (i = 0; i < aet.Composition.C; i++)
            {
                for (i0 = 0; i0 < aet.Composition[i].C; i0++)
                {
                    ref Layer obj = ref aet.Composition[i].E[i0];
                    obj.DataID = -1;
                    int dataOffset = obj.VidItmOff;
                    if (dataOffset > 0)
                             if (obj.Type == Layer.AetLayerType.Video      )
                            for (i1 = 0; i1 < aet.Video      .C; i1++)
                            { if (aet.Video      [i1].O == dataOffset) { obj.DataID = i1; break; } }
                        else if (obj.Type == Layer.AetLayerType.Audio      )
                            for (i1 = 0; i1 < aet.Audio      .C; i1++)
                            { if (aet.Audio      [i1].O == dataOffset) { obj.DataID = i1; break; } }
                        else if (obj.Type == Layer.AetLayerType.Composition)
                            for (i1 = 0; i1 < aet.Composition.C; i1++)
                            { if (aet.Composition[i1].P == dataOffset) { obj.DataID = i1; break; } }

                    int parentLayer = obj.ParentLayer;
                    if (parentLayer > 0)
                        for (i1 = 0; i1 < aet.Composition.C; i1++)
                        {
                            for (i2 = 0; i2 < aet.Composition[i1].C; i2++)
                                if (aet.Composition[i1].E[i2].Offset == parentLayer)
                                { obj.ParentLayer = aet.Composition[i1].E[i2].ID; break; }
                            if (parentLayer != obj.ParentLayer) break;
                        }
                    if (parentLayer == obj.ParentLayer) obj.ParentLayer = -1;
                }
            }
        }

        private void AETWriter(ref Pointer<Scene> aetData)
        {
            ref Scene aet = ref aetData.V;

            for (i = 0; i < aet.Video.C; i++)
            {
                ref Video region = ref aet.Video.E[i];
                if (region.Identifiers.C == 0) { region.Identifiers.O = 0; continue; }
                if (region.Identifiers.C > 1) s.A(0x20);
                region.Identifiers.O = s.P;
                for (i0 = 0; i0 < region.Identifiers.C; i0++) s.W(0L);
            }

            s.A(0x20);
            aet.Video.O = s.P;
            for (i = 0; i < aet.Video.C; i++)
            {
                ref Video region = ref aet.Video.E[i];
                region.O = s.P;
                s.W(region.Color  );
                s.W(region.Width  );
                s.W(region.Height );
                s.W(region.Frames );
                s.W(region.Identifiers);
            }

            s.A(0x20);
            aet.Composition.O = s.P;
            for (i = 0; i < aet.Composition.C; i++)
            {
                aet.Composition.E[i].O = s.P;
                s.W(0L);
            }

            for (i = 0; i < aet.Composition.C; i++)
            {
                if (aet.Composition.E[i].C < 1) continue;

                for (i0 = 0; i0 < aet.Composition.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Composition.E[i].E[i0];
                    ref VideoData video = ref obj.Video.V;
                    ref VideoData.Video3DData video3DData = ref video.Video3D.V;
                    ref AudioData audio = ref obj.Audio.V;
                    if (obj.Marker.C > 0)
                    {
                        if (obj.Marker.C > 3) s.A(0x20);
                        obj.Marker.O = s.P;
                        for (i1 = 0; i1 < obj.Marker.C; i1++)
                            s.W(0L);
                    }

                    if (obj.Video.V.Video3D.O > 0)
                    {
                        W(ref video3DData.   AnchorZ);
                        W(ref video3DData. PositionZ);
                        W(ref video3DData.DirectionX);
                        W(ref video3DData.DirectionY);
                        W(ref video3DData.DirectionZ);
                        W(ref video3DData. RotationX);
                        W(ref video3DData. RotationY);
                        W(ref video3DData.    ScaleZ);
                    }

                    if (obj.Video.O > 0)
                    {
                         W(ref video.  AnchorX);
                         W(ref video.  AnchorY);
                         W(ref video.PositionX);
                         W(ref video.PositionY);
                         W(ref video.Rotation );
                         W(ref video.   ScaleX);
                         W(ref video.   ScaleY);
                         W(ref video. Opacity );
                    }

                    if (obj.Video.V.Video3D.O > 0)
                    {
                        s.A(0x20);
                        video.Video3D.O = s.P;
                        s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                        s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                    }

                    if (obj.Audio.O > 0)
                    {
                        W(ref audio.VolumeL);
                        W(ref audio.VolumeR);
                        W(ref audio.   PanL);
                        W(ref audio.   PanR);
                    }

                    if (obj.Video.O > 0)
                    {
                        s.A(0x20);
                        obj.Video.O = s.P;
                        s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                        s.W(0L); s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                    }

                    if (obj.Audio.O > 0)
                    {
                        s.A(0x20);
                        obj.Audio.O = s.P;
                        s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                    }
                }

                s.A(0x20);
                aet.Composition.E[i].E[0].Offset = s.P;
                for (i0 = 0; i0 < aet.Composition.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Composition.E[i].E[i0];
                    obj.Offset = s.P;
                    s.W(0L); s.W(0L); s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                }
            }

            if (aet.Camera.O > 0)
            {
                ref Vec2<CountPointer<KFT2>> pos = ref aet.Camera.V;
                s.A(0x10);
                W(ref pos.X);
                W(ref pos.Y);

                s.A(0x20);
                aet.Camera.O = s.P;
                s.W(pos.X);
                s.W(pos.Y);

                W(ref pos.X);
                W(ref pos.Y);
            }

            s.A(0x20);
            aetData.O = s.P;
            s.W(0L); s.W(0L); s.W(0L); s.W(0L);
            s.W(0L); s.W(0L); s.W(0L); s.W(0L);

            s.A(0x10);
            {
                System.Collections.Generic.Dictionary<string, int> usedValues =
                    new System.Collections.Generic.Dictionary<string, int>();

                for (i = 0; i < aet.Video.C; i++)
                {
                    ref Video region = ref aet.Video.E[i];
                    for (i0 = 0; i0 < region.Identifiers.C; i0++)
                        WPS(ref usedValues, ref region.Identifiers.E[i0].Name);
                }

                //_IO.Align(0x4);
                for (i = 0; i < aet.Composition.C; i++)
                {
                    for (i0 = 0; i0 < aet.Composition.E[i].C; i0++)
                    {
                        ref Layer obj = ref aet.Composition.E[i].E[i0];
                        for (i1 = 0; i1 < obj.Marker.C; i1++)
                            WPS(ref usedValues, ref obj.Marker.E[i1].Name);
                    }

                    for (i0 = 0; i0 < aet.Composition.E[i].C; i0++)
                        WPS(ref usedValues, ref aet.Composition.E[i].E[i0].Name);
                }

                //_IO.Align(0x4);
                aet.Name.O = s.P;
                s.W(aet.Name.V + x00);
                s.SL(s.P);
            }

            aet.Audio.O = 0;
            if (aet.Audio.C > 0)
            {
                s.A(0x10);
                aet.Audio.O = s.P;
                for (i = 0; i < aet.Audio.C; i++)
                {
                    aet.Audio.E[i].O = s.P;
                    s.W(aet.Audio[i].SoundID);
                }
            }

            for (i = 0; i < aet.Composition.C; i++)
                for (i0 = 0; i0 < aet.Composition.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Composition.E[i].E[i0];
                         if (obj.Type == Layer.AetLayerType.Video      ) obj.VidItmOff = aet.Video      [obj.DataID].O;
                    else if (obj.Type == Layer.AetLayerType.Audio      ) obj.VidItmOff = aet.Audio      [obj.DataID].O;
                    else if (obj.Type == Layer.AetLayerType.Composition) obj.VidItmOff = aet.Composition[obj.DataID].O;
                    else obj.VidItmOff = 0;
                    if (obj.VidItmOff == 0)
                    {

                    }
                }

            s.A(0x4);
            int returnPos = s.P;
            int nvp = 0;

            s.F();

            for (i = 0; i < aet.Composition.C; i++)
            {
                if (aet.Composition.E[i].C < 1) continue;

                for (i0 = 0; i0 < aet.Composition.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Composition.E[i].E[i0];
                    ref VideoData video = ref obj.Video.V;
                    ref VideoData.Video3DData video3DData = ref video.Video3D.V;
                    ref AudioData audio = ref obj.Audio.V;

                    if (obj.Video.V.Video3D.O > 0)
                    {
                        s.P = video.Video3D.O;
                        W(ref s, ref video3DData.   AnchorZ);
                        W(ref s, ref video3DData. PositionZ);
                        W(ref s, ref video3DData.DirectionX);
                        W(ref s, ref video3DData.DirectionY);
                        W(ref s, ref video3DData.DirectionZ);
                        W(ref s, ref video3DData. RotationX);
                        W(ref s, ref video3DData. RotationY);
                        W(ref s, ref video3DData.    ScaleZ);
                    }

                    if (obj.Video.O > 0)
                    {
                        s.P = obj.Video.O;
                        s.W((byte)video.TransferMode.BlendMode );
                        s.W((byte)video.TransferMode.Flags     );
                        s.W((byte)video.TransferMode.TrackMatte);
                        s.W((byte)0);

                        W(ref s, ref video.  AnchorX);
                        W(ref s, ref video.  AnchorY);
                        W(ref s, ref video.PositionX);
                        W(ref s, ref video.PositionY);
                        W(ref s, ref video.Rotation );
                        W(ref s, ref video.   ScaleX);
                        W(ref s, ref video.   ScaleY);
                        W(ref s, ref video. Opacity );
                        s.W(video.Video3D.O);
                    }

                    if (obj.Audio.O > 0)
                    {
                        s.P = obj.Audio.O;
                        W(ref s, ref audio.VolumeL);
                        W(ref s, ref audio.VolumeR);
                        W(ref s, ref audio.   PanL);
                        W(ref s, ref audio.   PanR);
                    }

                    void W(ref Stream _IO, ref CountPointer<KFT2> keys)
                    {
                        if (keys.C < 1) _IO.W(0L);
                        else { if (keys.C > 0 && keys.O == 0)
                            { keys.O = returnPos + (nvp << 2); nvp++; } _IO.W(keys); }

                    }
                }
            }

            s.P = returnPos;
            for (i = 0; i < nvp; i++)
                s.W(0);
            s.A(0x10, true);

            s.F();

            for (i = 0; i < aet.Composition.C; i++)
                for (i0 = 0; i0 < aet.Composition.E[i].C; i0++)
                {
                    ref Layer obj = ref aet.Composition.E[i].E[i0];

                    s.P = obj.Offset;
                    s.W(obj.Name.O);
                    s.W(obj.StartFrame);
                    s.W(obj.EndFrame);
                    s.W(obj.OffsetFrame);
                    s.W(obj.TimeScale);
                    s.W((ushort)obj.Flags  );
                    s.W((  byte)obj.Quality);
                    s.W((  byte)obj.Type   );
                    s.W(obj.VidItmOff);

                    if (obj.ParentLayer > -1)
                    {
                        bool Found = false;
                        for (i1 = 0; i1 < aet.Composition.C; i1++)
                            for (i2 = 0; i2 < aet.Composition.E[i1].C; i2++)
                                if (aet.Composition[i1].E[i2].ID == obj.ParentLayer)
                                { Found = true; s.W(aet.Composition.E[i1].E[i2].Offset); break; }
                        if (!Found) s.W(0);
                    }
                    else s.W(0);
                    s.W(obj.Marker);
                    s.W(obj.Video.O);
                    s.W(obj.Audio.O);

                    ref CountPointer<Marker> Marker = ref obj.Marker;
                    s.P = Marker.O;
                    for (i1 = 0; i1 < Marker.C; i1++)
                    {
                        s.W(Marker[i1].Frame);
                        s.W(Marker[i1].Name.O);
                    }
                }

            s.F();

            for (i = 0; i < aet.Video.C; i++)
            {
                ref Video region = ref aet.Video.E[i];
                if (region.Identifiers.C == 0) continue;
                s.P = region.Identifiers.O;
                for (i0 = 0; i0 < region.Identifiers.C; i0++)
                {
                    s.W(region.Identifiers[i0].Name.O);
                    s.W(region.Identifiers[i0].ID);
                }
            }

            s.P = aet.Composition.O;
            for (i = 0; i < aet.Composition.C; i++)
            {
                if (aet.Composition[i].C > 0)
                {
                    s.W(aet.Composition[i].C);
                    s.W(aet.Composition[i].E[0].Offset);
                }
                else s.W(0L);
            }

            s.P = aetData.O;
            s.W(aet.Name.O);
            s.W(aet.StartFrame);
            s.W(aet.EndFrame);
            s.W(aet.FrameRate);
            s.W(aet.BackColor);
            s.W(aet.Width);
            s.W(aet.Height);
            s.W(aet.Camera.O);
            s.W(aet.Composition);
            s.W(aet.Video);
            s.W(aet.Audio);
            s.W(0L);
        }

        private void RAL(ref Composition layer)
        {
            for (i0 = 0; i0 < layer.C; i0++, i1++)
            {
                layer.E[i0].ID = i1;
                ref Layer obj = ref layer.E[i0];
                s.P = obj.Offset = layer.O + i0 * 0x30;
                obj.Name = s.RPS();
                obj.StartFrame    = s.RF32();
                obj.  EndFrame    = s.RF32();
                obj.OffsetFrame   = s.RF32();
                obj.TimeScale = s.RF32();
                obj.Flags   = (Layer.AetLayerFlags  )s.RU16();
                obj.Quality = (Layer.AetLayerQuality)s.RU8 ();
                obj.Type    = (Layer.AetLayerType   )s.RU8 ();
                obj.VidItmOff  = s.RI32();
                obj.ParentLayer = s.RI32();
                obj.Marker = s.ReadCountPointer<Marker>();
                obj.Video = s.RP<VideoData>();
                obj.Audio = s.RP<AudioData>();

                if (obj.Video.O > 0)
                {
                    ref VideoData video = ref obj.Video.V;
                    s.P = obj.Video.O;
                    video.TransferMode.BlendMode  = (VideoData.VideoTransferMode.TransferBlendMode )s.RU8();
                    video.TransferMode.Flags      = (VideoData.VideoTransferMode.TransferFlags     )s.RU8();
                    video.TransferMode.TrackMatte = (VideoData.VideoTransferMode.TransferTrackMatte)s.RU8();
                    video.Padding = s.RU8();
                    video.  AnchorX = s.ReadCountPointer<KFT2>();
                    video.  AnchorY = s.ReadCountPointer<KFT2>();
                    video.PositionX = s.ReadCountPointer<KFT2>();
                    video.PositionY = s.ReadCountPointer<KFT2>();
                    video.Rotation  = s.ReadCountPointer<KFT2>();
                    video.   ScaleX = s.ReadCountPointer<KFT2>();
                    video.   ScaleY = s.ReadCountPointer<KFT2>();
                    video.Opacity   = s.ReadCountPointer<KFT2>();
                    video.Video3D = s.RP<VideoData.Video3DData>();

                    RKF(ref video.  AnchorX);
                    RKF(ref video.  AnchorY);
                    RKF(ref video.PositionX);
                    RKF(ref video.PositionY);
                    RKF(ref video.Rotation );
                    RKF(ref video.   ScaleX);
                    RKF(ref video.   ScaleY);
                    RKF(ref video. Opacity );

                    if (video.Video3D.O > 0)
                    {
                        ref VideoData.Video3DData video3DData = ref video.Video3D.V;
                        s.P = video.Video3D.O;
                        video3DData.   AnchorZ = s.ReadCountPointer<KFT2>();
                        video3DData. PositionZ = s.ReadCountPointer<KFT2>();
                        video3DData.DirectionX = s.ReadCountPointer<KFT2>();
                        video3DData.DirectionY = s.ReadCountPointer<KFT2>();
                        video3DData.DirectionZ = s.ReadCountPointer<KFT2>();
                        video3DData. RotationX = s.ReadCountPointer<KFT2>();
                        video3DData. RotationY = s.ReadCountPointer<KFT2>();
                        video3DData.    ScaleZ = s.ReadCountPointer<KFT2>();

                        RKF(ref video3DData.   AnchorZ);
                        RKF(ref video3DData. PositionZ);
                        RKF(ref video3DData.DirectionX);
                        RKF(ref video3DData.DirectionY);
                        RKF(ref video3DData.DirectionZ);
                        RKF(ref video3DData. RotationX);
                        RKF(ref video3DData. RotationY);
                        RKF(ref video3DData.    ScaleZ);
                    }
                }

                if (obj.Audio.O > 0)
                {
                    ref AudioData audio = ref obj.Audio.V;
                    s.P = obj.Audio.O;
                    audio.VolumeL = s.ReadCountPointer<KFT2>();
                    audio.VolumeR = s.ReadCountPointer<KFT2>();
                    audio.   PanL = s.ReadCountPointer<KFT2>();
                    audio.   PanR = s.ReadCountPointer<KFT2>();

                    RKF(ref audio.VolumeL);
                    RKF(ref audio.VolumeR);
                    RKF(ref audio.   PanL);
                    RKF(ref audio.   PanR);
                }

                s.P = obj.Marker.O;
                for (i2 = 0; i2 < obj.Marker.C; i2++)
                    obj.Marker[i2] = new Marker() { Frame = s.RF32(), Name = s.RPSSJIS() };
            }
        }

        private void RKF(ref CountPointer<KFT2> keys)
        {
            if (keys.C  < 1) return;
            s.P = keys.O;
            if (keys.C == 1) { keys.E[0].V = s.RF32(); return; }

            keys.E = new KFT2[keys.C];
            for (uint i = 0; i < keys.C; i++) keys.E[i].F = s.RF32();
            for (uint i = 0; i < keys.C; i++)
            { keys.E[i].V = s.RF32();       keys.E[i].T = s.RF32(); }
        }

        private void W(ref CountPointer<KFT2> keys)
        {
            keys.O = 0;
            if (keys.C < 1) return;
            if (keys.C == 1)
            {
                if (keys.E[0].V != 0) { keys.O = s.P; s.W(keys.E[0].V); }
                return;
            }

            if (keys.C > 2) s.A(0x20);
            keys.O = s.P;

            for (uint i = 0; i < keys.C; i++) s.W(keys.E[i].F);
            for (uint i = 0; i < keys.C; i++)
            { s.W(keys.E[i].V);              s.W(keys.E[i].T); }
        }

        private void WPS(ref System.Collections.Generic.Dictionary<string, int> dict, ref Pointer<string> str)
        {
            if (dict.ContainsKey(str.V)) str.O = dict[str.V];
            else { str.O = s.P; dict.Add(str.V, str.O); s.WPSSJIS(str.V + "\0"); }
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
            aet.WriteAfterAll(false, true, file, json);
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

            if ((temp = msgPack["Audio", true]).NotNull)
            {
                aet.Audio.C = temp.Array.Length;
                for (i = 0; i < aet.Audio.C; i++)
                    aet.Audio.E[i] = new Audio { SoundID = temp[i].RU32("SoundID") };
            }

            if ((temp = msgPack["Video", true]).NotNull)
            {
                aet.Video.C = temp.Array.Length;
                for (i = 0; i < aet.Video.C; i++)
                {
                    ref Video region = ref aet.Video.E[i];
                    region = default;
                    region.Color  = temp[i].RU32("Color" );
                    region.Width  = temp[i].RU16("Width" );
                    region.Height = temp[i].RU16("Height");
                    region.Frames = temp[i].RF32("Frames");

                    MsgPack identifiers = temp[i]["Identifiers", true];
                    if (identifiers.NotNull)
                    {
                        region.Identifiers.C = identifiers.Array.Length;
                        for (i0 = 0; i0 < region.Identifiers.C; i0++)
                            region.Identifiers[i0] = new Video.Identifier() { Name = new Pointer<string>()
                            { V = identifiers[i0].RS("Name") }, ID = identifiers[i0].RU32("ID") };
                    }
                }
            }

            if ((temp = msgPack["Composition", true]).NotNull)
            {
                aet.Composition.C = temp.Array.Length + 1;
                for (i = 0; i < aet.Composition.C - 1; i++)
                {
                    ref Composition l = ref aet.Composition.E[i];
                    l = default;
                    MsgPack layer = temp[i]["Layer", true];
                    if (layer.NotNull)
                    {
                        l.C = layer.Array.Length;
                        for (i0 = 0; i0 < l.C; i0++)
                            RL(ref l.E[i0], layer[i0]);
                    }
                }
            }
            else aet.Composition.C = 1;

            if ((temp = msgPack["Root", true]).NotNull)
            {
                i = aet.Composition.C - 1;
                aet.Composition.E[i].C = temp.Array.Length;
                for (i0 = 0; i0 < aet.Composition[i].C; i0++)
                    RL(ref aet.Composition.E[i].E[i0], temp[i0]);
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


            i = aet.Composition.C - 1;
            MsgPack rootLayer = new MsgPack(aet.Composition[i].C, "Root");
            for (i0 = 0; i0 < aet.Composition[i].C; i0++)
                rootLayer[i0] = WL(ref aet.Composition[i].E[i0]);
            msgPack.Add(rootLayer);

            if (aet.Composition[i].C > 1)
            {
                MsgPack compositions = new MsgPack(aet.Composition.C - 1, "Composition");
                for (i = 0; i < aet.Composition.C - 1; i++)
                {
                    ref Composition composition = ref aet.Composition.E[i];
                    MsgPack compos = MsgPack.New.Add("ID", i);
                    if (composition.C > 0)
                    {
                        MsgPack objects = new MsgPack(composition.C, "Layer");
                        for (i0 = 0; i0 < composition.C; i0++)
                            objects[i0] = WL(ref composition.E[i0]);
                        compos.Add(objects);
                    }
                    compositions[i] = compos;
                }
                msgPack.Add(compositions);
            }

            MsgPack video = new MsgPack(aet.Video.C, "Video");
            for (i = 0; i < aet.Video.C; i++)
            {
                ref Video v = ref aet.Video.E[i];
                MsgPack entry = MsgPack.New.Add("ID", i).Add("Width", v.Width)
                    .Add("Height", v.Height).Add("Color", v.Color);

                if (v.Identifiers.C > 0)
                {
                    entry.Add("Frames", v.Frames);
                    MsgPack identifiers = new MsgPack(v.Identifiers.C, "Identifiers");
                    for (i0 = 0; i0 < v.Identifiers.C; i0++)
                        identifiers[i0] = MsgPack.New.Add("Name", v.Identifiers[i0].Name.V)
                                                     .Add("ID"  , v.Identifiers[i0].ID);
                    entry.Add(identifiers);
                }
                video[i] = entry;
            }
            msgPack.Add(video);

            MsgPack audio = new MsgPack(aet.Audio.C, "Audio");
            for (i = 0; i < aet.Audio.C; i++)
                audio[i] = MsgPack.New.Add("ID", i).Add("SoundID", aet.Audio[i].SoundID);
            msgPack.Add(audio);

            return msgPack;
        }

        private Layer RL(ref Layer layer, MsgPack msg)
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
            if ((temp = msg["Marker", true]).NotNull)
            {
                layer.Marker.C = temp.Array.Length;
                for (i = 0; i < layer.Marker.C; i++)
                {
                    layer.Marker.E[i].Frame   = temp[i].RF32("Frame");
                    layer.Marker.E[i]. Name.V = temp[i].RS  ( "Name");
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

            if ((temp = msg["AudioData"]).NotNull)
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

        private MsgPack WL(ref Layer layer, string name = null)
        {
            int i;
            MsgPack layerData = new MsgPack(name).Add("ID", layer.ID).Add("Name", layer.Name.V)
                .Add("StartFrame", layer.StartFrame).Add("EndFrame", layer.EndFrame)
                .Add("OffsetFrame", layer.OffsetFrame).Add("TimeScale", layer.TimeScale)
                .Add("Flags", (ushort)layer.Flags).Add("Quality", layer.Quality.ToString())
                .Add("Type", layer.Type.ToString());

            if (layer.DataID      > -1) layerData.Add("DataID"     , layer.DataID     );
            if (layer.ParentLayer > -1) layerData.Add("ParentLayer", layer.ParentLayer);

            if (layer.Marker.C > 0)
            {
                MsgPack Marker = new MsgPack(layer.Marker.C, "Marker");
                for (i = 0; i < layer.Marker.C; i++)
                    Marker[i] = MsgPack.New.Add("Frame", layer.Marker[i].Frame  )
                                            .Add( "Name", layer.Marker[i]. Name.V);
                layerData.Add(Marker);
            }

            if (layer.Video.O > 0)
            {
                ref VideoData video = ref layer.Video.V;
                MsgPack VideoData = new MsgPack("VideoData")
                    .Add("BlendMode" , video.TransferMode.BlendMode .ToString())
                    .Add("Flags"     , (byte)video.TransferMode.Flags)
                    .Add("TrackMatte", video.TransferMode.TrackMatte.ToString());

                VideoData.Add(WMP(ref video.AnchorX  ,   "AnchorX"));
                VideoData.Add(WMP(ref video.AnchorY  ,   "AnchorY"));
                VideoData.Add(WMP(ref video.PositionX, "PositionX"));
                VideoData.Add(WMP(ref video.PositionY, "PositionY"));
                VideoData.Add(WMP(ref video.Rotation , "Rotation" ));
                VideoData.Add(WMP(ref video.   ScaleX,    "ScaleX"));
                VideoData.Add(WMP(ref video.   ScaleY,    "ScaleY"));
                VideoData.Add(WMP(ref video. Opacity ,  "Opacity" ));
                if (video.Video3D.O > 0)
                {
                    ref VideoData.Video3DData video3D = ref video.Video3D.V;
                    MsgPack video3DData = new MsgPack("Video3DData");
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
        { if (!disposed) { if (s != null) s.D(); s = null; AET = default; disposed = true; } }
    }
}
