// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using AirX.Util;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using WinUIEx;

namespace AirX.View
{
    public sealed partial class NewTextWindow : BaseWindow
    {
        private const int WINDOW_WIDTH = 401;
        private const int WINDOW_HEIGHT = 210;

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
