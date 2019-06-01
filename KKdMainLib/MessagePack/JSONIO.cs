//Original or reader part: https://github.com/MarcosLopezC/LightJson/

using System;
using System.Collections.Generic;
using KKdMainLib.IO;

namespace KKdMainLib.MessagePack
{
    public class JSONIO
    {
        public Stream _IO;

        public JSONIO(         ) => _IO = File.OpenWriter();
        public JSONIO(Stream IO) => _IO = IO;

        public void Close() => _IO.Close();

        public MsgPack Read() => ReadValue(null);

        private string ReadKey() => ReadString();

        private MsgPack ReadValue(string Key)
        {
            char c = _IO.SkipWhitespace().PeekCharUTF8();
            object obj = null;
            if (char.IsDigit(c))
                          obj = ReadNumber ();
            switch (c)
            {
                case '"': obj = ReadString (); break;
                case '{': obj = ReadObject (); break;
                case '[': obj = ReadArray  (); break;
                case '-': obj = ReadNumber (); break;
                case 't':
                case 'f': obj = ReadBoolean(); break;
                case 'n': obj = ReadNull   (); break;
            }
            return new MsgPack(Key, obj);
        }
        
		private string ReadString()
		{
			if (!_IO.Assert('"')) return null;
            char c;
            string s = "";
			while (true)
			{
                c = _IO.ReadCharUTF8();

				if (c == '\\')
				{
					c = _IO.ReadCharUTF8();

					switch (char.ToLower(c))
					{
						case '"' :
						case '\\':
						case '/' : s += c; break;
						case 'b' : s += '\b'; break;
						case 'f' : s += '\f'; break;
                        case 'n' : s += '\n'; break;
						case 'r' : s += '\r'; break;
						case 't' : s += '\t'; break;
						case 'u' : s += ReadUnicodeLiteral(); break;
						default: return null;
					}
				}
				else if (c == '"') break;
				else if (char.IsControl(c)) return null;
                else s += c;
			}

			return s;
		}

		private char ReadUnicodeLiteral() =>
            (char)((((((ReadHexDigit() << 4) | ReadHexDigit()) << 4) | ReadHexDigit()) << 4) | ReadHexDigit());

		private int ReadHexDigit() => byte.Parse(_IO.ReadCharUTF8().ToString(),
            System.Globalization.NumberStyles.HexNumber);

        private List<object> ReadObject()
        {
            List<object> Obj = new List<object>();
            if (!_IO.Assert('{')) return null;
			if (_IO.SkipWhitespace().PeekCharUTF8() == '}') { _IO.ReadCharUTF8(); return null; }

            string key;
            while (true)
            {
                _IO.SkipWhitespace();

                key = ReadString();
                if (!_IO.SkipWhitespace().Assert(':')) return null;
                Obj.Add(ReadValue(key));

                _IO.SkipWhitespace();

                var next = _IO.ReadCharUTF8();

                     if (next == '}')    break;
                else if (next == ',') continue;
                else return null;
            }

            return Obj;
		}
        
        private object[] ReadArray()
        {
            List<object> Obj = new List<object>();
            if (!_IO.Assert('[')) return null;
            if (_IO.SkipWhitespace().PeekCharUTF8() == ']') { _IO.ReadCharUTF8(); return null; }

            char c;
            while (true)
            {
                Obj.Add(ReadValue(null));
                c = _IO.SkipWhitespace().ReadCharUTF8();

                     if (c == ']')    break;
                else if (c == ',') continue;
                else return null;
            }
            return Obj.ToArray();
		}

		private object ReadNumber()
		{
			string s = " ";
            _IO.SkipWhitespace();
            if (_IO.PeekCharUTF8() == '-') s += _IO.ReadCharUTF8();
			if (_IO.PeekCharUTF8() == '0') s += _IO.ReadCharUTF8();
            else                           s +=     ReadDigits  ();
			if (_IO.PeekCharUTF8() == '.') s += _IO.ReadCharUTF8() + ReadDigits();
            else return long.Parse(s);

            char c = _IO.PeekCharUTF8();
            if (c == 'e' || c == 'E')
			{
                s += _IO.ReadCharUTF8();
                c  = _IO.PeekCharUTF8();
                if (c == '+' || c == '-') s += _IO.ReadCharUTF8();
                s += ReadDigits();
			}
			return s.ToDouble();
        }

