using System;
using KKdBaseLib;
using KKdMainLib.IO;
using A3DADict = System.Collections.Generic.Dictionary<string, object>;

namespace KKdMainLib.DB
{
    public class Auth : IDisposable
    {
        private int i;
        private Stream _IO;

        public string[] Category;
        public UID[] UIDs;

        public void BINReader(string file)
        {
            A3DADict dict = new A3DADict();

            _IO = File.OpenReader(file + ".bin");

            _IO.Format = Format.F;
            int signature = _IO.RI32();
            if (signature != 0x44334123) return;
            signature = _IO.RI32();
            if (signature != 0x5F5F5F41) return;
            _IO.RI64();

            string[] strData = _IO.RS(_IO.L - _IO.P).Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            for (i = 0; i < strData.Length; i++)
                dict.GetDictionary(strData[i]);
            strData = null;

            if (dict.FindValue(out string value, "category.length"))
            {
                Category = new string[int.Parse(value)];
                for (i = 0; i < Category.Length; i++)
                    if (dict.FindValue(out value, "category." + i + ".value"))
                        Category[i] = value;
            }

            if (dict.FindValue(out value, "uid.length"))
            {
                UIDs = new UID[int.Parse(value)];
                for (i = 0; i < UIDs.Length; i++)
                {
                    dict.FindValue(out UIDs[i].Category, "uid." + i + ".category");
                    dict.FindValue(out UIDs[i].OrgUid  , "uid." + i + ".org_uid" );
                    dict.FindValue(out UIDs[i].Size    , "uid." + i + ".size"    );
                    dict.FindValue(out UIDs[i].Value   , "uid." + i + ".value"   );
                }
            }

            _IO.C();
            dict.Clear();
            dict = null;
        }

        public void BINWriter(string file)
        {
            _IO = File.OpenWriter(file + ".bin", true);

            _IO.W("#A3DA__________\n");
            _IO.W("#" + DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss yyyy",
                System.Globalization.CultureInfo.InvariantCulture) + "\n");

            if (Category != null)
            {
                int[] so = Category.Length.SortWriter();
                for (i = 0; i < Category.Length; i++)
                    _IO.W($"category.{so[i]}.value={ Category[so[i]]}\n");
                _IO.W($"category.length={Category.Length}\n");
            }

            if (UIDs != null)
            {
                int[] so = UIDs.Length.SortWriter();
                for (i = 0; i < UIDs.Length; i++)
                {
                    if (UIDs[so[i]].Category != null && UIDs[so[i]].Category != "")
                        _IO.W($"uid.{so[i]}.category=" + UIDs[so[i]].Category + "\n");
                    if (UIDs[so[i]].OrgUid   != null)
                        _IO.W($"uid.{so[i]}.org_uid="  + UIDs[so[i]].OrgUid   + "\n");
                    if (UIDs[so[i]].Size     != null)
                        _IO.W($"uid.{so[i]}.size="     + UIDs[so[i]].Size     + "\n");
                    if (UIDs[so[i]].Value    != null && UIDs[so[i]].Value    != "")
                        _IO.W($"uid.{so[i]}.value="    + UIDs[so[i]].Value    + "\n");
                }
                _IO.W($"uid.length={UIDs.Length}\n");
            }

            _IO.C();
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

            authDB.Write(true, file, json);
        }

        private bool disposed = false;
        public void Dispose()
        { if (!disposed) { if (_IO != null) _IO.D(); Category = null; UIDs = null; disposed = true; } }

        public struct UID
        {
            public int? Size;
            public int? OrgUid;
            public string Value;
            public string Category;
        }
    }
}
