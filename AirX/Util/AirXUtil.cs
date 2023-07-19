using AirX.Extension;
using AirX.Model;
using AirX.View;
using AirX.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Util
{
    /// <summary>
    /// 封装一些AirX特定操作
    /// </summary>
    class AirXUtil
    {
        /// <summary>
        /// 用户选择发送文件的流程
        /// </summary>
        /// <returns></returns>
        public static async Task UserSendFileAsync()
        {
            /// 用户选择文件
            var files = await FileUtil.OpenFileDialogAsync();
            if (files.Count == 0 || files.First() == null)
            {
                /// 如果没选，直接返回
                return;
            }

            /// 获得在线Peer数
            if (AirXBridge.GetPeers().Count == 0)
            {
                /// 无人在线的话，弹出提示
                UIUtil.MessageBoxAsync("Error", "No peers available", "OK", null)
                    .FireAndForget();
                return;
            }

            /// 弹出选择Peer的窗口
            var window = new SelectPeerWindow();
            window.SelectPeer(peer =>
            {
                /// 用户选择了Peer，开始发送文件
                foreach (var file in files)
                {
                    /// 为每个文件创建一个SendFile对象，进行发送
                    AirXBridge.TrySendFile(file.Path, peer.Value);
                }
            });
        }

        /// <summary>
        /// 用户切换AirX服务开启和关闭
        /// </summary>
        public static void UserToggleService()
        {
            if (GlobalViewModel.Instance.IsServiceOnline)
            {
                AirXBridge.TryStopAirXService();
            }
            else
            {
                AirXBridge.TryStartAirXService();
            }
        }
    }
}
