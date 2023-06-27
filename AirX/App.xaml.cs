using AirX.Bridge;
using AirX.Util;

namespace AirX
{
    public partial class App : Microsoft.UI.Xaml.Application
    {
        // The logical equivalent of main() or WinMain().
        public App()
        {
            this.InitializeComponent();
        }

        // Invoked when the application is launched.
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            SettingsUtil.TryInitializeConfigurationsForFirstRun();

            // Debug console
            if (SettingsUtil.Bool(Keys.ShouldShowConsole, false))
            {
                AirXBridge.RedirectAirXStdoutToDebugConsole();
            }

            AirXNative.airx_init();

            var window = new View.TrayIconHolderWindow();
            window.Activate();

            // TODO: Call Deinit when the app is closed.
        }

        public static Microsoft.UI.Xaml.Window LoginWindowShared;

    }
}
