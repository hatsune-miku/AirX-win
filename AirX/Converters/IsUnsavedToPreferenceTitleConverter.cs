using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Converters
{
    /// <summary>
    /// 是否未保存到偏好设置标题的转换器
    /// 用于及时在标题栏显示"Unsaved"
    /// </summary>
    public class IsUnsavedToPreferenceTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value
                ? "Preferences - Unsaved"
                : "Preferences";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
