//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct Mot
    {
        private int i, i0, i1;
        private MotHeader[] MOT;
        private Stream IO;

        public void MOTReader(string file)
        {
            IO = File.OpenReader(file + ".bin");

            i = 0;
            while (true)
                if    (IO.RI64() == 0) break;
                else { IO.RI64(); i++; }
            if (i == 0) return;

            int MOTCount = i;
            MsgPack m = new MsgPack(MOTCount, "Mot");
            MOT = new MotHeader[MOTCount];
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader Mot = ref MOT[i];
                Mot.KeySet    .O = IO.RI32();
                Mot.KeySetTypesOffset = IO.RI32();
                Mot.     KeySetOffset = IO.RI32();
                Mot.BoneInfo  .O = IO.RI32();
            }

            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader Mot = ref MOT[i];

                i0 = 1;
                IO.P = Mot.BoneInfo.O;
                IO.RU16();
                while (IO.RU16() != 0) i0++;

                Mot.BoneInfo.V = new BoneInfo[i0];
                IO.P = Mot.BoneInfo.O;
                for (i0 = 0; i0 < Mot.BoneInfo.V.Length; i0++)
                    Mot.BoneInfo.V[i0].Id = IO.RU16();

                IO.P = Mot.KeySet.O;
                int info = IO.RU16();
                Mot.HighBits = info >> 14;
                Mot.FrameCount = IO.RU16();

                Mot.KeySet.V = new KeySet[info & 0x3FFF];
                IO.P = Mot.KeySetTypesOffset;
                for (i0 = 0; i0 < Mot.KeySet.V.Length; i0++)
                {
                    if (i0 % 8 == 0) i1 = IO.RU16();

                    Mot.KeySet.V[i0] = new KeySet { Type = (KeySetType)((i1 >> (i0 % 8 * 2)) & 0b11) };
                }

                IO.P = Mot.KeySetOffset;
                for (i0 = 0; i0 < Mot.KeySet.V.Length; i0++)
                {
                    ref KeySet Key = ref Mot.KeySet.V[i0];
                    if (Key.Type == KeySetType.Static)
                    {   Key.Keys = new KFT2[1];
                        Key.Keys[0].V = IO.RF32(); }
                    else if (Key.Type == KeySetType.Linear)
                    {
                        Key.Keys = new KFT2[IO.RU16()];
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                            Key.Keys[i1].F = IO.RU16();
                        IO.A(0x4);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                            Key.Keys[i1].V = IO.RF32();
                    }
                    else if (Key.Type == KeySetType.Interpolated)
                    {
                        Key.Keys = new KFT2[IO.RU16()];
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                          Key.Keys[i1].F = IO.RU16();
                        IO.A(0x4);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                        { Key.Keys[i1].V = IO.RF32(); Key.Keys[i1].T = IO.RF32(); }
                    }
                }
            }
            IO.C();
        }

        public void MOTWriter(string file)
        {
            if (MOT == null) return;
            IO = File.OpenWriter(file + ".bin");

            int MOTCount = MOT.Length;
            IO.P = (MOTCount + 1) << 4;
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader Mot = ref MOT[i];
                Mot.KeySet.O = IO.P;
                IO.W((ushort)((Mot.HighBits << 14) | (Mot.KeySet.V.Length & 0x3FFF)));
                IO.W((ushort)Mot.FrameCount);

                Mot.KeySetTypesOffset = IO.P;
                for (i0 = 0, i1 = 0; i0 < Mot.KeySet.V.Length; i0++)
                {
                    i1 |= ((byte)Mot.KeySet.V[i0].Type << (i0 % 8 * 2)) & (0b11 << (i0 % 8 * 2));

                    if (i0 % 8 == 7) { IO.W((ushort)i1); i1 = 0; }
                }
                IO.W((ushort)i1);

                IO.A(0x4);
                Mot.KeySetOffset = IO.P;
                for (i0 = 0; i0 < Mot.KeySet.V.Length; i0++)
                {
                    ref KeySet Key = ref Mot.KeySet.V[i0];
                    if (Key.Type == KeySetType.Static)
                        IO.W(Key.Keys[0].V); 
                    else if (Key.Type == KeySetType.Linear)
                    {
                        IO.W((ushort)Key.Keys.Length);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                            IO.W(Key.Keys[i1].F);
                        IO.A(0x4);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                            IO.W(Key.Keys[i1].V);
                    }
                    else if (Key.Type == KeySetType.Interpolated)
                    {
                        IO.W((ushort)Key.Keys.Length);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                          IO.W(Key.Keys[i1].F);
                        IO.A(0x4);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                        { IO.W(Key.Keys[i1].V); IO.W(Key.Keys[i1].T); }
                    }
                }
                IO.A(0x4);

                Mot.BoneInfo.O = IO.P;
                for (i0 = 0; i0 < Mot.BoneInfo.V.Length; i0++)
                    IO.W((ushort)Mot.BoneInfo.V[i0].Id);
                IO.W((ushort)0);
            }
            IO.A(0x4, true);

            IO.P = 0;
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader Mot = ref MOT[i];
                IO.W(Mot.KeySet    .O);
                IO.W(Mot.KeySetTypesOffset);
                IO.W(Mot.     KeySetOffset);
                IO.W(Mot.BoneInfo  .O);
            }

            IO.C();
        }

        public void MsgPackReader(string file, bool JSON)
        {
            MOT = null;

            MsgPack MsgPack = file.ReadMP(JSON);
            MsgPack MOTS;
            if ((MOTS = MsgPack["MOT"]).NotNull)
            {
                MOT = new MotHeader[MOTS.Array.Length];
                for (int i = 0; i < MOT.Length; i++)
                    MsgPackReader(MOTS.Array[i], ref MOT[i]);
            }
            MOTS.Dispose();
            MsgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            int MOTCount = MOT.Length;
            MsgPack MOTS = new MsgPack(MOTCount, "MOT");
            for (i = 0; i < MOTCount; i++)
                MOTS[i] = MsgPackWriter(ref MOT[i]);
            MOTS.Write(true, file, JSON);
        }

        public void MsgPackReader(MsgPack MOT, ref MotHeader Mot)
        {
            MsgPack Temp = MsgPack.New;
            Mot.HighBits   = MOT.RI32("HighBits"  );
            Mot.FrameCount = MOT.RI32("FrameCount");

            if ((Temp = MOT["KeySets", true]).IsNull) { Temp.Dispose(); return; }

            Mot.KeySet.V = new KeySet[Temp.Array.Length];
            for (i0 = 0; i0 < Mot.KeySet.V.Length; i0++)
            {
                ref KeySet KeySet = ref Mot.KeySet.V[i0];

                if (Temp.Array[i0].Array == null || Temp.Array[i0].Array.Length != 2) continue;
                KeySet.Type = (KeySetType)Temp.Array[i0].Array[0].RI32();

                MsgPack keySet = Temp.Array[i0].Array[1];

                if (keySet.Array == null) { KeySet.Type = 0; continue; }
                else if (KeySet.Type == KeySetType.None) continue;
                else if (KeySet.Type == KeySetType.Static)
                {
                    KeySet.Keys = new KFT2[1];
                         if (keySet.Array == null)              continue;
                    else if (keySet.Array.Length == 0)          continue;
                    else if (keySet.Array[0].Array == null)     continue;
                    else if (keySet.Array[0].Array.Length == 0) continue;
                    else if (keySet.Array[0].Array.Length == 1)
                        KeySet.Keys[0] = new KFT2 (keySet.Array[0][0].RF32());
                    else if (keySet.Array[0].Array.Length >  1)
                        KeySet.Keys[0] = new KFT2 (keySet.Array[0][0].RF32(), keySet.Array[1].RF32());
                }
                else if (KeySet.Type == KeySetType.Linear)
                {
                    KeySet.Keys = new KFT2[keySet.Array.Length];
                    for (i1 = 0; i1 < keySet.Array.Length; i1++)
                    {
                        ref MsgPack Array = ref keySet.Array[i1];
                             if (Array.Array == null)     continue;
                        else if (Array.Array.Length == 0) continue;
                        else if (Array.Array.Length == 1)
                            KeySet.Keys[i1] = new KFT2 (Array[0].RF32());
                        else if (Array.Array.Length == 2)
                            KeySet.Keys[i1] = new KFT2 (Array[0].RF32(), Array[1].RF32());
                    }
                }
                else if (KeySet.Type == KeySetType.Interpolated)
                {
                    KeySet.Keys = new KFT2[keySet.Array.Length];
                    for (i1 = 0; i1 < keySet.Array.Length; i1++)
                    {
                        ref MsgPack Array = ref keySet.Array[i1];
                             if (Array.Array == null ||
                                 Array.Array.Length == 0) continue;
                        else if (Array.Array.Length == 1)
                            KeySet.Keys[i1] = new KFT2 (Array[0].RF32());
                        else if (Array.Array.Length == 2)
                            KeySet.Keys[i1] = new KFT2 (Array[0].RF32(), Array[1].RF32());
                        else if (Array.Array.Length >  2)
                            KeySet.Keys[i1] = new KFT2 (Array[0].RF32(),
                                Array[1].RF32(), keySet.Array[i1][2].RF32());
                    }
                }
            }
            
            if ((Temp = MOT["BoneInfo", true]).NotNull)
            {
                Mot.BoneInfo.V = new BoneInfo[Temp.Array.Length];
                for (i = 0; i < Mot.BoneInfo.V.Length; i++)
                    Mot.BoneInfo.V[i].Id = Temp[i].RI32();
            }
            Temp.Dispose();
        }

        public MsgPack MsgPackWriter(ref MotHeader Mot)
        {
            MsgPack MOT = MsgPack.NewReserve(4).Add("FrameCount", Mot.FrameCount).Add("HighBits", Mot.HighBits);

            MsgPack KeySets = new MsgPack(Mot.KeySet.V.Length, "KeySets");
            for (i0 = 0; i0 < Mot.KeySet.V.Length; i0++)
            {
                ref KeySet KeySet = ref Mot.KeySet.V[i0];
                if (KeySet.Type == KeySetType.None) continue;

                KeySets[i0] = new MsgPack(2);
                KeySets[i0].Array[0] = (byte)KeySet.Type;
                KeySets[i0].Array[1] = new MsgPack(KeySet.Keys.Length);
                if (KeySet.Type == KeySetType.Static)
                {
                    IKF KF = KeySet.Keys[i1].Check();
                         if (KF is KFT0 KFT0) KeySets[i0].Array[1][0] =
                           new MsgPack(null, new MsgPack[] { KFT0.F });
                    else if (KF is KFT1 KFT1) KeySets[i0].Array[1][0] =
                            new MsgPack(null, new MsgPack[] { KFT1.F, KFT1.V });
                }
                else if (KeySet.Type == KeySetType.Linear)
                    for (i1 = 0; i1 < KeySet.Keys.Length; i1++)
                    {
                        IKF KF = KeySet.Keys[i1].Check();
                             if (KF is KFT0 KFT0) KeySets[i0].Array[1][i1] =
                                new MsgPack(null, new MsgPack[] { KFT0.F });
                        else if (KF is KFT1 KFT1) KeySets[i0].Array[1][i1] =
                                new MsgPack(null, new MsgPack[] { KFT1.F, KFT1.V });
                    }
                else
                    for (i1 = 0; i1 < KeySet.Keys.Length; i1++)
                    {
                        IKF KF = KeySet.Keys[i1].Check();
                             if (KF is KFT0 KFT0) KeySets[i0].Array[1][i1] =
                                new MsgPack(null, new MsgPack[] { KFT0.F });
                        else if (KF is KFT1 KFT1) KeySets[i0].Array[1][i1] =
                                new MsgPack(null, new MsgPack[] { KFT1.F, KFT1.V });
                        else if (KF is KFT2 KFT2) KeySets[i0].Array[1][i1] =
                                new MsgPack(null, new MsgPack[] { KFT2.F, KFT2.V, KFT2.T });
                    }
            }
            MOT.Add(KeySets);

            MsgPack BoneInfo = new MsgPack(Mot.BoneInfo.V.Length, "BoneInfo");
            for (i0 = 0; i0 < Mot.BoneInfo.V.Length; i0++)
                BoneInfo[i0] = Mot.BoneInfo.V[i0].Id;
            MOT.Add(BoneInfo);

            return MOT;
        }

        public struct MotHeader
        {
            public int      KeySetOffset;
            public int KeySetTypesOffset;
            public Pointer< KeySet []>  KeySet ;
            public Pointer<BoneInfo[]> BoneInfo;

            public int HighBits;
            public int FrameCount;
        }

        public struct KeySet
        {
            public KFT2[] Keys;
            public KeySetType Type;

            public override string ToString() => $"Type: {Type}" + (Type == KeySetType.Static ?
                $"; Value: {Keys[0].Check()}" : Type > KeySetType.Static ? $"; Keys: {Keys.Length}" : "");
        }

        public enum KeySetType : byte
        {
            None         = 0b00,
            Static       = 0b01,
            Linear       = 0b10,
            Interpolated = 0b11,
        }

        public struct BoneInfo
        {
            public string Name;
            public int Id;
        }
    }
}
