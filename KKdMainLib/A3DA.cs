using System;
using System.Collections.Generic;
using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;
using Extensions = KKdBaseLib.Extensions;

namespace KKdMainLib.A3DA
{
    public class A3DA
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
        private Dictionary<int?, double?> UsedValues;
        private Dictionary<string, object> Dict;

        private bool IsX => Data.Format == Format.X || Data.Format == Format.XHD;

        public Stream IO;
        public A3DAData Data;

        public A3DA()
        { Data = new A3DAData(); Dict = new Dictionary<string, object>();
            IO = File.OpenWriter(); UsedValues = new Dictionary<int?, double?>(); }

        public int A3DAReader(string file)
        { IO = File.OpenReader(file + ".a3da"); return A3DAReader(ref IO); }

        public int A3DAReader(byte[] data)
        { IO = File.OpenReader(data          ); return A3DAReader(ref IO); }

        private int A3DAReader(ref Stream IO)
        {
            name = "";
            nameView = "";
            dataArray = new string[4];
            Dict = new Dictionary<string, object>();
            Data = new A3DAData();
            Header Header = new Header();

            Data.Format = IO.Format = Format.F;
            Header.SectionSignature = IO.ReadInt32();
            if (Header.SectionSignature == 0x41443341)
            { Header = IO.ReadHeader(true, true); Data.Format = Header.Format; }
            if (Header.SectionSignature != 0x44334123) { IO.Close(); return 0; }

            IO.Offset = IO.Position - 4;
            Header.SectionSignature = IO.ReadInt32();

            if (Header.SectionSignature == 0x5F5F5F41)
            {
                IO.Position = 0x10;
                Header.Format = IO.Format = Format.DT;
            }
            else if (Header.SectionSignature == 0x5F5F5F43)
            {
                IO.Position = 0x10;
                IO.ReadInt32();
                IO.ReadInt32();
                Data.HeaderOffset = IO.ReadInt32Endian(true);

                IO.Position = Data.HeaderOffset;
                if (IO.ReadInt32() != 0x50) { IO.Close(); return 0; }
                Data.StringOffset = IO.ReadInt32Endian(true);
                Data.StringLength = IO.ReadInt32Endian(true);
                Data.Count = IO.ReadInt32Endian(true);
                if (IO.ReadInt32() != 0x4C42) { IO.Close(); return 0; }
                Data.BinaryOffset = IO.ReadInt32Endian(true);
                Data.BinaryLength = IO.ReadInt32Endian(true);

                IO.Position = Data.StringOffset;
            }
            else { IO.Close(); return 0; }

            if (Header.Format == Format.DT)
                Data.StringLength = IO.Length - 0x10;

            string[] STRData = IO.ReadString(Data.StringLength).Replace("\r", "").Split('\n');
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
                IO.Position = IO.Offset + Data.BinaryOffset;
                IO.Offset = IO.Position;
                IO.Position = 0;
                IO = File.OpenReader(IO.ReadBytes(Data.BinaryLength));
                A3DCReader();
            }
            IO.Close();

