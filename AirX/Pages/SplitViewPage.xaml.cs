﻿using System;
using System.Collections.ObjectModel;
using AirX.View;
using AirX.ViewModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace AirX.Pages
{
    public class NavLink
    {
        public string Label { get; set; }
        public Symbol Symbol { get; set; }
        public Type ViewType { get; set; }
    }

    public sealed partial class SplitViewPage : Page
    {
        private GlobalViewModel SharedViewModel = GlobalViewModel.Instance;
        private SplitPageViewModel ViewModel = new SplitPageViewModel();
        private ObservableCollection<NavLink> _navLinks = new ObservableCollection<NavLink>()
        {
            new() {
                Label = "Dashboard",
                Symbol = Symbol.Message,
                ViewType = typeof(DashboardPage),
            },
            new() {
                Label = "Preferences",
                Symbol = Symbol.Globe,
                ViewType = typeof(ConfigurationPage),
            },
            new() {
                Label = "Sent Files",
                Symbol = Symbol.Send,
                ViewType = typeof(SentFilesPage),
            },
            new() {
                Label = "Received Files",
                Symbol = Symbol.Download,
                ViewType = typeof(ReceivedFilesPage),
            },
            new() {
                Label = "Contacts",
                Symbol = Symbol.People,
                ViewType = typeof(AboutPage),
            },
        };

        public ObservableCollection<NavLink> NavLinks
        {
            get { return _navLinks; }
        }

        public SplitViewPage()
        {
            this.InitializeComponent();
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // 页面加载完毕时，率先加载DashboardPage
            frame.Navigate(typeof(DashboardPage));
            ControlPanelWindow.Instance.SizeChanged += OnWindowSizeChanged;
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            ControlPanelWindow.Instance.SizeChanged -= OnWindowSizeChanged;
        }
         
        // 用于控制SplitView的展开与收起
        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            ViewModel.ShouldExpandPane = args.Size.Width > 850;
        }

        private void OnNavLinksListItemClicked(object sender, ItemClickEventArgs e)
        {
            // 相同页面不触发多余加载
            if (_navLinks[NavLinksList.SelectedIndex] == e.ClickedItem)
            {
                return;
            }
            frame.Navigate((e.ClickedItem as NavLink).ViewType);
        }

        private void ToggleDayNightClicked(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)Content;
            if (element.RequestedTheme != ElementTheme.Dark)
            {
                element.RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                element.RequestedTheme = ElementTheme.Light;
            }
        }
    }
}
