using System;
using System.Collections.Generic;
using KKdBaseLib;
using KKdBaseLib.F2;
using KKdBaseLib.A3DA;
using KKdMainLib.IO;
using Extensions = KKdBaseLib.Extensions;
using Object = KKdBaseLib.A3DA.Object;

namespace KKdMainLib
{
    public class A3DA : IDisposable
    {
        private bool A3DCOpt = true;
        private const string d = ".";

        private int i, i0, i1;
        private int SOi0;
        private int SOi1;
        private int[] SO0;
        private int[] SO1;
        private string name;
        private string nameView;
        private string value;
        private string[] dataArray;
        private Dictionary<int?, float?> UsedValues;
        private Dictionary<string, object> Dict;

        private bool IsX => Head.Format == Format.X || Head.Format == Format.XHD;

        public Stream IO;
        public A3DAData Data;
        public A3DAHeader Head;

        public A3DA()
        { Data = new A3DAData(); Dict = new Dictionary<string, object>(); UsedValues = new Dictionary<int?, float?>(); }

        public int A3DAReader(string file)
        {
            int Return;
            IO = File.OpenReader(file + ".a3da");
            Return = A3DAReader(ref IO);
            IO.Dispose();
            return Return;
        }

        public int A3DAReader(byte[] data)
        {
            int Return;
            IO = File.OpenReader(data);
            Return = A3DAReader(ref IO);
            IO.Dispose();
            return Return;
        }

        private int A3DAReader(ref Stream IO)
        {
            name = "";
            nameView = "";
            dataArray = new string[4];
            Dict = new Dictionary<string, object>();
            Data = new A3DAData();
            Head = new A3DAHeader();
            Header Header = new Header();

            Head.Format = IO.Format = Format.F;
            Header.SectionSignature = IO.RI32();
            if (Header.SectionSignature == 0x41443341)
            { Header = IO.ReadHeader(true, true); Head.Format = Header.Format; }
            if (Header.SectionSignature != 0x44334123) return 0;

            IO.O = IO.P - 4;
            Header.SectionSignature = IO.RI32();

            if (Header.SectionSignature == 0x5F5F5F41)
            {
                IO.P = 0x10;
                Header.Format = IO.Format = Format.DT;
            }
            else if (Header.SectionSignature == 0x5F5F5F43)
            {
                IO.P = 0x10;
                IO.RI32();
                IO.RI32();
                Head.HeaderOffset = IO.RI32E(true);

                IO.P = Head.HeaderOffset;
                if (IO.RI32() != 0x50) return 0;
                Head.StringOffset = IO.RI32E(true);
                Head.StringLength = IO.RI32E(true);
                Head.Count = IO.RI32E(true);
                if (IO.RI32() != 0x4C42) return 0;
                Head.BinaryOffset = IO.RI32E(true);
                Head.BinaryLength = IO.RI32E(true);

                IO.P = Head.StringOffset;
            }
            else return 0;

            if (Header.Format == Format.DT)
                Head.StringLength = IO.L - 0x10;

            string[] STRData = IO.RS(Head.StringLength).Replace("\r", "").Split('\n');
            for (i = 0; i < STRData.Length; i++)
            {
                dataArray = STRData[i].Split('=');
                if (dataArray.Length == 2)
                    Dict.GetDictionary(dataArray[0], dataArray[1]);
            }
            STRData = null;

            A3DAReader();

            if (Header.SectionSignature == 0x5F5F5F43)
            {
                IO.P = IO.O + Head.BinaryOffset;
                IO.O = IO.P;
                IO.P = 0;
                byte[] data = IO.RBy(Head.BinaryLength);
                IO.C();
                IO = File.OpenReader(data);
                A3DCReader();
            }

            name = "";
            nameView = "";
            dataArray = null;
            Dict = null;
            return 1;
        }

        private void A3DAReader()
        {
            if (Dict.StartsWith("_"))
            {
                Data._ = new _();
                Dict.FindValue(out Data._.CompressF16     , "_.compress_f16"     );
                Dict.FindValue(out Data._.ConverterVersion, "_.converter.version");
                Dict.FindValue(out Data._.FileName        , "_.file_name"        );
                Dict.FindValue(out Data._.PropertyVersion , "_.property.version" );
            }

            if (Dict.StartsWith("camera_auxiliary"))
            {
                name = "camera_auxiliary" + d;

                Data.CameraAuxiliary = new CameraAuxiliary
                {
                    AutoExposure = Dict.RK(name + "auto_exposure" + d),
                        Exposure = Dict.RK(name +      "exposure" + d),
                    Gamma        = Dict.RK(name + "gamma"         + d),
                    GammaRate    = Dict.RK(name + "gamma_rate"    + d),
                    Saturate     = Dict.RK(name + "saturate"      + d)
                };
                if (Data.CameraAuxiliary.GammaRate.Type > 0)
                    Head.Format = Format.F;
            }

            if (Dict.StartsWith("play_control"))
            {
                name = "play_control" + d;

                Data.PlayControl = new PlayControl();
                Dict.FindValue(out Data.PlayControl.Begin , name + "begin" );
                Dict.FindValue(out Data.PlayControl.Div   , name + "div"   );
                Dict.FindValue(out Data.PlayControl.FPS   , name + "fps"   );
                Dict.FindValue(out Data.PlayControl.Offset, name + "offset");
                Dict.FindValue(out Data.PlayControl.Size  , name + "size"  );
            }

            if (Dict.StartsWith("post_process"))
            {
                name = "post_process" + d;

                Data.PostProcess = new PostProcess()
                {
                    Ambient   = Dict.RRGBAK(name + "Ambient"    + d),
                    Diffuse   = Dict.RRGBAK(name + "Diffuse"    + d),
                    Specular  = Dict.RRGBAK(name + "Specular"   + d),
                    LensFlare = Dict.RK    (name + "lens_flare" + d),
                    LensGhost = Dict.RK    (name + "lens_ghost" + d),
                    LensShaft = Dict.RK    (name + "lens_shaft" + d),
                };
            }

            if (Dict.FindValue(out value, "dof.name"))
            {
                Data.DOF = new DOF { Name = value };
                Head.Format = Format.FT;
                Data.DOF.MT = Dict.RMT("dof" + d);
            }

            if (Dict.FindValue(out value, "ambient.length"))
            {
                Data.Ambient = new Ambient[int.Parse(value)];
                Head.Format = Format.MGF;
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    name = "ambient" + d + i0 + d;
                    Dict.FindValue(out Data.Ambient[i0].Name, name + "name");
                    Data.Ambient[i0].   LightDiffuse = Dict.RRGBAK(name +    "light.Diffuse" + d);
                    Data.Ambient[i0].RimLightDiffuse = Dict.RRGBAK(name + "rimlight.Diffuse" + d);
                }
            }

            if (Dict.FindValue(out value, "camera_root.length"))
            {
                Data.CameraRoot = new CameraRoot[int.Parse(value)];
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    name = "camera_root" + d + i0 + d;
                    nameView = name + "view_point" + d;

                    Dict.FindValue(out Data.CameraRoot[i0].VP.
                        Aspect         , nameView + "aspect"           );
                    Dict.FindValue(out Data.CameraRoot[i0].VP.
                        CameraApertureH, nameView + "camera_aperture_h");
                    Dict.FindValue(out Data.CameraRoot[i0].VP.
                        CameraApertureW, nameView + "camera_aperture_w");
                    Dict.FindValue(out i1, nameView + "fov_is_horizontal");
                    Data.CameraRoot[i0].VP.FOVHorizontal = i1 != 0;

                    Data.CameraRoot[i0].      MT = Dict.RMT(name);
                    Data.CameraRoot[i0].Interest = Dict.RMT(name + "interest" + d);
                    Data.CameraRoot[i0].VP.   MT = Dict.RMT(nameView);
                    Data.CameraRoot[i0].VP.FocalLength = Dict.RK(nameView + "focal_length" + d);
                    Data.CameraRoot[i0].VP.FOV         = Dict.RK(nameView +          "fov" + d);
                    Data.CameraRoot[i0].VP.Roll        = Dict.RK(nameView +         "roll" + d);
                }
            }

            if (Dict.FindValue(out value, "chara.length"))
            {
                Data.Chara = new ModelTransform[int.Parse(value)];
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    Data.Chara[i0] = Dict.RMT("chara" + d + i0 + d);
            }

