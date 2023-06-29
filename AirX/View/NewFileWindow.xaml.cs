using WinUIEx;

namespace AirX.View
{
    public sealed partial class NewFileWindow : BaseWindow
    {
        private const int WINDOW_WIDTH = 401;
        private const int WINDOW_HEIGHT = 210;

        private int _fileId;

        public NewFileWindow(int fileId)
        {
            this.InitializeComponent();

            _fileId = fileId;
            newFilePage.SetWindowInstance(this);
            newFilePage.FileId = fileId;

            PrepareWindow(
                new WindowParameters
                {
                    Title = "New File Window",
                    WidthPortion = WINDOW_WIDTH / 3840.0 * 1.75,
                    HeightPortion = WINDOW_HEIGHT / 2160.0 * 1.75,
                    CenterScreen = false,
                    TopMost = true,
                    Resizable = false,
                    HaveMaximumButton = false,
                    HaveMinimumButton = false,
                    EnableMicaEffect = true,
                }
            );

            SetTitleBar(titleBar);
            this.CenterOnScreen();
        }
    }
}
