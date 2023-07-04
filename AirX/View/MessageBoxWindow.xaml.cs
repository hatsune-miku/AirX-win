using AirX.Extension;
using AirX.Util;
using AirX.ViewModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Drawing;
using System.Threading.Tasks;
using WinUIEx;

namespace AirX.View
{
    public sealed partial class MessageBoxWindow : Window
    {
        private MessageBoxViewModel ViewModel = new();
        private string _title;
        private string _message;
        private string _primaryButtonTitle;
        private string _secondaryButtonTitle;
        private ContentDialog _dialog;

        public MessageBoxWindow(string title, string message, string buttonTitle, string secondaryButtonTitle)
        {
            this.InitializeComponent();

            _title = title;
            _message = message;
            _primaryButtonTitle = buttonTitle;
            _secondaryButtonTitle = secondaryButtonTitle;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(titleBar);

            var manager = WindowManager.Get(this);
            manager.IsMaximizable = false;
            manager.IsMinimizable = false;
            manager.IsAlwaysOnTop = true;
            manager.IsTitleBarVisible = false;

            var presenter = AppWindow.Presenter as OverlappedPresenter;
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsMaximizable = false;
            presenter.IsAlwaysOnTop = true;
            presenter.IsResizable = false;
            Title = "ContentDialog Modal Wrapping Window";

            WaitUntilXamlRootReadyAndInitDialogAsync()
                .FireAndForget();
            UIUtil.SetWindowVisibility(this, false);
        }

        private async Task WaitUntilXamlRootReadyAndInitDialogAsync()
        {
            while (Content.XamlRoot == null)
            {
                await Task.Delay(100);
            }

            _dialog = new ContentDialog
            {
                Title = _title,
                Content = _message,
                PrimaryButtonText = _primaryButtonTitle,
                SecondaryButtonText = _secondaryButtonTitle,
                XamlRoot = Content.XamlRoot,
            };
        }

        private async Task WaitUntilDialogReadyAsync()
        {
            while (_dialog == null)
            {
               await Task.Delay(100);
            }
        }

        public async Task<ContentDialogResult> ShowAsync()
        {
            Activate();
            await WaitUntilDialogReadyAsync();
            var screenSize = UIUtil.GetPrimaryScreenSize();
            _dialog.Opened += (_, _) =>
            {
                // TODO: accurate resize
                AppWindow.Resize(new(
                    (int)((_dialog.ActualWidth) / 2.96),
                    (int)((_dialog.ActualHeight) / 2.66)
                ));
                this.CenterOnScreen();
                UIUtil.SetWindowVisibility(this, true);
            };
            var result = await _dialog.ShowAsync();
            Close();
            return result;
        }
    }
}
