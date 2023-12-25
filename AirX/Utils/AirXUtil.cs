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
    class AirXUtil
    {
        public static async Task UserSendFileAsync()
        {
            var files = await FileUtil.OpenFileDialogAsync();
            if (files.Count == 0 || files.First() == null)
            {
                return;
            }

            if (AirXBridge.GetPeers().Count == 0)
            {
                UIUtil.MessageBoxAsync("Error", "No peers available", "OK", null)
                    .FireAndForget();
                return;
            }

            var window = new SelectPeerWindow();
            window.SelectPeer(peer =>
            {
                foreach (var file in files)
                {
                    AirXBridge.TrySendFile(file.Path, peer.Value);
                }
            });
        }

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
