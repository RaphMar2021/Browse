using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Browse
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TabViewer : Window
    {
        public ApplicationDataContainer localSettings;

        public TabViewer()
        {
            this.InitializeComponent();

            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Activated += TabViewWindowingSamplePage_Loaded;
            Closed += (s, e) => { Environment.Exit(0); };
            Tabs.SelectionChanged += Tabs_SelectionChanged;
            AddNewBrowserTab();
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("tabs: Selection changed!");
        }

        public void AddNewSettingsTab()
        {
            TabViewItem t = new()
            {
                Header = "Settings",
                IconSource = new FontIconSource() { Glyph = "\uE713" },
                Content = new SettingsPage() { ParentTabViewer = this }
            };

            Tabs.TabItems.Add(t);
        }

        public void AddNewBrowserTab()
        {
            TabViewItem t = new()
            {
                Header = "New Tab",
                Content = new BrowserPage() { ParentTabViewer = this }
            };

            ((BrowserPage)t.Content).TargetTabItem = t;

            Tabs.TabItems.Add(t);
        }

        private void TabViewWindowingSamplePage_Loaded(object sender, WindowActivatedEventArgs e)
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(CustomDragRegion);
            CustomDragRegion.MinWidth = 188;
        }

        private void Tabs_AddTabButtonClick(TabView sender, object args)
        {
            AddNewBrowserTab();
        }

        private void Tabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            Tabs.TabItems.Remove(args.Item);
        }
    }
}
