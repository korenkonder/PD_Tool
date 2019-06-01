//Original: aet.bt Version: 1.2 by samyuu

using System;
using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.MessagePack;
using KKdMainLib.Types;
using MPIO = KKdMainLib.MessagePack.IO;

namespace KKdMainLib
{
    public class Aet
    {
        public Aet()
        { }

        public struct Header
        {
            public int MAINPointer;
            public float Unk;
            public float Duration;
            public float FrameRate;
            public Vector2<int> Resolution;
            public int Always0;
            public int AnimationPointerTableSize;
            public int AnimationPointerTableOffset;
            public int TextureMetadataCount;
            public int TextureMetadataOffset;
            public Vector2<int> Empty;
            public Vector2<int> Unused;
        }

        public struct AnimationPointer
        {
            public int Count;
            public string G;
        }
    }
}
