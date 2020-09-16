//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib.DB
{
    public class Aet : System.IDisposable
    {
        private int i, i0, i1, i2;
        private Stream s;

        public AetSet[] AetSets;

        public void BINReader(string file)
        {
            s = File.OpenReader(file + ".bin");

            int aetSetsLength = s.RI32();
            int aetSetsOffset = s.RI32();
            int aetsLength    = s.RI32();
            int aetsOffset    = s.RI32();

            s.P = aetSetsOffset;
            AetSets = new AetSet[aetSetsLength];
            for (i = 0; i < aetSetsLength; i++)
            {
                AetSets[i].Id          = s.RI32();
                AetSets[i].Name        = s.RSaO();
                AetSets[i].FileName    = s.RSaO();
                s.RI32();
                AetSets[i].SpriteSetId = s.RI32();
            }

            int setIndex;
            AET aet = new AET();
            int[] AetCount = new int[aetSetsLength];

            s.P = aetsOffset;
            for (i = 0; i < aetsLength; i++)
            {
                s.PI64 += 10;
                setIndex = s.RI16();
                AetCount[setIndex]++;
            }

            for (int i = 0; i < aetSetsLength; i++)
            {
                AetSets[i].Aets  = new AET[AetCount[i]];
                AetCount[i] = 0;
            }

            s.P = aetsOffset;
            for (i = 0; i < aetsLength; i++)
            {
                aet.Id    = s.RI32();
                aet.Name  = s.RSaO();
                s.RI16();
                setIndex  = s.RI16();

                AetSets[setIndex].Aets[AetCount[setIndex]] = aet; AetCount[setIndex]++;
            }

            s.C();
        }


        public void BINWriter(string file)
        {
            if (AetSets        == null) return;
            if (AetSets.Length ==    0) return;

            KKdList<string> setName     = KKdList<string>.New;
            KKdList<string> setFileName = KKdList<string>.New;

            KKdList<int>    ids = KKdList<int>.New;
            KKdList<int> setIds = KKdList<int>.New;

            KKdList<int> notAdd = KKdList<int>.New;
            AET temp;
            AetSet set;

            for (i = 0; i < AetSets.Length; i++)
            {
                set = AetSets[i];
                if (set.    Name  != null)
                    if  (setName    .Contains(set.    Name)) { notAdd.Add(i); continue; }
                    else setName    .Add     (set.    Name);
                if (set.FileName  != null)
                    if  (setFileName.Contains(set.FileName)) { notAdd.Add(i); continue; }
                    else setFileName.Add     (set.FileName);

                if (set.NewId)
                {
                    AetSets[i].Id = null;
                    for (i0 = 0; i0 < set.Aets.Length; i0++) AetSets[i].Aets[i0].Id = null;
                    continue;
                }

                if (set.Id    != null)
                    if  (setIds.Contains((int)set.Id)) { notAdd.Add(i); continue; }
                    else setIds.Add     ((int)set.Id);

                for (i0 = 0; i0 < set.Aets.Length; i0++)
                {
                    temp = set.Aets[i0];
                    if (temp.Id != null)
                        if ( ids.Contains((int)temp.Id)) { notAdd.Add(i); break; }
                        else ids.Add     ((int)temp.Id);
                }
            }
            setName    .Dispose();
            setFileName.Dispose();

            for (i = 0; i < AetSets.Length; i++)
            {
                set = AetSets[i];
                if (notAdd.Contains(i)) continue;
                if (!set.NewId) continue;

                i1 = 0;
                if (set.Id    == null) while (true)
                    { if (!setIds.Contains(i1)) { AetSets[i].Id = i1; setIds.Add(i1); break; } i1++; }

                for (i0 = 0, i1 = 0; i0 < set.Aets.Length; i0++)
                    if (set.Aets[i0].Id == null) while (true) { if (!ids.Contains(i1))
                            { AetSets[i].Aets[i0].Id = i1; ids.Add(i1); break; } i1++; }
            }
               ids.Dispose();
            setIds.Dispose();

            for (i = 0, i0 = 0, i2 = 0; i < AetSets.Length; i++)
                if (!notAdd.Contains(i)) { i0 += AetSets[i].Aets.Length; i2++; }

            i1 = i0 * 12;
            i1 = i1.A(0x20) + 0x20;

            s = File.OpenWriter(file + ".bin", true);
            s.W(i2);
            s.W(i1);
            s.W(i0);
            s.W(0x20);
            s.W(0x9066906690669066);
            s.W(0x9066906690669066);

            s.P = (i1 + i2 * 0x14).A(0x20);
            for (i = 0; i < AetSets.Length; i++)
            {
                if (notAdd.Contains(i)) continue;
                AetSets[i].    NameOffset = s.P; s.W(AetSets[i].    Name + "\0");
                AetSets[i].FileNameOffset = s.P; s.W(AetSets[i].FileName + "\0");
            }

            for (i = 0; i < AetSets.Length; i++)
            {
                if (notAdd.Contains(i)) continue;
                for (i0 = 0; i0 < AetSets[i].Aets.Length; i0++)
                { AetSets[i].Aets[i0].NameOffset = s.P; s.W(AetSets[i].Aets[i0].Name + "\0"); }
            }
            s.A(0x08, true);

            s.P = 0x20;
            for (i = 0, i2 = 0; i < AetSets.Length; i++)
            {
                if (notAdd.Contains(i)) { i2++; continue; }

                for (i0 = 0; i0 < AetSets[i].Aets.Length; i0++)
                {
                    s.W(AetSets[i].Aets[i0].Id        );
                    s.W(AetSets[i].Aets[i0].NameOffset);
                    s.W((ushort)     i0 );
                    s.W((ushort)(i - i2));
                }
            }
            s.A(0x20);

            for (i = 0, i2 = 0; i < AetSets.Length; i++)
            {
                if (notAdd.Contains(i)) { i2++; continue; }

                s.W(AetSets[i].Id            );
                s.W(AetSets[i].    NameOffset);
                s.W(AetSets[i].FileNameOffset);
                s.W(i - i2);
                s.W(AetSets[i].SpriteSetId   );
            }
            notAdd.Dispose();
            s.C();
        }

        public void MsgPackReader(string file, bool json)
        {
            MsgPack MsgPack = file.ReadMPAllAtOnce(json);

            MsgPack Temp = default;
            if ((Temp = MsgPack["AetDB", true]).NotNull)
            {
                AetSets = new AetSet[Temp.Array.Length];
                for (int i = 0; i < AetSets.Length; i++)
                    AetSets[i].ReadMsgPack(Temp[i]);
            }
            Temp.Dispose();
            MsgPack.Dispose();
        }


        public void MsgPackWriter(string file, bool json)
        {
            if (AetSets        == null) return;
            if (AetSets.Length ==    0) return;

            MsgPack AetDB = new MsgPack(AetSets.Length, "AetDB");
            for (i = 0; i < AetSets.Length; i++)
                AetDB[i] = AetSets[i].WriteMsgPack();

            AetDB.Write(false, true, file, json);
        }

        private bool disposed = false;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; AetSets = null; disposed = true; } }

        public struct AET
        {
            public int NameOffset;
            public int? Id;
            public string Name;

            public void ReadMsgPack(MsgPack msg)
            { Id = msg.RnU16("Id"); Name  = msg.RS("Name"); }

            public MsgPack WriteMsgPack() =>
                MsgPack.New.Add("Id", Id).Add("Name", Name);
        }

        public struct AetSet
        {
            public int NameOffset;
            public int FileNameOffset;
            public int? Id;
            public int? SpriteSetId;
            public bool NewId;
            public string Name;
            public string FileName;
            public AET[] Aets;

            public void ReadMsgPack(MsgPack msg)
            {
                FileName    = msg.RS   ("FileName"   );
                Id          = msg.RnU16(         "Id");
                Name        = msg.RS   (    "Name"   );
                NewId       = msg.RB   ("NewId"      );
                SpriteSetId = msg.RnU16("SpriteSetId");

                MsgPack Temp;
                if ((Temp = msg["Aets", true]).NotNull)
                {
                    Aets = new AET[Temp.Array.Length];
                    for (int i0 = 0; i0 < Aets.Length; i0++)
                        Aets[i0].ReadMsgPack(Temp[i0]);
                }
                Temp.Dispose();
            }

            public MsgPack WriteMsgPack()
            {
                MsgPack Aets = new MsgPack(this.Aets.Length, "Aets");
                for (int i0 = 0; i0 < this.Aets.Length; i0++)
                    Aets[i0] = this.Aets[i0].WriteMsgPack();

                return MsgPack.New.Add("FileName", FileName).Add("Id", Id)
                    .Add("Name", Name).Add("SpriteSetId", SpriteSetId).Add(Aets);
            }
        }
    }
}
