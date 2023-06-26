using AirX.Extension;
using AirX.Util;
using AirX.ViewModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Drawing;
using System.Threading.Tasks;

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

            var presenter = AppWindow.Presenter as OverlappedPresenter;
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsMaximizable = false;
            presenter.IsAlwaysOnTop = true;
            presenter.IsResizable = false;
            Title = "ContentDialog Modal Wrapping Window";

            WaitUntilXamlRootReadyAndInitDialogAsync()
                .LogOnError();
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
            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
            _dialog.Loaded += (_, _) =>
            {
                AppWindow.Resize(new(
                    (int)((_dialog.ActualWidth - 64) / (graphics.DpiX / 100.0)),
                    (int)((_dialog.ActualHeight - 96) / (graphics.DpiY / 100.0))
                ));
                AppWindow.Move(new(
                    (int) (screenSize.Width / 2 - AppWindow.Size.Width / 2),
                    (int) (screenSize.Height / 2 - AppWindow.Size.Height / 2)
                ));
                UIUtil.SetWindowVisibility(this, true);
            };
            var result = await _dialog.ShowAsync();
            Close();
            return result;
        }
    }
}
