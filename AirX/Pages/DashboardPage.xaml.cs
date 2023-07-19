using AirX.Bridge;
using AirX.ViewModel;
using Microsoft.UI.Xaml.Controls;

namespace AirX.Pages
{
    public sealed partial class DashboardPage : Page
    {
        private DashboardPageViewModel ViewModel = new DashboardPageViewModel();
        private GlobalViewModel GlobalViewModel = GlobalViewModel.Instance;

        public DashboardPage()
        {
            ViewModel.AirXVersion = AirXNative.airx_version().ToString();
            this.InitializeComponent();
        }
    }
}
