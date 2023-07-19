using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static AirXBridge;

namespace AirX.Bridge
{
    // Source: bridge.h
    // 从libairx中的bridge.h照搬的内容
    // 只不过转换为C#语法
    internal class AirXNative
    {
        const string DLL_NAME = "libairx.dll";

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airx_version();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int airx_compatibility_number();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void airx_init();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr airx_create(
            UInt16 discovery_service_server_port,
            UInt16 discovery_service_client_port,
            IntPtr text_service_listen_addr,
            UInt32 text_service_listen_addr_len,
            UInt16 text_service_listen_port,
            UInt32 group_identity
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool airx_lan_broadcast(IntPtr airxPtr);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint airx_get_peers(IntPtr airxPtr, IntPtr buffer);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 airx_version_string(IntPtr buffer);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void airx_send_text(
            IntPtr airxPtr,
            string host,
            uint host_len,
            IntPtr text,
            uint text_len
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void airx_broadcast_text(
            IntPtr airxPtr,
            IntPtr text,
            uint len
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void airx_lan_discovery_service(
            IntPtr airxPtr,
            InterruptFunc should_interrupt
        );


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
        public static extern void airx_respond_to_file(
            IntPtr airxPtr,
            IntPtr host,
            uint host_len,
            byte file_id,
            UInt64 file_size,
            IntPtr file_path,
            uint file_path_len,
            bool accept
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void airx_try_send_file(
            IntPtr airxPtr,
            IntPtr host,
            uint host_len,
            IntPtr file_path,
            uint file_path_len
        );
    }
}
