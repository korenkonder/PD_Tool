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
        public Pointer<Vector2<CountPointer<KFT2>>> Camera;
        public CountPointer<Composition> Compositions;
        public CountPointer<Video      > Videos;
        public CountPointer<Audio      > Audios;
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
        public int VideoItemOffset;
        public int ParentLayer;
        public CountPointer<Marker> Markers;
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
                Alpha                    = 3,
                Additive                 = 5,
                DstColorZero             = 6,
                SrcAlphaOneMinusSrcColor = 7,
                Transparent              = 8,
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
        public CountPointer<Source> Sources;

        public override string ToString() => $"Width: {Width}; Height: {Height}; Color: {Color.ToString("X2")}";

        public struct Source
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
