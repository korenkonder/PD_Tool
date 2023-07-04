using KKdBaseLib;
using System;
using System.IO;
using BaseExtensions = KKdBaseLib.Extensions;

namespace KKdMainLib.IO
{
    public struct JSON : System.IDisposable
    {
        struct ReadBuffer
        {
            public char[] data;
            public int first;
            public int length;

            public ReadBuffer(int size)
            {
                data = new char[size];
                first = 0;
                length = 0;
            }
        };

        public JSON(Stream _IO) => this._IO = _IO;

        private Stream _IO;

        public void Close() => _IO.C();

        public byte[] ToArray(bool Close = false) => _IO.ToArray(Close);

        public MsgPack Read()
        {
            ReadBuffer buf = new ReadBuffer(0x400);
            buf.first = 0;
            buf.length = 0;
            buf.data[0] = '\0';
            buf.data[buf.data.Length - 1] = '\0';
            int c = ReadChar(ref buf);
            return new MsgPack(null, ReadInner(ref buf, ref c));
        }

        int ReadChar(ref ReadBuffer buf)
        {
            if (buf.length == 0)
            {
                buf.first = 1;
                buf.data[0] = buf.data[buf.data.Length - 1];
                byte[] temp = new byte[buf.data.Length];
                buf.length = _IO.RBy(buf.data.Length - 1, temp);
                Array.Copy(temp, 0, buf.data, 1, buf.length);
            }

            if (buf.length < 1)
                return -1;

            int c = buf.data[buf.first];
            buf.first++;
            buf.length--;
            return c;
        }

        void Seek(ref ReadBuffer buf, long offset)
        {
            if (buf.first < offset || offset >= buf.length - 1L) {
                long pos = _IO.PI64 - buf.length - offset;
                long off = pos % buf.data.Length;
                pos -= off;
                if (pos > 1)
                {
                    _IO.S(pos, SeekOrigin.Begin);
                    buf.first = (int)off;
                    byte[] temp = _IO.RBy(buf.data.Length);
                    buf.length = temp.Length;
                    Array.Copy(temp, 0, buf.data, 0, temp.Length);
                }
                else
                {
                    _IO.S(pos, SeekOrigin.Begin);
                    buf.first = (int)(off + 1);
                    buf.data[0] = '\0';
                    byte[] temp = _IO.RBy(buf.data.Length - 1);
                    buf.length = temp.Length;
                    Array.Copy(temp, 0, buf.data, 1, temp.Length);
                }
                buf.length -= (int)off;
            }
            else
            {
                buf.first -= (int)offset;
                buf.length += (int)offset;
            }
        }

        void SeekOne(ref ReadBuffer buf)
        {
            buf.first--;
            buf.length++;
        }

        object ReadInner(ref ReadBuffer buf, ref int c)
        {
            if (c == -1)
                return null;

            if (c == ' ' || c == '\n' || c == '\r' || c == '\t')
                c = ReadChar(ref buf);

            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return ReadFloat(ref buf, ref c);
                case '"':
                    return ReadString(ref buf, ref c);
                case '{':
                    return ReadMap(ref buf, ref c);
                case '[':
                    return ReadArray(ref buf, ref c);
                case 't':
                    return ReadBool(ref buf, ref c);
                case 'f':
                    return ReadBool(ref buf, ref c);
                case 'n':
                    return ReadNull(ref buf, ref c);
            }
            return null;
        }

        void ReadDigit(ref ReadBuffer buf, ref int c, byte[] dig_buf, ref int dig_buf_pos, int dig_buf_end) {
            int b = dig_buf_pos;
            while (c >= '0' && c <= '9' && b < dig_buf_end) {
                dig_buf[b++] = (byte)c;
                if ((c = ReadChar(ref buf)) == -1)
                    break;
            }
            dig_buf_pos = b;
        }

        object ReadFloat(ref ReadBuffer buf, ref int c)
        {
            byte[] dig_buf = new byte[0x100];
            int buf_pos = 0;
            int buf_end = dig_buf.Length;
            bool negate = false;
            bool zero = false;
            bool fraction = false;

