using AirX.Bridge;
using AirX.Extension;
using AirX.Model;
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

/// <summary>
/// 包含对libairx.dll的封装、监听剪贴板、处理文件传输等功能
/// </summary>
public class AirXBridge
{
    // ========================================================================================
    // 知识点：Delegate（代理方法），实质还是回调函数
    // 而回调函数的实质，是为了“异步告知结果”
    //
    // 比如面试时，面试官说：回家等电话，有结果通知你！
    // 此时，面试结果并不是立刻就有的，需要过一段时间才有，即异步的。
    // 这时候你的电话号码就形成了一个回调，面试官得到结果后，需要通知你，怎么通知呢，通过你提前留下联系方式。
    // ========================================================================================

    /// <summary>
    /// 当收到文本消息时触发的回调函数。
    /// 这就像你留下联系方式，以便未来有消息时可以通知你一样。
    /// </summary>
    /// <param name="text">文本内容</param>
    /// <param name="from">发送方IP地址</param>
    public delegate void OnTextReceivedHandler(string text, string from);

    /// <summary>
    /// 当收到文件传输请求时，触发的回调函数
    /// </summary>
    /// <param name="fileSize">文件字节数</param>
    /// <param name="fileName">文件在发送方的绝对路径</param>
    /// <param name="from">发送方IP地址</param>
    public delegate void OnFileComingHandler(UInt64 fileSize, string fileName, string from);

    /// <summary>
    /// 当我方所发出的文件，其传输状态发生变化时，触发的回调函数
    /// </summary>
    /// <param name="fileId">我方所规定的FileId</param>
    /// <param name="progress">已经传输的字节数</param>
    /// <param name="total">文件总字节数</param>
    /// <param name="status">传输状态</param>
    public delegate void OnFileSendingHandler(byte fileId, UInt64 progress, UInt64 total, FileStatus status);

    /// <summary>
    /// 每当我方收到一个文件分块时，触发的回调函数
    /// </summary>
    /// <param name="fileId">我方规定的FileId</param>
    /// <param name="offset">偏移字节数</param>
    /// <param name="length">应写入字节数</param>
    /// <param name="data">应写入的数据（字节数组形式）</param>
    /// <returns></returns>
    public delegate bool OnFilePartHandler(byte fileId, UInt64 offset, UInt64 length, byte[] data);

    /// <summary>
    ///  文件传输状态
    /// </summary>
    public enum FileStatus
    {
        Requested = 1,              /** 此状态代表：准备发出文件，等待接收方同意或拒绝中 */
        Rejected = 2,               /** 此状态代表：文件被接收方拒绝 */
        Accepted = 3,               /** 此状态代表：接收方同意接收 */
        InProgress = 4,             /** 此状态代表：正在传输过程中 */
        CancelledBySender = 5,      /** 此状态代表：发送方中途停止发送 */
        CancelledByReceiver = 6,    /** 此状态代表：接收方中途停止接收 */
        Completed = 7,              /** 此状态代表：传输完成，且接收方处理数据完毕 */
        Error = 8                   /** 此状态代表：传输过程中出现错误 */
    }

    /// <summary>
    /// AirX实例，用于调用libairx.dll中的方法
    /// </summary>
    private static IntPtr AirXInstance = IntPtr.Zero;

    /// <summary>
    /// 发现服务的线程
    /// </summary>
    private static Thread AirXDiscoveryThread = null;

    /// <summary>
    /// 数据服务的线程
    /// </summary>
    private static Thread AirXTextServiceThread = null;

    /// <summary>
    /// 是否应当停服
    /// </summary>
    private static bool ShouldInterruptSignal = false;

    /// <summary>
    /// 存放Peer List用
    /// </summary>
    private static IntPtr PeerListBuffer = Utf8StringAlloc(1024);

    /// <summary>
    /// 监听剪贴板变化用的周期计时器
    /// </summary>
    private static DispatcherTimer Timer = null;

    /// <summary>
    /// 上次剪贴板内容
    /// </summary>
    private static string lastClipboardText = "";

    /// <summary>
    /// 是否跳过下一次剪贴板变化事件
    /// </summary>
    private static bool ShouldSkipNextEvent = false;

    /// <summary>
    /// 上面一堆delegate是定义回调函数的概念
    /// 这里是用来存放具体的回调函数
    /// </summary>
    private static OnTextReceivedHandler onTextReceivedHandler = null;
    private static OnFileComingHandler onFileComingHandler = null;
    private static OnFileSendingHandler onFileSendingHandler = null;
    private static OnFilePartHandler onFilePartHandler = null;

