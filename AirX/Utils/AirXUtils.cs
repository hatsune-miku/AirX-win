using AirX.Extension;
using AirX.Model;
using AirX.View;
using AirX.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace AirX.Util
{
    class AirXUtils
    {
        public static async Task UserSendFileAsync()
        {
            var files = await FileUtils.OpenFileDialogAsync();
            if (files.Count == 0 || files.First() == null)
            {
                return;
            }

            if (AirXBridge.GetPeers().Count == 0)
            {
                PInvoke.MessageBox(
                    HWND.Null, "No peers available", "Error", Windows.Win32.UI.WindowsAndMessaging.MESSAGEBOX_STYLE.MB_ICONINFORMATION);
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
