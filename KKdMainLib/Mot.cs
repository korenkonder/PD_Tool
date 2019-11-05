//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct Mot
    {
        private int i, i0, i1;
        public MotHeader[] MOT;
        private Stream _IO;

        public void MOTReader(string file)
        {
            _IO = File.OpenReader(file + ".bin");

            i = 0;
            while (true)
                if    (_IO.RI64() == 0) break;
                else { _IO.RI64(); i++; }
            if (i == 0) return;

            _IO.P = 0;
            int motCount = i;
            MOT = new MotHeader[motCount];
            for (i = 0; i < motCount; i++)
            {
                ref MotHeader mot = ref MOT[i];
                mot.KeySet    .O      = _IO.RI32();
                mot.KeySetTypesOffset = _IO.RI32();
                mot.     KeySetOffset = _IO.RI32();
                mot.BoneInfo  .O      = _IO.RI32();
            }

            for (i = 0; i < motCount; i++)
            {
                ref MotHeader mot = ref MOT[i];

                i0 = 1;
                _IO.P = mot.BoneInfo.O;
                _IO.RU16();
                while (_IO.RU16() != 0) i0++;

                mot.BoneInfo.V = new BoneInfo[i0];
                _IO.P = mot.BoneInfo.O;
                for (i0 = 0; i0 < mot.BoneInfo.V.Length; i0++)
                    mot.BoneInfo.V[i0].Id = _IO.RU16();

                _IO.P = mot.KeySet.O;
                int info = _IO.RU16();
                mot.HighBits = info >> 14;
                mot.FrameCount = _IO.RU16();

                mot.KeySet.V = new KeySet[info & 0x3FFF];
                _IO.P = mot.KeySetTypesOffset;
                for (i0 = 0; i0 < mot.KeySet.V.Length; i0++)
                {
                    if (i0 % 8 == 0) i1 = _IO.RU16();

                    mot.KeySet.V[i0] = new KeySet { Type = (KeySetType)((i1 >> (i0 % 8 * 2)) & 0b11) };
                }

                _IO.P = mot.KeySetOffset;
                for (i0 = 0; i0 < mot.KeySet.V.Length; i0++)
                {
                    ref KeySet key = ref mot.KeySet.V[i0];
                    if (key.Type == KeySetType.Static)
                    {   key.Keys = new KFT2[1];
                        key.Keys[0].V = _IO.RF32(); }
                    else if (key.Type == KeySetType.Linear)
                    {
                        key.Keys = new KFT2[_IO.RU16()];
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                            key.Keys[i1].F = _IO.RU16();
                        _IO.A(0x4);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                            key.Keys[i1].V = _IO.RF32();
                    }
                    else if (key.Type == KeySetType.Interpolated)
                    {
                        key.Keys = new KFT2[_IO.RU16()];
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                          key.Keys[i1].F = _IO.RU16();
                        _IO.A(0x4);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                        { key.Keys[i1].V = _IO.RF32(); key.Keys[i1].T = _IO.RF32(); }
                    }
                }
            }
            _IO.C();
        }

        public void MOTWriter(string file)
        {
            if (MOT == null) return;
            _IO = File.OpenWriter(file + ".bin");

            int MOTCount = MOT.Length;
            _IO.P = (MOTCount + 1) << 4;
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader mot = ref MOT[i];
                mot.KeySet.O = _IO.P;
                _IO.W((ushort)((mot.HighBits << 14) | (mot.KeySet.V.Length & 0x3FFF)));
                _IO.W((ushort)mot.FrameCount);

                mot.KeySetTypesOffset = _IO.P;
                for (i0 = 0, i1 = 0; i0 < mot.KeySet.V.Length; i0++)
                {
                    i1 |= ((byte)mot.KeySet.V[i0].Type << (i0 % 8 * 2)) & (0b11 << (i0 % 8 * 2));

                    if (i0 % 8 == 7) { _IO.W((ushort)i1); i1 = 0; }
                }
                _IO.W((ushort)i1);

                _IO.A(0x4);
                mot.KeySetOffset = _IO.P;
                for (i0 = 0; i0 < mot.KeySet.V.Length; i0++)
                {
                    ref KeySet key = ref mot.KeySet.V[i0];
                    if (key.Type == KeySetType.Static)
                        _IO.W(key.Keys[0].V); 
                    else if (key.Type == KeySetType.Linear)
                    {
                        _IO.W((ushort)key.Keys.Length);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                            _IO.W(key.Keys[i1].F);
                        _IO.A(0x4);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                            _IO.W(key.Keys[i1].V);
                    }
                    else if (key.Type == KeySetType.Interpolated)
                    {
                        _IO.W((ushort)key.Keys.Length);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                          _IO.W(key.Keys[i1].F);
                        _IO.A(0x4);
                        for (i1 = 0; i1 < key.Keys.Length; i1++)
                        { _IO.W(key.Keys[i1].V); _IO.W(key.Keys[i1].T); }
                    }
                }
                _IO.A(0x4);

                mot.BoneInfo.O = _IO.P;
                for (i0 = 0; i0 < mot.BoneInfo.V.Length; i0++)
                    _IO.W((ushort)mot.BoneInfo.V[i0].Id);
                _IO.W((ushort)0);
            }
            _IO.A(0x4, true);

            _IO.P = 0;
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader mot = ref MOT[i];
                _IO.W(mot.KeySet    .O     );
                _IO.W(mot.KeySetTypesOffset);
                _IO.W(mot.     KeySetOffset);
                _IO.W(mot.BoneInfo  .O     );
            }

            _IO.C();
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
            mot.Write(true, file, json);
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
                         if (temp1.Array == null)              continue;
                    else if (temp1.Array.Length == 0)          continue;
                    else if (temp1.Array[0].Array == null)     continue;
                    else if (temp1.Array[0].Array.Length == 0) continue;
                    else if (temp1.Array[0].Array.Length == 1)
                        keySet.Keys[0] = new KFT2 (temp1.Array[0][0].RF32());
                    else if (temp1.Array[0].Array.Length >  1)
                        keySet.Keys[0] = new KFT2 (temp1.Array[0][0].RF32(), temp1.Array[1].RF32());
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
                            keySet.Keys[i1] = new KFT2 (array[0].RF32());
                        else if (array.Array.Length == 2)
                            keySet.Keys[i1] = new KFT2 (array[0].RF32(), array[1].RF32());
                    }
                }
                else if (keySet.Type == KeySetType.Interpolated)
                {
                    keySet.Keys = new KFT2[temp1.Array.Length];
                    for (i1 = 0; i1 < temp1.Array.Length; i1++)
                    {
                        ref MsgPack array = ref temp1.Array[i1];
                             if (array.Array == null ||
                                 array.Array.Length == 0) continue;
                        else if (array.Array.Length == 1)
                            keySet.Keys[i1] = new KFT2 (array[0].RF32());
                        else if (array.Array.Length == 2)
                            keySet.Keys[i1] = new KFT2 (array[0].RF32(), array[1].RF32());
                        else if (array.Array.Length >  2)
                            keySet.Keys[i1] = new KFT2 (array[0].RF32(),
                                array[1].RF32(), temp1.Array[i1][2].RF32());
                    }
                }
            }
            
            if ((temp = msgPack["BoneInfo", true]).NotNull)
            {
                mot.BoneInfo.V = new BoneInfo[temp.Array.Length];
                for (i = 0; i < mot.BoneInfo.V.Length; i++)
                    mot.BoneInfo.V[i].Id = temp[i].RI32();
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
                keySets[i0].Array[1] = new MsgPack(keySet.Keys.Length);
                if (keySet.Type == KeySetType.Static)
                {
                    IKF kf = keySet.Keys[0].Check();
                         if (kf is KFT0 KFT0) keySets[i0].Array[1][0] =
                            new MsgPack(null, new MsgPack[] { KFT0.F });
                    else if (kf is KFT1 KFT1) keySets[i0].Array[1][0] =
                            new MsgPack(null, new MsgPack[] { KFT1.F, KFT1.V });
                }
                else if (keySet.Type == KeySetType.Linear)
                    for (i1 = 0; i1 < keySet.Keys.Length; i1++)
                    {
                        IKF kf = keySet.Keys[i1].Check();
                             if (kf is KFT0 KFT0) keySets[i0].Array[1][i1] =
                                new MsgPack(null, new MsgPack[] { KFT0.F });
                        else if (kf is KFT1 KFT1) keySets[i0].Array[1][i1] =
                                new MsgPack(null, new MsgPack[] { KFT1.F, KFT1.V });
                    }
                else
                    for (i1 = 0; i1 < keySet.Keys.Length; i1++)
                    {
                        IKF kf = keySet.Keys[i1].Check();
                             if (kf is KFT0 KFT0) keySets[i0].Array[1][i1] =
                                new MsgPack(null, new MsgPack[] { KFT0.F });
                        else if (kf is KFT1 KFT1) keySets[i0].Array[1][i1] =
                                new MsgPack(null, new MsgPack[] { KFT1.F, KFT1.V });
                        else if (kf is KFT2 KFT2) keySets[i0].Array[1][i1] =
                                new MsgPack(null, new MsgPack[] { KFT2.F, KFT2.V, KFT2.T });
                    }
            }
            mot.Add(keySets);

            MsgPack boneInfo = new MsgPack(Mot.BoneInfo.V.Length, "BoneInfo");
            for (i0 = 0; i0 < Mot.BoneInfo.V.Length; i0++)
                boneInfo[i0] = Mot.BoneInfo.V[i0].Id;
            mot.Add(boneInfo);

            return mot;
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
