using AirX.Util;
using AirX.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AirX.Model
{
    public enum SettingsItemType
    {
        Boolean,
        String,
    }

    /// 设置管理
    public partial class SettingsItem : ObservableObject
    {
        /// 验证器函数！这也是一个代理函数，需要验证的时候通知你，然后你就验证一下，返回true或者false
        /// 验证标准则是由你自己定义的
        /// 用于验证用户输入的值是否合法
        /// 比如如果你定义了一个端口的验证器，则你应该检查一个字符串是否是一个合法的端口号
        public delegate bool ValidatorFunction(string valueStringRepresentation);

        /// <summary>
        /// 设置项的标题
        /// </summary>
        [ObservableProperty]
        string title;

        /// <summary>
        /// 设置项的描述
        /// </summary>
        [ObservableProperty]
        string description;

        /// <summary>
        /// 是否属于高级设置
        /// </summary>
        [ObservableProperty]
        bool isAdvanced;

        /// <summary>
        /// 设置项在SettingsUtil中的键
        /// </summary>
        [ObservableProperty]
        Keys settingsKey;

        /// <summary>
        /// 这项设置的验证器
        /// </summary>
        [ObservableProperty]
        ValidatorFunction validator;

        /// <summary>
        /// 这项设置的类型，开关？还是字符串？
        /// </summary>
        [ObservableProperty]
        SettingsItemType itemType;

        /// <summary>
        /// 每项设置持有对设置页面的ViewModel的引用
        /// </summary>
        public ConfigurationViewModel ViewModel { get; set; }

        /// <summary>
        /// 每项设置持有对设置页面的XamlRoot的引用
        /// </summary>
        public XamlRoot XamlRoot { get; set; }

        /// <summary>
        /// 设置的值的字符串形式
        /// 为什么统一用字符串形式？因为这样可以统一验证器的参数类型为string
        /// </summary>
        string stringRepresentation = null;

        /// <summary>
        /// 把这项设置读成字符串
        /// </summary>
        public string ReadAsString()
        {
            if (stringRepresentation == null)
            {
                stringRepresentation = SettingsUtil.String(SettingsKey, "");
            }
            return stringRepresentation;
        }

        /// <summary>
        /// 把这项设置读成布尔值
        /// </summary>
        /// <returns></returns>
        public bool ReadAsBoolean()
        {
            if (stringRepresentation == null)
            {
                stringRepresentation = SettingsUtil.String(SettingsKey, "false")
                    .ToLower();
            }
            if (bool.TryParse(stringRepresentation, out bool result))
            {
                return result;
            }
            return false;
        }

        /// <summary>
        /// 设置这项设置的值，以字符串形式
        /// </summary>
        public void SetAsString(string newValue)
        {
            stringRepresentation = newValue;
        }

        /// <summary>
        /// 如果这项设置是字符串类型的，那么这个函数会在用户输入的时候被调用
        /// </summary>
        public void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.IsUnsaved = true;
        }

        /// <summary>
        /// 设置这项设置的值，以布尔值形式
        /// </summary>
        public void SetAsBoolean(bool newValue)
        {
            stringRepresentation = newValue.ToString();
            SettingsUtil.Write(SettingsKey, stringRepresentation.ToLower());
        }

        /// <summary>
        /// 如果这项设置是布尔类型的，那么这个函数会在用户点击开关的时候被调用
        /// </summary>
        public void OnButtonValueSaved(object sender, RoutedEventArgs e)
        {
            if (Validator != null && !Validator(stringRepresentation))
            {
                if (XamlRoot != null)
                {
                    UIUtil.ShowContentDialog("Error", "You have entered an invalid value.", XamlRoot);
                }
                return;
            }
            ViewModel.IsUnsaved = false;
            SettingsUtil.Write(SettingsKey, stringRepresentation);
        }
    }
}
