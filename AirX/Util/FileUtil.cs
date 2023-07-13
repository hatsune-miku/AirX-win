using AirX.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

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
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB", "要上天啊", "nb" };
            double size = sizeInBytes;
            int unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{Math.Round(size, 2)} {units[unitIndex]}";
        }

        public static async Task<IReadOnlyList<StorageFile>> OpenFileDialogAsync()
        {
            var filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            filePicker.FileTypeFilter.Add("*");
            filePicker.CommitButtonText = "Send";

            var hwnd = WindowNative.GetWindowHandle(TrayIconHolderWindow.Instance);
            InitializeWithWindow.Initialize(filePicker, hwnd);

            return new List<StorageFile>() { await filePicker.PickSingleFileAsync() };
        }
    }
}
