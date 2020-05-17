//Original or reader part: https://github.com/MarcosLopezC/LightJson/

using KKdBaseLib;
using BaseExtensions = KKdBaseLib.Extensions;

namespace KKdMainLib.IO
{
    public struct JSON : System.IDisposable
    {
        public JSON(Stream _IO) => this._IO = _IO;

        private Stream _IO;

        public void Close() => _IO.C();

        public byte[] ToArray(bool Close = false) => _IO.ToArray(Close);

        public MsgPack Read() => ReadValue();

        private MsgPack ReadValue(string Key = null)
        {
            char c = _IO.SW().PCUTF8();
            object obj = null;
            if (char.IsDigit(c))
                obj = RF();
            else
                obj = c switch
                {
                    '"' => RS (),
                    '{' => RO (),
                    '[' => RA (),
                    '-' => RF (),
                    't' => RBo(),
                    'f' => RBo(),
                    'n' => RN (),
                    _   => null ,
                };
            return new MsgPack(Key, obj);
        }

        private string RS()
        {
            if (!Extensions.As(_IO, '"')) return null;
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
            KKdList<MsgPack> obj = KKdList<MsgPack>.New;
            if (!Extensions.As(_IO, '{')) return KKdList<MsgPack>.Null;
            if (_IO.SW().PCUTF8() == '}')
            { _IO.RCUTF8(); return KKdList<MsgPack>.Null; }

            string key;
            char c;
            while (true)
            {
                _IO.SW();

                key = RS();
                if (!Extensions.As(_IO.SW(), ':'))
                    return KKdList<MsgPack>.Null;

                obj.Add(ReadValue(key));
                c = _IO.SW().PCUTF8();

                     if (c == '}') { _IO.RCUTF8();    break; }
                else if (c == ',') { _IO.RCUTF8(); continue; }
                else return KKdList<MsgPack>.Null;
            }

            return obj;
        }

        private MsgPack[] RA()
        {
            KKdList<MsgPack> obj = KKdList<MsgPack>.New;
            if (!Extensions.As(_IO, '[')) return null;
            if (_IO.SW().PCUTF8() == ']')
            { _IO.RCUTF8(); return obj.ToArray(); }

            char c;
            while (true)
            {
                obj.Add(ReadValue(null));
                c = _IO.SW().PCUTF8();

                     if (c == ']') { _IO.RCUTF8();    break; }
                else if (c == ',') { _IO.RCUTF8(); continue; }
                else return null;
            }
            return obj.ToArray();
        }

        private object RF()
        {
            string s = " ";
            _IO.SW();
            if (_IO.PCUTF8() == '-') s += _IO.RCUTF8();
            if (_IO.PCUTF8() == '0') s += _IO.RCUTF8();
            else                     s +=     RD    ();
            char c = _IO.PCUTF8();
            if (c == '.') s += _IO.RCUTF8() + RD();
            else if (c != 'e' && c != 'E')
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

            c = _IO.PCUTF8();
            if (c == 'e' || c == 'E')
            {
                s += _IO.RCUTF8();
                c  = _IO.PCUTF8();
                if (c == '+' || c == '-') s += _IO.RCUTF8();
                s += RD();
            }
            double d = s.ToF64();
            return (float)d == d ? (float)d : d;
        }

        private bool RBo()
        {
            char c = _IO.PCUTF8();
                 if (c == 't' && _IO.As( "true")) return  true;
            else if (c == 'f' && _IO.As("false")) return false;
            return false;
        }

        private object RN() { _IO.As("null"); return null; }

        private string RD()
        { string s = ""; while (char.IsDigit(_IO.SW().
            PCUTF8())) s += _IO.RCUTF8(); return s; }

        public JSON W(MsgPack msgPack, string end = "\n", string tabChar = "  ") =>
            W(msgPack, end, tabChar, "", true);

        public JSON W(MsgPack msgPack, bool style = false) =>
                W(msgPack, "\n", "  ", "", style);

        private JSON W(MsgPack msgPack, string end, string tabChar, string tab, bool style, bool isArray = false)
        {
            string oldTab = tab;
            tab += tabChar;
            if (msgPack.Name   != null && !isArray) _IO.W("\"" + msgPack.Name + "\":" + (style ? " " : ""));
            if (msgPack.Object == null) { WN(); return this; }

            if (msgPack.List.NotNull)
            {
                WM();
                if (style) _IO.W(end);
                if (msgPack.List.Count > 1)
                    for (int i = 0; i < msgPack.List.Count; i++)
                    {
                        if (style) _IO.W(tab);
                        W(msgPack.List[i], end, tabChar, tab, style);
                        if (i + 1 < msgPack.List.Count) _IO.W(',');
                        if (style) _IO.W(end);
                    }
                else if (msgPack.List.Count == 1)
                {
                    if (style) _IO.W(tab);
                    W(msgPack.List[0], end, tabChar, tab, style);
                    if (style) _IO.W(end);
                }
                if (style) _IO.W(oldTab);
                WM(true);
            }
            else if (msgPack.Array != null)
            {
                WA();
                if (style) _IO.W(end);
                if (msgPack.Array.Length > 1)
                    for (int i = 0; i < msgPack.Array.Length; i++)
                    {
                        if (style) _IO.W(tab);
                        W(msgPack.Array[i], end, tabChar, tab, style, true);
                        if (i + 1 < msgPack.Array.Length) _IO.W(',');
                        if (style) _IO.W(end);
                    }
                else if (msgPack.Array.Length == 1)
                {
                    if (style) _IO.W(tab);
                    W(msgPack.Array[0], end, tabChar, tab, style, true);
                    if (style) _IO.W(end);
                }
                if (style) _IO.W(oldTab);
                WA(true);
            }
            else if (msgPack.Object is MsgPack msg) W(msg, end, tabChar, tab, style);
            else if (msgPack.Object is  string str) W(str);
            else _IO.W(BaseExtensions.ToS(msgPack.Object));

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
