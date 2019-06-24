using System;
using System.Collections.Generic;
using KKdMainLib;
using KKdMainLib.IO;
using KKdMainLib.Types;
using KKdMainLib.MessagePack;

namespace KKdMainLib.A3DA
{
    public class A3DA
    {
        private const bool A3DCOpt = true;
        private const string d = ".";
        private const string BO = "bin_offset";
        private const string MTBO = "model_transform.bin_offset";
        
        private int SOi0;
        private int SOi1;
        private int[] SO0;
        private int[] SO1;
        private string name;
        private string nameView;
        private string value;
        private string[] dataArray;
        private Dictionary<int?, double?> UsedValues;
        private Dictionary<string, object> Dict;

        private bool IsX => Data.Header.Format == Main.Format.X || Data.Header.Format == Main.Format.XHD;

        public Stream IO;
        public A3DAData Data;

        public A3DA()
        { Data = new A3DAData(); Dict = new Dictionary<string, object>();
            IO = File.OpenWriter(); UsedValues = new Dictionary<int?, double?>(); }

        public int A3DAReader(string file)
        {
            name = "";
            nameView = "";
            dataArray = new string[4];
            Dict = new Dictionary<string, object>();
            Data = new A3DAData { Header = new PDHead() };

            IO = File.OpenReader(file + ".a3da");
            Data.Header.Format = IO.Format = Main.Format.F;
            Data.Header.Signature = IO.ReadInt32();
            if (Data.Header.Signature == 0x41443341) Data.Header = IO.ReadHeader(true);
            if (Data.Header.Signature != 0x44334123) { IO.Close(); return 0; }

            IO.Offset = IO.Position - 4;
            Data.Header.Signature = IO.ReadInt32();

            if (Data.Header.Signature == 0x5F5F5F41)
            {
                IO.Position = 0x10;
                Data.Header.Format = IO.Format = Main.Format.DT;
            }
            else if (Data.Header.Signature == 0x5F5F5F43)
            {
                Data.Head = new Header();
                IO.Position = 0x10;
                IO.ReadInt32();
                IO.ReadInt32();
                Data.Head.HeaderOffset = IO.ReadInt32Endian(true);

                IO.Position = Data.Head.HeaderOffset;
                if (IO.ReadInt32() != 0x50) { IO.Close(); return 0; }
                Data.Head.StringOffset = IO.ReadInt32Endian(true);
                Data.Head.StringLength = IO.ReadInt32Endian(true);
                Data.Head.Count = IO.ReadInt32Endian(true);
                if (IO.ReadInt32() != 0x4C42) { IO.Close(); return 0; }
                Data.Head.BinaryOffset = IO.ReadInt32Endian(true);
                Data.Head.BinaryLength = IO.ReadInt32Endian(true);

                IO.Position = Data.Head.StringOffset;
            }
            else { IO.Close(); return 0; }

            if (Data.Header.Format < Main.Format.F || Data.Header.Format == Main.Format.FT)
                Data.Head.StringLength = IO.Length - 0x10;

            string[] STRData = IO.ReadString(Data.Head.StringLength).Replace("\r", "").Split('\n');
            for (int i = 0; i < STRData.Length; i++)
            {
                dataArray = STRData[i].Split('=');
                if (dataArray.Length == 2)
                    Dict.GetDictionary(dataArray[0], dataArray[1]);
            }
            STRData = null;

            A3DAReader();

            if (Data.Header.Format == Main.Format.F || Data.Header.Format > Main.Format.FT)
            {
                IO.Position = IO.Offset + Data.Head.BinaryOffset;
                IO.Offset = IO.Position;
                IO.Position = 0;
                A3DCReader();
            }

            IO.Close();

            name = "";
            nameView = "";
            dataArray = null;
            Dict = null;
            return 1;
        }