    /// <summary>
    /// 用于保证代码在UI线程上执行
    /// 只有在UI线程上才能操作UI，比如弹出一个消息到屏幕上
    /// </summary>
    private static SynchronizationContext synchronizationContext = SynchronizationContext.Current;

    /// <summary>
    /// 调用kernel32.dll中的WinAPI，实现：显示出本程序默认隐藏的控制台窗口
    /// </summary>
    public static void RedirectAirXStdoutToDebugConsole()
    {
        PInvoke.Kernel32.AllocConsole();
    }

    /// <summary>
    /// 一个时钟周期
    /// </summary>
    private static void OnTimerTick(object sender, object e)
    {
        // 得到剪贴板内容
        var packageView = Clipboard.GetContent();
        try
        {
            // 如果剪贴板根本不含文本，返回
            if (!packageView.AvailableFormats.Contains(StandardDataFormats.Text) || !packageView.Contains(StandardDataFormats.Text))
            {
                return;
            }
        }
        catch { }

        try
        {
            // 开始读取剪贴板内容。注意看这里 t => { ... } 就是一个回调函数
            // 当读取完成后，会调用这个回调函数
            packageView.GetTextAsync().AsTask().ContinueWith(t =>
            {
                // 此时剪贴板读取完毕
                try
                {
                    // 得到剪贴板内容
                    // 如果剪贴板内容和上次不一样，说明变化了
                    if (t.Result != lastClipboardText)
                    {
                        lastClipboardText = t.Result;

                        // 现在“剪贴板变化”有结果了！立刻通知联系人
                        OnClipboardChanged(t.Result);
                    }
                }
                catch (Exception)
                { }

                // "TaskScheduler.Default).FireAndForget()"是固定搭配
            }, TaskScheduler.Default).FireAndForget();
        }
        catch (Exception ex)
        {
            // 发生任何错误的话，打印出来
            Debug.WriteLine(ex);
        }
    }

    /// <summary>
    /// “剪贴板变化”的回调函数的具体实现
    /// </summary>
    /// <param name="newText">新的文本</param>
    private static void OnClipboardChanged(string newText)
    {
        if (ShouldSkipNextEvent)
        {
            ShouldSkipNextEvent = false;
            return;
        }
        Debug.WriteLine("Clipboard changed");

        // 新的文本转化为原始指针，以便传递给libairx.dll
        IntPtr buffer = CreateUtf8String(newText, out uint size);
        AirXNative.airx_broadcast_text(AirXInstance, buffer, size);

        // 释放原始指针
        FreeUtf8String(buffer);
        Debug.WriteLine("Sent");
    }

    /// <summary>
    /// “当收到文本”事件的回调函数
    /// </summary>
    /// <param name="text">文本指针</param>
    /// <param name="textLen">文本字节数</param>
    /// <param name="sourceIpAddress">源IP指针</param>
    /// <param name="sourceIpAddressLen">源IP字节数</param>
    private static void OnTextReceived(IntPtr text, uint textLen, IntPtr sourceIpAddress, uint sourceIpAddressLen)
    {
        ShouldSkipNextEvent = true;

        // 从原始指针转化为C#字符串
        string incomingText = Utf8StringFromPtr(text, (int)textLen);
        string sourceIpAddressSring = Utf8StringFromPtr(sourceIpAddress, (int)sourceIpAddressLen);

        // 进行文本复制
        try
        {
            var package = new DataPackage();
            package.SetText(incomingText);
            Clipboard.SetContent(package);
        }
        catch (Exception) { }

        // 然后通知回调
        // 为什么回调又调用回调？
        // 因为OnTextReceived是 libairx 回调 Bridge
        // 而onTextReceivedHandler是  Bridge 回调 实际业务逻辑
        // 有这么一个传递的过程
        onTextReceivedHandler?.Invoke(incomingText, sourceIpAddressSring);
    }

