using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using A3DADict = System.Collections.Generic.Dictionary<string, object>;

namespace KKdBaseLib
{
    public static unsafe class Main
    {
        public const string TimeFormatHHmmssfff = "{0:d2}:{1:d2}:{2:d2}.{3:d3}";

        public static void WT(this TimeSpan time, bool writeLine = false)
        {
            if (writeLine) Console.WriteLine(TimeFormatHHmmssfff,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            else           Console.Write    (TimeFormatHHmmssfff,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public static void WT(this TimeSpan time, string text, bool writeLine = true)
        {
            if (writeLine) Console.WriteLine(TimeFormatHHmmssfff + " - " + text,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
            else           Console.Write    (TimeFormatHHmmssfff + " - " + text,
                    time.Hours, time.Minutes, time.Seconds, time.Milliseconds);
        }

        public static string NT(this string source, ref int i, byte end)
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

        public static bool SW(this A3DADict dict, string args, char split = '.') =>
            dict.SW(args.Split(split));

        public static bool SW(this A3DADict dict, string[] args)
        {
            if (dict == null || args.Length < 1) return false;

            args[0] = args[0].ToLower();
            if (args.Length > 1)
            {
                string[] newArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++)
                    newArgs[i] = args[i + 1];
                return dict.ContainsKey(args[0]) && SW((A3DADict)dict[args[0]], newArgs);
            }
            return dict.ContainsKey(args[0]);
        }

        public static bool FV   (this A3DADict dict, ref   bool value, char split, string args) =>
              dict.FV(out string val, args.Split(split)) && bool.TryParse(val, out value);

        public static bool FV   (this A3DADict dict, ref    int value, char split, string args) =>
              dict.FV(out string val, args.Split(split)) && int.TryParse(val, out value);

        public static bool FV   (this A3DADict dict, ref  float value, char split, string args) =>
              dict.FV(out string val, args.Split(split)) && val.ToF32(out value);

        public static bool FV   (this A3DADict dict, ref double value, char split, string args) =>
              dict.FV(out string val, args.Split(split)) && val.ToF64(out value);

        public static bool FV   (this A3DADict dict, ref string value, char split, string args)
        { if (dict.FV(out string val  , args.Split(split)))
                       { value = val;                 return  true; } return false; }

        public static bool FV   (this A3DADict dict, out   bool  value, string   args)
        { if (dict.FV(out string val  , args.Split('.'  )))
                return bool.TryParse(val, out value); value = false; return false; }

        public static bool FV   (this A3DADict dict, out    int  value, string   args)
        { if (dict.FV(out string val  , args.Split('.'  )))
                return  int.TryParse(val, out value); value =      0; return false; }

        public static bool FV   (this A3DADict dict, out  float  value, string   args)
        { if (dict.FV(out string val  , args.Split('.'  )))
                return           val.ToF32(out value); value =     0; return false; }

        public static bool FV   (this A3DADict dict, out double  value, string   args)
        { if (dict.FV(out string val  , args.Split('.'  )))
                return           val.ToF64(out value); value =     0; return false; }

        public static bool FV<T>(this A3DADict dict, out      T  value, string   args) where T : struct
        { if (dict.FV(out string val  , args.Split('.'  )))
            { bool Val = Enum.TryParse(val, out   T _value); value = _value; return Val; }
                                                     value = default; return false; }

        public static bool FV   (this A3DADict dict, out    int? value, string   args)
        { if (dict.FV(out string val  , args.Split('.'  )))
            { bool Val =  int.TryParse(val, out int _value); value = _value; return Val; }
                                                     value =    null; return false; }

        public static bool FV   (this A3DADict dict, out  float? value, string   args)
        { if (dict.FV(out string val  , args.Split('.'  )))
                return           val.ToF32(out value); value =  null; return false; }

        public static bool FV   (this A3DADict dict, out double? value, string   args)
        { if (dict.FV(out string val  , args.Split('.'  )))
                return           val.ToF64(out value); value =  null; return false; }

        public static bool FV<T>(this A3DADict dict, out      T? value, string   args) where T : struct
        { if (dict.FV(out string val  , args.Split('.'  )))
            { bool Val = Enum.TryParse(val, out   T _value); value = _value; return Val; }
                                                     value =    null; return false; }

        public static bool FV   (this A3DADict dict, out string  value, string   args) =>
              dict.FV(out        value, args.Split('.'  ));

        public static bool FV(this A3DADict dict, out string  value, string[] args)
        {
            value = null;
            if (dict == null || args.Length < 1) return false;

            args[0] = args[0].ToLower();
                 if (!dict.ContainsKey(args[0])) return false;
            else if (args.Length > 1)
            {
                string[] newArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++) newArgs[i] = args[i + 1];
                return ((A3DADict)dict[args[0]]).FV(out value, newArgs);
            }
            else if (args.Length == 1)
            {
                if (dict[args[0]].GetType() == dict.GetType())
                    return ((A3DADict)dict[args[0]]).FV(out value, args);
                else if (dict[args[0]].GetType() != typeof(string)) return false;
            }

            value = (string)dict[args[0]];
            return true;
        }

        public static bool FK(this A3DADict dict, string   args) =>
              dict.FK(args.Split('.'  ));

        public static bool FK(this A3DADict dict, string[] args)
        {
            if (dict == null || args.Length < 1) return false;

            args[0] = args[0].ToLower();
                 if (!dict.ContainsKey(args[0])) return false;
            else if (args.Length > 1)
            {
                string[] newArgs = new string[args.Length - 1];
                for (int i = 0; i < args.Length - 1; i++) newArgs[i] = args[i + 1];
                return ((A3DADict)dict[args[0]]).FK(newArgs);
            }
            return true;
        }

        public static void GD(this A3DADict dict, string args, char split = '.')
        {
            string[] dataArray = args.Split('=');
            if (dataArray.Length == 2)
                dict.GD(dataArray[0].Split(split), dataArray[1]);
            dataArray = null;
        }

        public static void GD(this A3DADict dict, string args, string value, char split = '.') =>
            dict.GD(args.Split(split), value);

        public static void GD(this A3DADict dict, string[] args, string value)
        {
                 if (dict == null) dict = new A3DADict();
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
                        dict[args[0]] = new A3DADict { { "", dict[args[0]] } };
                }
                else dict[args[0]] = new A3DADict();
                A3DADict bufDict = (A3DADict)dict[args[0]];
                bufDict.GD(newArgs, value);
                dict[args[0]] = bufDict;
            }
            else if (!dict.ContainsKey(args[0])) dict.Add(args[0], value);
        }

        public static TKey GK<TKey, TVal>(this Dictionary<TKey, TVal> dict, TVal val) =>
            dict.First((System.Collections.Generic.KeyValuePair<TKey, TVal> x) => x.Value.Equals(val)).Key;

        public static string TTC(this string s) =>
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);

        public static int[] SW(this int length)
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
