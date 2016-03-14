using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Microsoft.Labs.SightsToSee.Common;
using Microsoft.Labs.SightsToSee.Models;
using Microsoft.Labs.SightsToSee.ViewModels;
using Microsoft.UI.Composition.Toolkit;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Microsoft.Labs.SightsToSee.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TripDetailPage : Page
    {
        public TripDetailPage()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                ViewModel.Map = Map;
            };
        }

        public TripDetailPageViewModel ViewModel => DataContext as TripDetailPageViewModel;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                var tripId = Guid.ParseExact(e.Parameter as string, "D");
                await ViewModel.LoadTripAsync(tripId);
            }
            base.OnNavigatedTo(e);
        }

        private void SightTemplateRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            var sight = senderElement.DataContext as Sight;
            var menuFlyout = FlyoutBase.GetAttachedFlyout(senderElement) as MenuFlyout;
            menuFlyout.Items.Clear();
            if (sight.IsMySight)
            {
                var menuItem = new MenuFlyoutItem
                {
                    Text = "Remove from My sights",
                };
                menuItem.Click += RemoveSight;
                menuFlyout.Items.Add(menuItem);
            }
            else
            {
                var menuItem = new MenuFlyoutItem
                {
                    Text = "Add to My sights",
                };
                menuItem.Click += AddSight;
                menuFlyout.Items.Add(menuItem);
            }
            menuFlyout.ShowAt(senderElement);
        }

        private void AddSight(object sender, RoutedEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            var sight = senderElement.DataContext as Sight;
            sight.IsMySight = true;
            ViewModel.UpdateSight(sight);
        }

        private void RemoveSight(object sender, RoutedEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            var sight = senderElement.DataContext as Sight;
            sight.IsMySight = false;
            ViewModel.UpdateSight(sight);
        }
    }
}