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

        public static int Rounding = 9;

        private Stream s;
        private bool a3dc;
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

        private const string BO = ".bin_offset";
        private const string MTBO = ".model_transform" + BO;

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
            uint signature = s.RU32();
            if (signature == 0x41443341)
            { header = s.ReadHeader(true); Head.Format = header.Format; signature = s.RU32(); }
            if (signature != 0x44334123) return 0;

            s.O = s.P - 4;
            signature = s.RU32();

            if ((signature & 0xFF) == (uint)'A')
            {
                header.Format = s.Format = Format.DT;
                Head.StringOffset = 0x10;
                Head.StringLength = s.L - 0x10;
            }
            else if ((signature & 0xFF) == (uint)'C')
            {
                s.P = 0x10;
                s.RI32();
                s.RI32();
                Head.SubHeadersOffset = s.RU32E(true);
                Head.SubHeadersCount = s.RU16E(true);
                Head.SubHeadersStride = s.RU16E(true);
                if (Head.SubHeadersCount != 0x02) return 0;

                s.PU32 = Head.SubHeadersOffset;
                if (s.RI32() != 0x50) return 0;
                Head.StringOffset = s.RI32E(true);
                Head.StringLength = s.RI32E(true);

                s.PU32 = Head.SubHeadersOffset + Head.SubHeadersStride;
                if (s.RI32() != 0x4C42) return 0;
                Head.BinaryOffset = s.RI32E(true);
                Head.BinaryLength = s.RI32E(true);

            }
            else return 0;

            s.P = Head.StringOffset;
            string[] strData = s.RS(Head.StringLength).Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            for (i = 0; i < strData.Length; i++)
                dict.GD(strData[i]);
            strData = null;

            A3DAReader();

            if (signature == 0x5F5F5F43)
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
                name = "camera_auxiliary";

                Data.CameraAuxiliary = new CameraAuxiliary
                {
                    AutoExposure     = RKN(name + ".auto_exposure"     ),
                        Exposure     = RKN(name +      ".exposure"     ),
                        ExposureRate = RKN(name +      ".exposure_rate"),
                    Gamma            = RKN(name + ".gamma"             ),
                    GammaRate        = RKN(name + ".gamma_rate"        ),
                    Saturate         = RKN(name + ".saturate"          ),
                };

                if (Data.CameraAuxiliary.Value.ExposureRate.HasValue
                    || Data.CameraAuxiliary.Value.GammaRate.HasValue)
                    if (Head.Format < Format.F || Head.Format == Format.AFT || Head.Format >= Format.FT)
                        Head.Format = Format.F;
            }

            if (dict.SW("play_control"))
            {
                name = "play_control";

                Data.PlayControl = new PlayControl();
                dict.FV(out Data.PlayControl.Begin , name + ".begin" );
                dict.FV(out Data.PlayControl.Div   , name + ".div"   );
                dict.FV(out Data.PlayControl.FPS   , name + ".fps"   );
                dict.FV(out Data.PlayControl.Offset, name + ".offset");
                dict.FV(out Data.PlayControl.Size  , name + ".size"  );
            }

            if (dict.SW("post_process"))
            {
                name = "post_process";

                Data.PostProcess = new PostProcess()
                {
                    Radius    = RRGBAKN(name + ".Ambient"   ),
                    Intensity = RRGBAKN(name + ".Diffuse"   ),
                    SceneFade = RRGBAKN(name + ".Specular"  ),
                    LensFlare = RKN    (name + ".lens_flare"),
                    LensGhost = RKN    (name + ".lens_ghost"),
                    LensShaft = RKN    (name + ".lens_shaft"),
                };
            }

            if (dict.FV(out value, "dof.name"))
            {
                Head.Format = Format.AFT;
                Data.DOF = new DOF { MT = RMT("dof") };
            }

            if (dict.FV(out value, "ambient.length"))
            {
                Data.Ambient = new Ambient[int.Parse(value)];
                Head.Format = Format.MGF;
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    name = "ambient." + i0;

                    dict.FV(out Data.Ambient[i0].Name, name + ".name");
                    Data.Ambient[i0].   LightDiffuse = RRGBAKN(name +    ".light.Diffuse");
                    Data.Ambient[i0].RimLightDiffuse = RRGBAKN(name + ".rimlight.Diffuse");
                }
            }

            if (dict.FV(out value, "auth_2d.length"))
            {
                Data.Auth2D = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    name = "auth_2d." + i0;

                    dict.FV(out Data.Auth2D[i0], name + ".name");
                }
            }

            if (dict.FV(out value, "camera_root.length"))
            {
                Data.CameraRoot = new CameraRoot[int.Parse(value)];
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    name = "camera_root." + i0;
                    nameView = name + ".view_point";

                    dict.FV(out Data.CameraRoot[i0].VP.Aspect, nameView + ".aspect");
                    if (dict.FV(out i1, nameView + ".fov_is_horizontal"))
                    {
                        Data.CameraRoot[i0].VP.FOVIsHorizontal = i1 != 0;
                        Data.CameraRoot[i0].VP.FOV         = RKNS(nameView +          ".fov");
                    }
                    else
                    {
                        dict.FV(out Data.CameraRoot[i0].VP.
                            CameraApertureH, nameView + ".camera_aperture_h");
                        dict.FV(out Data.CameraRoot[i0].VP.
                            CameraApertureW, nameView + ".camera_aperture_w");
                        Data.CameraRoot[i0].VP.FocalLength = RKNS(nameView + ".focal_length");
                    }

                    Data.CameraRoot[i0].      MT = RMT(name);
                    Data.CameraRoot[i0].Interest = RMT(name + ".interest");
                    Data.CameraRoot[i0].VP.   MT = RMT(nameView);
                    Data.CameraRoot[i0].VP.Roll = RKNS(nameView + ".roll");
                }
            }

            if (dict.FV(out value, "chara.length"))
            {
                Data.Chara = new Chara[int.Parse(value)];
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                {
                    name = "chara." + i0;

                    dict.FV(out Data.Chara[i0].Name, name + ".name");

                    Data.Chara[i0].MT = RMT(name);
                }
            }

            if (dict.FV(out value, "curve.length"))
            {
                Data.Curve = new Curve[int.Parse(value)];
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    name = "curve." + i0;

                    dict.FV(out Data.Curve[i0].Name, name + ".name");
                    Data.Curve[i0].CV = RK(name + ".cv");
                }
            }

            if (dict.FV(out value, "event.length"))
            {
                Data.Event = new Event[int.Parse(value)];
                for (i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    name = "event." + i0;

                    dict.FV(out Data.Event[i0].Begin       , name + ".begin"         );
                    dict.FV(out Data.Event[i0].ClipBegin   , name + ".clip_begin"    );
                    dict.FV(out Data.Event[i0].ClipEnd     , name + ".clip_en"       );
                    dict.FV(out Data.Event[i0].End         , name + ".end"           );
                    dict.FV(out Data.Event[i0].Name        , name + ".name"          );
                    dict.FV(out Data.Event[i0].Param1      , name + ".param1"        );
                    dict.FV(out Data.Event[i0].Ref         , name + ".ref"           );
                    dict.FV(out Data.Event[i0].TimeRefScale, name + ".time_ref_scale");
                    dict.FV(out Data.Event[i0].Type        , name + ".type"          );
                }
            }

            if (dict.FV(out value, "fog.length"))
            {
                Data.Fog = new Fog[int.Parse(value)];
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    name = "fog." + i0;

                    dict.FV(out Data.Fog[i0].Id, name + ".id");
                    Data.Fog[i0].Color   = RRGBAKN(name + ".Diffuse");
                    Data.Fog[i0].Density = RKN    (name + ".density");
                    Data.Fog[i0].End     = RKN    (name +     ".end");
                    Data.Fog[i0].Start   = RKN    (name +   ".start");
                }
            }

            if (dict.FV(out value, "light.length"))
            {
                Data.Light = new Light[int.Parse(value)];
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    name = "light." + i0;

                    dict.FV(out Data.Light[i0].Id  , name + ".id"  );
                    dict.FV(out Data.Light[i0].Type, name + ".type");

                    Data.Light[i0].Ambient       = RRGBAKN(name +        ".Ambient");
                    Data.Light[i0].ConeAngle     = RKN    (name +      ".ConeAngle");
                    Data.Light[i0].Constant      = RKN    (name +       ".CONSTANT");
                    Data.Light[i0].Diffuse       = RRGBAKN(name +        ".Diffuse");
                    Data.Light[i0].DropOff       = RKN    (name +        ".DropOff");
                    Data.Light[i0].Far           = RKN    (name +            ".FAR");
                    Data.Light[i0].ToneCurve     = RRGBAKN(name +  ".Incandescence");
                    Data.Light[i0].Intensity     = RKN    (name +      ".Intensity");
                    Data.Light[i0].Linear        = RKN    (name +         ".LINEAR");
                    Data.Light[i0].Quadratic     = RKN    (name +      ".QUADRATIC");
                    Data.Light[i0].Specular      = RRGBAKN(name +       ".Specular");
                    Data.Light[i0].Position      = RMT    (name +       ".position");
                    Data.Light[i0].SpotDirection = RMT    (name + ".spot_direction");

                    if (Data.Light[i0].ConeAngle.HasValue || Data.Light[i0].Constant.HasValue
                        || Data.Light[i0].DropOff.HasValue || Data.Light[i0].Far.HasValue
                        || Data.Light[i0].Linear.HasValue || Data.Light[i0].Quadratic.HasValue
                        || Data.Light[i0].Quadratic.HasValue)
                        Head.Format = Format.XHD;
                }
            }

            if (dict.FV(out value, "m_objhrc.length"))
            {
                Data.MObjectHRC = new MObjectHRC[int.Parse(value)];
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    name = "m_objhrc." + i0;

                    dict.FV(out Data.MObjectHRC[i0].Name, name + ".name");

                    if (dict.FV(out value, name + ".instance.length"))
                    {
                        Data.MObjectHRC[i0].Instances = new MObjectHRC.Instance[int.Parse(value)];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                        {
                            nameView = name + ".instance." + i1;

                            dict.FV(out Data.MObjectHRC[i0]
                                .Instances[i1].   Name, nameView +     ".name");
                            dict.FV(out Data.MObjectHRC[i0]
                                .Instances[i1].UIDName, nameView + ".uid_name");
                            dict.FV(out int shadow, nameView + ".shadow");
                            Data.MObjectHRC[i0].Instances[i1].Shadow = shadow != 0;

                            Data.MObjectHRC[i0].Instances[i1].MT = RMT(nameView);
                        }
                    }

                    if (dict.FV(out value, name + ".node.length"))
                    {
                        Data.MObjectHRC[i0].Node = new ObjectNode[int.Parse(value)];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            nameView = name + ".node." + i1;
                            if (dict.SW(nameView + ".joint_orient"))
                            {
                                Vec3 jointOrient = new Vec3();
                                dict.FV(out jointOrient.X, nameView + ".joint_orient.x");
                                dict.FV(out jointOrient.Y, nameView + ".joint_orient.y");
                                dict.FV(out jointOrient.Z, nameView + ".joint_orient.z");
                                Data.MObjectHRC[i0].Node[i1].JointOrient = jointOrient;
                            }
                            else
                                Data.MObjectHRC[i0].Node[i1].JointOrient = null;

                            dict.FV(out Data.MObjectHRC[i0].Node[i1].Name  , nameView + ".name"  );
                            dict.FV(out Data.MObjectHRC[i0].Node[i1].Parent, nameView + ".parent");

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
                    dict.FV(out Data.MObjectHRCList[i0], "m_objhrc_list." + i0);
            }

            if (dict.FV(out value, "material_list.length"))
            {
                Data.MaterialList = new MaterialList[int.Parse(value)];
                if (Head.Format != Format.XHD)
                    Head.Format = Format.X;
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    name = "material_list." + i0;
                    dict.FV(out Data.MaterialList[i0].Name, name + ".name");

                    Data.MaterialList[i0].BlendColor    = RRGBAKN(name +    ".blend_color");
                    Data.MaterialList[i0].GlowIntensity = RKN    (name + ".glow_intensity");
                    Data.MaterialList[i0].Incandescence = RRGBAKN(name +  ".incandescence");
                }
            }

            if (dict.FV(out value, "motion.length"))
            {
                Data.Motion = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.Motion.Length; i0++)
                    dict.FV(out Data.Motion[i0], "motion." + i0 + ".name");
            }

            if (dict.FV(out value, "object.length"))
            {
                Data.Object = new Object[int.Parse(value)];
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    name = "object." + i0;

                    if (dict.FV(out Data.Object[i0].Morph      , name + ".morph"       ))
                        dict.FV(out Data.Object[i0].MorphOffset, name + ".morph_offset");
                    dict.FV(out Data.Object[i0].       Name, name +         ".name");
                    dict.FV(out Data.Object[i0]. ParentName, name +  ".parent_name");
                    dict.FV(out Data.Object[i0]. ParentNode, name +  ".parent_node");
                    if (dict.FV(out Data.Object[i0].  Pat      , name +   ".pat"       ))
                        dict.FV(out Data.Object[i0].  PatOffset, name +   ".pat_offset");
                    dict.FV(out Data.Object[i0].    UIDName, name +     ".uid_name");

                    if (dict.FV(out value, name + ".tex_pat.length"))
                    {
                        Data.Object[i0].TexPat = new Object.TexturePattern[int.Parse(value)];
                        for (i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                        {
                            nameView = name + ".tex_pat." + i1;
                            dict.FV(out Data.Object[i0].TexPat[i1]
                                .Name     , nameView + ".name"      );
                            if (dict.FV(out Data.Object[i0].TexPat[i1]
                                .Pat      , nameView + ".pat"       ))
                                dict.FV(out Data.Object[i0].TexPat[i1]
                                    .PatOffset, nameView + ".pat_offset");
                        }
                    }

                    if (dict.FV(out value, name + ".tex_transform.length"))
                    {
                        Data.Object[i0].TexTrans = new Object.TextureTransform[int.Parse(value)];
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                        {
                            nameView = name + ".tex_transform." + i1;

                            dict.FV(out Data.Object[i0].TexTrans[i1].Name, nameView + ".name");
                            Data.Object[i0].TexTrans[i1].CoverageU       = RKN(nameView + ".coverageU"      );
                            Data.Object[i0].TexTrans[i1].CoverageV       = RKN(nameView + ".coverageV"      );
                            Data.Object[i0].TexTrans[i1].OffsetU         = RKN(nameView + ".offsetU"        );
                            Data.Object[i0].TexTrans[i1].OffsetV         = RKN(nameView + ".offsetV"        );
                            Data.Object[i0].TexTrans[i1].RepeatU         = RKN(nameView + ".repeatU"        );
                            Data.Object[i0].TexTrans[i1].RepeatV         = RKN(nameView + ".repeatV"        );
                            Data.Object[i0].TexTrans[i1].   Rotate       = RKN(nameView + ".rotate"         );
                            Data.Object[i0].TexTrans[i1].   RotateFrame  = RKN(nameView + ".rotateFrame"    );
                            Data.Object[i0].TexTrans[i1].TranslateFrameU = RKN(nameView + ".translateFrameU");
                            Data.Object[i0].TexTrans[i1].TranslateFrameV = RKN(nameView + ".translateFrameV");
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
                    name = "objhrc." + i0;

                    dict.FV(out Data.ObjectHRC[i0].      Name, name +        ".name");
                    dict.FV(out Data.ObjectHRC[i0].ParentName, name + ".parent_name");
                    dict.FV(out Data.ObjectHRC[i0].ParentNode, name + ".parent_node");
                    dict.FV(out Data.ObjectHRC[i0].   UIDName, name +    ".uid_name");
                    dict.FV(out int shadow, name + ".shadow");
                    Data.ObjectHRC[i0].Shadow = shadow != 0;
                    if (dict.FV(out value, name + ".node.length"))
                    {
                        Data.ObjectHRC[i0].Node = new ObjectNode[int.Parse(value)];
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        {
                            nameView = name + ".node." + i1;

                            if (dict.SW(nameView + ".joint_orient"))
                            {
                                Vec3 jointOrient = new Vec3();
                                dict.FV(out jointOrient.X, nameView + ".joint_orient.x");
                                dict.FV(out jointOrient.Y, nameView + ".joint_orient.y");
                                dict.FV(out jointOrient.Z, nameView + ".joint_orient.z");
                                Data.ObjectHRC[i0].Node[i1].JointOrient = jointOrient;
                            }
                            else
                                Data.ObjectHRC[i0].Node[i1].JointOrient = null;

                            dict.FV(out Data.ObjectHRC[i0].Node[i1].Name  , nameView + ".name"  );
                            dict.FV(out Data.ObjectHRC[i0].Node[i1].Parent, nameView + ".parent");

                            Data.ObjectHRC[i0].Node[i1].MT = RMT(nameView);
                        }
                    }
                }
            }

            if (dict.FV(out value, "object_list.length"))
            {
                Data.ObjectList = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.ObjectList.Length; i0++)
                    dict.FV(out Data.ObjectList[i0], "object_list." + i0);
            }

            if (dict.FV(out value, "objhrc_list.length"))
            {
                Data.ObjectHRCList = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
                    dict.FV(out Data.ObjectHRCList[i0], "objhrc_list." + i0);
            }

            if (dict.FV(out value, "point.length"))
            {
                Data.Point = new Point[int.Parse(value)];
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                {
                    name = "point." + i0;

                    dict.FV(out Data.Point[i0].Name, name + ".name");

                    Data.Point[i0].MT = RMT(name);
                }
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
                    name = "ambient." + soi0;
                    ref Ambient Ambient = ref Data.Ambient[soi0];

                    W(ref Ambient.   LightDiffuse, name +    ".light.Diffuse");
                    W(name + ".name", Ambient.Name);
                    W(ref Ambient.RimLightDiffuse, name + ".rimlight.Diffuse");
                }
                W("ambient.length", Data.Fog.Length);
            }

            if (Data.Auth2D != null)
            {
                so0 = Data.Auth2D.Length.SW();
                for (i0 = 0; i0 < Data.Auth2D.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "auth_2d." + soi0;

                    W(name + ".name", Data.Auth2D[soi0]);
                }
                W("auth_2d.length", Data.Fog.Length);
            }

            if (Data.CameraAuxiliary != null)
            {
                name = "camera_auxiliary";
                CameraAuxiliary ca = Data.CameraAuxiliary.Value;
                if (Head.Format != Format.F && (Head.Format <= Format.AFT || Head.Format >= Format.FT))
                {
                    W(ref ca.AutoExposure    , name + ".auto_exposure"     );
                    W(ref ca.    Exposure    , name +      ".exposure"     );
                }
                if (Head.Format == Format.F || (Head.Format > Format.AFT && Head.Format < Format.FT))
                    W(ref ca.    ExposureRate, name +      ".exposure_rate");
                if (Head.Format != Format.F && (Head.Format <= Format.AFT || Head.Format >= Format.FT))
                    W(ref ca.Gamma           , name + ".gamma"             );
                if (Head.Format == Format.F || (Head.Format > Format.AFT && Head.Format < Format.FT))
                    W(ref ca.GammaRate       , name + ".gamma_rate"        );
                W(ref ca.Saturate        , name + ".saturate"          );
                Data.CameraAuxiliary = ca;
            }

            if (Data.CameraRoot != null)
            {
                so0 = Data.CameraRoot.Length.SW();
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "camera_root." + soi0;
                    nameView = name + ".view_point";
                    ref CameraRoot cr = ref Data.CameraRoot[soi0];

                    W(ref cr.Interest, name + ".interest");
                    W(ref cr.MT, name, 0b11110);
                    W(nameView + ".aspect", cr.VP.Aspect);
                    if (cr.VP.FOV.HasValue)
                    {
                        if (cr.VP.FOV.HasValue)
                        {
                            Key key = cr.VP.FOV.Value;
                            W(ref key, nameView + ".fov");
                            cr.VP.FOV = key;
                        }
                        W(nameView + ".fov_is_horizontal", cr.VP.FOVIsHorizontal ? 1 : 0);
                    }
                    else
                    {
                        W(nameView + ".camera_aperture_h", cr.VP.CameraApertureH);
                        W(nameView + ".camera_aperture_w", cr.VP.CameraApertureW);
                        if (cr.VP.FocalLength.HasValue)
                        {
                            Key key = cr.VP.FocalLength.Value;
                            W(ref key, nameView + ".focal_length");
                            cr.VP.FocalLength = key;
                        }
                    }
                    W(ref cr.VP.MT  , nameView, 0b10000);
                    if (cr.VP.Roll.HasValue)
                    {
                        Key key = cr.VP.Roll.Value;
                        W(ref key, nameView + ".roll");
                        cr.VP.Roll = key;
                    }
                    W(ref cr.VP.MT  , nameView, 0b01111);
                    W(ref cr.MT     , name    , 0b00001);
                }
                W("camera_root.length", Data.CameraRoot.Length);
            }

            if (Data.Chara != null)
            {
                so0 = Data.Chara.Length.SW();
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                {
                    W("chara." + so0[i0] + ".name", Data.Chara[so0[i0]].Name);
                    W(ref Data.Chara[so0[i0]].MT, "chara." + so0[i0]);
                }
                W("chara.length", Data.Chara.Length);
            }

            if (Data.Curve != null)
            {
                so0 = Data.Curve.Length.SW();
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "curve." + soi0;
                    ref Curve curve = ref Data.Curve[soi0];

                    W(ref curve.CV, name + ".cv");
                    W(name + ".name", curve.Name);
                }
                W("curve.length", Data.Curve.Length);
            }

            if (Data.DOF != null && (Head.Format == Format.AFT || Head.Format == Format.FT))
            {
                DOF dof = Data.DOF.Value;
                W("dof.name", "DOF");
                W(ref dof.MT, "dof");
                Data.DOF = dof;
            }

            if (Data.Event != null)
            {
                so0 = Data.Event.Length.SW();
                for (i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "event." + soi0;
                    ref Event @event = ref Data.Event[soi0];

                    W(name + ".begin"         , @event.Begin       );
                    W(name + ".clip_begin"    , @event.ClipBegin   );
                    W(name + ".clip_en"       , @event.ClipEnd     );
                    W(name + ".end"           , @event.End         );
                    W(name + ".name"          , @event.Name        );
                    W(name + ".param1"        , @event.Param1      );
                    W(name + ".ref"           , @event.Ref         );
                    W(name + ".time_ref_scale", @event.TimeRefScale);
                    W(name + ".type"          , @event.Type        );
                }
                W("event.length", Data.Event.Length);
            }

            if (Data.Fog != null)
            {
                so0 = Data.Fog.Length.SW();
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "fog." + soi0;
                    ref Fog fog = ref Data.Fog[soi0];

                    W(ref fog.Color  , name + ".Diffuse");
                    W(ref fog.Density, name + ".density");
                    W(ref fog.End    , name + ".end"    );
                    W(name + ".id", fog.Id);
                    W(ref fog.Start  , name + ".start"  );
                }
                W("fog.length", Data.Fog.Length);
            }

            if (Data.Light != null)
            {
                so0 = Data.Light.Length.SW();
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "light." + soi0;
                    ref Light light = ref Data.Light[soi0];

                    W(ref light.Ambient      , name + ".Ambient"      );
                    if (Head.Format == Format.XHD)
                    {
                        W(ref light.Constant     , name + ".CONSTANT"     );
                        W(ref light.ConeAngle    , name + ".ConeAngle"    );
                    }
                    W(ref light.Diffuse, name + ".Diffuse");
                    if (Head.Format == Format.XHD)
                    {
                        W(ref light.DropOff      , name + ".DropOff"      );
                        W(ref light.Far          , name + ".FAR"          );
                    }
                    W(ref light.ToneCurve, name + ".Incandescence");
                    if (Head.Format == Format.XHD)
                    {
                        W(ref light.Intensity    , name + ".Intensity"    );
                        W(ref light.Linear       , name + ".LINEAR"       );
                        W(ref light.Quadratic    , name + ".QUADRATIC"    );
                    }
                    W(ref light.Specular     , name + ".Specular"     );
                    W(name + ".id"  , light.Id  );
                    W(name + ".name", light.Id switch
                    {
                        0 => "Char",
                        1 => "Stage",
                        2 => "Sun",
                        3 => "Reflect",
                        5 => "CharColor",
                        6 => "ToneCurve",
                        _ => "none",
                    });
                    W(ref light.Position     , name + ".position"      );
                    W(ref light.SpotDirection, name + ".spot_direction");
                    W(name + ".type", light.Type);
                }
                W("light.length", Data.Light.Length);
            }

            if (Data.MObjectHRC != null)
            {
                so0 = Data.MObjectHRC.Length.SW();
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "m_objhrc." + soi0;
                    ref MObjectHRC mObjectHRC = ref Data.MObjectHRC[soi0];

                    if (mObjectHRC.Instances != null)
                    {
                        so1 = mObjectHRC.Instances.Length.SW();
                        for (i1 = 0; i1 < mObjectHRC.Instances.Length; i1++)
                        {
                            soi1 = so1[i1];
                            nameView = name + ".instance." + soi1;
                            ref MObjectHRC.Instance instance = ref mObjectHRC.Instances[soi1];

                            W(ref instance.MT, nameView, 0b10000);
                            W(nameView +     ".name", instance.   Name);
                            W(ref instance.MT, nameView, 0b01100);
                            if (instance.Shadow)
                                W(nameView + ".shadow", 1);
                            W(ref instance.MT, nameView, 0b00010);
                            W(nameView + ".uid_name", instance.UIDName);
                            W(ref instance.MT, nameView, 0b00001);
                        }
                        W(name + ".instance.length", mObjectHRC.Instances.Length);
                    }

                    W(ref mObjectHRC.MT, name, 0b10000);
                    W(name + ".name", mObjectHRC.Name);

                    if (mObjectHRC.Node != null)
                    {
                        so1 = mObjectHRC.Node.Length.SW();
                        for (i1 = 0; i1 < mObjectHRC.Node.Length; i1++)
                        {
                            soi1 = so1[i1];
                            nameView = name + ".node." + soi1;
                            ref ObjectNode node = ref mObjectHRC.Node[soi1];

                            if (node.JointOrient.HasValue)
                            {
                                Vec3 jointOrient = node.JointOrient.Value;
                                W(nameView + ".joint_orient.x", jointOrient.X);
                                W(nameView + ".joint_orient.y", jointOrient.Y);
                                W(nameView + ".joint_orient.z", jointOrient.Z);
                            }

                            W(ref node.MT, nameView, 0b10000);

                            W(nameView +   ".name", node.Name  );
                            W(nameView + ".parent", node.Parent);
                            W(ref node.MT, nameView, 0b01111);
                        }
                        W(name + ".node.length", mObjectHRC.Node.Length);
                    }

                    W(ref mObjectHRC.MT, name, 0b01111);
                }
                W("m_objhrc.length", Data.MObjectHRC.Length);
            }

            if (Data.MObjectHRCList != null)
            {
                so0 = Data.MObjectHRCList.Length.SW();
                for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                    W("m_objhrc_list." + so0[i0], Data.MObjectHRCList[so0[i0]]);
                W("m_objhrc_list.length", Data.MObjectHRCList.Length);
            }

            if (Data.MaterialList != null && (Head.Format == Format.X || Head.Format == Format.XHD))
            {
                so0 = Data.MaterialList.Length.SW();
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "material_list." + soi0;
                    ref MaterialList ml = ref Data.MaterialList[soi0];

                    W(ref ml.BlendColor   , name + ".blend_color"   );
                    W(ref ml.GlowIntensity, name + ".glow_intensity");
                    W(name + ".hash_name", HashExt.HashMurmurHash(ml.Name.ToUTF8()));
                    W(ref ml.Incandescence, name + ".incandescence" );
                    W(name +      ".name", ml.Name);
                }
                W("material_list.length", Data.MaterialList.Length);
            }

            if (Data.Motion != null)
            {
                so0 = Data.Motion.Length.SW();
                for (i0 = 0; i0 < Data.Motion.Length; i0++)
                    W(name + "." + so0[i0] + ".name", Data.Motion[so0[i0]]);
                W("motion.length", Data.Motion.Length);
            }

            if (Data.Object != null)
            {
                so0 = Data.Object.Length.SW();
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "object." + soi0;
                    ref Object @object = ref Data.Object[soi0];

                    W(ref @object.MT, name, 0b10000);
                    if (@object.Morph != null)
                    {
                        W(name + ".morph"       , @object.Morph      );
                        W(name + ".morph_offset", @object.MorphOffset);
                    }
                    W(name + ".name"       , @object.Name      );
                    W(name + ".parent_name", @object.ParentName);
                    W(name + ".parent_node", @object.ParentNode);
                    if (@object.Pat != null)
                    {
                        W(name + ".pat"       , @object.Pat      );
                        W(name + ".pat_offset", @object.PatOffset);
                    }
                    W(ref @object.MT, name, 0b01100);

                    if (@object.TexPat != null)
                    {
                        so1 = @object.TexPat.Length.SW();
                        for (i1 = 0; i1 < @object.TexPat.Length; i1++)
                        {
                            soi1 = so1[i1];
                            nameView = name + ".tex_pat." + soi1;
                            ref Object.TexturePattern texPat = ref @object.TexPat[soi1];

                            W(nameView + ".name"      , texPat.Name     );
                            W(nameView + ".pat"       , texPat.Pat      );
                            W(nameView + ".pat_offset", texPat.PatOffset);
                        }
                        W(name + ".tex_pat.length", @object.TexPat.Length);
                    }

                    if (@object.TexTrans != null)
                    {
                        so1 = @object.TexTrans.Length.SW();
                        for (i1 = 0; i1 < @object.TexTrans.Length; i1++)
                        {
                            soi1 = so1[i1];
                            nameView = name + ".tex_transform." + soi1;
                            ref Object.TextureTransform texTrans = ref @object.TexTrans[soi1];

                            W(ref texTrans.CoverageU      , nameView + ".coverageU"      );
                            W(ref texTrans.CoverageV      , nameView + ".coverageV"      );
                            W(nameView + ".name", @object.TexTrans[soi1].Name);
                            W(ref texTrans.OffsetU        , nameView + ".offsetU"        );
                            W(ref texTrans.OffsetV        , nameView + ".offsetV"        );
                            W(ref texTrans.RepeatU        , nameView + ".repeatU"        );
                            W(ref texTrans.RepeatV        , nameView + ".repeatV"        );
                            W(ref texTrans.   Rotate      , nameView + ".rotate"         );
                            W(ref texTrans.   RotateFrame , nameView + ".rotateFrame"    );
                            W(ref texTrans.TranslateFrameU, nameView + ".translateFrameU");
                            W(ref texTrans.TranslateFrameV, nameView + ".translateFrameV");
                        }
                        W(name + ".tex_transform.length", @object.TexTrans.Length);
                    }

                    W(ref @object.MT, name, 0b00010);
                    W(name + ".uid_name", @object.UIDName);
                    W(ref @object.MT, name, 0b00001);
                }
                W("object.length", Data.Object.Length);
            }

            if (Data.ObjectList != null)
            {
                so0 = Data.ObjectList.Length.SW();
                for (i0 = 0; i0 < Data.ObjectList.Length; i0++)
                    W("object_list." + so0[i0], Data.ObjectList[so0[i0]]);
                W("object_list.length", Data.ObjectList.Length);
            }

            if (Data.ObjectHRC != null)
            {
                so0 = Data.ObjectHRC.Length.SW();
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    soi0 = so0[i0];
                    name = "objhrc." + soi0;
                    ref ObjectHRC objectHRC = ref Data.ObjectHRC[soi0];

                    W(name + ".name", objectHRC.Name);

                    if (objectHRC.Node != null)
                    {
                        so1 = objectHRC.Node.Length.SW();
                        for (i1 = 0; i1 < objectHRC.Node.Length; i1++)
                        {
                            soi1 = so1[i1];
                            nameView = name + ".node." + soi1;
                            ref ObjectNode node = ref objectHRC.Node[soi1];

                            if (node.JointOrient.HasValue)
                            {
                                Vec3 jointOrient = node.JointOrient.Value;
                                W(nameView + ".joint_orient.x", jointOrient.X);
                                W(nameView + ".joint_orient.y", jointOrient.Y);
                                W(nameView + ".joint_orient.z", jointOrient.Z);
                            }

                            W(ref node.MT, nameView, 0b10000);

                            W(nameView + ".name"  , node.Name  );
                            W(nameView + ".parent", node.Parent);
                            W(ref node.MT, nameView, 0b01111);
                        }
                        W(name + ".node.length", objectHRC.Node.Length);
                    }

                    W(name + ".parent_name", objectHRC.ParentName);
                    W(name + ".parent_node", objectHRC.ParentNode);
                    if (objectHRC.Shadow)
                        W(name + ".shadow", 1);
                    W(name +    ".uid_name", objectHRC.   UIDName);
                }
                W("objhrc.length", Data.ObjectHRC.Length);
            }

            if (Data.ObjectHRCList != null)
            {
                so0 = Data.ObjectHRCList.Length.SW();
                for (i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
                    W("objhrc_list." + so0[i0], Data.ObjectHRCList[so0[i0]]);
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
                name = "post_process";
                W(ref pp.Radius   , name + ".Ambient"   );
                W(ref pp.Intensity, name + ".Diffuse"   );
                W(ref pp.SceneFade, name + ".Specular"  );
                W(ref pp.LensFlare, name + ".lens_flare");
                W(ref pp.LensGhost, name + ".lens_ghost");
                W(ref pp.LensShaft, name + ".lens_shaft");
                Data.PostProcess = pp;
            }

            if (Data.Point != null)
            {
                so0 = Data.Point.Length.SW();
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                {
                    W("point." + so0[i0] + ".name", Data.Point[so0[i0]].Name);
                    W(ref Data.Point[so0[i0]].MT, "point." + so0[i0]);
                }
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

            mt.Rot        = RV3(str + ".rot"       );
            mt.Scale      = RV3(str + ".scale"     );
            mt.Trans      = RV3(str + ".trans"     );
            mt.Visibility = RK (str + ".visibility");
            return mt;
        }

        private Vec4<Key?>? RRGBAKN(string str) =>
            dict.SW(str) ? (Vec4<Key?>?)new Vec4<Key?> { W = RKNS(str + ".a"), Z = RKNS(str + ".b"),
                                                         Y = RKNS(str + ".g"), X = RKNS(str + ".r") } : null;

        private Vec3<Key> RV3(string str) =>
            new Vec3<Key> { X = RK(str + ".x"), Y = RK(str + ".y"), Z = RK(str + ".z") };

        private Key? RKNS(string str) =>
            dict.SW(str) ? (Key?)RK(str) : null;

        private Key? RKN(string str) =>
            dict.FV(out bool b, str + ".") && b ? (Key?)RK(str) : null;

        private Key RK(string str)
        {
            Key key = new Key();
            if ( dict.FV(out key.BinOffset, str + BO     )) return  key;
            if (!dict.FV(out int type     , str + ".type")) return default;

            key.Type = (KeyType)type;
            if (key.Type == KeyType.None  ) return key;
            if (key.Type == KeyType.Static) { dict.FV(out key.Value, str + ".value"); return key; }

            int i = 0;
            if (dict.FV(out int epTypePost, str + ".ep_type_post"))
                key.EPTypePost = (EPType)epTypePost;
            if (dict.FV(out int epTypePre , str + ".ep_type_pre" ))
                key.EPTypePre  = (EPType)epTypePre;
            dict.FV(out int length, str + ".key.length");
            dict.FV(out key.Max   , str + ".max"       );
            int keyType = 0;
            if (dict.SW(str + ".raw_data"))
                dict.FV(out keyType, str + ".raw_data_key_type");

            if (keyType != 0)
            {
                string[] VL = null;
                dict.FV(out string value_type, str + ".raw_data.value_type");
                if (dict.FV(out value, str + ".raw_data.value_list"))
                    VL = value.Split(',');
                dict.FV(out int valueListSize, str + ".raw_data.value_list_size");
                value = "";

                int ds = keyType + 1;
                length = valueListSize / ds;
                key.Keys = new KFT3[length];
                     if (keyType == 0)
                    for (i = 0; i < length; i++)
                        key.Keys[i] = new KFT3(VL[i * ds + 0].ToF32());
                else if (keyType == 1)
                    for (i = 0; i < length; i++)
                        key.Keys[i] = new KFT3(VL[i * ds + 0].ToF32(), VL[i * ds + 1].ToF32());
                else if (keyType == 2)
                    for (i = 0; i < length; i++)
                        key.Keys[i] = new KFT3(VL[i * ds + 0].ToF32(), VL[i * ds + 1].ToF32(),
                                               VL[i * ds + 2].ToF32());
                else if (keyType == 3)
                    for (i = 0; i < length; i++)
                        key.Keys[i] = new KFT3(VL[i * ds + 0].ToF32(), VL[i * ds + 1].ToF32(),
                                               VL[i * ds + 2].ToF32(), VL[i * ds + 3].ToF32());
            }
            else
            {
                key.Keys = new KFT3[length];
                for (i = 0; i < length; i++)
                {
                    if (!dict.FV(out value, str + ".key." + i + ".data")) continue;

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
            if (a3dc) { if ((flags & 0b10000) != 0) W(str + MTBO, mt.BinOffset); }
            else
            {
                if ((flags & 0b01000) != 0) W(ref mt.Rot       , str + ".rot"       );
                if ((flags & 0b00100) != 0) W(ref mt.Scale     , str + ".scale"     );
                if ((flags & 0b00010) != 0) W(ref mt.Trans     , str + ".trans"     );
                if ((flags & 0b00001) != 0) W(ref mt.Visibility, str + ".visibility", a3dc);
            }
        }

        private void W(ref Vec4<Key?>? rgba, string str)
        {
            if (!rgba.HasValue) return;

            bool a3dc = Head.Format != Format.AFT && this.a3dc; // A3DC reading is bugged in AFT

            Vec4<Key?> rgbav = rgba.Value;
            W(str, "true");
            if (rgbav.W.HasValue) { Key k = rgbav.W.Value; W(ref k, str + ".a", a3dc); rgbav.W = k; }
            if (rgbav.Z.HasValue) { Key k = rgbav.Z.Value; W(ref k, str + ".b", a3dc); rgbav.Z = k; }
            if (rgbav.Y.HasValue) { Key k = rgbav.Y.Value; W(ref k, str + ".g", a3dc); rgbav.Y = k; }
            if (rgbav.X.HasValue) { Key k = rgbav.X.Value; W(ref k, str + ".r", a3dc); rgbav.X = k; }
            rgba = rgbav;
        }

        private void W(ref Vec3<Key> key, string str)
        { W(ref key.X, str + ".x", a3dc); W(ref key.Y, str + ".y", a3dc); W(ref key.Z, str + ".z", a3dc); }

        private void W(ref Key? key, string str)
        { if (!key.HasValue) return; Key k = key.Value; W(str, "true"); W(ref k, str, a3dc); key = k; }

        private void W(ref Key key, string str) =>
            W(ref key, str, a3dc);

        private void W(ref Key key, string str, bool a3dc)
        {
            if (a3dc) { W(str + BO, key.BinOffset); return; }

            int i = 0;
            if (key.Type < KeyType.Linear || key.Keys == null || (key.Keys != null && key.Keys.Length == 0))
            {
                W(str + ".type", (int)key.Type);
                if (key.Type > 0) W(str + ".value", key.Value);
                return;
            }

            if (key.EPTypePost != EPType.None) W(str + ".ep_type_post", (int)key.EPTypePost);
            if (key.EPTypePre  != EPType.None) W(str + ".ep_type_pre" , (int)key.EPTypePre );
            if (!key.RawData && key.Keys != null)
            {
                int length = key.Keys.Length;
                IKF kf;
                so = length.SW();
                for (i = 0; i < length; i++)
                {
                    soi = so[i];
                    kf = key.Keys[soi].Check();
                    W(str + ".key." + soi + ".data", kf.ToString());
                    int Type = 0;
                         if (kf is KFT0) Type = 0;
                    else if (kf is KFT1) Type = 1;
                    else if (kf is KFT2) Type = 2;
                    else if (kf is KFT3) Type = 3;
                    W(str + ".key." + soi + ".type", Type);
                }
                W(str + ".key.length", length);
                W(str + ".max", (float)(key.Max ?? Data.PlayControl.Size));
                W(str + ".type", (int)key.Type);
            }
            else if (key.Keys != null)
            {
                int length = key.Keys.Length;
                int keyType = 0;
                IKF kf;
                W(str + ".max", (float)(key.Max ?? Data.PlayControl.Size));
                for (i = 0; i < length && keyType < 3; i++)
                {
                    kf = key.Keys[i].Check();
                         if (kf is KFT0 && keyType < 0) keyType = 0;
                    else if (kf is KFT1 && keyType < 1) keyType = 1;
                    else if (kf is KFT2 && keyType < 2) keyType = 2;
                    else if (kf is KFT3 && keyType < 3) keyType = 3;
                }
                int valueListSize = length * keyType + length;
                s.W(str + ".raw_data.value_list=");
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
                W(str + ".raw_data.value_list_size", valueListSize);
                W(str + ".raw_data.value_type"     , "float"      );
                W(str + ".raw_data_key_type"       , keyType      );
                W(str + ".type", (int)key.Type);
            }
        }

        private void W(string Data,   long? val)
        { if (val != null) W(Data, ( long)val   ); }
        private void W(string Data,  ulong? val)
        { if (val != null) W(Data, (ulong)val   ); }
        private void W(string Data,  float? val)
        { if (val != null) W(Data, (float)val   ); }
        private void W(string Data, double? val)
        { if (val != null) W(Data, (float)val   ); }
        private void W(string Data,   long  val) =>
                           W(Data,        val.ToS());
        private void W(string Data,  ulong  val) =>
                           W(Data,        val.ToS());
        private void W(string Data,  float  val) =>
                           W(Data,        val.ToS(Rounding));
        private void W(string Data, double  val) =>
                           W(Data,        val.ToS(Rounding));
        private void W(string Data, string  val)
        { if (val != null) s.W(Data + "=" + val + "\n"); }

        private void A3DCReader()
        {
            if (Data.Ambient != null)
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    RRGBAKN(ref Data.Ambient[i0].   LightDiffuse);
                    RRGBAKN(ref Data.Ambient[i0].RimLightDiffuse);
                }

            if (Data.CameraAuxiliary != null)
            {
                CameraAuxiliary ca = Data.CameraAuxiliary.Value;
                RKN(ref ca.AutoExposure    );
                RKN(ref ca.    Exposure    );
                RKN(ref ca.    ExposureRate);
                RKN(ref ca.Gamma           );
                RKN(ref ca.GammaRate       );
                RKN(ref ca.Saturate        );
                Data.CameraAuxiliary = ca;
            }

            if (Data.CameraRoot != null)
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    RMT(ref Data.CameraRoot[i0].      MT);
                    RMT(ref Data.CameraRoot[i0].Interest);
                    RMT(ref Data.CameraRoot[i0].VP.   MT);
                    RKN(ref Data.CameraRoot[i0].VP.FocalLength);
                    RKN(ref Data.CameraRoot[i0].VP.FOV        );
                    RKN(ref Data.CameraRoot[i0].VP.Roll       );
                }

            if (Data.Chara != null)
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    RMT(ref Data.Chara[i0].MT);

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
                    RRGBAKN(ref Data.Fog[i0].Color  );
                    RKN    (ref Data.Fog[i0].Density);
                    RKN    (ref Data.Fog[i0].End    );
                    RKN    (ref Data.Fog[i0].Start  );
                }

            if (Data.Light != null)
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    RRGBAKN(ref Data.Light[i0].Ambient      );
                    if (Head.Format == Format.XHD)
                    {
                        RKN    (ref Data.Light[i0].ConeAngle    );
                        RKN    (ref Data.Light[i0].Constant     );
                    }
                    RRGBAKN(ref Data.Light[i0].Diffuse      );
                    if (Head.Format == Format.XHD)
                    {
                        RKN    (ref Data.Light[i0].DropOff      );
                        RKN    (ref Data.Light[i0].Far          );
                    }
                    RRGBAKN(ref Data.Light[i0].ToneCurve);
                    if (Head.Format == Format.XHD)
                    {
                        RKN    (ref Data.Light[i0].Intensity    );
                        RKN    (ref Data.Light[i0].Linear       );
                    }
                    RMT    (ref Data.Light[i0].Position     );
                    if (Head.Format == Format.XHD)
                        RKN    (ref Data.Light[i0].Quadratic    );
                    RRGBAKN(ref Data.Light[i0].Specular     );
                    RMT    (ref Data.Light[i0].SpotDirection);
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
                    RRGBAKN(ref Data.MaterialList[i0].BlendColor   );
                    RKN    (ref Data.MaterialList[i0].GlowIntensity);
                    RRGBAKN(ref Data.MaterialList[i0].Incandescence);
                }

            if (Data.Object != null)
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    RMT(ref Data.Object[i0].MT);
                    if (Data.Object[i0].TexTrans != null)
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                        {
                            RKN(ref Data.Object[i0].TexTrans[i1].CoverageU      );
                            RKN(ref Data.Object[i0].TexTrans[i1].CoverageV      );
                            RKN(ref Data.Object[i0].TexTrans[i1].OffsetU        );
                            RKN(ref Data.Object[i0].TexTrans[i1].OffsetV        );
                            RKN(ref Data.Object[i0].TexTrans[i1].RepeatU        );
                            RKN(ref Data.Object[i0].TexTrans[i1].RepeatV        );
                            RKN(ref Data.Object[i0].TexTrans[i1].   Rotate      );
                            RKN(ref Data.Object[i0].TexTrans[i1].   RotateFrame );
                            RKN(ref Data.Object[i0].TexTrans[i1].TranslateFrameU);
                            RKN(ref Data.Object[i0].TexTrans[i1].TranslateFrameV);
                        }
                }

            if (Data.ObjectHRC != null)
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                    if (Data.ObjectHRC[i0].Node != null)
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            RMT(ref Data.ObjectHRC[i0].Node[i1].MT);


            if (Data.Point != null)
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    RMT(ref Data.Point[i0].MT);

            if (Data.PostProcess != null)
            {
                PostProcess pp = Data.PostProcess.Value;
                RRGBAKN(ref pp.Radius   );
                RRGBAKN(ref pp.Intensity);
                RRGBAKN(ref pp.SceneFade);
                RKN    (ref pp.LensFlare);
                RKN    (ref pp.LensGhost);
                RKN    (ref pp.LensShaft);
                Data.PostProcess = pp;
            }
        }

        public byte[] A3DCWriter()
        {
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

                if (Data.Chara != null)
                    for (i0 = 0; i0 < Data.Chara.Length; i0++)
                        WO(ref Data.Chara[i0].MT, ReturnToOffset);

                if (Data.Light != null)
                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        WO(ref Data.Light[i0].Position     , ReturnToOffset);
                        WO(ref Data.Light[i0].SpotDirection, ReturnToOffset);
                    }

                if (Data.MObjectHRC != null)
                    for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        WO(ref Data.MObjectHRC[i0].MT, ReturnToOffset);

                        if (Data.MObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                WO(ref Data.MObjectHRC[i0].Node[i1].MT, ReturnToOffset);

                        if (Data.MObjectHRC[i0].Instances != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                                WO(ref Data.MObjectHRC[i0].Instances[i1].MT, ReturnToOffset);
                    }

                if (Data.Object != null)
                    for (i0 = 0; i0 < Data.Object.Length; i0++)
                        WO(ref Data.Object[i0].MT, ReturnToOffset);

                if (Data.ObjectHRC != null)
                    for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                        if (Data.ObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                                WO(ref Data.ObjectHRC[i0].Node[i1].MT, ReturnToOffset);

                if (Data.Point != null)
                    for (i0 = 0; i0 < Data.Point.Length; i0++)
                        WO(ref Data.Point[i0].MT, ReturnToOffset);

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
                    W(ref ca.AutoExposure    );
                    W(ref ca.    Exposure    );
                    W(ref ca.    ExposureRate);
                    W(ref ca.Gamma           );
                    W(ref ca.GammaRate       );
                    W(ref ca.Saturate        );
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
                        W(ref Data.Chara[i0].MT);

                if (Data.Curve != null)
                    for (i0 = 0; i0 < Data.Curve.Length; i0++)
                        W(ref Data.Curve[i0].CV);


                if (Data.Light != null)
                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        W(ref Data.Light[i0].Position     );
                        W(ref Data.Light[i0].SpotDirection);

                        W(ref Data.Light[i0].Ambient      );
                        W(ref Data.Light[i0].Diffuse      );
                        W(ref Data.Light[i0].Specular     );
                        W(ref Data.Light[i0].ToneCurve    );

                        if (Head.Format == Format.XHD)
                        {
                            W(ref Data.Light[i0].Intensity    );
                            W(ref Data.Light[i0].Far          );
                            W(ref Data.Light[i0].Constant     );
                            W(ref Data.Light[i0].Linear       );
                            W(ref Data.Light[i0].Quadratic    );
                            W(ref Data.Light[i0].DropOff      );
                            W(ref Data.Light[i0].ConeAngle    );
                        }
                    }

                if (Data.Fog != null)
                    for (i0 = 0; i0 < Data.Fog.Length; i0++)
                    {
                        W(ref Data.Fog[i0].Density);
                        W(ref Data.Fog[i0].Start  );
                        W(ref Data.Fog[i0].End    );

                        W(ref Data.Fog[i0].Color);
                    }

                if (Data.MObjectHRC != null)
                    for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        W(ref Data.MObjectHRC[i0].MT);

                        if (Data.MObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                W(ref Data.MObjectHRC[i0].Node[i1].MT);

                        if (Data.MObjectHRC[i0].Instances != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                                W(ref Data.MObjectHRC[i0].Instances[i1].MT);
                    }

                if (Data.MaterialList != null && (Head.Format == Format.X || Head.Format == Format.XHD))
                    for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                    {
                        W(ref Data.MaterialList[i0].GlowIntensity);

                        W(ref Data.MaterialList[i0].BlendColor   );
                        W(ref Data.MaterialList[i0].Incandescence);
                    }

                if (Data.Object != null)
                    for (i0 = 0; i0 < Data.Object.Length; i0++)
                    {
                        W(ref Data.Object[i0].MT);
                        if (Data.Object[i0].TexTrans != null)
                            for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                            {
                                W(ref Data.Object[i0].TexTrans[i1].CoverageU      );
                                W(ref Data.Object[i0].TexTrans[i1].CoverageV      );
                                W(ref Data.Object[i0].TexTrans[i1].RepeatU        );
                                W(ref Data.Object[i0].TexTrans[i1].RepeatV        );
                                W(ref Data.Object[i0].TexTrans[i1].OffsetU        );
                                W(ref Data.Object[i0].TexTrans[i1].OffsetV        );
                                W(ref Data.Object[i0].TexTrans[i1].   Rotate      );
                                W(ref Data.Object[i0].TexTrans[i1].   RotateFrame );
                                W(ref Data.Object[i0].TexTrans[i1].TranslateFrameU);
                                W(ref Data.Object[i0].TexTrans[i1].TranslateFrameV);
                            }
                    }

                if (Data.ObjectHRC != null)
                    for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                        if (Data.ObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                                W(ref Data.ObjectHRC[i0].Node[i1].MT);

                if (Data.Point != null)
                    for (i0 = 0; i0 < Data.Point.Length; i0++)
                        W(ref Data.Point[i0].MT);

                if (Data.PostProcess != null)
                {
                    PostProcess pp = Data.PostProcess.Value;
                    W(ref pp.LensFlare);
                    W(ref pp.LensShaft);
                    W(ref pp.LensGhost);

                    W(ref pp.Intensity);
                    W(ref pp.Radius   );
                    W(ref pp.SceneFade);
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
            s.WE((ushort)0x02, true);
            s.WE((ushort)0x10, true);
            s.W('P');
            s.A(0x04);
            s.WE(Head.StringOffset, true);
            s.WE(Head.StringLength, true);
            s.WE(0x01, true);
            s.W('B');
            s.W('L');
            s.A(0x04);
            s.WE(Head.BinaryOffset, true);
            s.WE(Head.BinaryLength, true);
            s.WE(0x20, true);

            if (Head.Format > Format.AFT && Head.Format < Format.FT)
            {
                s.PU32 = A3DCEnd;
                s.WEOFC(0);
                s.O = 0;
                s.P = 0;
                Header header = new Header { Signature = 0x41443341,
                    InnerSignature = Head.Format == Format.XHD ? 0x00131010u : 0x01131010u,
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

        private void RRGBAKN(ref Vec4<Key?>? rgba)
        {
            if (!rgba.HasValue) return;
            Vec4<Key?> rgbav = rgba.Value;
            RKN(ref rgbav.X); RKN(ref rgbav.Y);
            RKN(ref rgbav.Z); RKN(ref rgbav.W);
            rgba = rgbav;
        }

        private void RV3(ref Vec3<Key> key, bool f16 = false)
        { RK(ref key.X, f16); RK(ref key.Y, f16); RK(ref key.Z, f16); }

        private void RKN(ref Key? key)
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
            int length = s.RI32();
            key.EPTypePost = (EPType)((Type >> 12) & 0xF);
            key.EPTypePre  = (EPType)((Type >>  8) & 0xF);
            key.Keys = new KFT3[length];

            if (f16 && Data._.CompressF16 == CompressF16.Type2)
                for (int i = 0; i < key.Keys.Length; i++)
                { ref KFT3 kf = ref key.Keys[i]; kf.F  = s.RI16(); kf.V  = s.RF16();
                                                 kf.T1 = s.RF16(); kf.T2 = s.RF16(); }
            else if (f16 && Data._.CompressF16 > 0)
                for (int i = 0; i < key.Keys.Length; i++)
                { ref KFT3 kf = ref key.Keys[i]; kf.F  = s.RI16(); kf.V  = s.RF16();
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

        private void W(ref Vec4<Key?>? rgba)
        {
            if (!rgba.HasValue || Head.Format == Format.AFT) return;

            Vec4<Key?> rgbav = rgba.Value;
            W(ref rgbav.X); W(ref rgbav.Y);
            W(ref rgbav.Z); W(ref rgbav.W);
            rgba = rgbav;
        }

        private void W(ref Vec3<Key> key, bool f16 = false)
        { W(ref key.X, f16); W(ref key.Y, f16); W(ref key.Z, f16); }

        private void W(ref Key? key)
        { if (!key.HasValue) return; Key k = key.Value; W(ref k); key = k; }

        private void W(ref Key key, bool f16 = false)
        {
            key.BinOffset = s.P;
            if (key.Type > KeyType.Static && key.Keys != null)
            {
                int i = 0;
                int Type = (int)key.Type & 0xFF;
                Type |= ((int)key.EPTypePost & 0xF) << 12;
                Type |= ((int)key.EPTypePre  & 0xF) <<  8;
                s.W(Type);
                s.W(0x00);
                s.W((float)(key.Max ?? Data.PlayControl.Size));
                s.W(key.Keys.Length);

                if (f16 && Data._.CompressF16 == CompressF16.Type2)
                    for (i = 0; i < key.Keys.Length; i++)
                    { ref KFT3 kf = ref key.Keys[i]; s.W((short)kf.F.Round()); s.W((Half)kf.V );
                                                     s.W(( Half)kf.T1       ); s.W((Half)kf.T2); }
                else if (f16 && Data._.CompressF16 > 0)
                    for (i = 0; i < key.Keys.Length; i++)
                    { ref KFT3 kf = ref key.Keys[i]; s.W((short)kf.F.Round()); s.W((Half)kf.V );
                                                     s.W(       kf.T1       ); s.W(      kf.T2); }
                else
                    for (i = 0; i < key.Keys.Length; i++)
                    { ref KFT3 kf = ref key.Keys[i]; s.W(new Vec4(kf.F, kf.V, kf.T1, kf.T2)); }
            }
            else if (key.Type == KeyType.Static) { s.W((int)key.Type); s.W((float)key.Value); }
            else s.W(0x00L);
        }

        public void A3DAMerger(ref Data mData)
        {
            if (Data.Ambient != null && mData.Ambient != null)
                for (i0 = 0; i0 < Data.Ambient.Length && i0 < mData.Ambient.Length; i0++)
                {
                    MRGBAKN(ref Data.Ambient[i0].   LightDiffuse, ref mData.Ambient[i0].   LightDiffuse);
                    MRGBAKN(ref Data.Ambient[i0].RimLightDiffuse, ref mData.Ambient[i0].RimLightDiffuse);
                }

            if (Data.CameraAuxiliary != null && mData.CameraAuxiliary != null)
            {
                CameraAuxiliary mCA = mData.CameraAuxiliary.Value;
                CameraAuxiliary ca = Data.CameraAuxiliary.Value;
                MKN(ref ca.AutoExposure    , ref mCA.AutoExposure    );
                MKN(ref ca.    Exposure    , ref mCA.    Exposure    );
                MKN(ref ca.    ExposureRate, ref mCA.    ExposureRate);
                MKN(ref ca.Gamma           , ref mCA.Gamma           );
                MKN(ref ca.GammaRate       , ref mCA.GammaRate       );
                MKN(ref ca.Saturate        , ref mCA.Saturate        );
                Data.CameraAuxiliary = ca;
                mData.CameraAuxiliary = mCA;
            }

            if (Data.CameraRoot != null && mData.CameraRoot != null)
                for (i0 = 0; i0 < Data.CameraRoot.Length && i0 < mData.CameraRoot.Length; i0++)
                {
                    MMT(ref Data.CameraRoot[i0].      MT, ref mData.CameraRoot[i0].      MT);
                    MMT(ref Data.CameraRoot[i0].Interest, ref mData.CameraRoot[i0].Interest);
                    MMT(ref Data.CameraRoot[i0].VP.   MT, ref mData.CameraRoot[i0].VP.   MT);
                    MKN(ref Data.CameraRoot[i0].VP.FocalLength, ref mData.CameraRoot[i0].VP.FocalLength);
                    MKN(ref Data.CameraRoot[i0].VP.FOV        , ref mData.CameraRoot[i0].VP.FOV        );
                    MKN(ref Data.CameraRoot[i0].VP.Roll       , ref mData.CameraRoot[i0].VP.Roll       );
                }

            if (Data.Chara != null && mData.Chara != null)
                for (i0 = 0; i0 < Data.Chara.Length && i0 < mData.Chara.Length; i0++)
                    MMT(ref Data.Chara[i0].MT, ref mData.Chara[i0].MT);

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
                    MRGBAKN(ref Data.Fog[i0].Color  , ref mData.Fog[i0].Color  );
                    MKN    (ref Data.Fog[i0].Density, ref mData.Fog[i0].Density);
                    MKN    (ref Data.Fog[i0].End    , ref mData.Fog[i0].End    );
                    MKN    (ref Data.Fog[i0].Start  , ref mData.Fog[i0].Start  );
                }

            if (Data.Light != null && mData.Light != null)
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    MRGBAKN(ref Data.Light[i0].Ambient      , ref mData.Light[i0].Ambient      );
                    if (Head.Format == Format.XHD)
                    {
                        MKN    (ref Data.Light[i0].ConeAngle    , ref mData.Light[i0].ConeAngle    );
                        MKN    (ref Data.Light[i0].Constant     , ref mData.Light[i0].Constant     );
                    }
                    MRGBAKN(ref Data.Light[i0].Diffuse      , ref mData.Light[i0].Diffuse      );
                    if (Head.Format == Format.XHD)
                    {
                        MKN    (ref Data.Light[i0].DropOff      , ref mData.Light[i0].DropOff      );
                        MKN    (ref Data.Light[i0].Far          , ref mData.Light[i0].Far          );
                    }
                    MRGBAKN(ref Data.Light[i0].ToneCurve, ref mData.Light[i0].ToneCurve);
                    if (Head.Format == Format.XHD)
                    {
                        MKN    (ref Data.Light[i0].Intensity    , ref mData.Light[i0].Intensity    );
                        MKN    (ref Data.Light[i0].Linear       , ref mData.Light[i0].Linear       );
                    }
                    MMT    (ref Data.Light[i0].Position     , ref mData.Light[i0].Position     );
                    if (Head.Format == Format.XHD)
                        MKN    (ref Data.Light[i0].Quadratic    , ref mData.Light[i0].Quadratic);
                    MRGBAKN(ref Data.Light[i0].Specular     , ref mData.Light[i0].Specular     );
                    MMT    (ref Data.Light[i0].SpotDirection, ref mData.Light[i0].SpotDirection);
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
                        ref ObjectNode[] mN = ref mData.MObjectHRC[i0].Node;
                        ref ObjectNode[]  n = ref  Data.MObjectHRC[i0].Node;
                        for (i1 = 0; i1 < n.Length && i1 < mN.Length; i1++)
                            MMT(ref n[i1].MT, ref mN[i1].MT);
                    }
                }

            if (Data.MaterialList != null && mData.MaterialList != null)
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    ref MaterialList mML = ref mData.MaterialList[i0];
                    ref MaterialList  ml = ref  Data.MaterialList[i0];
                    MRGBAKN(ref ml.BlendColor   , ref mML.BlendColor   );
                    MKN    (ref ml.GlowIntensity, ref mML.GlowIntensity);
                    MRGBAKN(ref ml.Incandescence, ref mML.Incandescence);
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
                            MKN(ref tt.CoverageU      , ref mTT.CoverageU      );
                            MKN(ref tt.CoverageV      , ref mTT.CoverageV      );
                            MKN(ref tt.OffsetU        , ref mTT.OffsetU        );
                            MKN(ref tt.OffsetV        , ref mTT.OffsetV        );
                            MKN(ref tt.RepeatU        , ref mTT.RepeatU        );
                            MKN(ref tt.RepeatV        , ref mTT.RepeatV        );
                            MKN(ref tt.   Rotate      , ref mTT.   Rotate      );
                            MKN(ref tt.   RotateFrame , ref mTT.   RotateFrame );
                            MKN(ref tt.TranslateFrameU, ref mTT.TranslateFrameU);
                            MKN(ref tt.TranslateFrameV, ref mTT.TranslateFrameV);
                        }
                }

            if (Data.ObjectHRC != null && mData.ObjectHRC != null)
                for (i0 = 0; i0 < Data.ObjectHRC.Length && i0 < mData.ObjectHRC.Length; i0++)
                    if (Data.ObjectHRC[i0].Node != null && mData.ObjectHRC[i0].Node != null)
                    {
                        ref ObjectNode[] mN = ref mData.ObjectHRC[i0].Node;
                        ref ObjectNode[]  n = ref  Data.ObjectHRC[i0].Node;
                        for (i1 = 0; i1 < n.Length && i1 < mN.Length; i1++)
                            MMT(ref n[i1].MT, ref mN[i1].MT);
                    }


            if (Data.Point != null && mData.Point != null)
                for (i0 = 0; i0 < Data.Point.Length && i0 < mData.Point.Length; i0++)
                    MMT(ref Data.Point[i0].MT, ref mData.Point[i0].MT);

            if (Data.PostProcess != null && mData.PostProcess != null)
            {
                PostProcess mPP = mData.PostProcess.Value;
                PostProcess  pp =  Data.PostProcess.Value;
                MRGBAKN(ref pp.Radius   , ref mPP.Radius   );
                MRGBAKN(ref pp.Intensity, ref mPP.Intensity);
                MRGBAKN(ref pp.SceneFade, ref mPP.SceneFade);
                MKN    (ref pp.LensFlare, ref mPP.LensFlare);
                MKN    (ref pp.LensGhost, ref mPP.LensGhost);
                MKN    (ref pp.LensShaft, ref mPP.LensShaft);
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

        private void MRGBAKN(ref Vec4<Key?>? rgba, ref Vec4<Key?>? mRGBA)
        {
            if (!rgba.HasValue) return;

            Vec4<Key?> rgbav = rgba.Value;
            Vec4<Key?> mrgbav = mRGBA.Value;
            MKN(ref rgbav.X, ref mrgbav.X); MKN(ref rgbav.Y, ref mrgbav.Y);
            MKN(ref rgbav.Z, ref mrgbav.Z); MKN(ref rgbav.W, ref mrgbav.W);
            mRGBA = mrgbav;
        }

        private void MV3(ref Vec3<Key> key, ref Vec3<Key> mKey)
        { MK(ref key.X, ref mKey.X); MK(ref key.Y, ref mKey.Y); MK(ref key.Z, ref mKey.Z); }

        private void MKN(ref Key? key, ref Key? mKey)
        { if (!key.HasValue || !mKey.HasValue) return;
          Key k = key.Value; Key mk = mKey.Value; MK(ref k, ref mk); key = k; mKey = mk; }

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
                key.Max = (int)key.Keys[key.Keys.Length - 1].F + 1;
                return;
            }
            else if (key.Keys.Length == 1 && mKey.Keys.Length > 1)
            {
                if (key.Keys[0] == mKey.Keys[0])
                {
                    key.Keys = mKey.Keys;
                    key.Max = (int)mKey.Keys[mKey.Keys.Length - 1].F + 1;
                    return;
                }
                return;
            }

            int i = key.Keys.Length;
            bool found = false;
            while (i > 0 && !found)
                if (key.Keys[--i] == mKey.Keys[0])
                    found = true;

            if (!found) return;

            int OldLength = i;
            Array.Resize(ref key.Keys, OldLength + mKey.Keys.Length);
            Array.Copy(mKey.Keys, 0, key.Keys, OldLength, mKey.Keys.Length);
            key.Max = (int)key.Keys[key.Keys.Length - 1].F + 1;
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
            MsgPack temp = MsgPack.New, temp1 = MsgPack.New, temp2 = MsgPack.New;
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
                                   Name = temp[i].RS     (           "Name"),
                           LightDiffuse = temp[i].RRGBAKN(   "LightDiffuse"),
                        RimLightDiffuse = temp[i].RRGBAKN("RimLightDiffuse"),
                    };
            }

            if ((temp = a3d["Auth2D", true]).NotNull)
            {
                Data.Auth2D = new string[temp.Array.Length];

                for (i = 0; i < Data.Auth2D.Length; i++)
                    Data.Auth2D[i] = temp[i].RS();
            }

            if ((temp = a3d["CameraAuxiliary"]).NotNull)
                Data.CameraAuxiliary = new CameraAuxiliary
                {
                    AutoExposure     = temp.RKN("AutoExposure"    ),
                        Exposure     = temp.RKN(    "Exposure"    ),
                        ExposureRate = temp.RKN(    "ExposureRate"),
                    Gamma            = temp.RKN("Gamma"           ),
                    GammaRate        = temp.RKN("GammaRate"       ),
                    Saturate         = temp.RKN("Saturate"        ),
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

                    if (temp1["FOVHorizontal"].Object != null || temp1["FOVIsHorizontal"].Object != null)
                        Data.CameraRoot[i].VP = new CameraRoot.ViewPoint
                        {
                            MT              = temp1.RMT(),
                            Aspect          = temp1.RF32("Aspect"),
                            FOVIsHorizontal = temp1.RB(temp1["FOVHorizontal"].Object != null
                                ? "FOVHorizontal" : "FOVIsHorizontal"),
                            FOV             = temp1.RK  ("FOV"   ),
                            Roll            = temp1.RK  ("Roll"  ),
                        };
                    else
                        Data.CameraRoot[i].VP = new CameraRoot.ViewPoint
                        {
                            MT              = temp1.RMT(),
                            Aspect          = temp1.RF32("Aspect"         ),
                            CameraApertureH = temp1.RF32("CameraApertureH"),
                            CameraApertureW = temp1.RF32("CameraApertureW"),
                            FocalLength     = temp1.RK  ("FocalLength"    ),
                            Roll            = temp1.RK  ("Roll"           ),
                        };
                }
            }

            if ((temp = a3d["Chara", true]).NotNull)
            {
                Data.Chara = new Chara[temp.Array.Length];
                for (i = 0; i < Data.Chara.Length; i++)
                    Data.Chara[i] = new Chara
                    {
                        MT   = temp[i].RMT(),
                        Name = temp[i].RS("Name"),
                    };
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
                Data.DOF = new DOF { MT = temp.RMT() };

            if ((temp = a3d["Event", true]).NotNull)
            {
                Data.Event = new Event[temp.Array.Length];
                for (i = 0; i < Data.Event.Length; i++)
                    Data.Event[i] = new Event
                    {
                            Begin    = temp[i].RF32(    "Begin"   ),
                        ClipBegin    = temp[i].RF32("ClipBegin"   ),
                        ClipEnd      = temp[i].RF32("ClipEnd"     ),
                            End      = temp[i].RF32(    "End"     ),
                        Name         = temp[i].RS  ("Name"        ),
                        Param1       = temp[i].RS  ("Param1"      ),
                        Ref          = temp[i].RS  ("Ref"         ),
                        TimeRefScale = temp[i].RF32("TimeRefScale"),
                        Type         = temp[i].RI32("Type"        ),
                    };
            }

            if ((temp = a3d["Fog", true]).NotNull)
            {
                Data.Fog = new Fog[temp.Array.Length];
                for (i = 0; i < Data.Fog.Length; i++)
                    if (temp1[i]["Diffuse"].NotNull)
                        Data.Fog[i] = new Fog
                        {
                            Color   = temp[i].RRGBAKN("Diffuse"),
                            Id      = temp[i].RI32   ("Id"     ),
                            Density = temp[i].RKN    ("Density"),
                            End     = temp[i].RKN    ("End"    ),
                            Start   = temp[i].RKN    ("Start"  ),
                        };
                    else
                        Data.Fog[i] = new Fog
                        {
                            Color   = temp[i].RRGBAKN("Color"  ),
                            Id      = temp[i].RI32   ("Id"     ),
                            Density = temp[i].RKN    ("Density"),
                            End     = temp[i].RKN    ("End"    ),
                            Start   = temp[i].RKN    ("Start"  ),
                        };
            }

            if ((temp = a3d["Light", true]).NotNull)
            {
                Data.Light = new Light[temp.Array.Length];
                for (i = 0; i < Data.Light.Length; i++)
                    if (temp1[i]["Incandescence"].NotNull)
                        Data.Light[i] = new Light
                        {
                            Id            = temp[i].RI32   ("Id"           ),
                            Type          = temp[i].RS     ("Type"         ),
                            Ambient       = temp[i].RRGBAKN("Ambient"      ),
                            ConeAngle     = temp[i].RKN    ("ConeAngle"    ),
                            Constant      = temp[i].RKN    ("Constant"     ),
                            Diffuse       = temp[i].RRGBAKN("Diffuse"      ),
                            DropOff       = temp[i].RKN    ("DropOff"      ),
                            Far           = temp[i].RKN    ("Far"          ),
                            ToneCurve     = temp[i].RRGBAKN("Incandescence"),
                            Intensity     = temp[i].RKN    ("Intensity"    ),
                            Linear        = temp[i].RKN    ("Linear"       ),
                            Position      = temp[i].RMT    ("Position"     ),
                            Quadratic     = temp[i].RKN    ("Quadratic"    ),
                            Specular      = temp[i].RRGBAKN("Specular"     ),
                            SpotDirection = temp[i].RMT    ("SpotDirection"),
                        };
                    else
                        Data.Light[i] = new Light
                        {
                            Id            = temp[i].RI32   ("Id"           ),
                            Type          = temp[i].RS     ("Type"         ),
                            Ambient       = temp[i].RRGBAKN("Ambient"      ),
                            ConeAngle     = temp[i].RKN    ("ConeAngle"    ),
                            Constant      = temp[i].RKN    ("Constant"     ),
                            Diffuse       = temp[i].RRGBAKN("Diffuse"      ),
                            DropOff       = temp[i].RKN    ("DropOff"      ),
                            Far           = temp[i].RKN    ("Far"          ),
                            Intensity     = temp[i].RKN    ("Intensity"    ),
                            Linear        = temp[i].RKN    ("Linear"       ),
                            Position      = temp[i].RMT    ("Position"     ),
                            Quadratic     = temp[i].RKN    ("Quadratic"    ),
                            Specular      = temp[i].RRGBAKN("Specular"     ),
                            SpotDirection = temp[i].RMT    ("SpotDirection"),
                            ToneCurve     = temp[i].RRGBAKN("ToneCurve"    ),
                        };
            }

            if ((temp = a3d["MaterialList", true]).NotNull)
            {
                Data.MaterialList = new MaterialList[temp.Array.Length];
                for (i = 0; i < Data.MaterialList.Length; i++)
                    Data.MaterialList[i] = new MaterialList
                    {
                            Name      = temp[i].RS     (         "Name"),
                        BlendColor    = temp[i].RRGBAKN(   "BlendColor"),
                        GlowIntensity = temp[i].RKN    ("GlowIntensity"),
                        Incandescence = temp[i].RRGBAKN("Incandescence"),
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

                    if ((temp1 = temp[i0]["Instance", true]).NotNull)
                    {
                        Data.MObjectHRC[i0].Instances = new MObjectHRC.Instance[temp1.Array.Length];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                            Data.MObjectHRC[i0].Instances[i1] = new MObjectHRC.Instance
                            {
                                     MT = temp1[i1].RMT(),
                                   Name = temp1[i1].RS(   "Name"),
                                 Shadow = temp1[i1].RB( "Shadow"),
                                UIDName = temp1[i1].RS("UIDName"),
                            };
                    }

                    if ((temp1 = temp[i0]["Node", true]).NotNull)
                    {
                        Data.MObjectHRC[i0].Node = new ObjectNode[temp1.Array.Length];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            Data.MObjectHRC[i0].Node[i1] = new ObjectNode
                            {
                                    MT = temp1[i1].RMT(),
                                  Name = temp1[i1].RS  (  "Name"),
                                Parent = temp1[i1].RI32("Parent"),
                            };

                            if ((temp2 = temp1[i1]["JointOrient"]).NotNull)
                                Data.MObjectHRC[i0].Node[i1].JointOrient = new Vec3
                                {
                                    X = temp1.RF32("X"),
                                    Y = temp1.RF32("Y"),
                                    Z = temp1.RF32("Z"),
                                };
                        }
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
                        Morph       = temp[i0].RS  ("Morph"      ),
                        MorphOffset = temp[i0].RF32("MorphOffset"),
                               Name = temp[i0].RS  (       "Name"),
                         ParentName = temp[i0].RS  ( "ParentName"),
                         ParentNode = temp[i0].RS  ( "ParentNode"),
                          Pat       = temp[i0].RS  (  "Pat"      ),
                          PatOffset = temp[i0].RF32(  "PatOffset"),
                            UIDName = temp[i0].RS  (    "UIDName"),
                    };

                    if ((temp1 = temp[i0]["TexturePattern", true]).NotNull)
                    {
                        Data.Object[i0].TexPat = new Object.TexturePattern[temp1.Array.Length];
                        for (i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                            Data.Object[i0].TexPat[i1] = new Object.TexturePattern
                            {
                                Name      = temp1[i1].RS  ("Name"     ),
                                Pat       = temp1[i1].RS  ("Pat"      ),
                                PatOffset = temp1[i1].RI32("PatOffset"),
                            };
                    }

                    if ((temp1 = temp[i0]["TextureTransform", true]).NotNull)
                    {
                        Data.Object[i0].TexTrans = new Object.TextureTransform[temp1.Array.Length];
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                        {
                            ref Object.TextureTransform texTrans = ref Data.Object[i0].TexTrans[i1];
                            texTrans = new Object.TextureTransform
                            {
                                Name            = temp1[i1].RS ("Name"           ),
                                CoverageU       = temp1[i1].RKN("CoverageU"      ),
                                CoverageV       = temp1[i1].RKN("CoverageV"      ),
                                OffsetU         = temp1[i1].RKN("OffsetU"        ),
                                OffsetV         = temp1[i1].RKN("OffsetV"        ),
                                RepeatU         = temp1[i1].RKN("RepeatU"        ),
                                RepeatV         = temp1[i1].RKN("RepeatV"        ),
                                   Rotate       = temp1[i1].RKN(   "Rotate"      ),
                                   RotateFrame  = temp1[i1].RKN(   "RotateFrame" ),
                                TranslateFrameU = temp1[i1].RKN("TranslateFrameU"),
                                TranslateFrameV = temp1[i1].RKN("TranslateFrameV"),
                            };

                            rkuv(temp1[i1], "Coverage",
                                ref texTrans.CoverageU      , ref texTrans.CoverageV      );
                            rkuv(temp1[i1], "Offset",
                                ref texTrans.OffsetU        , ref texTrans.OffsetV        );
                            rkuv(temp1[i1], "Repeat",
                                ref texTrans.RepeatU        , ref texTrans.RepeatV        );
                            rkuv(temp1[i1], "TranslateFrame",
                                ref texTrans.TranslateFrameU, ref texTrans.TranslateFrameV);

                            static void rkuv(MsgPack msgPack, string name, ref Key? u, ref Key? v)
                            {
                                if (!u.HasValue)
                                    u = msgPack[name].RKN("U");
                                if (!v.HasValue)
                                    v = msgPack[name].RKN("V");
                            }
                        }
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
                              Name = temp[i0].RS(      "Name"),
                        ParentName = temp[i0].RS("ParentName"),
                        ParentNode = temp[i0].RS("ParentNode"),
                            Shadow = temp[i0].RB(    "Shadow"),
                           UIDName = temp[i0].RS(   "UIDName"),
                    };

                    if ((temp1 = temp[i0]["Node", true]).NotNull)
                    {
                        Data.ObjectHRC[i0].Node = new ObjectNode[temp1.Array.Length];
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        {
                            Data.ObjectHRC[i0].Node[i1] = new ObjectNode
                            {
                                    MT = temp1[i1].RMT(),
                                  Name = temp1[i1].RS  (  "Name"),
                                Parent = temp1[i1].RI32("Parent"),
                            };

                            if ((temp2 = temp1[i1]["JointOrient"]).NotNull)
                                Data.ObjectHRC[i0].Node[i1].JointOrient = new Vec3
                                {
                                    X = temp1.RF32("X"),
                                    Y = temp1.RF32("Y"),
                                    Z = temp1.RF32("Z"),
                                };
                        }
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
                Data.Point = new Point[temp.Array.Length];
                for (i = 0; i < Data.Point.Length; i++)
                    Data.Point[i] = new Point
                    {
                        MT   = temp[i].RMT(),
                        Name = temp[i].RS("Name"),
                    };
            }

            if ((temp = a3d["PostProcess"]).NotNull)
                if (temp["Ambient"].NotNull)
                    Data.PostProcess = new PostProcess
                    {
                        Radius    = temp.RRGBAKN("Ambient"  ),
                        Intensity = temp.RRGBAKN("Diffuse"  ),
                        LensFlare = temp.RKN    ("LensFlare"),
                        LensGhost = temp.RKN    ("LensGhost"),
                        LensShaft = temp.RKN    ("LensShaft"),
                        SceneFade = temp.RRGBAKN("Specular" ),
                    };
                else
                    Data.PostProcess = new PostProcess
                    {
                        Intensity = temp.RRGBAKN("Intensity"),
                        LensFlare = temp.RKN    ("LensFlare"),
                        LensGhost = temp.RKN    ("LensGhost"),
                        LensShaft = temp.RKN    ("LensShaft"),
                        Radius    = temp.RRGBAKN("Radius"),
                        SceneFade = temp.RRGBAKN("SceneFade"),
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

            if (Data.Auth2D != null)
            {
                MsgPack auth2D = new MsgPack(Data.Ambient.Length, "Auth2D");
                for (i = 0; i < Data.Ambient.Length; i++)
                    auth2D[i] = new MsgPack(null, Data.Ambient[i].Name);
                a3d.Add(auth2D);
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
                    if (Data.CameraRoot[i].VP.FOV.HasValue)
                        cameraRoot[i] = MsgPack.New
                            .Add("Interest", ref Data.CameraRoot[i].Interest)
                            .Add(new MsgPack("ViewPoint")
                            .Add("Aspect"         ,     Data.CameraRoot[i].VP.Aspect         )
                            .Add("FOVIsHorizontal",     Data.CameraRoot[i].VP.FOVIsHorizontal)
                            .Add("FOV"            , ref Data.CameraRoot[i].VP.FOV            )
                            .Add("Roll"           , ref Data.CameraRoot[i].VP.Roll           )
                            .Add(ref Data.CameraRoot[i].VP.MT))
                            .Add(ref Data.CameraRoot[i].   MT);
                    else
                        cameraRoot[i] = MsgPack.New
                            .Add("Interest", ref Data.CameraRoot[i].Interest)
                            .Add(new MsgPack("ViewPoint")
                            .Add("Aspect"         ,     Data.CameraRoot[i].VP.Aspect         )
                            .Add("CameraApertureH",     Data.CameraRoot[i].VP.CameraApertureH)
                            .Add("CameraApertureW",     Data.CameraRoot[i].VP.CameraApertureW)
                            .Add("FocalLength"    , ref Data.CameraRoot[i].VP.FocalLength    )
                            .Add("Roll"           , ref Data.CameraRoot[i].VP.Roll           )
                            .Add(ref Data.CameraRoot[i].VP.MT))
                            .Add(ref Data.CameraRoot[i].   MT);
                a3d.Add(cameraRoot);
            }

            if (Data.Chara != null)
            {
                MsgPack chara = new MsgPack(Data.Chara.Length, "Chara");
                for (i = 0; i < Data.Chara.Length; i++)
                    chara[i] = MsgPack.New.Add("Name", Data.Chara[i].Name).Add(ref Data.Chara[i].MT);
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
                a3d.Add(new MsgPack("DOF").Add(ref dof.MT));
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
                    fog[i] = MsgPack.New.Add("Id"     , Data.Fog[i].Id)
                                        .Add("Color"  , ref Data.Fog[i].Color  )
                                        .Add("Density", ref Data.Fog[i].Density)
                                        .Add("End"    , ref Data.Fog[i].End    )
                                        .Add("Start"  , ref Data.Fog[i].Start  );
                a3d.Add(fog);
            }

            if (Data.Light != null)
            {
                MsgPack light = new MsgPack(Data.Light.Length, "Light");
                for (i = 0; i < Data.Light.Length; i++)
                    light[i] = MsgPack.New.Add("Id"  , Data.Light[i].Id)
                                          .Add("Type", Data.Light[i].Type)
                                          .Add("Ambient"      , ref Data.Light[i].Ambient      )
                                          .Add("ConeAngle"    , ref Data.Light[i].ConeAngle    )
                                          .Add("Constant"     , ref Data.Light[i].Constant     )
                                          .Add("Diffuse"      , ref Data.Light[i].Diffuse      )
                                          .Add("DropOff"      , ref Data.Light[i].DropOff      )
                                          .Add("Far"          , ref Data.Light[i].Far          )
                                          .Add("Intensity"    , ref Data.Light[i].Intensity    )
                                          .Add("Linear"       , ref Data.Light[i].Linear       )
                                          .Add("Position"     , ref Data.Light[i].Position     )
                                          .Add("Quadratic"    , ref Data.Light[i].Quadratic    )
                                          .Add("Specular"     , ref Data.Light[i].Specular     )
                                          .Add("SpotDirection", ref Data.Light[i].SpotDirection)
                                          .Add("ToneCurve"    , ref Data.Light[i].ToneCurve    );
                a3d.Add(light);
            }

            if (Data.MObjectHRC != null)
            {
                MsgPack mObjectHRC = new MsgPack(Data.MObjectHRC.Length, "MObjectHRC");
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    MsgPack _mObjectHRC = MsgPack.New.Add("Name", Data.MObjectHRC[i0].Name);

                    if (Data.MObjectHRC[i0].Instances != null)
                    {
                        MsgPack instance = new MsgPack(Data.MObjectHRC[i0].Instances.Length, "Instance");
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                        {
                            MsgPack _instance = MsgPack.New.Add("Name", Data.MObjectHRC[i0].Instances[i1].Name);
                            if (Data.MObjectHRC[i0].Instances[i1].Shadow)
                                _instance.Add("Shadow", true);
                            _instance.Add("UIDName", Data.MObjectHRC[i0].Instances[i1].UIDName);
                            instance[i1] = _instance.Add(ref Data.MObjectHRC[i0].Instances[i1].MT);
                        }

                        _mObjectHRC.Add(instance);
                    }

                    if (Data.MObjectHRC[i0].Node != null)
                    {
                        MsgPack node = new MsgPack(Data.MObjectHRC[i0].Node.Length, "Node");
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            MsgPack _node = MsgPack.New
                                .Add("Name"  , Data.MObjectHRC[i0].Node[i1].Name  )
                                .Add("Parent", Data.MObjectHRC[i0].Node[i1].Parent);

                            if (Data.MObjectHRC[i0].Node[i1].JointOrient.HasValue)
                            {
                                Vec3 jointOrient = Data.MObjectHRC[i0].Node[i1].JointOrient.Value;
                                _node.Add(new MsgPack("JointOrient")
                                    .Add("X", jointOrient.X)
                                    .Add("Y", jointOrient.Y)
                                    .Add("Z", jointOrient.Z));
                            }
                            node[i1] = _node.Add(ref Data.MObjectHRC[i0].Node[i1].MT);
                        }
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
                        .Add("Name", Data.MaterialList[i].Name)
                        .Add("BlendColor", ref Data.MaterialList[i].BlendColor)
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
                    MsgPack _object = MsgPack.New;
                    if (Data.Object[i0].Morph != null)
                        _object = _object.Add("Morph"      , Data.Object[i0].Morph      )
                                         .Add("MorphOffset", Data.Object[i0].MorphOffset);
                    _object.Add("Name", Data.Object[i0].Name);
                    if (Data.Object[i0].Pat != null)
                        _object = _object.Add("Pat"      , Data.Object[i0].Pat      )
                                         .Add("PatOffset", Data.Object[i0].PatOffset);
                    _object = _object.Add("ParentName", Data.Object[i0].ParentName)
                                     .Add("ParentNode", Data.Object[i0].ParentNode)
                                     .Add(   "UIDName", Data.Object[i0].   UIDName);
                    if (Data.Object[i0].TexPat != null)
                    {
                        MsgPack texPat = new MsgPack(Data.Object[i0].TexPat.Length, "TexturePattern");
                        for (i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                        {
                            MsgPack _texPat = MsgPack.New.Add("Name", Data.Object[i0].TexPat[i1].Name);
                            if (Data.Object[i0].TexPat[i1].Pat != null)
                                _texPat.Add("Pat"      , Data.Object[i0].TexPat[i1].Pat      )
                                       .Add("PatOffset", Data.Object[i0].TexPat[i1].PatOffset);
                            texPat[i1] = _texPat;
                        }
                        _object.Add(texPat);
                    }
                    if (Data.Object[i0].TexTrans != null)
                    {
                        MsgPack texTrans = new MsgPack(Data.Object[i0]
                            .TexTrans.Length, "TextureTransform");
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                            texTrans[i1] = MsgPack.New
                                .Add("Name", Data.Object[i0].TexTrans[i1].Name)
                                .Add("CoverageU"      , ref Data.Object[i0].TexTrans[i1].CoverageU      )
                                .Add("CoverageV"      , ref Data.Object[i0].TexTrans[i1].CoverageV      )
                                .Add("OffsetU"        , ref Data.Object[i0].TexTrans[i1].OffsetU        )
                                .Add("OffsetV"        , ref Data.Object[i0].TexTrans[i1].OffsetV        )
                                .Add("RepeatU"        , ref Data.Object[i0].TexTrans[i1].RepeatU        )
                                .Add("RepeatV"        , ref Data.Object[i0].TexTrans[i1].RepeatV        )
                                .Add(   "Rotate"      , ref Data.Object[i0].TexTrans[i1].   Rotate      )
                                .Add(   "RotateFrame" , ref Data.Object[i0].TexTrans[i1].   RotateFrame )
                                .Add("TranslateFrameU", ref Data.Object[i0].TexTrans[i1].TranslateFrameU)
                                .Add("TranslateFrameV", ref Data.Object[i0].TexTrans[i1].TranslateFrameV);
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
                    MsgPack _objectHRC = MsgPack.New
                        .Add("Name", Data.ObjectHRC[i0].Name)
                        .Add("ParentName", Data.ObjectHRC[i0].ParentName)
                        .Add("ParentNode", Data.ObjectHRC[i0].ParentNode);
                    if (Data.ObjectHRC[i0].Shadow)
                        _objectHRC.Add("Shadow", true);
                    _objectHRC.Add("UIDName", Data.ObjectHRC[i0].UIDName);

                    if (Data.ObjectHRC[i0].Node != null)
                    {
                        MsgPack node = new MsgPack(Data.ObjectHRC[i0].Node.Length, "Node");
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        {
                            MsgPack _node = MsgPack.New
                                .Add("Name"  , Data.ObjectHRC[i0].Node[i1].Name  )
                                .Add("Parent", Data.ObjectHRC[i0].Node[i1].Parent);

                            if (Data.ObjectHRC[i0].Node[i1].JointOrient.HasValue)
                            {
                                Vec3 jointOrient = Data.ObjectHRC[i0].Node[i1].JointOrient.Value;
                                _node.Add(new MsgPack("JointOrient")
                                    .Add("X", jointOrient.X)
                                    .Add("Y", jointOrient.Y)
                                    .Add("Z", jointOrient.Z));
                            }
                            node[i1] = _node.Add(ref Data.ObjectHRC[i0].Node[i1].MT);
                        }
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
                    point[i] = MsgPack.New.Add("Name", Data.Point[i].Name).Add(ref Data.Point[i].MT);
                a3d.Add(point);
            }

            if (Data.PostProcess != null)
            {
                PostProcess pp = Data.PostProcess.Value;
                a3d.Add(new MsgPack("PostProcess").Add("Intensity", ref pp.Intensity)
                                                  .Add("LensFlare", ref pp.LensFlare)
                                                  .Add("LensGhost", ref pp.LensGhost)
                                                  .Add("LensShaft", ref pp.LensShaft)
                                                  .Add("Radius"   , ref pp.Radius   )
                                                  .Add("SceneFade", ref pp.SceneFade));
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

        public static Vec4<Key?>? RRGBAKN(this MsgPack msgPack, string name) =>
            msgPack[name].NotNull ? (Vec4<Key?>?)msgPack[name].RRGBAK() : null;

        public static Vec4<Key?> RRGBAK(this MsgPack msgPack) =>
            new Vec4<Key?> { X = msgPack.RKN("R"), Y = msgPack.RKN("G"),
                             Z = msgPack.RKN("B"), W = msgPack.RKN("A") };

        public static Vec3<Key> RV3(this MsgPack msgPack, string name) =>
            msgPack[name].RV3();

        public static Vec3<Key> RV3(this MsgPack msgPack) =>
            new Vec3<Key> { X = msgPack.RK("X"), Y = msgPack.RK("Y"), Z = msgPack.RK("Z") };

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

            key.RawData = msgPack.RB("RawData");
            MsgPack trans;
            if ((trans = msgPack["Keys", true]).IsNull && (trans = msgPack["Trans", true]).IsNull) return key;

            int length = trans.Array.Length;
            key.Keys = new KFT3[length];
            for (int i = 0; i < length; i++)
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

        public static MsgPack Add(this MsgPack msgPack, string name, ref Vec4<Key?>? rgba)
        {
            if (!rgba.HasValue) return msgPack;

            Vec4<Key?> rgbav = rgba.Value;
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
                if (key.EPTypePost != EPType.None)
                    keys.Add("EPTypePost", key.EPTypePost.ToString());
                if (key.EPTypePre != EPType.None)
                    keys.Add("EPTypePre", key.EPTypePre.ToString());

                if (key.RawData) keys.Add("RawData", true);

                int length = key.Keys.Length;
                MsgPack Keys = new MsgPack(length, "Keys");
                for (int i = 0; i < length; i++)
                {
                    IKF kf = key.Keys[i].Check();
                         if (kf is KFT0 kft0) Keys[i] = new MsgPack(null,
                        new MsgPack[] { kft0.F });
                    else if (kf is KFT1 kft1) Keys[i] = new MsgPack(null,
                        new MsgPack[] { kft1.F, kft1.V });
                    else if (kf is KFT2 kft2) Keys[i] = new MsgPack(null,
                        new MsgPack[] { kft2.F, kft2.V, kft2.T });
                    else if (kf is KFT3 kft3) Keys[i] = new MsgPack(null,
                        new MsgPack[] { kft3.F, kft3.V, kft3.T1, kft3.T2, });
                }
                keys.Add(Keys);
            }
            else if (key.Type != KeyType.None) keys.Add("Value", key.Value);
            msgPack.Add(keys);
            return msgPack;
        }
    }

    public struct A3DAHeader
    {
        public uint SubHeadersOffset;
        public ushort SubHeadersCount;
        public ushort SubHeadersStride;

        public int BinaryLength;
        public int BinaryOffset;
        public int StringLength;
        public int StringOffset;

        public Format Format;
    }
}
