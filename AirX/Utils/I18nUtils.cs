using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace AirX.Utils
{
    internal class I18nUtils
    {
        private static readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("Resources");

        public static string Tr(string uid)
        {
            uid = uid.Replace(".", "/");
            return _loader.GetString(uid);
        }
    }

    public static class StringI18nExtension
    {
        public static string Tr(this string uid)
        {
            return I18nUtils.Tr(uid);
        }

        public static string Text(this string uid)
        {
            return I18nUtils.Tr(uid + "/Text");
        }

        public static string Description(this string uid)
        {
            return I18nUtils.Tr(uid + "/Description");
        }
    }
}
