using System;
using System.Net;
using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct DataBank : IDisposable
    {
        private int i;
        private Stream s;

        public bool pvListPDA;
        public PvList[] pvList;
        public PsrData[] psrData;

        private const string d = ".";
        private const string c = ",";

        public bool Success { get; private set; }

        public void DBReader(string file)
        {
            pvListPDA = false;
            Success = false;
            if (!File.Exists(file)) return;
            string text = File.ReadAllText(file);
            while (text.Contains("%")) text = WebUtility.UrlDecode(text);
            string[] array = text.Split(',');

            pvList = null;
            psrData = null;
            if (file.Contains("psrData") && array.Length % 13 == 0)
            {
                psrData = new PsrData[array.Length / 13];
                for (i = 0; i < psrData.Length; i++) psrData[i].SetValue(array, i);
                Success = true;
            }
            else if (file.Contains("psrData")) Success = true;
            else if (file.Contains("PvList") && (array.Length % 7 == 0 || array.Length % 6 == 0))
            {
                if (array[2] != "0" && array[2] != "1")
                {
                    pvListPDA = true;
                    pvList = new PvList[array.Length / 6];
                    for (i = 0; i < pvList.Length; i++) pvList[i].SetValue(array, i, true);
                }
                else
                {
                    pvList = new PvList[array.Length / 7];
                    for (i = 0; i < pvList.Length; i++) pvList[i].SetValue(array, i, false);
                }
                Success = true;
            }
            else if (file.Contains("PvList")) Success = true;
        }

        public byte[] DBWriter(string file)
        {
            if (!Success) return null;

            s = File.OpenWriter();
            if (file.Contains("psrData"))
            {
                if (psrData != null && psrData.Length > 0)
                    for (i = 0; i < psrData.Length; i++)
                        if (psrData[i].PV_ID == 92)
                            s.W(psrData[i].ToString() + c);
                        else
                            s.W(psrData[i].ToString() + c);
            }
            else if (file.Contains("PvList"))
            {
                if (pvList != null && pvList.Length > 0)
                {
                    for (i = 0; i < pvList.Length - 1; i++)
                        s.W(UrlEncode(pvList[i].ToString(pvListPDA) + c));
                    s.W(UrlEncode(pvList[i].ToString(pvListPDA)));
                }
                else s.W("%2A%2A%2A");
            }

            return s.ToArray(true);
        }

        public void DBWriter(string file, uint timestamp)
        {
            if (!Success) return;

            byte[] data = DBWriter(file);
            if (data == null) return;

            ushort hash = data.HashCRC16_CCITT();
            File.WriteAllBytes(file + "_" + hash + "_" + timestamp + ".dat", data);
        }

        public void MsgPackReader(string file, bool json)
        {
            pvListPDA = false;
            Success = false;
            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            bool compact = msgPack.RB("Compact");

            psrData = null;
            pvList = null;
            if (file.Contains("psrData"))
            {
                MsgPack psrData;
                if ((psrData = msgPack["psrData", true]).NotNull)
                {
                    this.psrData = new PsrData[psrData.Array.Length];
                    for (i = 0; i < this.psrData.Length; i++)
                        this.psrData[i].SetValue(psrData[i]);
                }
                Success = true;
                psrData.Dispose();
            }
            else if (file.Contains("PvList"))
            {
                MsgPack pvList;
                if ((pvList = msgPack["PvList", true]).NotNull)
                {
                    this.pvList = new PvList[pvList.Array.Length];
                    for (i = 0; i < this.pvList.Length; i++)
                        this.pvList[i].SetValue(pvList[i], compact);
                }
                else if ((pvList = msgPack["PvListPDA", true]).NotNull)
                {
                    pvListPDA = true;
                    this.pvList = new PvList[pvList.Array.Length];
                    for (i = 0; i < this.pvList.Length; i++)
                        this.pvList[i].SetValue(pvList[i], compact);
                }
                Success = true;
                pvList.Dispose();
            }

            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (!Success) return;
            MsgPack msgPack = MsgPack.New;

            if (file.Contains("psrData"))
            {
                if (psrData != null)
                {
                    MsgPack psrData = new MsgPack(this.psrData.Length, "psrData");
                    for (i = 0; i < this.psrData.Length; i++) psrData[i] = this.psrData[i].WriteMP();
                    msgPack.Add(psrData);
                }
                else msgPack.Add(new MsgPack("psrData", null));
            }
            else if (file.Contains("PvList"))
            {
                if (pvList != null)
                {msgPack.Add("Compact", true);

                    MsgPack PvList = new MsgPack(pvList.Length, pvListPDA ? "PvListPDA" : "PvList");
                    for (i = 0; i < pvList.Length; i++) PvList[i] = pvList[i].WriteMP();
                    msgPack.Add(PvList);
                }
                else msgPack.Add(new MsgPack(pvListPDA ? "PvListPDA" : "PvList", null));
            }
            msgPack.Write(file, false, json).Dispose();
        }

        public static string UrlEncode(string value) =>
            WebUtility.UrlEncode(value).Replace("+", "%20").Replace("!", "%21").Replace("*", "%2A");

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; psrData = null;
                pvList = null; pvListPDA = false; Success = false; disposed = true; } }

        public struct PsrData
        {
            public Player p1;
            public Player p2;
            public Player p3;
            public int PV_ID;

            public void SetValue(string[] data, int i = 0)
            {
                p1.SetValue(data, i, 0);
                p2.SetValue(data, i, 1);
                p3.SetValue(data, i, 2);
                PV_ID = int.Parse(data[i * 13 + 12]);
            }

            public void SetValue(MsgPack msg)
            {
                int? id = msg.RnI32("PV_ID");
                if (id != null) PV_ID = (int)id;
                else { id = msg.RnI32("ID"); if (id != null) PV_ID = (int)id; }

                MsgPack temp;
                if ((temp = msg["P1"]).NotNull) p1.SetValue(temp);
                if ((temp = msg["P2"]).NotNull) p2.SetValue(temp);
                if ((temp = msg["P3"]).NotNull) p3.SetValue(temp);
                temp.Dispose();
            }

            public MsgPack WriteMP() =>
                MsgPack.New.Add("PV_ID", PV_ID).Add(p1.WriteMP("P1"))
                     .Add(p2.WriteMP("P2")).Add(p3.WriteMP("P3"));

            public override string ToString() =>
                UrlEncode($"{p1},{p2},{p3},{PV_ID}");
        }

        public struct Player
        {
            public string Name;
            public string NameExtra;
            public int Score;
            public int ScoreExtra;
            public Difficulty Diff;
            public bool HasExtra => NameExtra != null;

            public void SetValue(string[] data, int i = 0, int offset = 0)
            {
                string[] array = data[i * 13 + offset * 4].Split('.');
                Score = int.Parse(array[0]);
                if (array.Length > 1) ScoreExtra = int.Parse(array[1]);
                string text = "";
                for (int j = 0; j < data[i * 13 + 1 + offset * 4].Length; j++)
                {
                    text += data[i * 13 + 1 + offset * 4][j].ToString();
                    if (text.EndsWith("xxx"))
                    {
                        Name = text.Remove(text.Length - 3);
                        text = "";
                    }
                }
                if (array.Length == 1) Name = text;
                else NameExtra = text;
                if (int.TryParse(data[i * 13 + 2 + offset * 4], out int Diff))
                    this.Diff = (Difficulty)Diff;
                else
                    Enum.TryParse(data[i * 13 + 2 + offset * 4], out this.Diff);
            }

            public void SetValue(MsgPack msg)
            {
                 Diff  = (Difficulty)msg.RI32("Diff");
                 Name      = msg.RS  ( "Name");
                 NameExtra = msg.RS  ( "NameExtra");
                Score      = msg.RI32("Score");
                ScoreExtra = msg.RI32("ScoreExtra");

                if (Name == null)
                {
                     Name      = msg.RS  ( "Name0");
                     NameExtra = msg.RS  ( "Name1");
                    Score      = msg.RI32("Score0");
                    ScoreExtra = msg.RI32("Score1");
                }
            }

            public MsgPack WriteMP(string name) =>
                HasExtra ? new MsgPack(name).Add("Diff", (int)Diff).Add("Name", Name)
                .Add("NameExtra", NameExtra).Add("Score", Score).Add("ScoreExtra", ScoreExtra) :
                new MsgPack(name).Add("Diff", (int)Diff).Add("Name" , Name) .Add("Score" , Score);

            public override string ToString() =>
                Score + (HasExtra ? (d + ScoreExtra) : "") + c + UrlEncode(Name) +
                (HasExtra ? ("xxx" + UrlEncode(NameExtra)) : "") + c +
                (int)Diff + c + (HasExtra ? "0.1" : "0");
        }

        public struct PvList
        {
            public int PV_ID;
            public int Version;
            public int Edition;
            public Date AdvDemoStart;
            public Date AdvDemoEnd;
            public Date StartShow;
            public Date   EndShow;

            public void SetValue(string[] data, int i = 0, bool pda = false)
            {
                if (pda)
                {
                    PV_ID = int.Parse(data[i * 6]);
                    Version = int.Parse(data[i * 6 + 1]);
                    AdvDemoStart.SV(data[i * 6 + 2]);
                    AdvDemoEnd.SV(data[i * 6 + 3]);
                    StartShow.SV(data[i * 6 + 4]);
                    EndShow.SV(data[i * 6 + 5]);
                }
                else
                {
                    PV_ID   = int.Parse(data[i * 7]);
                    Version = int.Parse(data[i * 7 + 1]);
                    Edition = int.Parse(data[i * 7 + 2]);
                    AdvDemoStart.SV(data[i * 7 + 3]);
                    AdvDemoEnd  .SV(data[i * 7 + 4]);
                    StartShow   .SV(data[i * 7 + 5]);
                      EndShow   .SV(data[i * 7 + 6]);
                }
            }

            public void SetValue(MsgPack msg, bool Compact)
            {
                Version = 1;
                Edition = 0;

                int? id = msg.RnI32("PV_ID");
                if (id != null) PV_ID = id.Value; else { id = msg.RnI32("ID"); if (id != null) PV_ID = id.Value; }
                bool? enable = msg.RnB("Enable"); if (enable != null) Version = enable.Value ? 1 : 0;
                bool? extra  = msg.RnB("Extra" ); if (extra  != null) Edition = extra .Value ? 1 : 0;
                int? version = msg.RnI32("Version"); if (version != null) Version = version.Value;
                int? edition = msg.RnI32("Edition"); if (edition != null) Edition = edition.Value;
                if (Compact)
                {
                    AdvDemoStart.SV(msg.RnI32("AdvDemoStart"),  true);
                    AdvDemoEnd  .SV(msg.RnI32("AdvDemoEnd"  ), false);
                    StartShow   .SV(msg.RnI32("StartShow"   ), false);
                      EndShow   .SV(msg.RnI32(  "EndShow"   ),  true);
                    return;
                }

                MsgPack temp;
                if ((temp = msg["AdvDemoStart", true]).NotNull) AdvDemoStart.SV(temp,  true);
                if ((temp = msg["AdvDemoEnd"  , true]).NotNull) AdvDemoEnd  .SV(temp, false);
                if ((temp = msg["StartShow"   , true]).NotNull) StartShow   .SV(temp, false);
                if ((temp = msg[  "EndShow"   , true]).NotNull)   EndShow   .SV(temp,  true);
                temp.Dispose();
            }

            public MsgPack WriteMP()
            {
                MsgPack msgPack = MsgPack.New.Add("ID"     , PV_ID  );
                if (Version != 1) msgPack.Add("Version", Version);
                if (Edition != 0) msgPack.Add("Edition", Edition);
                if (AdvDemoStart.WU) msgPack.Add("AdvDemoStart", AdvDemoStart.Int);
                if (AdvDemoEnd  .WL) msgPack.Add("AdvDemoEnd"  , AdvDemoEnd  .Int);
                if (StartShow   .WL) msgPack.Add("StartShow"   , StartShow   .Int);
                if (  EndShow   .WU) msgPack.Add(  "EndShow"   ,   EndShow   .Int);
                return msgPack;
            }

            public string ToString(bool pda) => !pda
                ? UrlEncode($"{PV_ID},{Version},{Edition},{AdvDemoStart},{AdvDemoEnd},{StartShow},{EndShow}")
                : UrlEncode($"{PV_ID},{Version},{AdvDemoStart},{AdvDemoEnd},{StartShow},{EndShow}");
        }

        public struct Date
        {
            private int  year;
            private int month;
            private int  day;

            public int  Year { get =>  year; set {  year = value; CheckDate(); } }
            public int Month { get => month; set { month = value; CheckDate(); } }
            public int   Day { get =>   day; set {   day = value; CheckDate(); } }

            public bool WU => Year != 2029 || Month != 1 || Day != 1;
            public bool WL => Year != 2000 || Month != 1 || Day != 1;

            public void SDL() => Year = 2000;
            public void SDU() => Year = 2029;

            public void SV(string data)
            {
                string[] array = data.Split('-');
                if (array.Length == 3)
                {
                     Year = int.Parse(array[0]);
                    Month = int.Parse(array[1]);
                      Day = int.Parse(array[2]);
                }
            }

            public void SV(int? ymd, bool setDefaultUpper)
            {
                if (!setDefaultUpper) SDL();
                else                  SDU();
                if (ymd != null)
                {
                    year  = ymd.Value / 10000;
                    month = ymd.Value / 100 % 100;
                    day   = ymd.Value % 100;
                    CheckDate();
                }
            }

            public void SV(MsgPack msg, bool setDefaultUpper)
            {
                if (!setDefaultUpper) SDL();
                else                  SDU();
                int?  Year = msg.RnI32( "Year");
                int? Month = msg.RnI32("Month");
                int?   Day = msg.RnI32(  "Day");
                if ( Year != null)  year =  Year.Value;
                if (Month != null) month = Month.Value;
                if (  Day != null)   day =   Day.Value;
                CheckDate();
            }

            public int Int => (Year * 100 + Month) * 100 + Day;

            private void CheckDate()
            {
                if (year <  2000) { year = 2000; month = 1; day = 1; return; }
                if (year >= 2029) { year = 2029; month = 1; day = 1; return; }
                     if (month <  1) month = 1;
                else if (month > 12) month = 12;
                     if (day <  1) day = 1;
                else if (day > 31 && (month == 1 || month ==  3 || month ==  5 ||
                        month == 7 || month == 8 || month == 10 || month == 12)) day = 31;
                else if (day > 30 && (month == 4 || month ==  6 ||
                                      month == 9 || month == 11)) day = 30;
                else if (day > 29 && month == 2 && year % 4 == 0) day = 29;
                else if (day > 28 && month == 2 && year % 4 != 0) day = 28;
            }

            public override string ToString() =>
                $"{Year:d4}-{Month:d2}-{Day:d2}";
        }

        public enum Difficulty
        {
            Easy    = 0,
            Normal  = 1,
            Hard    = 2,
            Extreme = 3,
        }
    }
}
