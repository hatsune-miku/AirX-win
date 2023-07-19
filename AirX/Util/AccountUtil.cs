using AirX.Bridge;
using AirX.Model;
using AirX.Services;
using AirX.View;
using AirX.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Util
{
    /// 用户账户工具类
    public class AccountUtil
    {
        /// 黑名单信息
        private static HashSet<string> _blockList = SettingsUtil.ReadBlockList();

        /// 登出，清除登录信息
        public static void ClearSavedUserInfoAndSignOut()
        {
            SettingsUtil.Delete(Keys.SavedCredential);
            SettingsUtil.Delete(Keys.LoggedInUid);
            GlobalViewModel.Instance.IsSignedIn = false;
            GlobalViewModel.Instance.LoggingInUid = "";
            GlobalViewModel.Instance.LoggingGreetingsName = "AirX User";
        }

        /// <summary>
        /// 用户是否在黑名单中
        /// </summary>
        public static bool IsInBlockList(string ipAddress)
        {
            if (!Peer.TryParse(ipAddress, out Peer peer))
            {
                return false;
            }
            return _blockList.Contains(peer.IpAddress);
        }

        /// <summary>
        /// 把用户加入黑名单
        /// </summary>
        public static void AddToBlockList(string ipAddress)
        {
            if (!Peer.TryParse(ipAddress, out Peer peer))
            {
                return;
            }
            _blockList.Add(peer.IpAddress);
            SettingsUtil.WriteBlockList(_blockList);
        }

        /// <summary>
        /// 发送Greetings请求，用于token有效性测试和用户账户信息的获取
        /// </summary>
        public static async Task<bool> SendGreetingsAsync()
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

        /// 尝试自动登录，返回是否登陆成功
        /**
         * Return: true if successfully logged in, otherwise, false.
         */
        public static async Task<bool> TryAutomaticLoginAsync()
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
                /// 尝试续期Token
                renewResponse = await AirXCloud.RenewAsync(uid);
                if (!renewResponse.success)
                {
                    /// 续期失败
                    Debug.WriteLine($"Failed: renew failed: {renewResponse.message}");
                    return false;
                }
            }
            catch
            {
                return false;
            }

            await SendGreetingsAsync();

            /// 续期成功，更新新的token
            Debug.WriteLine("Success.");
            GlobalViewModel.Instance.LoggingInUid = uid;
            GlobalViewModel.Instance.IsSignedIn = true;
            SettingsUtil.Write(Keys.LoggedInUid, uid);
            SettingsUtil.Write(Keys.SavedCredential, renewResponse.token);
            return true;
        }

        /// <summary>
        /// 切换用户登录状态
        /// </summary>
        public static void UserToggleSignInOut()
        {
            if (GlobalViewModel.Instance.IsSignedIn)
            {
                GlobalViewModel.Instance.IsSignedIn = false;
                ClearSavedUserInfoAndSignOut();
                return;
            }

            var window = new LoginWindow();
            window.Activate();
        }
    }
}
