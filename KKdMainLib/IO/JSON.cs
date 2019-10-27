//Original or reader part: https://github.com/MarcosLopezC/LightJson/

using KKdBaseLib;
using BaseExtensions = KKdBaseLib.Extensions;

namespace KKdMainLib.IO
{
    public struct JSON : System.IDisposable
    {
        public JSON(Stream IO) => _IO = IO;

        private Stream _IO;

        public void Close() => _IO.C();

        public byte[] ToArray(bool Close = false) => _IO.ToArray(Close);

        public MsgPack Read() => ReadValue();

        private MsgPack ReadValue(string Key = null)
        {
            char c = _IO.SW().PCUTF8();
            object obj = null;
            if (char.IsDigit(c))
                              obj = RF ();
            else
                switch (c)
                {
                    case '"': obj = RS (); break;
                    case '{': obj = RO (); break;
                    case '[': obj = RA (); break;
                    case '-': obj = RF (); break;
                    case 't':
                    case 'f': obj = RBo(); break;
                    case 'n': obj = RN (); break;
                }
            return new MsgPack(Key, obj);
        }
        
		private string RS()
		{
			if (!Extensions.A(_IO, '"')) return null;
            char c;
            string s = "";
			while (true)
			{
                c = _IO.RCUTF8();

				if (c == '\\')
				{
					c = _IO.RCUTF8();

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
						case 'u' : s += RUL(); break;
						default: return null;
					}
				}
				else if (c == '"') break;
				else if (char.IsControl(c))
                    return null;
                else s += c;
			}

			return s;
		}

		private char RUL() =>
            (char)((((((RHD() << 4) | RHD()) << 4) | RHD()) << 4) | RHD());

		private int RHD() => byte.Parse(_IO.RCUTF8().ToString(),
            System.Globalization.NumberStyles.HexNumber);

        private KKdList<MsgPack> RO()
        {
            KKdList<MsgPack> Obj = KKdList<MsgPack>.New;
            if (!Extensions.A(_IO, '{')) return KKdList<MsgPack>.Null;
			if (_IO.SW().PCUTF8() == '}')
            { _IO.RCUTF8(); return KKdList<MsgPack>.Null; }

            string key;
            char c;
            while (true)
            {
                _IO.SW();

                key = RS();
                if (!Extensions.A(_IO.SW(), ':'))
                    return KKdList<MsgPack>.Null;

                Obj.Add(ReadValue(key));
                c = _IO.SW().PCUTF8();
                
                     if (c == '}') { _IO.RCUTF8();    break; }
                else if (c == ',') { _IO.RCUTF8(); continue; }
                else return KKdList<MsgPack>.Null;
            }

            return Obj;
		}
        
        private MsgPack[] RA()
        {
            KKdList<MsgPack> Obj = KKdList<MsgPack>.New;
            if (!Extensions.A(_IO, '[')) return null;
            if (_IO.SW().PCUTF8() == ']')
            { _IO.RCUTF8(); return null; }

            char c;
            while (true)
            {
                Obj.Add(ReadValue(null));
                c = _IO.SW().PCUTF8();

                     if (c == ']') { _IO.RCUTF8();    break; }
                else if (c == ',') { _IO.RCUTF8(); continue; }
                else return null;
            }
            return Obj.ToArray();
		}

		private object RF()
		{
			string s = " ";
            _IO.SW();
            if (_IO.PCUTF8() == '-') s += _IO.RCUTF8();
			if (_IO.PCUTF8() == '0') s += _IO.RCUTF8();
            else                           s +=     ReadDigits  ();
            if (_IO.PCUTF8() == '.') s += _IO.RCUTF8() + ReadDigits();
            else
            {
                long val = long.Parse(s);
                     if (val >=  0x00000000 && val < 0x000000100) return (  byte)val;
                else if (val >= -0x00000080 && val < 0x000000080) return ( sbyte)val;
                else if (val >= -0x00008000 && val < 0x000008000) return ( short)val;
                else if (val >=  0x00000000 && val < 0x000010000) return (ushort)val;
                else if (val >= -0x80000000 && val < 0x000800000) return (   int)val;
                else if (val >=  0x00000000 && val < 0x100000000) return (  uint)val;
                else                                              return         val;
            }

