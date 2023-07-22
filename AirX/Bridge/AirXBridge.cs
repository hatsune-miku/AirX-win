using AirX.Bridge;
using AirX.Extension;
using AirX.Model;
using AirX.Services;
using AirX.Util;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

public class AirXBridge
{
    public delegate void OnTextReceivedHandler(string text, Peer from);
    public delegate void OnFileComingHandler(UInt64 fileSize, string fileName, Peer from);
    public delegate void OnFileSendingHandler(byte fileId, UInt64 progress, UInt64 total, FileStatus status);
    public delegate bool OnFilePartHandler(byte fileId, UInt64 offset, UInt64 length, byte[] data);

    public enum FileStatus
    {
        Requested = 1,
        Rejected = 2,
        Accepted = 3,
        InProgress = 4,
        CancelledBySender = 5,
        CancelledByReceiver = 6,
        Completed = 7,
        Error = 8
    }

    // AirX Threads
    private static IntPtr AirXInstance = IntPtr.Zero;
    private static Thread AirXDiscoveryThread = null;
    private static Thread AirXTextServiceThread = null;
    private static bool ShouldInterruptSignal = false;

    // AirX Alloc
    private static IntPtr PeerListBuffer = Utf8StringAlloc(1024);

    // Clipboard related
    private static DispatcherTimer Timer = null;
    private static string lastClipboardText = "";
    private static bool ShouldSkipNextEvent = false;

    // Handlers
    private static OnTextReceivedHandler onTextReceivedHandler = null;
    private static OnFileComingHandler onFileComingHandler = null;
    private static OnFileSendingHandler onFileSendingHandler = null;
    private static OnFilePartHandler onFilePartHandler = null;

    // Async
    private static SynchronizationContext synchronizationContext = SynchronizationContext.Current;

    public static void RedirectAirXStdoutToDebugConsole()
    {
        PInvoke.Kernel32.AllocConsole();
    }

    private static void OnTimerTick(object sender, object e)
    {
        ClipboardUtils.GetTextAsync().ContinueWith(t =>
        {
            if (t.Result != lastClipboardText)
            {
                lastClipboardText = t.Result;
                OnClipboardChanged(t.Result);
            }
        }, TaskScheduler.Default).FireAndForget();
    }

    private static void OnClipboardChanged(string newText)
    {
        if (ShouldSkipNextEvent)
        {
            ShouldSkipNextEvent = false;
            return;
        }
        Debug.WriteLine("Clipboard changed.");

        IntPtr buffer = CreateUtf8String(newText, out uint size);
        AirXNative.airx_broadcast_text(AirXInstance, buffer, size);
        FreeUtf8String(buffer);
        Debug.WriteLine("Sent over LAN.");

        if (GlobalViewModel.Instance.IsSignedIn)
        {
            AirXCloud.SendMessageAsync(newText, MessageType.Text).FireAndForget();
            Debug.WriteLine("Sent over Inet.");
        }
    }

    private static void OnTextReceived(IntPtr text, uint textLen, IntPtr sourceIpAddress, uint sourceIpAddressLen)
    {
        ShouldSkipNextEvent = true;
        string incomingText = Utf8StringFromPtr(text, (int)textLen);
        string sourceIpAddressString = Utf8StringFromPtr(sourceIpAddress, (int)sourceIpAddressLen);

        try
        {
            Peer peer = Peer.Parse(sourceIpAddressString);
            OnTextReceived(incomingText, peer);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to parse source IP address");
        }
    }

    public static void OnTextReceived(string incomingText, Peer peer)
    {
        ClipboardUtils.GetTextAsync().ContinueWith(t =>
        {
            if (t.Result == incomingText)
            {
                Debug.WriteLine("Text received from myself. Ignored.");
                return;
            }

            ClipboardUtils.SetText(incomingText);
            onTextReceivedHandler?.Invoke(incomingText, peer);
        }, TaskScheduler.Default).FireAndForget();
    }

