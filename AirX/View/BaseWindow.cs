using AirX.Helper;
using AirX.Util;
using Lombok.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using WinRT;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Composition;
using System.Drawing;
using Windows.Graphics.Display;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;

namespace AirX.View
{
    /// <summary>
    /// 所有Window的基类，提供了一些方便的功能。
    /// </summary>
    public abstract class BaseWindow : Window
    {
        protected readonly AppWindow CurrentAppWindow;
        protected readonly OverlappedPresenter Presenter;
        protected readonly SystemBackdropConfiguration backdropConfiguration;
        protected readonly MicaController micaController;

        protected BaseWindow()
        {
            CurrentAppWindow = UIUtil.GetAppWindow(this);
            Presenter = (OverlappedPresenter) CurrentAppWindow.Presenter;

            /// 为每个窗口做好接入Mica特效的准备工作（但是不一定接入）
            backdropConfiguration = new SystemBackdropConfiguration
            {
                IsInputActive = true
            };

            micaController = new MicaController
            {
                Kind = MicaKind.Base,
            };
        }

        /// <summary>
        /// 改变窗口尺寸这么基础的需求，居然需要这种旁门左道才能实现了
        /// </summary>
        protected void Resize(int width, int height)
        {
            AppWindow.Resize(new (width, height));
        }

        /// <summary>
        /// 准备一个窗口，设置好窗口的各种属性，属性列表参见 `WindowParameters`
        /// </summary>
        protected void PrepareWindow(WindowParameters parameters)
        {
            var screenSize = UIUtil.GetPrimaryScreenSize();
            var realWidth = (int)(parameters.WidthPortion * screenSize.Width);
            var realHeight = (int)(parameters.HeightPortion * screenSize.Height);
            Resize(realWidth, realHeight);

            if (parameters.CenterScreen)
            {
                UIUtil.MoveWindowToCenterScreen(CurrentAppWindow);
            }

            CurrentAppWindow.Title = parameters.Title;
            Presenter.IsResizable = parameters.Resizable;
            Presenter.IsMaximizable = parameters.HaveMaximumButton;
            Presenter.IsMinimizable = parameters.HaveMinimumButton;
            Presenter.IsAlwaysOnTop = parameters.TopMost;
            Presenter.SetBorderAndTitleBar(parameters.HaveBorder, parameters.HaveTitleBar);

            /// 注意：一旦启用Mica，那么SetBorderAndTitleBar失效！
            if (parameters.EnableMicaEffect)
            {
                TrySetMicaBackdrop();
            }
        }

        /// <summary>
        /// 微软给的代码，用于启用Mica特效给某个特定窗口
        /// </summary>
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

        /// <summary>
        /// 微软给的
        /// </summary>
        private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
        {
            backdropConfiguration.IsInputActive =
                args.WindowActivationState != WindowActivationState.Deactivated;
        }

        /// <summary>
        /// 微软给的
        /// </summary>
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

        /// <summary>
        /// 微软给的
        /// </summary>
        private void OnWindowThemeChanged(FrameworkElement sender, object args)
        {
            if (backdropConfiguration != null)
            {
                UpdateConfigurationSourceTheme();
            }
        }

        /// <summary>
        /// 微软给的
        /// </summary>
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

    /// <summary>
    /// 列举了BaseWindow能够操纵哪些窗口属性
    /// </summary>
    public class WindowParameters
    {
        public string Title { get; set; }
        public double WidthPortion { get; set; }
        public double HeightPortion { get; set; }
        public bool CenterScreen { get; set; } = true;
        public bool Resizable { get; set; } = true;
        public bool HaveMaximumButton { get; set; } = false;
        public bool HaveMinimumButton { get; set; } = true;
        public bool TopMost { get; set; } = false;
        public bool EnableMicaEffect { get; set; } = false;
        public bool HaveBorder { get; set; } = true;
        public bool HaveTitleBar { get; set; } = true;
    }
}
