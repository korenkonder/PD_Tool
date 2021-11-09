using System;
using KKdBaseLib;
using KKdMainLib.IO;
using A3DADict = System.Collections.Generic.Dictionary<string, object>;

namespace KKdMainLib.DB
{
    public class Auth : IDisposable
    {
        private int i;
        private Stream s;

        public string[] Category;
        public UID[] UIDs;

        public void BINReader(string file)
        {
            A3DADict dict = new A3DADict();

            s = File.OpenReader(file + ".bin");

            s.Format = Format.F;
            int signature = s.RI32();
            if (signature != 0x44334123) return;
            signature = s.RI32();
            if (signature != 0x5F5F5F41) return;
            s.RI64();

            string[] strData = s.RS(s.L - s.P).Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            for (i = 0; i < strData.Length; i++)
                dict.GD(strData[i]);
            strData = null;

            if (dict.FV(out string value, "category.length"))
            {
                Category = new string[int.Parse(value)];
                for (i = 0; i < Category.Length; i++)
                    if (dict.FV(out value, $"category.{i}.value"))
                        Category[i] = value;
            }

            if (dict.FV(out value, "uid.length"))
            {
                UIDs = new UID[int.Parse(value)];
                for (i = 0; i < UIDs.Length; i++)
                {
                    dict.FV(out UIDs[i].Category, $"uid.{i}.category");
                    dict.FV(out UIDs[i].OrgUid  , $"uid.{i}.org_uid" );
                    dict.FV(out UIDs[i].Size    , $"uid.{i}.size"    );
                    dict.FV(out UIDs[i].Value   , $"uid.{i}.value"   );
                }
            }

            s.C();
            dict.Clear();
            dict = null;
        }

        public void BINWriter(string file)
        {
            s = File.OpenWriter(file + ".bin", true);

            s.W("#A3DA__________\n");
            s.W("# date time was eliminated.\n");

            if (Category != null)
            {
                int[] so = Category.Length.SW();
                for (i = 0; i < Category.Length; i++)
                    s.W($"category.{so[i]}.value={ Category[so[i]]}\n");
                s.W($"category.length={Category.Length}\n");
            }

            if (UIDs != null)
            {
                int[] so = UIDs.Length.SW();
                int max = -1;
                for (i = 0; i < UIDs.Length; i++)
                {
                    if (UIDs[so[i]].Category != null && UIDs[so[i]].Category != "")
                        s.W($"uid.{so[i]}.category=" + UIDs[so[i]].Category + "\n");
                    if (UIDs[so[i]].OrgUid   != null)
                    {
                        int orgUid = UIDs[so[i]].OrgUid.Value;
                        s.W($"uid.{so[i]}.org_uid="  +             orgUid   + "\n");
                        if (max < orgUid)
                            max = orgUid;
                    }
                    if (UIDs[so[i]].Size     != null)
                        s.W($"uid.{so[i]}.size="     + UIDs[so[i]].Size     + "\n");
                    if (UIDs[so[i]].Value    != null && UIDs[so[i]].Value    != "")
                        s.W($"uid.{so[i]}.value="    + UIDs[so[i]].Value    + "\n");
                }
                s.W($"uid.length={UIDs.Length}\n");
                if (max != -1)
                    s.W($"uid.max={max}\n");
            }

            s.C();
        }

        public void MsgPackReader(string file, bool json)
        {
            MsgPack msgPack = file.ReadMPAllAtOnce(json);

            MsgPack authDB;
            if ((authDB = msgPack["AuthDB"]).NotNull)
            {
                MsgPack temp;
                if ((temp = authDB["Category", true]).NotNull)
                {
                    Category = new string[temp.Array.Length];
                    for (int i = 0; i < Category.Length; i++)
                        Category[i] = temp[i].RS();
                }

                if ((temp = authDB["UID", true]).NotNull)
                {
                    UIDs = new UID[temp.Array.Length];
                    for (int i = 0; i < UIDs.Length; i++)
                    {
                        UIDs[i].Category = temp[i].RS   ("Category");
                        UIDs[i].OrgUid   = temp[i].RnI32("OrgUid"  );
                        UIDs[i].Size     = temp[i].RnI32("Size"    );
                        UIDs[i].Value    = temp[i].RS   ("Value"   );
                    }
                }
                temp.Dispose();
            }
            authDB.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            MsgPack authDB = new MsgPack("AuthDB");
            if (Category != null)
            {
                MsgPack category = new MsgPack(Category.Length, "Category");
                for (int i = 0; i < Category.Length; i++)
                    category[i] = (MsgPack)Category[i];
                authDB.Add(category);
            }

            if (UIDs != null)
            {
                MsgPack uid = new MsgPack(UIDs.Length, "UID");
                for (int i = 0; i < UIDs.Length; i++)
                    uid[i] = MsgPack.New.Add("Category", UIDs[i].Category)
                                        .Add("OrgUid"  , UIDs[i].OrgUid  )
                                        .Add("Size"    , UIDs[i].Size    )
                                        .Add("Value"   , UIDs[i].Value   );
                authDB.Add(uid);
            }

            authDB.Write(false, true, file, json);
        }

        private bool disposed = false;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; Category = null; UIDs = null; disposed = true; } }

        public struct UID
        {
            public int? Size;
            public int? OrgUid;
            public string Value;
            public string Category;
        }
    }
}
