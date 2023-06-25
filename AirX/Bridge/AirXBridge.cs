using AirX.Extension;
using AirX.Util;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using static System.Net.Mime.MediaTypeNames;

public class AirXBridge
{
    public delegate void OnTextReceivedHandler(string text, string from);
    public delegate void OnFileComingHandler(UInt64 fileSize, string fileName, string from);
    public delegate void OnFileSendingHandler(byte fileId, UInt64 progress, UInt64 total, FileStatus status);
    public delegate void OnFilePartHandler(byte fileId, UInt32 offset, UInt32 length, byte[] data);

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

    private static void OnTimerTick(object sender, object e)
    {
        var DataPackageView = Clipboard.GetContent();
        if (!DataPackageView.Contains(StandardDataFormats.Text))
        {
            return;
        }
        DataPackageView.GetTextAsync().AsTask().ContinueWith(t =>
        {
            if (t.Result != lastClipboardText)
            {
                lastClipboardText = t.Result;
                try
                {
                    OnClipboardChanged(t.Result);
                }
                catch (Exception) { }
            }
        }, TaskScheduler.Default).LogOnError();
    }

    private static void OnClipboardChanged(string newText)
    {
        if (ShouldSkipNextEvent)
        {
            ShouldSkipNextEvent = false;
            return;
        }
        Debug.WriteLine("Clipboard changed");

        IntPtr buffer = CreateUtf8String(newText, out uint size);
        airx_broadcast_text(AirXInstance, buffer, size);
        FreeUtf8String(buffer);
        Debug.WriteLine("Sent");
    }

    private static void OnTextReceived(IntPtr text, uint textLen, IntPtr sourceIpAddress, uint sourceIpAddressLen)
    {
        ShouldSkipNextEvent = true;
        string incomingText = Utf8StringFromPtr(text, (int)textLen);
        string sourceIpAddressSring = Utf8StringFromPtr(sourceIpAddress, (int)sourceIpAddressLen);

        var package = new DataPackage();
        package.SetText(incomingText);
        Clipboard.SetContent(package);

        onTextReceivedHandler?.Invoke(incomingText, sourceIpAddressSring);
    }

    private static void OnFileComing(uint fileSize, IntPtr fileName, uint fileNamelen, IntPtr sourceIpAddress, uint sourceIpAddressLen)
    {
        string fileNameString = Utf8StringFromPtr(fileName, (int)fileNamelen);
        string sourceIpAddressString = Utf8StringFromPtr(sourceIpAddress, (int)sourceIpAddressLen);

        onFileComingHandler?.Invoke(fileSize, fileNameString, sourceIpAddressString);
    }

    private static void OnFileSending(byte fileId, ulong progress, ulong total, byte status)
    {
        onFileSendingHandler?.Invoke(fileId, progress, total, (FileStatus)status);
    }

    private static void OnFilePart(byte fileId, uint offset, uint length, IntPtr data)
    {
        byte[] dataBytes = new byte[length];
        Marshal.Copy(data, dataBytes, 0, (int)length);

        onFilePartHandler?.Invoke(fileId, offset, length, dataBytes);
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
            Console.WriteLine("AirX version: " + airx_version());
            Console.WriteLine("AirX compabilitily version: " + airx_compatibility_number());

            AirXInstance = airx_create(
                (ushort)SettingsUtil.Int(Keys.DiscoveryServiceServerPort, 9818),
                (ushort)SettingsUtil.Int(Keys.DiscoveryServiceClientPort, 0),
                listenAddressBuffer,
                listenAddressSize,
                (ushort)SettingsUtil.Int(Keys.DataServiceListenPort, 9819),
                ((byte)SettingsUtil.Int(Keys.GroupIdentity, 0))
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
            airx_lan_discovery_service(AirXInstance, ShouldInterrupt);
            Debug.WriteLine("Discovery end");

            // Run in UI thread

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
            airx_data_service(
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

    public static List<string> GetPeers()
    {
        uint len = airx_get_peers(AirXInstance, PeerListBuffer);
        if (len <= 0)
        {
            return new List<string>();
        }
        string peers = Utf8StringFromPtr(PeerListBuffer, (int)len);
        return new List<string>(peers.Split(','));
    }

    public static void Deinit()
    {
        FreeUtf8String(PeerListBuffer);
        PeerListBuffer = IntPtr.Zero;
    }

    const string DLL_NAME = "libairx.dll";

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern int airx_version();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern int airx_compatibility_number();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_init();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr airx_create(UInt16 discovery_service_server_port,
                                            UInt16 discovery_service_client_port,
                                            IntPtr text_service_listen_addr,
                                            UInt32 text_service_listen_addr_len,
                                            UInt16 text_service_listen_port,
                                            byte group_identity);


    // Define delegate for the interrupt function
    public delegate bool InterruptFunc();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_lan_discovery_service(IntPtr airxPtr, InterruptFunc should_interrupt);


    // Define delegate for the callback function
    public delegate void TextCallbackFunction(
        IntPtr text, uint textLen, IntPtr sourceIpAddress, uint sourceIpAddressLen);
    public delegate void FileComingCallbackFunction(
        uint fileSize, IntPtr fileName, uint fileNamelen, IntPtr sourceIpAddress, uint sourceIpAddressLen);
    public delegate void FileSendingCallbackFunction(
        byte fileId, UInt64 progress, UInt64 total, byte status);
    public delegate void FilePartCallbackFunction(
        byte fileId, UInt32 offset, UInt32 length, IntPtr data);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_data_service(
        IntPtr airxPtr, 
        TextCallbackFunction textCallback,
        FileComingCallbackFunction fileComingCallback,
        FileSendingCallbackFunction fileSendingCallback,
        FilePartCallbackFunction filePartCallback,
        InterruptFunc interruptCallback
    );

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool airx_lan_broadcast(IntPtr airxPtr);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint airx_get_peers(IntPtr airxPtr, IntPtr buffer);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_send_text(IntPtr airxPtr, string host, uint host_len, IntPtr text, uint text_len);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_broadcast_text(IntPtr airxPtr, IntPtr text, uint len);

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
