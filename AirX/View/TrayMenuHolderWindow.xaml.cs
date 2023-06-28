// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AirX.Bridge;
using AirX.Extension;
using AirX.Util;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using SRCounter;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinRT.Interop;

namespace AirX.View
{
    public sealed partial class TrayIconHolderWindow : Window
    {
        private static AirXBridge.OnTextReceivedHandler TextHandler = OnTextReceived;
        private static AirXBridge.OnFileSendingHandler FileSendingHandler = OnFileSending;
        private static AirXBridge.OnFilePartHandler FilePartHandler = OnFilePart;
        private static AirXBridge.OnFileComingHandler FileComingHandler = OnFileComing;

        private static SynchronizationContext context;

        private static FileStream writingFile = null;
        private static ulong totalSize = 0;

        public static TrayIconHolderWindow Instance { get; private set; }


        public TrayIconHolderWindow()
        {
            this.InitializeComponent();

            context = SynchronizationContext.Current;
            Instance = this;

            TrySignInAsync().LogOnError();
            AirXBridge.TryStartAirXService();
            AirXBridge.SetOnTextReceivedHandler(TextHandler);
            AirXBridge.SetOnFileComingHandler(FileComingHandler);
            AirXBridge.SetOnFileSendingHandler(FileSendingHandler);
            AirXBridge.SetOnFilePartHandler(FilePartHandler);

            AppWindow.Resize(new(1, 1));
            AppWindow.Move(new(32768, 32768));
        }

        private async Task TrySignInAsync()
        {
            if (!await AccountUtil.TryAutomaticLogin())
            {
                var window = new LoginWindow();
                window.Activate();
                return;
            }
            NotificationUtil.ShowNotification(
                "Welcome back, " + GlobalViewModel.Instance.LoggingGreetingsName + "!");
        }

        private static void OnFilePart(byte fileId, uint offset, uint length, byte[] data)
        {
            Debug.WriteLine($"File part received: offset={offset}, length={length}");
            if (writingFile == null)
            {
                Debug.WriteLine("File not accepted!");
                return;
            }
            
            writingFile.Write(data, (int) offset, (int)length);
            if (offset + length == totalSize)
            {
                writingFile.Close();
                writingFile = null;
                Debug.WriteLine("File recv completed!");
            }
        }

        private static void OnFileSending(byte fileId, ulong progress, ulong total, AirXBridge.FileStatus status)
        {
            switch (status)
            {
                case AirXBridge.FileStatus.Rejected:
                    {
                        Debug.WriteLine("File rejected!");
                        break;
                    }
                case AirXBridge.FileStatus.Accepted:
                    {
                        Debug.WriteLine("File accepted!");
                        break;
                    }
                case AirXBridge.FileStatus.Completed:
                    {
                        Debug.WriteLine("File completed!");
                        break;
                    }
                case AirXBridge.FileStatus.Error:
                    {
                        Debug.WriteLine("File error!");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private static void OnFileComing(ulong fileSize, string fileName, string from)
        {
            context.Post((_) =>
            {
                UIUtil.MessageBoxAsync(
                    "Received File",
                    $"File {fileName} from {from} is coming! Receive?",
                    "Receive!",
                    "Decline!"
                ).ContinueWith(t =>
                {
                    bool accept = t.Result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary;
                    if (accept)
                    {
                        // Preallocate file size
                        totalSize = fileSize;
                        writingFile = File.Create("D:\\" + fileName.Replace("/", "\\").Split("\\").Last());
                        writingFile.SetLength((long)totalSize);
                    }
                    AirXBridge.RespondToFile(
                        Peer.Parse(from),
                        1,
                        fileSize,
                        fileName,
                        accept
                    );
                }, TaskScheduler.Default).LogOnError();
            }, null);
        }

        private static void OnTextReceived(string text, string source)
        {
            if (AccountUtil.IsInBlockList(source))
            {
                return;
            }

            context.Post(_ =>
            {
                try
                {
                    var window = NewTextWindow.InstanceOf(text, source);
                    window.Activate();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }, null);
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            PInvoke.User32.ShowWindow(hwnd, PInvoke.User32.WindowShowStyle.SW_HIDE);
        }
    }
}
