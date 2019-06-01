using System;
using MSIO = System.IO;

namespace KKdMainLib.IO
{
    public static class Path
    {
        public static string GetFileName(string path) =>
            MSIO.Path.GetFileName(path);
        public static string GetFileNameWithoutExtension(string path) =>
            MSIO.Path.GetFileNameWithoutExtension(path);
        public static string GetExtension(string path) =>
            MSIO.Path.GetExtension(path);
        public static string GetFullPath(string path) =>
            MSIO.Path.GetFullPath(path);
        public static string Combine(string path1, string path2) =>
            MSIO.Path.Combine(path1, path2);
        public static string Combine(string path1, string path2, string path3) =>
            MSIO.Path.Combine(path1, path2, path3);
        public static string Combine(string path1, string path2, string path3, string path4) =>
            MSIO.Path.Combine(path1, path2, path3, path4);
        public static string Combine(params string[] paths) =>
            MSIO.Path.Combine(paths);
    }
}
