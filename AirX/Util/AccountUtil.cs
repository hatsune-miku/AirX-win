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
        public static void ClearSavedUserInfoAndSignOut()
        {
            SettingsUtil.Delete(DefaultKeys.SavedCredential);
            SettingsUtil.Delete(DefaultKeys.LoggedInUid);
            SettingsUtil.Delete(DefaultKeys.SavedCredentialType);
        }


        /**
         * Return: true if successfully logged in, otherwise, false.
         */
        public static async Task<bool> TryAutomaticLogin()
        {
            GlobalViewModel.Instance.IsSignedIn = false;
            Debug.WriteLine("Trying automatic login...");

            // Check credentials!
            if (SettingsUtil.ReadCredentialType() != CredentialType.AirXToken)
            {
                Debug.WriteLine("Failed: incorrect credential type");
                return false;
            }

            string token = SettingsUtil.String(DefaultKeys.SavedCredential, "");
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("Failed: empty token");
                return false;
            }

            string uid = SettingsUtil.String(DefaultKeys.SavedUid, "");
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

            AirXCloud.GreetingsResponse greetingsResponse;
            try
            {
                greetingsResponse = await AirXCloud.GreetingsAsync(uid);
                if (greetingsResponse.success)
                {
                    GlobalViewModel.Instance.LoggingGreetingsName = greetingsResponse.name;
                }
            }
            catch { }

            // Almost there!
            Debug.WriteLine("Success.");
            GlobalViewModel.Instance.LoggingInUid = uid;
            GlobalViewModel.Instance.IsSignedIn = true;
            SettingsUtil.Write(DefaultKeys.LoggedInUid, uid);
            SettingsUtil.Write(DefaultKeys.SavedCredential, renewResponse.token);
            return true;
        }
    }
}