            if (c == '-')
            {
                dig_buf[buf_pos++] = (byte)c;
                if ((c = ReadChar(ref buf)) == -1)
                    return null;
                negate = true;
            }

            if (c == '0')
            {
                dig_buf[buf_pos++] = (byte)c;
                if ((c = ReadChar(ref buf)) == -1)
                    return null;
                zero = true;
            }
            else
                ReadDigit(ref buf, ref c, dig_buf, ref buf_pos, buf_end);

            if (c == '.')
            {
                dig_buf[buf_pos++] = (byte)c;
                if ((c = ReadChar(ref buf)) == -1)
                    return null;
                ReadDigit(ref buf, ref c, dig_buf, ref buf_pos, buf_end);
                fraction = true;
            }

            if (zero && !fraction)
            {
                object obj;
                if (negate)
                    obj = -0.0f;
                else
                    obj = 0;
                SeekOne(ref buf);
                return obj;
            }
            else if (c != 'e' && c != 'E' && !fraction)
            {
                string temp = dig_buf.ToUTF8();
                int null_term = temp.IndexOf('\0');
                if (null_term >= 0)
                    temp = temp.Substring(0, null_term);
                if (!long.TryParse(temp, out long val))
                    return null;

                object obj;
                unchecked
                {
                    if (val >= 0x00 && val <= 0xFF)
                        obj = (byte)val;
                    else if (val >= (sbyte)0x80 && val <= (sbyte)0x7F)
                        obj = (sbyte)val;
                    else if (val >= 0x0000 && val <= 0xFFFF)
                        obj = (ushort)val;
                    else if (val >= (short)0x8000 && val <= (short)0x7FFF)
                        obj = (short)val;
                    else if (val >= 0x00000000 && val <= 0xFFFFFFFF)
                        obj = (uint)val;
                    else if (val >= (int)0x80000000 && val <= (int)0x7FFFFFFF)
                        obj = (int)val;
                    else
                        obj = val;
                }
                SeekOne(ref buf);
                return obj;
            }
            else if (c == 'e' || c == 'E')
            {
                dig_buf[buf_pos++] = (byte)c;
                if ((c = ReadChar(ref buf)) == -1)
                    return null;

                if (c == '+' || c == '-')
                {
                    dig_buf[buf_pos++] = (byte)c;
                    if ((c = ReadChar(ref buf)) == -1)
                        return null;
                }
                ReadDigit(ref buf, ref c, dig_buf, ref buf_pos, buf_end);
            }

            string ftemp = dig_buf.ToUTF8();
            int fnull_term = ftemp.IndexOf('\0');
            if (fnull_term >= 0)
                ftemp = ftemp.Substring(0, fnull_term);
            if (!ftemp.ToF64(out double fval))
                return null;

            object fobj;
            if (fval == (float)fval)
                fobj = (float)fval;
            else
                fobj = fval;
            SeekOne(ref buf);
            return fobj;
        }

        byte ReadStringHex(int c)
        {
            if (c >= '0' && c <= '9')
                return (byte)(c - '0');
            else if (c >= 'A' && c <= 'F')
                return (byte)(c - 'A' + 0xA);
            else if (c >= 'a' && c <= 'f')
                return (byte)(c - 'a' + 0xA);
            else
                return 0;
        }

