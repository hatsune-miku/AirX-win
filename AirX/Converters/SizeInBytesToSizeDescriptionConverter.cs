using AirX.Util;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Converters
{
    /// <summary>
    /// 字节大小到描述的转换器
    /// </summary>
    public class SizeInBytesToSizeDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ulong sizeInBytes = (ulong)value;
            return FileUtil.GetFileSizeDescription(sizeInBytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
