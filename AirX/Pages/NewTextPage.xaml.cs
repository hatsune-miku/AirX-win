using AirX.Util;
using AirX.View;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AirX.Pages
{
    public partial class NewTextPage : Page
    {
        private NewTextViewModel ViewModel = new();
        private NewTextWindow _instance;

        public NewTextPage()
        {
            this.InitializeComponent();
        }

        public void SetWindowInstance(NewTextWindow instance)
        {
            this._instance = instance;
        }

        public void UpdateInformation(string title, string source)
        {
            ViewModel.Title = title;
            ViewModel.From = source;
        }

        [RelayCommand]
        private void Block()
        {
            AccountUtil.AddToBlockList(ViewModel.From);
            _instance.Close();
        }

        [RelayCommand]
        private void Cancel()
        {

        }

        private void OnBlockClicked(object sender, RoutedEventArgs e)
        {
            _ = new ContentDialog()
            {
                Title = "Blocking " + ViewModel.From,
                Content = "Are you sure to block " + ViewModel.From + "?",
                PrimaryButtonText = "Block",
                SecondaryButtonText = "Cancel",
                PrimaryButtonCommand = BlockCommand,
                SecondaryButtonCommand = CancelCommand,
                XamlRoot = Content.XamlRoot,
            }.ShowAsync();
        }
    }
}