            char c = _IO.PCUTF8();
            if (c == 'e' || c == 'E')
			{
                s += _IO.RCUTF8();
                c  = _IO.PCUTF8();
                if (c == '+' || c == '-') s += _IO.RCUTF8();
                s += ReadDigits();
			}
            double d = s.ToDouble();
            return (float)d == d ? (float)d : d;
        }

		private bool RBo()
        {
            char c = _IO.PCUTF8();
                 if (c == 't' && _IO.a( "true")) return  true;
            else if (c == 'f' && _IO.a("false")) return false;
            return false;
        }

		private object RN() { _IO.a("null"); return null; }

        private string ReadDigits()
        { string s = ""; while (char.IsDigit(_IO.SW().
            PCUTF8())) s += _IO.RCUTF8(); return s; }

        public JSON W(MsgPack MsgPack, string End = "\n", string TabChar = "  ") =>
            W(MsgPack, End, TabChar, "", true);

        public JSON W(MsgPack MsgPack, bool Style = false) =>
                W(MsgPack, "\n", "  ", "", Style);

        private JSON W(MsgPack MsgPack, string End, string TabChar, string Tab, bool Style, bool IsArray = false)
        {
            string OldTab = Tab;
            Tab += TabChar;
            if (MsgPack.Name   != null && !IsArray) _IO.W("\"" + MsgPack.Name + "\":" + (Style ? " " : ""));
            if (MsgPack.Object == null) { WN(); return this; }

            if (MsgPack.List.NotNull)
            {
                WM();
                if (Style) _IO.W(End);
                if (MsgPack.List.Count > 1)
                    for (int i = 0; i < MsgPack.List.Count; i++)
                    {
                        if (Style) _IO.W(Tab);
                        W(MsgPack.List[i], End, TabChar, Tab, Style);
                        if (i + 1 < MsgPack.List.Count) _IO.W(',');
                        if (Style) _IO.W(End);
                    }
                else if (MsgPack.List.Count == 1)
                {
                    if (Style) _IO.W(Tab);
                    W(MsgPack.List[0], End, TabChar, Tab, Style);
                    if (Style) _IO.W(End);
                }
                if (Style) _IO.W(OldTab);
                WM(true);
            }
            else if (MsgPack.Array != null)
            {
                WA();
                if (Style) _IO.W(End);
                if (MsgPack.Array.Length > 1)
                    for (int i = 0; i < MsgPack.Array.Length; i++)
                    {
                        if (Style) _IO.W(Tab);
                        W(MsgPack.Array[i], End, TabChar, Tab, Style, true);
                        if (i + 1 < MsgPack.Array.Length) _IO.W(',');
                        if (Style) _IO.W(End);
                    }
                else if (MsgPack.Array.Length == 1)
                {
                    if (Style) _IO.W(Tab);
                    W(MsgPack.Array[0], End, TabChar, Tab, Style, true);
                    if (Style) _IO.W(End);
                }
                if (Style) _IO.W(OldTab);
                WA(true);
            }
            else if (MsgPack.Object is MsgPack msg) W(msg, End, TabChar, Tab, Style);
            else if (MsgPack.Object is  string str) W(str);
            else _IO.W(BaseExtensions.ToString(MsgPack.Object));

            return this;
        }

        public void Dispose() => _IO.C();

        private void W(string val) => _IO.W("\"" + val
            .Replace("\\", "\\\\").Replace("/" , "\\/").Replace("\"", "\\\"")
            .Replace("\0", "\\0" ).Replace("\b", "\\b").Replace("\f", "\\f" )
            .Replace("\n", "\\n" ).Replace("\r", "\\r").Replace("\t", "\\t" ) + "\"");

        private void WN() => _IO.W("null");
        private void WA(bool End = false) => _IO.W(End ? "]" : "[");
        private void WM(bool End = false) => _IO.W(End ? "}" : "{");
    }
}
