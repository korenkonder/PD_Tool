//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using System.Collections.Generic;
using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib.DB
{
    public class Aet : System.IDisposable
    {
        private int i, i0, i1, i2;
        private Stream _IO;

        public AetSet[] AetSets;

        public void BINReader(string file)
        {
            _IO = File.OpenReader(file + ".bin");

            int aetSetsLength = _IO.RI32();
            int aetSetsOffset = _IO.RI32();
            int aetsLength    = _IO.RI32();
            int aetsOffset    = _IO.RI32();

            _IO.P = aetSetsOffset;
            AetSets = new AetSet[aetSetsLength];
            for (i = 0; i < aetSetsLength; i++)
            {
                AetSets[i].Id          = _IO.RI32();
                AetSets[i].Name        = _IO.RSaO();
                AetSets[i].FileName    = _IO.RSaO();
                _IO.RI32();
                AetSets[i].SpriteSetId = _IO.RI32();
            }

            int setIndex;
            AET aet = new AET();
            int[] AetCount = new int[aetSetsLength];

            _IO.P = aetsOffset;
            for (i = 0; i < aetsLength; i++)
            {
                _IO.I64P += 10;
                setIndex = _IO.RI16();
                AetCount[setIndex]++;
            }

            for (int i = 0; i < aetSetsLength; i++)
            {
                AetSets[i].Aets  = new AET[AetCount[i]];
                AetCount[i] = 0;
            }

            _IO.P = aetsOffset;
            for (i = 0; i < aetsLength; i++)
            {
                aet.Id    = _IO.RI32();
                aet.Name  = _IO.RSaO();
                _IO.RI16();
                setIndex  = _IO.RI16();

                AetSets[setIndex].Aets[AetCount[setIndex]] = aet; AetCount[setIndex]++;
            }

            _IO.C();
        }


        public void BINWriter(string file)
        {
            if (AetSets        == null) return;
            if (AetSets.Length ==    0) return;

            List<string> SetName     = new List<string>();
            List<string> SetFileName = new List<string>();

            List<int>    Ids        = new List<int>();
            List<int> SetIds        = new List<int>();

            List<int> NotAdd = new List<int>();
            AET temp;
            AetSet set;

            for (i = 0; i < AetSets.Length; i++)
            {
                set = AetSets[i];
                if (set.    Name  != null)
                    if  (SetName    .Contains(set.    Name)) { NotAdd.Add(i); continue; }
                    else SetName    .Add     (set.    Name);
                if (set.FileName  != null)
                    if  (SetFileName.Contains(set.FileName)) { NotAdd.Add(i); continue; }
                    else SetFileName.Add     (set.FileName);

                if (set.NewId)
                {
                    AetSets[i].Id = null;
                    for (i0 = 0; i0 < set.Aets.Length; i0++) AetSets[i].Aets[i0].Id = null;
                    continue;
                }

                if (set.Id    != null)
                    if  (SetIds.Contains((int)set.Id)) { NotAdd.Add(i); continue; }
                    else SetIds.Add     ((int)set.Id);

                for (i0 = 0; i0 < set.Aets.Length; i0++)
                {
                    temp = set.Aets[i0];
                    if (temp.Id != null)
                        if ( Ids.Contains((int)temp.Id)) { NotAdd.Add(i); break; }
                        else Ids.Add     ((int)temp.Id);
                }
            }
            SetName     = null;
            SetFileName = null;

            for (i = 0; i < AetSets.Length; i++)
            {
                set = AetSets[i];
                if (NotAdd.Contains(i)) continue;
                if (!set.NewId) continue;

                i1 = 0;
                if (set.Id    == null) while (true)
                    { if (!SetIds.Contains(i1)) { AetSets[i].Id = i1; SetIds.Add(i1); break; } i1++; }

                for (i0 = 0, i1 = 0; i0 < set.Aets.Length; i0++)
                    if (set.Aets[i0].Id == null) while (true) { if (!Ids.Contains(i1))
                            { AetSets[i].Aets[i0].Id = i1; Ids.Add(i1); break; } i1++; }
            }

               Ids = null;
            SetIds = null;

            for (i = 0, i0 = 0, i2 = 0; i < AetSets.Length; i++)
                if (!NotAdd.Contains(i)) { i0 += AetSets[i].Aets.Length; i2++; }
            
            i1 = i0 * 12;
            i1 = i1.A(0x20) + 0x20;

            _IO = File.OpenWriter(file + ".bin", true);
            _IO.W(i2);
            _IO.W(i1);
            _IO.W(i0);
            _IO.W(0x20);
            _IO.W(0x9066906690669066);
            _IO.W(0x9066906690669066);

            _IO.P = (i1 + i2 * 0x14).A(0x20);
            for (i = 0; i < AetSets.Length; i++)
            {
                if (NotAdd.Contains(i)) continue;
                AetSets[i].    NameOffset = _IO.P; _IO.W(AetSets[i].    Name + "\0");
                AetSets[i].FileNameOffset = _IO.P; _IO.W(AetSets[i].FileName + "\0");
            }

            for (i = 0; i < AetSets.Length; i++)
            {
                if (NotAdd.Contains(i)) continue;
                for (i0 = 0; i0 < AetSets[i].Aets.Length; i0++)
                { AetSets[i].Aets[i0].NameOffset = _IO.P; _IO.W(AetSets[i].Aets[i0].Name + "\0"); }
            }
            _IO.A(0x08, true);
            
            _IO.P = 0x20;
            for (i = 0, i2 = 0; i < AetSets.Length; i++)
            {
                if (NotAdd.Contains(i)) { i2++; continue; }
                
                for (i0 = 0; i0 < AetSets[i].Aets.Length; i0++)
                {
                    _IO.W(AetSets[i].Aets[i0].Id        );
                    _IO.W(AetSets[i].Aets[i0].NameOffset);
                    _IO.W((ushort)     i0 );
                    _IO.W((ushort)(i - i2));
                }
            }
            _IO.A(0x20);
            for (i = 0, i2 = 0; i < AetSets.Length; i++)
            {
                if (NotAdd.Contains(i)) { i2++; continue; }

                _IO.W(AetSets[i].Id            );
                _IO.W(AetSets[i].    NameOffset);
                _IO.W(AetSets[i].FileNameOffset);
                _IO.W(i - i2);
                _IO.W(AetSets[i].SpriteSetId   );
            }
            _IO.C();
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

            AetDB.Write(true, file, json);
        }

        private bool disposed = false;
        public void Dispose()
        { if (!disposed) { if (_IO != null) _IO.D(); AetSets = null; disposed = true; } }

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
                FileName    = msg.RS ("FileName"   );
                Id          = msg.RnU16(         "Id");
                Name        = msg.RS (    "Name"   );
                NewId       = msg.RB("NewId"      );
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
