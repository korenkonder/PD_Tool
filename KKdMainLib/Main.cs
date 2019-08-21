﻿using System;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;
using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public static unsafe class Main
    {
        public static void ConsoleDesign(string text, params string[] args)
        {
            text = string.Format(text, args);
            string Text = "█                                                  █";
            Text = Text.Remove(3) + text + Text.Remove(0, text.Length + 3);
            Console.WriteLine(Text);
        }

        public static void ConsoleDesign(bool Fill)
        {
            if (Fill) Console.WriteLine("████████████████████████████████████████████████████");
            else      Console.WriteLine("█                                                  █");
        }

        public const string TimeFormatHHmmssfff = "{0:d2}:{1:d2}:{2:d2}.{3:d3}";

        public static void WriteTime(this TimeSpan time, bool WriteLine = false)
        {
            if (WriteLine) Console.WriteLine(TimeFormatHHmmssfff,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            else           Console.Write    (TimeFormatHHmmssfff,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public static void WriteTime(this TimeSpan time, string Text, bool WriteLine = true)
        {
            if (WriteLine) Console.WriteLine(TimeFormatHHmmssfff + " - " + Text,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            else           Console.Write    (TimeFormatHHmmssfff + " - " + Text,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        private static string GetArgs(string name, bool And, params string[] ext)
        {
            string Out = "";
            if (And) Out = "|";
            Out += name + " files (";
            for (int i = 0; i < ext.Length; i++)
            { Out += "*." + ext[i]; if (i + 1 < ext.Length) Out += ", "; }
            Out += ")|";
            for (int i = 0; i < ext.Length; i++)
            { Out += "*." + ext[i]; if (i + 1 < ext.Length) Out += ";"; }

            return Out;
        }

        private static string GetArgs(string name, params string[] ext)
        {
            string Out = name + " files (";
            for (int i = 0; i < ext.Length; i++)
            { Out += "*." + ext[i]; if (i + 1 < ext.Length) Out += ", "; }
            Out += ")|";
            for (int i = 0; i < ext.Length; i++)
            { Out += "*." + ext[i]; if (i + 1 < ext.Length) Out += ";"; }

            return Out;
        }

        public static string Choose(int code, string filetype, out string[] FileNames)
        {
            string MsgPack = GetArgs("MessagePack", true, "mp");
            string JSON = GetArgs("JSON", true, "json");
            string BIN = GetArgs("BIN", true, "bin");
            string WAV = GetArgs("WAV", true, "wav");

            FileNames = new string[0];
            if (code == 1)
            {
                OpenFileDialog ofd = new OpenFileDialog { InitialDirectory = Application.StartupPath,
                    Multiselect = true, Title = "Choose file(s) to open:" };

                ofd.Filter = GetArgs("All;", false, "*");
                     if (filetype == "a3da") ofd.Filter = GetArgs("A3DA", "a3da", "farc", "json", "mp") +
                        GetArgs("A3DA", true, "a3da") +   GetArgs("FARC", true, "farc") + JSON + MsgPack;
                else if (filetype == "bin" ) ofd.Filter = GetArgs("BIN" , "bin", "json", "mp") +
                        BIN + JSON + MsgPack;
                else if (filetype == "blt" ) ofd.Filter = GetArgs("BLT" , "blt");
                else if (filetype == "bon" ) ofd.Filter = GetArgs("BON" , "bon", "bin", "json", "mp") +
                        GetArgs("BON", true, "bon") + BIN + JSON + MsgPack;
                else if (filetype == "cct" ) ofd.Filter = GetArgs("CCT" , "cct");
                else if (filetype == "databank") ofd.Filter = GetArgs("DAT", "dat", "json", "mp") +
                        GetArgs("DAT", true, "dat") + JSON + MsgPack;
                else if (filetype == "dex" ) ofd.Filter = GetArgs("DEX" , "dex", "bin", "json", "mp") +
                        GetArgs("DEX", true, "dex") + BIN + JSON + MsgPack;
                else if (filetype == "dft")  ofd.Filter = GetArgs("DFT" , "dft");
                else if (filetype == "diva") ofd.Filter = GetArgs("DIVA", "diva", "wav") +
                        GetArgs("DIVA", true, "diva") + GetArgs("WAV", true, "wav");
                else if (filetype == "dsc" ) ofd.Filter = GetArgs("DSC" , "dsc", "json", "mp") +
                        GetArgs("DSC", true, "dsc") + JSON + MsgPack;
                else if (filetype == "farc") ofd.Filter = "FARC Archives (*.farc)|*.farc";
                else if (filetype == "json") ofd.Filter = "JSON (*.json)|*.json";
                else if (filetype == "mp"  ) ofd.Filter = GetArgs("MessagePack", "mp");
                else if (filetype == "lit")  ofd.Filter = GetArgs("LIT" , "lit");
                else if (filetype == "str" ) ofd.Filter = GetArgs("STR" , "str", "bin", "json", "mp") +
                        GetArgs("STR", true, "str") + BIN + JSON + MsgPack;
                else if (filetype == "vag" ) ofd.Filter = GetArgs("VAG" , "vag", "wav") +
                        GetArgs("VAG", true, "vag") + GetArgs("WAV", true, "wav");

                if (ofd.ShowDialog() == DialogResult.OK) FileNames = ofd.FileNames;
                ofd.Dispose();
            }
            else if (code == 2)
            {
                OpenFileDialog ofd = new OpenFileDialog { InitialDirectory = Application.StartupPath,
                    ValidateNames = false, CheckFileExists = false, Filter = " | ", CheckPathExists = true,
                    Title = "Choose any file in folder:", FileName = "Folder Selection." };
                string Return = "";
                if (ofd.ShowDialog() == DialogResult.OK)
                    Return = Path.GetDirectoryName(ofd.FileName);
                ofd.Dispose();
                return Return;
            }
            return "";
        }
        
        public static string NullTerminated(this string Source, ref int i, byte End)
        {
            string s = "";
            while (true)
            {
                if (Source[i] == End) break;
                else s += Source[i];
                i++;
            }
            return s;
        }

        public static bool StartsWith(this Dictionary<string, object> Dict, string args, char Split) =>
            Dict.StartsWith(args.Split(Split));

        public static bool StartsWith(this Dictionary<string, object> Dict, string args) =>
            Dict.StartsWith(args.Split('.'));

        public static bool StartsWith(this Dictionary<string, object> Dict, string[] args)
        {
            Dictionary<string, object> bufDict = new Dictionary<string, object>();
                 if (Dict == null)    return false;
            else if (args.Length < 1) return false;

            args[0] = args[0].ToLower();
            if (args.Length > 1)
            {
                string[] NewArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++)
                    NewArgs[i] = args[i + 1];
                if (!Dict.ContainsKey(args[0]))
                    return false;
                bufDict = (Dictionary<string, object>)Dict[args[0]];
                return StartsWith(bufDict, NewArgs);
            }
            return Dict.ContainsKey(args[0]);
        }

        public static bool FindValue(this Dictionary<string, object> Dict,
            ref   bool value, char Split, string args) =>
            Dict.FindValue(out string val, args.Split(Split)) ? bool.TryParse(val, out value) : false;

        public static bool FindValue(this Dictionary<string, object> Dict,
            ref    int value, char Split, string args) =>
            Dict.FindValue(out string val, args.Split(Split)) ?  int.TryParse(val, out value) : false;

        public static bool FindValue(this Dictionary<string, object> Dict,
            ref double value, char Split, string args) =>
            Dict.FindValue(out string val, args.Split(Split)) ?  val.ToDouble(     out value) : false;

        public static bool FindValue(this Dictionary<string, object> Dict,
            ref string value, char Split, string args)
        { if (Dict.FindValue(out string val, args.Split(Split)))
                           { value = val;             return true; } return false; }

        public static bool FindValue(this Dictionary<string, object> Dict,
            out   bool  value, string   args)
        { if (Dict.FindValue(out string val, args.Split('.'  )))
                return bool.TryParse(val, out value); value = false; return false; }

        public static bool FindValue(this Dictionary<string, object> Dict,
            out    int  value, string   args)
        { if (Dict.FindValue(out string val, args.Split('.'  )))
                return  int.TryParse(val, out value); value =     0; return false; }

        public static bool FindValue(this Dictionary<string, object> Dict,
            out double  value, string   args)
        { if (Dict.FindValue(out string val, args.Split('.'  )))
                return       val.ToDouble(out value); value =     0; return false; }

        public static bool FindValue(this Dictionary<string, object> Dict,
            out    int? value, string   args)
        { if (Dict.FindValue(out string val, args.Split('.'  )))
            { bool Val =  int.TryParse(val, out int _value);
                value = _value; return Val; }         value =  null; return false; }

        public static bool FindValue(this Dictionary<string, object> Dict,
            out double? value, string   args)
        { if (Dict.FindValue(out string val, args.Split('.'  )))
                return       val.ToDouble(out value); value =  null; return false; }

        public static bool FindValue(this Dictionary<string, object> Dict,
            out string  value, string   args)
        { if (Dict.FindValue(out string val, args.Split('.'  )))
              { value = val;           return true; } value =  null; return false; }

        public static bool FindValue(this Dictionary<string, object> Dict,
            out string  value, string[] args)
        {
            value = "";
                 if (Dict == null)    return false;
            else if (args.Length < 1) return false;

            args[0] = args[0].ToLower();
                 if (!Dict.ContainsKey(args[0])) return false;
            else if (args.Length > 1)
            {
                string[] NewArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++) NewArgs[i] = args[i + 1];
                return ((Dictionary<string, object>)Dict[args[0]]).FindValue(out value, NewArgs);
            }
            else if (args.Length == 1)
            {
                if (Dict[args[0]].GetType() == Dict.GetType())
                    return ((Dictionary<string, object>)Dict[args[0]]).FindValue(out value, args);
                else if (Dict[args[0]].GetType() != typeof(string)) return false;
            }

            value = (string)Dict[args[0]];
            return true;
        }
        
        public static void GetDictionary(this Dictionary<string, object> Dict,
            string args, string value, char Split = '.') =>
            Dict.GetDictionary(args.Split(Split), value);

        public static void GetDictionary(this Dictionary<string, object> Dict,
            string[] args, string value)
        {
            Dictionary<string, object> bufDict = new Dictionary<string, object>();
                 if (Dict == null) Dict = new Dictionary<string, object>();
            else if (args.Length < 1) return;

            args[0] = args[0].ToLower();
            if (args.Length > 1)
            {
                string[] NewArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++) NewArgs[i] = args[i + 1];
                if (!Dict.ContainsKey(args[0])) Dict.Add(args[0], null);
                if (Dict[args[0]] != null)
                {
                    if (Dict[args[0]].GetType() == typeof(string))
                        Dict[args[0]] = new Dictionary<string, object> { { "", Dict[args[0]] } };
                }
                else Dict[args[0]] = new Dictionary<string, object>();
                bufDict = (Dictionary<string, object>)Dict[args[0]];
                bufDict.GetDictionary(NewArgs, value);
                Dict[args[0]] = bufDict;
            }
            else if (!Dict.ContainsKey(args[0])) Dict.Add(args[0], value);
        }

        public static TKey GetKey<TKey, TVal>(this Dictionary<TKey, TVal> Dict, TVal val) =>
            Dict.First((KeyValuePair<TKey, TVal> x) => x.Value.Equals(val)).Key;

        public static string ToTitleCase(this string s)
        { return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s); }

        public static int[] SortWriter(this int Length)
        {
            int i = 0;
            List<string> A = new List<string>();
            for (i = 0; i < Length; i++) A.Add(i.ToString());
            A.Sort();
            int[] B = new int[Length];
            for (i = 0; i < Length; i++) B[i] = int.Parse(A[i]);
            return B;
        }
    }
}
