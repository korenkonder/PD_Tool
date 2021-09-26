namespace KKdBaseLib.Auth3D
{
    public struct Data
    {
        public string[] Motion;
        public string[] ObjectList;
        public string[] ObjectHRCList;
        public string[] MObjectHRCList;
        public _ _;
        public DOF? DOF;
        public Fog[] Fog;
        public Chara[] Chara;
        public Curve[] Curve;
        public Event[] Event;
        public Light[] Light;
        public Point[] Point;
        public Object[] Object;
        public Ambient[] Ambient;
        public ObjectHRC[] ObjectHRC;
        public CameraRoot[] CameraRoot;
        public MObjectHRC[] MObjectHRC;
        public PlayControl PlayControl;
        public PostProcess? PostProcess;
        public MaterialList[] MaterialList;
        public CameraAuxiliary? CameraAuxiliary;
    }

    public struct _
    {
        public CompressF16 CompressF16;
        public string FileName;
        public string PropertyVersion;
        public string ConverterVersion;
    }

    public struct Ambient
    {
        public string Name;
        public Vec4<Key?>?    LightDiffuse;
        public Vec4<Key?>? RimLightDiffuse;
    }

    public struct CameraAuxiliary
    {
        public Key? Gamma;
        public Key? Exposure;
        public Key? Saturate;
        public Key? GammaRate;
        public Key? ExposureRate;
        public Key? AutoExposure;
    }

    public struct CameraRoot
    {
        public ViewPoint VP;
        public ModelTransform MT;
        public ModelTransform Interest;

        public struct ViewPoint
        {
            public  bool FOVIsHorizontal;
            public float Aspect;
            public float CameraApertureH;
            public float CameraApertureW;
            public Key FOV;
            public Key Roll;
            public Key FocalLength;
            public ModelTransform MT;
        }
    }

    public struct Chara
    {
        public ModelTransform MT;
        public string Name;
    }

    public struct Curve
    {
        public string Name;
        public Key CV;
    }

    public struct DOF
    {
        public string Name;
        public ModelTransform MT;
    }

    public struct Event
    {
        public int Type;
        public float End;
        public float Begin;
        public float ClipEnd;
        public float ClipBegin;
        public float TimeRefScale;
        public string Name;
        public string Param1;
        public string Ref;
    }

    public struct Fog
    {
        public int Id;
        public Key? End;
        public Key? Start;
        public Key? Density;
        public Vec4<Key?>? Color;
    }

    public enum CompressF16 : int
    {
        Type0 = 0,
        Type1 = 1,
        Type2 = 2,
    }

    public enum EPType : int // Pre/Post Infinity
    {
        None        = 0,
        Linear      = 1,
        Cycle       = 2,
        CycleOffset = 3,
    }

    public enum KeyType : int
    {
        None    = 0,
        Static  = 1,
        Linear  = 2,
        Hermite = 3,
        Hold    = 4,
    }

    public struct Key
    {
        public KeyType Type;
        public int? BinOffset;
        public EPType EPTypePre;
        public EPType EPTypePost;
        public float? Max;
        public float Value;
        public bool RawData;
        public KFT3[] Keys;

        public Key(A3DAKey k)
        {
            Type = 0;
            Value = 0;
            BinOffset = null;
            RawData = default;
            Keys = null;

            EPTypePost = k.EPTypePost;
            EPTypePre = k.EPTypePre;
            Max = k.MaxFrames;
            if (k.Length > 1)
            {
                Type = k.Type;
                Keys = k.Keys;
            }
            else if (k.Length == 1)
            {
                Type = KeyType.Static;
                Value = k.Keys[0].V;
            }
        }
    }

    public struct Light
    {
        public int Id;
        public string Name;
        public string Type;
        public Key? ConeAngle;
        public Key? Constant;
        public Key? DropOff;
        public Key? Far;
        public Key? Intensity;
        public Key? Linear;
        public Key? Quadratic;
        public Vec4<Key?>? Ambient;
        public Vec4<Key?>? Diffuse;
        public Vec4<Key?>? Specular;
        public Vec4<Key?>? ToneCurve;
        public ModelTransform Position;
        public ModelTransform SpotDirection;
    }

    public struct MaterialList
    {
        public string Name;
        public Key? GlowIntensity;
        public Vec4<Key?>? BlendColor;
        public Vec4<Key?>? Incandescence;
    }

    public struct MObjectHRC
    {
        public string Name;
        public ObjectNode[] Node;
        public Instance[] Instances;
        public ModelTransform MT;

        public struct Instance
        {
            public bool Shadow;
            public string Name;
            public string UIDName;
            public ModelTransform MT;
        }
    }

    public struct ModelTransform
    {
        public int? BinOffset;
        public Key Visibility;
        public Vec3<Key> Rot;
        public Vec3<Key> Scale;
        public Vec3<Key> Trans;
    }

    public struct Object
    {
        public float PatOffset;
        public float MorphOffset;
        public string Name;
        public string Pat;
        public string Morph;
        public string UIDName;
        public string ParentName;
        public ModelTransform MT;
        public TexturePattern[] TexPat;
        public TextureTransform[] TexTrans;

        public struct TexturePattern
        {
            public float PatOffset;
            public string Pat;
            public string Name;
        }

        public struct TextureTransform
        {
            public string Name;
            public Key? Rotate;
            public Key? RotateFrame;
            public Key? OffsetU;
            public Key? OffsetV;
            public Key? RepeatU;
            public Key? RepeatV;
            public Key? CoverageU;
            public Key? CoverageV;
            public Key? TranslateFrameU;
            public Key? TranslateFrameV;
        }
    }

    public struct ObjectHRC
    {
        public bool Shadow;
        public string Name;
        public string UIDName;
        public ObjectNode[] Node;
    }

    public struct ObjectNode
    {
        public Vec3? JointOrient;
        public int Parent;
        public string Name;
        public ModelTransform MT;
    }

    public struct PlayControl
    {
        public int Begin;
        public int? Div;
        public int FPS;
        public int? Offset;
        public int Size;
    }

    public struct Point
    {
        public ModelTransform MT;
        public string Name;
    }

    public struct PostProcess
    {
        public Key? LensFlare;
        public Key? LensGhost;
        public Key? LensShaft;
        public Vec4<Key?>? Intensity;
        public Vec4<Key?>? Radius;
        public Vec4<Key?>? SceneFade;
    }
}
