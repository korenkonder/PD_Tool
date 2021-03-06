//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct Mot : System.IDisposable
    {
        private int i, i0, i1;
        public MotHeader[] MOT;
        private Stream s;

        [System.ThreadStatic] public bool Modern;

        public void MOTReader(string file)
        {
            s = File.OpenReader(file + ".bin");

            i = 0;
            while (true)
                if    (s.RI64() == 0) break;
                else { s.RI64(); i++; }
            if (i == 0) return;

            s.P = 0;
            int motCount = i;
            MOT = new MotHeader[motCount];
            for (i = 0; i < motCount; i++)
            {
                ref MotHeader mot = ref MOT[i];
                mot.KeySet    .O      = s.RI32();
                mot.KeySetTypesOffset = s.RI32();
                mot.     KeySetOffset = s.RI32();
                mot.BoneInfo  .O      = s.RI32();
            }

            for (i = 0; i < motCount; i++)
            {
                ref MotHeader mot = ref MOT[i];

                i0 = 1;
                s.P = mot.BoneInfo.O;
                s.RU16();
                while (s.P < s.L && s.RU16() != 0) i0++;

                mot.BoneInfo.V = new int[i0];
                s.P = mot.BoneInfo.O;
                for (i0 = 0; i0 < mot.BoneInfo.V.Length; i0++)
                    mot.BoneInfo.V[i0] = s.RU16();

                s.P = mot.KeySet.O;
                int info = s.RU16();
                mot.HighBits = info >> 14;
                mot.FrameCount = s.RU16();

                mot.KeySet.V = new KeySet[info & 0x3FFF];
                s.P = mot.KeySetTypesOffset;
                for (i0 = 0; i0 < mot.KeySet.V.Length; i0++)
                {
                    if (i0 % 8 == 0) i1 = s.RU16();
                    mot.KeySet.V[i0] = new KeySet { Type = (KeySetType)((i1 >> (i0 % 8 * 2)) & 0b11) };
                }

                s.P = mot.KeySetOffset;
                for (i0 = 0; i0 < mot.KeySet.V.Length; i0++)
                {
                    ref KeySet key = ref mot.KeySet.V[i0];
                    if (key.Type == KeySetType.Static)
                    {   key.Keys = new KFT2[1];
                        key.Keys[0].V = s.RF32(); }
                    else if (key.Type == KeySetType.Linear && !Modern)
                    {
                        key.Keys = new KFT2[s.RU16()];
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                            key.Keys[i1].F = s.RU16();
                        s.A(0x4);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                            key.Keys[i1].V = s.RF32();
                    }
                    else if (key.Type == KeySetType.Tangent && !Modern)
                    {
                        key.Keys = new KFT2[s.RU16()];
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                          key.Keys[i1].F = s.RU16();
                        s.A(0x4);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                        { key.Keys[i1].V = s.RF32(); key.Keys[i1].T = s.RF32(); }
                    }
                    else if (key.Type != KeySetType.None)
                    {
                        key.Keys = new KFT2[s.RU16()];
                        ushort type = s.RU16();
                        if (key.Type == KeySetType.Tangent)
                            for (i1 = 0; i1 < key.Keys.Length; i1++)
                                key.Keys[i1].T = s.RF32();

                        if (type == 1)
                        {
                            for (i1 = 0; i1 < key.Keys.Length; i1++)
                                key.Keys[i1].V = s.RF16();
                            s.A(0x4);
                        }
                        else
                            for (i1 = 0; i1 < key.Keys.Length; i1++)
                                key.Keys[i1].V = s.RF32();
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                            key.Keys[i1].F = s.RU16();
                        s.A(0x4);
                    }
                }
            }
            s.C();
        }

        public void MOTWriter(string file)
        {
            if (MOT == null) return;
            s = File.OpenWriter(file + ".bin", true);

            int MOTCount = MOT.Length;
            s.P = (MOTCount + 1) << 4;
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader mot = ref MOT[i];
                mot.KeySet.O = s.P;
                s.W((ushort)((mot.HighBits << 14) | (mot.KeySet.V.Length & 0x3FFF)));
                s.W((ushort)mot.FrameCount);

                mot.KeySetTypesOffset = s.P;
                for (i0 = 0, i1 = 0; i0 < mot.KeySet.V.Length; i0++)
                {
                    i1 |= ((byte)mot.KeySet.V[i0].Type << (i0 % 8 * 2)) & (0b11 << (i0 % 8 * 2));

                    if (i0 % 8 == 7) { s.W((ushort)i1); i1 = 0; }
                }
                s.W((ushort)i1);
                if (s.P % 4 != 0) s.W((ushort)0x00);

                mot.KeySetOffset = s.P;
                for (i0 = 0; i0 < mot.KeySet.V.Length; i0++)
                {
                    ref KeySet key = ref mot.KeySet.V[i0];
                    if (key.Type == KeySetType.Static)
                        s.W(key.Keys[0].V);
                    else if ((key.Type == KeySetType.Linear
                        || key.Type == KeySetType.Tangent) && !Modern)
                    {
                        s.W((ushort)key.Keys.Length);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                            s.W((ushort)key.Keys[i1].F);
                        if (s.P % 4 != 0) s.W((ushort)0x00);
                        if (key.Type == KeySetType.Tangent)
                            for (i1 = 0; i1 < key.Keys.Length; i1++)
                            { s.W(key.Keys[i1].V); s.W(key.Keys[i1].T); }
                        else
                            for (i1 = 0; i1 < key.Keys.Length; i1++)
                                s.W(key.Keys[i1].V);
                    }
                    else if (key.Type != KeySetType.None)
                    {
                        s.W((ushort)key.Keys.Length);
                        s.W((ushort)0);
                        if (key.Type == KeySetType.Tangent)
                            for (i1 = 0; i1 < key.Keys.Length; i1++)
                                s.W(key.Keys[i1].T);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                            s.W(key.Keys[i1].V);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                            s.W((ushort)key.Keys[i1].F);
                        s.A(0x4);
                    }
                }
                if (s.P % 4 != 0) s.W((ushort)0x00);

                mot.BoneInfo.O = s.P;
                for (i0 = 0; i0 < mot.BoneInfo.V.Length; i0++)
                    s.W((ushort)mot.BoneInfo.V[i0]);
                s.W((ushort)0);
            }
            if (s.P % 4 != 0) s.W((ushort)0x00);
            s.A(0x4, true);

            s.P = 0;
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader mot = ref MOT[i];
                s.W(mot.KeySet    .O     );
                s.W(mot.KeySetTypesOffset);
                s.W(mot.     KeySetOffset);
                s.W(mot.BoneInfo  .O     );
            }

            s.C();
        }

        public void MsgPackReader(string file, bool json)
        {
            MOT = null;

            MsgPack msgPack = file.ReadMP(json);
            MsgPack mot;
            if ((mot = msgPack["MOT"]).NotNull)
            {
                MOT = new MotHeader[mot.Array.Length];
                for (int i = 0; i < MOT.Length; i++)
                    MsgPackReader(mot.Array[i], ref MOT[i]);
            }
            mot.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            int motCount = MOT.Length;
            MsgPack mot = new MsgPack(motCount, "MOT");
            for (i = 0; i < motCount; i++)
                mot[i] = MsgPackWriter(ref MOT[i]);
            mot.Write(false, true, file, json);
        }

        public void MsgPackReader(MsgPack msgPack, ref MotHeader mot)
        {
            MsgPack temp = MsgPack.New;
            mot.HighBits   = msgPack.RI32("HighBits"  );
            mot.FrameCount = msgPack.RI32("FrameCount");

            if ((temp = msgPack["KeySets", true]).IsNull) { temp.Dispose(); return; }

            mot.KeySet.V = new KeySet[temp.Array.Length];
            for (i0 = 0; i0 < mot.KeySet.V.Length; i0++)
            {
                ref KeySet keySet = ref mot.KeySet.V[i0];

                if (temp.Array[i0].Array == null || temp.Array[i0].Array.Length != 2) continue;
                keySet.Type = (KeySetType)temp.Array[i0].Array[0].RI32();

                MsgPack temp1 = temp.Array[i0].Array[1];

                if (temp1.Array == null) { keySet.Type = 0; continue; }
                else if (keySet.Type == KeySetType.None) continue;
                else if (keySet.Type == KeySetType.Static)
                {
                    keySet.Keys = new KFT2[1];
                         if (temp1.Array == null)     continue;
                    else if (temp1.Array.Length == 0) continue;
                    else keySet.Keys[0] = new KFT2(0.0f, temp1.Array[0].RF32());
                }
                else if (keySet.Type == KeySetType.Linear)
                {
                    keySet.Keys = new KFT2[temp1.Array.Length];
                    for (i1 = 0; i1 < temp1.Array.Length; i1++)
                    {
                        ref MsgPack array = ref temp1.Array[i1];
                             if (array.Array == null)     continue;
                        else if (array.Array.Length == 0) continue;
                        else if (array.Array.Length == 1)
                            keySet.Keys[i1] = new KFT2(array[0].RF32());
                        else if (array.Array.Length == 2)
                            keySet.Keys[i1] = new KFT2(array[0].RF32(), array[1].RF32());
                    }
                }
                else if (keySet.Type == KeySetType.Tangent)
                {
                    keySet.Keys = new KFT2[temp1.Array.Length];
                    for (i1 = 0; i1 < temp1.Array.Length; i1++)
                    {
                        ref MsgPack array = ref temp1.Array[i1];
                             if (array.Array == null ||
                                 array.Array.Length == 0) continue;
                        else if (array.Array.Length == 1)
                            keySet.Keys[i1] = new KFT2(array[0].RF32());
                        else if (array.Array.Length == 2)
                            keySet.Keys[i1] = new KFT2(array[0].RF32(), array[1].RF32());
                        else if (array.Array.Length >  2)
                            keySet.Keys[i1] = new KFT2(array[0].RF32(),
                                array[1].RF32(), temp1.Array[i1][2].RF32());
                    }
                }
            }

            if ((temp = msgPack["BoneInfo", true]).NotNull)
            {
                mot.BoneInfo.V = new int[temp.Array.Length];
                for (i = 0; i < mot.BoneInfo.V.Length; i++)
                    mot.BoneInfo.V[i] = temp[i].RI32();
            }
            temp.Dispose();
        }

        public MsgPack MsgPackWriter(ref MotHeader Mot)
        {
            MsgPack mot = MsgPack.NewReserve(4).Add("FrameCount", Mot.FrameCount).Add("HighBits", Mot.HighBits);

            MsgPack keySets = new MsgPack(Mot.KeySet.V.Length, "KeySets");
            for (i0 = 0; i0 < Mot.KeySet.V.Length; i0++)
            {
                ref KeySet keySet = ref Mot.KeySet.V[i0];
                if (keySet.Type == KeySetType.None) continue;

                keySets[i0] = new MsgPack(2);
                keySets[i0].Array[0] = (byte)keySet.Type;
                if (keySet.Type == KeySetType.Static)
                {
                    IKF kf = keySet.Keys[0].Check();
                    MsgPack k = default;
                         if (kf is KFT0 KFT0) k = new MsgPack(null, new MsgPack[] { 0.0f });
                    else if (kf is KFT1 KFT1) k = new MsgPack(null, new MsgPack[] { KFT1.V });
                    keySets[i0].Array[1] = k;
                }
                else if (keySet.Type == KeySetType.Linear)
                {
                    MsgPack k = new MsgPack(keySet.Keys.Length);
                    for (i1 = 0; i1 < keySet.Keys.Length; i1++)
                    {
                        IKF kf = keySet.Keys[i1].Check();
                             if (kf is KFT0 KFT0) k[i1] = new MsgPack(null, new MsgPack[] { KFT0.F });
                        else if (kf is KFT1 KFT1) k[i1] = new MsgPack(null, new MsgPack[] { KFT1.F, KFT1.V });
                    }
                    keySets[i0].Array[1] = k;
                }
                else
                {
                    MsgPack k = new MsgPack(keySet.Keys.Length);
                    for (i1 = 0; i1 < keySet.Keys.Length; i1++)
                    {
                        IKF kf = keySet.Keys[i1].Check();
                             if (kf is KFT0 KFT0) k[i1] = new MsgPack(null, new MsgPack[] { KFT0.F });
                        else if (kf is KFT1 KFT1) k[i1] = new MsgPack(null, new MsgPack[] { KFT1.F, KFT1.V });
                        else if (kf is KFT2 KFT2) k[i1] = new MsgPack(null, new MsgPack[] { KFT2.F, KFT2.V, KFT2.T });
                    }
                    keySets[i0].Array[1] = k;
                }
            }
            mot.Add(keySets);

            MsgPack boneInfo = new MsgPack(Mot.BoneInfo.V.Length, "BoneInfo");
            for (i0 = 0; i0 < Mot.BoneInfo.V.Length; i0++)
                boneInfo[i0] = Mot.BoneInfo.V[i0];
            mot.Add(boneInfo);

            return mot;
        }

        public void Dispose()
        {
            s = null;
            MOT = null;
        }

        public struct MotHeader
        {
            public int      KeySetOffset;
            public int KeySetTypesOffset;
            public Pointer<KeySet[]>  KeySet ;
            public Pointer<   int[]> BoneInfo;

            public int HighBits;
            public int FrameCount;
        }

        public struct KeySet
        {
            public KFT2[] Keys;
            public KeySetType Type;

            public override string ToString() =>
                $"Type: {Type}" + (Type == KeySetType.Static ? $"; Value: {Keys[0].V}"
                : Type > KeySetType.Static ? $"; Keys: {Keys.Length}; First Key: {Keys[0]}" : "");
        }

        public enum KeySetType : byte
        {
            None    = 0b00,
            Static  = 0b01,
            Linear  = 0b10,
            Tangent = 0b11,
        }
    }
}
