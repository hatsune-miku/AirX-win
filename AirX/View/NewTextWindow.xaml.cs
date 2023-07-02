using AirX.Util;

namespace AirX.View
{
    public sealed partial class NewTextWindow : BaseWindow
    {
        private const int WINDOW_WIDTH = 640;
        private const int WINDOW_HEIGHT = 340;

        private string _title;
        private string _source;

        public static NewTextWindow Create(string title, string source)
        {
            var instance = new NewTextWindow();
            instance.UpdateInformation(title, source);
            return instance;
        }

        public void UpdateInformation(string title, string source)
        {
            _title = title;
            _source = source;
            newTextPage.UpdateInformation(title, source);
            newTextPage.SetWindowInstance(this);
        }

        private NewTextWindow()
        {
            this.InitializeComponent();

            PrepareWindow(
                new WindowParameters
                {
                    Title = "New Text Window",
                    WidthPortion = WINDOW_WIDTH / 3840.0 * 1.25,
                    HeightPortion = WINDOW_HEIGHT / 2160.0 * 1.25,
                    CenterScreen = false,
                    TopMost = true,
                    Resizable = false,
                    HaveMaximumButton = false,
                    HaveMinimumButton = false,
                    EnableMicaEffect = false,
                }
            );
            ExtendsContentIntoTitleBar = true;

            var screenSize = UIUtil.GetPrimaryScreenSize();
            AppWindow.Move(new Windows.Graphics.PointInt32(
                (int) screenSize.Width - AppWindow.Size.Width - 64,
                (int) screenSize.Height - AppWindow.Size.Height - 92
            ));
        }
    }
}
