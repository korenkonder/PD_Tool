using System;
using MSIO = System.IO;

namespace KKdMainLib.IO
{
    public static class File
    {
        public static Stream OpenReader(byte[] Data) => new Stream(new MSIO.MemoryStream(Data));
        public static Stream OpenWriter(           ) => new Stream(new MSIO.MemoryStream(    ));

        public static Stream OpenReader(string file, bool ReadAllAtOnce)
        { Stream IO = OpenReader(file); if (ReadAllAtOnce) { byte[] data =
                    IO.ToArray(); IO.Dispose(); return OpenReader(data); } return IO; }
        public static Stream OpenReader(string file)
        { Stream IO = new Stream(new MSIO.FileStream(file, MSIO.FileMode.Open, MSIO.FileAccess.ReadWrite,
              MSIO.FileShare.ReadWrite)) { File = file }; return IO; }
        public static Stream OpenWriter(string file, bool SetLength0)
        { Stream IO = OpenWriter(file); if (SetLength0) IO.SL(0); return IO; }
        public static Stream OpenWriter(string file, int SetLength)
        { Stream IO = OpenWriter(file); IO.SL(SetLength); return IO; }
        public static Stream OpenWriter(string file)
        { Stream IO = new Stream(new MSIO.FileStream(file,
            MSIO.FileMode.OpenOrCreate, MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite)) { File = file };
            MSIO.File.  SetCreationTimeUtc(file, DateTime.UtcNow);
            MSIO.File. SetLastWriteTimeUtc(file, DateTime.UtcNow);
            MSIO.File.SetLastAccessTimeUtc(file, DateTime.UtcNow); return IO; }

        public static   byte[] ReadAllBytes(string file, int length, int offset)
        { byte[] Data; using (Stream IO = OpenReader(file)) Data = IO.RBy(length, offset); return Data; }

        public static   byte[] ReadAllBytes(string file)
        { byte[] Data; using (Stream IO = OpenReader(file)) Data = IO.RBy(IO.L); return Data; }

        public static string   ReadAllText (string file)
        { string Data; using (Stream IO = OpenReader(file)) Data = IO.RSUTF8(IO.L);
            return Data.Replace(((char)0xFEFF).ToString(), ""); }

        public static string[] ReadAllLines(string file)
        { string Data; using (Stream IO = OpenReader(file)) Data = IO.RSUTF8(IO.L);
            return Data.Replace(((char)0xFEFF).ToString(), "").Replace("\r", "").Split('\n'); }

        public static void WriteAllBytes(string file,   byte[] data)
        { using (Stream IO = OpenWriter(file, true)) IO.W(data); }

        public static void WriteAllText (string file, string   data)
        { using (Stream IO = OpenWriter(file, true)) IO.W(data); }

        public static void WriteAllLines(string file, string[] data)
        { using (Stream IO = OpenWriter(file, true)) for (int i = 0; i < data.Length; i++)
                    IO.W(data[i] + "\r\n"); }

        public static bool Exists(string file) => MSIO.File.Exists(file);
        public static void Delete(string file) => MSIO.File.Delete(file);
    }
}
