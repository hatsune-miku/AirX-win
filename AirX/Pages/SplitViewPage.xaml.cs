// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using AirX.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AirX.Pages
{
    public sealed partial class SplitViewPage : Page
    {
        private GlobalViewModel SharedViewModel = GlobalViewModel.Instance;

        private ObservableCollection<NavLink> _navLinks = new ObservableCollection<NavLink>()
        {
            new NavLink() { Label = "Dashboard", Symbol = Symbol.Message },
            new NavLink() { Label = "Preferences", Symbol = Symbol.Globe },
            new NavLink() { Label = "Contacts", Symbol = Symbol.People },
            new NavLink() { Label = "Developer", Symbol = Symbol.Page },
        };

        public ObservableCollection<NavLink> NavLinks
        {
            get { return _navLinks; }
        }

        public SplitViewPage()
        {
            this.InitializeComponent();
            frame.Navigate(typeof(DashboardPage));
        }

        private void togglePaneButton_Click(object sender, RoutedEventArgs e)
        {
            if (splitView.IsPaneOpen)
            {
                splitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                splitView.IsPaneOpen = false;
            }
            else
            {
                splitView.IsPaneOpen = true;
                splitView.DisplayMode = SplitViewDisplayMode.Inline;
            }
        }


        private void NavLinksList_ItemClick(object sender, ItemClickEventArgs e)
        {
            
            switch ((e.ClickedItem as NavLink).Label) {
                case "Dashboard":
                    {
                        frame.Navigate(typeof(DashboardPage));
                        break;
                    }
                case "Preferences":
                    {
                        frame.Navigate(typeof(ConfigurationPage));
                        break;
                    }
                case "Contacts":
                    {
                        frame.Navigate(typeof(AboutPage));
                        break;
                    }
                case "Developer":
                    {
                        frame.Navigate(typeof(FigmaPage));
                        break;
                    }
            }
        }

        private void PanePlacement_Toggled(object sender, RoutedEventArgs e)
        {
            var ts = sender as ToggleSwitch;
            if (ts.IsOn)
            {
                splitView.PanePlacement = SplitViewPanePlacement.Right;
            }
            else
            {
                splitView.PanePlacement = SplitViewPanePlacement.Left;
            }
        }
        private void ToggleDayNightClicked(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)Content;
            if (element.RequestedTheme == ElementTheme.Default || element.RequestedTheme == ElementTheme.Light)
            {
                element.RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                element.RequestedTheme = ElementTheme.Light;
            }
        }

        private void displayModeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            splitView.DisplayMode = (SplitViewDisplayMode)Enum.Parse(typeof(SplitViewDisplayMode), (e.AddedItems[0] as ComboBoxItem).Content.ToString());
        }

        private void paneBackgroundCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var colorString = (e.AddedItems[0] as ComboBoxItem).Content.ToString();

            VisualStateManager.GoToState(this, colorString, false);
        }

    }

    public class NavLink
    {
        public string Label { get; set; }
        public Symbol Symbol { get; set; }
    }
}
