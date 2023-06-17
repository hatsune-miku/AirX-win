using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace AirX.Util
{
    public enum DefaultKeys
    {
        IsNotFirstRun,

        DiscoveryServiceClientPort,
        DiscoveryServiceServerPort,
        TextServiceListenPort,
        GroupIdentity,

        LoggedInUid,
        SavedUid,
        SavedCredentialType,
        SavedCredential,
        ShouldAutoSignIn,

        BlockList,
    }

    public enum CredentialType
    {
        Password,
        AirXToken,
        GoogleToken
    }

    public class SettingsUtil
    {
        private static ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public static void TryInitializeConfigurationsForFirstRun()
        {
            // TODO: remove
            Write(DefaultKeys.BlockList, "");

            if (Bool(DefaultKeys.IsNotFirstRun, false))
            {
                return;
            }
            
            // 像啊，很像啊
            Write(DefaultKeys.DiscoveryServiceClientPort, 0);
            Write(DefaultKeys.DiscoveryServiceServerPort, 9818);
            Write(DefaultKeys.TextServiceListenPort, 9819);
            Write(DefaultKeys.GroupIdentity, 0);
            Write(DefaultKeys.ShouldAutoSignIn, false);
            Write(DefaultKeys.IsNotFirstRun, true);
        }

        public static string String(DefaultKeys key, string def)
        {
            return localSettings.Values[key.ToString()] as string ?? def;
        }

        public static bool Bool(DefaultKeys key, bool def)
        {
            if (bool.TryParse(String(key, "!"), out bool ret))
            {
                return ret;
            }
            return def;
        }

        public static int Int(DefaultKeys key, int def)
        {
            if (int.TryParse(String(key, "!"), out int ret))
            {
                return ret;
            }
            return def;
        }

        public static double Double(DefaultKeys key, double def)
        {
            if (double.TryParse(String(key, "!"), out double ret))
            {
                return ret;
            }
            return def;
        }

        public static void Delete(DefaultKeys key)
        {
            localSettings.Values.Remove(key.ToString());
        }

        // Utility methods
        public static string SavedCredential()
        {
            return String(DefaultKeys.SavedCredential, "");
        }

        public static CredentialType ReadCredentialType()
        {
            string rawValue = String(DefaultKeys.SavedCredentialType, CredentialType.Password.ToString());
            return Enum.TryParse<CredentialType>(rawValue, out CredentialType credentialType)
                ? credentialType
                : CredentialType.Password;
        }

        public static HashSet<string> ReadBlockList()
        {
            string rawValue = String(DefaultKeys.BlockList, "");
            return rawValue.Split(
                ",",
                StringSplitOptions.TrimEntries 
                    & StringSplitOptions.RemoveEmptyEntries
            ).ToHashSet();
        }

        public static void WriteBlockList(HashSet<string> blockList)
        {
            Write(DefaultKeys.BlockList, string.Join(',', blockList));
        }

        public static void Write(DefaultKeys key, object value)
        {
            localSettings.Values[key.ToString()] = value;
        }

        public static void Write(DefaultKeys key, bool value)
        {
            Write(key, value.ToString());
        }

        public static void Write(DefaultKeys key, CredentialType value)
        {
            Write(key, value.ToString());
        }
    }
}
