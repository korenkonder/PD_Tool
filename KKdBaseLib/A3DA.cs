namespace KKdBaseLib.A3DA
{
    public struct A3DAData
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

    public struct CameraAuxiliary
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
        public Vector4<Key> Diffuse;
    }

    public enum KeyType : int
    {
        Null    = 0,
        Value   = 1,
        Lerp    = 2,
        Hermite = 3,
        Hold    = 4,
    }

    public enum EPType : int
    {
        EP_1 = 1,
        EP_2 = 2,
        EP_3 = 3,
    }

    public struct Key
    {
        public KeyType? Type;
        public int Length;
        public int? BinOffset;
        public EPType EPTypePre;
        public EPType EPTypePost;
        public float? Max;
        public float? Value;
        public RawD RawData;
        public KFT3[] Keys;

        public struct RawD
        {
            public int KeyType;
            public int ValueListSize;
            public string ValueType;
            public string[] ValueList;
        }

        public static Vector3<A3DAKey> ToA3DAKey(Vector3<Key> k) =>
            new Vector3<A3DAKey> { X = (A3DAKey)k.X, Y = (A3DAKey)k.Y, Z = (A3DAKey)k.Z };
        public static Vector3<Key> ToKey(Vector3<A3DAKey> k) =>
            new Vector3<Key> { X = (Key)k.X, Y = (Key)k.Y, Z = (Key)k.Z };

        public static explicit operator Key(A3DAKey k)
        {
            Key key = default;
            key.EPTypePost = k.EPTypePost;
            key.EPTypePre = k.EPTypePre;
            key.Max = k.MaxFrames;
            if (k.Length > 1)
            {
                key.Type = k.Type;
                key.Length = k.Length;
                key.Keys = k.Keys;
            }
            else if (k.Length == 1)
            {
                key.Type = KeyType.Value;
                key.Value = k.Keys[0].V;
            }
            return key;
        }

        public static explicit operator A3DAKey(Key k)
        {
            A3DAKey key = default;
            key.EPTypePost = k.EPTypePost;
            key.EPTypePre = k.EPTypePre;
            key.MaxFrames = k.Max ?? 0;
            if (k.Type != null && k.Length > 1)
            {
                key.Type = k.Type.Value;
                key.Length = k.Length;
                key.Keys = k.Keys;
                key.FrameDelta = k.Keys[k.Length - 1].F - k.Keys[0].F;
                key.ValueDelta = k.Keys[k.Length - 1].V - k.Keys[0].V;
            }
            else
            {
                key.Length = 1;
                key.Keys = new KFT3[1];
                if (k.Length == 1)
                {
                    key.Type = KeyType.Value;
                    key.Keys[0].V = k.Value ?? 0;
                }
                else if (k.Type.HasValue && k.Value.HasValue)
                {
                    key.Type = k.Type.Value;
                    key.Keys[0].V = k.Value.Value;
                }
            }
            return key;
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
        public Vector3<float?> JointOrient;
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
        public float? Shadow;
        public string Name;
        public string UIDName;
        public Node[] Node;
        public Vector3<float?> JointOrient;
    }

    public struct PlayControl
    {
        public float? Begin;
        public float? Div;
        public float? FPS;
        public float? Offset;
        public float? Size;
    }

    public struct PostProcess
    {
        public Key LensFlare;
        public Key LensGhost;
        public Key LensShaft;
        public Vector4<Key> Ambient;
        public Vector4<Key> Diffuse;
        public Vector4<Key> Specular;
    }
}
