using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirX.Converters
{
    /// <summary>
    /// 接收文件状态到是否应启用取消按钮的转换器
    /// </summary>
    class ReceiveFileStatusToShouldEnableCancelButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (AirXBridge.FileStatus) value;
            return status != AirXBridge.FileStatus.CancelledBySender
                && status != AirXBridge.FileStatus.CancelledByReceiver;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