        private void A3DAReader()
        {
            int i0 = 0;
            int i1 = 0;

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
                    AutoExposure = Dict.ReadKey(name + "auto_exposure" + d),
                        Exposure = Dict.ReadKey(name +      "exposure" + d),
                    Gamma        = Dict.ReadKey(name + "gamma"         + d),
                    GammaRate    = Dict.ReadKey(name + "gamma_rate"    + d),
                    Saturate     = Dict.ReadKey(name + "saturate"      + d)
                };
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
                    Ambient   = Dict.ReadRGBAKey(name + "Ambient"    + d),
                    Diffuse   = Dict.ReadRGBAKey(name + "Diffuse"    + d),
                    Specular  = Dict.ReadRGBAKey(name + "Specular"   + d),
                    LensFlare = Dict.ReadKey    (name + "lens_flare" + d),
                    LensGhost = Dict.ReadKey    (name + "lens_ghost" + d),
                    LensShaft = Dict.ReadKey    (name + "lens_shaft" + d),
                };
            }

            if (Dict.FindValue(out value, "dof.name"))
            {
                Data.DOF = new DOF { Name = value };
                Data.Header.Format = Main.Format.FT;
                Data.DOF.MT = Dict.ReadMT("dof" + d);
            }

            if (Dict.FindValue(out value, "ambient.length"))
            {
                Data.Ambient = new Ambient[int.Parse(value)];
                Data.Header.Format = Main.Format.MGF;
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    name = "ambient" + d + i0 + d;
                    Dict.FindValue(out Data.Ambient[i0].Name, name + "name");
                    Data.Ambient[i0].   LightDiffuse = Dict.ReadRGBAKey(name +    "light.Diffuse" + d);
                    Data.Ambient[i0].RimLightDiffuse = Dict.ReadRGBAKey(name + "rimlight.Diffuse" + d);
                }
            }

            if (Dict.FindValue(out value, "camera_root.length"))
            {
                Data.CameraRoot = new CameraRoot[int.Parse(value)];
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    name = "camera_root" + d + i0 + d;
                    nameView = name + "view_point" + d;

                    Dict.FindValue(out Data.CameraRoot[i0].ViewPoint.
                        Aspect         , nameView + "aspect"           );
                    Dict.FindValue(out Data.CameraRoot[i0].ViewPoint.
                        CameraApertureH, nameView + "camera_aperture_h");
                    Dict.FindValue(out Data.CameraRoot[i0].ViewPoint.
                        CameraApertureW, nameView + "camera_aperture_w");
                    Dict.FindValue(out Data.CameraRoot[i0].ViewPoint.
                        FOVHorizontal  , nameView + "fov_is_horizontal");

                    Data.CameraRoot[i0].MT           = Dict.ReadMT(name);
                    Data.CameraRoot[i0].Interest     = Dict.ReadMT(name + "interest" + d);
                    Data.CameraRoot[i0].ViewPoint.MT = Dict.ReadMT(nameView);
                    Data.CameraRoot[i0].ViewPoint.FocalLength = Dict.ReadKey(nameView + "focal_length" + d);
                    Data.CameraRoot[i0].ViewPoint.FOV         = Dict.ReadKey(nameView +          "fov" + d);
                    Data.CameraRoot[i0].ViewPoint.Roll        = Dict.ReadKey(nameView +         "roll" + d);
                }
            }

            if (Dict.FindValue(out value, "chara.length"))
            {
                Data.Chara = new ModelTransform[int.Parse(value)];
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    Data.Chara[i0] = Dict.ReadMT("chara" + d + i0 + d);
            }

            if (Dict.FindValue(out value, "curve.length"))
            {
                Data.Curve = new Curve[int.Parse(value)];
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    name = "curve" + d + i0 + d;

                    Dict.FindValue(out Data.Curve[i0].Name, name + "name");
                    Data.Curve[i0].CV = Dict.ReadKey(name + "cv" + d);
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
                    Data.Fog[i0].Density = Dict.ReadKey    (name + "density" + d);
                    Data.Fog[i0].Diffuse = Dict.ReadRGBAKey(name + "Diffuse" + d);
                    Data.Fog[i0].End     = Dict.ReadKey    (name +     "end" + d);
                    Data.Fog[i0].Start   = Dict.ReadKey    (name +   "start" + d);
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

                    Data.Light[i0].Ambient       = Dict.ReadRGBAKey(name +        "Ambient" + d);
                    Data.Light[i0].Diffuse       = Dict.ReadRGBAKey(name +        "Diffuse" + d);
                    Data.Light[i0].Incandescence = Dict.ReadRGBAKey(name +  "Incandescence" + d);
                    Data.Light[i0].Specular      = Dict.ReadRGBAKey(name +       "Specular" + d);
                    Data.Light[i0].Position      = Dict.ReadMT     (name +       "position" + d);
                    Data.Light[i0].SpotDirection = Dict.ReadMT     (name + "spot_direction" + d);
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
                        Data.MObjectHRC[i0].JointOrient = new Vector3<double?>();
                        Dict.FindValue(out Data.MObjectHRC[i0].JointOrient.X, name + "joint_orient.x");
                        Dict.FindValue(out Data.MObjectHRC[i0].JointOrient.Y, name + "joint_orient.y");
                        Dict.FindValue(out Data.MObjectHRC[i0].JointOrient.Z, name + "joint_orient.z");
                    }

                    if (Dict.FindValue(out value, name + "instance.length"))
                    {
                        Data.MObjectHRC[i0].Instance = new Instance[int.Parse(value)];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                        {
                            nameView = name + "instance" + d + i1 + d;

                            Dict.FindValue(out Data.MObjectHRC[i0].Instance[i1].   Name, nameView +     "name");
                            Dict.FindValue(out Data.MObjectHRC[i0].Instance[i1]. Shadow, nameView +   "shadow");
                            Dict.FindValue(out Data.MObjectHRC[i0].Instance[i1].UIDName, nameView + "uid_name");

                            Data.MObjectHRC[i0].Instance[i1].MT = Dict.ReadMT(nameView);
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

                            Data.MObjectHRC[i0].Node[i1].MT = Dict.ReadMT(nameView);
                        }
                    }
                    
                    Data.MObjectHRC[i0].MT = Dict.ReadMT(name);
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
                Data.Header.Format = Main.Format.X;
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    name = "material_list" + d + i0 + d;
                    Dict.FindValue(out Data.MaterialList[i0].HashName, name + "hash_name");
                    Dict.FindValue(out Data.MaterialList[i0].    Name, name +      "name");

                    Data.MaterialList[i0].BlendColor    = Dict.ReadRGBAKey(name +    "blend_color" + d);
                    Data.MaterialList[i0].GlowIntensity = Dict.ReadKey    (name + "glow_intensity" + d);
                    Data.MaterialList[i0].Incandescence = Dict.ReadRGBAKey(name +  "incandescence" + d);
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
                        Data.Object[i0].TP = new Object.TexturePattern[int.Parse(value)];
                        for (i1 = 0; i1 < Data.Object[i0].TP.Length; i1++)
                        {
                            nameView = name + "tex_pat" + d + i1 + d;
                            Dict.FindValue(out Data.Object[i0].TP[i1].Name     , nameView + "name"      );
                            Dict.FindValue(out Data.Object[i0].TP[i1].Pat      , nameView + "pat"       );
                            Dict.FindValue(out Data.Object[i0].TP[i1].PatOffset, nameView + "pat_offset");
                        }
                    }

                    if (Dict.FindValue(out value, name + "tex_transform.length"))
                    {
                        Data.Object[i0].TT = new Object.TextureTransform[int.Parse(value)];
                        for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                        {
                            nameView = name + "tex_transform" + d + i1 + d;

                            Dict.FindValue(out Data.Object[i0].TT[i1].Name, nameView + "name");
                            Data.Object[i0].TT[i1].C  = Dict.ReadKeyUV(nameView + "coverage"      );
                            Data.Object[i0].TT[i1].O  = Dict.ReadKeyUV(nameView + "offset"        );
                            Data.Object[i0].TT[i1].R  = Dict.ReadKeyUV(nameView + "repeat"        );
                            Data.Object[i0].TT[i1].Ro = Dict.ReadKey  (nameView + "rotate"     + d);
                            Data.Object[i0].TT[i1].RF = Dict.ReadKey  (nameView + "rotateFrame"+ d);
                            Data.Object[i0].TT[i1].TF = Dict.ReadKeyUV(nameView + "translateFrame");
                        }
                    }
                    
                    Data.Object[i0].MT = Dict.ReadMT(name);
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
                        Data.ObjectHRC[i0].JointOrient = new Vector3<double?>();
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

                            Data.ObjectHRC[i0].Node[i1].MT = Dict.ReadMT(nameView);
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
                    Data.Point[i0] = Dict.ReadMT("point" + d + i0 + d);
            }
        }

        public void A3DAWriter(bool A3DC = false)
        {
            int i0 = 0;
            int i1 = 0;
            DateTime date = DateTime.Now;
            if (A3DC && Data._.CompressF16 != null)
                if (Data._.CompressF16 != 0)
                    IO.Write("", "#-compress_f16");
            if (!A3DC)
                IO.Write("#A3DA__________\n");
            IO.Write("#", DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss yyyy",
                System.Globalization.CultureInfo.InvariantCulture));
            if (A3DC && Data._.CompressF16 != 0)
                IO.Write("_.compress_f16=", Data._.CompressF16);

            IO.Write("_.converter.version=", Data._.ConverterVersion);
            IO.Write("_.file_name=", Data._.FileName);
            IO.Write("_.property.version=", Data._.PropertyVersion);

            if (Data.Ambient != null && Data.Header.Format == Main.Format.MGF)
            {
                SO0 = Data.Ambient.Length.SortWriter();
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "ambient" + d + SOi0 + d;

                    IO.Write(Data.Ambient[SOi0].   LightDiffuse, name,    "light.Diffuse", A3DC);
                    IO.Write(name + "name=", Data.Ambient[SOi0].Name);
                    IO.Write(Data.Ambient[SOi0].RimLightDiffuse, name, "rimlight.Diffuse", A3DC);
                }
                IO.Write("fog.length=", Data.Fog.Length);
            }

            if (Data.CameraAuxiliary != null)
            {
                name = "camera_auxiliary" + d;
                IO.Write(Data.CameraAuxiliary.AutoExposure, name, "auto_exposure", A3DC);
                IO.Write(Data.CameraAuxiliary.    Exposure, name,      "exposure", A3DC);
                IO.Write(Data.CameraAuxiliary.Gamma       , name, "gamma"        , A3DC);
                IO.Write(Data.CameraAuxiliary.GammaRate   , name, "gamma_rate"   , A3DC);
                IO.Write(Data.CameraAuxiliary.Saturate    , name, "saturate"     , A3DC);
            }

            if (Data.CameraRoot != null)
            {
                SO0 = Data.CameraRoot.Length.SortWriter();
                SOi0 = 0;
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "camera_root" + d + SOi0 + d;
                    nameView = name + "view_point" + d;

                    IO.Write(Data.CameraRoot[SOi0].Interest, name + "interest" + d, A3DC, IsX);
                    IO.Write(Data.CameraRoot[SOi0].MT, name, A3DC, IsX, 0b11110);
                    IO.Write(nameView + "aspect=", Data.CameraRoot[i0].ViewPoint.Aspect);
                    if (Data.CameraRoot[i0].ViewPoint.CameraApertureH != null)
                        IO.Write(nameView + "camera_aperture_h=",
                            Data.CameraRoot[i0].ViewPoint.CameraApertureH);
                    if (Data.CameraRoot[i0].ViewPoint.CameraApertureW != null)
                        IO.Write(nameView + "camera_aperture_w=", Data.CameraRoot[i0].ViewPoint.CameraApertureW);
                    IO.Write(Data.CameraRoot[SOi0].ViewPoint.FocalLength, nameView + "focal_length" + d, A3DC);
                    IO.Write(Data.CameraRoot[SOi0].ViewPoint.FOV, nameView + "fov" + d, A3DC);
                    IO.Write(nameView + "fov_is_horizontal=", Data.CameraRoot[i0].ViewPoint.FOVHorizontal);
                    IO.Write(Data.CameraRoot[SOi0].ViewPoint.MT  , nameView, A3DC, IsX, 0b10000);
                    IO.Write(Data.CameraRoot[SOi0].ViewPoint.Roll, nameView + "roll" + d, A3DC);
                    IO.Write(Data.CameraRoot[SOi0].ViewPoint.MT  , nameView, A3DC, IsX, 0b01111);
                    IO.Write(Data.CameraRoot[SOi0]          .MT  , name    , A3DC, IsX, 0b00001);
                }
                IO.Write("camera_root.length=", Data.CameraRoot.Length);
            }

            if (Data.Chara != null)
            {
                SO0 = Data.Chara.Length.SortWriter();
                name = "chara" + d;

                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    IO.Write(Data.Chara[SO0[i0]], name + SO0[i0] + d, A3DC, IsX);
                IO.Write(name + "length=", Data.Chara.Length);
            }

            if (Data.Curve != null)
            {
                SO0 = Data.Curve.Length.SortWriter();
                SOi0 = 0;

                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "curve" + d + SOi0 + d;

                    IO.Write(Data.Curve[SOi0].CV, name + "cv" + d, A3DC);
                    IO.Write(name + "name=", Data.Curve[SOi0].Name);
                }
                IO.Write("curve.length=", Data.Curve.Length);
            }

            if (Data.DOF != null && Data.Header.Format == Main.Format.FT)
            {
                IO.Write("dof.name=", Data.DOF.Name);
                IO.Write(Data.DOF.MT, "dof" + d, A3DC, IsX);
            }

            if (Data.Event != null)
            {
                SO0 = Data.Event.Length.SortWriter();
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "event" + d + SOi0 + d;

                    IO.Write(name + "begin="         , Data.Event[SOi0].Begin       );
                    IO.Write(name + "clip_begin="    , Data.Event[SOi0].ClipBegin   );
                    IO.Write(name + "clip_en="       , Data.Event[SOi0].ClipEnd     );
                    IO.Write(name + "end="           , Data.Event[SOi0].End         );
                    IO.Write(name + "name="          , Data.Event[SOi0].Name        );
                    IO.Write(name + "param1="        , Data.Event[SOi0].Param1      );
                    IO.Write(name + "ref="           , Data.Event[SOi0].Ref         );
                    IO.Write(name + "time_ref_scale=", Data.Event[SOi0].TimeRefScale);
                    IO.Write(name + "type="          , Data.Event[SOi0].Type        );
                }
            }

            if (Data.Fog != null)
            {
                SO0 = Data.Fog.Length.SortWriter();
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "fog" + d + SOi0 + d;

                    IO.Write(Data.Fog[SOi0].Diffuse, name, "Diffuse", A3DC);
                    IO.Write(Data.Fog[SOi0].Density, name, "density", A3DC);
                    IO.Write(Data.Fog[SOi0].End    , name, "end"    , A3DC);
                    IO.Write(name + "id=", Data.Fog[SOi0].Id);
                    IO.Write(Data.Fog[SOi0].Start  , name, "start"  , A3DC);
                }
                IO.Write("fog.length=", Data.Fog.Length);
            }

            if (Data.Light != null)
            {
                SO0 = Data.Light.Length.SortWriter();
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "light" + d + SOi0 + d;

                    IO.Write(Data.Light[SOi0].Ambient      , name, "Ambient"      , A3DC);
                    IO.Write(Data.Light[SOi0].Diffuse      , name, "Diffuse"      , A3DC);
                    IO.Write(Data.Light[SOi0].Incandescence, name, "Incandescence", A3DC);
                    IO.Write(Data.Light[SOi0].Specular     , name, "Specular"     , A3DC);
                    IO.Write(name + "id="  , Data.Light[SOi0].Id  );
                    IO.Write(name + "name=", Data.Light[SOi0].Name);
                    IO.Write(Data.Light[SOi0].Position     , name + "position"       + d, A3DC, IsX);
                    IO.Write(Data.Light[SOi0].SpotDirection, name + "spot_direction" + d, A3DC, IsX);
                    IO.Write(name + "type=", Data.Light[SOi0].Type);
                }
                IO.Write("light.length=", Data.Light.Length);
            }

            if (Data.MObjectHRC != null)
            {
                SO0 = Data.MObjectHRC.Length.SortWriter();
                SOi0 = 0;
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "m_objhrc" + d + SOi0 + d;

                    if (IsX && Data.MObjectHRC[i0].JointOrient != null)
                    {
                        IO.Write(name + "joint_orient.x=", Data.MObjectHRC[SOi0].JointOrient.X);
                        IO.Write(name + "joint_orient.y=", Data.MObjectHRC[SOi0].JointOrient.Y);
                        IO.Write(name + "joint_orient.z=", Data.MObjectHRC[SOi0].JointOrient.Z);
                    }

                    if (Data.MObjectHRC[i0].Instance != null)
                    {
                        nameView = name + "instance" + d;
                        SO1 = Data.MObjectHRC[i0].Instance.Length.SortWriter();
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            IO.Write(Data.MObjectHRC[SOi0].Instance[SOi1].MT,
                                nameView + SOi1 + d, A3DC, IsX, 0b10000);
                            IO.Write(nameView + SOi1 + d +     "name=",
                                Data.MObjectHRC[SOi0].Instance[SOi1].   Name);
                            IO.Write(Data.MObjectHRC[SOi0].Instance[SOi1].MT,
                                nameView + SOi1 + d, A3DC, IsX, 0b01100);
                            IO.Write(nameView + SOi1 + d +   "shadow=",
                                Data.MObjectHRC[SOi0].Instance[SOi1]. Shadow);
                            IO.Write(Data.MObjectHRC[SOi0].Instance[SOi1].MT,
                                nameView + SOi1 + d, A3DC, IsX, 0b00010);
                            IO.Write(nameView + SOi1 + d + "uid_name=",
                                Data.MObjectHRC[SOi0].Instance[SOi1].UIDName);
                            IO.Write(Data.MObjectHRC[SOi0].Instance[SOi1].MT,
                                nameView + SOi1 + d, A3DC, IsX, 0b00001);
                        }
                        IO.Write(nameView + "length=", Data.MObjectHRC[SOi0].Instance.Length);
                    }

                    IO.Write(Data.MObjectHRC[SOi0].MT, name, A3DC, IsX, 0b10000);
                    IO.Write(name + "name=", Data.MObjectHRC[SOi0].Name);

                    if (Data.MObjectHRC[i0].Node != null)
                    {
                        nameView = name + "node" + d;
                        SO1 = Data.MObjectHRC[i0].Node.Length.SortWriter();
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            IO.Write(Data.MObjectHRC[SOi0].Node[SOi1].MT,
                                nameView + SOi1 + d, A3DC, IsX, 0b10000);
                            IO.Write(nameView + SOi1 + d +   "name=", Data.MObjectHRC[SOi0].Node[SOi1].Name  );
                            IO.Write(nameView + SOi1 + d + "parent=", Data.MObjectHRC[SOi0].Node[SOi1].Parent);
                            IO.Write(Data.MObjectHRC[SOi0].Node[SOi1].MT,
                                nameView + SOi1 + d, A3DC, IsX, 0b01111);
                        }
                        IO.Write(nameView + "length=", Data. MObjectHRC[SOi0].Node.Length);
                    }

                    IO.Write(Data.MObjectHRC[SOi0].MT, name, A3DC, IsX, 0b01111);
                }
                IO.Write("m_objhrc.length=", Data.MObjectHRC.Length);
            }

            if (Data.MObjectHRCList != null)
            {
                SO0 = Data.MObjectHRCList.Length.SortWriter();
                name = "m_objhrc_list" + d;
                for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                    IO.Write(name + SO0[i0] + "=", Data.MObjectHRCList[SO0[i0]]);
                IO.Write(name + "length=", Data.MObjectHRCList.Length);
            }

            if (Data.MaterialList != null && IsX)
            {
                SO0 = Data.MaterialList.Length.SortWriter();
                name = "material_list" + d;
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    IO.Write(Data.MaterialList[SOi0].BlendColor   , name + SOi0 + d,  "blend_color"   + d, A3DC);
                    IO.Write(Data.MaterialList[SOi0].GlowIntensity, name + SOi0 + d + "glow_intensity"   , A3DC);
                    IO.Write(name + SOi0 + "hash_name=", Data.MaterialList[SOi0].HashName);
                    IO.Write(Data.MaterialList[SOi0].Incandescence, name + SOi0 + d,  "incandescence" + d, A3DC);
                    IO.Write(name + SOi0 + "name=", Data.MaterialList[SOi0].Name);
                }
                IO.Write(name + "length=", Data.MaterialList.Length);
            }

            if (Data.Motion != null)
            {
                SO0 = Data.Motion.Length.SortWriter();
                name = "motion" + d;
                for (i0 = 0; i0 < Data.Motion.Length; i0++)
                    IO.Write(name + SO0[i0] + d + "name=", Data.Motion[SO0[i0]]);
                IO.Write(name + "length=", Data.Motion.Length);
            }

            if (Data.Object != null)
            {
                SO0 = Data.Object.Length.SortWriter();
                SOi0 = 0;
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "object" + d + SOi0 + d;

                    IO.Write(Data.Object[SOi0].MT, name, A3DC, IsX, 0b10000);
                    if (Data.Object[SOi0].Morph != null)
                    {
                        IO.Write(name + "morph="       , Data.Object[SOi0].Morph      );
                        IO.Write(name + "morph_offset=", Data.Object[SOi0].MorphOffset);
                    }
                    IO.Write(name + "name="       , Data.Object[SOi0].Name      );
                    IO.Write(name + "parent_name=", Data.Object[SOi0].ParentName);
                    IO.Write(Data.Object[SOi0].MT, name, A3DC, IsX, 0b01100);

                    if (Data.Object[SOi0].TP != null)
                    {
                        nameView = name + "tex_pat" + d;
                        SO1 = Data.Object[SOi0].TP.Length.SortWriter();
                        for (i1 = 0; i1 < Data.Object[SOi0].TP.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            IO.Write(nameView + SOi1 + d + "name="      , Data.Object[SOi0].TP[SOi1].Name     );
                            IO.Write(nameView + SOi1 + d + "pat="       , Data.Object[SOi0].TP[SOi1].Pat      );
                            IO.Write(nameView + SOi1 + d + "pat_offset=", Data.Object[SOi0].TP[SOi1].PatOffset);
                        }
                        IO.Write(nameView + "length=", Data.Object[SOi0].TP.Length);
                    }

                    if (Data.Object[SOi0].TT != null)
                    {
                        SO1 = Data.Object[SOi0].TT.Length.SortWriter();
                        nameView = name + "tex_transform" + d;
                        for (i1 = 0; i1 < Data.Object[SOi0].TT.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            IO.Write(nameView + SOi1 + d + "name=", Data.Object[SOi0].TT[SOi1].Name);
                            IO.Write(Data.Object[SOi0].TT[SOi1].C , nameView + SOi1 + d, "coverage"      , A3DC);
                            IO.Write(Data.Object[SOi0].TT[SOi1].O , nameView + SOi1 + d, "offset"        , A3DC);
                            IO.Write(Data.Object[SOi0].TT[SOi1].R , nameView + SOi1 + d, "repeat"        , A3DC);
                            IO.Write(Data.Object[SOi0].TT[SOi1].Ro, nameView + SOi1 + d, "rotate"        , A3DC);
                            IO.Write(Data.Object[SOi0].TT[SOi1].RF, nameView + SOi1 + d, "rotateFrame"   , A3DC);
                            IO.Write(Data.Object[SOi0].TT[SOi1].TF, nameView + SOi1 + d, "translateFrame", A3DC);
                        }
                        IO.Write(nameView + "length=", + Data.Object[SOi0].TT.Length);
                    }

                    IO.Write(Data.Object[SOi0].MT, name, A3DC, IsX, 0b00010);
                    IO.Write(name + "uid_name=", Data.Object[SOi0].UIDName);
                    IO.Write(Data.Object[SOi0].MT, name, A3DC, IsX, 0b00001);
                }
                IO.Write("object.length=", Data.Object.Length);
            }

            if (Data.ObjectList != null)
            {
                SO0 = Data.ObjectList.Length.SortWriter();
                for (i0 = 0; i0 < Data.ObjectList.Length; i0++)
                    IO.Write("object_list" + d + SO0[i0] + "=", Data.ObjectList[SO0[i0]]);
                IO.Write("object_list.length=", Data.ObjectList.Length);
            }

            if (Data.ObjectHRC != null)
            {
                SO0 = Data.ObjectHRC.Length.SortWriter();
                SOi0 = 0;
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "objhrc" + d + SOi0 + d;
                    IO.Write(name + "name=", Data.ObjectHRC[SOi0].Name);

                    if (IsX && Data.ObjectHRC[i0].JointOrient != null)
                    {
                        IO.Write(name + "joint_orient.x=", Data.ObjectHRC[SOi0].JointOrient.X);
                        IO.Write(name + "joint_orient.y=", Data.ObjectHRC[SOi0].JointOrient.Y);
                        IO.Write(name + "joint_orient.z=", Data.ObjectHRC[SOi0].JointOrient.Z);
                    }

                    if (Data.ObjectHRC[i0].Node != null)
                    {
                        SO1 = Data.ObjectHRC[i0].Node.Length.SortWriter();
                        nameView = name + "node" + d;
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            IO.Write(Data.ObjectHRC[SOi0].Node[SOi1].MT, nameView + SOi1 + d, A3DC, IsX, 0b10000);
                            IO.Write(nameView + SOi1 + d + "name="  , Data.ObjectHRC[SOi0].Node[SOi1].Name  );
                            IO.Write(nameView + SOi1 + d + "parent=", Data.ObjectHRC[SOi0].Node[SOi1].Parent);
                            IO.Write(Data.ObjectHRC[SOi0].Node[SOi1].MT, nameView + SOi1 + d, A3DC, IsX, 0b01111);
                        }
                        IO.Write(nameView + "length=", Data.ObjectHRC[SOi0].Node.Length);
                    }

                    if (Data.ObjectHRC[SOi0].Shadow != null)
                        IO.Write(name + "shadow=", Data.ObjectHRC[SOi0].Shadow);
                    IO.Write(name + "uid_name=", Data.ObjectHRC[SOi0].UIDName);
                }
                IO.Write("objhrc.length=", Data.ObjectHRC.Length);
            }

            if (Data.ObjectHRCList != null)
            {
                SO0 = Data.ObjectHRCList.Length.SortWriter();
                for (i0 = 0; i0 < Data.ObjectHRCList.Length; i0++)
                    IO.Write("objhrc_list" + d + SO0[i0] + "=", Data.ObjectHRCList[SO0[i0]]);
                IO.Write("objhrc_list.length=", Data.ObjectHRCList.Length);
            }

            if (Data.PlayControl != null)
            {
                IO.Write("play_control.begin=", Data.PlayControl.Begin);
                if (Data.PlayControl.Div    != null && A3DC)
                    IO.Write("play_control.div=", Data.PlayControl.Div);
                IO.Write("play_control.fps=", Data.PlayControl.FPS);
                if (Data.PlayControl.Offset != null)
                { if ( A3DC) { IO.Write("play_control.offset=", Data.PlayControl.Offset);
                               IO.Write("play_control.size="  , Data.PlayControl.Size  ); }
                  else IO.Write("play_control.size=", Data.PlayControl.Size + Data.PlayControl.Offset);
                }
                else   IO.Write("play_control.size=", Data.PlayControl.Size);
            }

            if (Data.PostProcess != null)
            {
                name = "post_process" + d;
                IO.Write(Data.PostProcess.Ambient  , name, "Ambient"   , A3DC);
                IO.Write(Data.PostProcess.Diffuse  , name, "Diffuse"   , A3DC);
                IO.Write(Data.PostProcess.Specular , name, "Specular"  , A3DC);
                IO.Write(Data.PostProcess.LensFlare, name, "lens_flare", A3DC);
                IO.Write(Data.PostProcess.LensGhost, name, "lens_ghost", A3DC);
                IO.Write(Data.PostProcess.LensShaft, name, "lens_shaft", A3DC);
            }

            if (Data.Point != null)
            {
                SO0 = Data.Point.Length.SortWriter();
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    IO.Write(Data.Point[SO0[i0]], "point" + d + SO0[i0] + d, A3DC, IsX);
                IO.Write("point.length=", Data.Point.Length);
            }

            if (!A3DC)
                IO.Close();
        }

        private int CompressF16 => Data._.CompressF16.GetValueOrDefault();

        private void A3DCReader()
        {
            int i0 = 0;
            int i1 = 0;

            if (Data.Ambient != null)
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    IO.ReadRGBAKey(ref Data.Ambient[i0].   LightDiffuse, CompressF16);
                    IO.ReadRGBAKey(ref Data.Ambient[i0].RimLightDiffuse, CompressF16);
                }

            if (Data.CameraAuxiliary != null)
            {
                IO.ReadKey(ref Data.CameraAuxiliary.AutoExposure, CompressF16);
                IO.ReadKey(ref Data.CameraAuxiliary.    Exposure, CompressF16);
                IO.ReadKey(ref Data.CameraAuxiliary.Gamma       , CompressF16);
                IO.ReadKey(ref Data.CameraAuxiliary.GammaRate   , CompressF16);
                IO.ReadKey(ref Data.CameraAuxiliary.Saturate    , CompressF16);
            }

            if (Data.CameraRoot != null)
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    IO.ReadMT(ref Data.CameraRoot[i0].MT          , CompressF16);
                    IO.ReadMT(ref Data.CameraRoot[i0].Interest    , CompressF16);
                    IO.ReadMT(ref Data.CameraRoot[i0].ViewPoint.MT, CompressF16);
                    IO.ReadKey(ref Data.CameraRoot[i0].ViewPoint.FocalLength, CompressF16);
                    IO.ReadKey(ref Data.CameraRoot[i0].ViewPoint.FOV        , CompressF16);
                    IO.ReadKey(ref Data.CameraRoot[i0].ViewPoint.Roll       , CompressF16);
                }

            if (Data.Chara != null)
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    IO.ReadMT(ref Data.Chara[i0], CompressF16);

            if (Data.Curve != null)
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                    IO.ReadKey(ref Data.Curve[i0].CV, CompressF16);

            if (Data.DOF != null)
                    IO.ReadMT(ref Data.DOF.MT, CompressF16);

            if (Data.Fog != null)
                for (i0 = 0; i0 < Data.Fog.Length; i0++)
                {
                    IO.ReadKey    (ref Data.Fog[i0].Density, CompressF16);
                    IO.ReadRGBAKey(ref Data.Fog[i0].Diffuse, CompressF16);
                    IO.ReadKey    (ref Data.Fog[i0].End    , CompressF16);
                    IO.ReadKey    (ref Data.Fog[i0].Start  , CompressF16);
                }

            if (Data.Light != null)
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    IO.ReadRGBAKey(ref Data.Light[i0].Ambient      , CompressF16);
                    IO.ReadRGBAKey(ref Data.Light[i0].Diffuse      , CompressF16);
                    IO.ReadRGBAKey(ref Data.Light[i0].Incandescence, CompressF16);
                    IO.ReadMT     (ref Data.Light[i0].Position     , CompressF16);
                    IO.ReadRGBAKey(ref Data.Light[i0].Specular     , CompressF16);
                    IO.ReadMT     (ref Data.Light[i0].SpotDirection, CompressF16);
                }

            if (Data.MObjectHRC != null)
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    IO.ReadMT(ref Data.MObjectHRC[i0].MT, CompressF16);

                    if (Data.MObjectHRC[i0].Instance != null)
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                            IO.ReadMT(ref Data.MObjectHRC[i0].Instance[i1].MT, CompressF16);

                    if (Data.MObjectHRC[i0].Node != null)
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            IO.ReadMT(ref Data.MObjectHRC[i0].Node[i1].MT, CompressF16);
                }

            if (Data.MaterialList != null)
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    IO.ReadRGBAKey(ref Data.MaterialList[i0].BlendColor   , CompressF16);
                    IO.ReadKey    (ref Data.MaterialList[i0].GlowIntensity, CompressF16);
                    IO.ReadRGBAKey(ref Data.MaterialList[i0].Incandescence, CompressF16);
                }

            if (Data.Object != null)
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    IO.ReadMT(ref Data.Object[i0].MT, CompressF16);
                    if (Data.Object[i0].TT != null)
                        for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                        {
                            IO.ReadKeyUV(ref Data.Object[i0].TT[i1].C , CompressF16);
                            IO.ReadKeyUV(ref Data.Object[i0].TT[i1].O , CompressF16);
                            IO.ReadKeyUV(ref Data.Object[i0].TT[i1].R , CompressF16);
                            IO.ReadKey  (ref Data.Object[i0].TT[i1].Ro, CompressF16);
                            IO.ReadKey  (ref Data.Object[i0].TT[i1].RF, CompressF16);
                            IO.ReadKeyUV(ref Data.Object[i0].TT[i1].TF, CompressF16);
                        }
                }

            if (Data.ObjectHRC != null)
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                    if (Data.ObjectHRC[i0].Node != null)
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            IO.ReadMT(ref Data.ObjectHRC[i0].Node[i1].MT, CompressF16);


            if (Data.Point != null)
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    IO.ReadMT(ref Data.Point[i0], CompressF16);

            if (Data.PostProcess != null)
            {
                IO.ReadRGBAKey(ref Data.PostProcess.Ambient  , CompressF16);
                IO.ReadRGBAKey(ref Data.PostProcess.Diffuse  , CompressF16);
                IO.ReadRGBAKey(ref Data.PostProcess.Specular , CompressF16);
                IO.ReadKey    (ref Data.PostProcess.LensFlare, CompressF16);
                IO.ReadKey    (ref Data.PostProcess.LensGhost, CompressF16);
                IO.ReadKey    (ref Data.PostProcess.LensShaft, CompressF16);
            }
        }

        public void A3DCWriter(string file)
        {
            int i0 = 0;
            int i1 = 0;
            if (A3DCOpt) UsedValues = new Dictionary<int?, double?>();

            if (Data.Header.Format > Main.Format.FT)
            {
                int a = (int)((long)int.Parse(Data._.ConverterVersion) * BitConverter.
                    ToInt32(Data._.FileName.ToUTF8(), 0) * int.Parse(Data._.PropertyVersion));

                IO.Write(0x41443341);
                IO.Write(0x00);
                IO.Write(0x40);
                IO.Write(0x10000000);
                IO.Write((long)0x00);
                IO.Write((long)0x00);
                IO.Write(a);
                IO.Write(0x00);
                IO.Write((long)0x00);
                IO.Write(0x01131010);
                IO.Write(0x00);
                IO.Write(0x00);
                IO.Write(0x00);
            }
            else Data._.CompressF16 = null;

            int A3DCStart = IO.Position;

            IO.Close();
            IO = File.OpenWriter();

            for (byte i = 0; i < 2; i++)
            {
                bool ReturnToOffset = i == 1;
                IO.Position = 0;

                if (Data.CameraRoot != null)
                    for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                    {
                        IO.WriteOffset(ref Data.CameraRoot[i0].Interest    , ReturnToOffset);
                        IO.WriteOffset(ref Data.CameraRoot[i0].          MT, ReturnToOffset);
                        IO.WriteOffset(ref Data.CameraRoot[i0].ViewPoint.MT, ReturnToOffset);
                    }

                if (Data.DOF != null)
                    IO.WriteOffset(ref Data.DOF.MT, ReturnToOffset);

                if (Data.Light != null)
                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        IO.WriteOffset(ref Data.Light[i0].Position     , ReturnToOffset);
                        IO.WriteOffset(ref Data.Light[i0].SpotDirection, ReturnToOffset);
                    }

                if (Data.MObjectHRC != null)
                    for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        if (Data.MObjectHRC[i0].Instance != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                                IO.WriteOffset(ref Data.MObjectHRC[i0].Instance[i1].MT, ReturnToOffset);

                        IO.WriteOffset(ref Data.MObjectHRC[i0].MT, ReturnToOffset);

                        if (Data.MObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                IO.WriteOffset(ref Data.MObjectHRC[i0].Node[i1].MT, ReturnToOffset);
                    }

                if (Data.Object != null)
                    for (i0 = 0; i0 < Data.Object.Length; i0++)
                        IO.WriteOffset(ref Data.Object[i0].MT, ReturnToOffset);

                if (Data.ObjectHRC != null)
                    for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                        if (Data.ObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                                IO.WriteOffset(ref Data.ObjectHRC[i0].Node[i1].MT, ReturnToOffset);

                if (ReturnToOffset) continue;

                if (Data.Ambient != null)
                    for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                    {
                        Write(ref Data.Ambient[i0].   LightDiffuse);
                        Write(ref Data.Ambient[i0].RimLightDiffuse);
                    }

                if (Data.CameraAuxiliary != null)
                {
                    Write(ref Data.CameraAuxiliary.AutoExposure);
                    Write(ref Data.CameraAuxiliary.    Exposure);
                    Write(ref Data.CameraAuxiliary.Gamma       );
                    Write(ref Data.CameraAuxiliary.GammaRate   );
                    Write(ref Data.CameraAuxiliary.Saturate    );
                }

                if (Data.CameraRoot != null)
                    for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                    {
                        Write(ref Data.CameraRoot[i0].Interest    );
                        Write(ref Data.CameraRoot[i0]          .MT);
                        Write(ref Data.CameraRoot[i0].ViewPoint.MT);
                        Write(ref Data.CameraRoot[i0].ViewPoint.FOV        );
                        Write(ref Data.CameraRoot[i0].ViewPoint.FocalLength);
                        Write(ref Data.CameraRoot[i0].ViewPoint.Roll       );
                    }

                if (Data.Chara != null)
                    for (i0 = 0; i0 < Data.Chara.Length; i0++)
                        Write(ref Data.Chara[i0]);

                if (Data.Curve != null)
                    for (i0 = 0; i0 < Data.Curve.Length; i0++)
                        Write(ref Data.Curve[i0].CV);

                if (Data.DOF != null && Data.Header.Format == Main.Format.FT)
                    Write(ref Data.DOF.MT);

                if (Data.Fog != null)
                    for (i0 = 0; i0 < Data.Fog.Length; i0++)
                    {
                        Write(ref Data.Fog[i0].Density);
                        Write(ref Data.Fog[i0].Diffuse);
                        Write(ref Data.Fog[i0].End    );
                        Write(ref Data.Fog[i0].Start  );
                    }

                if (Data.Light != null)
                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        Write(ref Data.Light[i0].Ambient      );
                        Write(ref Data.Light[i0].Diffuse      );
                        Write(ref Data.Light[i0].Incandescence);
                        Write(ref Data.Light[i0].Specular     );
                        Write(ref Data.Light[i0].Position     );
                        Write(ref Data.Light[i0].SpotDirection);
                    }

                if (Data.MObjectHRC != null)
                    for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        if (Data.MObjectHRC[i0].Instance != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                                Write(ref Data.MObjectHRC[i0].Instance[i1].MT);

                        Write(ref Data.MObjectHRC[i0].MT);

                        if (Data.MObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                Write(ref Data.MObjectHRC[i0].Node[i1].MT);
                    }

                if (Data.MaterialList != null && IsX)
                    for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                    {
                        Write(ref Data.MaterialList[SO0[i0]].BlendColor   );
                        Write(ref Data.MaterialList[SO0[i0]].GlowIntensity);
                        Write(ref Data.MaterialList[SO0[i0]].Incandescence);
                    }

                if (Data.Object != null)
                    for (i0 = 0; i0 < Data.Object.Length; i0++)
                    {
                        Write(ref Data.Object[i0].MT);
                        if (Data.Object[i0].TT != null)
                            for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                            {
                                Write(ref Data.Object[i0].TT[i1].C );
                                Write(ref Data.Object[i0].TT[i1].O );
                                Write(ref Data.Object[i0].TT[i1].R );
                                Write(ref Data.Object[i0].TT[i1].Ro);
                                Write(ref Data.Object[i0].TT[i1].RF);
                                Write(ref Data.Object[i0].TT[i1].TF);
                            }
                    }

                if (Data.ObjectHRC != null)
                    for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                        if (Data.ObjectHRC[i0].Node != null)
                            for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                                Write(ref Data.ObjectHRC[i0].Node[i1].MT);

                if (Data.Point != null)
                    for (i0 = 0; i0 < Data.Point.Length; i0++)
                        Write(ref Data.Point[i0]);

                if (Data.PostProcess != null)
                {
                    Write(ref Data.PostProcess.Ambient  );
                    Write(ref Data.PostProcess.Diffuse  );
                    Write(ref Data.PostProcess.Specular );
                    Write(ref Data.PostProcess.LensFlare);
                    Write(ref Data.PostProcess.LensGhost);
                    Write(ref Data.PostProcess.LensShaft);
                }

                IO.Align(0x10, true);
            }

            byte[] A3DCData = IO.ToArray(true);

            IO = File.OpenWriter();
            A3DAWriter(true);
            byte[] A3DAData = IO.ToArray(true);

            Data.Head = new Header();
            IO = File.OpenWriter(file + ".a3da");
            IO.Position = A3DCStart + 0x40;

            Data.Head.StringOffset = IO.Position - A3DCStart;
            IO.Write(A3DAData);
            Data.Head.StringLength = IO.Position - A3DCStart - Data.Head.StringOffset;
            IO.Align(0x20, true);

            Data.Head.BinaryOffset = IO.Position - A3DCStart;
            IO.Write(A3DCData);
            Data.Head.BinaryLength = IO.Position - A3DCStart - Data.Head.BinaryOffset;
            IO.Align(0x10, true);

            int A3DCEnd = IO.Position;

            if (CompressF16 != 0)
            {
                IO.Align(0x10);
                A3DCEnd = IO.Position;
                IO.WriteEOFC(0);
                IO.Position = 0x04;
                IO.Write(A3DCEnd - A3DCStart);
                IO.Position = 0x14;
                IO.Write(A3DCEnd - A3DCStart);
            }

            IO.Position = A3DCStart;
            IO.Write("#A3D", "C__________");
            IO.Write(0x2000);
            IO.Write(0x00);
            IO.WriteEndian(0x20, true);
            IO.Write(0x10000200);
            IO.Write(0x50);
            IO.WriteEndian(Data.Head.StringOffset, true);
            IO.WriteEndian(Data.Head.StringLength, true);
            IO.WriteEndian(0x01, true);
            IO.Write(0x4C42);
            IO.WriteEndian(Data.Head.BinaryOffset, true);
            IO.WriteEndian(Data.Head.BinaryLength, true);
            IO.WriteEndian(0x20, true);
            if (Data.Header.Format != Main.Format.F)
            {
                IO.Position = A3DCEnd;
                IO.WriteEOFC(0);
            }

            IO.Close();
        }

        private void Write(ref ModelTransform MT)
        { Write(ref MT.Scale); Write(ref MT.Rot, true); Write(ref MT.Trans); Write(ref MT.Visibility); }

        private void Write(ref RGBAKey RGBA)
        { Write(ref RGBA.R); Write(ref RGBA.G); Write(ref RGBA.B); Write(ref RGBA.A); }

        private void Write(ref Vector3<Key> Key, bool F16 = false)
        { Write(ref Key.X, F16); Write(ref Key.Y, F16); Write(ref Key.Z, F16); }

        private void Write(ref KeyUV UV)
        { Write(ref UV.U); Write(ref UV.V); }

        private void Write(ref Key Key, bool F16 = false)
        {
            if (Key == null) return;

            int i = 0;
            if (Key.Trans != null)
            {
                Key.BinOffset = IO.Position;
                IO.Write(Key.Type);
                IO.Write(0x00);
                IO.Write((float)Key.Max);
                IO.Write(Key.Trans.Length);
                for (i = 0; i < Key.Trans.Length; i++)
                {
                    if (Key.Trans[i] is KeyFrameT0<double, double> TransT0)
                    {
                        if (F16 && CompressF16 > 0)
                        { IO.Write((ushort)TransT0.Frame); IO.Write((ushort)0); }
                        else
                        { IO.Write(( float)TransT0.Frame); IO.Write(( float)0); }
                        if (F16 && CompressF16 == 2) IO.Write(0 );
                        else                         IO.Write(0L);
                    }
                    else if (Key.Trans[i] is KeyFrameT1<double, double> TransT1)
                    {
                        if (F16 && CompressF16 > 0)
                        { IO.Write((ushort)TransT1.Frame); IO.Write(( Half)TransT1.Value); }
                        else
                        { IO.Write(( float)TransT1.Frame); IO.Write((float)TransT1.Value); }
                        if (F16 && CompressF16 == 2) IO.Write(0 );
                        else                         IO.Write(0L);
                    }
                    else if (Key.Trans[i] is KeyFrameT2<double, double> TransT2)
                    {
                        if (F16 && CompressF16 > 0)
                        { IO.Write((ushort)TransT2.Frame); IO.Write(( Half)TransT2.Value); }
                        else
                        { IO.Write(( float)TransT2.Frame); IO.Write((float)TransT2.Value); }
                        if (F16 && CompressF16 == 2)
                        { IO.Write((  Half)TransT2.Interpolation); IO.Write(( Half)TransT2.Interpolation); }
                        else
                        { IO.Write(( float)TransT2.Interpolation); IO.Write((float)TransT2.Interpolation); }
                    }
                    else if (Key.Trans[i] is KeyFrameT3<double, double> TransT3)
                    {
                        if (F16 && CompressF16 > 0)
                        { IO.Write((ushort)TransT3.Frame); IO.Write(( Half)TransT3.Value); }
                        else
                        { IO.Write(( float)TransT3.Frame); IO.Write((float)TransT3.Value); }
                        if (F16 && CompressF16 == 2)
                        { IO.Write((  Half)TransT3.Interpolation1); IO.Write(( Half)TransT3.Interpolation2); }
                        else
                        { IO.Write(( float)TransT3.Interpolation1); IO.Write((float)TransT3.Interpolation2); }
                    }
                }
            }
            else
            {
                if (!UsedValues.ContainsValue(Key.Value) || !A3DCOpt)
                {
                    Key.BinOffset = IO.Position;
                    IO.Write(        Key.Type );
                    IO.Write((float?)Key.Value);
                    if (A3DCOpt)
                    { UsedValues.Add(Key.BinOffset, Key.Value); }
                    return;
                }
                else if (UsedValues.ContainsValue(Key.Value))
                { Key.BinOffset = UsedValues.GetKey(Key.Value); return; }
            }
        }

        public void MsgPackReader(string file, bool JSON)
        {
            int i  = 0;
            int i0 = 0;
            int i1 = 0;

            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);

            if (!MsgPack.Element("A3D", out MsgPack A3D)) { MsgPack = MsgPack.New; return; }

            Data.Header = new PDHead();
            MsgPack Temp = MsgPack.New;

            if (A3D.Element("_", out Temp))
            {
                Data._ = new _
                {
                    CompressF16      = Temp.ReadNInt32("CompressF16"     ),
                    ConverterVersion = Temp.ReadString("ConverterVersion"),
                    FileName         = Temp.ReadString("FileName"        ),
                    PropertyVersion  = Temp.ReadString("PropertyVersion" ),
                };
            }

            if (A3D.Element<MsgPack>("Ambient", out Temp))
            {
                Data.Ambient = new Ambient[Temp.Array.Length];

                for (i = 0; i < Data.Ambient.Length; i++)
                    if (Temp[i] is MsgPack Ambient)
                        Data.Ambient[i] = new Ambient
                        {
                                       Name = Ambient.ReadString (           "Name"),
                               LightDiffuse = Ambient.ReadRGBAKey(   "LightDiffuse"),
                            RimLightDiffuse = Ambient.ReadRGBAKey("RimLightDiffuse"),
                        };
            }

            if (A3D.Element("CameraAuxiliary", out Temp))
                Data.CameraAuxiliary = new CameraAuxiliary
                {
                    AutoExposure = Temp.ReadKey("AutoExposure"),
                        Exposure = Temp.ReadKey(    "Exposure"),
                    Gamma        = Temp.ReadKey("Gamma"       ),
                    GammaRate    = Temp.ReadKey("GammaRate"   ),
                    Saturate     = Temp.ReadKey("Saturate"    ),
                };

            if (A3D.Element<MsgPack>("CameraRoot", out Temp))
            {
                Data.CameraRoot = new CameraRoot[Temp.Array.Length];
                for (i = 0; i < Data.CameraRoot.Length; i++)
                    if (Temp[i] is MsgPack _Root)
                    {
                        Data.CameraRoot[i] = new CameraRoot
                        {
                            MT       = _Root.ReadMT(),
                            Interest = _Root.ReadMT("Interest"),
                        };
                        if (!_Root.Element("ViewPoint", out MsgPack ViewPoint)) continue;

                        Data.CameraRoot[i].ViewPoint = new ViewPoint
                        {
                            MT              = ViewPoint.ReadMT(),
                            Aspect          = ViewPoint.ReadNDouble("Aspect"         ),
                            CameraApertureH = ViewPoint.ReadNDouble("CameraApertureH"),
                            CameraApertureW = ViewPoint.ReadNDouble("CameraApertureW"),
                            FOVHorizontal   = ViewPoint.ReadNDouble("FOVHorizontal"  ),
                            FocalLength     = ViewPoint.ReadKey    ("FocalLength"    ),
                            FOV             = ViewPoint.ReadKey    ("FOV"            ),
                            Roll            = ViewPoint.ReadKey    ("Roll"           ),
                        };
                }
            }

            if (A3D.Element<MsgPack>("Chara", out Temp))
            {
                Data.Chara = new ModelTransform[Temp.Array.Length];
                for (i = 0; i < Data.Chara.Length; i++)
                    if (Temp[i] is MsgPack Chara)
                        Data.Chara[i] = Chara.ReadMT();
            }

            if (A3D.Element<MsgPack>("Curve", out Temp))
            {
                Data.Curve = new Curve[Temp.Array.Length];
                for (i = 0; i < Data.Curve.Length; i++)
                    if (Temp[i] is MsgPack Curve)
                        Data.Curve[i] = new Curve
                        {
                            Name = Curve.ReadString("Name"),
                            CV   = Curve.ReadKey   ("CV"  ),
                        };
            }

            if (A3D.Element("DOF", out Temp))
                Data.DOF = new DOF
                {
                    MT   = Temp.ReadMT(),
                    Name = Temp.ReadString("Name"),
                };

            if (A3D.Element<MsgPack>("Event", out Temp))
            {
                Data.Event = new Event[Temp.Array.Length];
                for (i = 0; i < Data.Event.Length; i++)
                    if (Temp[i] is MsgPack Event)
                        Data.Event[i] = new Event
                        {
                                Begin    = Event.ReadNDouble(    "Begin"   ),
                            ClipBegin    = Event.ReadNDouble("ClipBegin"   ),
                            ClipEnd      = Event.ReadNDouble("ClipEnd"     ),
                                End      = Event.ReadNDouble(    "End"     ),
                            Name         = Event.ReadString ("Name"        ),
                            Param1       = Event.ReadString ("Param1"      ),
                            Ref          = Event.ReadString ("Ref"         ),
                            TimeRefScale = Event.ReadNDouble("TimeRefScale"),
                            Type         = Event.ReadNInt32 ("Type"        ),
                        };
            }

            if (A3D.Element<MsgPack>("Fog", out Temp))
            {
                Data.Fog = new Fog[Temp.Array.Length];
                for (i = 0; i < Data.Fog.Length; i++)
                    if (Temp[i] is MsgPack Fog)
                        Data.Fog[i] = new Fog
                        {
                            Id      = Fog.ReadNInt32 ("Id"     ),
                            Density = Fog.ReadKey    ("Density"),
                            Diffuse = Fog.ReadRGBAKey("Diffuse"),
                            End     = Fog.ReadKey    ("End"    ),
                            Start   = Fog.ReadKey    ("Start"  ),
                        };
            }

            if (A3D.Element<MsgPack>("Light", out Temp))
            {
                Data.Light = new Light[Temp.Array.Length];
                for (i = 0; i < Data.Light.Length; i++)
                    if (Temp[i] is MsgPack Light)
                        Data.Light[i] = new Light
                        {
                            Id            = Light.ReadNInt32 ("Id"           ),
                            Name          = Light.ReadString ("Name"         ),
                            Type          = Light.ReadString ("Type"         ),
                            Ambient       = Light.ReadRGBAKey("Ambient"      ),
                            Diffuse       = Light.ReadRGBAKey("Diffuse"      ),
                            Incandescence = Light.ReadRGBAKey("Incandescence"),
                            Position      = Light.ReadMT     ("Position"     ),
                            Specular      = Light.ReadRGBAKey("Specular"     ),
                            SpotDirection = Light.ReadMT     ("SpotDirection"),
                        };
            }

            if (A3D.Element<MsgPack>("MaterialList", out Temp))
            {
                Data.MaterialList = new MaterialList[Temp.Array.Length];
                for (i = 0; i < Data.MaterialList.Length; i++)
                    if (Temp[i] is MsgPack Material)
                        Data.MaterialList[i] = new MaterialList
                        {
                            HashName      = Material.ReadString (     "HashName"),
                                Name      = Material.ReadString (         "Name"),
                            BlendColor    = Material.ReadRGBAKey(   "BlendColor"),
                            GlowIntensity = Material.ReadKey    ("GlowIntensity"),
                            Incandescence = Material.ReadRGBAKey("Incandescence"),
                        };
            }

            if (A3D.Element<MsgPack>("MObjectHRC", out Temp))
            {
                Data.MObjectHRC = new MObjectHRC[Temp.Array.Length];
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    if (Temp[i0] is MsgPack MObjectHRC)
                    {
                        Data.MObjectHRC[i0] = new MObjectHRC
                        {
                            MT   = MObjectHRC.ReadMT(),
                            Name = MObjectHRC.ReadString("Name"),
                        };

                        if (MObjectHRC.Element("JointOrient", out MsgPack JointOrient))
                            Data.MObjectHRC[i0].JointOrient = new Vector3<double?>
                            {
                                X = JointOrient.ReadDouble("X"),
                                Y = JointOrient.ReadDouble("Y"),
                                Z = JointOrient.ReadDouble("Z"),
                            };

                        if (MObjectHRC.Element<MsgPack>("Instance", out MsgPack Instance))
                        {
                            Data.MObjectHRC[i0].Instance = new Instance[Instance.Array.Length];
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                                if (Instance[i1] is MsgPack _Instance)
                                    Data.MObjectHRC[i0].Instance[i1] = new Instance
                                    {
                                        MT = _Instance.ReadMT(),
                                           Name = _Instance.ReadString(   "Name"),
                                         Shadow = _Instance.ReadNInt32( "Shadow"),
                                        UIDName = _Instance.ReadString("UIDName"),
                                    };
                        }

                        if (MObjectHRC.Element<MsgPack>("Node", out MsgPack Node))
                        {
                            Data.MObjectHRC[i0].Node = new Node[Temp.Array.Length];
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                                if (Node[i1] is MsgPack _Node)
                                    Data.MObjectHRC[i0].Node[i1] = new Node
                                    {
                                        MT = _Node.ReadMT(),
                                          Name = _Node.ReadString(  "Name"),
                                        Parent = _Node.ReadNInt32("Parent"),
                                    };
                        }
                }
            }

            if (A3D.Element<MsgPack>("MObjectHRCList", out Temp))
            {
                Data.MObjectHRCList = new string[Temp.Array.Length];
                for (i = 0; i < Data.MObjectHRCList.Length; i++)
                    if (Temp[i] is MsgPack MObjectHRC)
                        Data.MObjectHRCList[i] = MObjectHRC.ReadString();
            }

            if (A3D.Element<MsgPack>("Motion", out Temp))
            {
                Data.Motion = new string[Temp.Array.Length];
                for (i = 0; i < Data.Motion.Length; i++)
                    if (Temp[i] is MsgPack Motion)
                        Data.Motion[i] = Motion.ReadString();
            }

            if (A3D.Element<MsgPack>("Object", out Temp))
            {
                Data.Object = new Object[Temp.Array.Length];
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                    if (Temp[i0] is MsgPack Object)
                    {
                        Data.Object[i0] = new Object
                        {
                                     MT = Object.ReadMT(),
                            Morph       = Object.ReadString("Morph"      ),
                            MorphOffset = Object.ReadNInt32("MorphOffset"),
                                   Name = Object.ReadString(       "Name"),
                             ParentName = Object.ReadString( "ParentName"),
                                UIDName = Object.ReadString(    "UIDName"),
                        };

                        if (Object.Element<MsgPack>("TP", out MsgPack TP))
                        {
                            Data.Object[i0].TP = new Object.TexturePattern[TP.Array.Length];
                            for (i1 = 0; i1 < Data.Object[i0].TP.Length; i1++)
                                if (TP[i1] is MsgPack _TP)
                                    Data.Object[i0].TP[i1] = new Object.TexturePattern
                                    {
                                        Name      = _TP.ReadString("Name"     ),
                                        Pat       = _TP.ReadString("Pat"      ),
                                        PatOffset = _TP.ReadNInt32("PatOffset"),
                                    };
                         }

                        if (Object.Element<MsgPack>("TT", out MsgPack TT))
                        {
                            Data.Object[i0].TT = new Object.TextureTransform[TT.Array.Length];
                            for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                                if (TT[i1] is MsgPack _TT)
                                Data.Object[i0].TT[i1] = new Object.TextureTransform
                                {
                                    Name = _TT.ReadString("Name"),
                                    C    = _TT.ReadKeyUV ("C"   ),
                                    O    = _TT.ReadKeyUV ("O"   ),
                                    R    = _TT.ReadKeyUV ("R"   ),
                                    Ro   = _TT.ReadKey   ("Ro"  ),
                                    RF   = _TT.ReadKey   ("RF"  ),
                                    TF   = _TT.ReadKeyUV ("TF"  ),
                                };
                        }
                    }
            }

            if (A3D.Element<MsgPack>("ObjectHRC", out Temp))
            {
                Data.ObjectHRC = new ObjectHRC[Temp.Array.Length];
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                    if (Temp[i0] is MsgPack ObjectHRC)
                    {
                        Data.ObjectHRC[i0] = new ObjectHRC
                        {
                               Name = ObjectHRC.ReadString (   "Name"),
                             Shadow = ObjectHRC.ReadNDouble( "Shadow"),
                            UIDName = ObjectHRC.ReadString ("UIDName"),
                        };

                        if (ObjectHRC.Element("JointOrient", out MsgPack JointOrient))
                            Data.ObjectHRC[i0].JointOrient = new Vector3<double?>
                            {
                                X = JointOrient.ReadDouble("X"),
                                Y = JointOrient.ReadDouble("Y"),
                                Z = JointOrient.ReadDouble("Z"),
                            };
    
                        if (ObjectHRC.Element<MsgPack>("Node", out MsgPack Node))
                        {
                            Data.ObjectHRC[i0].Node = new Node[Node.Array.Length];
                            for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                                if (Node[i1] is MsgPack _Node)
                                    Data.ObjectHRC[i0].Node[i1] = new Node
                                    {
                                            MT = _Node.ReadMT(),
                                          Name = _Node.ReadString(  "Name"),
                                        Parent = _Node.ReadInt32 ("Parent"),
                                    };
                    }
                }
            }

            if (A3D.Element<MsgPack>("ObjectHRCList", out Temp))
            {
                Data.ObjectHRCList = new string[Temp.Array.Length];
                for (i = 0; i < Data.ObjectHRCList.Length; i++)
                    if (Temp[i] is MsgPack ObjectHRC)
                        Data.ObjectHRCList[i] = ObjectHRC.ReadString();
            }

            if (A3D.Element<MsgPack>("ObjectList", out Temp))
            {
                Data.ObjectList = new string[Temp.Array.Length];
                for (i = 0; i < Data.ObjectList.Length; i++)
                    if (Temp[i] is MsgPack Object)
                        Data.ObjectList[i] = Object.ReadString();
            }

            if (A3D.Element("PlayControl", out Temp))
                Data.PlayControl = new PlayControl
                {
                    Begin  = Temp.ReadNDouble("Begin" ),
                    Div    = Temp.ReadNDouble("Div"   ),
                    FPS    = Temp.ReadNDouble("FPS"   ),
                    Offset = Temp.ReadNDouble("Offset"),
                    Size   = Temp.ReadNDouble("Size"  ),
                };

            if (A3D.Element<MsgPack>("Point", out Temp))
            {
                Data.Point = new ModelTransform[Temp.Array.Length];
                for (i = 0; i < Data.Point.Length; i++)
                    if (Temp[i] is MsgPack Point)
                        Data.Point[i] = Point.ReadMT();
            }

            if (A3D.Element("PostProcess", out Temp))
                Data.PostProcess = new PostProcess
                {
                    Ambient   = Temp.ReadRGBAKey("Ambient"  ),
                    Diffuse   = Temp.ReadRGBAKey("Diffuse"  ),
                    LensFlare = Temp.ReadKey    ("LensFlare"),
                    LensGhost = Temp.ReadKey    ("LensGhost"),
                    LensShaft = Temp.ReadKey    ("LensShaft"),
                    Specular  = Temp.ReadRGBAKey("Specular" ),
                };
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            int i  = 0;
            int i0 = 0;
            int i1 = 0;

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

            if (Data.CameraAuxiliary != null)
                A3D.Add(new MsgPack("CameraAuxiliary")
                    .Add("AutoExposure", Data.CameraAuxiliary.AutoExposure)
                    .Add(    "Exposure", Data.CameraAuxiliary.    Exposure)
                    .Add("Gamma"       , Data.CameraAuxiliary.Gamma       )
                    .Add("GammaRate"   , Data.CameraAuxiliary.GammaRate   )
                    .Add("Saturate"    , Data.CameraAuxiliary.Saturate    ));

            if (Data.CameraRoot != null)
            {
                MsgPack CameraRoot = new MsgPack(Data.CameraRoot.Length, "CameraRoot");
                for (i = 0; i < Data.CameraRoot.Length; i++)
                {
                    CameraRoot[i] = MsgPack.New
                        .Add(null, Data.CameraRoot[i].MT)
                        .Add("Interest"       , Data.CameraRoot[i].Interest    )
                        .Add(new MsgPack("ViewPoint")
                            .Add(null, Data.CameraRoot[i].ViewPoint.MT)
                            .Add("Aspect"         , Data.CameraRoot[i].ViewPoint.Aspect         )
                            .Add("CameraApertureH", Data.CameraRoot[i].ViewPoint.CameraApertureH)
                            .Add("CameraApertureW", Data.CameraRoot[i].ViewPoint.CameraApertureW)
                            .Add("FOVHorizontal"  , Data.CameraRoot[i].ViewPoint.FOVHorizontal  )
                            .Add("FocalLength"    , Data.CameraRoot[i].ViewPoint.FocalLength    )
                            .Add("FOV"            , Data.CameraRoot[i].ViewPoint.FOV            )
                            .Add("Roll"           , Data.CameraRoot[i].ViewPoint.Roll           ));
                }
                A3D.Add(CameraRoot);
            }

            if (Data.Chara != null)
            {
                MsgPack Chara = new MsgPack(Data.Chara.Length, "Chara");
                for (i = 0; i < Data.Curve.Length; i++) Chara[i] = MsgPack.New.Add(null, Data.Chara[i]);
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
                A3D.Add(new MsgPack("DOF").Add("Name", Data.DOF.Name).Add("MT", Data.DOF.MT));

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

                    if (IsX && Data.MObjectHRC[i0].JointOrient != null)
                        _MObjectHRC.Add(new MsgPack("JointOrient")
                            .Add("X", Data.MObjectHRC[i0].JointOrient.X)
                            .Add("Y", Data.MObjectHRC[i0].JointOrient.Y)
                            .Add("Z", Data.MObjectHRC[i0].JointOrient.Z));

                    if (Data.MObjectHRC[i0].Instance != null)
                    {
                        MsgPack Instance = new MsgPack(Data.MObjectHRC[i0].Instance.Length, "Instance");
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instance.Length; i1++)
                            Instance[i1] = MsgPack.New.Add(null, Data.MObjectHRC[i0].Instance[i1].MT)
                                .Add(   "Name", Data.MObjectHRC[i0].Instance[i1].   Name)
                                .Add("Shadow" , Data.MObjectHRC[i0].Instance[i1].Shadow )
                                .Add("UIDName", Data.MObjectHRC[i0].Instance[i1].UIDName);
                        _MObjectHRC.Add(Instance);
                    }

                    if (Data.MObjectHRC[i0].Node != null)
                    {
                        MsgPack Node = new MsgPack(Data.MObjectHRC[i0].Node.Length, "Node");
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            Node[i1] = MsgPack.New.Add(null, Data.MObjectHRC[i0].Node[i1].MT)
                                .Add("Name"  , Data.MObjectHRC[i0].Node[i1].Name  )
                                .Add("Parent", Data.MObjectHRC[i0].Node[i1].Parent);
                        _MObjectHRC.Add(Node);
                    }

                    MObjectHRC[i0] = _MObjectHRC.Add(null, Data.MObjectHRC[i0].MT);
                }
                A3D.Add(MObjectHRC);
            }

            if (Data.MObjectHRCList != null)
            {
                MsgPack MObjectHRCList = new MsgPack(Data.MObjectHRCList.Length, "MObjectHRCList");
                for (i = 0; i < Data.MObjectHRCList.Length; i++) MObjectHRCList[i] = Data.MObjectHRCList[i];
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
                    MsgPack _Object = MsgPack.New.Add(null, Data.Object[i0].MT)
                        .Add("Morph"      , Data.Object[i0].Morph      )
                        .Add("MorphOffset", Data.Object[i0].MorphOffset)
                        .Add(      "Name" , Data.Object[i0].      Name )
                        .Add("ParentName" , Data.Object[i0].ParentName )
                        .Add(   "UIDName" , Data.Object[i0].   UIDName );
                    if (Data.Object[i0].TP != null)
                    {
                        MsgPack TP = new MsgPack(Data.Object[i0].TP.Length, "TP");
                        for (i1 = 0; i1 < Data.Object[i0].TP.Length; i1++)
                            TP[i1] = MsgPack.New.Add("Name"     , Data.Object[i0].TP[i1].Name     )
                                                .Add("Pat"      , Data.Object[i0].TP[i1].Pat      )
                                                .Add("PatOffset", Data.Object[i0].TP[i1].PatOffset);
                        _Object.Add(TP);
                    }
                    if (Data.Object[i0].TT != null)
                    {
                        MsgPack TT = new MsgPack(Data.Object[i0].TT.Length, "TT");
                        for (i1 = 0; i1 < Data.Object[i0].TT.Length; i1++)
                            TT[i1] = MsgPack.New.Add("Name", Data.Object[i0].TT[i1].Name)
                                                .Add("C"   , Data.Object[i0].TT[i1].C   )
                                                .Add("O"   , Data.Object[i0].TT[i1].O   )
                                                .Add("R"   , Data.Object[i0].TT[i1].R   )
                                                .Add("Ro"  , Data.Object[i0].TT[i1].Ro  )
                                                .Add("RF"  , Data.Object[i0].TT[i1].RF  )
                                                .Add("TF"  , Data.Object[i0].TT[i1].TF  );
                        _Object.Add(TT);
                    }
                    Object[i0] = _Object;
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
                    
                    if (IsX && Data.ObjectHRC[i0].JointOrient != null)
                        _ObjectHRC.Add(new MsgPack("JointOrient")
                            .Add("X", Data.ObjectHRC[i0].JointOrient.X)
                            .Add("Y", Data.ObjectHRC[i0].JointOrient.Y)
                            .Add("Z", Data.ObjectHRC[i0].JointOrient.Z));

                    if (Data.ObjectHRC[i0].Node != null)
                    {
                        MsgPack Node = new MsgPack(Data.ObjectHRC[i0].Node.Length, "Node");
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            Node[i1] = MsgPack.New.Add(null, Data.ObjectHRC[i0].Node[i1].MT)
                                .Add("Name"  , Data.ObjectHRC[i0].Node[i1].Name  )
                                .Add("Parent", Data.ObjectHRC[i0].Node[i1].Parent);
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

            if (Data.PlayControl != null)
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
                    Point[i] = MsgPack.New.Add(null, Data.Point[i]);
                A3D.Add(Point);
            }

            if (Data.PostProcess != null)
                A3D.Add(new MsgPack("PostProcess").Add("Ambient"  , Data.PostProcess.Ambient  )
                                                  .Add("Diffuse"  , Data.PostProcess.Diffuse  )
                                                  .Add("LensFlare", Data.PostProcess.LensFlare)
                                                  .Add("LensGhost", Data.PostProcess.LensGhost)
                                                  .Add("LensShaft", Data.PostProcess.LensShaft)
                                                  .Add("Specular" , Data.PostProcess.Specular ));
            
            A3D.Write(true, file, JSON);
        }
    }

    public struct _
    {
        public int? CompressF16;
        public string FileName;
        public string PropertyVersion;
        public string ConverterVersion;
    }

    public struct A3DAData
    {
        public string[] Motion;
        public string[] ObjectList;
        public string[] ObjectHRCList;
        public string[] MObjectHRCList;
        public _ _;
        public DOF DOF;
        public Fog[] Fog;
        public Curve[] Curve;
        public Event[] Event;
        public Light[] Light;
        public Header Head;
        public PDHead Header;
        public Object[] Object;
        public Ambient[] Ambient;
        public ObjectHRC[] ObjectHRC;
        public CameraRoot[] CameraRoot;
        public MObjectHRC[] MObjectHRC;
        public PlayControl PlayControl;
        public PostProcess PostProcess;
        public MaterialList[] MaterialList;
        public ModelTransform[] Chara;
        public ModelTransform[] Point;
        public CameraAuxiliary CameraAuxiliary;
    }

    public struct Ambient
    {
        public string  Name;
        public RGBAKey    LightDiffuse;
        public RGBAKey RimLightDiffuse;
    }

    public class CameraAuxiliary
    {
        public Key Gamma;
        public Key Exposure;
        public Key Saturate;
        public Key GammaRate;
        public Key AutoExposure;
    }

    public struct CameraRoot
    {
        public ViewPoint ViewPoint;
        public ModelTransform MT;
        public ModelTransform Interest;
    }

    public struct Curve
    {
        public string Name;
        public Key CV;
    }

    public class DOF
    {
        public string Name;
        public ModelTransform MT;
    }

    public struct Event
    {
        public int? Type;
        public double? End;
        public double? Begin;
        public double? ClipEnd;
        public double? ClipBegin;
        public double? TimeRefScale;
        public string Name;
        public string Param1;
        public string Ref;
    }

    public struct Fog
    {
        public int? Id;
        public Key End;
        public Key Start;
        public Key Density;
        public RGBAKey Diffuse;
    }

    public struct Header
    {
        public int Count;
        public int BinaryLength;
        public int BinaryOffset;
        public int HeaderOffset;
        public int StringLength;
        public int StringOffset;
    }

    public class Key
    {
        public int? Type;
        public int? Length;
        public int? BinOffset;
        public double? Max;
        public double? Value;
        public double? EPTypePost;
        public double? EPTypePre;
        public RawD RawData;
        public KeyFrame<double, double>[] Trans;

        public class RawD
        {
            public int? KeyType;
            public int? ValueListSize;
            public string ValueType;
            public string[] ValueList;
        }
    }

    public struct Instance
    {
        public int? Shadow;
        public string Name;
        public string UIDName;
        public ModelTransform MT;
    }

    public struct KeyUV
    {
        public Key U;
        public Key V;
    }

    public struct Light
    {
        public int? Id;
        public string Name;
        public string Type;
        public RGBAKey Ambient;
        public RGBAKey Diffuse;
        public RGBAKey Specular;
        public RGBAKey Incandescence;
        public ModelTransform Position;
        public ModelTransform SpotDirection;
    }

    public struct MaterialList
    {
        public string Name;
        public string HashName;
        public Key GlowIntensity;
        public RGBAKey BlendColor;
        public RGBAKey Incandescence;
    }

    public struct MObjectHRC
    {
        public string Name;
        public Node[] Node;
        public Vector3<double?> JointOrient;
        public Instance[] Instance;
        public ModelTransform MT;
    }

    public struct ModelTransform
    {
        public bool Writed;
        public int? BinOffset;
        public Key Visibility;
        public Vector3<Key> Rot;
        public Vector3<Key> Scale;
        public Vector3<Key> Trans;
    }

    public struct Node
    {
        public int? Parent;
        public string Name;
        public ModelTransform MT;
    }

    public struct Object
    {
        public int? MorphOffset;
        public string Name;
        public string Morph;
        public string UIDName;
        public string ParentName;
        public ModelTransform MT;
        public TexturePattern[] TP; //TexPat
        public TextureTransform[] TT; //TexTrans

        public struct TexturePattern
        {
            public int? PatOffset;
            public string Pat;
            public string Name;
        }

        public struct TextureTransform
        {
            public string Name;
            public KeyUV C;  //Coverage
            public KeyUV O;  //Offset
            public KeyUV R;  //Repeat
            public Key Ro; //Rotate
            public Key RF; //RotateFrame
            public KeyUV TF; //TranslateFrameU
        }
    }

    public struct ObjectHRC
    {
        public double? Shadow;
        public string Name;
        public string UIDName;
        public Node[] Node;
        public Vector3<double?> JointOrient;
    }

    public class PlayControl
    {
        public double? Begin;
        public double? Div;
        public double? FPS;
        public double? Offset;
        public double? Size;
    }

    public class PostProcess
    {
        public Key LensFlare;
        public Key LensGhost;
        public Key LensShaft;
        public RGBAKey Ambient;
        public RGBAKey Diffuse;
        public RGBAKey Specular;
    }

    public struct RGBAKey
    {
        public Key R;
        public Key G;
        public Key B;
        public Key A;
    }

    public class Vector3<T>
    {
        public T X;
        public T Y;
        public T Z;
    }

    public struct ViewPoint
    {
        public double? Aspect;
        public double? FOVHorizontal;
        public double? CameraApertureH;
        public double? CameraApertureW;
        public Key FOV;
        public Key Roll;
        public Key FocalLength;
        public ModelTransform MT;
    }
}
