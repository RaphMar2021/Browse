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
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.WebUI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Browse
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowserPage : Page
    {
        HttpClient hc;
        public TabViewItem TargetTabItem { get; set; } = null;
        public TabViewer ParentTabViewer { get; set; } = null;

        public BrowserPage()
        {
            this.InitializeComponent();
            br.CoreWebView2Initialized += Br_CoreWebView2Initialized;
            hc = new();
        }

        private void ShowMenu(CommandBarFlyout c, FrameworkElement e, bool isTransient)
        {
            FlyoutShowOptions myOption = new FlyoutShowOptions();
            myOption.ShowMode = isTransient ? FlyoutShowMode.Transient : FlyoutShowMode.Standard;
            c.ShowAt(e, myOption);
        }

        private async void Br_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            await br.EnsureCoreWebView2Async();
            br.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
            br.CoreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;
            br.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
        }

        private async void CoreWebView2_ProcessFailed(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2ProcessFailedEventArgs args)
        {
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "We are sorry";
            dialog.CloseButtonText = "OK";
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.Content = $"An underlying WebView process crashed. We will restart WebView2 for you (Error {args.ProcessFailedKind})";

            dialog.ShowAsync();
        }

        private void CoreWebView2_WebMessageReceived(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
        {
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Alert";
            dialog.CloseButtonText = "OK";
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.Content = args.TryGetWebMessageAsString();

            dialog.ShowAsync();
        }

        async void CoreWebView2_DOMContentLoaded(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs args)
        {
            await br.EnsureCoreWebView2Async();

            // Download favicon
            try
            {
                string TempFile = Path.GetTempFileName();
                using (FileStream s = File.Create(TempFile))
                {
                    Stream s2 = await hc.GetStreamAsync(br.CoreWebView2.FaviconUri);
                    s2.CopyTo(s);
                    s2.Close();
                    s.Close();
                }
                Debug.WriteLine($"favicon: {TempFile}");
                TargetTabItem.IconSource = new BitmapIconSource() { UriSource = new(TempFile), ShowAsMonochrome = false };
            }
            catch { }

            TargetTabItem.Header = br.CoreWebView2.DocumentTitle;
            addressBar.Text = br.Source.AbsoluteUri;
            backButton.IsEnabled = br.CanGoBack;
            forwardButton.IsEnabled = br.CanGoForward;

            br.CoreWebView2.ExecuteScriptAsync("window.alert = function(message) { window.chrome.webview.postMessage(message); }");
        }

        private void AddressBar_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                br.Source = new(((TextBox)sender).Text);
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (br.CanGoBack)
                br.GoBack();
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (br.CanGoForward)
                br.GoForward();
        }

        private void downloadMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            br.Source = new("edge:\\\\downloads");
        }

        private async void devToolsMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Are you sure?";
            dialog.PrimaryButtonText = "Yes";
            dialog.CloseButtonText = "No";
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.Content = "Are you sure you want to open DevTools? It could lead to issues if you modify content shown inside! If someone tells you to paste something in the DevTools console, it's a scam!";

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
                br.CoreWebView2.OpenDevToolsWindow();
        }

        private async void flagsMenuItemFlyout_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Are you sure?";
            dialog.PrimaryButtonText = "Yes";
            dialog.CloseButtonText = "No";
            dialog.DefaultButton = ContentDialogButton.Close;
            dialog.Content = "Are you sure you want to open the browser flags? It could lead to browser bugs!";

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
                br.Source = new("edge:\\\\flags");
        }

        private void settingsMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            ParentTabViewer.AddNewSettingsTab();
        }
    }
}
