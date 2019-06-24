//Original or reader part: https://github.com/MarcosLopezC/LightJson/

using System;
using KKdMainLib.IO;
using KKdMainLib.Types;

namespace KKdMainLib.MessagePack
{
    public class JSONIO
    {
        public Stream _IO;

        public JSONIO(         ) => _IO = File.OpenWriter();
        public JSONIO(Stream IO) => _IO = IO;

        public void Close() => _IO.Close();

        public MsgPack Read() => ReadValue();

        private string ReadKey() => ReadString();

        private MsgPack ReadValue(string Key = null)
        {
            char c = _IO.SkipWhitespace().PeekCharUTF8();
            object obj = null;
            if (char.IsDigit(c))
                              obj = ReadNumber ();
            else
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

        private KKdList<object> ReadObject()
        {
            KKdList<object> Obj = KKdList<object>.New;
            if (!_IO.Assert('{')) return KKdList<object>.Null;
			if (_IO.SkipWhitespace().PeekCharUTF8() == '}') { _IO.ReadCharUTF8(); return KKdList<object>.Null; }

            string key;
            char c;
            while (true)
            {
                _IO.SkipWhitespace();

                key = ReadString();
                if (!_IO.SkipWhitespace().Assert(':'))
                    return KKdList<object>.Null;

                Obj.Add(ReadValue(key));
                c = _IO.SkipWhitespace().PeekCharUTF8();
                
                     if (c == '}') { _IO.ReadCharUTF8();    break; }
                else if (c == ',') { _IO.ReadCharUTF8(); continue; }
                else return KKdList<object>.Null;
            }

            return Obj;
		}
        
        private object[] ReadArray()
        {
            KKdList<object> Obj = KKdList<object>.New;
            if (!_IO.Assert('[')) return null;
            if (_IO.SkipWhitespace().PeekCharUTF8() == ']') { _IO.ReadCharUTF8(); return null; }

            char c;
            while (true)
            {
                Obj.Add(ReadValue(null));
                c = _IO.SkipWhitespace().PeekCharUTF8();

                     if (c == ']') { _IO.ReadCharUTF8();    break; }
                else if (c == ',') { _IO.ReadCharUTF8(); continue; }
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
            else
            {
                long val = long.Parse(s);
                     if (val >=  0x000000 && val < 0x0000100) return (  byte)val;
                else if (val >= -0x000080 && val < 0x0000080) return ( sbyte)val;
                else if (val >= -0x008000 && val < 0x0008000) return ( short)val;
                else if (val >=  0x000000 && val < 0x0010000) return (ushort)val;
                else if (val >= -0x800000 && val < 0x0800000) return (   int)val;
                else if (val >=  0x000000 && val < 0x1000000) return (  uint)val;
                else                                          return         val;
            }

            char c = _IO.PeekCharUTF8();
            if (c == 'e' || c == 'E')
			{
                s += _IO.ReadCharUTF8();
                c  = _IO.PeekCharUTF8();
                if (c == '+' || c == '-') s += _IO.ReadCharUTF8();
                s += ReadDigits();
			}
            double d = s.ToDouble();
            return (float)d == d ? (float)d : d;
        }

		private bool ReadBoolean()
        {
            char c = _IO.PeekCharUTF8();
                 if (c == 't' && _IO.Assert( "true")) return  true;
            else if (c == 'f' && _IO.Assert("false")) return false;
            return false;
        }

		private object ReadNull() { _IO.Assert("null"); return null; }

        private string ReadDigits()
        { string s = ""; while (char.IsDigit(_IO.SkipWhitespace().
            PeekCharUTF8())) s += _IO.ReadCharUTF8(); return s; }

        public JSONIO Write(MsgPack MsgPack, bool Close, string End = "\n", string TabChar = "  ")
        { Write(MsgPack, End, TabChar, "", true); if (Close) this.Close(); return this; }

        public JSONIO Write(MsgPack MsgPack, bool Close, bool Style = false)
        { Write(MsgPack, "\n", "  ", "", Style); if (Close) this.Close(); return this; }

        private JSONIO Write(MsgPack MsgPack, string End, string TabChar, string Tab, bool Style)
        {
            string OldTab = Tab;
            Tab += TabChar;
            if (MsgPack.Name   != null) _IO.Write("\"" + MsgPack.Name + "\":" + (Style ? " " : ""));
            if (MsgPack.Object == null) { WriteNil(); return this; }
            
            if (MsgPack.List.NotNull)
            {
                WriteMap();
                if (Style) _IO.Write(End);
                for (int i = 0; i < MsgPack.List.Count; i++)
                {
                    if (Style) _IO.Write(Tab);
                    Write(MsgPack.List[i], End, TabChar, Tab, Style);
                    if (i + 1 < MsgPack.List. Count) _IO.Write(',');
                    if (Style) _IO.Write(End);
                }
                if (Style) _IO.Write(OldTab);
                WriteMap(true);
            }
            else if (MsgPack.Array != null)
            {
                WriteArr();
                if (Style) _IO.Write(End);
                for (int i = 0; i < MsgPack.Array.Length; i++)
                {
                    if (Style) _IO.Write(Tab);
                    Write(MsgPack.Array[i], End, TabChar, Tab, Style);
                    if (i + 1 < MsgPack.Array.Length) _IO.Write(',');
                    if (Style) _IO.Write(End);
                }
                if (Style) _IO.Write(OldTab);
                WriteArr(true);
            }
            else if (MsgPack.Object is MsgPack msg)
                Write(msg, End, TabChar, Tab, Style);
            else Write(MsgPack.Object, End, TabChar, Tab, Style);

            return this;
        }

        private void Write(object obj, string End, string TabChar, string Tab, bool Style)
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
