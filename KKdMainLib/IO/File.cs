using System;
using MSIO = System.IO;

namespace KKdMainLib.IO
{
    public static class File
    {
        public static Stream OpenReader(byte[] Data) => new Stream(new MSIO.MemoryStream(Data));
        public static Stream OpenWriter(           ) => new Stream(new MSIO.MemoryStream(    ));

        public static Stream OpenReader(string file, bool ReadAllAtOnce)
        { Stream IO = OpenReader(file); if (ReadAllAtOnce) return OpenReader(IO.ToArray(true)); return IO; }
        public static Stream OpenReader(string file)
        { Stream IO = new Stream(new MSIO.FileStream(file, MSIO.FileMode.Open, MSIO.FileAccess.ReadWrite,
              MSIO.FileShare.ReadWrite)) { File = file }; return IO; }
        public static Stream OpenWriter(string file, bool SetLength0)
        { Stream IO = OpenWriter(file); if (SetLength0) IO.SetLength(0); return IO; }
        public static Stream OpenWriter(string file, int SetLength)
        { Stream IO = OpenWriter(file); IO.SetLength(SetLength); return IO; }
        public static Stream OpenWriter(string file)
        { Stream IO = new Stream(new MSIO.FileStream(file,
            MSIO.FileMode.OpenOrCreate, MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite)) { File = file };
            MSIO.File.  SetCreationTimeUtc(file, DateTime.UtcNow);
            MSIO.File. SetLastWriteTimeUtc(file, DateTime.UtcNow);
            MSIO.File.SetLastAccessTimeUtc(file, DateTime.UtcNow); return IO; }

        public static   byte[] ReadAllBytes(string file, int length, int offset)
        { Stream IO = OpenReader(file); byte[] Data = IO.ReadBytes(length, offset); IO.Close(); return Data; }

        public static   byte[] ReadAllBytes(string file)
        { Stream IO = OpenReader(file); byte[] Data = IO.ReadBytes(IO.Length); IO.Close(); return Data; }

        public static string   ReadAllText (string file)
        { Stream IO = OpenReader(file); string Data = IO.ReadStringUTF8(IO.Length); IO.Close(); return Data; }

        public static string[] ReadAllLines(string file)
        { Stream IO = OpenReader(file); string Data = IO.ReadStringUTF8(IO.Length); IO.Close();
            return Data.Replace("\r", "").Split('\n'); }

        public static void WriteAllBytes(string file,   byte[] data)
        { Stream IO = OpenWriter(file, true); IO.Write(data); IO.Close(); }

        public static void WriteAllText (string file, string   data)
        { Stream IO = OpenWriter(file, true); IO.Write(data); IO.Close(); }

        public static void WriteAllLines(string file, string[] data)
        { Stream IO = OpenWriter(file, true); for (int i = 0; i < data.Length; i++)
                IO.Write(data[i] + "\r\n"); IO.Close(); }

        public static bool Exists(string file) => MSIO.File.Exists(file);
        public static void Delete(string file) => MSIO.File.Delete(file);
    }
}
