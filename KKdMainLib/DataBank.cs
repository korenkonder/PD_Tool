using System;
using System.Net;
using System.Xml.Linq;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public class DataBank
    {
        public DataBank()
        { Success = false; Xml = null; IO = null; pvList = null; }

        public bool Success { get; private set; }

        private Xml Xml;
        private Stream IO;

        private PvList[] pvList;

        public void DBReader(string file)
        {
            Success = false;
            if (!File.Exists(file)) return;

            string out_data = File.ReadAllText(file);
            while (out_data.Contains("%")) out_data = WebUtility.UrlDecode(out_data);

            string[] data_split = out_data.Split(',');
            
            if (file.Contains("PvList"))
            {
                if (data_split.Length % 7 < 2)
                {
                    int Count = data_split.Length / 7;
                    pvList = new PvList[Count];
                    for (int i = 0; i < Count; i++)
                        pvList[i].SetValue(data_split, i);
                    Success = true;
                    return;
                }
            }
        }

        public void DBWriter(string file)
        {
            if (!Success) return;
            
            IO = File.OpenWriter();
            if (file.Contains("PvList"))
                if (pvList.Length > 0)
                    for (int i = 0; i < pvList.Length; i++)
                        IO.Write(UrlEncode(pvList[i].ToString() +
                            (i + 1 != pvList.Length ? c : "")));
                else IO.Write("%2A%2A%2A");

            byte[] data = IO.ToArray(true);

            ushort checksum = DCC.CalculateChecksum(data);
            uint time = (uint)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            File.WriteAllBytes(file + "_" + checksum + "_" + time + ".dat", data);
        }

        public void XMLReader(string file)
        {
            Success = false;

            if (!File.Exists(file)) return;

            Xml = new Xml();
            Xml.OpenXml(file, true);

            if (file.Contains("PvList"))
                foreach (XElement PvList in Xml.doc.Elements("PvList"))
                {
                    int Count = 0;
                    foreach (XElement PV in PvList.Elements()) if (PV.Name == "PV") Count++;

                    pvList = new PvList[Count];
                    int i = 0;
                    foreach (XElement PV in PvList.Elements())
                    {
                        pvList[i].SetValue(PV);
                        i++;
                    }
                }

            Success = true;
        }

        public void XMLWriter(string file)
        {
            if (!Success) return;

            Xml = new Xml { Compact = true };

            if (file.Contains("PvList"))
            {
                XElement PvList = new XElement("PvList");
                foreach (PvList pv in pvList)
                    PvList.Add(pv.WriteXml(Xml, "PV"));
                Xml.doc.Add(PvList);
            }

            if (File.Exists(file)) File.Delete(file);
            Xml.SaveXml(file);
        }

        public struct Player
        {
            public int Score0;
            public int Score1;
            public string Name0;
            public string Name1;
            public int Diff;
            public bool Has2P => Name1 != null;

            public void SetValue(string[] data, int i = 0, int offset = 0)
            {
                string[] arr = data[i * 13 + 0 + offset * 4].Split('.');
                Score0 = int.Parse(arr[0]);
                if (arr.Length > 1) Score1 = int.Parse(arr[1]);

                string temp = "";
                for (int i1 = 0; i1 < data[i * 13 + 1 + offset * 4].Length; i1++)
                {
                    temp += data[i * 13 + 1 + offset * 4][i1];
                    if (temp.EndsWith("xxx")) { Name0 = temp.Remove(temp.Length - 3); temp = ""; }
                }
                if (arr.Length == 1) Name0 = temp;
                else                 Name1 = temp;
                Diff = int.Parse(data[i * 13 + 2 + offset * 4]);
            }

            public void SetValue(XElement value)
            {
                Name1 = null;
                foreach (XAttribute Entry in value.Attributes())
                         if (Entry.Name == "Score" ) Score0 = int.Parse(Entry.Value);
                    else if (Entry.Name == "Name"  ) Name0  = Entry.Value;
                    else if (Entry.Name == "Diff"  ) Diff   = int.Parse(Entry.Value);
                    else if (Entry.Name == "Score0") Score0 = int.Parse(Entry.Value);
                    else if (Entry.Name == "Score1") Score1 = int.Parse(Entry.Value);
                    else if (Entry.Name == "Name0" ) Name0  = Entry.Value;
                    else if (Entry.Name == "Name1" ) Name1  = Entry.Value;
            }

            public XElement WriteXml(Xml Xml, string name)
            {
                XElement element = new XElement(name);
                if (!Has2P)
                {
                    Xml.Writer(element, Score0, "Score");
                    Xml.Writer(element, Name0 , "Name" );
                }
                else
                {
                    Xml.Writer(element, Score0, "Score0");
                    Xml.Writer(element, Score1, "Score1");
                    Xml.Writer(element, Name0 , "Name0" );
                    Xml.Writer(element, Name1 , "Name1" );
                }
                Xml.Writer(element, Diff, "Diff");
                return element;
            }

            public override string ToString() => (Score0 + (Has2P ? d + Score1 : "") + c + UrlEncode(Name0) +
                (Has2P ? "xxx" + UrlEncode(Name1) : "") + c + Diff + c + (Has2P ? "0.1" : "0")).Replace("*", "%2A");
        }

        public static string UrlEncode(string value) => WebUtility.UrlEncode(value).Replace("+", "%20");

        public struct PvList
        {
            public int ID;
            public bool Enable;
            public bool Extra;
            public Date AdvDemoStart;
            public Date AdvDemoEnd;
            public Date StartShow;
            public Date   EndShow;

            public void SetValue(string[] data, int i = 0)
            {
                ID     = int.Parse(data[i * 7 + 0]);
                Enable = int.Parse(data[i * 7 + 1]) == 1;
                Extra  = int.Parse(data[i * 7 + 2]) == 1;
                AdvDemoStart.SetValue(data[i * 7 + 3]);
                AdvDemoEnd  .SetValue(data[i * 7 + 4]);
                StartShow   .SetValue(data[i * 7 + 5]);
                  EndShow   .SetValue(data[i * 7 + 6]);
            }

            public override string ToString() => UrlEncode(ID +
                c + (Enable ? 1 : 0) + c + (Extra ? 1 : 0) +
                c + AdvDemoStart.ToString() + c + AdvDemoEnd.ToString() +
                c + StartShow   .ToString() + c + EndShow   .ToString());

            public void SetValue(XElement Value)
            {
                Enable = true;
                foreach (XAttribute Entry in Value.Attributes())
                         if (Entry.Name == "ID"       ) ID        =  int.Parse(Entry.Value);
                    else if (Entry.Name == "Enable"   ) Enable    = bool.Parse(Entry.Value);
                    else if (Entry.Name == "Extra"    ) Extra     = bool.Parse(Entry.Value);

                AdvDemoStart.SetDefaultUpper();
                AdvDemoEnd  .SetDefaultLower();
                StartShow   .SetDefaultLower();
                  EndShow   .SetDefaultUpper();

                foreach (XElement value in Value.Elements())
                         if (value.Name == "AdvDemoStart") AdvDemoStart.SetValue(value);
                    else if (value.Name == "AdvDemoEnd"  ) AdvDemoEnd  .SetValue(value);
                    else if (value.Name == "StartShow"   ) StartShow   .SetValue(value);
                    else if (value.Name ==   "EndShow"   )   EndShow   .SetValue(value);
            }

            public XElement WriteXml(Xml Xml, string name)
            {
                XElement element = new XElement(name);
                Xml.Writer(element, ID    , "ID"    );
                if (!Enable) Xml.Writer(element, Enable, "Enable");
                if ( Extra ) Xml.Writer(element, Extra , "Extra" );
                if (AdvDemoEnd.WriteLower)
                    element.Add(AdvDemoStart.WriteXml(Xml, "AdvDemoStart"));
                if (AdvDemoEnd.WriteLower)
                    element.Add(AdvDemoEnd  .WriteXml(Xml, "AdvDemoEnd"  ));
                if (StartShow     .WriteLower)
                    element.Add(StartShow   .WriteXml(Xml, "StartShow"   ));
                if (  EndShow     .WriteUpper)
                    element.Add(EndShow     .WriteXml(Xml,   "EndShow"   ));
                return element;
            }
        }

        private const string d = ".";
        private const string c = ",";

        public struct Date
        {
            private int year ;
            private int month;
            private int day  ;

            public int Year  { get => year ; set { year  = value; CheckDate(); } }
            public int Month { get => month; set { month = value; CheckDate(); } }
            public int Day   { get => day  ; set { day   = value; CheckDate(); } }

            public bool WriteUpper => (Year == 2029 && Month == 1 && Day == 1) ^ true;
            public bool WriteLower => (Year == 2000 && Month == 1 && Day == 1) ^ true;

            public void SetDefaultLower() => Year = 1999;
            public void SetDefaultUpper() => Year = 2029;

            public void SetValue(string data)
            {
                string[] arr = data.Split('-');
                if (arr.Length != 3) return;
                Year  = int.Parse(arr[0]);
                Month = int.Parse(arr[1]);
                Day   = int.Parse(arr[2]);
            }

            private void CheckDate()
            {
                     if (year <  2000) { year = 2000; month = 1; day = 1; return; }
                else if (year >= 2029) { year = 2029; month = 1; day = 1; return; }

                     if (month <  1) month =  1;
                else if (month > 12) month = 12;
                     if (day   <  1) day   =  1;
                else if (day   > 31 && (month == 1 || month == 3 || month == 5 ||
                    month == 7 || month == 8 || month == 10 || month == 12)) day = 31;
                else if (day   > 30 && (month == 4 || month == 6 || month == 9 || month == 11)) day = 30;
                else if (day   > 29 && month == 2 && year % 4 == 0) day = 29;
                else if (day   > 28 && month == 2 && year % 4 != 0) day = 29;
            }

            public override string ToString() =>
                Year.ToString("d4") + "-" + Month.ToString("d2") + "-" + Day.ToString("d2");

            public void SetValue(XElement value)
            {
                foreach (XAttribute Entry in value.Attributes())
                         if (Entry.Name == "Year" ) Year  = int.Parse(Entry.Value);
                    else if (Entry.Name == "Month") Month = int.Parse(Entry.Value);
                    else if (Entry.Name == "Day"  ) Day   = int.Parse(Entry.Value);
            }

            public XElement WriteXml(Xml Xml, string name)
            {
                XElement element = new XElement(name);
                Xml.Writer(element, Year , "Year" );
                Xml.Writer(element, Month, "Month");
                Xml.Writer(element, Day  , "Day"  );
                return element;
            }
        }
    }
}