            name = "";
            nameView = "";
            dataArray = null;
            Dict = null;
            IO = null;
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
                Data.Format = Format.FT;
                Data.DOF.MT = Dict.ReadMT("dof" + d);
            }

            if (Dict.FindValue(out value, "ambient.length"))
            {
                Data.Ambient = new Ambient[int.Parse(value)];
                Data.Format = Format.MGF;
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

                    Dict.FindValue(out Data.CameraRoot[i0].VP.
                        Aspect         , nameView + "aspect"           );
                    Dict.FindValue(out Data.CameraRoot[i0].VP.
                        CameraApertureH, nameView + "camera_aperture_h");
                    Dict.FindValue(out Data.CameraRoot[i0].VP.
                        CameraApertureW, nameView + "camera_aperture_w");
                    Dict.FindValue(out i1, nameView + "fov_is_horizontal");
                    Data.CameraRoot[i0].VP.FOVHorizontal = i1 != 0;

                    Data.CameraRoot[i0].      MT = Dict.ReadMT(name);
                    Data.CameraRoot[i0].Interest = Dict.ReadMT(name + "interest" + d);
                    Data.CameraRoot[i0].VP.   MT = Dict.ReadMT(nameView);
                    Data.CameraRoot[i0].VP.FocalLength = Dict.ReadKey(nameView + "focal_length" + d);
                    Data.CameraRoot[i0].VP.FOV         = Dict.ReadKey(nameView +          "fov" + d);
                    Data.CameraRoot[i0].VP.Roll        = Dict.ReadKey(nameView +         "roll" + d);
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
                        Data.MObjectHRC[i0].Instances = new MObjectHRC.Instance[int.Parse(value)];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                        {
                            nameView = name + "instance" + d + i1 + d;

                            Dict.FindValue(out Data.MObjectHRC[i0].Instances[i1].   Name, nameView +     "name");
                            Dict.FindValue(out Data.MObjectHRC[i0].Instances[i1]. Shadow, nameView +   "shadow");
                            Dict.FindValue(out Data.MObjectHRC[i0].Instances[i1].UIDName, nameView + "uid_name");

                            Data.MObjectHRC[i0].Instances[i1].MT = Dict.ReadMT(nameView);
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
                Data.Format = Format.X;
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
                                Dict.ReadKeyUV(nameView + "coverage"      );
                            Data.Object[i0].TexTrans[i1].Offset         =
                                Dict.ReadKeyUV(nameView + "offset"        );
                            Data.Object[i0].TexTrans[i1].Repeat         =
                                Dict.ReadKeyUV(nameView + "repeat"        );
                            Data.Object[i0].TexTrans[i1].   Rotate      =
                                Dict.ReadKey  (nameView + "rotate"     + d);
                            Data.Object[i0].TexTrans[i1].   RotateFrame =
                                Dict.ReadKey  (nameView + "rotateFrame"+ d);
                            Data.Object[i0].TexTrans[i1].TranslateFrame =
                                Dict.ReadKeyUV(nameView + "translateFrame");
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

        public byte[] A3DAWriter(bool A3DC = false)
        {
            IO = File.OpenWriter();
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
            IO.Write("_.file_name="        , Data._.FileName        );
            IO.Write("_.property.version=" , Data._. PropertyVersion);

            if (Data.Ambient != null && Data.Format == Format.MGF)
            {
                SO0 = Data.Ambient.Length.SortWriter();
                for (i0 = 0; i0 < Data.Ambient.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "ambient" + d + SOi0 + d;
                    ref Ambient Ambient = ref Data.Ambient[SOi0];

                    IO.Write(Ambient.   LightDiffuse, name +    "light.Diffuse", A3DC);
                    IO.Write(name + "name=", Ambient.Name);
                    IO.Write(Ambient.RimLightDiffuse, name + "rimlight.Diffuse", A3DC);
                }
                IO.Write("ambient.length=", Data.Fog.Length);
            }

            if (Data.CameraAuxiliary != null)
            {
                name = "camera_auxiliary" + d;
                ref CameraAuxiliary CA = ref Data.CameraAuxiliary;

                IO.Write(CA.AutoExposure, name + "auto_exposure", true, A3DC);
                IO.Write(CA.    Exposure, name +      "exposure", true, A3DC);
                IO.Write(CA.Gamma       , name + "gamma"        , true, A3DC);
                IO.Write(CA.GammaRate   , name + "gamma_rate"   , true, A3DC);
                IO.Write(CA.Saturate    , name + "saturate"     , true, A3DC);
            }

            if (Data.CameraRoot != null)
            {
                SO0 = Data.CameraRoot.Length.SortWriter();
                for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "camera_root" + d + SOi0 + d;
                    nameView = name + "view_point" + d;
                    ref CameraRoot CR = ref Data.CameraRoot[SOi0];

                    IO.Write(CR.Interest, name + "interest" + d, A3DC, IsX);
                    IO.Write(CR.MT, name, A3DC, IsX, 0b11110);
                    IO.Write(nameView + "aspect=", CR.VP.Aspect);
                    if (CR.VP.CameraApertureH != null)
                        IO.Write(nameView + "camera_aperture_h=", CR.VP.CameraApertureH);
                    if (CR.VP.CameraApertureW != null)
                        IO.Write(nameView + "camera_aperture_w=", CR.VP.CameraApertureW);
                    IO.Write(CR.VP.FocalLength, nameView + "focal_length" + d, A3DC);
                    IO.Write(CR.VP.FOV, nameView + "fov" + d, A3DC);
                    if (CR.VP.FOVHorizontal != null)
                        IO.Write(nameView + "fov_is_horizontal=", CR.VP.FOVHorizontal.Value ? 1 : 0);
                    IO.Write(CR.VP.MT  , nameView, A3DC, IsX, 0b10000);
                    IO.Write(CR.VP.Roll, nameView + "roll" + d, A3DC);
                    IO.Write(CR.VP.MT  , nameView, A3DC, IsX, 0b01111);
                    IO.Write(CR   .MT  , name    , A3DC, IsX, 0b00001);
                }
                IO.Write("camera_root.length=", Data.CameraRoot.Length);
            }

            if (Data.Chara != null)
            {
                SO0 = Data.Chara.Length.SortWriter();
                for (i0 = 0; i0 < Data.Chara.Length; i0++)
                    IO.Write(Data.Chara[SO0[i0]], "chara" + d + SO0[i0] + d, A3DC, IsX);
                IO.Write("chara.length=", Data.Chara.Length);
            }

            if (Data.Curve != null)
            {
                SO0 = Data.Curve.Length.SortWriter();
                for (i0 = 0; i0 < Data.Curve.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "curve" + d + SOi0 + d;
                    ref Curve Curve = ref Data.Curve[SOi0];

                    IO.Write(Curve.CV, name + "cv" + d, A3DC);
                    IO.Write(name + "name=", Curve.Name);
                }
                IO.Write("curve.length=", Data.Curve.Length);
            }

            if (Data.DOF != null && Data.Format == Format.FT)
            {
                IO.Write("dof.name=", Data.DOF.Name);
                IO.Write(Data.DOF.MT, "dof" + d, A3DC, IsX);
            }

            if (Data.Event != null)
            {
                SO0 = Data.Event.Length.SortWriter();
                for (i0 = 0; i0 < Data.Event.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "event" + d + SOi0 + d;
                    ref Event Event = ref Data.Event[SOi0];

                    IO.Write(name + "begin="         , Event.Begin       );
                    IO.Write(name + "clip_begin="    , Event.ClipBegin   );
                    IO.Write(name + "clip_en="       , Event.ClipEnd     );
                    IO.Write(name + "end="           , Event.End         );
                    IO.Write(name + "name="          , Event.Name        );
                    IO.Write(name + "param1="        , Event.Param1      );
                    IO.Write(name + "ref="           , Event.Ref         );
                    IO.Write(name + "time_ref_scale=", Event.TimeRefScale);
                    IO.Write(name + "type="          , Event.Type        );
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

                    IO.Write(Fog.Diffuse, name + "Diffuse",       A3DC);
                    IO.Write(Fog.Density, name + "density", true, A3DC);
                    IO.Write(Fog.End    , name + "end"    , true, A3DC);
                    IO.Write(name + "id=", Fog.Id);
                    IO.Write(Fog.Start  , name + "start"  , true, A3DC);
                }
                IO.Write("fog.length=", Data.Fog.Length);
            }

            if (Data.Light != null)
            {
                SO0 = Data.Light.Length.SortWriter();
                for (i0 = 0; i0 < Data.Light.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "light" + d + SOi0 + d;
                    ref Light Light = ref Data.Light[SOi0];

                    IO.Write(Light.Ambient      , name + "Ambient"      , A3DC);
                    IO.Write(Light.Diffuse      , name + "Diffuse"      , A3DC);
                    IO.Write(Light.Incandescence, name + "Incandescence", A3DC);
                    IO.Write(Light.Specular     , name + "Specular"     , A3DC);
                    IO.Write(name + "id="  , Light.Id  );
                    IO.Write(name + "name=", Light.Name);
                    IO.Write(Light.Position     , name + "position"       + d, A3DC, IsX);
                    IO.Write(Light.SpotDirection, name + "spot_direction" + d, A3DC, IsX);
                    IO.Write(name + "type=", Light.Type);
                }
                IO.Write("light.length=", Data.Light.Length);
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
                        IO.Write(name + "joint_orient.x=", MObjectHRC.JointOrient.X);
                        IO.Write(name + "joint_orient.y=", MObjectHRC.JointOrient.Y);
                        IO.Write(name + "joint_orient.z=", MObjectHRC.JointOrient.Z);
                    }

                    if (MObjectHRC.Instances != null)
                    {
                        SO1 = MObjectHRC.Instances.Length.SortWriter();
                        for (i1 = 0; i1 < MObjectHRC.Instances.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            nameView = name + "instance" + d + SOi1 + d;
                            ref MObjectHRC.Instance Instance = ref MObjectHRC.Instances[SOi1];

                            IO.Write(Instance.MT, nameView, A3DC, IsX, 0b10000);
                            IO.Write(nameView +     "name=", Instance.   Name);
                            IO.Write(Instance.MT, nameView, A3DC, IsX, 0b01100);
                            IO.Write(nameView +   "shadow=", Instance. Shadow);
                            IO.Write(Instance.MT, nameView, A3DC, IsX, 0b00010);
                            IO.Write(nameView + "uid_name=", Instance.UIDName);
                            IO.Write(Instance.MT, nameView, A3DC, IsX, 0b00001);
                        }
                        IO.Write(name + "instance.length=", MObjectHRC.Instances.Length);
                    }

                    IO.Write(MObjectHRC.MT, name, A3DC, IsX, 0b10000);
                    IO.Write(name + "name=", MObjectHRC.Name);

                    if (MObjectHRC.Node != null)
                    {
                        SO1 = MObjectHRC.Node.Length.SortWriter();
                        for (i1 = 0; i1 < MObjectHRC.Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            nameView = name + "node" + d + SOi1 + d;
                            ref Node Node = ref MObjectHRC.Node[SOi1];

                            IO.Write(Node.MT, nameView, A3DC, IsX, 0b10000);
                            IO.Write(nameView +   "name=", Node.Name  );
                            IO.Write(nameView + "parent=", Node.Parent);
                            IO.Write(Node.MT, nameView, A3DC, IsX, 0b01111);
                        }
                        IO.Write(name + "node.length=", MObjectHRC.Node.Length);
                    }

                    IO.Write(MObjectHRC.MT, name, A3DC, IsX, 0b01111);
                }
                IO.Write("m_objhrc.length=", Data.MObjectHRC.Length);
            }

            if (Data.MObjectHRCList != null)
            {
                SO0 = Data.MObjectHRCList.Length.SortWriter();
                for (i0 = 0; i0 < Data.MObjectHRCList.Length; i0++)
                    IO.Write("m_objhrc_list" + d + SO0[i0] + "=", Data.MObjectHRCList[SO0[i0]]);
                IO.Write("m_objhrc_list.length=", Data.MObjectHRCList.Length);
            }

            if (Data.MaterialList != null && IsX)
            {
                SO0 = Data.MaterialList.Length.SortWriter();
                for (i0 = 0; i0 < Data.MaterialList.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "material_list" + d + SOi0 + d;
                    ref MaterialList ML = ref Data.MaterialList[SOi0];

                    IO.Write(ML.BlendColor   , name + "blend_color"    + d, A3DC);
                    IO.Write(ML.GlowIntensity, name + "glow_intensity" + d, A3DC);
                    IO.Write(name + "hash_name=", ML.HashName);
                    IO.Write(ML.Incandescence, name + "incandescence"  + d, A3DC);
                    IO.Write(name +      "name=", ML.    Name);
                }
                IO.Write("material_list.length=", Data.MaterialList.Length);
            }

            if (Data.Motion != null)
            {
                SO0 = Data.Motion.Length.SortWriter();
                for (i0 = 0; i0 < Data.Motion.Length; i0++)
                    IO.Write(name + SO0[i0] + d + "name=", Data.Motion[SO0[i0]]);
                IO.Write("motion.length=", Data.Motion.Length);
            }

            if (Data.Object != null)
            {
                SO0 = Data.Object.Length.SortWriter();
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "object" + d + SOi0 + d;
                    ref Object Object = ref Data.Object[SOi0];

                    IO.Write(Object.MT, name, A3DC, IsX, 0b10000);
                    if (Object.Morph != null)
                    {
                        IO.Write(name + "morph="       , Object.Morph      );
                        IO.Write(name + "morph_offset=", Object.MorphOffset);
                    }
                    IO.Write(name + "name="       , Object.Name      );
                    IO.Write(name + "parent_name=", Object.ParentName);
                    IO.Write(Object.MT, name, A3DC, IsX, 0b01100);

                    if (Object.TexPat != null)
                    {
                        SO1 = Object.TexPat.Length.SortWriter();
                        for (i1 = 0; i1 < Object.TexPat.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            nameView = name + "tex_pat" + d + SOi1 + d;
                            ref Object.TexturePattern TexPat = ref Object.TexPat[SOi1];

                            IO.Write(nameView + "name="      , TexPat.Name     );
                            IO.Write(nameView + "pat="       , TexPat.Pat      );
                            IO.Write(nameView + "pat_offset=", TexPat.PatOffset);
                        }
                        IO.Write(nameView + "length=", Object.TexPat.Length);
                    }

                    if (Object.TexTrans != null)
                    {
                        SO1 = Object.TexTrans.Length.SortWriter();
                        for (i1 = 0; i1 < Object.TexTrans.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            nameView = name + "tex_transform" + d + SOi1 + d;
                            ref Object.TextureTransform TexTrans = ref Object.TexTrans[SOi1];

                            IO.Write(nameView + "name=", Object.TexTrans[SOi1].Name);
                            IO.Write(TexTrans.Coverage      , nameView + "coverage"      , A3DC);
                            IO.Write(TexTrans.Offset        , nameView + "offset"        , A3DC);
                            IO.Write(TexTrans.Repeat        , nameView + "repeat"        , A3DC);
                            IO.Write(TexTrans.   Rotate     , nameView + "rotate"        , A3DC);
                            IO.Write(TexTrans.   RotateFrame, nameView + "rotateFrame"   , A3DC);
                            IO.Write(TexTrans.TranslateFrame, nameView + "translateFrame", A3DC);
                        }
                        IO.Write(name + "tex_transform.length=", + Object.TexTrans.Length);
                    }

                    IO.Write(Object.MT, name, A3DC, IsX, 0b00010);
                    IO.Write(name + "uid_name=", Object.UIDName);
                    IO.Write(Object.MT, name, A3DC, IsX, 0b00001);
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
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    SOi0 = SO0[i0];
                    name = "objhrc" + d + SOi0 + d;
                    ref ObjectHRC ObjectHRC = ref Data.ObjectHRC[SOi0];

                    IO.Write(name + "name=", ObjectHRC.Name);

                    if (IsX && ObjectHRC.JointOrient.NotNull)
                    {
                        IO.Write(name + "joint_orient.x=", ObjectHRC.JointOrient.X);
                        IO.Write(name + "joint_orient.y=", ObjectHRC.JointOrient.Y);
                        IO.Write(name + "joint_orient.z=", ObjectHRC.JointOrient.Z);
                    }

                    if (ObjectHRC.Node != null)
                    {
                        SO1 = ObjectHRC.Node.Length.SortWriter();
                        for (i1 = 0; i1 < ObjectHRC.Node.Length; i1++)
                        {
                            SOi1 = SO1[i1];
                            nameView = name + "node" + d + SOi1 + d;
                            ref Node Node = ref ObjectHRC.Node[SOi1];

                            IO.Write(Node.MT, nameView, A3DC, IsX, 0b10000);
                            IO.Write(nameView + "name="  , Node.Name  );
                            IO.Write(nameView + "parent=", Node.Parent);
                            IO.Write(Node.MT, nameView, A3DC, IsX, 0b01111);
                        }
                        IO.Write(nameView + "length=", ObjectHRC.Node.Length);
                    }

                    if (ObjectHRC.Shadow != null)
                        IO.Write(name + "shadow=", ObjectHRC.Shadow);
                    IO.Write(name + "uid_name=", ObjectHRC.UIDName);
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

            IO.Write("play_control.begin=", Data.PlayControl.Begin);
            if (Data.PlayControl.Div    != null && A3DC)
                IO.Write("play_control.div=", Data.PlayControl.Div);
            IO.Write("play_control.fps=", Data.PlayControl.FPS);
            if (Data.PlayControl.Offset != null)
            { if (A3DC) { IO.Write("play_control.offset=", Data.PlayControl.Offset);
                          IO.Write("play_control.size="  , Data.PlayControl.Size  ); }
              else IO.Write("play_control.size=", Data.PlayControl.Size + Data.PlayControl.Offset);
            }
            else   IO.Write("play_control.size=", Data.PlayControl.Size);

            if (Data.PostProcess != null)
            {
                ref PostProcess PP = ref Data.PostProcess;
                name = "post_process" + d;
                IO.Write(PP.Ambient  , name + "Ambient"   ,       A3DC);
                IO.Write(PP.Diffuse  , name + "Diffuse"   ,       A3DC);
                IO.Write(PP.Specular , name + "Specular"  ,       A3DC);
                IO.Write(PP.LensFlare, name + "lens_flare", true, A3DC);
                IO.Write(PP.LensGhost, name + "lens_ghost", true, A3DC);
                IO.Write(PP.LensShaft, name + "lens_shaft", true, A3DC);
            }

            if (Data.Point != null)
            {
                SO0 = Data.Point.Length.SortWriter();
                for (i0 = 0; i0 < Data.Point.Length; i0++)
                    IO.Write(Data.Point[SO0[i0]], "point" + d + SO0[i0] + d, A3DC, IsX);
                IO.Write("point.length=", Data.Point.Length);
            }

            IO.Align(0x1, true);
            return IO.ToArray(true);
        }

        private int CompressF16 => Data._.CompressF16.GetValueOrDefault();

        private void A3DCReader()
        {
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
                    IO.ReadMT(ref Data.CameraRoot[i0].      MT, CompressF16);
                    IO.ReadMT(ref Data.CameraRoot[i0].Interest, CompressF16);
                    IO.ReadMT(ref Data.CameraRoot[i0].VP.   MT, CompressF16);
                    IO.ReadKey(ref Data.CameraRoot[i0].VP.FocalLength, CompressF16);
                    IO.ReadKey(ref Data.CameraRoot[i0].VP.FOV        , CompressF16);
                    IO.ReadKey(ref Data.CameraRoot[i0].VP.Roll       , CompressF16);
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

                    if (Data.MObjectHRC[i0].Instances != null)
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                            IO.ReadMT(ref Data.MObjectHRC[i0].Instances[i1].MT, CompressF16);

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
                    if (Data.Object[i0].TexTrans != null)
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                        {
                            IO.ReadKeyUV(ref Data.Object[i0].TexTrans[i1].Coverage      , CompressF16);
                            IO.ReadKeyUV(ref Data.Object[i0].TexTrans[i1].Offset        , CompressF16);
                            IO.ReadKeyUV(ref Data.Object[i0].TexTrans[i1].Repeat        , CompressF16);
                            IO.ReadKey  (ref Data.Object[i0].TexTrans[i1].   Rotate     , CompressF16);
                            IO.ReadKey  (ref Data.Object[i0].TexTrans[i1].   RotateFrame, CompressF16);
                            IO.ReadKeyUV(ref Data.Object[i0].TexTrans[i1].TranslateFrame, CompressF16);
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

        public byte[] A3DCWriter()
        {
            if (A3DCOpt) UsedValues = new Dictionary<int?, double?>();
            if (Data.Format < Format.F2LE) Data._.CompressF16 = null;

            IO = File.OpenWriter();
            for (byte i = 0; i < 2; i++)
            {
                bool ReturnToOffset = i == 1;
                IO.Position = 0;

                if (Data.CameraRoot != null)
                    for (i0 = 0; i0 < Data.CameraRoot.Length; i0++)
                    {
                        IO.WriteOffset(ref Data.CameraRoot[i0].      MT, ReturnToOffset);
                        IO.WriteOffset(ref Data.CameraRoot[i0].VP.   MT, ReturnToOffset);
                        IO.WriteOffset(ref Data.CameraRoot[i0].Interest, ReturnToOffset);
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
                        if (Data.MObjectHRC[i0].Instances != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                                IO.WriteOffset(ref Data.MObjectHRC[i0].Instances[i1].MT, ReturnToOffset);

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
                        Write(ref Data.CameraRoot[i0].      MT);
                        Write(ref Data.CameraRoot[i0].VP.   MT);
                        Write(ref Data.CameraRoot[i0].VP.Roll       );
                        Write(ref Data.CameraRoot[i0].VP.FocalLength);
                        Write(ref Data.CameraRoot[i0].VP.FOV        );
                        Write(ref Data.CameraRoot[i0].Interest);
                    }

                if (Data.Chara != null)
                    for (i0 = 0; i0 < Data.Chara.Length; i0++)
                        Write(ref Data.Chara[i0]);

                if (Data.Curve != null)
                    for (i0 = 0; i0 < Data.Curve.Length; i0++)
                        Write(ref Data.Curve[i0].CV);

                if (Data.DOF != null && Data.Format == Format.FT)
                    Write(ref Data.DOF.MT);

                if (Data.Light != null)
                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        Write(ref Data.Light[i0].Position     );
                        Write(ref Data.Light[i0].SpotDirection);
                    }

                if (Data.Light != null)
                    for (i0 = 0; i0 < Data.Light.Length; i0++)
                    {
                        Write(ref Data.Light[i0].Ambient      );
                        Write(ref Data.Light[i0].Diffuse      );
                        Write(ref Data.Light[i0].Incandescence);
                        Write(ref Data.Light[i0].Specular     );
                    }

                if (Data.Fog != null)
                    for (i0 = 0; i0 < Data.Fog.Length; i0++)
                    {
                        Write(ref Data.Fog[i0].Density);
                        Write(ref Data.Fog[i0].Diffuse);
                        Write(ref Data.Fog[i0].Start  );
                        Write(ref Data.Fog[i0].End    );
                    }

                if (Data.MObjectHRC != null)
                    for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                    {
                        if (Data.MObjectHRC[i0].Instances != null)
                            for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                                Write(ref Data.MObjectHRC[i0].Instances[i1].MT);

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
                        if (Data.Object[i0].TexTrans != null)
                            for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                            {
                                Write(ref Data.Object[i0].TexTrans[i1].Coverage      );
                                Write(ref Data.Object[i0].TexTrans[i1].Offset        );
                                Write(ref Data.Object[i0].TexTrans[i1].Repeat        );
                                Write(ref Data.Object[i0].TexTrans[i1].   Rotate     );
                                Write(ref Data.Object[i0].TexTrans[i1].   RotateFrame);
                                Write(ref Data.Object[i0].TexTrans[i1].TranslateFrame);
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
                    Write(ref Data.PostProcess.LensShaft);
                    Write(ref Data.PostProcess.LensGhost);
                }

                IO.Align(0x10, true);
            }
            byte[] A3DCData = IO.ToArray(true);
            byte[] A3DAData = A3DAWriter(true);

            IO = File.OpenWriter();
            IO.Offset = Data.Format > Format.FT ? 0x40 : 0;
            IO.Position = 0x40;

            Data.StringOffset = IO.Position;
            Data.StringLength = A3DAData.Length;
            IO.Write(A3DAData);
            IO.Align(0x20, true);

            Data.BinaryOffset = IO.Position;
            Data.BinaryLength = A3DCData.Length;
            IO.Write(A3DCData);
            IO.Align(0x10, true);

            int A3DCEnd = IO.Position;

            IO.Position = 0;
            IO.Write("#A3D", "C__________");
            IO.Write(0x2000);
            IO.Write(0x00);
            IO.WriteEndian(0x20, true);
            IO.Write(0x10000200);
            IO.Write(0x50);
            IO.WriteEndian(Data.StringOffset, true);
            IO.WriteEndian(Data.StringLength, true);
            IO.WriteEndian(0x01, true);
            IO.Write(0x4C42);
            IO.WriteEndian(Data.BinaryOffset, true);
            IO.WriteEndian(Data.BinaryLength, true);
            IO.WriteEndian(0x20, true);
            
            if (Data.Format > Format.FT)
            {
                IO.Position = A3DCEnd;
                IO.WriteEOFC(0);
                IO.Offset   = 0;
                IO.Position = 0;
                Header Header = new Header { Signature = 0x41443341, Format = Format.F2LE,
                    DataSize = A3DCEnd, SectionSize = A3DCEnd, InnerSignature = 0x01131010 };
                IO.Write(Header, true);
            }

            return IO.ToArray(true);
        }

        private void Write(ref ModelTransform MT)
        { Write(ref MT.Scale); Write(ref MT.Rot, true); Write(ref MT.Trans); Write(ref MT.Visibility); }

        private void Write(ref Vector4<Key> RGBA)
        { Write(ref RGBA.X); Write(ref RGBA.Y); Write(ref RGBA.Z); Write(ref RGBA.W); }

        private void Write(ref Vector3<Key> Key, bool F16 = false)
        { Write(ref Key.X, F16); Write(ref Key.Y, F16); Write(ref Key.Z, F16); }

        private void Write(ref Vector2<Key> UV)
        { Write(ref UV.X); Write(ref UV.Y); }

        private void Write(ref Key Key, bool F16 = false)
        {
            if (Key.Type == null) return;

            int i = 0;
            if (Key.Keys != null)
            {
                Key.BinOffset = IO.Position;
                int Type = (int)Key.Type & 0xFF;
                if (Key.EPTypePost.HasValue) Type |= (Key.EPTypePost.Value & 0xF) << 12;
                if (Key.EPTypePre .HasValue) Type |= (Key.EPTypePre .Value & 0xF) <<  8;
                IO.Write(Type);
                IO.Write(0x00);
                IO.Write((float)Key.Max);
                IO.Write(Key.Keys.Length);
                for (i = 0; i < Key.Keys.Length; i++)
                {
                    ref KFT3<float, float> KF = ref Key.Keys[i];
                    if (F16 && CompressF16 >  0) { IO.Write((ushort)KF.F ); IO.Write((Half)KF.V ); }
                    else                         { IO.Write(        KF.F ); IO.Write(      KF.V ); }
                    if (F16 && CompressF16 == 2) { IO.Write((  Half)KF.T1); IO.Write((Half)KF.T2); }
                    else                         { IO.Write(        KF.T1); IO.Write(      KF.T2); }
                }
            }
            else
            {
                if (!UsedValues.ContainsValue(Key.Value) || !A3DCOpt)
                {
                    Key.BinOffset = IO.Position;
                    IO.Write((  int)Key.Type );
                    IO.Write((float)Key.Value);
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
            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);
            if (!MsgPack.Element("A3D", out MsgPack A3D)) { MsgPack.Dispose(); return; }
            MsgPackReader(A3D);
        }

        public void MsgPackReader(MsgPack A3D)
        {
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

            if (A3D.ElementArray("Ambient", out Temp))
            {
                Data.Ambient = new Ambient[Temp.Array.Length];

                for (i = 0; i < Data.Ambient.Length; i++)
                    Data.Ambient[i] = new Ambient
                    {
                                   Name = Temp[i].ReadString (           "Name"),
                           LightDiffuse = Temp[i].ReadRGBAKey(   "LightDiffuse"),
                        RimLightDiffuse = Temp[i].ReadRGBAKey("RimLightDiffuse"),
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

            if (A3D.ElementArray("CameraRoot", out Temp))
            {
                Data.CameraRoot = new CameraRoot[Temp.Array.Length];
                for (i = 0; i < Data.CameraRoot.Length; i++)
                {
                    Data.CameraRoot[i] = new CameraRoot
                    {
                        MT       = Temp[i].ReadMT(),
                        Interest = Temp[i].ReadMT("Interest"),
                    };
                    if (!Temp[i].Element("ViewPoint", out MsgPack ViewPoint)) continue;

                    Data.CameraRoot[i].VP = new CameraRoot.ViewPoint
                    {
                        MT              = ViewPoint.ReadMT(),
                        Aspect          = ViewPoint.ReadNDouble("Aspect"         ),
                        CameraApertureH = ViewPoint.ReadNDouble("CameraApertureH"),
                        CameraApertureW = ViewPoint.ReadNDouble("CameraApertureW"),
                        FOVHorizontal   = ViewPoint.ReadBoolean("FOVHorizontal"  ),
                        FocalLength     = ViewPoint.ReadKey    ("FocalLength"    ),
                        FOV             = ViewPoint.ReadKey    ("FOV"            ),
                        Roll            = ViewPoint.ReadKey    ("Roll"           ),
                    };
                }
            }

            if (A3D.ElementArray("Chara", out Temp))
            {
                Data.Chara = new ModelTransform[Temp.Array.Length];
                for (i = 0; i < Data.Chara.Length; i++)
                    Data.Chara[i] = Temp[i].ReadMT();
            }

            if (A3D.ElementArray("Curve", out Temp))
            {
                Data.Curve = new Curve[Temp.Array.Length];
                for (i = 0; i < Data.Curve.Length; i++)
                    Data.Curve[i] = new Curve
                    {
                        Name = Temp[i].ReadString("Name"),
                        CV   = Temp[i].ReadKey   ("CV"  ),
                    };
            }

            if (A3D.Element("DOF", out Temp))
                Data.DOF = new DOF
                {
                    MT   = Temp.ReadMT(),
                    Name = Temp.ReadString("Name"),
                };

            if (A3D.ElementArray("Event", out Temp))
            {
                Data.Event = new Event[Temp.Array.Length];
                for (i = 0; i < Data.Event.Length; i++)
                    Data.Event[i] = new Event
                    {
                            Begin    = Temp[i].ReadNDouble(    "Begin"   ),
                        ClipBegin    = Temp[i].ReadNDouble("ClipBegin"   ),
                        ClipEnd      = Temp[i].ReadNDouble("ClipEnd"     ),
                            End      = Temp[i].ReadNDouble(    "End"     ),
                        Name         = Temp[i].ReadString ("Name"        ),
                        Param1       = Temp[i].ReadString ("Param1"      ),
                        Ref          = Temp[i].ReadString ("Ref"         ),
                        TimeRefScale = Temp[i].ReadNDouble("TimeRefScale"),
                        Type         = Temp[i].ReadNInt32 ("Type"        ),
                    };
            }

            if (A3D.ElementArray("Fog", out Temp))
            {
                Data.Fog = new Fog[Temp.Array.Length];
                for (i = 0; i < Data.Fog.Length; i++)
                    Data.Fog[i] = new Fog
                    {
                        Id      = Temp[i].ReadNInt32 ("Id"     ),
                        Density = Temp[i].ReadKey    ("Density"),
                        Diffuse = Temp[i].ReadRGBAKey("Diffuse"),
                        End     = Temp[i].ReadKey    ("End"    ),
                        Start   = Temp[i].ReadKey    ("Start"  ),
                    };
            }

            if (A3D.ElementArray("Light", out Temp))
            {
                Data.Light = new Light[Temp.Array.Length];
                for (i = 0; i < Data.Light.Length; i++)
                    Data.Light[i] = new Light
                    {
                        Id            = Temp[i].ReadNInt32 ("Id"           ),
                        Name          = Temp[i].ReadString ("Name"         ),
                        Type          = Temp[i].ReadString ("Type"         ),
                        Ambient       = Temp[i].ReadRGBAKey("Ambient"      ),
                        Diffuse       = Temp[i].ReadRGBAKey("Diffuse"      ),
                        Incandescence = Temp[i].ReadRGBAKey("Incandescence"),
                        Position      = Temp[i].ReadMT     ("Position"     ),
                        Specular      = Temp[i].ReadRGBAKey("Specular"     ),
                        SpotDirection = Temp[i].ReadMT     ("SpotDirection"),
                    };
            }

            if (A3D.ElementArray("MaterialList", out Temp))
            {
                Data.MaterialList = new MaterialList[Temp.Array.Length];
                for (i = 0; i < Data.MaterialList.Length; i++)
                    Data.MaterialList[i] = new MaterialList
                    {
                        HashName      = Temp[i].ReadString (     "HashName"),
                            Name      = Temp[i].ReadString (         "Name"),
                        BlendColor    = Temp[i].ReadRGBAKey(   "BlendColor"),
                        GlowIntensity = Temp[i].ReadKey    ("GlowIntensity"),
                        Incandescence = Temp[i].ReadRGBAKey("Incandescence"),
                    };
            }

            if (A3D.ElementArray("MObjectHRC", out Temp))
            {
                Data.MObjectHRC = new MObjectHRC[Temp.Array.Length];
                for (i0 = 0; i0 < Data.MObjectHRC.Length; i0++)
                {
                    Data.MObjectHRC[i0] = new MObjectHRC
                    {
                        MT   = Temp[i0].ReadMT(),
                        Name = Temp[i0].ReadString("Name"),
                    };

                    if (Temp[i0].Element("JointOrient", out MsgPack JointOrient))
                        Data.MObjectHRC[i0].JointOrient = new Vector3<double?>
                        {
                            X = JointOrient.ReadDouble("X"),
                            Y = JointOrient.ReadDouble("Y"),
                            Z = JointOrient.ReadDouble("Z"),
                        };

                    if (Temp[i0].ElementArray("Instance", out MsgPack Instance))
                    {
                        Data.MObjectHRC[i0].Instances = new MObjectHRC.Instance[Instance.Array.Length];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Instances.Length; i1++)
                            Data.MObjectHRC[i0].Instances[i1] = new MObjectHRC.Instance
                            {
                                MT = Instance[i1].ReadMT(),
                                   Name = Instance[i1].ReadString(   "Name"),
                                 Shadow = Instance[i1].ReadNInt32( "Shadow"),
                                UIDName = Instance[i1].ReadString("UIDName"),
                            };
                    }

                    if (Temp[i0].ElementArray("Node", out MsgPack Node))
                    {
                        Data.MObjectHRC[i0].Node = new Node[Temp.Array.Length];
                        for (i1 = 0; i1 < Data.MObjectHRC[i0].Node.Length; i1++)
                            Data.MObjectHRC[i0].Node[i1] = new Node
                            {
                                MT = Node[i1].ReadMT(),
                                  Name = Node[i1].ReadString(  "Name"),
                                Parent = Node[i1].ReadNInt32("Parent"),
                            };
                    }
                }
            }

            if (A3D.ElementArray("MObjectHRCList", out Temp))
            {
                Data.MObjectHRCList = new string[Temp.Array.Length];
                for (i = 0; i < Data.MObjectHRCList.Length; i++)
                    Data.MObjectHRCList[i] = Temp[i].ReadString();
            }

            if (A3D.ElementArray("Motion", out Temp))
            {
                Data.Motion = new string[Temp.Array.Length];
                for (i = 0; i < Data.Motion.Length; i++)
                    Data.Motion[i] = Temp[i].ReadString();
            }

            if (A3D.ElementArray("Object", out Temp))
            {
                Data.Object = new Object[Temp.Array.Length];
                for (i0 = 0; i0 < Data.Object.Length; i0++)
                {
                    Data.Object[i0] = new Object
                    {
                                 MT = Temp[i0].ReadMT(),
                        Morph       = Temp[i0].ReadString("Morph"      ),
                        MorphOffset = Temp[i0].ReadNInt32("MorphOffset"),
                               Name = Temp[i0].ReadString(       "Name"),
                         ParentName = Temp[i0].ReadString( "ParentName"),
                            UIDName = Temp[i0].ReadString(    "UIDName"),
                    };

                    if (Temp[i0].ElementArray("TexturePattern", out MsgPack TexPat))
                    {
                        Data.Object[i0].TexPat = new Object.TexturePattern[TexPat.Array.Length];
                        for (i1 = 0; i1 < Data.Object[i0].TexPat.Length; i1++)
                            Data.Object[i0].TexPat[i1] = new Object.TexturePattern
                            {
                                Name      = TexPat[i1].ReadString("Name"     ),
                                Pat       = TexPat[i1].ReadString("Pat"      ),
                                PatOffset = TexPat[i1].ReadNInt32("PatOffset"),
                            };
                    }

                    if (Temp[i0].ElementArray("TextureTransform", out MsgPack TexTrans))
                    {
                        Data.Object[i0].TexTrans = new Object.TextureTransform[TexTrans.Array.Length];
                        for (i1 = 0; i1 < Data.Object[i0].TexTrans.Length; i1++)
                            Data.Object[i0].TexTrans[i1] = new Object.TextureTransform
                            {
                                Name = TexTrans[i1].ReadString("Name"),
                                Coverage       = TexTrans[i1].ReadKeyUV("Coverage"      ),
                                Offset         = TexTrans[i1].ReadKeyUV("Offset"        ),
                                Repeat         = TexTrans[i1].ReadKeyUV("Repeat"        ),
                                   Rotate      = TexTrans[i1].ReadKey  (   "Rotate"     ),
                                   RotateFrame = TexTrans[i1].ReadKey  (   "RotateFrame"),
                                TranslateFrame = TexTrans[i1].ReadKeyUV("TranslateFrame"),
                            };
                    }
                }
            }

            if (A3D.ElementArray("ObjectHRC", out Temp))
            {
                Data.ObjectHRC = new ObjectHRC[Temp.Array.Length];
                for (i0 = 0; i0 < Data.ObjectHRC.Length; i0++)
                {
                    Data.ObjectHRC[i0] = new ObjectHRC
                    {
                           Name = Temp[i0].ReadString (   "Name"),
                         Shadow = Temp[i0].ReadNDouble( "Shadow"),
                        UIDName = Temp[i0].ReadString ("UIDName"),
                    };

                    if (Temp[i0].Element("JointOrient", out MsgPack JointOrient))
                        Data.ObjectHRC[i0].JointOrient = new Vector3<double?>
                        {
                            X = JointOrient.ReadDouble("X"),
                            Y = JointOrient.ReadDouble("Y"),
                            Z = JointOrient.ReadDouble("Z"),
                        };

                    if (Temp[i0].ElementArray("Node", out MsgPack Node))
                    {
                        Data.ObjectHRC[i0].Node = new Node[Node.Array.Length];
                        for (i1 = 0; i1 < Data.ObjectHRC[i0].Node.Length; i1++)
                            Data.ObjectHRC[i0].Node[i1] = new Node
                            {
                                    MT = Node[i1].ReadMT(),
                                  Name = Node[i1].ReadString(  "Name"),
                                Parent = Node[i1].ReadInt32 ("Parent"),
                            };
                    }
                }
            }

            if (A3D.ElementArray("ObjectHRCList", out Temp))
            {
                Data.ObjectHRCList = new string[Temp.Array.Length];
                for (i = 0; i < Data.ObjectHRCList.Length; i++)
                    Data.ObjectHRCList[i] = Temp[i].ReadString();
            }

            if (A3D.ElementArray("ObjectList", out Temp))
            {
                Data.ObjectList = new string[Temp.Array.Length];
                for (i = 0; i < Data.ObjectList.Length; i++)
                    Data.ObjectList[i] = Temp[i].ReadString();
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

            if (A3D.ElementArray("Point", out Temp))
            {
                Data.Point = new ModelTransform[Temp.Array.Length];
                for (i = 0; i < Data.Point.Length; i++)
                    Data.Point[i] = Temp[i].ReadMT();
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

        public static ModelTransform ReadMT(this Dictionary<string, object> Dict, string Temp)
        {
            ModelTransform MT = new ModelTransform();
            Dict.FindValue(out MT.BinOffset, Temp + MTBO);
            
            MT.Rot        = Dict.ReadVec3(Temp + "rot"        + d);
            MT.Scale      = Dict.ReadVec3(Temp + "scale"      + d);
            MT.Trans      = Dict.ReadVec3(Temp + "trans"      + d);
            MT.Visibility = Dict.ReadKey (Temp + "visibility" + d);
            return MT;
        }

        public static Vector4<Key> ReadRGBAKey(this Dictionary<string, object> Dict, string Temp) =>
            new Vector4<Key> { W = Dict.ReadKey(Temp + "a" + d), Z = Dict.ReadKey(Temp + "b" + d),
                               Y = Dict.ReadKey(Temp + "g" + d), X = Dict.ReadKey(Temp + "r" + d) };

        public static Vector3<Key> ReadVec3(this Dictionary<string, object> Dict, string Temp) =>
            new Vector3<Key> { X = Dict.ReadKey(Temp + "x" + d), Y =
                Dict.ReadKey(Temp + "y" + d), Z = Dict.ReadKey(Temp + "z" + d) };

        public static Vector2<Key> ReadKeyUV(this Dictionary<string, object> Dict, string Temp) =>
            new Vector2<Key> { X = Dict.ReadKey(Temp + "U" + d), Y = Dict.ReadKey(Temp + "V" + d) };

        public static Key ReadKey(this Dictionary<string, object> Dict, string Temp)
        {
            Key Key = new Key();
            if ( Dict.FindValue(out Key.BinOffset, Temp + BO    )) return  Key;
            if (!Dict.FindValue(out int Type     , Temp + "type")) return default;

            Key.Type = (Key.Interpolation)Type;
            if (Type == 0x0000) return Key;
            if (Type == 0x0001) { Dict.FindValue(out Key.Value, Temp + "value"); return Key; }

            int i = 0;
            Dict.FindValue(out Key.EPTypePost, Temp + "ep_type_post");
            Dict.FindValue(out Key.EPTypePre , Temp + "ep_type_pre" );
            Dict.FindValue(out Key.Length    , Temp + "key.length"  );
            Dict.FindValue(out Key.Max       , Temp + "max"         );
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
                Key.Keys = new KFT3<float, float>[Key.Length];
                     if (Key.RawData.KeyType == 0)
                    for (i = 0; i < Key.Length; i++)
                        Key.Keys[i] = new KFT3<float, float>
                        (ValueList[i * DS + 0].ToSingle());
                else if (Key.RawData.KeyType == 1)
                    for (i = 0; i < Key.Length; i++)
                        Key.Keys[i] = new KFT3<float, float>
                        (ValueList[i * DS + 0].ToSingle(), ValueList[i * DS + 1].ToSingle());
                else if (Key.RawData.KeyType == 2)
                    for (i = 0; i < Key.Length; i++)
                        Key.Keys[i] = new KFT3<float, float>
                        (ValueList[i * DS + 0].ToSingle(), ValueList[i * DS + 1].ToSingle(),
                         ValueList[i * DS + 2].ToSingle(), ValueList[i * DS + 2].ToSingle());
                else if (Key.RawData.KeyType == 3)
                    for (i = 0; i < Key.Length; i++)
                        Key.Keys[i] = new KFT3<float, float>
                        (ValueList[i * DS + 0].ToSingle(), ValueList[i * DS + 1].ToSingle(),
                         ValueList[i * DS + 2].ToSingle(), ValueList[i * DS + 3].ToSingle());

                Key.RawData.ValueList = null;
            }
            else
            {
                Key.Keys = new KFT3<float, float>[Key.Length];
                for (i = 0; i < Key.Length; i++)
                {
                    if (!Dict.FindValue(out value, Temp + "key" + d + i + d + "data")) continue;

                    dataArray = value.Replace("(", "").Replace(")", "").Split(',');
                    Type = dataArray.Length - 1;
                         if (Type == 0) Key.Keys[i] = new KFT3<float, float>
                        (dataArray[0].ToSingle());
                    else if (Type == 1) Key.Keys[i] = new KFT3<float, float>
                        (dataArray[0].ToSingle(), dataArray[1].ToSingle());
                    else if (Type == 2) Key.Keys[i] = new KFT3<float, float>
                        (dataArray[0].ToSingle(), dataArray[1].ToSingle(),
                         dataArray[2].ToSingle(), dataArray[2].ToSingle());
                    else if (Type == 3) Key.Keys[i] = new KFT3<float, float>
                        (dataArray[0].ToSingle(), dataArray[1].ToSingle(),
                         dataArray[2].ToSingle(), dataArray[3].ToSingle());
                }
            }
            return Key;
        }

        public static void Write(this Stream IO, ModelTransform MT,
            string Temp, bool A3DC, bool IsX = false, byte Flags = 0b11111)
        {
            if (A3DC && !MT.Writed && (Flags & 0b10000) == 0b10000)
            { IO.Write(Temp + MTBO + "=", MT.BinOffset); MT.Writed = true; }

            if (A3DC && !IsX) return;

            if ((Flags & 0b01000) == 0b01000) IO.Write(MT.Rot       , Temp + "rot"        + d, A3DC);
            if ((Flags & 0b00100) == 0b00100) IO.Write(MT.Scale     , Temp + "scale"      + d, A3DC);
            if ((Flags & 0b00010) == 0b00010) IO.Write(MT.Trans     , Temp + "trans"      + d, A3DC);
            if ((Flags & 0b00001) == 0b00001) IO.Write(MT.Visibility, Temp + "visibility" + d, A3DC);
        }

        public static void Write(this Stream IO, Vector4<Key> RGBA, string Temp, bool A3DC = false)
        {
            if (RGBA.X.Type == null && RGBA.Y.Type == null && RGBA.Z.Type == null && RGBA.W.Type == null) return;
            IO.Write(Temp + "=", "true");
            IO.Write(RGBA.W, Temp + d + "a" + d, A3DC);
            IO.Write(RGBA.Z, Temp + d + "b" + d, A3DC);
            IO.Write(RGBA.Y, Temp + d + "g" + d, A3DC);
            IO.Write(RGBA.X, Temp + d + "r" + d, A3DC);
        }

        public static void Write(this Stream IO, Vector3<Key> Key, string Temp, bool A3DC = false)
        { IO.Write(Key.X, Temp + "x" + d, A3DC); IO.Write(Key.Y,
            Temp + "y" + d, A3DC); IO.Write(Key.Z, Temp + "z" + d, A3DC); }

        public static void Write(this Stream IO, Vector2<Key> UV, string Temp, bool A3DC = false)
        { IO.Write(UV.X, Temp + "U", true, A3DC); IO.Write(UV.Y, Temp + "V", true, A3DC); }

        public static void Write(this Stream IO, Key Key, string Temp, bool SetBoolean, bool A3DC = false)
        { if (Key.Type == null) return; if (SetBoolean) IO.Write(Temp + "=", "true"); IO.Write(Key, Temp + d, A3DC); }

        public static void Write(this Stream IO, Key Key, string Temp, bool A3DC = false)
        {
            if (Key.Type == null) return;

            if (A3DC) { IO.Write(Temp + BO + "=", Key.BinOffset); return; }

            int i = 0;
            if (Key.Keys != null)
                if (Key.Keys.Length == 0)
                {
                    IO.Write(Temp + "type=", (int)Key.Type);
                    if (Key.Type > 0) IO.Write(Temp + "value=", Key.Value);
                    return;
                }

            if (Key.EPTypePost != null) IO.Write(Temp + "ep_type_post=", Key.EPTypePost);
            if (Key.EPTypePre  != null) IO.Write(Temp + "ep_type_pre=" , Key.EPTypePre );
            if (Key.RawData.KeyType == 0 && Key.Keys != null)
            {
                IKF<float, float> KF;
                SO = Key.Keys.Length.SortWriter();
                for (i = 0; i < Key.Keys.Length; i++)
                {
                    SOi = SO[i];
                    KF = Key.Keys[SOi].Check();
                    IO.Write(Temp + "key" + d + SOi + d + "data=", KF.Round(7).ToString());
                    int Type = 0;
                         if (KF is KFT0<float, float>) Type = 0;
                    else if (KF is KFT1<float, float>) Type = 1;
                    else if (KF is KFT2<float, float>) Type = 2;
                    else if (KF is KFT3<float, float>) Type = 3;
                    IO.Write(Temp + "key" + d + SOi + d + "type=", Type);
                }
                IO.Write(Temp + "key.length=", Key.Length);
                if (Key.Max != null) IO.Write(Temp + "max=", Key.Max);
            }
            else if (Key.Keys != null)
            {
                int Length = Key.Keys.Length;
                ref int KeyType = ref Key.RawData.KeyType;
                KeyType = 0;
                IKF<float, float> KF;
                if (Key.Max != null) IO.Write(Temp + "max=", Key.Max);
                for (i = 0; i < Length; i++)
                {
                    KF = Key.Keys[i].Check();
                         if (KF is KFT0<float, float> && KeyType < 0)   KeyType = 0;
                    else if (KF is KFT1<float, float> && KeyType < 1)   KeyType = 1;
                    else if (KF is KFT2<float, float> && KeyType < 2)   KeyType = 2;
                    else if (KF is KFT3<float, float> && KeyType < 3) { KeyType = 3; break; }
                }
                Key.RawData.ValueListSize = Length * KeyType + Length;
                IO.Write(Temp + "raw_data.value_list=");
                     if (KeyType == 0) for (i = 0; i < Length; i++)
                        IO.Write(Key.Keys[i].ToT0().ToString(false) + (i + 1 < Length ? "," : ""));
                else if (KeyType == 1) for (i = 0; i < Length; i++)
                        IO.Write(Key.Keys[i].ToT1().ToString(false) + (i + 1 < Length ? "," : ""));
                else if (KeyType == 2) for (i = 0; i < Length; i++)
                        IO.Write(Key.Keys[i].ToT2().ToString(false) + (i + 1 < Length ? "," : ""));
                else if (KeyType == 3) for (i = 0; i < Length; i++)
                        IO.Write(Key.Keys[i]       .ToString(false) + (i + 1 < Length ? "," : ""));
                IO.Position--;
                IO.Write('\n');
                IO.Write(Temp + "raw_data.value_list_size=", Key.RawData.ValueListSize);
                IO.Write(Temp + "raw_data.value_type="     , Key.RawData.ValueType    );
                IO.Write(Temp + "raw_data_key_type="       , Key.RawData.  KeyType    );
            }
            IO.Write(Temp + "type=", (int)Key.Type);
            if (Key.RawData.KeyType == 0 && Key.Keys == null && Key.Value != null)
                if (Key.Value != 0) IO.Write(Temp + "value=", Key.Value);
        }

        public static void ReadMT(this Stream IO, ref ModelTransform MT, int C_F16)
        {
            if (MT.BinOffset == null) return;

            IO.Position = (int)MT.BinOffset;

            IO.ReadOffset(out MT.Scale);
            IO.ReadOffset(out MT.Rot  );
            IO.ReadOffset(out MT.Trans);
            MT.Visibility = new Key { BinOffset = IO.ReadInt32() };

            IO.ReadVec3(ref MT.Scale     , C_F16);
            IO.ReadVec3(ref MT.Rot       , C_F16,  true);
            IO.ReadVec3(ref MT.Trans     , C_F16);
            IO.ReadKey (ref MT.Visibility, C_F16);
        }

        public static void ReadRGBAKey(this Stream IO, ref Vector4<Key> RGBA, int C_F16)
        { IO.ReadKey(ref RGBA.X, C_F16); IO.ReadKey(ref RGBA.Y, C_F16);
          IO.ReadKey(ref RGBA.Z, C_F16); IO.ReadKey(ref RGBA.W, C_F16); }

        public static void ReadVec3(this Stream IO, ref Vector3<Key> Key, int C_F16, bool F16 = false)
        { IO.ReadKey(ref Key.X, C_F16, F16); IO.ReadKey(ref Key.Y,
            C_F16, F16); IO.ReadKey(ref Key.Z, C_F16, F16); }

        public static void ReadKeyUV(this Stream IO, ref Vector2<Key> UV, int C_F16)
        { IO.ReadKey(ref UV.X, C_F16); IO.ReadKey(ref UV.Y, C_F16); }

        public static void ReadKey(this Stream IO, ref Key Key, int C_F16, bool F16 = false)
        {
            if (Key.BinOffset == null || Key.BinOffset < 0) return;
            
            IO.Position = (int)Key.BinOffset;
            int Type = IO.ReadInt32();
            Key.Value = IO.ReadSingle();
            Key.Type = (Key.Interpolation)(Type & 0xFF);
            if (Key.Type < Key.Interpolation.Lerp) return;
            Key.Max    = IO.ReadSingle();
            Key.Length = IO.ReadInt32 ();
            if (Type >> 8 != 0)
            {
                Key.EPTypePost = (Type >> 12) & 0xF;
                Key.EPTypePre  = (Type >>  8) & 0xF;
                if (Key.EPTypePost == 0) Key.EPTypePost = null;
                if (Key.EPTypePre  == 0) Key.EPTypePre  = null;
            }
            Key.Keys = new KFT3<float, float>[Key.Length];
            for (int i = 0; i < Key.Length; i++)
            {
                if (F16 && C_F16 > 0)
                { Key.Keys[i].F  = IO.ReadUInt16(); Key.Keys[i].V  = IO.ReadHalf  (); }
                else
                { Key.Keys[i].F  = IO.ReadSingle(); Key.Keys[i].V  = IO.ReadSingle(); }

                if (F16 && C_F16 == 2)
                { Key.Keys[i].T1 = IO.ReadHalf  (); Key.Keys[i].T2 = IO.ReadHalf  (); }
                else
                { Key.Keys[i].T1 = IO.ReadSingle(); Key.Keys[i].T2 = IO.ReadSingle(); }
            }
        }

        public static void ReadOffset(this Stream IO, out Vector3<Key> Key)
        { Key = new Vector3<Key> { X = new Key { BinOffset = IO.ReadInt32() },
                                   Y = new Key { BinOffset = IO.ReadInt32() },
                                   Z = new Key { BinOffset = IO.ReadInt32() }, }; }

        public static void WriteOffset(this Stream IO, ref ModelTransform MT, bool ReturnToOffset)
        {
            if (ReturnToOffset)
            {
                IO.Position = (int)MT.BinOffset;
                IO.WriteOffset(MT.Scale);
                IO.WriteOffset(MT.Rot  );
                IO.WriteOffset(MT.Trans);
                IO.Write(MT.Visibility.BinOffset);
            }
            else
            {
                MT.BinOffset = IO.Position;
                IO.Position += 0x30;
                IO.Length   += 0x30;
            }
        }

        public static void WriteOffset(this Stream IO, Vector3<Key> Key)
        {
            IO.Write(Key.X.BinOffset);
            IO.Write(Key.Y.BinOffset);
            IO.Write(Key.Z.BinOffset);
        }

        public static ModelTransform ReadMT(this MsgPack k, string name) =>
            k.Element(name, out MsgPack Name) ? Name.ReadMT() : default;

        public static ModelTransform ReadMT(this MsgPack k) =>
            new ModelTransform { Rot   = k.ReadVec3("Rot"  ), Scale      = k.ReadVec3("Scale"     ),
                                 Trans = k.ReadVec3("Trans"), Visibility = k.ReadKey ("Visibility") };

        public static Vector4<Key> ReadRGBAKey(this MsgPack k, string name) =>
            k.Element(name, out MsgPack Name) ? Name.ReadRGBAKey() : default;

        public static Vector4<Key> ReadRGBAKey(this MsgPack k) =>
            new Vector4<Key> { X = k.ReadKey("R"), Y = k.ReadKey("G"), Z = k.ReadKey("B"), W = k.ReadKey("A") };

        public static Vector3<Key> ReadVec3(this MsgPack k, string name) =>
            k.Element(name, out MsgPack Name) ? Name.ReadVec3() : default;

        public static Vector3<Key> ReadVec3(this MsgPack k) =>
            new Vector3<Key> { X = k.ReadKey("X"), Y = k.ReadKey("Y"), Z = k.ReadKey("Z") };

        public static Vector2<Key> ReadKeyUV(this MsgPack k, string name) =>
            k.Element(name, out MsgPack Name) ? Name.ReadKeyUV() : default;

        public static Vector2<Key> ReadKeyUV(this MsgPack k) =>
            new Vector2<Key> { X = k.ReadKey("U"), Y = k.ReadKey("V") };

        public static Key ReadKey(this MsgPack k, string name) =>
            k.Element(name, out MsgPack Name) ? Name.ReadKey() : default;

        public static Key ReadKey(this MsgPack k)
        {
            if (k.Object == null) return default;
            
            Key Key = new Key { EPTypePost = k.ReadNInt32("EPTypePost"), EPTypePre =
                k.ReadNInt32("EPTypePre"), Max = k.ReadNDouble("Max"), Value = k.ReadNDouble("Value") };
            if (!Enum.TryParse(k.ReadString("Type"), out Key.Interpolation Type)) { Key.Value = null; return Key; }
            Key.Type = Type;
            if (Key.Type == 0) { Key.Value = 0; return Key; }
            else if (Key.Type < Key.Interpolation.Lerp) return Key;

            if (k.ReadBoolean("RawData")) Key.RawData = new Key.RawD() { KeyType = -1, ValueType = "float" };
            if (!k.ElementArray("Trans", out MsgPack Trans)) return Key;

            Key.Length = Trans.Array.Length;
            Key.Keys = new KFT3<float, float>[Key.Length];
            for (int i = 0; i < Key.Length; i++)
            {
                if (Trans[i].Array == null) continue;
                else if (Trans[i].Array.Length == 0) continue;
                else if (Trans[i].Array.Length == 1)
                    Key.Keys[i] = new KFT3<float, float>
                        (Trans[i][0].ReadSingle());
                else if (Trans[i].Array.Length == 2)
                    Key.Keys[i] = new KFT3<float, float>
                        (Trans[i][0].ReadSingle(), Trans[i][1].ReadSingle());
                else if (Trans[i].Array.Length == 3)
                    Key.Keys[i] = new KFT3<float, float>
                        (Trans[i][0].ReadSingle(), Trans[i][1].ReadSingle(),
                         Trans[i][2].ReadSingle(), Trans[i][2].ReadSingle());
                else if (Trans[i].Array.Length == 4)
                    Key.Keys[i] = new KFT3<float, float>
                        (Trans[i][0].ReadSingle(), Trans[i][1].ReadSingle(),
                         Trans[i][2].ReadSingle(), Trans[i][3].ReadSingle());
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
            if (Key.Keys != null && Key.Type != Key.Interpolation.Null)
            {
                Keys = Keys.Add("Max", Key.Max).Add("EPTypePost", Key.EPTypePost).Add("EPTypePre", Key.EPTypePre);

                if (Key.RawData.KeyType != 0) Keys.Add("RawData", true);
                
                MsgPack Trans = new MsgPack(Key.Keys.Length, "Trans");
                for (int i = 0; i < Key.Keys.Length; i++)
                {
                    IKF<float, float> KF = Key.Keys[i].Check();
                         if (KF is KFT0<float, float> KFT0) Trans[i] = new MsgPack(null, new MsgPack[] 
                        { KFT0.F });
                    else if (KF is KFT1<float, float> KFT1) Trans[i] = new MsgPack(null, new MsgPack[]
                        { KFT1.F, KFT1.V });
                    else if (KF is KFT2<float, float> KFT2) Trans[i] = new MsgPack(null, new MsgPack[]
                        { KFT2.F, KFT2.V, KFT2.T });
                    else if (KF is KFT3<float, float> KFT3) Trans[i] = new MsgPack(null, new MsgPack[]
                        { KFT3.F, KFT3.V, KFT3.T1, KFT3.T2, });
                }
                Keys.Add(Trans);
            }
            else if (Key.Value != 0) Keys.Add("Value", Key.Value);
            return MsgPack.Add(Keys);
        }

        public static void Write(this Stream IO, string Data, ref bool? val)
        { if (val != null) IO.Write(Data, (  bool)val   ); }
        public static void Write(this Stream IO, string Data,     long? val)
        { if (val != null) IO.Write(Data, (  long)val   ); }
        public static void Write(this Stream IO, string Data,    ulong? val)
        { if (val != null) IO.Write(Data, ( ulong)val   ); }
        public static void Write(this Stream IO, string Data,   double? val)
        { if (val != null) IO.Write(Data, (double)val   ); }
        public static void Write(this Stream IO, string Data,   double? val, byte r)
        { if (val != null) IO.Write(Data, (double)val, r); }
        public static void Write(this Stream IO, string Data, ref bool  val)         =>
            IO.Write(Data, Extensions.ToString(val));
        public static void Write(this Stream IO, string Data,     long  val)         =>
            IO.Write(Data,  val.ToString(   ));
        public static void Write(this Stream IO, string Data,    ulong  val)         =>
            IO.Write(Data,  val.ToString(   ));
        public static void Write(this Stream IO, string Data,   double  val)         =>
            IO.Write(Data,  val.ToString(   ));
        public static void Write(this Stream IO, string Data,   double  val, byte r) =>
            IO.Write(Data,  val.ToString(r  ));
        public static void Write(this Stream IO, string Data,   string  val)
        { if (val != null) IO.Write((Data + val + "\n").ToUTF8()); }
    }

    public struct A3DAData
    {
        public int Count;
        public int BinaryLength;
        public int BinaryOffset;
        public int HeaderOffset;
        public int StringLength;
        public int StringOffset;

        public Format Format;

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

    public struct _
    {
        public int? CompressF16;
        public string FileName;
        public string PropertyVersion;
        public string ConverterVersion;
    }

    public struct Ambient
    {
        public string  Name;
        public Vector4<Key>    LightDiffuse;
        public Vector4<Key> RimLightDiffuse;
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
        public ViewPoint VP;
        public ModelTransform MT;
        public ModelTransform Interest;

        public struct ViewPoint
        {
            public bool? FOVHorizontal;
            public double? Aspect;
            public double? CameraApertureH;
            public double? CameraApertureW;
            public Key FOV;
            public Key Roll;
            public Key FocalLength;
            public ModelTransform MT;
        }
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
        public Vector4<Key> Diffuse;
    }

    public struct Key
    {
        public Interpolation? Type;
        public int Length;
        public int? BinOffset;
        public int? EPTypePre;
        public int? EPTypePost;
        public double? Max;
        public double? Value;
        public RawD RawData;
        public KFT3<float, float>[] Keys;

        public struct RawD
        {
            public int KeyType;
            public int ValueListSize;
            public string ValueType;
            public string[] ValueList;
        }

        public enum Interpolation
        {
            Null    = 0,
            Value   = 1,
            Lerp    = 2,
            Hermite = 3,
            Hold    = 4,
        }
    }

    public struct Light
    {
        public int? Id;
        public string Name;
        public string Type;
        public Vector4<Key> Ambient;
        public Vector4<Key> Diffuse;
        public Vector4<Key> Specular;
        public Vector4<Key> Incandescence;
        public ModelTransform Position;
        public ModelTransform SpotDirection;
    }

    public struct MaterialList
    {
        public string Name;
        public string HashName;
        public Key GlowIntensity;
        public Vector4<Key> BlendColor;
        public Vector4<Key> Incandescence;
    }

    public struct MObjectHRC
    {
        public string Name;
        public Node[] Node;
        public Vector3<double?> JointOrient;
        public Instance[] Instances;
        public ModelTransform MT;

        public struct Instance
        {
            public int? Shadow;
            public string Name;
            public string UIDName;
            public ModelTransform MT;
        }
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
        public TexturePattern[] TexPat;
        public TextureTransform[] TexTrans;

        public struct TexturePattern
        {
            public int? PatOffset;
            public string Pat;
            public string Name;
        }

        public struct TextureTransform
        {
            public string Name;
            public Key Rotate;
            public Key RotateFrame;
            public Vector2<Key> Offset;
            public Vector2<Key> Repeat;
            public Vector2<Key> Coverage;
            public Vector2<Key> TranslateFrame;
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

    public struct PlayControl
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
        public Vector4<Key> Ambient;
        public Vector4<Key> Diffuse;
        public Vector4<Key> Specular;
    }
}
