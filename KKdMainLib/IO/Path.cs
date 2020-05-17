using MSIOP = System.IO.Path;

namespace KKdMainLib.IO
{
    public static class Path
    {
        public static char DirectorySeparatorChar => MSIOP.DirectorySeparatorChar;
        public static char AltDirectorySeparatorChar => MSIOP.AltDirectorySeparatorChar;
        public static char VolumeSeparatorChar => MSIOP.VolumeSeparatorChar;
        public static char PathSeparator => MSIOP.PathSeparator;

        public static string ChangeExtension(string path, string extension) =>
            MSIOP.ChangeExtension(path, extension);

        public static string Combine(string path1, string path2, string path3) =>
            MSIOP.Combine(path1, path2, path3);

        public static string Combine(string path1, string path2) =>
            MSIOP.Combine(path1, path2);

        public static string Combine(string path1, string path2, string path3, string path4) =>
            MSIOP.Combine(path1, path2, path3, path4);

        public static string Combine(params string[] paths) =>
            MSIOP.Combine(paths);

        public static string GetDirectoryName(string path) =>
            MSIOP.GetDirectoryName(path);

        public static string GetExtension(string path) =>
            MSIOP.GetExtension(path);

        public static string GetFileName(string path) =>
            MSIOP.GetFileName(path);

        public static string GetFileNameWithoutExtension(string path) =>
            MSIOP.GetFileNameWithoutExtension(path);

        public static string GetFullPath(string path) =>
            MSIOP.GetFullPath(path);

        public static char[] GetInvalidFileNameChars() =>
            MSIOP.GetInvalidFileNameChars();

        public static char[] GetInvalidPathChars() =>
            MSIOP.GetInvalidPathChars();

        public static string GetPathRoot(string path) =>
            MSIOP.GetPathRoot(path);

        public static string GetRandomFileName() =>
            MSIOP.GetRandomFileName();

        public static string GetTempFileName() =>
            MSIOP.GetTempFileName();

        public static string GetTempPath() =>
            MSIOP.GetTempPath();

        public static bool HasExtension(string path) =>
            MSIOP.HasExtension(path);

        public static bool IsPathRooted(string path) =>
            MSIOP.IsPathRooted(path);
    }
}
