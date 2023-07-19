using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace AirX.Util
{
    /// <summary>
    ///  所有设置项
    /// </summary>
    public enum Keys
    {
        IsNotFirstRun,                  /** 是否并非首次运行 */

        DiscoveryServiceClientPort,
        DiscoveryServiceServerPort,
        DataServiceListenPort,
        DataServiceAddressIpV4,
        GroupIdentifier,

        LoggedInUid,
        SavedUid,                       /** 上次登录的UID */
        SavedCredentialType,            /** 保存的密码类型（明文？token？） */
        SavedCredential,                /** 保存的密码内容 */
        ShouldAutoSignIn,               /** 是否应记住密码 */

        BlockList,

        ShouldShowConsole,              /** 是否显示开发者控制台  */
        ShouldShowAdvancedSettings,     /** 是否显示高级设置 */ 

        IsKafkaProducer,                /** 是否启用服务端的KafkaProducer组件，从而允许向互联网的用户分享剪贴板数据（正在施工） */
        IsKafkaConsumer,                /** 是否启用服务端的KafkaConsumer组件，从而允许从互联网的用户读取剪贴板数据（正在施工） */
        AirXCloudAddress,               /** 设置AirX服务器地址，更改即可实现私服 */

        SaveFilePath,                   /** 下载文件的保存目录 */

        NewTextPopupDisplayTimeMillis,  /** 新文本窗口在多少毫秒后自动消失 */
    }

    public enum CredentialType
    {
        Password,
        AirXToken,
        GoogleToken
    }

    public class SettingsUtil
    {
        /// 得到管理设置的对象，设置被称为LocalSettings
        private static ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        /// 尝试初始化设置
        public static void TryInitializeConfigurationsForFirstRun()
        {
            /// 如果不是第一次运行了，不要初始化
            if (Bool(Keys.IsNotFirstRun, false))
            {
                return;
            }

            /// 只有第一次运行，才初始化
            Write(Keys.DiscoveryServiceClientPort, 0);
            Write(Keys.DiscoveryServiceServerPort, 9818);
            Write(Keys.DataServiceListenPort, 9819);
            Write(Keys.DataServiceAddressIpV4, "0.0.0.0");
            Write(Keys.GroupIdentifier, 0);
            Write(Keys.ShouldAutoSignIn, false);
            Write(Keys.IsNotFirstRun, true);
            Write(Keys.ShouldShowConsole, false);
            Write(Keys.ShouldShowAdvancedSettings, false);
            Write(Keys.IsKafkaProducer, true);
            Write(Keys.IsKafkaConsumer, true);
            Write(Keys.AirXCloudAddress, "https://airx.eggtartc.com");
            Write(Keys.SaveFilePath, "C:\\AirXFiles");
            Write(Keys.NewTextPopupDisplayTimeMillis, 6000);
        }

        /// 读取String类型数据
        public static string String(Keys key, string def)
        {
            return localSettings.Values[key.ToString()] as string ?? def;
        }

        /// 读取bool类型数据
        public static bool Bool(Keys key, bool def)
        {
            if (bool.TryParse(String(key, "!"), out bool ret))
            {
                return ret;
            }
            return def;
        }

        public static int Int(Keys key, int def)
        {
            if (int.TryParse(String(key, "!"), out int ret))
            {
                return ret;
            }
            return def;
        }

        public static double Double(Keys key, double def)
        {
            if (double.TryParse(String(key, "!"), out double ret))
            {
                return ret;
            }
            return def;
        }

        public static void Delete(Keys key)
        {
            localSettings.Values.Remove(key.ToString());
        }

        /// 读取保存的密码信息
        public static string SavedCredential()
        {
            return String(Keys.SavedCredential, "");
        }

        /// 读取保存的密码类型信息
        public static CredentialType ReadCredentialType()
        {
            string rawValue = String(Keys.SavedCredentialType, CredentialType.Password.ToString());
            return Enum.TryParse<CredentialType>(rawValue, out CredentialType credentialType)
                ? credentialType
                : CredentialType.Password;
        }

        /// 读取黑名单数据
        public static HashSet<string> ReadBlockList()
        {
            string rawValue = String(Keys.BlockList, "");
            return rawValue.Split(
                ",",
                StringSplitOptions.TrimEntries 
                    & StringSplitOptions.RemoveEmptyEntries
            ).ToHashSet();
        }

        /// 写入黑名单数据
        public static void WriteBlockList(HashSet<string> blockList)
        {
            Write(Keys.BlockList, string.Join(',', blockList));
        }

        /// 写入任意数据
        public static void Write(Keys key, object value)
        {
            localSettings.Values[key.ToString()] = value.ToString();
        }
    }
}
