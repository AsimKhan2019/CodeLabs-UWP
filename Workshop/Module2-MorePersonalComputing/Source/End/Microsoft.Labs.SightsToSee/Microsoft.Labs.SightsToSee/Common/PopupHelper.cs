using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Shapes;

namespace Microsoft.Labs.SightsToSee.Common
{
    public static class PopupManager
    {
        public static Page Host { get; private set; }
        public static Rectangle HitBlocker { get; private set; }
        public static Popup Popup { get; private set; }
        public static UserControl Content { get; private set; }

        public static void Configure(Page host, Popup popup, Rectangle hitBlocker)
        {
            if (Host != null)
            {
                Host.SizeChanged -= Host_SizeChanged;
            }
            if (Popup != null)
            {
                Popup.Opened -= Popup_OnOpened;
                Popup.Closed -= Popup_OnClosed;
            }
            Host = host;
            Popup = popup;
            HitBlocker = hitBlocker;

            Host.SizeChanged += Host_SizeChanged;
            Popup.Opened += Popup_OnOpened;
            Popup.Closed += Popup_OnClosed;
        }

        private static void Host_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizePopup();
        }

        private static void ResizePopup()
        {
            if (Content == null) return;

            if (Host.ActualWidth < 720)
            {
                Popup.HorizontalOffset = 0;
                Popup.VerticalOffset = 0;

                Content.Height = Host.ActualHeight;
                Content.Width = Host.ActualWidth;
            }
            else
            {
                var horizontalOffset = (Host.ActualWidth - Content.ActualWidth)/2;
                var verticalOffset = (Host.ActualHeight - Content.ActualHeight)/2;
                Popup.HorizontalOffset = horizontalOffset < 0 ? 0 : horizontalOffset;
                Popup.VerticalOffset = verticalOffset < 0 ? 0 : verticalOffset;

                Content.Height = double.NaN;
                Content.Width = double.NaN;

                Content.MaxHeight = Host.ActualHeight;
                Content.MaxWidth = Host.ActualWidth;
            }
        }

        private static void Popup_OnOpened(object sender, object e)
        {
            HitBlocker.Visibility = Visibility.Visible;
        }

        private static void Popup_OnClosed(object sender, object e)
        {
            HitBlocker.Visibility = Visibility.Collapsed;
        }

        public static void ShowContent(UserControl content)
        {
            Content = content;
            if (Popup != null && !Popup.IsOpen)
            {
                Popup.Child = null;
                Popup.Child = Content;
                Popup.IsOpen = true;
                ResizePopup();
            }
        }

        public static void HideContent()
        {
            if (Popup != null && Popup.IsOpen)
            {
                Content = null;
                Popup.IsOpen = false;
            }
        }
    }
}