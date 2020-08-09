using MSIO = System.IO;
using MSIOF = System.IO.File;

namespace KKdMainLib.IO
{
    public static class File
    {
        public static Stream OpenReadWriter(byte[] data) => new Stream(new MSIO.MemoryStream(data));
        public static Stream OpenReader    (byte[] data) => new Stream(new MSIO.MemoryStream(data), canWrite: false);
        public static Stream OpenWriter    (byte[] data) => new Stream(new MSIO.MemoryStream(data), canRead : false);
        public static Stream OpenWriter    (           ) => new Stream(new MSIO.MemoryStream(    ), canRead : false);

        public static Stream OpenReadWriter(string file)
        { bool exist = Exists(file);
            Stream _IO = new Stream(new MSIO.FileStream(file, MSIO.FileMode.OpenOrCreate,
            MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite)) { File = file };
            System.DateTime t = System.DateTime.UtcNow;
            if (exist) MSIOF.SetCreationTimeUtc(file, t);
            MSIOF. SetLastWriteTimeUtc(file, t);
            MSIOF.SetLastAccessTimeUtc(file, t); return _IO; }
        public static Stream OpenReader(string file, bool readAllAtOnce)
        { Stream _IO = OpenReader(file); if (readAllAtOnce) { byte[] data =
                    _IO.ToArray(); _IO.Dispose(); return OpenReader(data); } return _IO; }
        public static Stream OpenReader(string file)
        { Stream _IO = new Stream(new MSIO.FileStream(file, MSIO.FileMode.Open, MSIO.FileAccess.Read,
              MSIO.FileShare.ReadWrite), canWrite: false) { File = file }; return _IO; }
        public static Stream OpenWriter(string file, bool setLength0)
        { Stream _IO = OpenWriter(file); if (setLength0) _IO.SL(0); return _IO; }
        public static Stream OpenWriter(string file, int setLength)
        { Stream _IO = OpenWriter(file); _IO.SL(setLength); return _IO; }
        public static Stream OpenWriter(string file)
        { bool exist = Exists(file);
            Stream _IO = new Stream(new MSIO.FileStream(file, MSIO.FileMode.OpenOrCreate,
                MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite), canRead: false) { File = file };
            System.DateTime t = System.DateTime.UtcNow;
            if (!exist) MSIOF.SetCreationTimeUtc(file, t);
            MSIOF. SetLastWriteTimeUtc(file, t);
            MSIOF.SetLastAccessTimeUtc(file, t); return _IO; }

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
            if (data != null) { int c = data.Length; for (int i = 0; i < c; i++)
                { if (data[i] != null) _IO.W(data[i]); if (i + 1 < c) _IO.W("\n"); } } }

        public static void WriteAllBytes(string file,   byte[] data, long length)
        { using Stream _IO = OpenWriter(file, true); if (data != null) { _IO.W(data); _IO.LI64 = length; } }

        public static void WriteAllText (string file, string   data, long length)
        { using Stream _IO = OpenWriter(file, true); if (data != null) { _IO.W(data); _IO.LI64 = length; } }

        public static void WriteAllLines(string file, string[] data, long length)
        { using Stream _IO = OpenWriter(file, true);
            if (data != null) { int c = data.Length; for (int i = 0; i < c; i++)
                { if (data[i] != null) _IO.W(data[i]); if (i + 1 < c) _IO.W("\n"); } _IO.LI64 = length; } }

        public static bool Exists(string file) =>
            MSIOF.Exists(file);

        public static void Delete(string file) =>
            MSIOF.Delete(file);

        public static MSIO.FileAttributes GetAttributes(string path) =>
            MSIOF.GetAttributes(path);

        public static System.DateTime GetCreationTime(string path) =>
            MSIOF.GetCreationTime(path);

        public static System.DateTime GetCreationTimeUtc(string path) =>
            MSIOF.GetCreationTimeUtc(path);

        public static System.DateTime GetLastAccessTime(string path) =>
            MSIOF.GetLastAccessTime(path);

        public static System.DateTime GetLastAccessTimeUtc(string path) =>
            MSIOF.GetLastAccessTimeUtc(path);

        public static System.DateTime GetLastWriteTime(string path) =>
            MSIOF.GetLastWriteTime(path);

        public static System.DateTime GetLastWriteTimeUtc(string path) =>
            MSIOF.GetLastWriteTimeUtc(path);

        public static void Move(string sourceFileName, string destFileName) =>
            MSIOF.Move(sourceFileName, destFileName);

        public static void SetAttributes(string path, MSIO.FileAttributes fileAttributes) =>
            MSIOF.SetAttributes(path, fileAttributes);

        public static void SetCreationTime(string path, System.DateTime creationTime) =>
            MSIOF.SetCreationTime(path, creationTime);

        public static void SetCreationTimeUtc(string path, System.DateTime creationTimeUtc) =>
            MSIOF.SetCreationTimeUtc(path, creationTimeUtc);

        public static void SetLastAccessTime(string path, System.DateTime lastAccessTime) =>
            MSIOF.SetLastAccessTime(path, lastAccessTime);

        public static void SetLastAccessTimeUtc(string path, System.DateTime lastAccessTimeUtc) =>
            MSIOF.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

        public static void SetLastWriteTime(string path, System.DateTime lastWriteTime) =>
            MSIOF.SetLastWriteTime(path, lastWriteTime);

        public static void SetLastWriteTimeUtc(string path, System.DateTime lastWriteTimeUtc) =>
            MSIOF.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
    }
}
