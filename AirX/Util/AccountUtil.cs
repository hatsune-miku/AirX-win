using AirX.Bridge;
using AirX.Model;
using AirX.Services;
using AirX.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Util
{
    public class AccountUtil
    {
        private static HashSet<string> _blockList = SettingsUtil.ReadBlockList();

        public static void ClearSavedUserInfoAndSignOut()
        {
            SettingsUtil.Delete(Keys.SavedCredential);
            SettingsUtil.Delete(Keys.LoggedInUid);
            GlobalViewModel.Instance.IsSignedIn = false;
            GlobalViewModel.Instance.LoggingInUid = "";
            GlobalViewModel.Instance.LoggingGreetingsName = "AirX User";
        }

        public static bool IsInBlockList(string ipAddress)
        {
            if (!Peer.TryParse(ipAddress, out Peer peer))
            {
                return false;
            }
            return _blockList.Contains(peer.IpAddress);
        }

        public static void AddToBlockList(string ipAddress)
        {
            if (!Peer.TryParse(ipAddress, out Peer peer))
            {
                return;
            }
            _blockList.Add(peer.IpAddress);
            SettingsUtil.WriteBlockList(_blockList);
        }

        public static async Task<bool> TryGreetings()
        {
            AirXCloud.GreetingsResponse greetingsResponse;
            try
            {
                var uid = SettingsUtil.String(Keys.SavedUid, "");
                greetingsResponse = await AirXCloud.GreetingsAsync(uid);
                if (greetingsResponse.success)
                {
                    GlobalViewModel.Instance.LoggingGreetingsName = greetingsResponse.name;
                    return true;
                }
            }
            catch { }

            return false;
        }

        /**
         * Return: true if successfully logged in, otherwise, false.
         */
        public static async Task<bool> TryAutomaticLogin()
        {
            if (!SettingsUtil.Bool(Keys.ShouldAutoSignIn, false))
            {
                return false;
            }

            GlobalViewModel.Instance.IsSignedIn = false;
            Debug.WriteLine("Trying automatic login...");

            // Check credentials!
            if (SettingsUtil.ReadCredentialType() != CredentialType.AirXToken)
            {
                Debug.WriteLine("Failed: incorrect credential type");
                return false;
            }

            string token = SettingsUtil.String(Keys.SavedCredential, "");
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("Failed: empty token");
                return false;
            }

            string uid = SettingsUtil.String(Keys.SavedUid, "");
            if (string.IsNullOrEmpty(uid))
            {
                Debug.WriteLine("Failed: empty uid");
                return false;
            }

            AirXCloud.RenewResponse renewResponse;
            try
            {
                renewResponse = await AirXCloud.RenewAsync(uid);
                if (!renewResponse.success)
                {
                    Debug.WriteLine($"Failed: renew failed: {renewResponse.message}");
                    return false;
                }
            }
            catch
            {
                return false;
            }

            await TryGreetings();

            // Almost there!
            Debug.WriteLine("Success.");
            GlobalViewModel.Instance.LoggingInUid = uid;
            GlobalViewModel.Instance.IsSignedIn = true;
            SettingsUtil.Write(Keys.LoggedInUid, uid);
            SettingsUtil.Write(Keys.SavedCredential, renewResponse.token);
            return true;
        }
    }
}
