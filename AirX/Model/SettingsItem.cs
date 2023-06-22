using AirX.Util;
using AirX.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace AirX.Model
{
    public partial class SettingsItem : ObservableObject
    {
        public delegate bool ValidatorFunction(string valueStringRepresentation);

        [ObservableProperty]
        string title;

        [ObservableProperty]
        string description;

        [ObservableProperty]
        Keys settingsKey;

        [ObservableProperty]
        ValidatorFunction validator;

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

        public void SetAsString(string newValue)
        {
            stringRepresentation = newValue;
            ViewModel.IsUnsaved = true;
        }

        public void OnValueSaved(object sender, RoutedEventArgs e)
        {
            if (!Validator(stringRepresentation))
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
