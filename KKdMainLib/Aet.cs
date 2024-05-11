//Original: AetSet.bt Version: 5.0 by samyuu

using KKdBaseLib;
using KKdBaseLib.Auth2D;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct Aet : System.IDisposable
    {
        private Stream s;
        private const string x00 = "\0";

        public Header AET;

        public void AETReader(string file)
        {
            AET = default;
            s = File.OpenReader(file + ".bin", true);

            int Pos = -1;
            int count = 0;
            while (true) { Pos = s.RI32(); if (Pos != 0 && Pos < s.L) count++; else break; }

            s.P = 0;
            AET.Scenes = new Pointer<Scene>[count];
            for (int i = 0; i < AET.Scenes.Length; i++) AET.Scenes[i] = s.RP<Scene>();
            for (int i = 0; i < AET.Scenes.Length; i++)
            {
                ref Pointer<Scene> aetData = ref AET.Scenes[i];

                s.P = aetData.O;
                ref Scene aet = ref aetData.V;

                aet.Name = s.RPS();
                aet.StartFrame  = s.RF32();
                aet.EndFrame    = s.RF32();
                aet.FrameRate   = s.RF32();
                aet.BackColor   = s.RU32();
                aet.Width       = s.RU32();
                aet.Height      = s.RU32();
                aet.Camera      = s.RP<Camera>();
                aet.Composition = s.RCP<Composition>();
                aet.Video       = s.RCP<Video      >();
                aet.Audio       = s.RCP<Audio      >();

                if (aet.Camera.O > 0)
                {
                    s.P = aet.Camera.O;
                    ref Camera camera = ref aet.Camera.V;
                    camera.      EyeX.O = s.RI32();
                    camera.      EyeY.O = s.RI32();
                    camera.      EyeZ.O = s.RI32();
                    camera. PositionX.O = s.RI32();
                    camera. PositionY.O = s.RI32();
                    camera. PositionZ.O = s.RI32();
                    camera.DirectionX.O = s.RI32();
                    camera.DirectionY.O = s.RI32();
                    camera.DirectionZ.O = s.RI32();
                    camera. RotationX.O = s.RI32();
                    camera. RotationY.O = s.RI32();
                    camera. RotationZ.O = s.RI32();
                    camera.     Zoom .O = s.RI32();
                    RKF(ref camera.      EyeX);
                    RKF(ref camera.      EyeY);
                    RKF(ref camera.      EyeZ);
                    RKF(ref camera. PositionX);
                    RKF(ref camera. PositionY);
                    RKF(ref camera. PositionZ);
                    RKF(ref camera.DirectionX);
                    RKF(ref camera.DirectionY);
                    RKF(ref camera.DirectionZ);
                    RKF(ref camera. RotationX);
                    RKF(ref camera. RotationY);
                    RKF(ref camera. RotationZ);
                    RKF(ref camera.     Zoom );
                }

                if (aet.Audio.O > 0)
                {
                    s.P = aet.Audio.O;
                    for (int j = 0; j < aet.Audio.C; j++)
                    {
                        ref Audio aif = ref aet.Audio.E[j];
                        aif.O = s.P;
                        aif.SoundID = s.RU32();
                    }
                }

                s.P = aet.Composition.O;
                for (int j = 0; j < aet.Composition.C; j++)
                    aet.Composition[j] = new Composition() { P = s.P, C = s.RI32(), O = s.RI32() };

                int id = 0;
                RAL(ref aet.Composition.E[aet.Composition.C - 1], ref id);
                for (int j = 0; j < aet.Composition.C - 1; j++)
                    RAL(ref aet.Composition.E[j], ref id);

                s.P = aet.Video.O;
                for (int j = 0; j < aet.Video.C; j++)
                    aet.Video[j] = new Video { O = s.P, Color = s.RU32(), Width = s.RU16(),
                        Height = s.RU16(), Frames = s.RF32(), Identifiers = s.RCP<Video.Identifier>() };

                for (int j = 0; j < aet.Video.C; j++)
                {
                    s.P = aet.Video[j].Identifiers.O;
                    for (int k = 0; k < aet.Video[j].Identifiers.C; k++)
                        aet.Video.E[j].Identifiers[k] = new Video.Identifier { Name = s.RPS(), ID = s.RU32() };
                }

                for (int j = 0; j < aet.Composition.C; j++)
                {
                    for (int k = 0; k < aet.Composition[j].C; k++)
                    {
                        ref Layer obj = ref aet.Composition[j].E[k];
                        obj.DataID = -1;
                        int dataOffset = obj.VidItmOff;
                        if (dataOffset > 0)
                                 if (obj.Type == Layer.AetLayerType.Video      )
                                for (int l = 0; l < aet.Video      .C; l++)
                                { if (aet.Video      [l].O == dataOffset) { obj.DataID = l; break; } }
                            else if (obj.Type == Layer.AetLayerType.Audio      )
                                for (int l = 0; l < aet.Audio      .C; l++)
                                { if (aet.Audio      [l].O == dataOffset) { obj.DataID = l; break; } }
                            else if (obj.Type == Layer.AetLayerType.Composition)
                                for (int l = 0; l < aet.Composition.C; l++)
                                { if (aet.Composition[l].P == dataOffset) { obj.DataID = l; break; } }

                        int parentLayer = obj.ParentLayer;
                        if (parentLayer > 0)
                            for (int l = 0; l < aet.Composition.C; l++)
                            {
                                for (int m = 0; m < aet.Composition[l].C; m++)
                                    if (aet.Composition[l].E[m].Offset == parentLayer)
                                    { obj.ParentLayer = aet.Composition[l].E[m].ID; break; }
                                if (parentLayer != obj.ParentLayer) break;
                            }
                        if (parentLayer == obj.ParentLayer) obj.ParentLayer = -1;
                    }
                }
            }

            s.C();
        }

        public void AETWriter(string file)
        {
            if (AET.Scenes == null || AET.Scenes.Length == 0) return;
            s = File.OpenWriter();

            s.P = (AET.Scenes.Length * 0x04).A(0x20);

            System.Collections.Generic.Dictionary<string, int> usedValues =
                new System.Collections.Generic.Dictionary<string, int>();

            for (int i = 0; i < AET.Scenes.Length; i++)
            {
                ref Pointer<Scene> aetData = ref AET.Scenes[i];

                ref Scene aet = ref aetData.V;

                for (int j = 0; j < aet.Video.C; j++)
                {
                    ref Video region = ref aet.Video.E[j];
                    if (region.Identifiers.C == 0) { region.Identifiers.O = 0; continue; }
                    if (region.Identifiers.C > 1) s.A(0x20);
                    region.Identifiers.O = s.P;
                    for (int k = 0; k < region.Identifiers.C; k++) s.W(0L);
                }

                if (aet.Video.C > 1) s.A(0x20);
                aet.Video.O = s.P;
                for (int j = 0; j < aet.Video.C; j++)
                {
                    ref Video region = ref aet.Video.E[j];
                    region.O = s.P;
                    s.W(region.Color  );
                    s.W(region.Width  );
                    s.W(region.Height );
                    s.W(region.Frames );
                    s.W(region.Identifiers);
                }

                if (aet.Composition.C > 1) s.A(0x20);
                aet.Composition.O = s.P;
                for (int j = 0; j < aet.Composition.C; j++)
                {
                    aet.Composition.E[j].O = s.P;
                    s.W(0L);
                }

                for (int j = 0; j < aet.Composition.C; j++)
                {
                    if (aet.Composition.E[j].C < 1) continue;

                    for (int k = 0; k < aet.Composition.E[j].C; k++)
                    {
                        ref Layer obj = ref aet.Composition.E[j].E[k];
                        if (obj.Marker.C > 0)
                        {
                            if (obj.Marker.C > 3) s.A(0x20);
                            obj.Marker.O = s.P;
                            for (int l = 0; l < obj.Marker.C; l++)
                                s.W(0L);
                        }

                        if (obj.Video.O > 0)
                        {
                            ref VideoData video = ref obj.Video.V;
                            if (video.Video3D.O > 0)
                            {
                                ref VideoData.Video3DData video3DData = ref video.Video3D.V;
                                W(ref video3DData.   AnchorZ);
                                W(ref video3DData. PositionZ);
                                W(ref video3DData.DirectionX);
                                W(ref video3DData.DirectionY);
                                W(ref video3DData.DirectionZ);
                                W(ref video3DData. RotationX);
                                W(ref video3DData. RotationY);
                                W(ref video3DData.    ScaleZ);
                            }

                            W(ref video.  AnchorX);
                            W(ref video.  AnchorY);
                            W(ref video.PositionX);
                            W(ref video.PositionY);
                            W(ref video.Rotation );
                            W(ref video.   ScaleX);
                            W(ref video.   ScaleY);
                            W(ref video. Opacity );

                            if (video.Video3D.O > 0)
                            {
                                s.A(0x20);
                                video.Video3D.O = s.P;
                                s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                                s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                            }
                        }

                        if (obj.Audio.O > 0)
                        {
                            ref AudioData audio = ref obj.Audio.V;
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
                    aet.Composition.E[j].E[0].Offset = s.P;
                    for (int k = 0; k < aet.Composition.E[j].C; k++)
                    {
                        ref Layer obj = ref aet.Composition.E[j].E[k];
                        obj.Offset = s.P;
                        s.W(0L); s.W(0L); s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                    }
                }

                if (aet.Camera.O > 0)
                {
                    ref Camera camera = ref aet.Camera.V;
                    s.A(0x10);
                    W(ref camera.      EyeX);
                    W(ref camera.      EyeY);
                    W(ref camera.      EyeZ);
                    W(ref camera. PositionX);
                    W(ref camera. PositionY);
                    W(ref camera. PositionZ);
                    W(ref camera.DirectionX);
                    W(ref camera.DirectionY);
                    W(ref camera.DirectionZ);
                    W(ref camera. RotationX);
                    W(ref camera. RotationY);
                    W(ref camera. RotationZ);
                    W(ref camera.     Zoom );

                    s.A(0x20);
                    aet.Camera.O = s.P;
                    s.W(0L); s.W(0L); s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                    s.W(0L); s.W(0L); s.W(0L); s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                }

                s.A(0x20);
                aetData.O = s.P;
                s.W(0L); s.W(0L); s.W(0L); s.W(0L);
                s.W(0L); s.W(0L); s.W(0L); s.W(0L);

                s.A(0x10);
            }

            for (int i = 0; i < AET.Scenes.Length; i++)
            {
                ref Pointer<Scene> aetData = ref AET.Scenes[i];

                ref Scene aet = ref aetData.V;

                for (int j = 0; j < aet.Video.C; j++)
                {
                    ref Video region = ref aet.Video.E[j];
                    for (int k = 0; k < region.Identifiers.C; k++)
                        WPS(ref usedValues, ref region.Identifiers.E[k].Name);
                }

                //_IO.Align(0x4);
                for (int j = 0; j < aet.Composition.C; j++)
                {
                    for (int k = 0; k < aet.Composition.E[j].C; k++)
                    {
                        ref Layer obj = ref aet.Composition.E[j].E[k];
                        for (int l = 0; l < obj.Marker.C; l++)
                            WPS(ref usedValues, ref obj.Marker.E[l].Name);
                    }

                    for (int k = 0; k < aet.Composition.E[j].C; k++)
                        WPS(ref usedValues, ref aet.Composition.E[j].E[k].Name);
                }

                //_IO.Align(0x4);
                aet.Name.O = s.P;
                s.W(aet.Name.V + x00);
                s.A(0x4);
            }

            s.F();

            for (int i = 0; i < AET.Scenes.Length; i++)
            {
                ref Scene aet = ref AET.Scenes[i].V;

                if (aet.Audio.C > 0)
                {
                    aet.Audio.O = s.P;
                    for (int j = 0; j < aet.Audio.C; j++)
                    {
                        aet.Audio.E[j].O = s.P;
                        s.W(aet.Audio[j].SoundID);
                    }
                }
            }

            int nullDataPos = s.P;
            int nullDataCount = 0;
            for (int i = 0; i < AET.Scenes.Length; i++)
            {
                ref Scene aet = ref AET.Scenes[i].V;

                for (int j = 0; j < aet.Composition.C; j++)
                {
                    if (aet.Composition.E[j].C < 1) continue;

                    for (int k = 0; k < aet.Composition.E[j].C; k++)
                    {
                        ref Layer obj = ref aet.Composition.E[j].E[k];

                        if (obj.Video.O > 0)
                        {
                            ref VideoData video = ref obj.Video.V;
                            if (video.Video3D.O > 0)
                            {
                                ref VideoData.Video3DData video3DData = ref video.Video3D.V;
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
                            ref AudioData audio = ref obj.Audio.V;
                            s.P = obj.Audio.O;
                            W(ref s, ref audio.VolumeL);
                            W(ref s, ref audio.VolumeR);
                            W(ref s, ref audio.   PanL);
                            W(ref s, ref audio.   PanR);
                        }
                    }
                }
                
                if (aet.Camera.O > 0)
                {
                    ref Camera camera = ref aet.Camera.V;
                    s.P = aet.Camera.O;
                    W(ref s, ref camera.      EyeX);
                    W(ref s, ref camera.      EyeY);
                    W(ref s, ref camera.      EyeZ);
                    W(ref s, ref camera. PositionX);
                    W(ref s, ref camera. PositionY);
                    W(ref s, ref camera. PositionZ);
                    W(ref s, ref camera.DirectionX);
                    W(ref s, ref camera.DirectionY);
                    W(ref s, ref camera.DirectionZ);
                    W(ref s, ref camera. RotationX);
                    W(ref s, ref camera. RotationY);
                    W(ref s, ref camera. RotationZ);
                    W(ref s, ref camera.     Zoom );
                }

                void W(ref Stream _IO, ref CountPointer<KFT2> keys)
                {
                    if (keys.C < 1) _IO.W(0L);
                    else {
                        if (keys.C > 0 && keys.O == 0)
                        { keys.O = nullDataPos + (nullDataCount << 2); nullDataCount++; }
                        _IO.W(keys);
                    }
                }
            }

            s.P = nullDataPos;
            for (int j = 0; j < nullDataCount; j++)
                s.W(0);
            s.A(0x10, true);
            s.F();

            for (int i = 0; i < AET.Scenes.Length; i++)
            {
                ref Pointer<Scene> aetData = ref AET.Scenes[i];

                ref Scene aet = ref aetData.V;

                for (int j = 0; j < aet.Composition.C; j++)
                    for (int k = 0; k < aet.Composition.E[j].C; k++)
                    {
                        ref Layer obj = ref aet.Composition.E[j].E[k];
                             if (obj.Type == Layer.AetLayerType.Video      ) obj.VidItmOff = aet.Video      [obj.DataID].O;
                        else if (obj.Type == Layer.AetLayerType.Audio      ) obj.VidItmOff = aet.Audio      [obj.DataID].O;
                        else if (obj.Type == Layer.AetLayerType.Composition) obj.VidItmOff = aet.Composition[obj.DataID].O;
                        else obj.VidItmOff = 0;
                    }

                for (int j = 0; j < aet.Composition.C; j++)
                    for (int k = 0; k < aet.Composition.E[j].C; k++)
                    {
                        ref Layer obj = ref aet.Composition.E[j].E[k];

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
                            for (int l = 0; l < aet.Composition.C; l++)
                                for (int m = 0; m < aet.Composition.E[l].C; m++)
                                    if (aet.Composition[l].E[m].ID == obj.ParentLayer)
                                    { Found = true; s.W(aet.Composition.E[l].E[m].Offset); break; }
                            if (!Found) s.W(0);
                        }
                        else s.W(0);
                        s.W(obj.Marker);
                        s.W(obj.Video.O);
                        s.W(obj.Audio.O);

                        ref CountPointer<Marker> Marker = ref obj.Marker;
                        s.P = Marker.O;
                        for (int l = 0; l < Marker.C; l++)
                        {
                            s.W(Marker[l].Frame);
                            s.W(Marker[l].Name.O);
                        }
                    }

                for (int j = 0; j < aet.Video.C; j++)
                {
                    ref Video region = ref aet.Video.E[j];
                    if (region.Identifiers.C == 0) continue;
                    s.P = region.Identifiers.O;
                    for (int k = 0; k < region.Identifiers.C; k++)
                    {
                        s.W(region.Identifiers[k].Name.O);
                        s.W(region.Identifiers[k].ID);
                    }
                }

                s.P = aet.Composition.O;
                for (int j = 0; j < aet.Composition.C; j++)
                {
                    if (aet.Composition[j].C > 0)
                    {
                        s.W(aet.Composition[j].C);
                        s.W(aet.Composition[j].E[0].Offset);
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

            s.P = 0;
            for (int i = 0; i < AET.Scenes.Length; i++)
                if (AET.Scenes[i].O > 0) s.W(AET.Scenes[i].O);
            byte[] data = s.ToArray();
            s.Dispose();

            using (s = File.OpenWriter(file + ".bin", true))
                s.W(data);
            data = null;
        }

        private void RAL(ref Composition layer, ref int id)
        {
            for (int i = 0; i < layer.C; i++, id++)
            {
                layer.E[i].ID = id;
                ref Layer obj = ref layer.E[i];
                s.P = obj.Offset = layer.O + i * 0x30;
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
                obj.Marker = s.RCP<Marker>();
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
                    video.  AnchorX = s.RCP<KFT2>();
                    video.  AnchorY = s.RCP<KFT2>();
                    video.PositionX = s.RCP<KFT2>();
                    video.PositionY = s.RCP<KFT2>();
                    video.Rotation  = s.RCP<KFT2>();
                    video.   ScaleX = s.RCP<KFT2>();
                    video.   ScaleY = s.RCP<KFT2>();
                    video.Opacity   = s.RCP<KFT2>();
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
                        video3DData.   AnchorZ = s.RCP<KFT2>();
                        video3DData. PositionZ = s.RCP<KFT2>();
                        video3DData.DirectionX = s.RCP<KFT2>();
                        video3DData.DirectionY = s.RCP<KFT2>();
                        video3DData.DirectionZ = s.RCP<KFT2>();
                        video3DData. RotationX = s.RCP<KFT2>();
                        video3DData. RotationY = s.RCP<KFT2>();
                        video3DData.    ScaleZ = s.RCP<KFT2>();

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
                    audio.VolumeL = s.RCP<KFT2>();
                    audio.VolumeR = s.RCP<KFT2>();
                    audio.   PanL = s.RCP<KFT2>();
                    audio.   PanR = s.RCP<KFT2>();

                    RKF(ref audio.VolumeL);
                    RKF(ref audio.VolumeR);
                    RKF(ref audio.   PanL);
                    RKF(ref audio.   PanR);
                }

                s.P = obj.Marker.O;
                for (int j = 0; j < obj.Marker.C; j++)
                    obj.Marker[j] = new Marker() { Frame = s.RF32(), Name = s.RPSSJIS() };
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

                ref Camera camera = ref aet.Camera.V;
                camera.      EyeX = RKF(temp,       "EyeX");
                camera.      EyeY = RKF(temp,       "EyeY");
                camera.      EyeZ = RKF(temp,       "EyeZ");
                camera. PositionX = RKF(temp,  "PositionX");
                camera. PositionY = RKF(temp,  "PositionY");
                camera. PositionZ = RKF(temp,  "PositionZ");
                camera.DirectionX = RKF(temp, "DirectionX");
                camera.DirectionY = RKF(temp, "DirectionY");
                camera.DirectionZ = RKF(temp, "DirectionZ");
                camera. RotationX = RKF(temp,  "RotationX");
                camera. RotationY = RKF(temp,  "RotationY");
                camera. RotationZ = RKF(temp,  "RotationZ");
                camera.     Zoom  = RKF(temp,      "Zoom" );
            }

            if ((temp = msgPack["Audio", true]).NotNull)
            {
                aet.Audio.C = temp.Array.Length;
                for (int i = 0; i < aet.Audio.C; i++)
                    aet.Audio.E[i] = new Audio { SoundID = temp[i].RU32("SoundID") };
            }

            if ((temp = msgPack["Video", true]).NotNull)
            {
                aet.Video.C = temp.Array.Length;
                for (int i = 0; i < aet.Video.C; i++)
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
                        for (int j = 0; j < region.Identifiers.C; j++)
                            region.Identifiers[j] = new Video.Identifier() { Name = new Pointer<string>()
                            { V = identifiers[j].RS("Name") }, ID = identifiers[j].RU32("ID") };
                    }
                }
            }

            if ((temp = msgPack["Composition", true]).NotNull)
            {
                aet.Composition.C = temp.Array.Length + 1;
                for (int i = 0; i < aet.Composition.C - 1; i++)
                {
                    ref Composition l = ref aet.Composition.E[i];
                    l = default;
                    MsgPack layer = temp[i]["Layer", true];
                    if (layer.NotNull)
                    {
                        l.C = layer.Array.Length;
                        for (int j = 0; j < l.C; j++)
                            RL(ref l.E[j], layer[j]);
                    }
                }
            }
            else aet.Composition.C = 1;

            if ((temp = msgPack["Root", true]).NotNull)
            {
                ref Composition rootComp = ref aet.Composition.E[aet.Composition.C - 1];
                rootComp.C = temp.Array.Length;
                for (int j = 0; j < rootComp.C; j++)
                    RL(ref rootComp.E[j], temp[j]);
            }
            temp.Dispose();
        }

        public MsgPack MsgPackWriter(ref Pointer<Scene> aetData)
        {
            if (aetData.O <= 0) return default;

            ref Scene aet = ref aetData.V;

            MsgPack msgPack = MsgPack.New.Add("Name", aet.Name.V).Add("StartFrame", aet.StartFrame)
                .Add("EndFrame", aet.EndFrame).Add("FrameRate", aet.FrameRate)
                .Add("BackColor", aet.BackColor).Add("Width", aet.Width).Add("Height", aet.Height);


            if (aet.Camera.O > 0)
            {
                ref Camera camera = ref aet.Camera.V;
                MsgPack cameraData = new MsgPack("Camera");

                cameraData.Add(WMP(ref camera.      EyeX,       "EyeX"));
                cameraData.Add(WMP(ref camera.      EyeY,       "EyeY"));
                cameraData.Add(WMP(ref camera.      EyeZ,       "EyeZ"));
                cameraData.Add(WMP(ref camera. PositionX,  "PositionX"));
                cameraData.Add(WMP(ref camera. PositionY,  "PositionY"));
                cameraData.Add(WMP(ref camera. PositionZ,  "PositionZ"));
                cameraData.Add(WMP(ref camera.DirectionX, "DirectionX"));
                cameraData.Add(WMP(ref camera.DirectionY, "DirectionY"));
                cameraData.Add(WMP(ref camera.DirectionZ, "DirectionZ"));
                cameraData.Add(WMP(ref camera. RotationX,  "RotationX"));
                cameraData.Add(WMP(ref camera. RotationY,  "RotationY"));
                cameraData.Add(WMP(ref camera. RotationZ,  "RotationZ"));
                cameraData.Add(WMP(ref camera.     Zoom ,      "Zoom" ));
            }


            ref Composition rootComp = ref aet.Composition.E[aet.Composition.C - 1];
            MsgPack rootLayer = new MsgPack(rootComp.C, "Root");
            for (int j = 0; j < rootComp.C; j++)
                rootLayer[j] = WL(ref rootComp.E[j]);
            msgPack.Add(rootLayer);

            if (aet.Composition.C > 1)
            {
                MsgPack compositions = new MsgPack(aet.Composition.C - 1, "Composition");
                for (int i = 0; i < aet.Composition.C - 1; i++)
                {
                    ref Composition composition = ref aet.Composition.E[i];
                    MsgPack compos = MsgPack.New.Add("ID", i);
                    if (composition.C > 0)
                    {
                        MsgPack objects = new MsgPack(composition.C, "Layer");
                        for (int j = 0; j < composition.C; j++)
                            objects[j] = WL(ref composition.E[j]);
                        compos.Add(objects);
                    }
                    compositions[i] = compos;
                }
                msgPack.Add(compositions);
            }

            MsgPack video = new MsgPack(aet.Video.C, "Video");
            for (int i = 0; i < aet.Video.C; i++)
            {
                ref Video v = ref aet.Video.E[i];
                MsgPack entry = MsgPack.New.Add("ID", i).Add("Width", v.Width)
                    .Add("Height", v.Height).Add("Color", v.Color);

                if (v.Identifiers.C > 0)
                {
                    entry.Add("Frames", v.Frames);
                    MsgPack identifiers = new MsgPack(v.Identifiers.C, "Identifiers");
                    for (int j = 0; j < v.Identifiers.C; j++)
                        identifiers[j] = MsgPack.New.Add("Name", v.Identifiers[j].Name.V)
                                                     .Add("ID"  , v.Identifiers[j].ID);
                    entry.Add(identifiers);
                }
                video[i] = entry;
            }
            msgPack.Add(video);

            MsgPack audio = new MsgPack(aet.Audio.C, "Audio");
            for (int i = 0; i < aet.Audio.C; i++)
                audio[i] = MsgPack.New.Add("ID", i).Add("SoundID", aet.Audio[i].SoundID);
            msgPack.Add(audio);

            return msgPack;
        }

        private Layer RL(ref Layer layer, MsgPack msg)
        {
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
                for (int i = 0; i < layer.Marker.C; i++)
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
            if (kfe.C <  1) return default;

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
