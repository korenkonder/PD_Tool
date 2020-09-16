//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib.DB
{
    public class Spr : System.IDisposable
    {
        private int i, i0, i1, i2;
        private Stream s;

        public SpriteSet[] SpriteSets;

        public void BINReader(string file)
        {
            s = File.OpenReader(file + ".bin");

            int spriteSetsLength = s.RI32();
            int spriteSetsOffset = s.RI32();
            int spritesLength    = s.RI32();
            int spritesOffset    = s.RI32();

            s.P = spriteSetsOffset;
            SpriteSets = new SpriteSet[spriteSetsLength];
            for (i = 0; i < spriteSetsLength; i++)
            {
                SpriteSets[i].Id       = s.RI32();
                SpriteSets[i].Name     = s.RSaO();
                SpriteSets[i].FileName = s.RSaO();
                s.RI32();
            }

            int setIndex;
            bool isTexture;
            SpriteTexture st = new SpriteTexture();
            int[] sprCount = new int[spriteSetsLength];
            int[] texCount = new int[spriteSetsLength];

            s.P = spritesOffset;
            for (i = 0; i < spritesLength; i++)
            {
                s.PI64 += 10;
                setIndex = s.RI16();
                isTexture = (setIndex & 0x1000) == 0x1000;
                setIndex &= 0xFFF;

                if (isTexture) texCount[setIndex]++;
                else           sprCount[setIndex]++;
            }

            for (int i = 0; i < spriteSetsLength; i++)
            {
                SpriteSets[i].Sprites  = new SpriteTexture[sprCount[i]];
                SpriteSets[i].Textures = new SpriteTexture[texCount[i]];

                sprCount[i] = 0;
                texCount[i] = 0;
            }

            s.P = spritesOffset;
            for (i = 0; i < spritesLength; i++)
            {
                st.Id    = s.RI32();
                st.Name  = s.RSaO();
                s.RI16();
                setIndex = s.RI16();
                isTexture = (setIndex & 0x1000) == 0x1000;
                setIndex &= 0xFFF;

                if (isTexture) { SpriteSets[setIndex].Textures[texCount[setIndex]] = st; texCount[setIndex]++; }
                else           { SpriteSets[setIndex]. Sprites[sprCount[setIndex]] = st; sprCount[setIndex]++; }
            }

            s.C();
        }

        public void BINWriter(string file)
        {
            if (SpriteSets        == null) return;
            if (SpriteSets.Length ==    0) return;

            KKdList<string> setName     = KKdList<string>.New;
            KKdList<string> setFileName = KKdList<string>.New;

            KKdList<int>    ids = KKdList<int>.New;
            KKdList<int> setIds = KKdList<int>.New;

            KKdList<int> notAdd = KKdList<int>.New;
            SpriteTexture temp;
            SpriteSet set;

            for (i = 0; i < SpriteSets.Length; i++)
            {
                set = SpriteSets[i];
                if (set.    Name  != null)
                    if  (setName    .Contains(set.    Name)) { notAdd.Add(i); continue; }
                    else setName    .Add     (set.    Name);
                if (set.FileName  != null)
                    if  (setFileName.Contains(set.FileName)) { notAdd.Add(i); continue; }
                    else setFileName.Add     (set.FileName);

                if (set.NewId)
                {
                    SpriteSets[i].Id = null;
                    for (i0 = 0; i0 < set.Textures.Length; i0++) SpriteSets[i].Textures[i0].Id = null;
                    for (i0 = 0; i0 < set. Sprites.Length; i0++) SpriteSets[i]. Sprites[i0].Id = null;
                    continue;
                }

                if (set.Id    != null)
                    if  (setIds.Contains((int)set.Id)) { notAdd.Add(i); continue; }
                    else setIds.Add     ((int)set.Id);

                for (i0 = 0; i0 < set. Sprites.Length; i0++)
                {
                    temp = set. Sprites[i0];
                    if (temp.Id != null)
                        if ( ids.Contains((int)temp.Id)) { notAdd.Add(i); break; }
                        else ids.Add     ((int)temp.Id);
                }
                if (i0 < set.Sprites.Length) continue;

                for (i0 = 0; i0 < set.Textures.Length; i0++)
                {
                    temp = set.Textures[i0];
                    if (temp.Id != null)
                        if ( ids.Contains((int)temp.Id)) { notAdd.Add(i); break; }
                        else ids.Add     ((int)temp.Id);
                }
            }
            setName    .Dispose();
            setFileName.Dispose();

            for (i = 0; i < SpriteSets.Length; i++)
            {
                set = SpriteSets[i];
                if (notAdd.Contains(i)) continue;
                if (!set.NewId) continue;

                i1 = 0;
                if (set.Id    == null) while (true)
                    { if (!setIds.Contains(i1)) { SpriteSets[i].Id = i1; setIds.Add(i1); break; } i1++; }

                for (i0 = 0, i1 = 0; i0 < set.Textures.Length; i0++)
                    while (set.Textures[i0].Id == null)
                        if (!ids.Contains(i1)) ids.Add((int)(SpriteSets[i].Textures[i0].Id = i1)); else i1++;

                for (i0 = 0, i1 = 0; i0 < set. Sprites.Length; i0++)
                    while (set. Sprites[i0].Id == null)
                        if (!ids.Contains(i1)) ids.Add((int)(SpriteSets[i]. Sprites[i0].Id = i1)); else i1++;
            }
               ids.Dispose();
            setIds.Dispose();

            for (i = 0, i0 = 0, i2 = 0; i < SpriteSets.Length; i++)
                if (!notAdd.Contains(i)) { i0 += SpriteSets[i].Sprites.Length + SpriteSets[i].Textures.Length; i2++; }

            i1 = i0 * 12;
            i1 = i1.A(0x20) + 0x20;

            s = File.OpenWriter(file + ".bin", true);
            s.W(i2);
            s.W(i1);
            s.W(i0);
            s.W(0x20);
            s.W(0x9066906690669066);
            s.W(0x9066906690669066);

            s.P = (i1 + i2 * 0x10).A(0x20);
            for (i = 0; i < SpriteSets.Length; i++)
            {
                if (notAdd.Contains(i)) continue;
                for (i0 = 0; i0 < SpriteSets[i].Textures.Length; i0++)
                { SpriteSets[i].Textures[i0].NameOffset = s.P;
                    s.W(SpriteSets[i].Textures[i0].Name + "\0"); }

                for (i0 = 0; i0 < SpriteSets[i]. Sprites.Length; i0++)
                { SpriteSets[i]. Sprites[i0].NameOffset = s.P;
                    s.W(SpriteSets[i]. Sprites[i0].Name + "\0"); }
            }

            for (i = 0; i < SpriteSets.Length; i++)
            {
                if (notAdd.Contains(i)) continue;
                SpriteSets[i].    NameOffset = s.P; s.W(SpriteSets[i].    Name + "\0");
                SpriteSets[i].FileNameOffset = s.P; s.W(SpriteSets[i].FileName + "\0");
            }
            s.A(0x08, true);

            s.P = 0x20;
            for (i = 0, i2 = 0; i < SpriteSets.Length; i++)
            {
                if (notAdd.Contains(i)) { i2++; continue; }

                for (i0 = 0; i0 < SpriteSets[i].Textures.Length; i0++)
                {
                    s.W(SpriteSets[i].Textures[i0].Id        );
                    s.W(SpriteSets[i].Textures[i0].NameOffset);
                    s.W((ushort)               i0  );
                    s.W((ushort)(0x1000 | (i - i2)));
                }

                for (i0 = 0; i0 < SpriteSets[i]. Sprites.Length; i0++)
                {
                    s.W(SpriteSets[i]. Sprites[i0].Id        );
                    s.W(SpriteSets[i]. Sprites[i0].NameOffset);
                    s.W((ushort)     i0 );
                    s.W((ushort)(i - i2));
                }
            }
            s.A(0x20);

            for (i = 0, i2 = 0; i < SpriteSets.Length; i++)
            {
                if (notAdd.Contains(i)) { i2++; continue; }

                s.W(SpriteSets[i].Id            );
                s.W(SpriteSets[i].    NameOffset);
                s.W(SpriteSets[i].FileNameOffset);
                s.W(i - i2);
            }
            notAdd.Dispose();
            s.C();
        }

        public void MsgPackReader(string file, bool json = false)
        {
            MsgPack msgPack = file.ReadMPAllAtOnce(json);

            MsgPack sprDB;
            if ((sprDB = msgPack["SprDB", true]).NotNull)
            {
                SpriteSets = new SpriteSet[sprDB.Array.Length];
                for (int i = 0; i < SpriteSets.Length; i++)
                    SpriteSets[i].ReadMsgPack(sprDB[i]);
            }
            sprDB.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (SpriteSets        == null) return;
            if (SpriteSets.Length ==    0) return;

            MsgPack sprDB = new MsgPack(SpriteSets.Length, "SprDB");
            for (i = 0; i < SpriteSets.Length; i++)
                sprDB[i] = SpriteSets[i].WriteMsgPack();

            sprDB.Write(false, true, file, json);
        }

        private bool disposed = false;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; SpriteSets = null; disposed = true; } }

        public struct SpriteTexture
        {
            public int NameOffset;
            public int? Id;
            public string Name;

            public void ReadMsgPack(MsgPack msg)
            { Id = msg.RnI32("Id"); Name = msg.RS("Name"); }

            public MsgPack WriteMsgPack() =>
                MsgPack.New.Add("Id", Id).Add("Name", Name);
        }

        public struct SpriteSet
        {
            public int NameOffset;
            public int FileNameOffset;
            public int? Id;
            public bool NewId;
            public string Name;
            public string FileName;
            public SpriteTexture[] Sprites;
            public SpriteTexture[] Textures;

            public void ReadMsgPack(MsgPack msg)
            {
                FileName = msg.RS   ("FileName");
                Id       = msg.RnI32(   "Id"   );
                Name     = msg.RS   ("Name"    );
                NewId    = msg.RB   ("NewId"   );

                MsgPack temp;
                if ((temp = msg["Sprites", true]).NotNull)
                {
                    Sprites = new SpriteTexture[temp.Array.Length];
                    for (int i0 = 0; i0 < Sprites.Length; i0++)
                        Sprites[i0].ReadMsgPack(temp[i0]);
                }

                if ((temp = msg["Textures", true]).NotNull)
                {
                    Textures = new SpriteTexture[temp.Array.Length];
                    for (int i0 = 0; i0 < Textures.Length; i0++)
                        Textures[i0].ReadMsgPack(temp[i0]);
                }
                temp.Dispose();
            }

            public MsgPack WriteMsgPack()
            {
                MsgPack  sprites = new MsgPack( Sprites.Length,  "Sprites");
                for (int i0 = 0; i0 < Sprites.Length; i0++)
                     sprites[i0] = Sprites[i0].WriteMsgPack();

                MsgPack textures = new MsgPack(Textures.Length, "Textures");
                for (int i0 = 0; i0 <  Textures.Length; i0++)
                    textures[i0] = Textures[i0].WriteMsgPack();

                return MsgPack.New.Add("FileName", FileName).Add("Id", Id)
                    .Add("Name", Name).Add(sprites).Add(textures);
            }
        }
    }
}
