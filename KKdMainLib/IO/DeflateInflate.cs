using System.Runtime.InteropServices;

namespace KKdMainLib.IO
{
    public static unsafe class DeflateInflate
    {
        public static bool HasFile = false;

        private const string libDeflateString = "libdeflate.dll";

        public static void CheckLib()
        {
            if (!File.Exists(libDeflateString))
                File.WriteAllBytes(libDeflateString, Properties.Resources.libdeflate);
            HasFile = true;
        }

        public static byte[] Deflate(this byte[] data, long compressionLevel)
        {
            CheckLib();

            void* c = libdeflate_alloc_compressor((int)compressionLevel);
            int maxOutBytes = libdeflate_deflate_compress_bound(c, data.Length);
            byte[] outData = new byte[maxOutBytes];
            int actualOut = 0;
            fixed (byte*  inPtr =    data)
            fixed (byte* outPtr = outData)
                actualOut = libdeflate_deflate_compress(c, inPtr, data.Length, outPtr, maxOutBytes);
            libdeflate_free_compressor(c);
            System.Array.Resize(ref outData, (int)actualOut);
            return outData;
        }

        public static byte[] Inflate(this byte[] data, long length)
        {
            CheckLib();

            byte[] outData = new byte[length];

            int actualOut = 0;
            void* d = libdeflate_alloc_decompressor();
            fixed (byte*  inPtr =    data)
            fixed (byte* outPtr = outData)
                libdeflate_deflate_decompress(d, inPtr, data.Length, outPtr, (int)length, &actualOut);
            libdeflate_free_decompressor(d);

            if (actualOut == 0 || actualOut > length)
                return data.Inflate(length * 2);
            else if (actualOut < length)
                System.Array.Resize(ref outData, (int)actualOut);
            return outData;
        }

        public static byte[] DeflateGZip(this byte[] data, long compressionLevel)
        {
            CheckLib();

            void* c = libdeflate_alloc_compressor((int)compressionLevel);
            int maxOutBytes = libdeflate_gzip_compress_bound(c, data.Length);
            byte[] outData = new byte[maxOutBytes];
            int actualOut = 0;
            fixed (byte*  inPtr =    data)
            fixed (byte* outPtr = outData)
                actualOut = libdeflate_gzip_compress(c, inPtr, data.Length, outPtr, maxOutBytes);
            libdeflate_free_compressor(c);
            System.Array.Resize(ref outData, (int)actualOut);
            return outData;
        }

        public static byte[] InflateGZip(this byte[] data, long length)
        {
            CheckLib();

            byte[] outData = new byte[length];

            int result;
            int actualOut = 0;
            void* d = libdeflate_alloc_decompressor();
            fixed (byte*  inPtr =    data)
            fixed (byte* outPtr = outData)
                result = libdeflate_gzip_decompress(d, inPtr, data.Length, outPtr, (int)length, &actualOut);
            libdeflate_free_decompressor(d);

            if (actualOut == 0 || actualOut > length)
                return data.InflateGZip(length * 2 + 1);
            else if (actualOut < length)
                System.Array.Resize(ref outData, (int)actualOut);
            return outData;
        }

        public static byte[] DeflateZLib(this byte[] data, long compressionLevel)
        {
            CheckLib();

            void* c = libdeflate_alloc_compressor((int)compressionLevel);
            int maxOutBytes = libdeflate_zlib_compress_bound(c, data.Length);
            byte[] outData = new byte[maxOutBytes];
            int actualOut = 0;
            fixed (byte*  inPtr =    data)
            fixed (byte* outPtr = outData)
                actualOut = libdeflate_zlib_compress(c, inPtr, data.Length, outPtr, maxOutBytes);
            libdeflate_free_compressor(c);
            System.Array.Resize(ref outData, (int)actualOut);
            return outData;
        }

        public static byte[] InflateZLib(this byte[] data, long length)
        {
            CheckLib();

            byte[] outData = new byte[length];

            int actualOut = 0;
            void* d = libdeflate_alloc_decompressor();
            fixed (byte*  inPtr =    data)
            fixed (byte* outPtr = outData)
                libdeflate_zlib_decompress(d, inPtr, data.Length, outPtr, (int)length, &actualOut);
            libdeflate_free_decompressor(d);

            if (actualOut == 0 || actualOut > length)
                return data.InflateZLib(length * 2);
            else if (actualOut < length)
                System.Array.Resize(ref outData, (int)actualOut);
            return outData;
        }

        [DllImport(libDeflateString)]
        private static extern void* libdeflate_alloc_compressor(int compressionLevel);

        [DllImport(libDeflateString)]
        private static extern void* libdeflate_alloc_decompressor();

        [DllImport(libDeflateString)]
        private static extern int libdeflate_deflate_compress_bound(void* c, int length);

        [DllImport(libDeflateString)]
        private static extern int libdeflate_deflate_compress(void* c, byte* inData, int inBytes,
            byte* outData, int maxOutBytes);

        [DllImport(libDeflateString)]
        private static extern int libdeflate_deflate_decompress(void* d, byte* inData, int inBytes,
            byte* outData, int outBytesAvail, int* actualOutBytes);

        [DllImport(libDeflateString)]
        private static extern int libdeflate_deflate_decompress_ex(void* d, byte* inData, int inBytes,
            byte* outData, int outBytesAvail, int* actualInBytes, int* actualOutBytes);

        [DllImport(libDeflateString)]
        private static extern int libdeflate_gzip_compress_bound(void* c, int length);

        [DllImport(libDeflateString)]
        private static extern int libdeflate_gzip_compress(void* c, byte* inData, int inBytes,
            byte* outData, int maxOutBytes);

        [DllImport(libDeflateString)]
        private static extern int libdeflate_gzip_decompress(void* d, byte* inData, int inBytes,
            byte* outData, int outBytesAvail, int* actualOutBytes);

        [DllImport(libDeflateString)]
        private static extern int libdeflate_gzip_decompress_ex(void* d, byte* inData, int inBytes,
            byte* outData, int outBytesAvail, int* actualInBytes, int* actualOutBytes);

        [DllImport(libDeflateString)]
        private static extern int libdeflate_zlib_compress_bound(void* c, int length);

        [DllImport(libDeflateString)]
        private static extern int libdeflate_zlib_compress(void* c, byte* inData, int inBytes,
            byte* outData, int maxOutBytes);

        [DllImport(libDeflateString)]
        private static extern int libdeflate_zlib_decompress(void* d, byte* inData, int inBytes,
            byte* outData, int outBytesAvail, int* actualOutBytes);

        [DllImport(libDeflateString)]
        private static extern void libdeflate_free_compressor(void* c);

        [DllImport(libDeflateString)]
        private static extern void libdeflate_free_decompressor(void* d);
    }
}
