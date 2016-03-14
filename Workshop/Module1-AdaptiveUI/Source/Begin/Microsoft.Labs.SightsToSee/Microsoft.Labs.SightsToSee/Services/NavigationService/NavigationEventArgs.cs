using System;
using Windows.UI.Xaml.Navigation;

namespace Microsoft.Labs.SightsToSee.Services.NavigationService
{
    public class NavigationEventArgs : EventArgs
    {
        public NavigationMode NavigationMode { get; set; }

        public string Parameter { get; set; }
    }
}