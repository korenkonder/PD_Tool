using System;
using KKdBaseLib;
using KKdBaseLib.F2;
using KKdBaseLib.Auth3D;
using KKdMainLib.IO;
using Object = KKdBaseLib.Auth3D.Object;
using A3DADict = System.Collections.Generic.Dictionary<string, object>;
using UsedDict = System.Collections.Generic.Dictionary<   int, KKdBaseLib.KeyValuePair<int, float>>;

namespace KKdMainLib
{
    public struct A3DA : IDisposable
    {
        public Data Data;
        public A3DAHeader Head;

        private Stream s;
        private bool a3dc;
        private bool a3dcOpt;
        private int i, i0, i1;
        private int soi;
        private int soi0;
        private int soi1;
        private int[] so;
        private int[] so0;
        private int[] so1;
        private string name;
        private string nameView;
        private string value;
        private string[] dataArray;
        private A3DADict dict;
        private UsedDict usedValues;

        private const string d = ".";
        private const string BO = "bin_offset";
        private const string MTBO = "model_transform" + d + BO;

        public A3DA(bool a3dcOpt = true)
        { a3dc = false; i = i0 = i1 = soi = soi0 = soi1 = 0; value = null;
            name = null; nameView = null; so = null; so0 = null; so1 = null;
            dataArray = null; s = null; Head = default; Data = default;
            dict = null; usedValues = null; this.a3dcOpt = a3dcOpt; }

        public int A3DAReader(string file)
        { using (s = File.OpenReader(file + ".a3da"))
              return A3DAReader(ref s); }

        public int A3DAReader(byte[] data)
        { using (s = File.OpenReader(data))
              return A3DAReader(ref s); }

        public byte[] A3DAWriter() => A3DAWriter(false);

        private int A3DAReader(ref Stream s)
        {
            name = "";
            nameView = "";
            dataArray = new string[4];
            dict = new A3DADict();
            Data = new Data();
            Head = new A3DAHeader();
            Header header = new Header();

            Head.Format = s.Format = Format.F;
            header.SectionSignature = s.RU32();
            if (header.SectionSignature == 0x41443341)
            { header = s.ReadHeader(true, true); Head.Format = header.Format; }
            if (header.SectionSignature != 0x44334123) return 0;

            s.O = s.P - 4;
            header.SectionSignature = s.RU32();

            if (header.SectionSignature == 0x5F5F5F41)
            {
                s.P = 0x10;
                header.Format = s.Format = Format.DT;
            }
            else if (header.SectionSignature == 0x5F5F5F43)
            {
                s.P = 0x10;
                s.RI32();
                s.RI32();
                Head.HeaderOffset = s.RI32E(true);

                s.P = Head.HeaderOffset;
                if (s.RI32() != 0x50) return 0;
                Head.StringOffset = s.RI32E(true);
                Head.StringLength = s.RI32E(true);
                Head.Count = s.RI32E(true);
                if (s.RI32() != 0x4C42) return 0;
                Head.BinaryOffset = s.RI32E(true);
                Head.BinaryLength = s.RI32E(true);

                s.P = Head.StringOffset;
            }
            else return 0;

            if (header.Format == Format.DT)
                Head.StringLength = s.L - 0x10;

            string[] strData = s.RS(Head.StringLength).Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            for (i = 0; i < strData.Length; i++)
                dict.GD(strData[i]);
            strData = null;

            A3DAReader();

            if (header.SectionSignature == 0x5F5F5F43)
            {
                s.P = s.O + Head.BinaryOffset;
                s.O = s.P;
                s.P = 0;
                byte[] data = s.RBy(Head.BinaryLength);
                s.C();
                s = File.OpenReader(data);
                A3DCReader();
            }

            name = "";
            nameView = "";
            dataArray = null;
            dict.Clear();
            dict = null;
            return 1;
        }

        private void A3DAReader()
        {
            if (dict.SW("_"))
            {
                Data._ = new _();
                if (dict.FV(out int compressF16, "_.compress_f16"))
                    Data._.CompressF16 = (CompressF16)compressF16;
                dict.FV(out Data._.ConverterVersion, "_.converter.version");
                dict.FV(out Data._.FileName        , "_.file_name"        );
                dict.FV(out Data._.PropertyVersion , "_.property.version" );
            }

            if (dict.SW("camera_auxiliary"))
            {
                name = "camera_auxiliary" + d;

                Data.CameraAuxiliary = new CameraAuxiliary
                {
                    AutoExposure = RKN(name + "auto_exposure" + d),
                        Exposure = RKN(name +      "exposure" + d),
                    Gamma        = RKN(name + "gamma"         + d),
                    GammaRate    = RKN(name + "gamma_rate"    + d),
                    Saturate     = RKN(name + "saturate"      + d),
                };
                if (Data.CameraAuxiliary.Value.GammaRate.HasValue)
                    if (Head.Format < Format.F || Head.Format == Format.AFT || Head.Format >= Format.FT)
                        Head.Format = Format.F;
            }

            if (dict.SW("play_control"))
            {
                name = "play_control" + d;

                Data.PlayControl = new PlayControl();
                dict.FV(out Data.PlayControl.Begin , name + "begin" );
                dict.FV(out Data.PlayControl.Div   , name + "div"   );
                dict.FV(out Data.PlayControl.FPS   , name + "fps"   );
                dict.FV(out Data.PlayControl.Offset, name + "offset");
                dict.FV(out Data.PlayControl.Size  , name + "size"  );
            }

            if (dict.SW("post_process"))
            {
                name = "post_process" + d;

                Data.PostProcess = new PostProcess()
                {
                    Ambient   = RRGBAK(name + "Ambient"    + d),
                    Diffuse   = RRGBAK(name + "Diffuse"    + d),
                    Specular  = RRGBAK(name + "Specular"   + d),
                    LensFlare = RKN   (name + "lens_flare" + d),
                    LensGhost = RKN   (name + "lens_ghost" + d),
                    LensShaft = RKN   (name + "lens_shaft" + d),
                };
            }

            if (dict.FV(out value, "dof.name"))
            {
                DOF dof = new DOF { Name = value };
                Head.Format = Format.AFT;
                dof.MT = RMT("dof" + d);
                Data.DOF = dof;
            }

            if (dict.FV(out value, "ambient.length"))
            {
                Data.Ambient = new Ambient[int.Parse(value)];
                Head.Format = Format.MGF;
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    name = "ambient" + d + i0 + d;
                    dict.FV(out Data.Ambient[i0].Name, name + "name");
                    Data.Ambient[i0].   LightDiffuse = RRGBAK(name +    "light.Diffuse" + d);
                    Data.Ambient[i0].RimLightDiffuse = RRGBAK(name + "rimlight.Diffuse" + d);
                }
            }

            if (dict.FV(out value, "camera_root.length"))
            {
                Data.CameraRoot = new CameraRoot[int.Parse(value)];
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    name = "camera_root" + d + i0 + d;
                    nameView = name + "view_point" + d;

                    dict.FV(out Data.CameraRoot[i0].VP.
                        Aspect         , nameView + "aspect"           );
                    dict.FV(out Data.CameraRoot[i0].VP.
                        CameraApertureH, nameView + "camera_aperture_h");
                    dict.FV(out Data.CameraRoot[i0].VP.
                        CameraApertureW, nameView + "camera_aperture_w");
                    dict.FV(out i1, nameView + "fov_is_horizontal");
                    Data.CameraRoot[i0].VP.FOVHorizontal = i1 != 0;

                    Data.CameraRoot[i0].      MT = RMT(name);
                    Data.CameraRoot[i0].Interest = RMT(name + "interest" + d);
                    Data.CameraRoot[i0].VP.   MT = RMT(nameView);
                    Data.CameraRoot[i0].VP.FocalLength = RK(nameView + "focal_length" + d);
                    Data.CameraRoot[i0].VP.FOV         = RK(nameView +          "fov" + d);
                    Data.CameraRoot[i0].VP.Roll        = RK(nameView +         "roll" + d);
                }
            }

            if (dict.FV(out value, "chara.length"))
            {
                Data.Chara = new ModelTransform[int.Parse(value)];
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    Data.Chara[i0] = RMT("chara" + d + i0 + d);
            }