    private static void OnFileComing(UInt64 fileSize, IntPtr fileName, uint fileNamelen, IntPtr sourceIpAddress, uint sourceIpAddressLen)
    {
        string fileNameString = Utf8StringFromPtr(fileName, (int)fileNamelen);
        string sourceIpAddressString = Utf8StringFromPtr(sourceIpAddress, (int)sourceIpAddressLen);

        try
        {
            Peer peer = Peer.Parse(sourceIpAddressString);
            onFileComingHandler?.Invoke(fileSize, fileNameString, peer);
        }
        catch (Exception)
        {
            Debug.WriteLine("Failed to parse source IP address");
        }
    }

    private static void OnFileSending(byte fileId, ulong progress, ulong total, byte status)
    {
        onFileSendingHandler?.Invoke(fileId, progress, total, (FileStatus)status);
    }

    private static bool OnFilePart(byte fileId, UInt64 offset, UInt64 length, IntPtr data)
    {
        try
        {
            byte[] dataBytes = new byte[length];
            Marshal.Copy(data, dataBytes, 0, (int)length);

            return onFilePartHandler?.Invoke(fileId, offset, length, dataBytes) ?? false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }

    private static bool ShouldInterrupt()
    {
        return ShouldInterruptSignal;
    }

    public static void SetOnTextReceivedHandler(OnTextReceivedHandler handler)
    {
        onTextReceivedHandler = handler;
    }

    public static void SetOnFileComingHandler(OnFileComingHandler handler)
    {
        onFileComingHandler = handler;
    }

    public static void SetOnFileSendingHandler(OnFileSendingHandler handler)
    {
        onFileSendingHandler = handler;
    }

    public static void SetOnFilePartHandler(OnFilePartHandler handler)
    {
        onFilePartHandler = handler;
    }

    public static bool TryStartAirXService()
    {
        if (GlobalViewModel.Instance.IsServiceOnline)
        {
            return false;
        }

        ShouldInterruptSignal = false;

        string listenAddress = "0.0.0.0";
        IntPtr listenAddressBuffer = CreateUtf8String(listenAddress, out uint listenAddressSize);

        try
        {
            Debug.WriteLine("AirX version: " + AirXNative.airx_version());
            Debug.WriteLine("AirX compabilitily version: " + AirXNative.airx_compatibility_number());

            AirXInstance = AirXNative.airx_create(
                (ushort)SettingsUtil.Int(Keys.DiscoveryServiceServerPort, 9818),
                (ushort)SettingsUtil.Int(Keys.DiscoveryServiceClientPort, 0),
                listenAddressBuffer,
                listenAddressSize,
                (ushort)SettingsUtil.Int(Keys.DataServiceListenPort, 9819),
                ((byte)SettingsUtil.Int(Keys.GroupIdentifier, 0))
            );
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        FreeUtf8String(listenAddressBuffer);

        AirXDiscoveryThread = new Thread(() =>
        {
            Debug.WriteLine("Discovery start");
            AirXNative.airx_lan_discovery_service(AirXInstance, ShouldInterrupt);
            Debug.WriteLine("Discovery end");

            synchronizationContext.Post((_) =>
            {
                GlobalViewModel.Instance.IsDiscoveryServiceOnline = false;
                if (!GlobalViewModel.Instance.IsTextServiceOnline)
                {
                    GlobalViewModel.Instance.IsServiceOnline = false;
                }
            }, null);
        });
        AirXTextServiceThread = new Thread(() =>
        {
            Debug.WriteLine("Text start");
            AirXNative.airx_data_service(
                AirXInstance,
                OnTextReceived, 
                OnFileComing,
                OnFileSending,
                OnFilePart,
                ShouldInterrupt
            );
            Debug.WriteLine("Text end");

            synchronizationContext.Post((_) =>
            {
                GlobalViewModel.Instance.IsTextServiceOnline = false;
                if (!GlobalViewModel.Instance.IsDiscoveryServiceOnline)
                {
                    GlobalViewModel.Instance.IsServiceOnline = false;
                }
            }, null);
        });

        AirXDiscoveryThread.Start();
        AirXTextServiceThread.Start();

        Timer = new DispatcherTimer();
        Timer.Interval = TimeSpan.FromMilliseconds(500);
        Timer.Tick += OnTimerTick;
        Timer.Start();

        GlobalViewModel.Instance.IsServiceOnline = true;
        GlobalViewModel.Instance.IsDiscoveryServiceOnline = true;
        GlobalViewModel.Instance.IsTextServiceOnline = true;
        return true;
    }

    public static void TryStopAirXService()
    {
        if (!GlobalViewModel.Instance.IsServiceOnline)
        {
            return;
        }
        ShouldInterruptSignal = true;

        Timer.Stop();
        Timer = null;
    }

    public static List<Peer> GetPeers()
    {
        uint len = AirXNative.airx_get_peers(AirXInstance, PeerListBuffer);
        if (len <= 0)
        {
            return new List<Peer>();
        }
        string peers = Utf8StringFromPtr(PeerListBuffer, (int)len);
        try
        {
            return new List<Peer>(
                peers.Split(',')
                .Select(ps => Peer.Parse(ps)
                )
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new List<Peer>();
        }
    }

    public static void TrySendFile(string path, Peer peer)
    {
        var hostString = CreateUtf8String(peer.IpAddress, out uint hostStringSize);
        var filePathString = CreateUtf8String(path, out uint filePathStringSize);
        AirXNative.airx_try_send_file(
            AirXInstance,
            hostString,
            hostStringSize,
            filePathString,
            filePathStringSize
        );
    }

    public static void RespondToFile(Peer peer, byte fileId, UInt64 fileSize, string filePath, bool accept)
    {
        var hostString = CreateUtf8String(peer.IpAddress, out uint hostStringSize);
        var filePathString = CreateUtf8String(filePath, out uint filePathStringSize);
        AirXNative.airx_respond_to_file(
            AirXInstance,
            hostString,
            hostStringSize,
            fileId,
            fileSize,
            filePathString,
            filePathStringSize,
            accept
        );
    }

    public static string GetVersionString()
    {
        var versionString = Utf8StringAlloc(128);
        var actualLength = AirXNative.airx_version_string(versionString);
        var ret = Utf8StringFromPtr(versionString, (int) actualLength);
        FreeUtf8String(versionString);
        return ret;
    }

    public static void Deinit()
    {
        FreeUtf8String(PeerListBuffer);
        PeerListBuffer = IntPtr.Zero;
    }


    // Define delegate for the interrupt function
    public delegate bool InterruptFunc();

    // Define delegate for the callback function
    public delegate void TextCallbackFunction(
        IntPtr text, uint textLen, IntPtr sourceIpAddress, uint sourceIpAddressLen);
    public delegate void FileComingCallbackFunction(
        UInt64 fileSize, IntPtr fileName, uint fileNamelen, IntPtr sourceIpAddress, uint sourceIpAddressLen);
    public delegate void FileSendingCallbackFunction(
        byte fileId, UInt64 progress, UInt64 total, byte status);
    public delegate bool FilePartCallbackFunction(
        byte fileId, UInt64 offset, UInt64 length, IntPtr data);


    public static IntPtr CreateUtf8String(string s, out uint size)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        size = (uint) bytes.Length;
        return ptr;
    }

    public static void FreeUtf8String(IntPtr ptr)
    {
        Marshal.FreeHGlobal(ptr);
    }

    public static string Utf8StringFromPtr(IntPtr ptr, int length)
    {
        return Marshal.PtrToStringUTF8(ptr, length);
    }

    public static IntPtr Utf8StringAlloc(uint size)
    {
        return Marshal.AllocHGlobal((int)size);
    }
}