		private bool ReadBoolean()
        {
            char c = _IO.PeekCharUTF8();
                 if (c == 't' && _IO.Assert( "true")) return  true;
            else if (c == 'f' && _IO.Assert("false")) return false;
            return false;
        }

		private bool ReadNull() => _IO.Assert("null");

        private string ReadDigits()
        { string s = ""; while (char.IsDigit(_IO.SkipWhitespace().
            PeekCharUTF8())) s += _IO.ReadCharUTF8(); return s; }

        public JSONIO Write(MsgPack MsgPack, bool Close, string End = "\n", string TabChar = "\t")
        { Write(MsgPack, End, TabChar, "", true); if (Close) this.Close(); return this; }

        public JSONIO Write(MsgPack MsgPack, bool Close, bool Style = false)
        { Write(MsgPack, "\n", "\t", "", Style); if (Close) this.Close(); return this; }

        private JSONIO Write(MsgPack MsgPack, string End, string TabChar, string Tab, bool Style)
        {
            string OldTab = Tab;
            Tab += TabChar;
            if (MsgPack.Name   != null) _IO.Write("\"" + MsgPack.Name + "\":" + (Style ? " " : ""));
            if (MsgPack.Object == null) { WriteNil(); return this; }

            Type type = MsgPack.Object.GetType();
            if (type == typeof(List<object>))
            {
                List<object> Obj = (List<object>)MsgPack.Object;
                WriteMap();
                if (Style) _IO.Write(End);
                for (int i = 0; i < Obj. Count; i++)
                {
                    if (Style) _IO.Write(Tab);
                    Write(Obj[i], Obj[i].GetType(), End, TabChar, Tab, Style);
                    if (i + 1 < Obj. Count) _IO.Write(',');
                    if (Style) _IO.Write(End);
                }
                if (Style) _IO.Write(OldTab);
                WriteMap(true);
            }
            else if (type == typeof(object[]))
            {
                object[] Obj = (object[])MsgPack.Object;
                WriteArr();
                if (Style) _IO.Write(End);
                for (int i = 0; i < Obj.Length; i++)
                {
                    if (Style) _IO.Write(Tab);
                    Write(Obj[i], Obj[i].GetType(), End, TabChar, Tab, Style);
                    if (i + 1 < Obj.Length) _IO.Write(',');
                    if (Style) _IO.Write(End);
                }
                if (Style) _IO.Write(OldTab);
                WriteArr(true);
            }
            else if (type == typeof(MsgPack))
                Write((MsgPack)MsgPack.Object, End, TabChar, Tab, Style);
            else Write(MsgPack.Object, type, End, TabChar, Tab, Style);

            return this;
        }

        private void Write(object obj, Type type, string End, string TabChar, string Tab, bool Style)
        {
            if (obj == null) { WriteNil(); return; }
            switch (obj)
            {
                case MsgPack val: Write(val, End, TabChar, Tab, Style); break;
                case    bool val: Write(val); break;
                case  string val: Write(val); break;
                case   sbyte val: _IO.Write(Main.ToString(val)); break;
                case    byte val: _IO.Write(Main.ToString(val)); break;
                case   short val: _IO.Write(Main.ToString(val)); break;
                case  ushort val: _IO.Write(Main.ToString(val)); break;
                case     int val: _IO.Write(Main.ToString(val)); break;
                case    uint val: _IO.Write(Main.ToString(val)); break;
                case    long val: _IO.Write(Main.ToString(val)); break;
                case   ulong val: _IO.Write(Main.ToString(val)); break;
                case   float val: _IO.Write(Main.ToString(val)); break;
                case  double val: _IO.Write(Main.ToString(val)); break;
            }
        }
        private void Write(  bool val) => _IO.Write(val ? "true" : "false");
        private void Write(string val) => _IO.Write("\"" + val
            .Replace("\\", "\\\\").Replace("/" , "\\/").Replace("\'", "\\\'").Replace("\"", "\\\"")
            .Replace("\0", "\\0" ).Replace("\a", "\\a").Replace("\b", "\\b" ).Replace("\f", "\\f" )
            .Replace("\n", "\\n" ).Replace("\r", "\\r").Replace("\t", "\\t" ) + "\"");

        private void WriteNil() => _IO.Write("null");
        private void WriteArr(bool End = false) => _IO.Write(End ? "]" : "[");
        private void WriteMap(bool End = false) => _IO.Write(End ? "}" : "{");
    }
}
