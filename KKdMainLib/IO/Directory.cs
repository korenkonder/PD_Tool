using System;
using MSIO = System.IO;

namespace KKdMainLib.IO
{
    public static class Directory
    {
        public static bool Exists(string path) => MSIO.Directory.Exists(path);
        public static void Delete(string path) => MSIO.Directory.Delete(path);
        public static string GetCurrentDirectory() => MSIO.Directory.GetCurrentDirectory();
        public static string[] GetFiles(string path) => MSIO.Directory.GetFiles(path);
        public static string[] GetDirectories(string path) => MSIO.Directory.GetDirectories(path);
        public static MSIO.DirectoryInfo CreateDirectory(string path) => MSIO.Directory.CreateDirectory(path);
    }
}

