using System;
using MSIO = System.IO;

namespace KKdMainLib.IO
{
    public static class File
    {
        public static Stream OpenReader(byte[] Data) => new Stream(new MSIO.MemoryStream(Data));
        public static Stream OpenWriter(byte[] Data) => new Stream(new MSIO.MemoryStream(Data));
        public static Stream OpenWriter(           ) => new Stream(new MSIO.MemoryStream(    ));

        public static Stream OpenReader(string file, bool ReadAllAtOnce)
        { Stream _IO = OpenReader(file); if (ReadAllAtOnce) { byte[] data =
                    _IO.ToArray(); _IO.Dispose(); return OpenReader(data); } return _IO; }
        public static Stream OpenReader(string file)
        { Stream _IO = new Stream(new MSIO.FileStream(file, MSIO.FileMode.Open, MSIO.FileAccess.ReadWrite,
              MSIO.FileShare.ReadWrite)) { File = file }; return _IO; }
        public static Stream OpenWriter(string file, bool SetLength0)
        { Stream _IO = OpenWriter(file); if (SetLength0) _IO.SL(0); return _IO; }
        public static Stream OpenWriter(string file, int SetLength)
        { Stream _IO = OpenWriter(file); _IO.SL(SetLength); return _IO; }
        public static Stream OpenWriter(string file)
        { Stream _IO = new Stream(new MSIO.FileStream(file,
            MSIO.FileMode.OpenOrCreate, MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite)) { File = file };
            MSIO.File.  SetCreationTimeUtc(file, DateTime.UtcNow);
            MSIO.File. SetLastWriteTimeUtc(file, DateTime.UtcNow);
            MSIO.File.SetLastAccessTimeUtc(file, DateTime.UtcNow); return _IO; }

        public static   byte[] ReadAllBytes(string file, int length, int offset)
        { byte[] Data; using (Stream _IO = OpenReader(file)) Data = _IO.RBy(length, offset); return Data; }

        public static   byte[] ReadAllBytes(string file)
        { byte[] Data; using (Stream _IO = OpenReader(file)) Data = _IO.RBy(_IO.L); return Data; }

        public static string   ReadAllText (string file)
        { string Data; using (Stream _IO = OpenReader(file)) Data = _IO.RSUTF8(_IO.L);
            return Data.Replace(((char)0xFEFF).ToString(), ""); }

        public static string[] ReadAllLines(string file)
        { string Data; using (Stream _IO = OpenReader(file)) Data = _IO.RSUTF8(_IO.L);
            return Data.Replace(((char)0xFEFF).ToString(), "").Replace("\r", "").Split('\n'); }

        public static void WriteAllBytes(string file,   byte[] data)
        { using Stream _IO = OpenWriter(file, true); if (data != null) _IO.W(data); }

        public static void WriteAllText (string file, string   data)
        { using Stream _IO = OpenWriter(file, true); if (data != null) _IO.W(data); }

        public static void WriteAllLines(string file, string[] data)
        { using Stream _IO = OpenWriter(file, true);
            if (data != null) for (int i = 0; i < data.Length; i++)
                    if (data[i] != null) _IO.W(data[i] + "\r\n"); }

        public static void WriteAllBytes(string file,   byte[] data, long length)
        { using Stream _IO = OpenWriter(file, true); if (data != null) { _IO.W(data); _IO.LI64 = length; } }

        public static void WriteAllText (string file, string   data, long length)
        { using Stream _IO = OpenWriter(file, true); if (data != null) { _IO.W(data); _IO.LI64 = length; } }

        public static void WriteAllLines(string file, string[] data, long length)
        { using Stream _IO = OpenWriter(file, true);
            if (data != null) { for (int i = 0; i < data.Length; i++)
                    if (data[i] != null) _IO.W(data[i] + "\r\n"); } _IO.LI64 = length; }

        public static bool Exists(string file) => MSIO.File.Exists(file);
        public static void Delete(string file) => MSIO.File.Delete(file);
    }
}
