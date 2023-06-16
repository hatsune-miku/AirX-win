using AirX.Util;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;

public class AirXBridge
{
    public delegate void OnTextReceivedHandler(string text, string from);

    private static IntPtr AirXInstance = IntPtr.Zero;
    private static Thread AirXDiscoveryThread = null;
    private static Thread AirXTextServiceThread = null;
    private static DispatcherTimer Timer = null;
    private static string lastClipboardText = "";
    private static bool ShouldSkipNextEvent = false;
    private static OnTextReceivedHandler handler = null;
    private static bool ShouldInterruptSignal = false;
    private static SynchronizationContext synchronizationContext = SynchronizationContext.Current;

    private static async void OnTimerTick(object sender, object e)
    {
        var DataPackageView = Clipboard.GetContent();
        if (DataPackageView.Contains(StandardDataFormats.Text))
        {
            var text = await DataPackageView.GetTextAsync();
            if (text != lastClipboardText)
            {
                lastClipboardText = text;
                OnClipboardChanged(text);
            }
        }
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

        handler?.Invoke(incomingText, sourceIpAddressSring);
    }

    private static bool ShouldInterrupt()
    {
        return ShouldInterruptSignal;
    }

    public static void SetOnTextReceivedHandler(OnTextReceivedHandler handler)
    {
        AirXBridge.handler = handler;
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
            Console.WriteLine("Is first run: " + airx_is_first_run());

            AirXInstance = airx_create(
                (ushort)SettingsUtil.Int(DefaultKeys.DiscoveryServiceServerPort, 9818),
                (ushort)SettingsUtil.Int(DefaultKeys.DiscoveryServiceClientPort, 0),
                listenAddressBuffer,
                listenAddressSize,
                (ushort)SettingsUtil.Int(DefaultKeys.TextServiceListenPort, 9819),
                ((byte)SettingsUtil.Int(DefaultKeys.GroupIdentity, 0))
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
            airx_text_service(AirXInstance, OnTextReceived, ShouldInterrupt);
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


    const string DLL_NAME = "libairx.dll";

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern int airx_version();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool airx_is_first_run();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr airx_create(UInt16 discovery_service_server_port,
                                            UInt16 discovery_service_client_port,
                                            IntPtr text_service_listen_addr,
                                            UInt32 text_service_listen_addr_len,
                                            UInt16 text_service_listen_port,
                                            byte group_identity);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr airx_restore();

    // Define delegate for the interrupt function
    public delegate bool InterruptFunc();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_lan_discovery_service(IntPtr airx_ptr, InterruptFunc should_interrupt);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_lan_discovery_service_async(IntPtr airx_ptr, InterruptFunc should_interrupt);

    // Define delegate for the callback function
    public delegate void CallbackFunc(IntPtr text, uint textLen, IntPtr sourceIpAddress, uint sourceIpAddressLen);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_text_service(IntPtr airx_ptr, CallbackFunc callback, InterruptFunc should_interrupt);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_text_service_async(IntPtr airx_ptr, CallbackFunc callback, InterruptFunc should_interrupt);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool airx_lan_broadcast(IntPtr airx_ptr);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint airx_get_peers(IntPtr airx_ptr, string buffer);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_start_auto_broadcast(IntPtr airx_ptr);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_send_text(IntPtr airx_ptr, string host, uint host_len, IntPtr text, uint text_len);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void airx_broadcast_text(IntPtr airx_ptr, IntPtr text, uint len);

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
}
