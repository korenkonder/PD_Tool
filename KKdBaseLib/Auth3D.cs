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
        public Curve[] Curve;
        public Event[] Event;
        public Light[] Light;
        public Object[] Object;
        public Ambient[] Ambient;
        public ObjectHRC[] ObjectHRC;
        public CameraRoot[] CameraRoot;
        public MObjectHRC[] MObjectHRC;
        public PlayControl PlayControl;
        public PostProcess? PostProcess;
        public MaterialList[] MaterialList;
        public ModelTransform[] Chara;
        public ModelTransform[] Point;
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
        public Vec4<Key>?    LightDiffuse;
        public Vec4<Key>? RimLightDiffuse;
    }

    public struct CameraAuxiliary
    {
        public Key? Gamma;
        public Key? Exposure;
        public Key? Saturate;
        public Key? GammaRate;
        public Key? AutoExposure;
    }

    public struct CameraRoot
    {
        public ViewPoint VP;
        public ModelTransform MT;
        public ModelTransform Interest;

        public struct ViewPoint
        {
            public  bool  FOVHorizontal;
            public float? Aspect;
            public float? CameraApertureH;
            public float? CameraApertureW;
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

    public struct DOF
    {
        public string Name;
        public ModelTransform MT;
    }

    public struct Event
    {
        public int? Type;
        public float? End;
        public float? Begin;
        public float? ClipEnd;
        public float? ClipBegin;
        public float? TimeRefScale;
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
        public Vec4<Key>? Diffuse;
    }

    public enum CompressF16 : int
    {
        Type0 = 0,
        Type1 = 1,
        Type2 = 2,
    }

    public enum EPType : int // Pre/Post Infinity
    {
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
        public int Length;
        public int? BinOffset;
        public EPType EPTypePre;
        public EPType EPTypePost;
        public float? Max;
        public float Value;
        public RawD RawData;
        public KFT3[] Keys;

        public Key(A3DAKey k)
        {
            Type = 0;
            Value = 0;
            Length = 0;
            BinOffset = null;
            RawData = default;
            Keys = null;

            EPTypePost = k.EPTypePost;
            EPTypePre = k.EPTypePre;
            Max = k.MaxFrames;
            if (k.Length > 1)
            {
                Type = k.Type;
                Length = (int)k.Length;
                Keys = k.Keys;
            }
            else if (k.Length == 1)
            {
                Type = KeyType.Static;
                Value = k.Keys[0].V;
            }
        }

        public struct RawD
        {
            public int KeyType;
            public int ValueListSize;
            public string ValueType;
            public string[] ValueList;
        }
    }

    public struct Light
    {
        public int? Id;
        public string Name;
        public string Type;
        public Vec4<Key>? Ambient;
        public Vec4<Key>? Diffuse;
        public Vec4<Key>? Specular;
        public Vec4<Key>? Incandescence;
        public ModelTransform Position;
        public ModelTransform SpotDirection;
    }

    public struct MaterialList
    {
        public string Name;
        public string HashName;
        public Key GlowIntensity;
        public Vec4<Key>? BlendColor;
        public Vec4<Key>? Incandescence;
    }

    public struct MObjectHRC
    {
        public string Name;
        public Node[] Node;
        public Vec3<float?> JointOrient;
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
        public Vec3<Key> Rot;
        public Vec3<Key> Scale;
        public Vec3<Key> Trans;
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
            public Key? Rotate;
            public Key? RotateFrame;
            public Vec2<Key?> Offset;
            public Vec2<Key?> Repeat;
            public Vec2<Key?> Coverage;
            public Vec2<Key?> TranslateFrame;
        }
    }

    public struct ObjectHRC
    {
        public bool? Shadow;
        public string Name;
        public string UIDName;
        public Node[] Node;
        public Vec3<float?> JointOrient;
    }

    public struct PlayControl
    {
        public int Begin;
        public int? Div;
        public int FPS;
        public int? Offset;
        public int Size;
    }

    public struct PostProcess
    {
        public Key? LensFlare;
        public Key? LensGhost;
        public Key? LensShaft;
        public Vec4<Key>? Ambient;
        public Vec4<Key>? Diffuse;
        public Vec4<Key>? Specular;
    }
}
