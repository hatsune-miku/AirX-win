using AirX.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using System.Diagnostics;

namespace AirX.Util
{
    /// 文件工具类
    public class FileUtil
    {
        /// 内置的FileId计数器
        private static byte _fileId = 0;

        /// 得到一个绝对路径的文件名部分
        /// /Users/miku/1.txt -> 1.txt
        /// C:\aaa\1.txt -> 1.txt
        public static string GetFileName(string path)
        {
            return path
                .Replace('/', '\\')
                .Split('\\')
                .Last();
        }

        /// 得到一个绝对路径的目录部分，以Windows的路径分隔符分隔
        /// /Users/miku/1.txt -> Users\\miku\\1.txt
        public static string GetPath(string fullPath)
        {
            return fullPath
                .Replace('/', '\\')
                .Substring(0, fullPath.LastIndexOf('\\'));
        }

        /// 把某一文件在资源管理器中显示并选中
        public static void OpenFolderInExplorer(string fullPath)
        {
            string args = $"/select, \"{fullPath}\"";
            ProcessStartInfo info = new()
            {
                FileName = "explorer",
                Arguments = args,
            };
            Process.Start(info);
        }

        /// 分配下一个不重复的FileId
        public static byte NextFileId()
        {
            if (_fileId == 255)
            {
                /// 用完了则从0开始重复利用，只要不出现同时有255个文件在传的场面就够用
                _fileId = 0;
            }
            return _fileId++;
        }

        /// 把字节数转换为对应的，最合适的单位
        /// 10240 -> 10 KB
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

        /// 选择文件的窗口
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
