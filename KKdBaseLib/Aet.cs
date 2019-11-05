namespace KKdBaseLib.Aet
{
    public struct AetHeader
    {
        public Pointer<AetData>[] Data;
    }

    public struct AetData
    {
        public Pointer<string> Name;
        public float StartFrame;
        public float EndFrame;
        public float FrameRate;
        public uint BackColor;
        public uint Width;
        public uint Height;
        public Pointer<Vector2<CountPointer<KFT2>>> Camera;
        public CountPointer<AetComposition > Compositions ;
        public CountPointer<AetSurface> Surfaces;
        public CountPointer<AetSoundEffect > SoundEffects ;
    }

    public struct AetComposition
    {
        public int P;
        public int C { get => E != null ? E.Length : 0;
                       set => E = value > -1 ? new AetLayer[value] : null; }

        public int O;
        public AetLayer[] E;
        
        public override string ToString() => "Count: " + C;
    }


    public struct AetLayer
    {
        public int ID;
        public int Offset;
        public Pointer<string> Name;
        public float StartFrame;
        public float EndFrame;
        public float StartOffset;
        public float PlaybackSpeed;
        public AetLayerFlags Flags;
        public byte Pad;
        public AetLayerType Type;
        public int DataID;
        public int ParentLayer;
        public CountPointer<AetMarker> Marker;
        public Pointer<AnimationData> Data;
        public Pointer<AudioData> ExtraData;

        public enum AetLayerFlags : ushort
        {
            Visible      = 0b0000000000000001,
            Audible      = 0b0000000000000010,
            Unk2         = 0b0000000000000100,
            Unk3         = 0b0000000000001000,
            Unk4         = 0b0000000000010000,
            AudioRealted = 0b0000000000100000,
            Unk6         = 0b0000000001000000,
            SpriteFrames = 0b0000000010000000,
            Unk8         = 0b0000000100000000,
            Unk9         = 0b0000001000000000,
            Unk10        = 0b0000010000000000,
            Unk11        = 0b0000100000000000,
            Unk12        = 0b0001000000000000,
            Unk13        = 0b0010000000000000,
            Unk14        = 0b0100000000000000,
            Unk15        = 0b1000000000000000,
        }

        public enum AetLayerType : byte
        {
            Nop = 0,
            Pic = 1,
            Aif = 2,
            Eff = 3,
        }

        public override string ToString() => $"ID: {ID}; Name: {Name.V}; Type: {Type}" +
            (        DataID > -1 ? $"; Data ID: "    + $"{        DataID}" : "") +
            (ParentLayer > -1 ? $"; Parent Object ID: {ParentLayer}" : "");
    }
    
    public struct AetMarker
    {
        public float Frame;
        public Pointer<string> Name;
    }

    public struct AnimationData
    {
        public BlendMode Mode;
        public byte Padding0;
        public bool UseTextureMask;
        public byte Padding1;
        public CountPointer<KFT2>   OriginX;
        public CountPointer<KFT2>   OriginY;
        public CountPointer<KFT2> PositionX;
        public CountPointer<KFT2> PositionY;
        public CountPointer<KFT2> Rotation;
        public CountPointer<KFT2>    ScaleX;
        public CountPointer<KFT2>    ScaleY;
        public CountPointer<KFT2> Opacity;
        public Pointer<Perspective> Persp;

        public enum BlendMode : byte
        {
            Alpha                    = 3,
            Additive                 = 5,
            DstColorZero             = 6,
            SrcAlphaOneMinusSrcColor = 7,
            Transparent              = 8,
        }
        
        public struct Perspective
        {
            public CountPointer<KFT2> Unk1      ;
            public CountPointer<KFT2> Unk2      ;
            public CountPointer<KFT2> RotReturnX;
            public CountPointer<KFT2> RotReturnY;
            public CountPointer<KFT2> RotReturnZ;
            public CountPointer<KFT2>  RotationX;
            public CountPointer<KFT2>  RotationY;
            public CountPointer<KFT2>     ScaleZ;
        }
    }

    public struct AudioData
    {
        public CountPointer<KFT2> Data0;
        public CountPointer<KFT2> Data1;
        public CountPointer<KFT2> Data2;
        public CountPointer<KFT2> Data3;
    }

    public struct AetSurface
    {
        public int O;
        public uint Color;
        public ushort Width;
        public ushort Height;
        public float Frames;
        public CountPointer<AetSpriteIdentifier> Sprites;

        public override string ToString() => $"Width: {Width}; Height: {Height}; Color: {Color.ToString("X2")}";
    }

    public struct AetSpriteIdentifier
    {
        public Pointer<string> Name;
        public uint ID;

        public override string ToString() => $"ID: {ID}; Name: {Name}";
    }

    public struct AetSoundEffect
    {
        public int O;
        public uint Unk;
    }
}
