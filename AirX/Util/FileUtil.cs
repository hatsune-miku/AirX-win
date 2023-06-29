using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Util
{
    public class FileUtil
    {
        private static byte _fileId = 0;

        public static string GetFileName(string path)
        {
            return path
                .Replace('/', '\\')
                .Split('\\')
                .Last();
        }

        public static byte NextFileId()
        {
            if (_fileId == 255)
            {
                _fileId = 0;
            }
            return _fileId++;
        }

        public static string GetFileSizeDescription(ulong sizeInBytes)
        {
            if (sizeInBytes < 1024)
            {
                return $"{sizeInBytes} B";
            }
            if (sizeInBytes < 1024 * 1024)
            {
                return $"{sizeInBytes / 1024} KB";
            }
            if (sizeInBytes < 1024 * 1024 * 1024)
            {
                return $"{sizeInBytes / 1024 / 1024} MB";
            }
            if (sizeInBytes < 1024UL * 1024 * 1024 * 1024)
            {
                return $"{sizeInBytes / 1024 / 1024 / 1024} GB";
            }
            return $"{sizeInBytes / 1024 / 1024 / 1024 / 1024} TB";
        }
    }
}