        string ReadStringInner(ref ReadBuffer buf, ref int c)
        {
            if (c != '"')
                return null;

            long pos = _IO.PI64 - buf.length;
            ulong len = 0;
            while (c != -1)
            {
                c = ReadChar(ref buf);
                if (c == '\\')
                {
                    if ((c = ReadChar(ref buf)) == -1)
                        break;

                    switch (c)
                    {
                        case '"':
                        case '\\':
                        case '/':
                        case 'b':
                        case 'f':
                        case 'n':
                        case 'r':
                        case 't':
                            len++;
                            break;
                        case 'u':
                            {
                                uint uc = 0;
                                if ((c = ReadChar(ref buf)) == -1)
                                    break;
                                uc |= (uint)ReadStringHex(c) << 24;

                                if ((c = ReadChar(ref buf)) == -1)
                                    break;
                                uc |= (uint)ReadStringHex(c) << 16;

                                if ((c = ReadChar(ref buf)) == -1)
                                    break;
                                uc |= (uint)ReadStringHex(c) << 8;

                                if ((c = ReadChar(ref buf)) == -1)
                                    break;
                                uc |= (uint)ReadStringHex(c);

                                if (uc <= 0x7F)
                                    len++;
                                else if (c <= 0x7FF)
                                    len += 2;
                                else
                                    len += 3;
                            }
                            break;
                    }
                }
                else if (c == '"')
                    break;
                else
                    len++;
            }
            if (c == -1)
                return null;
            long pos_end = _IO.PI64 - buf.length;

            Seek(ref buf, pos_end - pos);
            byte[] temp = new byte[len + 1];
            for (ulong i = 0; i < len;)
            {
                c = ReadChar(ref buf);

                if (c == '\\')
                {
                    c = ReadChar(ref buf);

                    switch (c)
                    {
                        case '"':
                            temp[i++] = (byte)'"';
                            break;
                        case '\\':
                            temp[i++] = (byte)'\\';
                            break;
                        case '/':
                            temp[i++] = (byte)'/';
                            break;
                        case 'b':
                            temp[i++] = (byte)'\b';
                            break;
                        case 'f':
                            temp[i++] = (byte)'\f';
                            break;
                        case 'n':
                            temp[i++] = (byte)'\n';
                            break;
                        case 'r':
                            temp[i++] = (byte)'\r';
                            break;
                        case 't':
                            temp[i++] = (byte)'\t';
                            break;
                        case 'u':
                            {
                                uint uc = 0;
                                uc |= (uint)ReadStringHex(ReadChar(ref buf)) << 24;
                                uc |= (uint)ReadStringHex(ReadChar(ref buf)) << 16;
                                uc |= (uint)ReadStringHex(ReadChar(ref buf)) << 8;
                                uc |= (uint)ReadStringHex(ReadChar(ref buf));
                                if (uc <= 0x7F)
                                    temp[i] = (byte)uc;
                                else if (uc <= 0x7FF)
                                {
                                    temp[i++] = (byte)(0xC0 | ((uc >> 6) & 0x1F));
                                    temp[i] = (byte)(0x80 | (uc & 0x3F));
                                }
                                else
                                {
                                    temp[i++] = (byte)(0xE0 | ((uc >> 12) & 0xF));
                                    temp[i++] = (byte)(0x80 | ((uc >> 6) & 0x3F));
                                    temp[i++] = (byte)(0x80 | (uc & 0x3F));
                                }
                            }
                            break;
                    }
                }
                else if (c == '"')
                    break;
                else
                    temp[i++] = (byte)c;
            }

            if ((c = ReadChar(ref buf)) == -1)
                return null;

            string str = temp.ToUTF8();
            int null_term = str.IndexOf('\0');
            if (null_term >= 0)
                str = str.Substring(0, null_term);
            return str;
        }

        object ReadString(ref ReadBuffer buf, ref int c)
        {
            return ReadStringInner(ref buf, ref c);
        }

        void ReadSkipWhitespace(ref ReadBuffer buf, ref int c)
        {
            while ((c = ReadChar(ref buf)) != -1)
            {
                if (c == ' ' || c == '\n' || c == '\r' || c == '\t')
                    continue;
                break;
            }
        }

        object ReadMap(ref ReadBuffer buf, ref int c)
        {
            KKdList<MsgPack> map = KKdList<MsgPack>.New;
            if (c != '}')
            {
                while (true)
                {
                    ReadSkipWhitespace(ref buf, ref c);

                    if (c == '}')
                        break;

                    string key = ReadStringInner(ref buf, ref c);
                    if (key == null)
                        return null;

                    ReadSkipWhitespace(ref buf, ref c);
                    if (c != ':')
                        return null;
                    ReadSkipWhitespace(ref buf, ref c);

                    map.Add(new MsgPack(key, ReadInner(ref buf, ref c)));

                    ReadSkipWhitespace(ref buf, ref c);

                    if (c == '}')
                        break;
                    else if (c != ',')
                        return null;
                }
            }