            if (Dict.FindValue(out value, "curve.length"))
            {
                Data.Curve = new Curve[int.Parse(value)];
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    name = "curve" + d + i0 + d;

                    Dict.FindValue(out Data.Curve[i0].Name, name + "name");
                    Data.Curve[i0].CV = Dict.RK(name + "cv" + d);
                }
            }

            if (Dict.FindValue(out value, "event.length"))
            {
                Data.Event = new Event[int.Parse(value)];
                for (i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    name = "event" + d + i0 + d;

                    Dict.FindValue(out Data.Event[i0].Begin       , name + "begin"         );
                    Dict.FindValue(out Data.Event[i0].ClipBegin   , name + "clip_begin"    );
                    Dict.FindValue(out Data.Event[i0].ClipEnd     , name + "clip_en"       );
                    Dict.FindValue(out Data.Event[i0].End         , name + "end"           );
                    Dict.FindValue(out Data.Event[i0].Name        , name + "name"          );
                    Dict.FindValue(out Data.Event[i0].Param1      , name + "param1"        );
                    Dict.FindValue(out Data.Event[i0].Ref         , name + "ref"           );
                    Dict.FindValue(out Data.Event[i0].TimeRefScale, name + "time_ref_scale");
                    Dict.FindValue(out Data.Event[i0].Type        , name + "type"          );
                }
            }

            if (Dict.FindValue(out value, "fog.length"))
            {
                Data.Fog = new Fog[int.Parse(value)];
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    name = "fog" + d + i0 + d;

                    Dict.FindValue(out Data.Fog[i0].Id, name + "id");
                    Data.Fog[i0].Density = Dict.RK    (name + "density" + d);
                    Data.Fog[i0].Diffuse = Dict.RRGBAK(name + "Diffuse" + d);
                    Data.Fog[i0].End     = Dict.RK    (name +     "end" + d);
                    Data.Fog[i0].Start   = Dict.RK    (name +   "start" + d);
                }
            }

            if (Dict.FindValue(out value, "light.length"))
            {
                Data.Light = new Light[int.Parse(value)];
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    name = "light" + d + i0 + d;

                    Dict.FindValue(out Data.Light[i0].Id  , name + "id"  );
                    Dict.FindValue(out Data.Light[i0].Name, name + "name");
                    Dict.FindValue(out Data.Light[i0].Type, name + "type");

                    Data.Light[i0].Ambient       = Dict.RRGBAK(name +        "Ambient" + d);
                    Data.Light[i0].Diffuse       = Dict.RRGBAK(name +        "Diffuse" + d);
                    Data.Light[i0].Incandescence = Dict.RRGBAK(name +  "Incandescence" + d);
                    Data.Light[i0].Specular      = Dict.RRGBAK(name +       "Specular" + d);
                    Data.Light[i0].Position      = Dict.RMT     (name +       "position" + d);
                    Data.Light[i0].SpotDirection = Dict.RMT     (name + "spot_direction" + d);
                }
            }

            if (Dict.FindValue(out value, "m_objhrc.length"))
            {
                Data.MObjectHRC = new MObjectHRC[int.Parse(value)];
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    name = "m_objhrc" + d + i0 + d;

                    Dict.FindValue(out Data.MObjectHRC[i0].Name, name + "name");
                    
                    if (Dict.StartsWith(name + "joint_orient"))
                    {
                        Data.MObjectHRC[i0].JointOrient = new Vector3<float?>();
                        Dict.FindValue(out Data.MObjectHRC[i0].JointOrient.X, name + "joint_orient.x");
                        Dict.FindValue(out Data.MObjectHRC[i0].JointOrient.Y, name + "joint_orient.y");
                        Dict.FindValue(out Data.MObjectHRC[i0].JointOrient.Z, name + "joint_orient.z");
                    }

                    if (Dict.FindValue(out value, name + "instance.length"))
                    {
                        Data.MObjectHRC[i0].Instances = new MObjectHRC.Instance[int.Parse(value)];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                        {
                            nameView = name + "instance" + d + i1 + d;

                            Dict.FindValue(out Data.MObjectHRC[i0].Instances[i1].   Name, nameView +     "name");
                            Dict.FindValue(out Data.MObjectHRC[i0].Instances[i1]. Shadow, nameView +   "shadow");
                            Dict.FindValue(out Data.MObjectHRC[i0].Instances[i1].UIDName, nameView + "uid_name");

                            Data.MObjectHRC[i0].Instances[i1].MT = Dict.RMT(nameView);
                        }
                    }

                    if (Dict.FindValue(out value, name + "node.length"))
                    {
                        Data.MObjectHRC[i0].Node = new Node[int.Parse(value)];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            nameView = name + "node" + d + i1 + d;
                            Dict.FindValue(out Data.MObjectHRC[i0].Node[i1].Name  , nameView + "name"  );
                            Dict.FindValue(out Data.MObjectHRC[i0].Node[i1].Parent, nameView + "parent");

                            Data.MObjectHRC[i0].Node[i1].MT = Dict.RMT(nameView);
                        }
                    }
                    
                    Data.MObjectHRC[i0].MT = Dict.RMT(name);
                }
            }

            if (Dict.FindValue(out value, "m_objhrc_list.length"))
            {
                Data.MObjectHRCList = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                    Dict.FindValue(out Data.MObjectHRCList[i0], "m_objhrc_list" + d + i0);
            }

            if (Dict.FindValue(out value, "material_list.length"))
            {
                Data.MaterialList = new MaterialList[int.Parse(value)];
                Head.Format = Format.X;
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    name = "material_list" + d + i0 + d;
                    Dict.FindValue(out Data.MaterialList[i0].HashName, name + "hash_name");
                    Dict.FindValue(out Data.MaterialList[i0].    Name, name +      "name");

                    Data.MaterialList[i0].BlendColor    = Dict.RRGBAK(name +    "blend_color" + d);
                    Data.MaterialList[i0].GlowIntensity = Dict.RK    (name + "glow_intensity" + d);
                    Data.MaterialList[i0].Incandescence = Dict.RRGBAK(name +  "incandescence" + d);
                }
            }

            if (Dict.FindValue(out value, "motion.length"))
            {
                Data.Motion = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.Motion.Length; i0++)
                    Dict.FindValue(out Data.Motion[i0], "motion" + d + i0 + d + "name");
            }

            if (Dict.FindValue(out value, "object.length"))
            {
                Data.Object = new Object[int.Parse(value)];
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    name = "object" + d + i0 + d;

                    Dict.FindValue(out Data.Object[i0].Morph      , name + "morph"       );
                    Dict.FindValue(out Data.Object[i0].MorphOffset, name + "morph_offset");
                    Dict.FindValue(out Data.Object[i0].       Name, name +         "name");
                    Dict.FindValue(out Data.Object[i0]. ParentName, name +  "parent_name");
                    Dict.FindValue(out Data.Object[i0].    UIDName, name +     "uid_name");

                    if (Dict.FindValue(out value, name + "tex_pat.length"))
                    {
                        Data.Object[i0].TexPat = new Object.TexturePattern[int.Parse(value)];
                        for (i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                        {
                            nameView = name + "tex_pat" + d + i1 + d;
                            Dict.FindValue(out Data.Object[i0].TexPat[i1].Name     , nameView + "name"      );
                            Dict.FindValue(out Data.Object[i0].TexPat[i1].Pat      , nameView + "pat"       );
                            Dict.FindValue(out Data.Object[i0].TexPat[i1].PatOffset, nameView + "pat_offset");
                        }
                    }

                    if (Dict.FindValue(out value, name + "tex_transform.length"))
                    {
                        Data.Object[i0].TexTrans = new Object.TextureTransform[int.Parse(value)];
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                        {
                            nameView = name + "tex_transform" + d + i1 + d;

                            Dict.FindValue(out Data.Object[i0].TexTrans[i1].Name, nameView + "name");
                            Data.Object[i0].TexTrans[i1].Coverage       =
                                Dict.RKUV(nameView + "coverage"      );
                            Data.Object[i0].TexTrans[i1].Offset         =
                                Dict.RKUV(nameView + "offset"        );
                            Data.Object[i0].TexTrans[i1].Repeat         =
                                Dict.RKUV(nameView + "repeat"        );
                            Data.Object[i0].TexTrans[i1].   Rotate      =
                                Dict.RK  (nameView + "rotate"     + d);
                            Data.Object[i0].TexTrans[i1].   RotateFrame =
                                Dict.RK  (nameView + "rotateFrame"+ d);
                            Data.Object[i0].TexTrans[i1].TranslateFrame =
                                Dict.RKUV(nameView + "translateFrame");
                        }
                    }
                    
                    Data.Object[i0].MT = Dict.RMT(name);
                }
            }

            if (Dict.FindValue(out value, "objhrc.length"))
            {
                Data.ObjectHRC = new ObjectHRC[int.Parse(value)];
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    name = "objhrc" + d + i0 + d;

                    if (Dict.StartsWith(name + "joint_orient"))
                    {
                        Data.ObjectHRC[i0].JointOrient = new Vector3<float?>();
                        Dict.FindValue(out Data.ObjectHRC[i0].JointOrient.X, name + "joint_orient.x");
                        Dict.FindValue(out Data.ObjectHRC[i0].JointOrient.Y, name + "joint_orient.y");
                        Dict.FindValue(out Data.ObjectHRC[i0].JointOrient.Z, name + "joint_orient.z");
                    }
                    Dict.FindValue(out Data.ObjectHRC[i0].   Name, name +     "name");
                    Dict.FindValue(out Data.ObjectHRC[i0]. Shadow, name +   "shadow");
                    Dict.FindValue(out Data.ObjectHRC[i0].UIDName, name + "uid_name");
                    if (Dict.FindValue(out value, name + "node.length"))
                    {
                        Data.ObjectHRC[i0].Node = new Node[int.Parse(value)];
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        {
                            nameView = name + "node" + d + i1 + d;

                            Dict.FindValue(out Data.ObjectHRC[i0].Node[i1].Name  , nameView + "name"  );
                            Dict.FindValue(out Data.ObjectHRC[i0].Node[i1].Parent, nameView + "parent");

                            Data.ObjectHRC[i0].Node[i1].MT = Dict.RMT(nameView);
                        }
                    }
                }
            }

            if (Dict.FindValue(out value, "object_list.length"))
            {
                Data.ObjectList = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.ObjectList.Length; i0++)
                    Dict.FindValue(out Data.ObjectList[i0], "object_list" + d + i0);
            }

            if (Dict.FindValue(out value, "objhrc_list.length"))
            {
                Data.ObjectHRCList = new string[int.Parse(value)];
                for (i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
                    Dict.FindValue(out Data.ObjectHRCList[i0], "objhrc_list" + d + i0);
            }

            if (Dict.FindValue(out value, "point.length"))
            {
                Data.Point = new ModelTransform[int.Parse(value)];
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    Data.Point[i0] = Dict.RMT("point" + d + i0 + d);
            }
        }

        public byte[] A3DAWriter(bool A3DC = false)
        {
            IO = File.OpenWriter();
            DateTime date = DateTime.Now;
            if (A3DC && Data._.CompressF16 != null)
                if (Data._.CompressF16 != 0)
                    IO.W("", "#-compress_f16");
            if (!A3DC)
                IO.W("#A3DA__________\n");
            IO.W("#", DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss yyyy",
                System.Globalization.CultureInfo.InvariantCulture));
            if (A3DC && Data._.CompressF16 != 0)
                IO.W("_.compress_f16=", Data._.CompressF16);

            IO.W("_.converter.version=", Data._.ConverterVersion);
            IO.W("_.file_name="        , Data._.FileName        );
            IO.W("_.property.version=" , Data._. PropertyVersion);

            if (Data.Ambient != null && Head.Format == Format.MGF)
            {
                SO0 = Data.Ambient.Length.SortWriter();
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "ambient" + d + SOi0 + d;
                    ref Ambient Ambient = ref Data.Ambient[SOi0];

                    IO.W(Ambient.   LightDiffuse, name +    "light.Diffuse", A3DC);
                    IO.W(name + "name=", Ambient.Name);
                    IO.W(Ambient.RimLightDiffuse, name + "rimlight.Diffuse", A3DC);
                }
                IO.W("ambient.length=", Data.Fog.Length);
            }
            
            name = "camera_auxiliary" + d;
            ref CameraAuxiliary CA = ref Data.CameraAuxiliary;
            
            IO.W(CA.AutoExposure, name + "auto_exposure", true, A3DC);
            IO.W(CA.    Exposure, name +      "exposure", true, A3DC);
            IO.W(CA.Gamma       , name + "gamma"        , true, A3DC);
            if (Head.Format >= Format.F && Head.Format == Format.FT)
                IO.W(CA.GammaRate   , name + "gamma_rate"   , true, A3DC);
            IO.W(CA.Saturate    , name + "saturate"     , true, A3DC);

            if (Data.CameraRoot != null)
            {
                SO0 = Data.CameraRoot.Length.SortWriter();
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "camera_root" + d + SOi0 + d;
                    nameView = name + "view_point" + d;
                    ref CameraRoot CR = ref Data.CameraRoot[SOi0];

                    IO.W(CR.Interest, name + "interest" + d, A3DC, IsX);
                    IO.W(CR.MT, name, A3DC, IsX, 0b11110);
                    IO.W(nameView + "aspect=", CR.VP.Aspect);
                    if (CR.VP.CameraApertureH != null)
                        IO.W(nameView + "camera_aperture_h=", CR.VP.CameraApertureH);
                    if (CR.VP.CameraApertureW != null)
                        IO.W(nameView + "camera_aperture_w=", CR.VP.CameraApertureW);
                    IO.W(CR.VP.FocalLength, nameView + "focal_length" + d, A3DC);
                    IO.W(CR.VP.FOV, nameView + "fov" + d, A3DC);
                    if (CR.VP.FOVHorizontal != null)
                        IO.W(nameView + "fov_is_horizontal=", CR.VP.FOVHorizontal.Value ? 1 : 0);
                    IO.W(CR.VP.MT  , nameView, A3DC, IsX, 0b10000);
                    IO.W(CR.VP.Roll, nameView + "roll" + d, A3DC);
                    IO.W(CR.VP.MT  , nameView, A3DC, IsX, 0b01111);
                    IO.W(CR   .MT  , name    , A3DC, IsX, 0b00001);
                }
                IO.W("camera_root.length=", Data.CameraRoot.Length);
            }

            if (Data.Chara != null)
            {
                SO0 = Data.Chara.Length.SortWriter();
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    IO.W(Data.Chara[SO0[i0]], "chara" + d + SO0[i0] + d, A3DC, IsX);
                IO.W("chara.length=", Data.Chara.Length);
            }

            if (Data.Curve != null)
            {
                SO0 = Data.Curve.Length.SortWriter();
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "curve" + d + SOi0 + d;
                    ref Curve Curve = ref Data.Curve[SOi0];

                    IO.W(Curve.CV, name + "cv" + d, A3DC);
                    IO.W(name + "name=", Curve.Name);
                }
                IO.W("curve.length=", Data.Curve.Length);
            }

            if (Data.DOF != null && Head.Format == Format.FT)
            {
                IO.W("dof.name=", Data.DOF.Name);
                IO.W(Data.DOF.MT, "dof" + d, A3DC, IsX);
            }

            if (Data.Event != null)
            {
                SO0 = Data.Event.Length.SortWriter();
                for (i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "event" + d + SOi0 + d;
                    ref Event Event = ref Data.Event[SOi0];

                    IO.W(name + "begin="         , Event.Begin       );
                    IO.W(name + "clip_begin="    , Event.ClipBegin   );
                    IO.W(name + "clip_en="       , Event.ClipEnd     );
                    IO.W(name + "end="           , Event.End         );
                    IO.W(name + "name="          , Event.Name        );
                    IO.W(name + "param1="        , Event.Param1      );
                    IO.W(name + "ref="           , Event.Ref         );
                    IO.W(name + "time_ref_scale=", Event.TimeRefScale);
                    IO.W(name + "type="          , Event.Type        );
                }
            }

            if (Data.Fog != null)
            {
                SO0 = Data.Fog.Length.SortWriter();
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "fog" + d + SOi0 + d;
                    ref Fog Fog = ref Data.Fog[SOi0];

                    IO.W(Fog.Diffuse, name + "Diffuse",       A3DC);
                    IO.W(Fog.Density, name + "density", true, A3DC);
                    IO.W(Fog.End    , name + "end"    , true, A3DC);
                    IO.W(name + "id=", Fog.Id);
                    IO.W(Fog.Start  , name + "start"  , true, A3DC);
                }
                IO.W("fog.length=", Data.Fog.Length);
            }

            if (Data.Light != null)
            {
                SO0 = Data.Light.Length.SortWriter();
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "light" + d + SOi0 + d;
                    ref Light Light = ref Data.Light[SOi0];

                    IO.W(Light.Ambient      , name + "Ambient"      , A3DC);
                    IO.W(Light.Diffuse      , name + "Diffuse"      , A3DC);
                    IO.W(Light.Incandescence, name + "Incandescence", A3DC);
                    IO.W(Light.Specular     , name + "Specular"     , A3DC);
                    IO.W(name + "id="  , Light.Id  );
                    IO.W(name + "name=", Light.Name);
                    IO.W(Light.Position     , name + "position"       + d, A3DC, IsX);
                    IO.W(Light.SpotDirection, name + "spot_direction" + d, A3DC, IsX);
                    IO.W(name + "type=", Light.Type);
                }
                IO.W("light.length=", Data.Light.Length);
            }

            if (Data.MObjectHRC != null)
            {
                SO0 = Data.MObjectHRC.Length.SortWriter();
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "m_objhrc" + d + SOi0 + d;
                    ref MObjectHRC MObjectHRC = ref Data.MObjectHRC[SOi0];

                    if (IsX && MObjectHRC.JointOrient.NotNull)
                    {
                        IO.W(name + "joint_orient.x=", MObjectHRC.JointOrient.X);
                        IO.W(name + "joint_orient.y=", MObjectHRC.JointOrient.Y);
                        IO.W(name + "joint_orient.z=", MObjectHRC.JointOrient.Z);
                    }

                    if (MObjectHRC.Instances != null)
                    {
                        SO1 = MObjectHRC.Instances.Length.SortWriter();
                        for (i1 = 0; i1 < MObjectHRC.Instances.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            nameView = name + "instance" + d + SOi1 + d;
                            ref MObjectHRC.Instance Instance = ref MObjectHRC.Instances[SOi1];

                            IO.W(Instance.MT, nameView, A3DC, IsX, 0b10000);
                            IO.W(nameView +     "name=", Instance.   Name);
                            IO.W(Instance.MT, nameView, A3DC, IsX, 0b01100);
                            IO.W(nameView +   "shadow=", Instance. Shadow);
                            IO.W(Instance.MT, nameView, A3DC, IsX, 0b00010);
                            IO.W(nameView + "uid_name=", Instance.UIDName);
                            IO.W(Instance.MT, nameView, A3DC, IsX, 0b00001);
                        }
                        IO.W(name + "instance.length=", MObjectHRC.Instances.Length);
                    }

                    IO.W(MObjectHRC.MT, name, A3DC, IsX, 0b10000);
                    IO.W(name + "name=", MObjectHRC.Name);

                    if (MObjectHRC.Node != null)
                    {
                        SO1 = MObjectHRC.Node.Length.SortWriter();
                        for (i1 = 0; i1 < MObjectHRC.Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            nameView = name + "node" + d + SOi1 + d;
                            ref Node Node = ref MObjectHRC.Node[SOi1];

                            IO.W(Node.MT, nameView, A3DC, IsX, 0b10000);
                            IO.W(nameView +   "name=", Node.Name  );
                            IO.W(nameView + "parent=", Node.Parent);
                            IO.W(Node.MT, nameView, A3DC, IsX, 0b01111);
                        }
                        IO.W(name + "node.length=", MObjectHRC.Node.Length);
                    }

                    IO.W(MObjectHRC.MT, name, A3DC, IsX, 0b01111);
                }
                IO.W("m_objhrc.length=", Data.MObjectHRC.Length);
            }

            if (Data.MObjectHRCList != null)
            {
                SO0 = Data.MObjectHRCList.Length.SortWriter();
                for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                    IO.W("m_objhrc_list" + d + SO0[i0] + "=", Data.MObjectHRCList[SO0[i0]]);
                IO.W("m_objhrc_list.length=", Data.MObjectHRCList.Length);
            }

            if (Data.MaterialList != null && IsX)
            {
                SO0 = Data.MaterialList.Length.SortWriter();
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "material_list" + d + SOi0 + d;
                    ref MaterialList ML = ref Data.MaterialList[SOi0];

                    IO.W(ML.BlendColor   , name + "blend_color"    + d, A3DC);
                    IO.W(ML.GlowIntensity, name + "glow_intensity" + d, A3DC);
                    IO.W(name + "hash_name=", ML.HashName);
                    IO.W(ML.Incandescence, name + "incandescence"  + d, A3DC);
                    IO.W(name +      "name=", ML.    Name);
                }
                IO.W("material_list.length=", Data.MaterialList.Length);
            }

            if (Data.Motion != null)
            {
                SO0 = Data.Motion.Length.SortWriter();
                for (i0 = 0; i0 < Data.Motion.Length; i0++)
                    IO.W(name + SO0[i0] + d + "name=", Data.Motion[SO0[i0]]);
                IO.W("motion.length=", Data.Motion.Length);
            }

            if (Data.Object != null)
            {
                SO0 = Data.Object.Length.SortWriter();
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "object" + d + SOi0 + d;
                    ref Object Object = ref Data.Object[SOi0];

                    IO.W(Object.MT, name, A3DC, IsX, 0b10000);
                    if (Object.Morph != null)
                    {
                        IO.W(name + "morph="       , Object.Morph      );
                        IO.W(name + "morph_offset=", Object.MorphOffset);
                    }
                    IO.W(name + "name="       , Object.Name      );
                    IO.W(name + "parent_name=", Object.ParentName);
                    IO.W(Object.MT, name, A3DC, IsX, 0b01100);

                    if (Object.TexPat != null)
                    {
                        SO1 = Object.TexPat.Length.SortWriter();
                        for (i1 = 0; i1 < Object.TexPat.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            nameView = name + "tex_pat" + d + SOi1 + d;
                            ref Object.TexturePattern TexPat = ref Object.TexPat[SOi1];

                            IO.W(nameView + "name="      , TexPat.Name     );
                            IO.W(nameView + "pat="       , TexPat.Pat      );
                            IO.W(nameView + "pat_offset=", TexPat.PatOffset);
                        }
                        IO.W(nameView + "length=", Object.TexPat.Length);
                    }

                    if (Object.TexTrans != null)
                    {
                        SO1 = Object.TexTrans.Length.SortWriter();
                        for (i1 = 0; i1 < Object.TexTrans.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            nameView = name + "tex_transform" + d + SOi1 + d;
                            ref Object.TextureTransform TexTrans = ref Object.TexTrans[SOi1];

                            IO.W(nameView + "name=", Object.TexTrans[SOi1].Name);
                            IO.W(TexTrans.Coverage      , nameView + "coverage"      , A3DC);
                            IO.W(TexTrans.Offset        , nameView + "offset"        , A3DC);
                            IO.W(TexTrans.Repeat        , nameView + "repeat"        , A3DC);
                            IO.W(TexTrans.   Rotate     , nameView + "rotate"        , A3DC);
                            IO.W(TexTrans.   RotateFrame, nameView + "rotateFrame"   , A3DC);
                            IO.W(TexTrans.TranslateFrame, nameView + "translateFrame", A3DC);
                        }
                        IO.W(name + "tex_transform.length=", + Object.TexTrans.Length);
                    }

                    IO.W(Object.MT, name, A3DC, IsX, 0b00010);
                    IO.W(name + "uid_name=", Object.UIDName);
                    IO.W(Object.MT, name, A3DC, IsX, 0b00001);
                }
                IO.W("object.length=", Data.Object.Length);
            }

            if (Data.ObjectList != null)
            {
                SO0 = Data.ObjectList.Length.SortWriter();
                for (i0 = 0; i0 < Data.ObjectList.Length; i0++)
                    IO.W("object_list" + d + SO0[i0] + "=", Data.ObjectList[SO0[i0]]);
                IO.W("object_list.length=", Data.ObjectList.Length);
            }

            if (Data.ObjectHRC != null)
            {
                SO0 = Data.ObjectHRC.Length.SortWriter();
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "objhrc" + d + SOi0 + d;
                    ref ObjectHRC ObjectHRC = ref Data.ObjectHRC[SOi0];

                    IO.W(name + "name=", ObjectHRC.Name);

                    if (IsX && ObjectHRC.JointOrient.NotNull)
                    {
                        IO.W(name + "joint_orient.x=", ObjectHRC.JointOrient.X);
                        IO.W(name + "joint_orient.y=", ObjectHRC.JointOrient.Y);
                        IO.W(name + "joint_orient.z=", ObjectHRC.JointOrient.Z);
                    }

                    if (ObjectHRC.Node != null)
                    {
                        SO1 = ObjectHRC.Node.Length.SortWriter();
                        for (i1 = 0; i1 < ObjectHRC.Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            nameView = name + "node" + d + SOi1 + d;
                            ref Node Node = ref ObjectHRC.Node[SOi1];

                            IO.W(Node.MT, nameView, A3DC, IsX, 0b10000);
                            IO.W(nameView + "name="  , Node.Name  );
                            IO.W(nameView + "parent=", Node.Parent);
                            IO.W(Node.MT, nameView, A3DC, IsX, 0b01111);
                        }
                        IO.W(name + "node.length=", ObjectHRC.Node.Length);
                    }

                    if (ObjectHRC.Shadow != null)
                        IO.W(name + "shadow=", ObjectHRC.Shadow);
                    IO.W(name + "uid_name=", ObjectHRC.UIDName);
                }
                IO.W("objhrc.length=", Data.ObjectHRC.Length);
            }

            if (Data.ObjectHRCList != null)
            {
                SO0 = Data.ObjectHRCList.Length.SortWriter();
                for (i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
                    IO.W("objhrc_list" + d + SO0[i0] + "=", Data.ObjectHRCList[SO0[i0]]);
                IO.W("objhrc_list.length=", Data.ObjectHRCList.Length);
            }

            IO.W("play_control.begin=", Data.PlayControl.Begin);
            if (Data.PlayControl.Div    != null && A3DC)
                IO.W("play_control.div=", Data.PlayControl.Div);
            IO.W("play_control.fps=", Data.PlayControl.FPS);
            if (Data.PlayControl.Offset != null)
            { if (A3DC) { IO.W("play_control.offset=", Data.PlayControl.Offset);
                          IO.W("play_control.size="  , Data.PlayControl.Size  ); }
              else IO.W("play_control.size=", Data.PlayControl.Size + Data.PlayControl.Offset);
            }
            else   IO.W("play_control.size=", Data.PlayControl.Size);

            if (Data.PostProcess != null)
            {
                ref PostProcess PP = ref Data.PostProcess;
                name = "post_process" + d;
                IO.W(PP.Ambient  , name + "Ambient"   ,       A3DC);
                IO.W(PP.Diffuse  , name + "Diffuse"   ,       A3DC);
                IO.W(PP.Specular , name + "Specular"  ,       A3DC);
                IO.W(PP.LensFlare, name + "lens_flare", true, A3DC);
                IO.W(PP.LensGhost, name + "lens_ghost", true, A3DC);
                IO.W(PP.LensShaft, name + "lens_shaft", true, A3DC);
            }

            if (Data.Point != null)
            {
                SO0 = Data.Point.Length.SortWriter();
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    IO.W(Data.Point[SO0[i0]], "point" + d + SO0[i0] + d, A3DC, IsX);
                IO.W("point.length=", Data.Point.Length);
            }

            IO.A(0x1, true);
            byte[] data = IO.ToArray();
            IO.Dispose();
            return data;
        }

        private int CompressF16 => Data._.CompressF16 ?? 0;

        private void A3DCReader()
        {
            if (Data.Ambient != null)
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    IO.RRGBAK(ref Data.Ambient[i0].   LightDiffuse, CompressF16);
                    IO.RRGBAK(ref Data.Ambient[i0].RimLightDiffuse, CompressF16);
                }

            
            IO.RK(ref Data.CameraAuxiliary.AutoExposure, CompressF16);
            IO.RK(ref Data.CameraAuxiliary.    Exposure, CompressF16);
            IO.RK(ref Data.CameraAuxiliary.Gamma       , CompressF16);
            IO.RK(ref Data.CameraAuxiliary.GammaRate   , CompressF16);
            IO.RK(ref Data.CameraAuxiliary.Saturate    , CompressF16);

            if (Data.CameraRoot != null)
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    IO.RMT(ref Data.CameraRoot[i0].      MT, CompressF16);
                    IO.RMT(ref Data.CameraRoot[i0].Interest, CompressF16);
                    IO.RMT(ref Data.CameraRoot[i0].VP.   MT, CompressF16);
                    IO.RK (ref Data.CameraRoot[i0].VP.FocalLength, CompressF16);
                    IO.RK (ref Data.CameraRoot[i0].VP.FOV        , CompressF16);
                    IO.RK (ref Data.CameraRoot[i0].VP.Roll       , CompressF16);
                }

            if (Data.Chara != null)
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    IO.RMT(ref Data.Chara[i0], CompressF16);

            if (Data.Curve != null)
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                    IO.RK(ref Data.Curve[i0].CV, CompressF16);

            if (Data.DOF != null)
                    IO.RMT(ref Data.DOF.MT, CompressF16);

            if (Data.Fog != null)
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    IO.RK    (ref Data.Fog[i0].Density, CompressF16);
                    IO.RRGBAK(ref Data.Fog[i0].Diffuse, CompressF16);
                    IO.RK    (ref Data.Fog[i0].End    , CompressF16);
                    IO.RK    (ref Data.Fog[i0].Start  , CompressF16);
                }

            if (Data.Light != null)
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    IO.RRGBAK(ref Data.Light[i0].Ambient      , CompressF16);
                    IO.RRGBAK(ref Data.Light[i0].Diffuse      , CompressF16);
                    IO.RRGBAK(ref Data.Light[i0].Incandescence, CompressF16);
                    IO.RMT   (ref Data.Light[i0].Position     , CompressF16);
                    IO.RRGBAK(ref Data.Light[i0].Specular     , CompressF16);
                    IO.RMT   (ref Data.Light[i0].SpotDirection, CompressF16);
                }

            if (Data.MObjectHRC != null)
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    IO.RMT(ref Data.MObjectHRC[i0].MT, CompressF16);

                    if (Data.MObjectHRC[i0].Instances != null)
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                            IO.RMT(ref Data.MObjectHRC[i0].Instances[i1].MT, CompressF16);

                    if (Data.MObjectHRC[i0].Node != null)
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            IO.RMT(ref Data.MObjectHRC[i0].Node[i1].MT, CompressF16);
                }

            if (Data.MaterialList != null)
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    IO.RRGBAK(ref Data.MaterialList[i0].BlendColor   , CompressF16);
                    IO.RK    (ref Data.MaterialList[i0].GlowIntensity, CompressF16);
                    IO.RRGBAK(ref Data.MaterialList[i0].Incandescence, CompressF16);
                }

            if (Data.Object != null)
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    IO.RMT(ref Data.Object[i0].MT, CompressF16);
                    if (Data.Object[i0].TexTrans != null)
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                        {
                            IO.RKUV(ref Data.Object[i0].TexTrans[i1].Coverage      , CompressF16);
                            IO.RKUV(ref Data.Object[i0].TexTrans[i1].Offset        , CompressF16);
                            IO.RKUV(ref Data.Object[i0].TexTrans[i1].Repeat        , CompressF16);
                            IO.RK  (ref Data.Object[i0].TexTrans[i1].   Rotate     , CompressF16);
                            IO.RK  (ref Data.Object[i0].TexTrans[i1].   RotateFrame, CompressF16);
                            IO.RKUV(ref Data.Object[i0].TexTrans[i1].TranslateFrame, CompressF16);
                        }
                }

            if (Data.ObjectHRC != null)
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                    if (Data.ObjectHRC[i0].Node != null)
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            IO.RMT(ref Data.ObjectHRC[i0].Node[i1].MT, CompressF16);


            if (Data.Point != null)
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    IO.RMT(ref Data.Point[i0], CompressF16);

            if (Data.PostProcess != null)
            {
                IO.RRGBAK(ref Data.PostProcess.Ambient  , CompressF16);
                IO.RRGBAK(ref Data.PostProcess.Diffuse  , CompressF16);
                IO.RRGBAK(ref Data.PostProcess.Specular , CompressF16);
                IO.RK    (ref Data.PostProcess.LensFlare, CompressF16);
                IO.RK    (ref Data.PostProcess.LensGhost, CompressF16);
                IO.RK    (ref Data.PostProcess.LensShaft, CompressF16);
            }
        }

        public byte[] A3DCWriter()
        {
            if (A3DCOpt) UsedValues = new Dictionary<int?, float?>();
            if (Head.Format < Format.F2LE) Data._.CompressF16 = null;

            IO = File.OpenWriter();
            for (byte i = 0; i < 2; i++)
            {
                bool ReturnToOffset = i == 1;
                IO.P = 0;

                if (Data.CameraRoot != null)
                    for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                    {
                        IO.WO(ref Data.CameraRoot[i0].      MT, ReturnToOffset);
                        IO.WO(ref Data.CameraRoot[i0].VP.   MT, ReturnToOffset);
                        IO.WO(ref Data.CameraRoot[i0].Interest, ReturnToOffset);
                    }

                if (Data.DOF != null)
                    IO.WO(ref Data.DOF.MT, ReturnToOffset);

                if (Data.Light != null)
                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        IO.WO(ref Data.Light[i0].Position     , ReturnToOffset);
                        IO.WO(ref Data.Light[i0].SpotDirection, ReturnToOffset);
                    }

                if (Data.MObjectHRC != null)
                    for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        if (Data.MObjectHRC[i0].Instances != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                                IO.WO(ref Data.MObjectHRC[i0].Instances[i1].MT, ReturnToOffset);

                        IO.WO(ref Data.MObjectHRC[i0].MT, ReturnToOffset);

                        if (Data.MObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                IO.WO(ref Data.MObjectHRC[i0].Node[i1].MT, ReturnToOffset);
                    }

                if (Data.Object != null)
                    for (i0 = 0; i0 < Data.Object.Length; i0++)
                        IO.WO(ref Data.Object[i0].MT, ReturnToOffset);

                if (Data.ObjectHRC != null)
                    for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                        if (Data.ObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                                IO.WO(ref Data.ObjectHRC[i0].Node[i1].MT, ReturnToOffset);

                if (ReturnToOffset) continue;

                if (Data.Ambient != null)
                    for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                    {
                        W(ref Data.Ambient[i0].   LightDiffuse);
                        W(ref Data.Ambient[i0].RimLightDiffuse);
                    }

                
                W(ref Data.CameraAuxiliary.AutoExposure);
                W(ref Data.CameraAuxiliary.    Exposure);
                W(ref Data.CameraAuxiliary.Gamma       );
                W(ref Data.CameraAuxiliary.GammaRate   );
                W(ref Data.CameraAuxiliary.Saturate    );

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

                if (Data.DOF != null && Head.Format == Format.FT)
                    W(ref Data.DOF.MT);

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

                if (Data.MaterialList != null && IsX)
                    for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                    {
                        W(ref Data.MaterialList[SO0[i0]].BlendColor   );
                        W(ref Data.MaterialList[SO0[i0]].GlowIntensity);
                        W(ref Data.MaterialList[SO0[i0]].Incandescence);
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
                    W(ref Data.PostProcess.Ambient  );
                    W(ref Data.PostProcess.Diffuse  );
                    W(ref Data.PostProcess.Specular );
                    W(ref Data.PostProcess.LensFlare);
                    W(ref Data.PostProcess.LensShaft);
                    W(ref Data.PostProcess.LensGhost);
                }

                IO.A(0x10, true);
            }
            byte[] A3DCData = IO.ToArray(); IO.Dispose();
            byte[] A3DAData = A3DAWriter(true);

            IO = File.OpenWriter();
            IO.O = Head.Format > Format.FT ? 0x40 : 0;
            IO.P = 0x40;

            Head.StringOffset = IO.P;
            Head.StringLength = A3DAData.Length;
            IO.W(A3DAData);
            IO.A(0x20, true);

            Head.BinaryOffset = IO.P;
            Head.BinaryLength = A3DCData.Length;
            IO.W(A3DCData);
            IO.A(0x10, true);

            int A3DCEnd = IO.P;

            IO.P = 0;
            IO.W("#A3D", "C__________");
            IO.W(0x2000);
            IO.W(0x00);
            IO.WE(0x20, true);
            IO.W(0x10000200);
            IO.W(0x50);
            IO.WE(Head.StringOffset, true);
            IO.WE(Head.StringLength, true);
            IO.WE(0x01, true);
            IO.W(0x4C42);
            IO.WE(Head.BinaryOffset, true);
            IO.WE(Head.BinaryLength, true);
            IO.WE(0x20, true);
            
            if (Head.Format > Format.FT)
            {
                IO.P = A3DCEnd;
                IO.WEOFC(0);
                IO.O   = 0;
                IO.P = 0;
                Header Header = new Header { Signature = 0x41443341, Format = Format.F2LE,
                    DataSize = A3DCEnd, SectionSize = A3DCEnd, InnerSignature = 0x01131010 };
                IO.W(Header, true);
            }

            byte[] data = IO.ToArray();
            IO.Dispose();
            return data;
        }

        private void W(ref ModelTransform MT)
        { W(ref MT.Scale); W(ref MT.Rot, true); W(ref MT.Trans); W(ref MT.Visibility); }

        private void W(ref Vector4<Key> RGBA)
        { W(ref RGBA.X); W(ref RGBA.Y); W(ref RGBA.Z); W(ref RGBA.W); }

        private void W(ref Vector3<Key> Key, bool F16 = false)
        { W(ref Key.X, F16); W(ref Key.Y, F16); W(ref Key.Z, F16); }

        private void W(ref Vector2<Key> UV)
        { W(ref UV.X); W(ref UV.Y); }

        private void W(ref Key Key, bool F16 = false)
        {
            if (Key.Type == null) return;

            int i = 0;
            if (Key.Keys != null)
            {
                Key.BinOffset = IO.P;
                IO.W(((int)Key.Type & 0xFF) | (((int)Key.EPTypePost & 0xF) << 4) | ((int)Key.EPTypePre & 0xF) << 8);
                IO.W(0x00);
                IO.W((float)Key.Max);
                IO.W(Key.Keys.Length);

                if (F16 && CompressF16 == 2)
                    for (i = 0; i < Key.Keys.Length; i++)
                    { ref KFT3 KF = ref Key.Keys[i]; IO.W((ushort)KF.F ); IO.W((Half)KF.V );
                                                     IO.W((  Half)KF.T1); IO.W((Half)KF.T2); }
                else if (F16 && CompressF16 > 0)
                    for (i = 0; i < Key.Keys.Length; i++)
                    { ref KFT3 KF = ref Key.Keys[i]; IO.W((ushort)KF.F ); IO.W((Half)KF.V );
                                                     IO.W(        KF.T1); IO.W(      KF.T2); }
                else 
                    for (i = 0; i < Key.Keys.Length; i++)
                    { ref KFT3 KF = ref Key.Keys[i]; IO.W(        KF.F ); IO.W(      KF.V );
                                                     IO.W(        KF.T1); IO.W(      KF.T2); }
            }
            else
            {
                if (!UsedValues.ContainsValue(Key.Value) || !A3DCOpt)
                {
                    Key.BinOffset = IO.P;
                    IO.W((  int)Key.Type );
                    IO.W((float)Key.Value);
                    if (A3DCOpt) UsedValues.Add(Key.BinOffset, Key.Value);
                    return;
                }
                else if (UsedValues.ContainsValue(Key.Value))
                { Key.BinOffset = UsedValues.GetKey(Key.Value); return; }
            }
        }

        public void MsgPackReader(string file, bool JSON)
        {
            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);
            MsgPack A3D = MsgPack["A3D"];
            if (A3D.NotNull) MsgPackReader(A3D);
            MsgPack.Dispose();
        }

        public void MsgPackReader(MsgPack A3D)
        {
            MsgPack temp = MsgPack.New, Temp = MsgPack.New;
            if ((Temp = A3D["_"]).NotNull)
            {
                Data._ = new _
                {
                    CompressF16      = Temp.RnI32("CompressF16"     ),
                    ConverterVersion = Temp.RS   ("ConverterVersion"),
                    FileName         = Temp.RS   ("FileName"        ),
                    PropertyVersion  = Temp.RS   ("PropertyVersion" ),
                };
            }

            if ((Temp = A3D["Ambient", true]).NotNull)
            {
                Data.Ambient = new Ambient[Temp.Array.Length];

                for (i = 0; i < Data.Ambient.Length; i++)
                    Data.Ambient[i] = new Ambient
                    {
                                   Name = Temp[i].RS    (           "Name"),
                           LightDiffuse = Temp[i].RRGBAK(   "LightDiffuse"),
                        RimLightDiffuse = Temp[i].RRGBAK("RimLightDiffuse"),
                    };
            }

            if ((Temp = A3D["CameraAuxiliary"]).NotNull)
                Data.CameraAuxiliary = new CameraAuxiliary
                {
                    AutoExposure = Temp.RK("AutoExposure"),
                        Exposure = Temp.RK(    "Exposure"),
                    Gamma        = Temp.RK("Gamma"       ),
                    GammaRate    = Temp.RK("GammaRate"   ),
                    Saturate     = Temp.RK("Saturate"    ),
                };

            if ((Temp = A3D["CameraRoot", true]).NotNull)
            {
                MsgPack ViewPoint;
                Data.CameraRoot = new CameraRoot[Temp.Array.Length];
                for (i = 0; i < Data.CameraRoot.Length; i++)
                {
                    Data.CameraRoot[i] = new CameraRoot
                    {
                        MT       = Temp[i].RMT(),
                        Interest = Temp[i].RMT("Interest"),
                    };

                    if ((ViewPoint = Temp[i]["ViewPoint"]).IsNull) continue;
                    Data.CameraRoot[i].VP = new CameraRoot.ViewPoint
                    {
                        MT              = ViewPoint.RMT(),
                        Aspect          = ViewPoint.RnF32("Aspect"         ),
                        CameraApertureH = ViewPoint.RnF32("CameraApertureH"),
                        CameraApertureW = ViewPoint.RnF32("CameraApertureW"),
                        FOVHorizontal   = ViewPoint.RB   ("FOVHorizontal"  ),
                        FocalLength     = ViewPoint.RK   ("FocalLength"    ),
                        FOV             = ViewPoint.RK   ("FOV"            ),
                        Roll            = ViewPoint.RK   ("Roll"           ),
                    };
                }
            }

            if ((Temp = A3D["Chara", true]).NotNull)
            {
                Data.Chara = new ModelTransform[Temp.Array.Length];
                for (i = 0; i < Data.Chara.Length; i++)
                    Data.Chara[i] = Temp[i].RMT();
            }

            if ((Temp = A3D["Curve", true]).NotNull)
            {
                Data.Curve = new Curve[Temp.Array.Length];
                for (i = 0; i < Data.Curve.Length; i++)
                    Data.Curve[i] = new Curve
                    {
                        Name = Temp[i].RS("Name"),
                        CV   = Temp[i].RK("CV"  ),
                    };
            }

            if ((Temp = A3D["DOF"]).NotNull)
                Data.DOF = new DOF
                {
                    MT   = Temp.RMT(),
                    Name = Temp.RS("Name"),
                };

            if ((Temp = A3D["Event", true]).NotNull)
            {
                Data.Event = new Event[Temp.Array.Length];
                for (i = 0; i < Data.Event.Length; i++)
                    Data.Event[i] = new Event
                    {
                            Begin    = Temp[i].RnF32(    "Begin"   ),
                        ClipBegin    = Temp[i].RnF32("ClipBegin"   ),
                        ClipEnd      = Temp[i].RnF32("ClipEnd"     ),
                            End      = Temp[i].RnF32(    "End"     ),
                        Name         = Temp[i].RS   ("Name"        ),
                        Param1       = Temp[i].RS   ("Param1"      ),
                        Ref          = Temp[i].RS   ("Ref"         ),
                        TimeRefScale = Temp[i].RnF32("TimeRefScale"),
                        Type         = Temp[i].RnI32("Type"        ),
                    };
            }

            if ((Temp = A3D["Fog", true]).NotNull)
            {
                Data.Fog = new Fog[Temp.Array.Length];
                for (i = 0; i < Data.Fog.Length; i++)
                    Data.Fog[i] = new Fog
                    {
                        Id      = Temp[i].RnI32 ("Id"     ),
                        Density = Temp[i].RK    ("Density"),
                        Diffuse = Temp[i].RRGBAK("Diffuse"),
                        End     = Temp[i].RK    ("End"    ),
                        Start   = Temp[i].RK    ("Start"  ),
                    };
            }

            if ((Temp = A3D["Light", true]).NotNull)
            {
                Data.Light = new Light[Temp.Array.Length];
                for (i = 0; i < Data.Light.Length; i++)
                    Data.Light[i] = new Light
                    {
                        Id            = Temp[i].RnI32 ("Id"           ),
                        Name          = Temp[i].RS    ("Name"         ),
                        Type          = Temp[i].RS    ("Type"         ),
                        Ambient       = Temp[i].RRGBAK("Ambient"      ),
                        Diffuse       = Temp[i].RRGBAK("Diffuse"      ),
                        Incandescence = Temp[i].RRGBAK("Incandescence"),
                        Position      = Temp[i].RMT   ("Position"     ),
                        Specular      = Temp[i].RRGBAK("Specular"     ),
                        SpotDirection = Temp[i].RMT   ("SpotDirection"),
                    };
            }

            if ((Temp = A3D["MaterialList", true]).NotNull)
            {
                Data.MaterialList = new MaterialList[Temp.Array.Length];
                for (i = 0; i < Data.MaterialList.Length; i++)
                    Data.MaterialList[i] = new MaterialList
                    {
                        HashName      = Temp[i].RS    (     "HashName"),
                            Name      = Temp[i].RS    (         "Name"),
                        BlendColor    = Temp[i].RRGBAK(   "BlendColor"),
                        GlowIntensity = Temp[i].RK    ("GlowIntensity"),
                        Incandescence = Temp[i].RRGBAK("Incandescence"),
                    };
            }

            if ((Temp = A3D["MObjectHRC", true]).NotNull)
            {
                Data.MObjectHRC = new MObjectHRC[Temp.Array.Length];
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    Data.MObjectHRC[i0] = new MObjectHRC
                    {
                        MT   = Temp[i0].RMT(),
                        Name = Temp[i0].RS("Name"),
                    };

                    if ((temp = Temp[i0]["JointOrient"]).NotNull)
                        Data.MObjectHRC[i0].JointOrient = new Vector3<float?>
                        {
                            X = temp.RnF32("X"),
                            Y = temp.RnF32("Y"),
                            Z = temp.RnF32("Z"),
                        };
                    
                    if ((temp = Temp[i0]["Instance", true]).NotNull)
                    {
                        Data.MObjectHRC[i0].Instances = new MObjectHRC.Instance[temp.Array.Length];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                            Data.MObjectHRC[i0].Instances[i1] = new MObjectHRC.Instance
                            {
                                     MT = temp[i1].RMT(),
                                   Name = temp[i1].RS   (   "Name"),
                                 Shadow = temp[i1].RnI32( "Shadow"),
                                UIDName = temp[i1].RS   ("UIDName"),
                            };
                    }
                    
                    if ((temp = Temp[i0]["Node", true]).NotNull)
                    {
                        Data.MObjectHRC[i0].Node = new Node[temp.Array.Length];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            Data.MObjectHRC[i0].Node[i1] = new Node
                            {
                                    MT = temp[i1].RMT(),
                                  Name = temp[i1].RS   (  "Name"),
                                Parent = temp[i1].RnI32("Parent"),
                            };
                    }
                }
            }

            if ((Temp = A3D["MObjectHRCList", true]).NotNull)
            {
                Data.MObjectHRCList = new string[Temp.Array.Length];
                for (i = 0; i < Data.MObjectHRCList.Length; i++)
                    Data.MObjectHRCList[i] = Temp[i].RS();
            }
            
            if ((Temp = A3D["Motion", true]).NotNull)
            {
                Data.Motion = new string[Temp.Array.Length];
                for (i = 0; i < Data.Motion.Length; i++)
                    Data.Motion[i] = Temp[i].RS();
            }

            if ((Temp = A3D["Object", true]).NotNull)
            {
                Data.Object = new Object[Temp.Array.Length];
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    Data.Object[i0] = new Object
                    {
                                 MT = Temp[i0].RMT(),
                        Morph       = Temp[i0].RS   ("Morph"      ),
                        MorphOffset = Temp[i0].RnI32("MorphOffset"),
                               Name = Temp[i0].RS   (       "Name"),
                         ParentName = Temp[i0].RS   ( "ParentName"),
                            UIDName = Temp[i0].RS   (    "UIDName"),
                    };

                    if ((temp = Temp[i0]["TexturePattern", true]).NotNull)
                    {
                        Data.Object[i0].TexPat = new Object.TexturePattern[temp.Array.Length];
                        for (i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                            Data.Object[i0].TexPat[i1] = new Object.TexturePattern
                            {
                                Name      = temp[i1].RS   ("Name"     ),
                                Pat       = temp[i1].RS   ("Pat"      ),
                                PatOffset = temp[i1].RnI32("PatOffset"),
                            };
                    }

                    if ((temp = Temp[i0]["TextureTransform", true]).NotNull)
                    {
                        Data.Object[i0].TexTrans = new Object.TextureTransform[temp.Array.Length];
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                            Data.Object[i0].TexTrans[i1] = new Object.TextureTransform
                            {
                                Name           = temp[i1].RS  ("Name"          ),
                                Coverage       = temp[i1].RKUV("Coverage"      ),
                                Offset         = temp[i1].RKUV("Offset"        ),
                                Repeat         = temp[i1].RKUV("Repeat"        ),
                                   Rotate      = temp[i1].RK  (   "Rotate"     ),
                                   RotateFrame = temp[i1].RK  (   "RotateFrame"),
                                TranslateFrame = temp[i1].RKUV("TranslateFrame"),
                            };
                    }
                }
            }

            if ((Temp = A3D["ObjectHRC", true]).NotNull)
            {
                Data.ObjectHRC = new ObjectHRC[Temp.Array.Length];
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    Data.ObjectHRC[i0] = new ObjectHRC
                    {
                           Name = Temp[i0].RS   (   "Name"),
                         Shadow = Temp[i0].RnF32( "Shadow"),
                        UIDName = Temp[i0].RS   ("UIDName"),
                    };

                    if ((temp = Temp[i0]["JointOrient"]).NotNull)
                        Data.ObjectHRC[i0].JointOrient = new Vector3<float?>
                        {
                            X = temp.RF32("X"),
                            Y = temp.RF32("Y"),
                            Z = temp.RF32("Z"),
                        };

                    if ((temp = Temp[i0]["Node", true]).NotNull)
                    {
                        Data.ObjectHRC[i0].Node = new Node[temp.Array.Length];
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            Data.ObjectHRC[i0].Node[i1] = new Node
                            {
                                    MT = temp[i1].RMT(),
                                  Name = temp[i1].RS  (  "Name"),
                                Parent = temp[i1].RI32("Parent"),
                            };
                    }
                }
            }

            if ((Temp = A3D["ObjectHRCList", true]).NotNull)
            {
                Data.ObjectHRCList = new string[Temp.Array.Length];
                for (i = 0; i < Data.ObjectHRCList.Length; i++)
                    Data.ObjectHRCList[i] = Temp[i].RS();
            }

            if ((Temp = A3D["ObjectList", true]).NotNull)
            {
                Data.ObjectList = new string[Temp.Array.Length];
                for (i = 0; i < Data.ObjectList.Length; i++)
                    Data.ObjectList[i] = Temp[i].RS();
            }

            if ((Temp = A3D["PlayControl"]).NotNull)
                Data.PlayControl = new PlayControl
                {
                    Begin  = Temp.RnF32("Begin" ),
                    Div    = Temp.RnF32("Div"   ),
                    FPS    = Temp.RnF32("FPS"   ),
                    Offset = Temp.RnF32("Offset"),
                    Size   = Temp.RnF32("Size"  ),
                };

            if ((Temp = A3D["Point", true]).NotNull)
            {
                Data.Point = new ModelTransform[Temp.Array.Length];
                for (i = 0; i < Data.Point.Length; i++)
                    Data.Point[i] = Temp[i].RMT();
            }

            if ((Temp = A3D["PostProcess"]).NotNull)
                Data.PostProcess = new PostProcess
                {
                    Ambient   = Temp.RRGBAK("Ambient"  ),
                    Diffuse   = Temp.RRGBAK("Diffuse"  ),
                    LensFlare = Temp.RK    ("LensFlare"),
                    LensGhost = Temp.RK    ("LensGhost"),
                    LensShaft = Temp.RK    ("LensShaft"),
                    Specular  = Temp.RRGBAK("Specular" ),
                };

            temp.Dispose();
            Temp.Dispose();
        }

        public void MsgPackWriter(string file, bool JSON) =>
            MsgPackWriter().Write(true, file, JSON);

        public MsgPack MsgPackWriter()
        {
            MsgPack A3D = new MsgPack("A3D")
                .Add(new MsgPack("_").Add("CompressF16"     , Data._.CompressF16     )
                                     .Add("ConverterVersion", Data._.ConverterVersion)
                                     .Add("FileName"        , Data._.FileName        )
                                     .Add("PropertyVersion" , Data._.PropertyVersion ));

            if (Data.Ambient != null)
            {
                MsgPack Ambient = new MsgPack(Data.Ambient.Length, "Ambient");
                for (i = 0; i < Data.Ambient.Length; i++)
                    Ambient[i] = MsgPack.New.Add(   "LightDiffuse", Data.Ambient[i].   LightDiffuse)
                                            .Add("Name"           , Data.Ambient[i].Name           )
                                            .Add("RimLightDiffuse", Data.Ambient[i].RimLightDiffuse);
                A3D.Add(Ambient);
            }
            
            MsgPack CameraAuxiliary = new MsgPack("CameraAuxiliary")
                .Add("AutoExposure", Data.CameraAuxiliary.AutoExposure)
                .Add(    "Exposure", Data.CameraAuxiliary.    Exposure)
                .Add("Gamma"       , Data.CameraAuxiliary.Gamma       )
                .Add("GammaRate"   , Data.CameraAuxiliary.GammaRate   )
                .Add("Saturate"    , Data.CameraAuxiliary.Saturate    );
            if (CameraAuxiliary.List.Count > 0) A3D.Add(CameraAuxiliary);

            if (Data.CameraRoot != null)
            {
                MsgPack CameraRoot = new MsgPack(Data.CameraRoot.Length, "CameraRoot");
                for (i = 0; i < Data.CameraRoot.Length; i++)
                    CameraRoot[i] = MsgPack.New
                        .Add("Interest", Data.CameraRoot[i].Interest)
                        .Add(new MsgPack("ViewPoint")
                        .Add("Aspect"         , Data.CameraRoot[i].VP.Aspect         )
                        .Add("CameraApertureH", Data.CameraRoot[i].VP.CameraApertureH)
                        .Add("CameraApertureW", Data.CameraRoot[i].VP.CameraApertureW)
                        .Add("FOVHorizontal"  , Data.CameraRoot[i].VP.FOVHorizontal  )
                        .Add("FocalLength"    , Data.CameraRoot[i].VP.FocalLength    )
                        .Add("FOV"            , Data.CameraRoot[i].VP.FOV            )
                        .Add("Roll"           , Data.CameraRoot[i].VP.Roll           )
                        .Add(Data.CameraRoot[i].VP.MT))
                        .Add(Data.CameraRoot[i].   MT);
                A3D.Add(CameraRoot);
            }

            if (Data.Chara != null)
            {
                MsgPack Chara = new MsgPack(Data.Chara.Length, "Chara");
                for (i = 0; i < Data.Chara.Length; i++) Chara[i] = MsgPack.New.Add(Data.Chara[i]);
                A3D.Add(Chara);
            }

            if (Data.Curve != null)
            {
                MsgPack Curve = new MsgPack(Data.Curve.Length, "Curve");
                for (i = 0; i < Data.Curve.Length; i++)
                    Curve[i] = MsgPack.New.Add("Name", Data.Curve[i].Name).Add("CV", Data.Curve[i].CV);
                A3D.Add(Curve);
            }

            if (Data.DOF != null)
                A3D.Add(new MsgPack("DOF").Add("Name", Data.DOF.Name).Add(Data.DOF.MT));

            if (Data.Event != null)
            {
                MsgPack Event = new MsgPack(Data.Event.Length, "Events");
                for (i = 0; i < Data.Event.Length; i++)
                    Event[i] = MsgPack.New.Add("Begin"       , Data.Event[i].Begin       )
                                          .Add("ClipBegin"   , Data.Event[i].ClipBegin   )
                                          .Add("ClipEnd"     , Data.Event[i].ClipEnd     )
                                          .Add("End"         , Data.Event[i].End         )
                                          .Add("Name"        , Data.Event[i].Name        )
                                          .Add("Param1"      , Data.Event[i].Param1      )
                                          .Add("Ref"         , Data.Event[i].Ref         )
                                          .Add("TimeRefScale", Data.Event[i].TimeRefScale)
                                          .Add("Type"        , Data.Event[i].Type        );
                A3D.Add(Event);
            }

            if (Data.Fog != null)
            {
                MsgPack Fog = new MsgPack(Data.Fog.Length, "Fog");
                for (i = 0; i < Data.Fog.Length; i++)
                    Fog[i] = MsgPack.New.Add("Id"     , Data.Fog[i].Id     )
                                        .Add("Density", Data.Fog[i].Density)
                                        .Add("Diffuse", Data.Fog[i].Diffuse)
                                        .Add("End"    , Data.Fog[i].End    )
                                        .Add("Start"  , Data.Fog[i].Start  );
                A3D.Add(Fog);
            }

            if (Data.Light != null)
            {
                MsgPack Light = new MsgPack(Data.Light.Length, "Light");
                for (i = 0; i < Data.Light.Length; i++)
                    Light[i] = MsgPack.New.Add("Id"           , Data.Light[i].Id           )
                                          .Add("Name"         , Data.Light[i].Name         )
                                          .Add("Type"         , Data.Light[i].Type         )
                                          .Add("Ambient"      , Data.Light[i].Ambient      )
                                          .Add("Diffuse"      , Data.Light[i].Diffuse      )
                                          .Add("Incandescence", Data.Light[i].Incandescence)
                                          .Add("Position"     , Data.Light[i].Position     )
                                          .Add("Specular"     , Data.Light[i].Specular     )
                                          .Add("SpotDirection", Data.Light[i].SpotDirection);
                A3D.Add(Light);
            }

            if (Data.MObjectHRC != null)
            {
                MsgPack MObjectHRC = new MsgPack(Data.MObjectHRC.Length, "MObjectHRC");
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    MsgPack _MObjectHRC = MsgPack.New.Add("Name", Data.MObjectHRC[i0].Name);

                    if (IsX && Data.MObjectHRC[i0].JointOrient.NotNull)
                        _MObjectHRC.Add(new MsgPack("JointOrient")
                            .Add("X", Data.MObjectHRC[i0].JointOrient.X)
                            .Add("Y", Data.MObjectHRC[i0].JointOrient.Y)
                            .Add("Z", Data.MObjectHRC[i0].JointOrient.Z));

                    if (Data.MObjectHRC[i0].Instances != null)
                    {
                        MsgPack Instance = new MsgPack(Data.MObjectHRC[i0].Instances.Length, "Instance");
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                            Instance[i1] = MsgPack.New.Add(Data.MObjectHRC[i0].Instances[i1].MT)
                                .Add(   "Name", Data.MObjectHRC[i0].Instances[i1].   Name)
                                .Add("Shadow" , Data.MObjectHRC[i0].Instances[i1].Shadow )
                                .Add("UIDName", Data.MObjectHRC[i0].Instances[i1].UIDName);
                        _MObjectHRC.Add(Instance);
                    }

                    if (Data.MObjectHRC[i0].Node != null)
                    {
                        MsgPack Node = new MsgPack(Data.MObjectHRC[i0].Node.Length, "Node");
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            Node[i1] = MsgPack.New
                                .Add("Name"  , Data.MObjectHRC[i0].Node[i1].Name  )
                                .Add("Parent", Data.MObjectHRC[i0].Node[i1].Parent)
                                .Add(          Data.MObjectHRC[i0].Node[i1].MT    );
                        _MObjectHRC.Add(Node);
                    }

                    MObjectHRC[i0] = _MObjectHRC.Add(Data.MObjectHRC[i0].MT);
                }
                A3D.Add(MObjectHRC);
            }

            if (Data.MObjectHRCList != null)
            {
                MsgPack MObjectHRCList = new MsgPack(Data.MObjectHRCList.Length, "MObjectHRCList");
                for (i = 0; i < Data.MObjectHRCList.Length; i++)
                    MObjectHRCList[i] = Data.MObjectHRCList[i];
                A3D.Add(MObjectHRCList);
            }

            if (Data.MaterialList != null)
            {
                MsgPack MaterialList = new MsgPack(Data.MaterialList.Length, "MaterialList");
                for (i = 0; i < Data.MaterialList.Length; i++)
                    MaterialList[i] = new MsgPack("Material")
                        .Add("HashName"     , Data.MaterialList[i].HashName     )
                        .Add(    "Name"     , Data.MaterialList[i].    Name     )
                        .Add("BlendColor"   , Data.MaterialList[i].BlendColor   )
                        .Add("GlowIntensity", Data.MaterialList[i].GlowIntensity)
                        .Add("Incandescence", Data.MaterialList[i].Incandescence);
                A3D.Add(MaterialList);
            }

            if (Data.Motion != null)
            {
                MsgPack Motion = new MsgPack(Data.Motion.Length, "Motion");
                for (i = 0; i < Data.Motion.Length; i++) Motion[i] = Data.Motion[i];
                A3D.Add(Motion);
            }

            if (Data.Object != null)
            {
                MsgPack Object = new MsgPack(Data.Object.Length, "Object");
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    MsgPack _Object = MsgPack.New.Add("Morph"      , Data.Object[i0].Morph      )
                                                 .Add("MorphOffset", Data.Object[i0].MorphOffset)
                                                 .Add(      "Name" , Data.Object[i0].      Name )
                                                 .Add("ParentName" , Data.Object[i0].ParentName )
                                                 .Add(   "UIDName" , Data.Object[i0].   UIDName );
                    if (Data.Object[i0].TexPat != null)
                    {
                        MsgPack TexPat = new MsgPack(Data.Object[i0].TexPat.Length, "TexturePattern");
                        for (i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                            TexPat[i1] = MsgPack.New.Add("Name"     , Data.Object[i0].TexPat[i1].Name     )
                                                .Add("Pat"      , Data.Object[i0].TexPat[i1].Pat      )
                                                .Add("PatOffset", Data.Object[i0].TexPat[i1].PatOffset);
                        _Object.Add(TexPat);
                    }
                    if (Data.Object[i0].TexTrans != null)
                    {
                        MsgPack TexTrans = new MsgPack(Data.Object[i0].TexTrans.Length, "TextureTransform");
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                            TexTrans[i1] = MsgPack.New
                                .Add("Name"          , Data.Object[i0].TexTrans[i1].Name          )
                                .Add("Coverage"      , Data.Object[i0].TexTrans[i1].Coverage      )
                                .Add("Offset"        , Data.Object[i0].TexTrans[i1].Offset        )
                                .Add("Repeat"        , Data.Object[i0].TexTrans[i1].Repeat        )
                                .Add(   "Rotate"     , Data.Object[i0].TexTrans[i1].Rotate        )
                                .Add(   "RotateFrame", Data.Object[i0].TexTrans[i1].   RotateFrame)
                                .Add("TranslateFrame", Data.Object[i0].TexTrans[i1].TranslateFrame);
                        _Object.Add(TexTrans);
                    }
                    Object[i0] = _Object.Add(Data.Object[i0].MT);
                }
                A3D.Add(Object);
            }

            if (Data.ObjectHRC != null)
            {
                MsgPack ObjectHRC = new MsgPack(Data.ObjectHRC.Length, "ObjectHRC");
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    MsgPack _ObjectHRC = MsgPack.New.Add(   "Name", Data.ObjectHRC[i0].   Name)
                                                    .Add("Shadow" , Data.ObjectHRC[i0].Shadow )
                                                    .Add("UIDName", Data.ObjectHRC[i0].UIDName);
                    
                    if (IsX && Data.ObjectHRC[i0].JointOrient.NotNull)
                        _ObjectHRC.Add(new MsgPack("JointOrient")
                            .Add("X", Data.ObjectHRC[i0].JointOrient.X)
                            .Add("Y", Data.ObjectHRC[i0].JointOrient.Y)
                            .Add("Z", Data.ObjectHRC[i0].JointOrient.Z));

                    if (Data.ObjectHRC[i0].Node != null)
                    {
                        MsgPack Node = new MsgPack(Data.ObjectHRC[i0].Node.Length, "Node");
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            Node[i1] = MsgPack.New
                                .Add("Name"  , Data.ObjectHRC[i0].Node[i1].Name  )
                                .Add("Parent", Data.ObjectHRC[i0].Node[i1].Parent)
                                .Add(          Data.ObjectHRC[i0].Node[i1].MT    );
                        _ObjectHRC.Add(Node);
                    }
                    ObjectHRC[i0] = _ObjectHRC;
                }
                A3D.Add(ObjectHRC);
            }

            if (Data.ObjectHRCList != null)
            {
                MsgPack ObjectHRCList = new MsgPack(Data.ObjectHRCList.Length, "ObjectHRCList");
                for (i = 0; i < Data.ObjectHRCList.Length; i++) ObjectHRCList[i] = Data.ObjectHRCList[i];
                A3D.Add(ObjectHRCList);
            }

            if (Data.ObjectList != null)
            {
                MsgPack ObjectList = new MsgPack(Data.ObjectList.Length, "ObjectList");
                for (i = 0; i < Data.ObjectList.Length; i++) ObjectList[i] = Data.ObjectList[i];
                A3D.Add(ObjectList);
            }

            A3D.Add(new MsgPack("PlayControl")
                .Add("Begin" , Data.PlayControl.Begin )
                .Add("Div"   , Data.PlayControl.Div   )
                .Add("FPS"   , Data.PlayControl.FPS   )
                .Add("Offset", Data.PlayControl.Offset)
                .Add("Size"  , Data.PlayControl.Size  ));

            if (Data.Point != null)
            {
                MsgPack Point = new MsgPack(Data.Point.Length, "Point");
                for (i = 0; i < Data.Point.Length; i++)
                    Point[i] = MsgPack.New.Add(Data.Point[i]);
                A3D.Add(Point);
            }

            if (Data.PostProcess != null)
                A3D.Add(new MsgPack("PostProcess").Add("Ambient"  , Data.PostProcess.Ambient  )
                                                  .Add("Diffuse"  , Data.PostProcess.Diffuse  )
                                                  .Add("LensFlare", Data.PostProcess.LensFlare)
                                                  .Add("LensGhost", Data.PostProcess.LensGhost)
                                                  .Add("LensShaft", Data.PostProcess.LensShaft)
                                                  .Add("Specular" , Data.PostProcess.Specular ));
            return A3D;
        }

        public void Dispose()
        {
            i = i0 = i1 = SOi0 = SOi1 = 0;
            SO0 = null;
            SO1 = null;
            name = null;
            nameView = null;
            value = null;
            dataArray = null;
            UsedValues = null;
            Dict = null;
            IO = null;
            Data = default;
            Head = default;
        }
    }
    
    public static class A3DAExt
    {
        private const string d = ".";
        private const string BO = "bin_offset";
        private const string MTBO = "model_transform" + d + BO;

        [ThreadStatic] private static string value;
        [ThreadStatic] private static string[] dataArray;
        [ThreadStatic] private static int SOi;
        [ThreadStatic] private static int[] SO;

        public static ModelTransform RMT(this Dictionary<string, object> Dict, string Temp)
        {
            ModelTransform MT = new ModelTransform();
            Dict.FindValue(out MT.BinOffset, Temp + MTBO);
            
            MT.Rot        = Dict.RV3(Temp + "rot"        + d);
            MT.Scale      = Dict.RV3(Temp + "scale"      + d);
            MT.Trans      = Dict.RV3(Temp + "trans"      + d);
            MT.Visibility = Dict.RK (Temp + "visibility" + d);
            return MT;
        }

        public static Vector4<Key> RRGBAK(this Dictionary<string, object> Dict, string Temp) =>
            new Vector4<Key> { W = Dict.RK(Temp + "a" + d), Z = Dict.RK(Temp + "b" + d),
                               Y = Dict.RK(Temp + "g" + d), X = Dict.RK(Temp + "r" + d) };

        public static Vector3<Key> RV3(this Dictionary<string, object> Dict, string Temp) =>
            new Vector3<Key> { X = Dict.RK(Temp + "x" + d), Y =
                Dict.RK(Temp + "y" + d), Z = Dict.RK(Temp + "z" + d) };

        public static Vector2<Key> RKUV(this Dictionary<string, object> Dict, string Temp) =>
            new Vector2<Key> { X = Dict.RK(Temp + "U" + d), Y = Dict.RK(Temp + "V" + d) };

        public static Key RK(this Dictionary<string, object> Dict, string Temp)
        {
            Key Key = new Key();
            if ( Dict.FindValue(out Key.BinOffset, Temp + BO    )) return  Key;
            if (!Dict.FindValue(out int Type     , Temp + "type")) return default;

            Key.Type = (Key.KeyType)Type;
            if (Type == 0x0000) return Key;
            if (Type == 0x0001) { Dict.FindValue(out Key.Value, Temp + "value"); return Key; }

            int i = 0;
            if (Dict.FindValue(out int EPTypePost, Temp + "ep_type_post")) Key.EPTypePost = (Key.EPType)EPTypePost;
            if (Dict.FindValue(out int EPTypePre , Temp + "ep_type_pre" )) Key.EPTypePre  = (Key.EPType)EPTypePre ;
            Dict.FindValue(out Key.Length, Temp + "key.length");
            Dict.FindValue(out Key.Max   , Temp + "max"       );
            if (Dict.StartsWith(Temp + "raw_data"))
                Dict.FindValue(out Key.RawData.KeyType, Temp + "raw_data_key_type");

            if (Key.RawData.KeyType != 0)
            {
                ref string[] ValueList = ref Key.RawData.ValueList;
                Dict.FindValue(out Key.RawData.ValueType, Temp + "raw_data.value_type");
                if (Dict.FindValue(out value, Temp + "raw_data.value_list"))
                    ValueList = value.Split(',');
                Dict.FindValue(out Key.RawData.ValueListSize, Temp + "raw_data.value_list_size");
                value = "";

                int DS = Key.RawData.KeyType + 1;
                Key.Length = Key.RawData.ValueListSize / DS;
                Key.Keys = new KFT3[Key.Length];
                     if (Key.RawData.KeyType == 0)
                    for (i = 0; i < Key.Length; i++)
                        Key.Keys[i] = new KFT3
                        (ValueList[i * DS + 0].ToSingle());
                else if (Key.RawData.KeyType == 1)
                    for (i = 0; i < Key.Length; i++)
                        Key.Keys[i] = new KFT3
                        (ValueList[i * DS + 0].ToSingle(), ValueList[i * DS + 1].ToSingle());
                else if (Key.RawData.KeyType == 2)
                    for (i = 0; i < Key.Length; i++)
                        Key.Keys[i] = new KFT3
                        (ValueList[i * DS + 0].ToSingle(), ValueList[i * DS + 1].ToSingle(),
                         ValueList[i * DS + 2].ToSingle(), ValueList[i * DS + 2].ToSingle());
                else if (Key.RawData.KeyType == 3)
                    for (i = 0; i < Key.Length; i++)
                        Key.Keys[i] = new KFT3
                        (ValueList[i * DS + 0].ToSingle(), ValueList[i * DS + 1].ToSingle(),
                         ValueList[i * DS + 2].ToSingle(), ValueList[i * DS + 3].ToSingle());

                Key.RawData.ValueList = null;
            }
            else
            {
                Key.Keys = new KFT3[Key.Length];
                for (i = 0; i < Key.Length; i++)
                {
                    if (!Dict.FindValue(out value, Temp + "key" + d + i + d + "data")) continue;

                    dataArray = value.Replace("(", "").Replace(")", "").Split(',');
                    Type = dataArray.Length - 1;
                         if (Type == 0) Key.Keys[i] = new KFT3
                        (dataArray[0].ToSingle());
                    else if (Type == 1) Key.Keys[i] = new KFT3
                        (dataArray[0].ToSingle(), dataArray[1].ToSingle());
                    else if (Type == 2) Key.Keys[i] = new KFT3
                        (dataArray[0].ToSingle(), dataArray[1].ToSingle(),
                         dataArray[2].ToSingle(), dataArray[2].ToSingle());
                    else if (Type == 3) Key.Keys[i] = new KFT3
                        (dataArray[0].ToSingle(), dataArray[1].ToSingle(),
                         dataArray[2].ToSingle(), dataArray[3].ToSingle());
                }
            }
            return Key;
        }

        public static void W(this Stream IO, ModelTransform MT,
            string Temp, bool A3DC, bool IsX = false, byte Flags = 0b11111)
        {
            if (A3DC && !MT.Writed && (Flags & 0b10000) == 0b10000)
            { IO.W(Temp + MTBO + "=", MT.BinOffset); MT.Writed = true; }

            if (A3DC && !IsX) return;

            if ((Flags & 0b01000) == 0b01000) IO.W(MT.Rot       , Temp + "rot"        + d, A3DC);
            if ((Flags & 0b00100) == 0b00100) IO.W(MT.Scale     , Temp + "scale"      + d, A3DC);
            if ((Flags & 0b00010) == 0b00010) IO.W(MT.Trans     , Temp + "trans"      + d, A3DC);
            if ((Flags & 0b00001) == 0b00001) IO.W(MT.Visibility, Temp + "visibility" + d, A3DC);
        }

        public static void W(this Stream IO, Vector4<Key> RGBA, string Temp, bool A3DC = false)
        {
            if (RGBA.X.Type == null && RGBA.Y.Type == null && RGBA.Z.Type == null && RGBA.W.Type == null) return;
            IO.W(Temp + "=", "true");
            IO.W(RGBA.W, Temp + d + "a" + d, A3DC);
            IO.W(RGBA.Z, Temp + d + "b" + d, A3DC);
            IO.W(RGBA.Y, Temp + d + "g" + d, A3DC);
            IO.W(RGBA.X, Temp + d + "r" + d, A3DC);
        }

        public static void W(this Stream IO, Vector3<Key> Key, string Temp, bool A3DC = false)
        { IO.W(Key.X, Temp + "x" + d, A3DC); IO.W(Key.Y,
            Temp + "y" + d, A3DC); IO.W(Key.Z, Temp + "z" + d, A3DC); }

        public static void W(this Stream IO, Vector2<Key> UV, string Temp, bool A3DC = false)
        { IO.W(UV.X, Temp + "U", true, A3DC); IO.W(UV.Y, Temp + "V", true, A3DC); }

        public static void W(this Stream IO, Key Key, string Temp, bool SetBoolean, bool A3DC = false)
        { if (Key.Type == null) return; if (SetBoolean) IO.W(Temp + "=", "true"); IO.W(Key, Temp + d, A3DC); }

        public static void W(this Stream IO, Key Key, string Temp, bool A3DC = false)
        {
            if (Key.Type == null) return;

            if (A3DC) { IO.W(Temp + BO + "=", Key.BinOffset); return; }

            int i = 0;
            if (Key.Keys != null)
                if (Key.Keys.Length == 0)
                {
                    IO.W(Temp + "type=", (int)Key.Type);
                    if (Key.Type > 0) IO.W(Temp + "value=", Key.Value);
                    return;
                }

            if ((int)Key.EPTypePost > 0 && (int)Key.EPTypePost < 3) IO.W(Temp + "ep_type_post=", (int)Key.EPTypePost);
            if ((int)Key.EPTypePre  > 0 && (int)Key.EPTypePre  < 3) IO.W(Temp + "ep_type_pre=" , (int)Key.EPTypePre );
            if (Key.RawData.KeyType == 0 && Key.Keys != null)
            {
                IKF KF;
                SO = Key.Keys.Length.SortWriter();
                for (i = 0; i < Key.Keys.Length; i++)
                {
                    SOi = SO[i];
                    KF = Key.Keys[SOi].Check();
                    IO.W(Temp + "key" + d + SOi + d + "data=", KF.ToString());
                    int Type = 0;
                         if (KF is KFT0) Type = 0;
                    else if (KF is KFT1) Type = 1;
                    else if (KF is KFT2) Type = 2;
                    else if (KF is KFT3) Type = 3;
                    IO.W(Temp + "key" + d + SOi + d + "type=", Type);
                }
                IO.W(Temp + "key.length=", Key.Length);
                if (Key.Max != null) IO.W(Temp + "max=", Key.Max);
            }
            else if (Key.Keys != null)
            {
                int Length = Key.Keys.Length;
                ref int KeyType = ref Key.RawData.KeyType;
                KeyType = 0;
                IKF KF;
                if (Key.Max != null) IO.W(Temp + "max=", Key.Max);
                for (i = 0; i < Length; i++)
                {
                    KF = Key.Keys[i].Check();
                         if (KF is KFT0 && KeyType < 0)   KeyType = 0;
                    else if (KF is KFT1 && KeyType < 1)   KeyType = 1;
                    else if (KF is KFT2 && KeyType < 2)   KeyType = 2;
                    else if (KF is KFT3 && KeyType < 3) { KeyType = 3; break; }
                }
                Key.RawData.ValueListSize = Length * KeyType + Length;
                IO.W(Temp + "raw_data.value_list=");
                     if (KeyType == 0) for (i = 0; i < Length; i++)
                        IO.W(Key.Keys[i].ToT0().ToString(false) + (i + 1 < Length ? "," : ""));
                else if (KeyType == 1) for (i = 0; i < Length; i++)
                        IO.W(Key.Keys[i].ToT1().ToString(false) + (i + 1 < Length ? "," : ""));
                else if (KeyType == 2) for (i = 0; i < Length; i++)
                        IO.W(Key.Keys[i].ToT2().ToString(false) + (i + 1 < Length ? "," : ""));
                else if (KeyType == 3) for (i = 0; i < Length; i++)
                        IO.W(Key.Keys[i]       .ToString(false) + (i + 1 < Length ? "," : ""));
                IO.P--;
                IO.W('\n');
                IO.W(Temp + "raw_data.value_list_size=", Key.RawData.ValueListSize);
                IO.W(Temp + "raw_data.value_type="     , Key.RawData.ValueType    );
                IO.W(Temp + "raw_data_key_type="       , Key.RawData.  KeyType    );
            }
            IO.W(Temp + "type=", (int)Key.Type);
            if (Key.RawData.KeyType == 0 && Key.Keys == null && Key.Value != null)
                if (Key.Value != 0) IO.W(Temp + "value=", Key.Value);
        }

        public static void RMT(this Stream IO, ref ModelTransform MT, int C_F16)
        {
            if (MT.BinOffset == null) return;

            IO.P = (int)MT.BinOffset;

            IO.ReadOffset(out MT.Scale);
            IO.ReadOffset(out MT.Rot  );
            IO.ReadOffset(out MT.Trans);
            MT.Visibility = new Key { BinOffset = IO.RI32() };

            IO.RV3(ref MT.Scale     , C_F16);
            IO.RV3(ref MT.Rot       , C_F16, true);
            IO.RV3(ref MT.Trans     , C_F16);
            IO.RK (ref MT.Visibility, C_F16);
        }

        public static void RRGBAK(this Stream IO, ref Vector4<Key> RGBA, int C_F16)
        { IO.RK(ref RGBA.X, C_F16); IO.RK(ref RGBA.Y, C_F16);
          IO.RK(ref RGBA.Z, C_F16); IO.RK(ref RGBA.W, C_F16); }

        public static void RV3(this Stream IO, ref Vector3<Key> Key, int C_F16, bool F16 = false)
        { IO.RK(ref Key.X, C_F16, F16); IO.RK(ref Key.Y,
            C_F16, F16); IO.RK(ref Key.Z, C_F16, F16); }

        public static void RKUV(this Stream IO, ref Vector2<Key> UV, int C_F16)
        { IO.RK(ref UV.X, C_F16); IO.RK(ref UV.Y, C_F16); }

        public static void RK(this Stream IO, ref Key Key, int C_F16, bool F16 = false)
        {
            if (Key.BinOffset == null || Key.BinOffset < 0) return;
            
            IO.P = (int)Key.BinOffset;
            int Type = IO.RI32();
            Key.Value = IO.RF32();
            Key.Type = (Key.KeyType)(Type & 0xFF);
            if (Key.Type < Key.KeyType.Lerp) return;
            Key.Max    = IO.RF32();
            Key.Length = IO.RI32 ();
            if (Type >> 8 != 0)
            {
                Key.EPTypePost = (Key.EPType)((Type >> 12) & 0xF);
                Key.EPTypePre  = (Key.EPType)((Type >>  8) & 0xF);
            }
            Key.Keys = new KFT3[Key.Length];

            if (F16 && C_F16 == 2)
                for (int i = 0; i < Key.Keys.Length; i++)
                { ref KFT3 KF = ref Key.Keys[i]; Key.Keys[i].F  = IO.RU16(); Key.Keys[i].V  = IO.RF16  ();
                                                 Key.Keys[i].T1 = IO.RF16  (); Key.Keys[i].T2 = IO.RF16  (); }
            else if (F16 && C_F16 > 0)
                for (int i = 0; i < Key.Keys.Length; i++)
                { ref KFT3 KF = ref Key.Keys[i]; Key.Keys[i].F  = IO.RU16(); Key.Keys[i].V  = IO.RF16  ();
                                                 Key.Keys[i].T1 = IO.RF32(); Key.Keys[i].T2 = IO.RF32(); }
            else 
                for (int i = 0; i < Key.Keys.Length; i++)
                { ref KFT3 KF = ref Key.Keys[i]; Key.Keys[i].F  = IO.RF32(); Key.Keys[i].V  = IO.RF32();
                                                 Key.Keys[i].T1 = IO.RF32(); Key.Keys[i].T2 = IO.RF32(); }
        }

        public static void ReadOffset(this Stream IO, out Vector3<Key> Key)
        { Key = new Vector3<Key> { X = new Key { BinOffset = IO.RI32() },
                                   Y = new Key { BinOffset = IO.RI32() },
                                   Z = new Key { BinOffset = IO.RI32() }, }; }

        public static void WO(this Stream IO, ref ModelTransform MT, bool ReturnToOffset)
        {
            if (ReturnToOffset)
            {
                IO.P = (int)MT.BinOffset;
                IO.WriteOffset(MT.Scale);
                IO.WriteOffset(MT.Rot  );
                IO.WriteOffset(MT.Trans);
                IO.W(MT.Visibility.BinOffset);
            }
            else
            {
                MT.BinOffset = IO.P;
                IO.P += 0x30;
                IO.L   += 0x30;
            }
        }

        public static void WriteOffset(this Stream IO, Vector3<Key> Key)
        {
            IO.W(Key.X.BinOffset);
            IO.W(Key.Y.BinOffset);
            IO.W(Key.Z.BinOffset);
        }

        public static ModelTransform RMT(this MsgPack k, string name) =>
            k[name].RMT();

        public static ModelTransform RMT(this MsgPack k) =>
            new ModelTransform { Rot   = k.RV3("Rot"  ), Scale      = k.RV3("Scale"     ),
                                 Trans = k.RV3("Trans"), Visibility = k.RK ("Visibility") };

        public static Vector4<Key> RRGBAK(this MsgPack k, string name) =>
            k[name].RRGBAK();

        public static Vector4<Key> RRGBAK(this MsgPack k) =>
            new Vector4<Key> { X = k.RK("R"), Y = k.RK("G"), Z = k.RK("B"), W = k.RK("A") };

        public static Vector3<Key> RV3(this MsgPack k, string name) =>
            k[name].RV3();

        public static Vector3<Key> RV3(this MsgPack k) =>
            new Vector3<Key> { X = k.RK("X"), Y = k.RK("Y"), Z = k.RK("Z") };

        public static Vector2<Key> RKUV(this MsgPack k, string name) =>
            k[name].RKUV();

        public static Vector2<Key> RKUV(this MsgPack k) =>
            new Vector2<Key> { X = k.RK("U"), Y = k.RK("V") };

        public static Key RK(this MsgPack k, string name) =>
            k[name].ReadKey();

        public static Key ReadKey(this MsgPack k)
        {
            if (k.Object == null) return default;
            
            Key Key = new Key { Max = k.RnF32("Max"), Value = k.RnF32("Value") };
            if (Enum.TryParse(k.RS("EPTypePost"), out Key.EPType EPTypePost)) Key.EPTypePost = EPTypePost;
            if (Enum.TryParse(k.RS("EPTypePre" ), out Key.EPType EPTypePre )) Key.EPTypePre  = EPTypePre;
            if (!Enum.TryParse(k.RS("Type"), out Key.KeyType KeyType)) { Key.Value = null; return Key; }
            Key.Type = KeyType;
            if (Key.Type == 0) { Key.Value = 0; return Key; }
            else if (Key.Type < Key.KeyType.Lerp) return Key;

            if (k.RB("RawData")) Key.RawData = new Key.RawD() { KeyType = -1, ValueType = "float" };
            MsgPack Trans;
            if ((Trans = k["Trans", true]).IsNull) return Key;

            Key.Length = Trans.Array.Length;
            Key.Keys = new KFT3[Key.Length];
            for (int i = 0; i < Key.Length; i++)
            {
                     if (Trans[i].Array == null)     continue;
                else if (Trans[i].Array.Length == 0) continue;
                else if (Trans[i].Array.Length == 1)
                    Key.Keys[i] = new KFT3
                        (Trans[i][0].RF32());
                else if (Trans[i].Array.Length == 2)
                    Key.Keys[i] = new KFT3
                        (Trans[i][0].RF32(), Trans[i][1].RF32());
                else if (Trans[i].Array.Length == 3)
                    Key.Keys[i] = new KFT3
                        (Trans[i][0].RF32(), Trans[i][1].RF32(),
                         Trans[i][2].RF32(), Trans[i][2].RF32());
                else if (Trans[i].Array.Length == 4)
                    Key.Keys[i] = new KFT3
                        (Trans[i][0].RF32(), Trans[i][1].RF32(),
                         Trans[i][2].RF32(), Trans[i][3].RF32());
            }
            return Key;
        }

        public static MsgPack Add(this MsgPack MsgPack, ModelTransform MT) =>
            MsgPack.Add("Rot"       , MT.Rot       )
                   .Add("Scale"     , MT.Scale     )
                   .Add("Trans"     , MT.Trans     )
                   .Add("Visibility", MT.Visibility);

        public static MsgPack Add(this MsgPack MsgPack, string name, ModelTransform MT) =>
            MsgPack.Add(new MsgPack(name).Add("Rot"       , MT.Rot       )
                                         .Add("Scale"     , MT.Scale     )
                                         .Add("Trans"     , MT.Trans     )
                                         .Add("Visibility", MT.Visibility));

        public static MsgPack Add(this MsgPack MsgPack, string name, Vector4<Key> RGBA) =>
            (RGBA.X.Type == null && RGBA.Y.Type == null && RGBA.Z.Type == null && RGBA.W.Type == null) ? MsgPack :
           MsgPack.Add(new MsgPack(name).Add("R", RGBA.X).Add("G", RGBA.Y)
                                        .Add("B", RGBA.Z).Add("A", RGBA.W));

        public static MsgPack Add(this MsgPack MsgPack, string name, Vector3<Key> Key) =>
            MsgPack.Add(new MsgPack(name).Add("X", Key.X).Add("Y", Key.Y).Add("Z", Key.Z));

        public static MsgPack Add(this MsgPack MsgPack, string name, Vector2<Key> UV) =>
            (UV.X.Type == null && UV.Y.Type == null) ? MsgPack :
            MsgPack.Add(new MsgPack(name).Add("U", UV.X).Add("V", UV.Y));

        public static MsgPack Add(this MsgPack MsgPack, string name, Key Key)
        {
            if (Key.Type == null) return MsgPack;

            MsgPack Keys = new MsgPack(name).Add("Type", Key.Type.ToString());
            if (Key.Keys != null && Key.Type != Key.KeyType.Null)
            {
                Keys.Add("Max", Key.Max);
                if ((int)Key.EPTypePost > 0 && (int)Key.EPTypePost < 3)
                    Keys.Add("EPTypePost", Key.EPTypePost.ToString());
                if ((int)Key.EPTypePre > 0 && (int)Key.EPTypePre < 3)
                    Keys.Add("EPTypePre", Key.EPTypePre.ToString());

                if (Key.RawData.KeyType != 0) Keys.Add("RawData", true);
                
                MsgPack Trans = new MsgPack(Key.Keys.Length, "Trans");
                for (int i = 0; i < Key.Keys.Length; i++)
                {
                    IKF KF = Key.Keys[i].Check();
                         if (KF is KFT0 KFT0) Trans[i] = new MsgPack(null, new MsgPack[] 
                        { KFT0.F });
                    else if (KF is KFT1 KFT1) Trans[i] = new MsgPack(null, new MsgPack[]
                        { KFT1.F, KFT1.V });
                    else if (KF is KFT2 KFT2) Trans[i] = new MsgPack(null, new MsgPack[]
                        { KFT2.F, KFT2.V, KFT2.T });
                    else if (KF is KFT3 KFT3) Trans[i] = new MsgPack(null, new MsgPack[]
                        { KFT3.F, KFT3.V, KFT3.T1, KFT3.T2, });
                }
                Keys.Add(Trans);
            }
            else if (Key.Value != 0) Keys.Add("Value", Key.Value);
            return MsgPack.Add(Keys);
        }

        public static void W(this Stream IO, string Data, ref bool? val)
        { if (val != null) IO.W(Data, ( bool)val   ); }
        public static void W(this Stream IO, string Data,     long? val)
        { if (val != null) IO.W(Data, ( long)val   ); }
        public static void W(this Stream IO, string Data,    ulong? val)
        { if (val != null) IO.W(Data, (ulong)val   ); }
        public static void W(this Stream IO, string Data,    float? val)
        { if (val != null) IO.W(Data, (float)val   ); }
        public static void W(this Stream IO, string Data,    float? val, byte r)
        { if (val != null) IO.W(Data, (float)val, r); }
        public static void W(this Stream IO, string Data, ref bool  val)         =>
            IO.W(Data, Extensions.ToString(val));
        public static void W(this Stream IO, string Data,     long  val)         =>
            IO.W(Data,  val.ToString(   ));
        public static void W(this Stream IO, string Data,    ulong  val)         =>
            IO.W(Data,  val.ToString(   ));
        public static void W(this Stream IO, string Data,    float  val)         =>
            IO.W(Data,  val.ToString(   ));
        public static void W(this Stream IO, string Data,    float  val, byte r) =>
            IO.W(Data,  val.ToString(r  ));
        public static void W(this Stream IO, string Data,   string  val)
        { if (val != null) IO.W((Data + val + "\n").ToUTF8()); }
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
