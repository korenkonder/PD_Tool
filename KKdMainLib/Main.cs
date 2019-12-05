using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using KKdBaseLib;

namespace KKdMainLib
{
    public static unsafe class Main
    {
        public const string TimeFormatHHmmssfff = "{0:d2}:{1:d2}:{2:d2}.{3:d3}";

        public static void WriteTime(this TimeSpan time, bool writeLine = false)
        {
            if (writeLine) Console.WriteLine(TimeFormatHHmmssfff,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            else           Console.Write    (TimeFormatHHmmssfff,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public static void WriteTime(this TimeSpan time, string Text, bool writeLine = true)
        {
            if (writeLine) Console.WriteLine(TimeFormatHHmmssfff + " - " + Text,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            else           Console.Write    (TimeFormatHHmmssfff + " - " + Text,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public static string NullTerminated(this string source, ref int i, byte end)
        {
            string s = "";
            while (true)
            {
                if (source[i] == end) break;
                else s += source[i];
                i++;
            }
            return s;
        }

        public static bool StartsWith(this Dictionary<string, object> dict, string args, char split = '.') =>
            dict.StartsWith(args.Split(split));

        public static bool StartsWith(this Dictionary<string, object> dict, string[] args)
        {
            Dictionary<string, object> bufDict = new Dictionary<string, object>();
                 if (dict == null)    return false;
            else if (args.Length < 1) return false;

            args[0] = args[0].ToLower();
            if (args.Length > 1)
            {
                string[] newArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++)
                    newArgs[i] = args[i + 1];
                if (!dict.ContainsKey(args[0]))
                    return false;
                bufDict = (Dictionary<string, object>)dict[args[0]];
                return StartsWith(bufDict, newArgs);
            }
            return dict.ContainsKey(args[0]);
        }

        public static bool FindValue(this Dictionary<string, object> dict,
            ref   bool value, char split, string args) =>
            dict.FindValue(out string val, args.Split(split)) ? bool.TryParse(val, out value) : false;

        public static bool FindValue(this Dictionary<string, object> dict,
            ref    int value, char split, string args) =>
            dict.FindValue(out string val, args.Split(split)) ?  int.TryParse(val, out value) : false;

        public static bool FindValue(this Dictionary<string, object> dict,
            ref  float value, char split, string args) =>
            dict.FindValue(out string val, args.Split(split)) ?  val.ToF32(     out value) : false;

        public static bool FindValue(this Dictionary<string, object> dict,
            ref double value, char split, string args) =>
            dict.FindValue(out string val, args.Split(split)) ?  val.ToF64(     out value) : false;

        public static bool FindValue(this Dictionary<string, object> Dict,
            ref string value, char split, string args)
        { if (Dict.FindValue(out string val, args.Split(split)))
                           { value = val;             return true; } return false; }

        public static bool FindValue(this Dictionary<string, object> dict,
            out   bool  value, string   args)
        { if (dict.FindValue(out string val, args.Split('.'  )))
                return bool.TryParse(val, out value); value = false; return false; }

        public static bool FindValue(this Dictionary<string, object> Dict,
            out    int  value, string   args)
        { if (Dict.FindValue(out string val, args.Split('.'  )))
                return  int.TryParse(val, out value); value =     0; return false; }

        public static bool FindValue(this Dictionary<string, object> dict,
            out  float  value, string   args)
        { if (dict.FindValue(out string val, args.Split('.'  )))
                return       val.ToF32(out value); value =     0; return false; }

        public static bool FindValue(this Dictionary<string, object> dict,
            out double  value, string   args)
        { if (dict.FindValue(out string val, args.Split('.'  )))
                return       val.ToF64(out value); value =     0; return false; }

        public static bool FindValue(this Dictionary<string, object> dict,
            out    int? value, string   args)
        { if (dict.FindValue(out string val, args.Split('.'  )))
            { bool Val =  int.TryParse(val, out int _value);
                value = _value; return Val; }         value =  null; return false; }

        public static bool FindValue(this Dictionary<string, object> dict,
            out  float? value, string   args)
        { if (dict.FindValue(out string val, args.Split('.'  )))
                return       val.ToF32(out value); value =  null; return false; }

        public static bool FindValue(this Dictionary<string, object> dict,
            out double? value, string   args)
        { if (dict.FindValue(out string val, args.Split('.'  )))
                return       val.ToF64(out value); value =  null; return false; }

        public static bool FindValue(this Dictionary<string, object> dict,
            out string  value, string   args)
        { if (dict.FindValue(out string val, args.Split('.'  )))
              { value = val;           return true; } value =  null; return false; }

        public static bool FindValue(this Dictionary<string, object> dict,
            out string  value, string[] args)
        {
            value = "";
                 if (dict == null)    return false;
            else if (args.Length < 1) return false;

            args[0] = args[0].ToLower();
                 if (!dict.ContainsKey(args[0])) return false;
            else if (args.Length > 1)
            {
                string[] newArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++) newArgs[i] = args[i + 1];
                return ((Dictionary<string, object>)dict[args[0]]).FindValue(out value, newArgs);
            }
            else if (args.Length == 1)
            {
                if (dict[args[0]].GetType() == dict.GetType())
                    return ((Dictionary<string, object>)dict[args[0]]).FindValue(out value, args);
                else if (dict[args[0]].GetType() != typeof(string)) return false;
            }

            value = (string)dict[args[0]];
            return true;
        }

        public static void GetDictionary(this Dictionary<string, object> dict,
            string args, string value, char split = '.') =>
            dict.GetDictionary(args.Split(split), value);

        public static void GetDictionary(this Dictionary<string, object> dict,
            string[] args, string value)
        {
            Dictionary<string, object> bufDict = new Dictionary<string, object>();
                 if (dict == null) dict = new Dictionary<string, object>();
            else if (args.Length < 1) return;

            args[0] = args[0].ToLower();
            if (args.Length > 1)
            {
                string[] newArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++) newArgs[i] = args[i + 1];
                if (!dict.ContainsKey(args[0])) dict.Add(args[0], null);
                if (dict[args[0]] != null)
                {
                    if (dict[args[0]].GetType() == typeof(string))
                        dict[args[0]] = new Dictionary<string, object> { { "", dict[args[0]] } };
                }
                else dict[args[0]] = new Dictionary<string, object>();
                bufDict = (Dictionary<string, object>)dict[args[0]];
                bufDict.GetDictionary(newArgs, value);
                dict[args[0]] = bufDict;
            }
            else if (!dict.ContainsKey(args[0])) dict.Add(args[0], value);
        }

        public static TKey GetKey<TKey, TVal>(this Dictionary<TKey, TVal> dict, TVal val) =>
            dict.First((System.Collections.Generic.KeyValuePair<TKey, TVal> x) => x.Value.Equals(val)).Key;

        public static string ToTitleCase(this string s)
        { return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s); }

        public static int[] SortWriter(this int length)
        {
            int i = 0;
            List<string> A = new List<string>();
            for (i = 0; i < length; i++) A.Add(i.ToString());
            A.Sort();
            int[] B = new int[length];
            for (i = 0; i < length; i++) B[i] = int.Parse(A[i]);
            return B;
        }
    }
}
