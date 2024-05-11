//Original: AetSet.bt Version: 5.0 by samyuu

namespace KKdBaseLib.Auth2D
{
    public struct Header
    {
        public Pointer<Scene>[] Scenes;
    }

    public struct Scene
    {
        public Pointer<string> Name;
        public float StartFrame;
        public float EndFrame;
        public float FrameRate;
        public uint BackColor;
        public uint Width;
        public uint Height;
        public Pointer<Camera> Camera;
        public CountPointer<Composition> Composition;
        public CountPointer<Video      > Video;
        public CountPointer<Audio      > Audio;
    }

    public struct Camera
    {
        public CountPointer<KFT2>       EyeX;
        public CountPointer<KFT2>       EyeY;
        public CountPointer<KFT2>       EyeZ;
        public CountPointer<KFT2>  PositionX;
        public CountPointer<KFT2>  PositionY;
        public CountPointer<KFT2>  PositionZ;
        public CountPointer<KFT2> DirectionX;
        public CountPointer<KFT2> DirectionY;
        public CountPointer<KFT2> DirectionZ;
        public CountPointer<KFT2>  RotationX;
        public CountPointer<KFT2>  RotationY;
        public CountPointer<KFT2>  RotationZ;
        public CountPointer<KFT2>      Zoom ;
    }

    public struct Composition
    {
        public int P;
        public int C { get => E != null ? E.Length : 0;
                       set => E = value > -1 ? new Layer[value] : null; }

        public int O;
        public Layer[] E;

        public override string ToString() => "Count: " + C;
    }


    public struct Layer
    {
        public int ID;
        public int Offset;
        public Pointer<string> Name;
        public float  StartFrame;
        public float    EndFrame;
        public float OffsetFrame;
        public float TimeScale;
        public AetLayerFlags   Flags;
        public AetLayerQuality Quality;
        public AetLayerType    Type;
        public int VidItmOff; //VideoItemOffset
        public int ParentLayer;
        public CountPointer<Marker> Marker;
        public Pointer<VideoData> Video;
        public Pointer<AudioData> Audio;

        public int DataID;

        public enum AetLayerFlags : ushort
        {
              VideoActive      = 0b0000000000000001,
              AudioActive      = 0b0000000000000010,
            EffectsActive      = 0b0000000000000100,
            MotionBlur         = 0b0000000000001000,
            FrameBlending      = 0b0000000000010000,
            Locked             = 0b0000000000100000,
            Shy                = 0b0000000001000000,
            Collapse           = 0b0000000010000000,
            AutoOrientRotation = 0b0000000100000000,
            AdjustmentLayer    = 0b0000001000000000,
            TimeRemapping      = 0b0000010000000000,
            LayerIs3D          = 0b0000100000000000,
            LookAtCamera       = 0b0001000000000000,
            LookAtPOI          = 0b0010000000000000, //POI - Point Of Interest
            Solo               = 0b0100000000000000,
            MarkersLocked      = 0b1000000000000000,
        }

        public enum AetLayerQuality : byte
        {
            None      = 0,
            Wireframe = 1,
            Draft     = 2,
            Best      = 3,
        }

        public enum AetLayerType : byte
        {
            None        = 0,
            Video       = 1,
            Audio       = 2,
            Composition = 3,
        }

        public override string ToString() => $"ID: {ID}; Name: {Name.V}; Type: {Type}" +
            (     DataID > -1 ? $"; " +    $"Data ID: {     DataID}" : "") +
            (ParentLayer > -1 ? $"; Parent Object ID: {ParentLayer}" : "");
    }

    public struct Marker
    {
        public float Frame;
        public Pointer<string> Name;
    }

    public struct VideoData
    {
        public VideoTransferMode TransferMode;
        public byte Padding;
        public CountPointer<KFT2>   AnchorX;
        public CountPointer<KFT2>   AnchorY;
        public CountPointer<KFT2> PositionX;
        public CountPointer<KFT2> PositionY;
        public CountPointer<KFT2> Rotation ;
        public CountPointer<KFT2>    ScaleX;
        public CountPointer<KFT2>    ScaleY;
        public CountPointer<KFT2>  Opacity ;
        public Pointer<Video3DData> Video3D;

        public struct VideoTransferMode
        {
            public TransferBlendMode  BlendMode;
            public TransferFlags      Flags;
            public TransferTrackMatte TrackMatte;

            public enum TransferBlendMode : byte
            {
                None              =  0,
                Copy              =  1,
                Behind            =  2,
                Normal            =  3,
                Dissolve          =  4,
                Add               =  5,
                Multiply          =  6,
                Screen            =  7,
                Overlay           =  8,
                SoftLight         =  9,
                HardLight         = 10,
                Darken            = 11,
                Lighten           = 12,
                ClassicDifference = 13,
                Hue               = 14,
                Saturation        = 15,
                Color             = 16,
                Luminosity        = 17,
                StenciilAlpha     = 18,
                StencilLuma       = 19,
                SilhouetteAlpha   = 20,
                SilhouetteLuma    = 21,
                LuminescentPremul = 22,
                AlphaAdd          = 23,
                ClassicColorDodge = 24,
                ClassicColorBurn  = 25,
                Exclusion         = 26,
                Difference        = 27,
                ColorDodge        = 28,
                ColorBurn         = 29,
                LinearDodge       = 30,
                LinearBurn        = 31,
                LinearLight       = 32,
                VividLight        = 33,
                PinLight          = 34,
                HardMix           = 35,
                LighterColor      = 36,
                DarkerColor       = 37,
                Subtract          = 38,
                Divide            = 39,
            }

            public enum TransferFlags : byte
            {
                 PreserveAlpha    = 0b01,
                RandomizeDissolve = 0b10,
            }

            public enum TransferTrackMatte : byte
            {
                NoTrackMatte = 0,
                       Alpha = 1,
                    NotAlpha = 2,
                       Luma  = 3,
                    NotLuma  = 4,
            }
        }

        public struct Video3DData
        {
            public CountPointer<KFT2>    AnchorZ;
            public CountPointer<KFT2>  PositionZ;
            public CountPointer<KFT2> DirectionX;
            public CountPointer<KFT2> DirectionY;
            public CountPointer<KFT2> DirectionZ;
            public CountPointer<KFT2>  RotationX;
            public CountPointer<KFT2>  RotationY;
            public CountPointer<KFT2>     ScaleZ;
        }
    }

    public struct AudioData
    {
        public CountPointer<KFT2> VolumeL;
        public CountPointer<KFT2> VolumeR;
        public CountPointer<KFT2>    PanL;
        public CountPointer<KFT2>    PanR;
    }

    public struct Video
    {
        public int O;
        public uint Color;
        public ushort Width;
        public ushort Height;
        public float Frames;
        public CountPointer<Identifier> Identifiers;

        public override string ToString() => $"Width: {Width}; Height: {Height}; Color: {Color:X2)}";

        public struct Identifier
        {
            public Pointer<string> Name;
            public uint ID;

            public override string ToString() => $"ID: {ID}; Name: {Name}";
        }
    }

    public struct Audio
    {
        public int O;
        public uint SoundID;
    }
}
