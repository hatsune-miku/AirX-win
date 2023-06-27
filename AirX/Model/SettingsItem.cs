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

    public partial class SettingsItem : ObservableObject
    {
        public delegate bool ValidatorFunction(string valueStringRepresentation);

        [ObservableProperty]
        string title;

        [ObservableProperty]
        string description;

        [ObservableProperty]
        bool isAdvanced;

        [ObservableProperty]
        Keys settingsKey;

        [ObservableProperty]
        ValidatorFunction validator;

        [ObservableProperty]
        SettingsItemType itemType;

        public ConfigurationViewModel ViewModel { get; set; }
        public XamlRoot XamlRoot { get; set; }

        string stringRepresentation = null;

        public string ReadAsString()
        {
            if (stringRepresentation == null)
            {
                stringRepresentation = SettingsUtil.String(SettingsKey, "");
            }
            return stringRepresentation;
        }

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

        public void SetAsString(string newValue)
        {
            stringRepresentation = newValue;
        }

        public void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.IsUnsaved = true;
        }

        public void SetAsBoolean(bool newValue)
        {
            stringRepresentation = newValue.ToString();
            SettingsUtil.Write(SettingsKey, stringRepresentation.ToLower());
        }

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
