using MSIO = System.IO;
using MSIOD = System.IO.Directory;

namespace KKdMainLib.IO
{
    public static class Directory
    {
        public static MSIO.DirectoryInfo CreateDirectory(string path) =>
            MSIOD.CreateDirectory(path);

        public static void Delete(string path, bool recursive) =>
            MSIOD.Delete(path, recursive);

        public static void Delete(string path) =>
            MSIOD.Delete(path);

        public static bool Exists(string path) =>
            MSIOD.Exists(path);

        public static System.DateTime GetCreationTime(string path) =>
            MSIOD.GetCreationTime(path);

        public static System.DateTime GetCreationTimeUtc(string path) =>
            MSIOD.GetCreationTimeUtc(path);

        public static string GetCurrentDirectory() =>
            MSIOD.GetCurrentDirectory();

        public static string[] GetDirectories(string path, string searchPattern, MSIO.SearchOption searchOption) =>
            MSIOD.GetDirectories(path, searchPattern, searchOption);

        public static string[] GetDirectories(string path, string searchPattern) =>
            MSIOD.GetDirectories(path, searchPattern);

        public static string[] GetDirectories(string path) =>
            MSIOD.GetDirectories(path);

        public static string GetDirectoryRoot(string path) =>
            MSIOD.GetDirectoryRoot(path);

        public static string[] GetFiles(string path, string searchPattern, MSIO.SearchOption searchOption) =>
            MSIOD.GetFiles(path, searchPattern, searchOption);

        public static string[] GetFiles(string path) =>
            MSIOD.GetFiles(path);

        public static string[] GetFiles(string path, string searchPattern) =>
            MSIOD.GetFiles(path, searchPattern);

        public static string[] GetFileSystemEntries(string path) =>
            MSIOD.GetFileSystemEntries(path);

        public static string[] GetFileSystemEntries(string path, string searchPattern) =>
            MSIOD.GetFileSystemEntries(path, searchPattern);

        public static string[] GetFileSystemEntries(string path, string searchPattern, MSIO.SearchOption searchOption) =>
            MSIOD.GetFileSystemEntries(path, searchPattern, searchOption);

        public static System.DateTime GetLastAccessTime(string path) =>
            MSIOD.GetLastAccessTime(path);

        public static System.DateTime GetLastAccessTimeUtc(string path) =>
            MSIOD.GetLastAccessTimeUtc(path);

        public static System.DateTime GetLastWriteTime(string path) =>
            MSIOD.GetLastWriteTime(path);

        public static System.DateTime GetLastWriteTimeUtc(string path) =>
            MSIOD.GetLastWriteTimeUtc(path);

        public static string[] GetLogicalDrives() =>
            MSIOD.GetLogicalDrives();

        public static MSIO.DirectoryInfo GetParent(string path) =>
            MSIOD.GetParent(path);

        public static void Move(string sourceDirName, string destDirName) =>
            MSIOD.Move(sourceDirName, destDirName);

        public static void SetCreationTime(string path, System.DateTime creationTime) =>
            MSIOD.SetCreationTime(path, creationTime);

        public static void SetCreationTimeUtc(string path, System.DateTime creationTimeUtc) =>
            MSIOD.SetCreationTimeUtc(path, creationTimeUtc);

        public static void SetCurrentDirectory(string path) =>
            MSIOD.SetCurrentDirectory(path);

        public static void SetLastAccessTime(string path, System.DateTime lastAccessTime) =>
            MSIOD.SetLastAccessTime(path, lastAccessTime);

        public static void SetLastAccessTimeUtc(string path, System.DateTime lastAccessTimeUtc) =>
            MSIOD.SetLastAccessTimeUtc(path, lastAccessTimeUtc);

        public static void SetLastWriteTime(string path, System.DateTime lastWriteTime) =>
            MSIOD.SetLastWriteTime(path, lastWriteTime);

        public static void SetLastWriteTimeUtc(string path, System.DateTime lastWriteTimeUtc) =>
            MSIOD.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
    }
}

