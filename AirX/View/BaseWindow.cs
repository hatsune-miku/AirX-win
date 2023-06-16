using AirX.Helper;
using AirX.Util;
using Lombok.NET;
using Microsoft.Graphics.Display;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using WinRT;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Composition;

namespace AirX.View
{
    public abstract class BaseWindow : Window
    {
        protected readonly AppWindow AppWindow;
        protected readonly OverlappedPresenter Presenter;
        protected readonly SystemBackdropConfiguration backdropConfiguration;
        protected readonly MicaController micaController;

        protected BaseWindow()
        {
            AppWindow = UIUtil.GetAppWindow(this);
            Presenter = (OverlappedPresenter) AppWindow.Presenter;

            backdropConfiguration = new SystemBackdropConfiguration
            {
                IsInputActive = true
            };

            micaController = new MicaController
            {
                Kind = MicaKind.Base,
            };
        }

        protected void PrepareWindow(PrepareWindowParameters parameters)
        {
            AppWindow.Resize(new SizeInt32(parameters.Width, parameters.Height));
            if (parameters.CenterScreen)
            {
                UIUtil.MoveWindowToCenterScreen(AppWindow);
            }

            AppWindow.Title = parameters.Title;
            Presenter.IsResizable = parameters.Resizable;
            Presenter.IsMaximizable = parameters.HaveMaximumButton;
            Presenter.IsMinimizable = parameters.HaveMinimumButton;
            Presenter.IsAlwaysOnTop = parameters.TopMost;

            if (parameters.EnableMicaEffect)
            {
                TrySetMicaBackdrop();
            }
        }


        bool TrySetMicaBackdrop()
        {
            if (!MicaController.IsSupported())
            {
                return false;
            }

            ((FrameworkElement)Content).RequestedTheme = ElementTheme.Default;
            ExtendsContentIntoTitleBar = true;

            new WindowsSystemDispatcherQueueHelper()
                .EnsureWindowsSystemDispatcherQueueController();

            // Hooking up the policy object.
            this.Activated += OnWindowActivated;
            this.Closed += OnWindowClosed;
            ((FrameworkElement)Content).ActualThemeChanged += OnWindowThemeChanged;

            // Initial configuration state.
            UpdateConfigurationSourceTheme();

            // Enable the system backdrop.
            micaController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
            micaController.SetSystemBackdropConfiguration(backdropConfiguration);
            return true;
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
        {
            backdropConfiguration.IsInputActive =
                args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void OnWindowClosed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
            // use this closed window.
            if (micaController != null)
            {
                micaController.Dispose();
            }
            this.Activated -= OnWindowActivated;
        }

        private void OnWindowThemeChanged(FrameworkElement sender, object args)
        {
            if (backdropConfiguration != null)
            {
                UpdateConfigurationSourceTheme();
            }
        }

        private void UpdateConfigurationSourceTheme()
        {
            switch (((FrameworkElement)Content).ActualTheme)
            {
                case ElementTheme.Dark:
                    backdropConfiguration.Theme = SystemBackdropTheme.Dark;
                    break;

                case ElementTheme.Light:
                    backdropConfiguration.Theme = SystemBackdropTheme.Light;
                    break;

                case ElementTheme.Default:
                    backdropConfiguration.Theme = SystemBackdropTheme.Default;
                    break;
            }
        }
    }

    public partial class PrepareWindowParameters
    {
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool CenterScreen { get; set; } = true;
        public bool Resizable { get; set; } = true;
        public bool HaveMaximumButton { get; set; } = false;
        public bool HaveMinimumButton { get; set; } = true;
        public bool TopMost { get; set; } = false;
        public bool EnableMicaEffect { get; set; } = false;
    }
}