            map.Capacity = map.Count;
            return map;
        }

        object ReadArray(ref ReadBuffer buf, ref int c)
        {
            KKdList<MsgPack> array = KKdList<MsgPack>.New;
            if (c != ']')
            {
                while (true)
                {
                    ReadSkipWhitespace(ref buf, ref c);

                    if (c == ']')
                        break;

                    array.Add(new MsgPack(null, ReadInner(ref buf, ref c)));

                    ReadSkipWhitespace(ref buf, ref c);

                    if (c == ']')
                        break;
                    else if (c != ',')
                        return null;
                }
            }

            array.Capacity = array.Count;
            return array.ToArray();
        }

        object ReadBool(ref ReadBuffer buf, ref int c)
        {
            if (c != 't')
            {
                if ((c = ReadChar(ref buf)) == -1 || c != 'a')
                    return null;
                if ((c = ReadChar(ref buf)) == -1 || c != 'l')
                    return null;
                if ((c = ReadChar(ref buf)) == -1 || c != 's')
                    return null;
                if ((c = ReadChar(ref buf)) == -1 || c != 'e')
                    return null;
                return false;
            }
            else
            {
                if ((c = ReadChar(ref buf)) == -1 || c != 'r')
                    return null;
                if ((c = ReadChar(ref buf)) == -1 || c != 'u')
                    return null;
                if ((c = ReadChar(ref buf)) == -1 || c != 'e')
                    return null;
                return true;
            }
        }

        object ReadNull(ref ReadBuffer buf, ref int c)
        {
            if ((c = ReadChar(ref buf)) == -1 || c != 'u')
                return null;
            if ((c = ReadChar(ref buf)) == -1 || c != 'l')
                return null;
            if ((c = ReadChar(ref buf)) == -1 || c != 'l')
                return null;

            return null;
        }

        public JSON W(MsgPack msgPack, bool ignoreNull, string end = "\n", string tabChar = "  ") =>
            W(msgPack, ignoreNull, end, tabChar, "", true);

        public JSON W(MsgPack msgPack, bool ignoreNull, bool style = false) =>
            W(msgPack, ignoreNull, "\n", "  ", "", style);

        private JSON W(MsgPack msgPack, bool ignoreNull, string end,
            string tabChar, string tab, bool style, bool isArray = false)
        {
            string oldTab = tab;
            tab += tabChar;
            if (msgPack.Name   != null && !isArray   ) _IO.W("\"" + msgPack.Name + "\":" + (style ? " " : ""));
            if (msgPack.Object == null) { if (!ignoreNull) WN(); return this; }

            if (msgPack.List.NotNull)
            {
                WM();
                if (style) _IO.W(end);
                if (msgPack.List.Count > 1)
                    for (int i = 0; i < msgPack.List.Count; i++)
                    {
                        if (style) _IO.W(tab);
                        W(msgPack.List[i], ignoreNull, end, tabChar, tab, style);
                        if (i + 1 < msgPack.List.Count) _IO.W(',');
                        if (style) _IO.W(end);
                    }
                else if (msgPack.List.Count == 1)
                {
                    if (style) _IO.W(tab);
                    W(msgPack.List[0], ignoreNull, end, tabChar, tab, style);
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
                        W(msgPack.Array[i], ignoreNull, end, tabChar, tab, style, true);
                        if (i + 1 < msgPack.Array.Length) _IO.W(',');
                        if (style) _IO.W(end);
                    }
                else if (msgPack.Array.Length == 1)
                {
                    if (style) _IO.W(tab);
                    W(msgPack.Array[0], ignoreNull, end, tabChar, tab, style, true);
                    if (style) _IO.W(end);
                }
                if (style) _IO.W(oldTab);
                WA(true);
            }
            else if (msgPack.Object is MsgPack msg) W(msg, ignoreNull, end, tabChar, tab, style);
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