            if (dict.FV(out value, "curve.length"))
            {
                Data.Curve = new Curve[int.Parse(value)];
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    name = "curve" + d + i0 + d;

                    dict.FV(out Data.Curve[i0].Name, name + "name");
                    Data.Curve[i0].CV = RK(name + "cv" + d);
                }
            }

            if (dict.FV(out value, "event.length"))
            {
                Data.Event = new Event[int.Parse(value)];
                for (i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    name = "event" + d + i0 + d;

                    dict.FV(out Data.Event[i0].Begin       , name + "begin"         );
                    dict.FV(out Data.Event[i0].ClipBegin   , name + "clip_begin"    );
                    dict.FV(out Data.Event[i0].ClipEnd     , name + "clip_en"       );
                    dict.FV(out Data.Event[i0].End         , name + "end"           );
                    dict.FV(out Data.Event[i0].Name        , name + "name"          );
                    dict.FV(out Data.Event[i0].Param1      , name + "param1"        );
                    dict.FV(out Data.Event[i0].Ref         , name + "ref"           );
                    dict.FV(out Data.Event[i0].TimeRefScale, name + "time_ref_scale");
                    dict.FV(out Data.Event[i0].Type        , name + "type"          );
                }
            }

            if (dict.FV(out value, "fog.length"))
            {
                Data.Fog = new Fog[int.Parse(value)];
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    name = "fog" + d + i0 + d;

                    dict.FV(out Data.Fog[i0].Id, name + "id");
                    Data.Fog[i0].Density = RK    (name + "density" + d);
                    Data.Fog[i0].Diffuse = RRGBAK(name + "Diffuse" + d);
                    Data.Fog[i0].End     = RK    (name +     "end" + d);
                    Data.Fog[i0].Start   = RK    (name +   "start" + d);
                }
            }

            if (dict.FV(out value, "light.length"))
            {
                Data.Light = new Light[int.Parse(value)];
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    name = "light" + d + i0 + d;

                    dict.FV(out Data.Light[i0].Id  , name + "id"  );
                    dict.FV(out Data.Light[i0].Name, name + "name");
                    dict.FV(out Data.Light[i0].Type, name + "type");

                    Data.Light[i0].Ambient       = RRGBAK(name +        "Ambient" + d);
                    Data.Light[i0].Diffuse       = RRGBAK(name +        "Diffuse" + d);
                    Data.Light[i0].Incandescence = RRGBAK(name +  "Incandescence" + d);
                    Data.Light[i0].Specular      = RRGBAK(name +       "Specular" + d);
                    Data.Light[i0].Position      = RMT     (name +       "position" + d);
                    Data.Light[i0].SpotDirection = RMT     (name + "spot_direction" + d);
                }
            }

            if (dict.FV(out value, "m_objhrc.length"))
            {
                Data.MObjectHRC = new MObjectHRC[int.Parse(value)];
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    name = "m_objhrc" + d + i0 + d;

                    dict.FV(out Data.MObjectHRC[i0].Name, name + "name");

                    if (dict.SW(name + "joint_orient"))
                    {
                        Data.MObjectHRC[i0].JointOrient = new Vec3<float?>();
                        dict.FV(out Data.MObjectHRC[i0].JointOrient.X, name + "joint_orient.x");
                        dict.FV(out Data.MObjectHRC[i0].JointOrient.Y, name + "joint_orient.y");
                        dict.FV(out Data.MObjectHRC[i0].JointOrient.Z, name + "joint_orient.z");
                    }

                    if (dict.FV(out value, name + "instance.length"))
                    {
                        Data.MObjectHRC[i0].Instances = new MObjectHRC.Instance[int.Parse(value)];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                        {
                            nameView = name + "instance" + d + i1 + d;

                            dict.FV(out Data.MObjectHRC[i0]
                                .Instances[i1].   Name, nameView +     "name");
                            dict.FV(out Data.MObjectHRC[i0]
                                .Instances[i1]. Shadow, nameView +   "shadow");
                            dict.FV(out Data.MObjectHRC[i0]
                                .Instances[i1].UIDName, nameView + "uid_name");

                            Data.MObjectHRC[i0].Instances[i1].MT = RMT(nameView);
                        }
                    }

                    if (dict.FV(out value, name + "node.length"))
                    {
                        Data.MObjectHRC[i0].Node = new Node[int.Parse(value)];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            nameView = name + "node" + d + i1 + d;
                            dict.FV(out Data.MObjectHRC[i0].Node[i1].Name  , nameView + "name"  );
                            dict.FV(out Data.MObjectHRC[i0].Node[i1].Parent, nameView + "parent");

                            Data.MObjectHRC[i0].Node[i1].MT = RMT(nameView);
                        }
                    }

                    Data.MObjectHRC[i0].MT = RMT(name);
                }
            }

            if (dict.FV(out value, "m_objhrc_list.length"))
            {
                Data.MObjectHRCList = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                    dict.FV(out Data.MObjectHRCList[i0], "m_objhrc_list" + d + i0);
            }

            if (dict.FV(out value, "material_list.length"))
            {
                Data.MaterialList = new MaterialList[int.Parse(value)];
                Head.Format = Format.X;
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    name = "material_list" + d + i0 + d;
                    dict.FV(out Data.MaterialList[i0].HashName, name + "hash_name");
                    dict.FV(out Data.MaterialList[i0].    Name, name +      "name");

                    Data.MaterialList[i0].BlendColor    = RRGBAK(name +    "blend_color" + d);
                    Data.MaterialList[i0].GlowIntensity = RK    (name + "glow_intensity" + d);
                    Data.MaterialList[i0].Incandescence = RRGBAK(name +  "incandescence" + d);
                }
            }

            if (dict.FV(out value, "motion.length"))
            {
                Data.Motion = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.Motion.Length; i0++)
                    dict.FV(out Data.Motion[i0], "motion" + d + i0 + d + "name");
            }

            if (dict.FV(out value, "object.length"))
            {
                Data.Object = new Object[int.Parse(value)];
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    name = "object" + d + i0 + d;

                    dict.FV(out Data.Object[i0].Morph      , name + "morph"       );
                    dict.FV(out Data.Object[i0].MorphOffset, name + "morph_offset");
                    dict.FV(out Data.Object[i0].       Name, name +         "name");
                    dict.FV(out Data.Object[i0]. ParentName, name +  "parent_name");
                    dict.FV(out Data.Object[i0].    UIDName, name +     "uid_name");

                    if (dict.FV(out value, name + "tex_pat.length"))
                    {
                        Data.Object[i0].TexPat = new Object.TexturePattern[int.Parse(value)];
                        for (i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                        {
                            nameView = name + "tex_pat" + d + i1 + d;
                            dict.FV(out Data.Object[i0].TexPat[i1]
                                .Name     , nameView + "name"      );
                            dict.FV(out Data.Object[i0].TexPat[i1]
                                .Pat      , nameView + "pat"       );
                            dict.FV(out Data.Object[i0].TexPat[i1]
                                .PatOffset, nameView + "pat_offset");
                        }
                    }

                    if (dict.FV(out value, name + "tex_transform.length"))
                    {
                        Data.Object[i0].TexTrans = new Object.TextureTransform[int.Parse(value)];
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                        {
                            nameView = name + "tex_transform" + d + i1 + d;

                            dict.FV(out Data.Object[i0].TexTrans[i1].Name, nameView + "name");
                            Data.Object[i0].TexTrans[i1].Coverage       = RKUV(nameView + "coverage"      );
                            Data.Object[i0].TexTrans[i1].Offset         = RKUV(nameView + "offset"        );
                            Data.Object[i0].TexTrans[i1].Repeat         = RKUV(nameView + "repeat"        );
                            Data.Object[i0].TexTrans[i1].   Rotate      = RKN (nameView + "rotate"     + d);
                            Data.Object[i0].TexTrans[i1].   RotateFrame = RKN (nameView + "rotateFrame"+ d);
                            Data.Object[i0].TexTrans[i1].TranslateFrame = RKUV(nameView + "translateFrame");
                        }
                    }

                    Data.Object[i0].MT = RMT(name);
                }
            }

            if (dict.FV(out value, "objhrc.length"))
            {
                Data.ObjectHRC = new ObjectHRC[int.Parse(value)];
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    name = "objhrc" + d + i0 + d;

                    if (dict.SW(name + "joint_orient"))
                    {
                        Data.ObjectHRC[i0].JointOrient = new Vec3<float?>();
                        dict.FV(out Data.ObjectHRC[i0].JointOrient.X, name + "joint_orient.x");
                        dict.FV(out Data.ObjectHRC[i0].JointOrient.Y, name + "joint_orient.y");
                        dict.FV(out Data.ObjectHRC[i0].JointOrient.Z, name + "joint_orient.z");
                    }
                    dict.FV(out Data.ObjectHRC[i0].   Name, name +     "name");
                    dict.FV(out Data.ObjectHRC[i0]. Shadow, name +   "shadow");
                    dict.FV(out Data.ObjectHRC[i0].UIDName, name + "uid_name");
                    if (dict.FV(out value, name + "node.length"))
                    {
                        Data.ObjectHRC[i0].Node = new Node[int.Parse(value)];
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        {
                            nameView = name + "node" + d + i1 + d;

                            dict.FV(out Data.ObjectHRC[i0].Node[i1].Name  , nameView + "name"  );
                            dict.FV(out Data.ObjectHRC[i0].Node[i1].Parent, nameView + "parent");

                            Data.ObjectHRC[i0].Node[i1].MT = RMT(nameView);
                        }
                    }
                }
            }

            if (dict.FV(out value, "object_list.length"))
            {
                Data.ObjectList = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.ObjectList.Length; i0++)
                    dict.FV(out Data.ObjectList[i0], "object_list" + d + i0);
            }

            if (dict.FV(out value, "objhrc_list.length"))
            {
                Data.ObjectHRCList = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
                    dict.FV(out Data.ObjectHRCList[i0], "objhrc_list" + d + i0);
            }

            if (dict.FV(out value, "point.length"))
            {
                Data.Point = new ModelTransform[int.Parse(value)];
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    Data.Point[i0] = RMT("point" + d + i0 + d);
            }
        }

        private byte[] A3DAWriter(bool a3dc)
        {
            this.a3dc = a3dc;
            s = File.OpenWriter();
            DateTime date = DateTime.Now;
            if (a3dc && Data._.CompressF16 != 0)
                s.W("#-compress_f16\n");
            if (!a3dc)
                s.W("#A3DA__________\n");
            s.W("#" + DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss yyyy",
                System.Globalization.CultureInfo.InvariantCulture) + "\n");
            if (a3dc && Data._.CompressF16 != 0)
                W("_.compress_f16", (int)Data._.CompressF16);

            W("_.converter.version", Data._.ConverterVersion);
            W("_.file_name"        , Data._.FileName        );
            W("_.property.version" , Data._. PropertyVersion);

            if (Data.Ambient != null && Head.Format == Format.MGF)
            {
                so0 = Data.Ambient.Length.SW();
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "ambient" + d + soi0 + d;
                    ref Ambient Ambient = ref Data.Ambient[soi0];

                    W(ref Ambient.   LightDiffuse, name +    "light.Diffuse");
                    W(name + "name", Ambient.Name);
                    W(ref Ambient.RimLightDiffuse, name + "rimlight.Diffuse");
                }
                W("ambient.length", Data.Fog.Length);
            }

            if (Data.CameraAuxiliary != null)
            {
                name = "camera_auxiliary" + d;
                CameraAuxiliary ca = Data.CameraAuxiliary.Value;
                W(ref ca.AutoExposure, name + "auto_exposure");
                W(ref ca.    Exposure, name +      "exposure");
                W(ref ca.Gamma       , name + "gamma"        );
                if (Head.Format == Format.F || (Head.Format > Format.AFT && Head.Format < Format.FT))
                    W(ref ca.GammaRate   , name + "gamma_rate"   );
                W(ref ca.Saturate    , name + "saturate"     );
                Data.CameraAuxiliary = ca;
            }

            if (Data.CameraRoot != null)
            {
                so0 = Data.CameraRoot.Length.SW();
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "camera_root" + d + soi0 + d;
                    nameView = name + "view_point" + d;
                    ref CameraRoot cr = ref Data.CameraRoot[soi0];

                    W(ref cr.Interest, name + "interest" + d);
                    W(ref cr.MT, name, 0b11110);
                    W(nameView + "aspect", cr.VP.Aspect);
                    if (cr.VP.CameraApertureH.HasValue)
                        W(nameView + "camera_aperture_h", cr.VP.CameraApertureH);
                    if (cr.VP.CameraApertureW.HasValue)
                        W(nameView + "camera_aperture_w", cr.VP.CameraApertureW);
                    W(ref cr.VP.FocalLength, nameView + "focal_length" + d);
                    W(ref cr.VP.FOV, nameView + "fov" + d);
                    if (cr.VP.FOVHorizontal)
                        W(nameView + "fov_is_horizontal", cr.VP.FOVHorizontal ? 1 : 0);
                    W(ref cr.VP.MT  , nameView, 0b10000);
                    W(ref cr.VP.Roll, nameView + "roll" + d);
                    W(ref cr.VP.MT  , nameView, 0b01111);
                    W(ref cr.MT  , name    , 0b00001);
                }
                W("camera_root.length", Data.CameraRoot.Length);
            }

            if (Data.Chara != null)
            {
                so0 = Data.Chara.Length.SW();
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    W(ref Data.Chara[so0[i0]], "chara" + d + so0[i0] + d);
                W("chara.length", Data.Chara.Length);
            }

            if (Data.Curve != null)
            {
                so0 = Data.Curve.Length.SW();
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "curve" + d + soi0 + d;
                    ref Curve curve = ref Data.Curve[soi0];

                    W(ref curve.CV, name + "cv" + d);
                    W(name + "name", curve.Name);
                }
                W("curve.length", Data.Curve.Length);
            }

            if (Data.DOF != null && (Head.Format == Format.AFT || Head.Format == Format.FT))
            {
                DOF dof = Data.DOF.Value;
                W("dof.name", dof.Name);
                W(ref dof.MT, "dof" + d);
                Data.DOF = dof;
            }

            if (Data.Event != null)
            {
                so0 = Data.Event.Length.SW();
                for (i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "event" + d + soi0 + d;
                    ref Event @event = ref Data.Event[soi0];

                    W(name + "begin"         , @event.Begin       );
                    W(name + "clip_begin"    , @event.ClipBegin   );
                    W(name + "clip_en"       , @event.ClipEnd     );
                    W(name + "end"           , @event.End         );
                    W(name + "name"          , @event.Name        );
                    W(name + "param1"        , @event.Param1      );
                    W(name + "ref"           , @event.Ref         );
                    W(name + "time_ref_scale", @event.TimeRefScale);
                    W(name + "type"          , @event.Type        );
                }
            }

            if (Data.Fog != null)
            {
                so0 = Data.Fog.Length.SW();
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "fog" + d + soi0 + d;
                    ref Fog fog = ref Data.Fog[soi0];

                    W(ref fog.Diffuse, name + "Diffuse");
                    W(ref fog.Density, name + "density");
                    W(ref fog.End    , name + "end"    );
                    W(name + "id", fog.Id);
                    W(ref fog.Start  , name + "start"  );
                }
                W("fog.length", Data.Fog.Length);
            }

            if (Data.Light != null)
            {
                so0 = Data.Light.Length.SW();
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "light" + d + soi0 + d;
                    ref Light light = ref Data.Light[soi0];

                    W(ref light.Ambient      , name + "Ambient"      );
                    W(ref light.Diffuse      , name + "Diffuse"      );
                    W(ref light.Incandescence, name + "Incandescence");
                    W(ref light.Specular     , name + "Specular"     );
                    W(name + "id"  , light.Id  );
                    W(name + "name", light.Name);
                    W(ref light.Position     , name + "position"       + d);
                    W(ref light.SpotDirection, name + "spot_direction" + d);
                    W(name + "type", light.Type);
                }
                W("light.length", Data.Light.Length);
            }

            if (Data.MObjectHRC != null)
            {
                so0 = Data.MObjectHRC.Length.SW();
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "m_objhrc" + d + soi0 + d;
                    ref MObjectHRC mObjectHRC = ref Data.MObjectHRC[soi0];

                    if (mObjectHRC.JointOrient.NotNull && (Head.Format ==
                        Format.X || Head.Format == Format.XHD))
                    {
                        W(name + "joint_orient.x", mObjectHRC.JointOrient.X);
                        W(name + "joint_orient.y", mObjectHRC.JointOrient.Y);
                        W(name + "joint_orient.z", mObjectHRC.JointOrient.Z);
                    }

                    if (mObjectHRC.Instances != null)
                    {
                        so1 = mObjectHRC.Instances.Length.SW();
                        for (i1 = 0; i1 < mObjectHRC.Instances.Length; i1++)
                        {
                            soi1 = so1[i1];
                            nameView = name + "instance" + d + soi1 + d;
                            ref MObjectHRC.Instance instance = ref mObjectHRC.Instances[soi1];

                            W(ref instance.MT, nameView, 0b10000);
                            W(nameView +     "name", instance.   Name);
                            W(ref instance.MT, nameView, 0b01100);
                            W(nameView +   "shadow", instance. Shadow);
                            W(ref instance.MT, nameView, 0b00010);
                            W(nameView + "uid_name", instance.UIDName);
                            W(ref instance.MT, nameView, 0b00001);
                        }
                        W(name + "instance.length", mObjectHRC.Instances.Length);
                    }

                    W(ref mObjectHRC.MT, name, 0b10000);
                    W(name + "name", mObjectHRC.Name);

                    if (mObjectHRC.Node != null)
                    {
                        so1 = mObjectHRC.Node.Length.SW();
                        for (i1 = 0; i1 < mObjectHRC.Node.Length; i1++)
                        {
                            soi1 = so1[i1];
                            nameView = name + "node" + d + soi1 + d;
                            ref Node node = ref mObjectHRC.Node[soi1];

                            W(ref node.MT, nameView, 0b10000);
                            W(nameView +   "name", node.Name  );
                            W(nameView + "parent", node.Parent);
                            W(ref node.MT, nameView, 0b01111);
                        }
                        W(name + "node.length", mObjectHRC.Node.Length);
                    }

                    W(ref mObjectHRC.MT, name, 0b01111);
                }
                W("m_objhrc.length", Data.MObjectHRC.Length);
            }

            if (Data.MObjectHRCList != null)
            {
                so0 = Data.MObjectHRCList.Length.SW();
                for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                    W("m_objhrc_list" + d + so0[i0] + "", Data.MObjectHRCList[so0[i0]]);
                W("m_objhrc_list.length", Data.MObjectHRCList.Length);
            }

            if (Data.MaterialList != null && (Head.Format == Format.X || Head.Format == Format.XHD))
            {
                so0 = Data.MaterialList.Length.SW();
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "material_list" + d + soi0 + d;
                    ref MaterialList ml = ref Data.MaterialList[soi0];

                    W(ref ml.BlendColor   , name + "blend_color"   );
                    W(ref ml.GlowIntensity, name + "glow_intensity");
                    W(name + "hash_name", ml.HashName);
                    W(ref ml.Incandescence, name + "incandescence" );
                    W(name +      "name", ml.    Name);
                }
                W("material_list.length", Data.MaterialList.Length);
            }

            if (Data.Motion != null)
            {
                so0 = Data.Motion.Length.SW();
                for (i0 = 0; i0 < Data.Motion.Length; i0++)
                    W(name + so0[i0] + d + "name", Data.Motion[so0[i0]]);
                W("motion.length", Data.Motion.Length);
            }

            if (Data.Object != null)
            {
                so0 = Data.Object.Length.SW();
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "object" + d + soi0 + d;
                    ref Object @object = ref Data.Object[soi0];

                    W(ref @object.MT, name, 0b10000);
                    if (@object.Morph != null)
                    {
                        W(name + "morph"       , @object.Morph      );
                        W(name + "morph_offset", @object.MorphOffset);
                    }
                    W(name + "name"       , @object.Name      );
                    W(name + "parent_name", @object.ParentName);
                    W(ref @object.MT, name, 0b01100);

                    if (@object.TexPat != null)
                    {
                        so1 = @object.TexPat.Length.SW();
                        for (i1 = 0; i1 < @object.TexPat.Length; i1++)
                        {
                            soi1 = so1[i1];
                            nameView = name + "tex_pat" + d + soi1 + d;
                            ref Object.TexturePattern texPat = ref @object.TexPat[soi1];

                            W(nameView + "name"      , texPat.Name     );
                            W(nameView + "pat"       , texPat.Pat      );
                            W(nameView + "pat_offset", texPat.PatOffset);
                        }
                        W(name + "tex_pat.length", @object.TexPat.Length);
                    }

                    if (@object.TexTrans != null)
                    {
                        so1 = @object.TexTrans.Length.SW();
                        for (i1 = 0; i1 < @object.TexTrans.Length; i1++)
                        {
                            soi1 = so1[i1];
                            nameView = name + "tex_transform" + d + soi1 + d;
                            ref Object.TextureTransform texTrans = ref @object.TexTrans[soi1];

                            W(nameView + "name", @object.TexTrans[soi1].Name);
                            W(ref texTrans.Coverage      , nameView + "coverage"      );
                            W(ref texTrans.Offset        , nameView + "offset"        );
                            W(ref texTrans.Repeat        , nameView + "repeat"        );
                            W(ref texTrans.   Rotate     , nameView + "rotate"        );
                            W(ref texTrans.   RotateFrame, nameView + "rotateFrame"   );
                            W(ref texTrans.TranslateFrame, nameView + "translateFrame");
                        }
                        W(name + "tex_transform.length", @object.TexTrans.Length);
                    }

                    W(ref @object.MT, name, 0b00010);
                    W(name + "uid_name", @object.UIDName);
                    W(ref @object.MT, name, 0b00001);
                }
                W("object.length", Data.Object.Length);
            }

            if (Data.ObjectList != null)
            {
                so0 = Data.ObjectList.Length.SW();
                for (i0 = 0; i0 < Data.ObjectList.Length; i0++)
                    W("object_list" + d + so0[i0] + "", Data.ObjectList[so0[i0]]);
                W("object_list.length", Data.ObjectList.Length);
            }

            if (Data.ObjectHRC != null)
            {
                so0 = Data.ObjectHRC.Length.SW();
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "objhrc" + d + soi0 + d;
                    ref ObjectHRC objectHRC = ref Data.ObjectHRC[soi0];

                    W(name + "name", objectHRC.Name);

                    if (objectHRC.JointOrient.NotNull && (Head.Format ==
                        Format.X || Head.Format == Format.XHD))
                    {
                        W(name + "joint_orient.x", objectHRC.JointOrient.X);
                        W(name + "joint_orient.y", objectHRC.JointOrient.Y);
                        W(name + "joint_orient.z", objectHRC.JointOrient.Z);
                    }

                    if (objectHRC.Node != null)
                    {
                        so1 = objectHRC.Node.Length.SW();
                        for (i1 = 0; i1 < objectHRC.Node.Length; i1++)
                        {
                            soi1 = so1[i1];
                            nameView = name + "node" + d + soi1 + d;
                            ref Node node = ref objectHRC.Node[soi1];

                            W(ref node.MT, nameView, 0b10000);
                            W(nameView + "name"  , node.Name  );
                            W(nameView + "parent", node.Parent);
                            W(ref node.MT, nameView, 0b01111);
                        }
                        W(name + "node.length", objectHRC.Node.Length);
                    }

                    if (objectHRC.Shadow != null)
                        W(name + "shadow", objectHRC.Shadow.HasValue && objectHRC.Shadow.Value ? 1 : 0);
                    W(name + "uid_name", objectHRC.UIDName);
                }
                W("objhrc.length", Data.ObjectHRC.Length);
            }

            if (Data.ObjectHRCList != null)
            {
                so0 = Data.ObjectHRCList.Length.SW();
                for (i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
                    W("objhrc_list" + d + so0[i0] + "", Data.ObjectHRCList[so0[i0]]);
                W("objhrc_list.length", Data.ObjectHRCList.Length);
            }

            W("play_control.begin", Data.PlayControl.Begin);
            if (Head.Format > Format.AFT && Head.Format < Format.FT)
                W("play_control.div", Data.PlayControl.Div);
            W("play_control.fps", Data.PlayControl.FPS);
            if (Data.PlayControl.Offset != null)
            { if (Head.Format > Format.AFT && Head.Format < Format.FT)
              {    W("play_control.offset", Data.PlayControl.Offset);
                   W("play_control.size"  , Data.PlayControl.Size  ); }
              else W("play_control.size"  , Data.PlayControl.Size + Data.PlayControl.Offset);
            }
            else   W("play_control.size"  , Data.PlayControl.Size);

            if (Data.PostProcess != null)
            {
                PostProcess pp = Data.PostProcess.Value;
                name = "post_process" + d;
                W(ref pp.Ambient  , name + "Ambient"   );
                W(ref pp.Diffuse  , name + "Diffuse"   );
                W(ref pp.Specular , name + "Specular"  );
                W(ref pp.LensFlare, name + "lens_flare");
                W(ref pp.LensGhost, name + "lens_ghost");
                W(ref pp.LensShaft, name + "lens_shaft");
                Data.PostProcess = pp;
            }

            if (Data.Point != null)
            {
                so0 = Data.Point.Length.SW();
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    W(ref Data.Point[so0[i0]], "point" + d + so0[i0] + d);
                W("point.length", Data.Point.Length);
            }

            s.A(0x1, true);
            byte[] data = s.ToArray();
            s.Dispose();
            return data;
        }

        private ModelTransform RMT(string str)
        {
            ModelTransform mt = new ModelTransform();
            dict.FV(out mt.BinOffset, str + MTBO);

            mt.Rot        = RV3(str + "rot"        + d);
            mt.Scale      = RV3(str + "scale"      + d);
            mt.Trans      = RV3(str + "trans"      + d);
            mt.Visibility = RK (str + "visibility" + d);
            return mt;
        }

        private Vec4<Key>? RRGBAK(string str) =>
            dict.FV(out bool b, str) ? b ?
            (Vec4<Key>?)new Vec4<Key> { W = RK(str + "a" + d), Z = RK(str + "b" + d),
                                        Y = RK(str + "g" + d), X = RK(str + "r" + d) }: null : null;

        private Vec3<Key> RV3(string str) =>
            new Vec3<Key> { X = RK(str + "x" + d), Y = RK(str + "y" + d), Z = RK(str + "z" + d) };

        private Vec2<Key?> RKUV(string str) =>
            new Vec2<Key?> { X = RKN(str + "U" + d), Y = RKN(str + "V" + d) };

        private Key? RKN(string str) =>
            dict.FV(out bool b, str) ? b ? (Key?)RK(str) : null : null;

        private Key RK(string str)
        {
            Key key = new Key();
            if ( dict.FV(out key.BinOffset, str + BO    )) return  key;
            if (!dict.FV(out int type     , str + "type")) return default;

            key.Type = (KeyType)type;
            if (type == 0x0000) return key;
            if (type == 0x0001) { dict.FV(out key.Value, str + "value"); return key; }

            int i = 0;
            if (dict.FV(out int epTypePost, str + "ep_type_post"))
                key.EPTypePost = (EPType)epTypePost;
            if (dict.FV(out int epTypePre , str + "ep_type_pre" ))
                key.EPTypePre  = (EPType)epTypePre ;
            dict.FV(out key.Length, str + "key.length");
            dict.FV(out key.Max   , str + "max"       );
            if (dict.SW(str + "raw_data"))
                dict.FV(out key.RawData.KeyType, str + "raw_data_key_type");

            if (key.RawData.KeyType != 0)
            {
                ref string[] VL = ref key.RawData.ValueList;
                dict.FV(out key.RawData.ValueType, str + "raw_data.value_type");
                if (dict.FV(out value, str + "raw_data.value_list"))
                    VL = value.Split(',');
                dict.FV(out key.RawData.ValueListSize, str + "raw_data.value_list_size");
                value = "";

                int ds = key.RawData.KeyType + 1;
                key.Length = key.RawData.ValueListSize / ds;
                key.Keys = new KFT3[key.Length];
                     if (key.RawData.KeyType == 0)
                    for (i = 0; i < key.Length; i++)
                        key.Keys[i] = new KFT3(VL[i * ds + 0].ToF32());
                else if (key.RawData.KeyType == 1)
                    for (i = 0; i < key.Length; i++)
                        key.Keys[i] = new KFT3(VL[i * ds + 0].ToF32(), VL[i * ds + 1].ToF32());
                else if (key.RawData.KeyType == 2)
                    for (i = 0; i < key.Length; i++)
                        key.Keys[i] = new KFT3(VL[i * ds + 0].ToF32(), VL[i * ds + 1].ToF32(),
                                               VL[i * ds + 2].ToF32());
                else if (key.RawData.KeyType == 3)
                    for (i = 0; i < key.Length; i++)
                        key.Keys[i] = new KFT3(VL[i * ds + 0].ToF32(), VL[i * ds + 1].ToF32(),
                                               VL[i * ds + 2].ToF32(), VL[i * ds + 3].ToF32());

                key.RawData.ValueList = null;
            }
            else
            {
                key.Keys = new KFT3[key.Length];
                for (i = 0; i < key.Length; i++)
                {
                    if (!dict.FV(out value, str + "key" + d + i + d + "data")) continue;

                    dataArray = value.Replace("(", "").Replace(")", "").Split(',');
                    type = dataArray.Length - 1;
                         if (type == 0) key.Keys[i] = new KFT3
                        (dataArray[0].ToF32());
                    else if (type == 1) key.Keys[i] = new KFT3
                        (dataArray[0].ToF32(), dataArray[1].ToF32());
                    else if (type == 2) key.Keys[i] = new KFT3
                        (dataArray[0].ToF32(), dataArray[1].ToF32(), dataArray[2].ToF32());
                    else if (type == 3)  key.Keys[i] = new KFT3
                        (dataArray[0].ToF32(), dataArray[1].ToF32(), dataArray[2].ToF32(), dataArray[3].ToF32());
                }
            }
            return key;
        }

        private void W(ref ModelTransform mt, string str, byte flags = 0b11111)
        {
            if (a3dc && !mt.Writed && (flags & 0b10000) != 0)
            { W(str + MTBO + "", mt.BinOffset); mt.Writed = true; }

            if (!a3dc)
            {
                if ((flags & 0b01000) != 0) W(ref mt.Rot       , str + "rot"        + d);
                if ((flags & 0b00100) != 0) W(ref mt.Scale     , str + "scale"      + d);
                if ((flags & 0b00010) != 0) W(ref mt.Trans     , str + "trans"      + d);
                if ((flags & 0b00001) != 0) W(ref mt.Visibility, str + "visibility" + d);
            }
        }

        private void W(ref Vec4<Key>? rgba, string str)
        {
            if (!rgba.HasValue) return;

            W(str, "true");
            Vec4<Key> rgbav = rgba.Value;
            W(ref rgbav.W, str + d + "a" + d);
            W(ref rgbav.Z, str + d + "b" + d);
            W(ref rgbav.Y, str + d + "g" + d);
            W(ref rgbav.X, str + d + "r" + d);
            rgba = rgbav;
        }

        private void W(ref Vec3<Key> key, string str)
        { W(ref key.X, str + "x" + d); W(ref key.Y, str + "y" + d); W(ref key.Z, str + "z" + d); }

        private void W(ref Vec2<Key?> uv, string str)
        { W(ref uv.X, str + "U"); W(ref uv.Y, str + "V"); }

        private void W(ref Key? key, string str)
        { if (!key.HasValue) return; Key k = key.Value; W(str, "true"); W(ref k, str + d); key = k; }

        private void W(ref Key key, string str)
        {
            if (a3dc) { W(str + BO + "", key.BinOffset); return; }

            int i = 0;
            if (key.Type < KeyType.Linear || key.Keys == null || (key.Keys != null && key.Keys.Length == 0))
            {
                W(str + "type", (int)key.Type);
                if (key.Type > 0) W(str + "value", key.Value);
                return;
            }

            if (key.EPTypePost > 0) W(str + "ep_type_post", (int)key.EPTypePost);
            if (key.EPTypePre  > 0) W(str + "ep_type_pre" , (int)key.EPTypePre );
            if (key.RawData.KeyType == 0 && key.Keys != null)
            {
                IKF kf;
                so = key.Keys.Length.SW();
                for (i = 0; i < key.Keys.Length; i++)
                {
                    soi = so[i];
                    kf = key.Keys[soi].Check();
                    W(str + "key" + d + soi + d + "data", kf.ToString());
                    int Type = 0;
                         if (kf is KFT0) Type = 0;
                    else if (kf is KFT1) Type = 1;
                    else if (kf is KFT2) Type = 2;
                    else if (kf is KFT3) Type = 3;
                    W(str + "key" + d + soi + d + "type", Type);
                }
                W(str + "key.length", key.Length);
                W(str + "max", (float)(key.Max ?? Data.PlayControl.Size));
                W(str + "type", (int)key.Type);
            }
            else if (key.Keys != null)
            {
                int length = key.Keys.Length;
                ref int keyType = ref key.RawData.KeyType;
                keyType = 0;
                IKF kf;
                W(str + "max", (float)(key.Max ?? Data.PlayControl.Size));
                for (i = 0; i < length && keyType < 3; i++)
                {
                    kf = key.Keys[i].Check();
                         if (kf is KFT0 && keyType < 0) keyType = 0;
                    else if (kf is KFT1 && keyType < 1) keyType = 1;
                    else if (kf is KFT2 && keyType < 2) keyType = 2;
                    else if (kf is KFT3 && keyType < 3) keyType = 3;
                }
                key.RawData.ValueListSize = length * keyType + length;
                s.W(str + "raw_data.value_list");
                     if (keyType == 0) for (i = 0; i < length; i++)
                        s.W(key.Keys[i].ToT0().ToString(false) + (i + 1 < length ? "," : ""));
                else if (keyType == 1) for (i = 0; i < length; i++)
                        s.W(key.Keys[i].ToT1().ToString(false) + (i + 1 < length ? "," : ""));
                else if (keyType == 2) for (i = 0; i < length; i++)
                        s.W(key.Keys[i].ToT2().ToString(false) + (i + 1 < length ? "," : ""));
                else if (keyType == 3) for (i = 0; i < length; i++)
                        s.W(key.Keys[i]       .ToString(false) + (i + 1 < length ? "," : ""));
                s.P--;
                s.W('\n');
                W(str + "raw_data.value_list_size", key.RawData.ValueListSize);
                W(str + "raw_data.value_type"     , key.RawData.ValueType    );
                W(str + "raw_data_key_type"       , key.RawData.  KeyType    );
                W(str + "type", (int)key.Type);
            }
        }

        private void W(string Data,   long? val)
        { if (val != null) W(Data, ( long)val   ); }
        private void W(string Data,  float? val)
        { if (val != null) W(Data, (float)val   ); }
        private void W(string Data,   long  val) =>
                           W(Data,        val.ToS());
        private void W(string Data,  float  val) =>
                           W(Data,        val.ToS(9));
        private void W(string Data, string  val)
        { if (val != null) s.W(Data + "=" + val + "\n"); }

        private void A3DCReader()
        {
            if (Data.Ambient != null)
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    RRGBAK(ref Data.Ambient[i0].   LightDiffuse);
                    RRGBAK(ref Data.Ambient[i0].RimLightDiffuse);
                }

            if (Data.CameraAuxiliary != null)
            {
                CameraAuxiliary ca = Data.CameraAuxiliary.Value;
                RK(ref ca.AutoExposure);
                RK(ref ca.    Exposure);
                RK(ref ca.Gamma       );
                RK(ref ca.GammaRate   );
                RK(ref ca.Saturate    );
                Data.CameraAuxiliary = ca;
            }

            if (Data.CameraRoot != null)
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    RMT(ref Data.CameraRoot[i0].      MT);
                    RMT(ref Data.CameraRoot[i0].Interest);
                    RMT(ref Data.CameraRoot[i0].VP.   MT);
                    RK (ref Data.CameraRoot[i0].VP.FocalLength);
                    RK (ref Data.CameraRoot[i0].VP.FOV        );
                    RK (ref Data.CameraRoot[i0].VP.Roll       );
                }

            if (Data.Chara != null)
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    RMT(ref Data.Chara[i0]);

            if (Data.Curve != null)
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                    RK(ref Data.Curve[i0].CV);

            if (Data.DOF != null)
            {
                DOF dof = Data.DOF.Value;
                RMT(ref dof.MT);
                Data.DOF = dof;
            }

            if (Data.Fog != null)
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    RK    (ref Data.Fog[i0].Density);
                    RRGBAK(ref Data.Fog[i0].Diffuse);
                    RK    (ref Data.Fog[i0].End    );
                    RK    (ref Data.Fog[i0].Start  );
                }

            if (Data.Light != null)
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    RRGBAK(ref Data.Light[i0].Ambient      );
                    RRGBAK(ref Data.Light[i0].Diffuse      );
                    RRGBAK(ref Data.Light[i0].Incandescence);
                    RMT   (ref Data.Light[i0].Position     );
                    RRGBAK(ref Data.Light[i0].Specular     );
                    RMT   (ref Data.Light[i0].SpotDirection);
                }

            if (Data.MObjectHRC != null)
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    RMT(ref Data.MObjectHRC[i0].MT);

                    if (Data.MObjectHRC[i0].Instances != null)
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                            RMT(ref Data.MObjectHRC[i0].Instances[i1].MT);

                    if (Data.MObjectHRC[i0].Node != null)
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            RMT(ref Data.MObjectHRC[i0].Node[i1].MT);
                }

            if (Data.MaterialList != null)
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    RRGBAK(ref Data.MaterialList[i0].BlendColor   );
                    RK    (ref Data.MaterialList[i0].GlowIntensity);
                    RRGBAK(ref Data.MaterialList[i0].Incandescence);
                }

            if (Data.Object != null)
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    RMT(ref Data.Object[i0].MT);
                    if (Data.Object[i0].TexTrans != null)
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                        {
                            RKUV(ref Data.Object[i0].TexTrans[i1].Coverage      );
                            RKUV(ref Data.Object[i0].TexTrans[i1].Offset        );
                            RKUV(ref Data.Object[i0].TexTrans[i1].Repeat        );
                            RK  (ref Data.Object[i0].TexTrans[i1].   Rotate     );
                            RK  (ref Data.Object[i0].TexTrans[i1].   RotateFrame);
                            RKUV(ref Data.Object[i0].TexTrans[i1].TranslateFrame);
                        }
                }

            if (Data.ObjectHRC != null)
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                    if (Data.ObjectHRC[i0].Node != null)
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            RMT(ref Data.ObjectHRC[i0].Node[i1].MT);


            if (Data.Point != null)
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    RMT(ref Data.Point[i0]);

            if (Data.PostProcess != null)
            {
                PostProcess pp = Data.PostProcess.Value;
                RRGBAK(ref pp.Ambient  );
                RRGBAK(ref pp.Diffuse  );
                RRGBAK(ref pp.Specular );
                RK    (ref pp.LensFlare);
                RK    (ref pp.LensGhost);
                RK    (ref pp.LensShaft);
                Data.PostProcess = pp;
            }
        }

        public byte[] A3DCWriter()
        {
            if (a3dcOpt) usedValues = new UsedDict();
            Data._.CompressF16 = Head.Format > Format.AFT && Head.Format < Format.FT ?
                (Head.Format == Format.MGF ? CompressF16.Type2 : CompressF16.Type1) : 0;

            s = File.OpenWriter();
            for (byte i = 0; i < 2; i++)
            {
                bool ReturnToOffset = i == 1;
                s.P = 0;

                if (Data.CameraRoot != null)
                    for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                    {
                        WO(ref Data.CameraRoot[i0].      MT, ReturnToOffset);
                        WO(ref Data.CameraRoot[i0].VP.   MT, ReturnToOffset);
                        WO(ref Data.CameraRoot[i0].Interest, ReturnToOffset);
                    }

                if (Data.DOF != null)
                {
                    DOF dof = Data.DOF.Value;
                    WO(ref dof.MT, ReturnToOffset);
                    Data.DOF = dof;
                }

                if (Data.Light != null)
                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        WO(ref Data.Light[i0].Position     , ReturnToOffset);
                        WO(ref Data.Light[i0].SpotDirection, ReturnToOffset);
                    }

                if (Data.MObjectHRC != null)
                    for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        if (Data.MObjectHRC[i0].Instances != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                                WO(ref Data.MObjectHRC[i0].Instances[i1].MT, ReturnToOffset);

                        WO(ref Data.MObjectHRC[i0].MT, ReturnToOffset);

                        if (Data.MObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                WO(ref Data.MObjectHRC[i0].Node[i1].MT, ReturnToOffset);
                    }

                if (Data.Object != null)
                    for (i0 = 0; i0 < Data.Object.Length; i0++)
                        WO(ref Data.Object[i0].MT, ReturnToOffset);

                if (Data.ObjectHRC != null)
                    for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                        if (Data.ObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                                WO(ref Data.ObjectHRC[i0].Node[i1].MT, ReturnToOffset);

                if (ReturnToOffset) continue;

                if (Data.Ambient != null)
                    for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                    {
                        W(ref Data.Ambient[i0].   LightDiffuse);
                        W(ref Data.Ambient[i0].RimLightDiffuse);
                    }


                if (Data.CameraAuxiliary != null)
                {
                    CameraAuxiliary ca = Data.CameraAuxiliary.Value;
                    W(ref ca.AutoExposure);
                    W(ref ca.    Exposure);
                    W(ref ca.Gamma       );
                    W(ref ca.GammaRate   );
                    W(ref ca.Saturate    );
                    Data.CameraAuxiliary = ca;
                }

                if (Data.CameraRoot != null)
                    for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                    {
                        W(ref Data.CameraRoot[i0].      MT);
                        W(ref Data.CameraRoot[i0].VP.   MT);
                        W(ref Data.CameraRoot[i0].VP.Roll       );
                        W(ref Data.CameraRoot[i0].VP.FocalLength);
                        W(ref Data.CameraRoot[i0].VP.FOV        );
                        W(ref Data.CameraRoot[i0].Interest);
                    }

                if (Data.Chara != null)
                    for (i0 = 0; i0 < Data.Chara.Length; i0++)
                        W(ref Data.Chara[i0]);

                if (Data.Curve != null)
                    for (i0 = 0; i0 < Data.Curve.Length; i0++)
                        W(ref Data.Curve[i0].CV);

                if (Data.DOF != null && (Head.Format == Format.AFT || Head.Format == Format.FT))
                {
                    DOF dof = Data.DOF.Value;
                    W(ref dof.MT);
                    Data.DOF = dof;
                }

                if (Data.Light != null)
                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        W(ref Data.Light[i0].Position     );
                        W(ref Data.Light[i0].SpotDirection);
                    }

                if (Data.Light != null)
                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        W(ref Data.Light[i0].Ambient      );
                        W(ref Data.Light[i0].Diffuse      );
                        W(ref Data.Light[i0].Incandescence);
                        W(ref Data.Light[i0].Specular     );
                    }

                if (Data.Fog != null)
                    for (i0 = 0; i0 < Data.Fog.Length; i0++)
                    {
                        W(ref Data.Fog[i0].Density);
                        W(ref Data.Fog[i0].Diffuse);
                        W(ref Data.Fog[i0].Start  );
                        W(ref Data.Fog[i0].End    );
                    }

                if (Data.MObjectHRC != null)
                    for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        if (Data.MObjectHRC[i0].Instances != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                                W(ref Data.MObjectHRC[i0].Instances[i1].MT);

                        W(ref Data.MObjectHRC[i0].MT);

                        if (Data.MObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                W(ref Data.MObjectHRC[i0].Node[i1].MT);
                    }

                if (Data.MaterialList != null && (Head.Format == Format.X || Head.Format == Format.XHD))
                    for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                    {
                        W(ref Data.MaterialList[i0].BlendColor   );
                        W(ref Data.MaterialList[i0].GlowIntensity);
                        W(ref Data.MaterialList[i0].Incandescence);
                    }

                if (Data.Object != null)
                    for (i0 = 0; i0 < Data.Object.Length; i0++)
                    {
                        W(ref Data.Object[i0].MT);
                        if (Data.Object[i0].TexTrans != null)
                            for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                            {
                                W(ref Data.Object[i0].TexTrans[i1].Coverage      );
                                W(ref Data.Object[i0].TexTrans[i1].Offset        );
                                W(ref Data.Object[i0].TexTrans[i1].Repeat        );
                                W(ref Data.Object[i0].TexTrans[i1].   Rotate     );
                                W(ref Data.Object[i0].TexTrans[i1].   RotateFrame);
                                W(ref Data.Object[i0].TexTrans[i1].TranslateFrame);
                            }
                    }

                if (Data.ObjectHRC != null)
                    for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                        if (Data.ObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                                W(ref Data.ObjectHRC[i0].Node[i1].MT);

                if (Data.Point != null)
                    for (i0 = 0; i0 < Data.Point.Length; i0++)
                        W(ref Data.Point[i0]);

                if (Data.PostProcess != null)
                {
                    PostProcess pp = Data.PostProcess.Value;
                    W(ref pp.Ambient  );
                    W(ref pp.Diffuse  );
                    W(ref pp.Specular );
                    W(ref pp.LensFlare);
                    W(ref pp.LensShaft);
                    W(ref pp.LensGhost);
                    Data.PostProcess = pp;
                }

                s.A(0x10, true);
            }
            byte[] A3DCData = s.ToArray(); s.Dispose();
            byte[] A3DAData = A3DAWriter(true);

            s = File.OpenWriter();
            s.O = Head.Format > Format.AFT && Head.Format < Format.FT ? 0x40 : 0;
            s.P = 0x40;

            Head.StringOffset = s.P;
            Head.StringLength = A3DAData.Length;
            s.W(A3DAData);
            s.A(0x20, true);

            Head.BinaryOffset = s.P;
            Head.BinaryLength = A3DCData.Length;
            s.W(A3DCData);
            s.A(0x10, true);

            uint A3DCEnd = s.PU32;

            s.P = 0;
            s.W("#A3DC__________\n");
            s.W(0x2000);
            s.W(0x00);
            s.WE(0x20, true);
            s.W(0x10000200);
            s.W(0x50);
            s.WE(Head.StringOffset, true);
            s.WE(Head.StringLength, true);
            s.WE(0x01, true);
            s.W(0x4C42);
            s.WE(Head.BinaryOffset, true);
            s.WE(Head.BinaryLength, true);
            s.WE(0x20, true);

            if (Head.Format > Format.AFT && Head.Format < Format.FT)
            {
                s.PU32 = A3DCEnd;
                s.WEOFC(0);
                s.O = 0;
                s.P = 0;
                Header header = new Header { Signature = 0x41443341, InnerSignature = 0x01131010,
                    Format = Format.F2, DataSize = A3DCEnd, SectionSize = A3DCEnd, UseSectionSize = true };
                s.W(header, true);
            }

            byte[] data = s.ToArray();
            s.Dispose();
            return data;
        }

        private void RMT(ref ModelTransform mt)
        {
            if (mt.BinOffset == null) return;

            s.P = (int)mt.BinOffset;

            ReadOffset(out mt.Scale);
            ReadOffset(out mt.Rot  );
            ReadOffset(out mt.Trans);
            mt.Visibility = new Key { BinOffset = s.RI32() };

            RV3(ref mt.Scale     );
            RV3(ref mt.Rot       , true);
            RV3(ref mt.Trans     );
            RK (ref mt.Visibility);
        }

        private void RRGBAK(ref Vec4<Key>? rgba)
        {
            if (!rgba.HasValue) return;
            Vec4<Key> rgbav = rgba.Value;
            RK(ref rgbav.X); RK(ref rgbav.Y);
            RK(ref rgbav.Z); RK(ref rgbav.W);
            rgba = rgbav;
        }

        private void RV3(ref Vec3<Key> key, bool f16 = false)
        { RK(ref key.X, f16); RK(ref key.Y, f16); RK(ref key.Z, f16); }

        private void RKUV(ref Vec2<Key?> uv)
        { RK(ref uv.X); RK(ref uv.Y); }

        private void RK(ref Key? key)
        { if (!key.HasValue) return; Key k = key.Value; RK(ref k); key = k; }

        private void RK(ref Key key, bool f16 = false)
        {
            if (key.BinOffset == null || key.BinOffset < 0) return;

            s.P = (int)key.BinOffset;
            int Type = s.RI32();
            key.Value = s.RF32();
            key.Type = (KeyType)(Type & 0xFF);
            if (key.Type < KeyType.Linear) return;
            key.Max    = s.RF32();
            key.Length = s.RI32();
            key.EPTypePost = (EPType)((Type >> 12) & 0xF);
            key.EPTypePre  = (EPType)((Type >>  8) & 0xF);
            key.Keys = new KFT3[key.Length];

            if (f16 && Data._.CompressF16 == CompressF16.Type2)
                for (int i = 0; i < key.Keys.Length; i++)
                { ref KFT3 kf = ref key.Keys[i]; kf.F  = s.RU16(); kf.V  = s.RF16();
                                                 kf.T1 = s.RF16(); kf.T2 = s.RF16(); }
            else if (f16 && Data._.CompressF16 > 0)
                for (int i = 0; i < key.Keys.Length; i++)
                { ref KFT3 kf = ref key.Keys[i]; kf.F  = s.RU16(); kf.V  = s.RF16();
                                                 kf.T1 = s.RF32(); kf.T2 = s.RF32(); }
            else
                for (int i = 0; i < key.Keys.Length; i++)
                { ref KFT3 kf = ref key.Keys[i]; Vec4 v = s.RV4(); kf.F  = v.X; kf.V  = v.Y;
                                                                   kf.T1 = v.Z; kf.T2 = v.W; }
        }

        private void ReadOffset(out Vec3<Key> key)
        { key = new Vec3<Key> { X = new Key { BinOffset = s.RI32() },
                                   Y = new Key { BinOffset = s.RI32() },
                                   Z = new Key { BinOffset = s.RI32() }, }; }

        private void WO(ref ModelTransform mt, bool ReturnToOffset)
        {
            if (ReturnToOffset)
            {
                s.P = (int)mt.BinOffset;
                WriteOffset(mt.Scale);
                WriteOffset(mt.Rot  );
                WriteOffset(mt.Trans);
                s.W(mt.Visibility.BinOffset);
            }
            else { mt.BinOffset = s.P; s.P += 0x30; s.L += 0x30; }
        }

        private void WriteOffset(Vec3<Key> key)
        { s.W(key.X.BinOffset); s.W(key.Y.BinOffset); s.W(key.Z.BinOffset); }

        private void W(ref ModelTransform mt)
        { W(ref mt.Scale); W(ref mt.Rot, true); W(ref mt.Trans); W(ref mt.Visibility); }

        private void W(ref Vec4<Key>? rgba)
        {
            if (!rgba.HasValue) return;

            Vec4<Key> rgbav = rgba.Value;
            W(ref rgbav.X); W(ref rgbav.Y);
            W(ref rgbav.Z); W(ref rgbav.W);
            rgba = rgbav;
        }

        private void W(ref Vec3<Key> key, bool f16 = false)
        { W(ref key.X, f16); W(ref key.Y, f16); W(ref key.Z, f16); }

        private void W(ref Vec2<Key?> uv)
        { W(ref uv.X); W(ref uv.Y); }

        private void W(ref Key? key)
        { if (!key.HasValue) return; Key k = key.Value; W(ref k); key = k; }

        private void W(ref Key key, bool f16 = false)
        {
            int i = 0;
            if (key.Type > KeyType.Static && key.Keys != null)
            {
                key.BinOffset = s.P;
                int Type = (int)key.Type & 0xFF;
                Type |= ((int)key.EPTypePost & 0xF) << 12;
                Type |= ((int)key.EPTypePre  & 0xF) <<  8;
                s.W(Type);
                s.W(0x00);
                s.W((float)(key.Max ?? Data.PlayControl.Size));
                s.W(key.Keys.Length);

                if (f16 && Data._.CompressF16 == CompressF16.Type2)
                    for (i = 0; i < key.Keys.Length; i++)
                    { ref KFT3 kf = ref key.Keys[i]; s.W((ushort)kf.F ); s.W((Half)kf.V );
                                                     s.W((  Half)kf.T1); s.W((Half)kf.T2); }
                else if (f16 && Data._.CompressF16 > 0)
                    for (i = 0; i < key.Keys.Length; i++)
                    { ref KFT3 kf = ref key.Keys[i]; s.W((ushort)kf.F ); s.W((Half)kf.V );
                                                     s.W(        kf.T1); s.W(      kf.T2); }
                else
                    for (i = 0; i < key.Keys.Length; i++)
                    { ref KFT3 kf = ref key.Keys[i]; s.W(new Vec4(kf.F, kf.V, kf.T1, kf.T2)); }
            }
            else
            {
                KeyValuePair<int, float> kvp = new KeyValuePair<int, float>((int)key.Type, key.Value);
                if (!a3dcOpt)
                {
                    key.BinOffset = s.P;
                    if (key.Type == 0) s.W(0x00L);
                    else { s.W((int)key.Type); s.W((float)key.Value); }
                }
                else if (!usedValues.ContainsValue(kvp))
                {
                    key.BinOffset = s.P;
                    if (key.Type == 0) s.W(0x00L);
                    else { s.W((int)key.Type); s.W((float)key.Value); }
                    usedValues.Add(key.BinOffset.Value, kvp);
                }
                else key.BinOffset = usedValues.GK(kvp);
            }
        }

        public void A3DAMerger(ref Data mData)
        {
            if (Data.Ambient != null && mData.Ambient != null)
                for (i0 = 0; i0 < Data.Ambient.Length && i0 < mData.Ambient.Length; i0++)
                {
                    MRGBAK(ref Data.Ambient[i0].   LightDiffuse, ref mData.Ambient[i0].   LightDiffuse);
                    MRGBAK(ref Data.Ambient[i0].RimLightDiffuse, ref mData.Ambient[i0].RimLightDiffuse);
                }

            if (Data.CameraAuxiliary != null && mData.CameraAuxiliary != null)
            {
                CameraAuxiliary mCA = mData.CameraAuxiliary.Value;
                CameraAuxiliary ca = Data.CameraAuxiliary.Value;
                MK(ref ca.AutoExposure, ref mCA.AutoExposure);
                MK(ref ca.    Exposure, ref mCA.    Exposure);
                MK(ref ca.Gamma       , ref mCA.Gamma       );
                MK(ref ca.GammaRate   , ref mCA.GammaRate   );
                MK(ref ca.Saturate    , ref mCA.Saturate    );
                Data.CameraAuxiliary = ca;
                mData.CameraAuxiliary = mCA;
            }

            if (Data.CameraRoot != null && mData.CameraRoot != null)
                for (i0 = 0; i0 < Data.CameraRoot.Length && i0 < mData.CameraRoot.Length; i0++)
                {
                    MMT(ref Data.CameraRoot[i0].      MT, ref mData.CameraRoot[i0].      MT);
                    MMT(ref Data.CameraRoot[i0].Interest, ref mData.CameraRoot[i0].Interest);
                    MMT(ref Data.CameraRoot[i0].VP.   MT, ref mData.CameraRoot[i0].VP.   MT);
                    MK (ref Data.CameraRoot[i0].VP.FocalLength, ref mData.CameraRoot[i0].VP.FocalLength);
                    MK (ref Data.CameraRoot[i0].VP.FOV        , ref mData.CameraRoot[i0].VP.FOV        );
                    MK (ref Data.CameraRoot[i0].VP.Roll       , ref mData.CameraRoot[i0].VP.Roll       );
                }

            if (Data.Chara != null && mData.Chara != null)
                for (i0 = 0; i0 < Data.Chara.Length && i0 < mData.Chara.Length; i0++)
                    MMT(ref Data.Chara[i0], ref mData.Chara[i0]);

            if (Data.Curve != null && mData.Curve != null)
                for (i0 = 0; i0 < Data.Curve.Length && i0 < mData.Curve.Length; i0++)
                    MK(ref Data.Curve[i0].CV, ref mData.Curve[i0].CV);

            if (Data.DOF != null && mData.DOF != null)
            {
                DOF mDOF = mData.DOF.Value;
                DOF  dof =  Data.DOF.Value;
                MMT(ref dof.MT, ref mDOF.MT);
                Data.DOF = dof;
                mData.DOF = mDOF;
            }

            if (Data.Fog != null && mData.Fog != null)
                for (i0 = 0; i0 < Data.Fog.Length && i0 < Data.Fog.Length; i0++)
                {
                    MK    (ref Data.Fog[i0].Density, ref mData.Fog[i0].Density);
                    MRGBAK(ref Data.Fog[i0].Diffuse, ref mData.Fog[i0].Diffuse);
                    MK    (ref Data.Fog[i0].End    , ref mData.Fog[i0].End    );
                    MK    (ref Data.Fog[i0].Start  , ref mData.Fog[i0].Start  );
                }

            if (Data.Light != null && mData.Light != null)
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    MRGBAK(ref Data.Light[i0].Ambient      , ref mData.Light[i0].Ambient      );
                    MRGBAK(ref Data.Light[i0].Diffuse      , ref mData.Light[i0].Diffuse      );
                    MRGBAK(ref Data.Light[i0].Incandescence, ref mData.Light[i0].Incandescence);
                    MMT   (ref Data.Light[i0].Position     , ref mData.Light[i0].Position     );
                    MRGBAK(ref Data.Light[i0].Specular     , ref mData.Light[i0].Specular     );
                    MMT   (ref Data.Light[i0].SpotDirection, ref mData.Light[i0].SpotDirection);
                }

            if (Data.MObjectHRC != null && mData.MObjectHRC != null)
                for (i0 = 0; i0 < Data.MObjectHRC.Length && i0 < mData.MObjectHRC.Length; i0++)
                {
                    MMT(ref Data.MObjectHRC[i0].MT, ref mData.MObjectHRC[i0].MT);

                    if (Data.MObjectHRC[i0].Instances != null && mData.MObjectHRC[i0].Instances != null)
                    {
                        ref MObjectHRC.Instance[] mI = ref mData.MObjectHRC[i0].Instances;
                        ref MObjectHRC.Instance[]  i = ref  Data.MObjectHRC[i0].Instances;
                        for (i1 = 0; i1 < i.Length && i1 < mI.Length; i1++)
                            MMT(ref i[i1].MT, ref mI[i1].MT);
                    }

                    if (Data.MObjectHRC[i0].Node != null && mData.MObjectHRC[i0].Node != null)
                    {
                        ref Node[] mN = ref mData.MObjectHRC[i0].Node;
                        ref Node[]  n = ref  Data.MObjectHRC[i0].Node;
                        for (i1 = 0; i1 < n.Length && i1 < mN.Length; i1++)
                            MMT(ref n[i1].MT, ref mN[i1].MT);
                    }
                }

            if (Data.MaterialList != null && mData.MaterialList != null)
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    ref MaterialList mML = ref mData.MaterialList[i0];
                    ref MaterialList  ml = ref  Data.MaterialList[i0];
                    MRGBAK(ref ml.BlendColor   , ref mML.BlendColor   );
                    MK    (ref ml.GlowIntensity, ref mML.GlowIntensity);
                    MRGBAK(ref ml.Incandescence, ref mML.Incandescence);
                }

            if (Data.Object != null && mData.Object != null)
                for (i0 = 0; i0 < Data.Object.Length && i0 < mData.Object.Length; i0++)
                {
                    MMT(ref Data.Object[i0].MT, ref mData.Object[i0].MT);
                    if (Data.Object[i0].TexTrans != null && mData.Object[i0].TexTrans != null)
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length &&
                            i1 < mData.Object[i0].TexTrans.Length; i1++)
                        {
                            ref Object.TextureTransform mTT = ref mData.Object[i0].TexTrans[i1];
                            ref Object.TextureTransform  tt = ref  Data.Object[i0].TexTrans[i1];
                            MKUV(ref tt.Coverage      , ref mTT.Coverage      );
                            MKUV(ref tt.Offset        , ref mTT.Offset        );
                            MKUV(ref tt.Repeat        , ref mTT.Repeat        );
                            MK  (ref tt.   Rotate     , ref mTT.   Rotate     );
                            MK  (ref tt.   RotateFrame, ref mTT.   RotateFrame);
                            MKUV(ref tt.TranslateFrame, ref mTT.TranslateFrame);
                        }
                }

            if (Data.ObjectHRC != null && mData.ObjectHRC != null)
                for (i0 = 0; i0 < Data.ObjectHRC.Length && i0 < mData.ObjectHRC.Length; i0++)
                    if (Data.ObjectHRC[i0].Node != null && mData.ObjectHRC[i0].Node != null)
                    {
                        ref Node[] mN = ref mData.ObjectHRC[i0].Node;
                        ref Node[]  n = ref  Data.ObjectHRC[i0].Node;
                        for (i1 = 0; i1 < n.Length && i1 < mN.Length; i1++)
                            MMT(ref n[i1].MT, ref mN[i1].MT);
                    }


            if (Data.Point != null && mData.Point != null)
                for (i0 = 0; i0 < Data.Point.Length && i0 < mData.Point.Length; i0++)
                    MMT(ref Data.Point[i0], ref mData.Point[i0]);

            if (Data.PostProcess != null && mData.PostProcess != null)
            {
                PostProcess mPP = mData.PostProcess.Value;
                PostProcess  pp =  Data.PostProcess.Value;
                MRGBAK(ref pp.Ambient  , ref mPP.Ambient  );
                MRGBAK(ref pp.Diffuse  , ref mPP.Diffuse  );
                MRGBAK(ref pp.Specular , ref mPP.Specular );
                MK    (ref pp.LensFlare, ref mPP.LensFlare);
                MK    (ref pp.LensGhost, ref mPP.LensGhost);
                MK    (ref pp.LensShaft, ref mPP.LensShaft);
                Data.PostProcess = pp;
                mData.PostProcess = mPP;
            }
        }

        private void MMT(ref ModelTransform mt, ref ModelTransform mMT)
        {
            MV3(ref mt.Scale     , ref mMT.Scale     );
            MV3(ref mt.Rot       , ref mMT.Rot       );
            MV3(ref mt.Trans     , ref mMT.Trans     );
            MK (ref mt.Visibility, ref mMT.Visibility);
        }

        private void MRGBAK(ref Vec4<Key>? rgba, ref Vec4<Key>? mRGBA)
        {
            if (!rgba.HasValue) return;

            Vec4<Key> rgbav = rgba.Value;
            Vec4<Key> mrgbav = mRGBA.Value;
            MK(ref rgbav.X, ref mrgbav.X); MK(ref rgbav.Y, ref mrgbav.Y);
            MK(ref rgbav.Z, ref mrgbav.Z); MK(ref rgbav.W, ref mrgbav.W);
            mRGBA = mrgbav;
        }

        private void MV3(ref Vec3<Key> key, ref Vec3<Key> mKey)
        { MK(ref key.X, ref mKey.X); MK(ref key.Y, ref mKey.Y); MK(ref key.Z, ref mKey.Z); }

        private void MKUV(ref Vec2<Key?> uv, ref Vec2<Key?> mUV)
        { MK(ref uv.X, ref mUV.X); MK(ref uv.Y, ref mUV.Y); }

        private void MK(ref Key? key, ref Key? mKey)
        { if (!key.HasValue || !mKey.HasValue) return; Key k = key.Value; Key mk = mKey.Value; MK(ref k, ref mk); key = k; mKey = mk; }

        private void MK(ref Key key, ref Key mKey)
        {
            if (key.Keys == null && mKey.Keys != null)
                return;

            if (key.Keys != null && mKey.Keys == null)
                return;

            if (key.Keys == null || mKey.Keys == null)
                return;

            if (key.Keys.Length >= 1 && mKey.Keys.Length == 1)
            {
                key.Length = key.Keys.Length;
                key.Max = (int)key.Keys[key.Length - 1].F + 1;
                return;
            }
            else if (key.Keys.Length == 1 && mKey.Keys.Length > 1)
            {
                if (key.Keys[0] == mKey.Keys[0])
                {
                    key.Length = mKey.Keys.Length;
                    key.Keys = mKey.Keys;
                    key.Max = (int)mKey.Keys[key.Length - 1].F + 1;
                    return;
                }
                return;
            }

            int i = key.Keys.Length;
            bool found = false;
            while (i > 1 && !found)
                if (key.Keys[--i] == mKey.Keys[0])
                    found = true;

            if (!found) return;

            int OldLength = i;
            Array.Resize(ref key.Keys, OldLength + mKey.Keys.Length);
            Array.Copy(mKey.Keys, 0, key.Keys, OldLength, mKey.Keys.Length);
            key.Length = key.Keys.Length;
            key.Max = (int)key.Keys[key.Length - 1].F + 1;
        }

        public void MsgPackReader(string file, bool json)
        {
            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            MsgPack a3d = msgPack["A3D"];
            if (a3d.NotNull) MsgPackReader(a3d);
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json) =>
            MsgPackWriter().Write(false, true, file, json);

        private void MsgPackReader(MsgPack a3d)
        {
            MsgPack temp = MsgPack.New, temp1 = MsgPack.New;
            if ((temp = a3d["_"]).NotNull)
            {
                Data._ = new _
                {
                    ConverterVersion = temp.RS("ConverterVersion"),
                    FileName         = temp.RS("FileName"        ),
                    PropertyVersion  = temp.RS("PropertyVersion" ),
                };
            }

            if ((temp = a3d["Ambient", true]).NotNull)
            {
                Data.Ambient = new Ambient[temp.Array.Length];

                for (i = 0; i < Data.Ambient.Length; i++)
                    Data.Ambient[i] = new Ambient
                    {
                                   Name = temp[i].RS    (           "Name"),
                           LightDiffuse = temp[i].RRGBAK(   "LightDiffuse"),
                        RimLightDiffuse = temp[i].RRGBAK("RimLightDiffuse"),
                    };
            }

            if ((temp = a3d["CameraAuxiliary"]).NotNull)
                Data.CameraAuxiliary = new CameraAuxiliary
                {
                    AutoExposure = temp.RKN("AutoExposure"),
                        Exposure = temp.RKN(    "Exposure"),
                    Gamma        = temp.RKN("Gamma"       ),
                    GammaRate    = temp.RKN("GammaRate"   ),
                    Saturate     = temp.RKN("Saturate"    ),
                };

            if ((temp = a3d["CameraRoot", true]).NotNull)
            {
                Data.CameraRoot = new CameraRoot[temp.Array.Length];
                for (i = 0; i < Data.CameraRoot.Length; i++)
                {
                    Data.CameraRoot[i] = new CameraRoot
                    {
                        MT       = temp[i].RMT(),
                        Interest = temp[i].RMT("Interest"),
                    };

                    if ((temp1 = temp[i]["ViewPoint"]).IsNull) continue;
                    Data.CameraRoot[i].VP = new CameraRoot.ViewPoint
                    {
                        MT              = temp1.RMT(),
                        Aspect          = temp1. RF32("Aspect"         ),
                        CameraApertureH = temp1.RnF32("CameraApertureH"),
                        CameraApertureW = temp1.RnF32("CameraApertureW"),
                        FOVHorizontal   = temp1.RB   ("FOVHorizontal"  ),
                        FocalLength     = temp1.RK   ("FocalLength"    ),
                        FOV             = temp1.RK   ("FOV"            ),
                        Roll            = temp1.RK   ("Roll"           ),
                    };
                }
            }

            if ((temp = a3d["Chara", true]).NotNull)
            {
                Data.Chara = new ModelTransform[temp.Array.Length];
                for (i = 0; i < Data.Chara.Length; i++)
                    Data.Chara[i] = temp[i].RMT();
            }

            if ((temp = a3d["Curve", true]).NotNull)
            {
                Data.Curve = new Curve[temp.Array.Length];
                for (i = 0; i < Data.Curve.Length; i++)
                    Data.Curve[i] = new Curve
                    {
                        Name = temp[i].RS("Name"),
                        CV   = temp[i].RK("CV"  ),
                    };
            }

            if ((temp = a3d["DOF"]).NotNull)
                Data.DOF = new DOF
                {
                    MT   = temp.RMT(),
                    Name = temp.RS("Name"),
                };

            if ((temp = a3d["Event", true]).NotNull)
            {
                Data.Event = new Event[temp.Array.Length];
                for (i = 0; i < Data.Event.Length; i++)
                    Data.Event[i] = new Event
                    {
                            Begin    = temp[i].RnF32(    "Begin"   ),
                        ClipBegin    = temp[i].RnF32("ClipBegin"   ),
                        ClipEnd      = temp[i].RnF32("ClipEnd"     ),
                            End      = temp[i].RnF32(    "End"     ),
                        Name         = temp[i].RS   ("Name"        ),
                        Param1       = temp[i].RS   ("Param1"      ),
                        Ref          = temp[i].RS   ("Ref"         ),
                        TimeRefScale = temp[i].RnF32("TimeRefScale"),
                        Type         = temp[i].RnI32("Type"        ),
                    };
            }

            if ((temp = a3d["Fog", true]).NotNull)
            {
                Data.Fog = new Fog[temp.Array.Length];
                for (i = 0; i < Data.Fog.Length; i++)
                    Data.Fog[i] = new Fog
                    {
                        Id      = temp[i].RnI32 ("Id"     ),
                        Density = temp[i].RK    ("Density"),
                        Diffuse = temp[i].RRGBAK("Diffuse"),
                        End     = temp[i].RK    ("End"    ),
                        Start   = temp[i].RK    ("Start"  ),
                    };
            }

            if ((temp = a3d["Light", true]).NotNull)
            {
                Data.Light = new Light[temp.Array.Length];
                for (i = 0; i < Data.Light.Length; i++)
                    Data.Light[i] = new Light
                    {
                        Id            = temp[i].RnI32 ("Id"           ),
                        Name          = temp[i].RS    ("Name"         ),
                        Type          = temp[i].RS    ("Type"         ),
                        Ambient       = temp[i].RRGBAK("Ambient"      ),
                        Diffuse       = temp[i].RRGBAK("Diffuse"      ),
                        Incandescence = temp[i].RRGBAK("Incandescence"),
                        Position      = temp[i].RMT   ("Position"     ),
                        Specular      = temp[i].RRGBAK("Specular"     ),
                        SpotDirection = temp[i].RMT   ("SpotDirection"),
                    };
            }

            if ((temp = a3d["MaterialList", true]).NotNull)
            {
                Data.MaterialList = new MaterialList[temp.Array.Length];
                for (i = 0; i < Data.MaterialList.Length; i++)
                    Data.MaterialList[i] = new MaterialList
                    {
                        HashName      = temp[i].RS    (     "HashName"),
                            Name      = temp[i].RS    (         "Name"),
                        BlendColor    = temp[i].RRGBAK(   "BlendColor"),
                        GlowIntensity = temp[i].RK    ("GlowIntensity"),
                        Incandescence = temp[i].RRGBAK("Incandescence"),
                    };
            }

            if ((temp = a3d["MObjectHRC", true]).NotNull)
            {
                Data.MObjectHRC = new MObjectHRC[temp.Array.Length];
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    Data.MObjectHRC[i0] = new MObjectHRC
                    {
                        MT   = temp[i0].RMT(),
                        Name = temp[i0].RS("Name"),
                    };

                    if ((temp1 = temp[i0]["JointOrient"]).NotNull)
                        Data.MObjectHRC[i0].JointOrient = new Vec3<float?>
                        {
                            X = temp1.RnF32("X"),
                            Y = temp1.RnF32("Y"),
                            Z = temp1.RnF32("Z"),
                        };

                    if ((temp1 = temp[i0]["Instance", true]).NotNull)
                    {
                        Data.MObjectHRC[i0].Instances = new MObjectHRC.Instance[temp1.Array.Length];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                            Data.MObjectHRC[i0].Instances[i1] = new MObjectHRC.Instance
                            {
                                     MT = temp1[i1].RMT(),
                                   Name = temp1[i1].RS   (   "Name"),
                                 Shadow = temp1[i1].RnI32( "Shadow"),
                                UIDName = temp1[i1].RS   ("UIDName"),
                            };
                    }

                    if ((temp1 = temp[i0]["Node", true]).NotNull)
                    {
                        Data.MObjectHRC[i0].Node = new Node[temp1.Array.Length];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            Data.MObjectHRC[i0].Node[i1] = new Node
                            {
                                    MT = temp1[i1].RMT(),
                                  Name = temp1[i1].RS   (  "Name"),
                                Parent = temp1[i1].RnI32("Parent"),
                            };
                    }
                }
            }

            if ((temp = a3d["MObjectHRCList", true]).NotNull)
            {
                Data.MObjectHRCList = new string[temp.Array.Length];
                for (i = 0; i < Data.MObjectHRCList.Length; i++)
                    Data.MObjectHRCList[i] = temp[i].RS();
            }

            if ((temp = a3d["Motion", true]).NotNull)
            {
                Data.Motion = new string[temp.Array.Length];
                for (i = 0; i < Data.Motion.Length; i++)
                    Data.Motion[i] = temp[i].RS();
            }

            if ((temp = a3d["Object", true]).NotNull)
            {
                Data.Object = new Object[temp.Array.Length];
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    Data.Object[i0] = new Object
                    {
                                 MT = temp[i0].RMT(),
                        Morph       = temp[i0].RS   ("Morph"      ),
                        MorphOffset = temp[i0].RnI32("MorphOffset"),
                               Name = temp[i0].RS   (       "Name"),
                         ParentName = temp[i0].RS   ( "ParentName"),
                            UIDName = temp[i0].RS   (    "UIDName"),
                    };

                    if ((temp1 = temp[i0]["TexturePattern", true]).NotNull)
                    {
                        Data.Object[i0].TexPat = new Object.TexturePattern[temp1.Array.Length];
                        for (i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                            Data.Object[i0].TexPat[i1] = new Object.TexturePattern
                            {
                                Name      = temp1[i1].RS   ("Name"     ),
                                Pat       = temp1[i1].RS   ("Pat"      ),
                                PatOffset = temp1[i1].RnI32("PatOffset"),
                            };
                    }

                    if ((temp1 = temp[i0]["TextureTransform", true]).NotNull)
                    {
                        Data.Object[i0].TexTrans = new Object.TextureTransform[temp1.Array.Length];
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                            Data.Object[i0].TexTrans[i1] = new Object.TextureTransform
                            {
                                Name           = temp1[i1].RS  ("Name"          ),
                                Coverage       = temp1[i1].RKUV("Coverage"      ),
                                Offset         = temp1[i1].RKUV("Offset"        ),
                                Repeat         = temp1[i1].RKUV("Repeat"        ),
                                   Rotate      = temp1[i1].RKN (   "Rotate"     ),
                                   RotateFrame = temp1[i1].RKN (   "RotateFrame"),
                                TranslateFrame = temp1[i1].RKUV("TranslateFrame"),
                            };
                    }
                }
            }

            if ((temp = a3d["ObjectHRC", true]).NotNull)
            {
                Data.ObjectHRC = new ObjectHRC[temp.Array.Length];
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    Data.ObjectHRC[i0] = new ObjectHRC
                    {
                           Name = temp[i0].RS (   "Name"),
                         Shadow = temp[i0].RnB( "Shadow"),
                        UIDName = temp[i0].RS ("UIDName"),
                    };

                    if ((temp1 = temp[i0]["JointOrient"]).NotNull)
                        Data.ObjectHRC[i0].JointOrient = new Vec3<float?>
                        {
                            X = temp1.RF32("X"),
                            Y = temp1.RF32("Y"),
                            Z = temp1.RF32("Z"),
                        };

                    if ((temp1 = temp[i0]["Node", true]).NotNull)
                    {
                        Data.ObjectHRC[i0].Node = new Node[temp1.Array.Length];
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            Data.ObjectHRC[i0].Node[i1] = new Node
                            {
                                    MT = temp1[i1].RMT(),
                                  Name = temp1[i1].RS  (  "Name"),
                                Parent = temp1[i1].RI32("Parent"),
                            };
                    }
                }
            }

            if ((temp = a3d["ObjectHRCList", true]).NotNull)
            {
                Data.ObjectHRCList = new string[temp.Array.Length];
                for (i = 0; i < Data.ObjectHRCList.Length; i++)
                    Data.ObjectHRCList[i] = temp[i].RS();
            }

            if ((temp = a3d["ObjectList", true]).NotNull)
            {
                Data.ObjectList = new string[temp.Array.Length];
                for (i = 0; i < Data.ObjectList.Length; i++)
                    Data.ObjectList[i] = temp[i].RS();
            }

            if ((temp = a3d["PlayControl"]).NotNull)
                Data.PlayControl = new PlayControl
                {
                    Begin  = temp. RI32("Begin" ),
                    Div    = temp.RnI32("Div"   ),
                    FPS    = temp. RI32("FPS"   ),
                    Offset = temp.RnI32("Offset"),
                    Size   = temp. RI32("Size"  ),
                };

            if ((temp = a3d["Point", true]).NotNull)
            {
                Data.Point = new ModelTransform[temp.Array.Length];
                for (i = 0; i < Data.Point.Length; i++)
                    Data.Point[i] = temp[i].RMT();
            }

            if ((temp = a3d["PostProcess"]).NotNull)
                Data.PostProcess = new PostProcess
                {
                    Ambient   = temp.RRGBAK("Ambient"  ),
                    Diffuse   = temp.RRGBAK("Diffuse"  ),
                    LensFlare = temp.RKN   ("LensFlare"),
                    LensGhost = temp.RKN   ("LensGhost"),
                    LensShaft = temp.RKN   ("LensShaft"),
                    Specular  = temp.RRGBAK("Specular" ),
                };

            temp1.Dispose();
            temp.Dispose();
        }

        private MsgPack MsgPackWriter()
        {
            MsgPack a3d = new MsgPack("A3D")
                .Add(new MsgPack("_").Add("ConverterVersion", Data._.ConverterVersion)
                                     .Add("FileName"        , Data._.FileName        )
                                     .Add("PropertyVersion" , Data._.PropertyVersion ));

            if (Data.Ambient != null)
            {
                MsgPack ambient = new MsgPack(Data.Ambient.Length, "Ambient");
                for (i = 0; i < Data.Ambient.Length; i++)
                    ambient[i] = MsgPack.New.Add("Name", Data.Ambient[i].Name)
                                            .Add(   "LightDiffuse", ref Data.Ambient[i].   LightDiffuse)
                                            .Add("RimLightDiffuse", ref Data.Ambient[i].RimLightDiffuse);
                a3d.Add(ambient);
            }

            if (Data.CameraAuxiliary != null)
            {
                CameraAuxiliary ca = Data.CameraAuxiliary.Value;
                MsgPack cameraAuxiliary = new MsgPack("CameraAuxiliary")
                    .Add("AutoExposure", ref ca.AutoExposure)
                    .Add(    "Exposure", ref ca.    Exposure)
                    .Add("Gamma"       , ref ca.Gamma       )
                    .Add("GammaRate"   , ref ca.GammaRate   )
                    .Add("Saturate"    , ref ca.Saturate    );
                a3d.Add(cameraAuxiliary);
                Data.CameraAuxiliary = ca;
            }

            if (Data.CameraRoot != null)
            {
                MsgPack cameraRoot = new MsgPack(Data.CameraRoot.Length, "CameraRoot");
                for (i = 0; i < Data.CameraRoot.Length; i++)
                    cameraRoot[i] = MsgPack.New
                        .Add("Interest", ref Data.CameraRoot[i].Interest)
                        .Add(new MsgPack("ViewPoint")
                        .Add("Aspect"         ,     Data.CameraRoot[i].VP.Aspect         )
                        .Add("CameraApertureH",     Data.CameraRoot[i].VP.CameraApertureH)
                        .Add("CameraApertureW",     Data.CameraRoot[i].VP.CameraApertureW)
                        .Add("FOVHorizontal"  ,     Data.CameraRoot[i].VP.FOVHorizontal  )
                        .Add("FocalLength"    , ref Data.CameraRoot[i].VP.FocalLength    )
                        .Add("FOV"            , ref Data.CameraRoot[i].VP.FOV            )
                        .Add("Roll"           , ref Data.CameraRoot[i].VP.Roll           )
                        .Add(ref Data.CameraRoot[i].VP.MT))
                        .Add(ref Data.CameraRoot[i].   MT);
                a3d.Add(cameraRoot);
            }

            if (Data.Chara != null)
            {
                MsgPack chara = new MsgPack(Data.Chara.Length, "Chara");
                for (i = 0; i < Data.Chara.Length; i++) chara[i] = MsgPack.New.Add(ref Data.Chara[i]);
                a3d.Add(chara);
            }

            if (Data.Curve != null)
            {
                MsgPack curve = new MsgPack(Data.Curve.Length, "Curve");
                for (i = 0; i < Data.Curve.Length; i++)
                    curve[i] = MsgPack.New.Add("Name", Data.Curve[i].Name)
                                          .Add("CV", ref Data.Curve[i].CV);
                a3d.Add(curve);
            }

            if (Data.DOF != null)
            {
                DOF dof = Data.DOF.Value;
                a3d.Add(new MsgPack("DOF").Add("Name", dof.Name).Add(ref dof.MT));
                Data.DOF = dof;
            }

            if (Data.Event != null)
            {
                MsgPack @event = new MsgPack(Data.Event.Length, "Events");
                for (i = 0; i < Data.Event.Length; i++)
                    @event[i] = MsgPack.New.Add("Begin"       , Data.Event[i].Begin       )
                                           .Add("ClipBegin"   , Data.Event[i].ClipBegin   )
                                           .Add("ClipEnd"     , Data.Event[i].ClipEnd     )
                                           .Add("End"         , Data.Event[i].End         )
                                           .Add("Name"        , Data.Event[i].Name        )
                                           .Add("Param1"      , Data.Event[i].Param1      )
                                           .Add("Ref"         , Data.Event[i].Ref         )
                                           .Add("TimeRefScale", Data.Event[i].TimeRefScale)
                                           .Add("Type"        , Data.Event[i].Type        );
                a3d.Add(@event);
            }

            if (Data.Fog != null)
            {
                MsgPack fog = new MsgPack(Data.Fog.Length, "Fog");
                for (i = 0; i < Data.Fog.Length; i++)
                    fog[i] = MsgPack.New.Add("Id"     , Data.Fog[i].Id     )
                                        .Add("Density", ref Data.Fog[i].Density)
                                        .Add("Diffuse", ref Data.Fog[i].Diffuse)
                                        .Add("End"    , ref Data.Fog[i].End    )
                                        .Add("Start"  , ref Data.Fog[i].Start  );
                a3d.Add(fog);
            }

            if (Data.Light != null)
            {
                MsgPack light = new MsgPack(Data.Light.Length, "Light");
                for (i = 0; i < Data.Light.Length; i++)
                    light[i] = MsgPack.New.Add("Id"  , Data.Light[i].Id)
                                          .Add("Name", Data.Light[i].Name)
                                          .Add("Type", Data.Light[i].Type)
                                          .Add("Ambient"      , ref Data.Light[i].Ambient      )
                                          .Add("Diffuse"      , ref Data.Light[i].Diffuse      )
                                          .Add("Incandescence", ref Data.Light[i].Incandescence)
                                          .Add("Position"     , ref Data.Light[i].Position     )
                                          .Add("Specular"     , ref Data.Light[i].Specular     )
                                          .Add("SpotDirection", ref Data.Light[i].SpotDirection);
                a3d.Add(light);
            }

            if (Data.MObjectHRC != null)
            {
                MsgPack mObjectHRC = new MsgPack(Data.MObjectHRC.Length, "MObjectHRC");
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    MsgPack _mObjectHRC = MsgPack.New.Add("Name", Data.MObjectHRC[i0].Name);

                    if (Data.MObjectHRC[i0].JointOrient.NotNull &&
                        (Head.Format == Format.X || Head.Format == Format.XHD))
                        _mObjectHRC.Add(new MsgPack("JointOrient")
                            .Add("X", Data.MObjectHRC[i0].JointOrient.X)
                            .Add("Y", Data.MObjectHRC[i0].JointOrient.Y)
                            .Add("Z", Data.MObjectHRC[i0].JointOrient.Z));

                    if (Data.MObjectHRC[i0].Instances != null)
                    {
                        MsgPack instance = new MsgPack(Data.MObjectHRC[i0].Instances.Length, "Instance");
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                            instance[i1] = MsgPack.New.Add(ref Data.MObjectHRC[i0].Instances[i1].MT)
                                .Add(   "Name", Data.MObjectHRC[i0].Instances[i1].   Name)
                                .Add("Shadow" , Data.MObjectHRC[i0].Instances[i1].Shadow )
                                .Add("UIDName", Data.MObjectHRC[i0].Instances[i1].UIDName);
                        _mObjectHRC.Add(instance);
                    }

                    if (Data.MObjectHRC[i0].Node != null)
                    {
                        MsgPack node = new MsgPack(Data.MObjectHRC[i0].Node.Length, "Node");
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            node[i1] = MsgPack.New
                                .Add("Name"  , Data.MObjectHRC[i0].Node[i1].Name  )
                                .Add("Parent", Data.MObjectHRC[i0].Node[i1].Parent)
                                .Add(ref Data.MObjectHRC[i0].Node[i1].MT);
                        _mObjectHRC.Add(node);
                    }

                    mObjectHRC[i0] = _mObjectHRC.Add(ref Data.MObjectHRC[i0].MT);
                }
                a3d.Add(mObjectHRC);
            }

            if (Data.MObjectHRCList != null)
            {
                MsgPack mObjectHRCList = new MsgPack(Data.MObjectHRCList.Length, "MObjectHRCList");
                for (i = 0; i < Data.MObjectHRCList.Length; i++)
                    mObjectHRCList[i] = Data.MObjectHRCList[i];
                a3d.Add(mObjectHRCList);
            }

            if (Data.MaterialList != null)
            {
                MsgPack materialList = new MsgPack(Data.MaterialList.Length, "MaterialList");
                for (i = 0; i < Data.MaterialList.Length; i++)
                    materialList[i] = new MsgPack("Material")
                        .Add("HashName"     , Data.MaterialList[i].HashName     )
                        .Add(    "Name"     , Data.MaterialList[i].    Name     )
                        .Add("BlendColor"   , ref Data.MaterialList[i].BlendColor   )
                        .Add("GlowIntensity", ref Data.MaterialList[i].GlowIntensity)
                        .Add("Incandescence", ref Data.MaterialList[i].Incandescence);
                a3d.Add(materialList);
            }

            if (Data.Motion != null)
            {
                MsgPack motion = new MsgPack(Data.Motion.Length, "Motion");
                for (i = 0; i < Data.Motion.Length; i++) motion[i] = Data.Motion[i];
                a3d.Add(motion);
            }

            if (Data.Object != null)
            {
                MsgPack @object = new MsgPack(Data.Object.Length, "Object");
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    MsgPack _object = MsgPack.New.Add("Morph"      , Data.Object[i0].Morph      )
                                                 .Add("MorphOffset", Data.Object[i0].MorphOffset)
                                                 .Add(      "Name" , Data.Object[i0].      Name )
                                                 .Add("ParentName" , Data.Object[i0].ParentName )
                                                 .Add(   "UIDName" , Data.Object[i0].   UIDName );
                    if (Data.Object[i0].TexPat != null)
                    {
                        MsgPack texPat = new MsgPack(Data.Object[i0].TexPat.Length, "TexturePattern");
                        for (i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                            texPat[i1] = MsgPack.New
                                .Add("Name"     , Data.Object[i0].TexPat[i1].Name     )
                                .Add("Pat"      , Data.Object[i0].TexPat[i1].Pat      )
                                .Add("PatOffset", Data.Object[i0].TexPat[i1].PatOffset);
                        _object.Add(texPat);
                    }
                    if (Data.Object[i0].TexTrans != null)
                    {
                        MsgPack texTrans = new MsgPack(Data.Object[i0]
                            .TexTrans.Length, "TextureTransform");
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                            texTrans[i1] = MsgPack.New
                                .Add("Name", Data.Object[i0].TexTrans[i1].Name)
                                .Add("Coverage"      , ref Data.Object[i0].TexTrans[i1].Coverage      )
                                .Add("Offset"        , ref Data.Object[i0].TexTrans[i1].Offset        )
                                .Add("Repeat"        , ref Data.Object[i0].TexTrans[i1].Repeat        )
                                .Add(   "Rotate"     , ref Data.Object[i0].TexTrans[i1].Rotate        )
                                .Add(   "RotateFrame", ref Data.Object[i0].TexTrans[i1].   RotateFrame)
                                .Add("TranslateFrame", ref Data.Object[i0].TexTrans[i1].TranslateFrame);
                        _object.Add(texTrans);
                    }
                    @object[i0] = _object.Add(ref Data.Object[i0].MT);
                }
                a3d.Add(@object);
            }

            if (Data.ObjectHRC != null)
            {
                MsgPack objectHRC = new MsgPack(Data.ObjectHRC.Length, "ObjectHRC");
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    MsgPack _objectHRC = MsgPack.New.Add(   "Name", Data.ObjectHRC[i0].   Name)
                                                    .Add("Shadow" , Data.ObjectHRC[i0].Shadow )
                                                    .Add("UIDName", Data.ObjectHRC[i0].UIDName);

                    if (Data.ObjectHRC[i0].JointOrient.NotNull &&
                        (Head.Format == Format.X || Head.Format == Format.XHD))
                        _objectHRC.Add(new MsgPack("JointOrient")
                            .Add("X", Data.ObjectHRC[i0].JointOrient.X)
                            .Add("Y", Data.ObjectHRC[i0].JointOrient.Y)
                            .Add("Z", Data.ObjectHRC[i0].JointOrient.Z));

                    if (Data.ObjectHRC[i0].Node != null)
                    {
                        MsgPack node = new MsgPack(Data.ObjectHRC[i0].Node.Length, "Node");
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            node[i1] = MsgPack.New
                                .Add("Name"  , Data.ObjectHRC[i0].Node[i1].Name  )
                                .Add("Parent", Data.ObjectHRC[i0].Node[i1].Parent)
                                .Add(ref Data.ObjectHRC[i0].Node[i1].MT);
                        _objectHRC.Add(node);
                    }
                    objectHRC[i0] = _objectHRC;
                }
                a3d.Add(objectHRC);
            }

            if (Data.ObjectHRCList != null)
            {
                MsgPack objectHRCList = new MsgPack(Data.ObjectHRCList.Length, "ObjectHRCList");
                for (i = 0; i < Data.ObjectHRCList.Length; i++) objectHRCList[i] = Data.ObjectHRCList[i];
                a3d.Add(objectHRCList);
            }

            if (Data.ObjectList != null)
            {
                MsgPack objectList = new MsgPack(Data.ObjectList.Length, "ObjectList");
                for (i = 0; i < Data.ObjectList.Length; i++) objectList[i] = Data.ObjectList[i];
                a3d.Add(objectList);
            }

            a3d.Add(new MsgPack("PlayControl")
                .Add("Begin" , Data.PlayControl.Begin )
                .Add("Div"   , Data.PlayControl.Div   )
                .Add("FPS"   , Data.PlayControl.FPS   )
                .Add("Offset", Data.PlayControl.Offset)
                .Add("Size"  , Data.PlayControl.Size  ));

            if (Data.Point != null)
            {
                MsgPack point = new MsgPack(Data.Point.Length, "Point");
                for (i = 0; i < Data.Point.Length; i++)
                    point[i] = MsgPack.New.Add(ref Data.Point[i]);
                a3d.Add(point);
            }

            if (Data.PostProcess != null)
            {
                PostProcess pp = Data.PostProcess.Value;
                a3d.Add(new MsgPack("PostProcess").Add("Ambient"  , ref pp.Ambient  )
                                                  .Add("Diffuse"  , ref pp.Diffuse  )
                                                  .Add("LensFlare", ref pp.LensFlare)
                                                  .Add("LensGhost", ref pp.LensGhost)
                                                  .Add("LensShaft", ref pp.LensShaft)
                                                  .Add("Specular" , ref pp.Specular ));
                Data.PostProcess = pp;
            }
            return a3d;
        }

        public void Dispose()
        {
            i = i0 = i1 = soi0 = soi1 = 0;
            so0 = so1 = null;
            name = nameView = value = null;
            dataArray = null;
            usedValues = null;
            dict = null;
            s = null;
            Data = default;
            Head = default;
        }
    }

    public static class A3DAExt
    {
        public static ModelTransform RMT(this MsgPack msgPack, string name) =>
            msgPack[name].RMT();

        public static ModelTransform RMT(this MsgPack msgPack) =>
            new ModelTransform { Rot   = msgPack.RV3("Rot"  ), Scale      = msgPack.RV3("Scale"     ),
                                 Trans = msgPack.RV3("Trans"), Visibility = msgPack.RK ("Visibility") };

        public static Vec4<Key> RRGBAK(this MsgPack msgPack, string name) =>
            msgPack[name].RRGBAK();

        public static Vec4<Key> RRGBAK(this MsgPack msgPack) =>
            new Vec4<Key> { X = msgPack.RK("R"), Y = msgPack.RK("G"),
                            Z = msgPack.RK("B"), W = msgPack.RK("A") };

        public static Vec3<Key> RV3(this MsgPack msgPack, string name) =>
            msgPack[name].RV3();

        public static Vec3<Key> RV3(this MsgPack msgPack) =>
            new Vec3<Key> { X = msgPack.RK("X"), Y = msgPack.RK("Y"), Z = msgPack.RK("Z") };

        public static Vec2<Key?> RKUV(this MsgPack msgPack, string name) =>
            msgPack[name].RKUV();

        public static Vec2<Key?> RKUV(this MsgPack msgPack) =>
            new Vec2<Key?> { X = msgPack.RKN("U"), Y = msgPack.RKN("V") };

        public static Key? RKN(this MsgPack msgPack, string name) =>
            msgPack[name].Object != null ? (Key?)msgPack[name].RK() : default;

        public static Key RK(this MsgPack msgPack, string name) =>
            msgPack[name].RK();

        public static Key RK(this MsgPack msgPack)
        {
            if (msgPack.Object == null) return default;

            Key key = default;
            if (Enum.TryParse(msgPack.RS("EPTypePost"), out EPType epTypePost)) key.EPTypePost = epTypePost;
            if (Enum.TryParse(msgPack.RS("EPTypePre" ), out EPType epTypePre )) key.EPTypePre  = epTypePre;
            if (!Enum.TryParse(msgPack.RS("Type"), out KeyType keyType))
            {
                string type = msgPack.RS("Type");
                if (type == null || type == "")  { key.Value = 0; return key; }
                {
                         if (type == "Null" ) keyType = KeyType.None;
                    else if (type == "Value") keyType = KeyType.Static;
                    else if (type == "Lerp" ) keyType = KeyType.Linear;
                }
            }
            key.Max = msgPack.RnF32("Max");
            key.Value = msgPack.RF32("Value");
            key.Type = keyType;
            if (key.Type == 0) { key.Value = 0; return key; }
            else if (key.Type < KeyType.Linear) return key;

            if (msgPack.RB("RawData")) key.RawData = new Key.RawD() { KeyType = -1, ValueType = "float" };
            MsgPack trans;
            if ((trans = msgPack["Trans", true]).IsNull) return key;

            key.Length = trans.Array.Length;
            key.Keys = new KFT3[key.Length];
            for (int i = 0; i < key.Length; i++)
            {
                     if (trans[i].Array == null)     continue;
                else if (trans[i].Array.Length == 0) continue;
                else if (trans[i].Array.Length == 1)
                    key.Keys[i] = new KFT3
                        (trans[i][0].RF32());
                else if (trans[i].Array.Length == 2)
                    key.Keys[i] = new KFT3
                        (trans[i][0].RF32(), trans[i][1].RF32());
                else if (trans[i].Array.Length == 3)
                    key.Keys[i] = new KFT3
                        (trans[i][0].RF32(), trans[i][1].RF32(), trans[i][2].RF32());
                else if (trans[i].Array.Length == 4)
                    key.Keys[i] = new KFT3
                        (trans[i][0].RF32(), trans[i][1].RF32(), trans[i][2].RF32(), trans[i][3].RF32());
            }
            return key;
        }

        public static MsgPack Add(this MsgPack msgPack, ref ModelTransform mt) =>
            msgPack.Add("Rot"       , ref mt.Rot       )
                   .Add("Scale"     , ref mt.Scale     )
                   .Add("Trans"     , ref mt.Trans     )
                   .Add("Visibility", ref mt.Visibility);

        public static MsgPack Add(this MsgPack msgPack, string name, ref ModelTransform mt) =>
            msgPack.Add(new MsgPack(name).Add("Rot"       , ref mt.Rot       )
                                         .Add("Scale"     , ref mt.Scale     )
                                         .Add("Trans"     , ref mt.Trans     )
                                         .Add("Visibility", ref mt.Visibility));

        public static MsgPack Add(this MsgPack msgPack, string name, ref Vec4<Key>? rgba)
        {
            if (!rgba.HasValue) return msgPack;

            Vec4<Key> rgbav = rgba.Value;
            msgPack = msgPack.Add(new MsgPack(name).Add("R", ref rgbav.X).Add("G", ref rgbav.Y)
                                                   .Add("B", ref rgbav.Z).Add("A", ref rgbav.W));
            rgba = rgbav;
            return msgPack;
        }

        public static MsgPack Add(this MsgPack msgPack, string name, ref Vec3<Key> key)
        {
            MsgPack m = new MsgPack(name).Add("X", ref key.X).Add("Y", ref key.Y).Add("Z", ref key.Z);
            return m.List.Count > 0 ? msgPack.Add(m) : msgPack;
        }

        public static MsgPack Add(this MsgPack msgPack, string name, ref Vec2<Key?> uv)
        {
            if (!uv.X.HasValue && !uv.Y.HasValue) return msgPack;
            MsgPack m = new MsgPack(name);
            if (uv.X.HasValue) m = m.Add("U", ref uv.X);
            if (uv.Y.HasValue) m = m.Add("V", ref uv.Y);
            return msgPack.Add(m);
        }

        public static MsgPack Add(this MsgPack msgPack, string name, ref Key? key)
        {
            if (!key.HasValue) return msgPack;

            Key k = key.Value;
            msgPack = msgPack.Add(name, ref k);
            key = k;
            return msgPack;
        }

        public static MsgPack Add(this MsgPack msgPack, string name, ref Key key)
        {
            MsgPack keys = new MsgPack(name).Add("Type", key.Type.ToString());
            if (key.Keys != null && key.Type > KeyType.Static)
            {
                keys.Add("Max", key.Max);
                if ((int)key.EPTypePost > 0)
                    keys.Add("EPTypePost", key.EPTypePost.ToString());
                if ((int)key.EPTypePre > 0)
                    keys.Add("EPTypePre", key.EPTypePre.ToString());

                if (key.RawData.KeyType != 0) keys.Add("RawData", true);

                MsgPack Trans = new MsgPack(key.Keys.Length, "Trans");
                for (int i = 0; i < key.Keys.Length; i++)
                {
                    IKF kf = key.Keys[i].Check();
                         if (kf is KFT0 kft0) Trans[i] = new MsgPack(null,
                        new MsgPack[] { kft0.F });
                    else if (kf is KFT1 kft1) Trans[i] = new MsgPack(null,
                        new MsgPack[] { kft1.F, kft1.V });
                    else if (kf is KFT2 kft2) Trans[i] = new MsgPack(null,
                        new MsgPack[] { kft2.F, kft2.V, kft2.T });
                    else if (kf is KFT3 kft3) Trans[i] = new MsgPack(null,
                        new MsgPack[] { kft3.F, kft3.V, kft3.T1, kft3.T2, });
                }
                keys.Add(Trans);
            }
            else if (key.Type != KeyType.None) keys.Add("Value", key.Value);
            msgPack.Add(keys);
            return msgPack;
        }
    }

    public struct A3DAHeader
    {
        public int Count;
        public int BinaryLength;
        public int BinaryOffset;
        public int HeaderOffset;
        public int StringLength;
        public int StringOffset;

        public Format Format;
    }
}