    /// <summary>
    /// “当收到文件”事件的回调函数
    /// </summary>
    /// <param name="fileSize">文件数据字节数</param>
    /// <param name="fileName">文件名指针</param>
    /// <param name="fileNamelen">文件名字节数</param>
    /// <param name="sourceIpAddress">源IP指针</param>
    /// <param name="sourceIpAddressLen">源IP字节数</param>
    private static void OnFileComing(UInt64 fileSize, IntPtr fileName, uint fileNamelen, IntPtr sourceIpAddress, uint sourceIpAddressLen)
    {
        // 同上
        string fileNameString = Utf8StringFromPtr(fileName, (int)fileNamelen);
        string sourceIpAddressString = Utf8StringFromPtr(sourceIpAddress, (int)sourceIpAddressLen);

        onFileComingHandler?.Invoke(fileSize, fileNameString, sourceIpAddressString);
    }

    /// <summary>
    /// “当我方所发文件有了新动态”事件的回调函数
    /// </summary>
    private static void OnFileSending(byte fileId, ulong progress, ulong total, byte status)
    {
        // 同上
        onFileSendingHandler?.Invoke(fileId, progress, total, (FileStatus)status);
    }

    /// <summary>
    /// “收到文件的分块”事件的回调函数
    /// </summary>
    /// <returns>true: 断开连接，false: 不断开</returns>
    /// 这个函数具有决定TCP连接是否应该断开的权利：返回`true`断开，返回`false`不断。
    /// 这样一来，当发现用户已经把某个文件取消掉的时候，就可以通过返回`true`来断开连接
    /// 从而防止发送方白白浪费带宽
    private static bool OnFilePart(byte fileId, UInt64 offset, UInt64 length, IntPtr data)
    {
        try
        {
            // 从指针里把实际字节数组读取出来
            byte[] dataBytes = new byte[length];
            Marshal.Copy(data, dataBytes, 0, (int)length);

            // 带着字节数组再去通知回调
            // 不然带着指针去通知回调，回调还得自己去读取指针里的内容，不像话
            return onFilePartHandler?.Invoke(fileId, offset, length, dataBytes)
                ?? false;

            // ↑ 可能注意到函数调用总有个 ?，这是因为回调函数可能为空，即没人用得着我去通知
            // ?.Invoke，意为如果有，则调用，没有则不调用
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }

    /// 是否应该停服，交给libairx去调用的
    private static bool ShouldInterrupt()
    {
        return ShouldInterruptSignal;
    }

    /// 这四个Set Handler参见前文的Delegate定义
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

    /// <summary>
    /// 开服！
    /// </summary>
    public static bool TryStartAirXService()
    {
        // 如果已经开了那就不开了
        if (GlobalViewModel.Instance.IsServiceOnline)
        {
            return false;
        }

        ShouldInterruptSignal = false;

        /// 调用libairx，凡是字符串这种复杂类型都要先退化为指针
        /// 也就是CreateUtf8String
        string listenAddress = "0.0.0.0";
        IntPtr listenAddressBuffer = CreateUtf8String(listenAddress, out uint listenAddressSize);

        try
        {
            Debug.WriteLine("AirX version: " + AirXNative.airx_version());
            Debug.WriteLine("AirX compabilitily version: " + AirXNative.airx_compatibility_number());

            /// 正式调用
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

        /// 指针用完记得手动释放
        FreeUtf8String(listenAddressBuffer);

        /// 启动两个线程，一个用来处理发现服务，一个用来处理数据服务
        AirXDiscoveryThread = new Thread(() =>
        {
            // 线程开始
            Debug.WriteLine("Discovery start");
            AirXNative.airx_lan_discovery_service(AirXInstance, ShouldInterrupt);
            Debug.WriteLine("Discovery end");

            // 代码执行到这里说明发现服务停止了
            synchronizationContext.Post((_) =>
            {
                // 在UI线程上通知UI：发现服务停止了
                GlobalViewModel.Instance.IsDiscoveryServiceOnline = false;
                if (!GlobalViewModel.Instance.IsTextServiceOnline)
                {
                    GlobalViewModel.Instance.IsServiceOnline = false;
                }
            }, null);
        });
        AirXTextServiceThread = new Thread(() =>
        {
            // 另一个线程也同时开始
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

            // 代码执行到这里说明数据服务停止了
            synchronizationContext.Post((_) =>
            {
                // 在UI线程上通知UI：数据服务停止了
                GlobalViewModel.Instance.IsTextServiceOnline = false;
                if (!GlobalViewModel.Instance.IsDiscoveryServiceOnline)
                {
                    GlobalViewModel.Instance.IsServiceOnline = false;
                }
            }, null);
        });

        /// 启动两根儿线程
        AirXDiscoveryThread.Start();
        AirXTextServiceThread.Start();

        /// 启动用于监听剪贴板的定时器
        /// 时钟周期为500ms，即每500ms检查一次剪贴板是否有变
        Timer = new DispatcherTimer();
        Timer.Interval = TimeSpan.FromMilliseconds(500);
        Timer.Tick += OnTimerTick;
        Timer.Start();

        /// 通知UI：两个服务已经开启
        GlobalViewModel.Instance.IsServiceOnline = true;
        GlobalViewModel.Instance.IsDiscoveryServiceOnline = true;
        GlobalViewModel.Instance.IsTextServiceOnline = true;
        return true;
    }

    /// <summary>
    /// 停服！
    /// </summary>
    public static void TryStopAirXService()
    {
        // 如果已经停服了那就不停了
        if (!GlobalViewModel.Instance.IsServiceOnline)
        {
            return;
        }

        // 把“是否应该停服”信号置为true
        ShouldInterruptSignal = true;

        // 停止监听剪贴板
        Timer.Stop();
        Timer = null;
    }

    /// 读取当前Peers列表
    public static List<Peer> GetPeers()
    {
        /// 调用libairx读取Peers，同时得到数据的字节数
        uint len = AirXNative.airx_get_peers(AirXInstance, PeerListBuffer);
        if (len <= 0)
        {
            return new List<Peer>();
        }

        /// 把字节数组转换为字符串
        string peers = Utf8StringFromPtr(PeerListBuffer, (int)len);
        try
        {
            /// 把字符串转换为Peer列表
            return new List<Peer>(
                peers.Split(',')
                .Select(ps => Peer.Parse(ps)
                )
            );
        }
        catch (Exception ex)
        {
            /// 如果转换失败，那就返回空列表，视为当前无人在线
            Debug.WriteLine(ex.Message);
            return new List<Peer>();
        }
    }

    /// 发送文件的封装
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

    /// 响应一个文件是否要接收
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

    /// 读取字符串形式的libairx版本信息
    public static string GetVersionString()
    {
        var versionString = Utf8StringAlloc(128);
        var actualLength = AirXNative.airx_version_string(versionString);
        var ret = Utf8StringFromPtr(versionString, (int) actualLength);
        FreeUtf8String(versionString);
        return ret;
    }

    /// <summary>
    /// 程序退出之前的收尾工作
    /// </summary>
    public static void Deinit()
    {
        /// 释放存放Peer List用的内存
        FreeUtf8String(PeerListBuffer);
        PeerListBuffer = IntPtr.Zero;
    }


    // Define delegate for the interrupt function
    public delegate bool InterruptFunc();

    // Define delegate for the callback function
    /// 又是一大波delegate！
    /// 和前半段的delegate不同，这里的delegate是用来传递给libairx的
    /// 而那段儿是从这里传递到实际业务逻辑的
    public delegate void TextCallbackFunction(
        IntPtr text, uint textLen, IntPtr sourceIpAddress, uint sourceIpAddressLen);
    public delegate void FileComingCallbackFunction(
        UInt64 fileSize, IntPtr fileName, uint fileNamelen, IntPtr sourceIpAddress, uint sourceIpAddressLen);
    public delegate void FileSendingCallbackFunction(
        byte fileId, UInt64 progress, UInt64 total, byte status);
    public delegate bool FilePartCallbackFunction(
        byte fileId, UInt64 offset, UInt64 length, IntPtr data);

    /// <summary>
    /// 工具函数：把一个字符串转换为UTF8编码的字节数组的指针
    /// </summary>
    public static IntPtr CreateUtf8String(string s, out uint size)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        size = (uint) bytes.Length;
        return ptr;
    }

    /// <summary>
    /// 工具函数：释放一个UTF8编码的字节数组的指针
    /// </summary>
    public static void FreeUtf8String(IntPtr ptr)
    {
        Marshal.FreeHGlobal(ptr);
    }

    /// <summary>
    /// 工具函数：从一个UTF8编码的字节数组的指针中读取字符串
    /// </summary>
    public static string Utf8StringFromPtr(IntPtr ptr, int length)
    {
        return Marshal.PtrToStringUTF8(ptr, length);
    }

    /// <summary>
    /// 工具函数：分配一块儿内存
    /// </summary>
    public static IntPtr Utf8StringAlloc(uint size)
    {
        return Marshal.AllocHGlobal((int)size);
    }
}
