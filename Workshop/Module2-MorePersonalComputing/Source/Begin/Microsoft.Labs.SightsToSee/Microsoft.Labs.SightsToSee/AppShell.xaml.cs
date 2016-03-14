using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Labs.SightsToSee.Common;
using Microsoft.Labs.SightsToSee.Controls;
using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Labs.SightsToSee.Library.Services.DataModelService;
using Microsoft.Labs.SightsToSee.Views;
using Microsoft.Labs.SightsToSee.Models;
using Windows.UI.Popups;

namespace Microsoft.Labs.SightsToSee
{
    /// <summary>
    ///     The "chrome" layer of the app that provides top-level navigation with
    ///     proper keyboarding navigation.
    /// </summary>
    public sealed partial class AppShell : Page
    {
        public static AppShell Current;
        // Declare the top level nav items

        /// <summary>
        ///     Initializes a new instance of the AppShell, sets the static 'Current' reference,
        ///     adds callbacks for Back requests and changes in the SplitView's DisplayMode, and
        ///     provide the nav menu list with the data to display.
        /// </summary>
        public AppShell()
        {
            InitializeComponent();

            Loaded += async (sender, args) =>
            {
                Current = this;

                //TogglePaneButton.Focus(FocusState.Programmatic);
                NavMenuList.SelectedIndex = 0;
                var dm = DataModelServiceFactory.CurrentDataModelService();

#if SQLITE
                var trips = await dm.LoadTripsAsync();

                if (AppSettings.HasRun && trips.Any())
                {
                    // Load trips from DB
                    foreach (var trip in trips)
                    {
                        AddTrip(trip.Name, trip.Id);
                    }
                    var parameter = new TripNavigationParameter {TripId = trips.First().Id}.GetJson();
                    NavigateToPage(typeof (TripDetailPage), parameter);
                    LandingPage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    LandingPage.ShowCreateFirstTrip = true;
                }
#else
                // When connecting to Azure, this is where we authenticate and then load the seed data into the local tables.
                // Then sync to the cloud
                bool isAuthenticated = false;
                while (!isAuthenticated)
                {
                    var authResponse = await dm.AuthenticateAsync();

                    isAuthenticated = authResponse.Item1;
                    if (!isAuthenticated)
                    {
                        var dialog = new MessageDialog(authResponse.Item2);
                        dialog.Commands.Add(new UICommand("OK"));
                        await dialog.ShowAsync();
                    }
                }

                await SetBusyAsync("Synchronising");
                var trips = await dm.LoadTripsAsync();
                await ClearBusyAsync();

                if (trips.Any())
                {
                    AppSettings.HasRun = true;

                    // Load trips from DB
                    foreach (var trip in trips)
                    {
                        AddTrip(trip.Name, trip.Id);
                    }
                    var parameter = new TripNavigationParameter {TripId = trips.First().Id}.GetJson();
                    NavigateToPage(typeof (TripDetailPage), parameter);
                    LandingPage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    LandingPage.ShowCreateFirstTrip = true;
                }
#endif
            };

                RootSplitView.RegisterPropertyChangedCallback(
                SplitView.DisplayModeProperty,
                (s, a) =>
                {
                    // Ensure that we update the reported size of the TogglePaneButton when the SplitView's
                    // DisplayMode changes.
                    CheckTogglePaneButtonSizeChanged();
                });

            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;
            PopupManager.Configure(this, ParentedPopup, HitBlocker);
        }

        public Popup PopupHost
        {
            get { return ParentedPopup; }
        }

        public Frame AppFrame
        {
            get { return frame; }
        }

        public Rect TogglePaneButtonRect { get; private set; }

        public ObservableCollection<NavMenuItem> NavList { get; set; } = new ObservableCollection<NavMenuItem>(
            new[]
            {
                //new NavMenuItem
                //{
                //    SymbolAsChar = (char) Symbol.Home,
                //    Label = "Home",
                //    DestPage = typeof (LandingPage)
                //},
                new NavMenuItem
                {
                    SymbolAsChar = (char) Symbol.Add,
                    Label = "New Trip",
                    DestPage = typeof (NewTripPage)
                }
            });

        public void AddTrip(string tripName, Guid tripId)
        {
            NavList.Add(new NavMenuItem
            {
                SymbolAsChar = '\uE709',
                Label = tripName,
                DestPage = typeof (TripDetailPage),
                Arguments = new TripNavigationParameter { TripId = tripId }.GetJson()
            });
        }


        /// <summary>
        ///     Default keyboard focus movement for any unhandled keyboarding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppShell_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var direction = FocusNavigationDirection.None;
            switch (e.Key)
            {
                case VirtualKey.Left:
                case VirtualKey.GamepadDPadLeft:
                case VirtualKey.GamepadLeftThumbstickLeft:
                case VirtualKey.NavigationLeft:
                    direction = FocusNavigationDirection.Left;
                    break;
                case VirtualKey.Right:
                case VirtualKey.GamepadDPadRight:
                case VirtualKey.GamepadLeftThumbstickRight:
                case VirtualKey.NavigationRight:
                    direction = FocusNavigationDirection.Right;
                    break;

                case VirtualKey.Up:
                case VirtualKey.GamepadDPadUp:
                case VirtualKey.GamepadLeftThumbstickUp:
                case VirtualKey.NavigationUp:
                    direction = FocusNavigationDirection.Up;
                    break;

                case VirtualKey.Down:
                case VirtualKey.GamepadDPadDown:
                case VirtualKey.GamepadLeftThumbstickDown:
                case VirtualKey.NavigationDown:
                    direction = FocusNavigationDirection.Down;
                    break;
            }

            if (direction != FocusNavigationDirection.None)
            {
                var control = FocusManager.FindNextFocusableElement(direction) as Control;
                if (control != null)
                {
                    control.Focus(FocusState.Programmatic);
                    e.Handled = true;
                }
            }
        }

#region BackRequested Handlers

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (PopupManager.Popup.IsOpen)
            {
                PopupManager.Popup.IsOpen = false;
                e.Handled = true;
            }

            if (!e.Handled && AppFrame.CanGoBack)
            {
                e.Handled = true;
                AppFrame.GoBack();
            }
        }

#endregion

        /// <summary>
        ///     An event to notify listeners when the hamburger button may occlude other content in the app.
        ///     The custom "PageHeader" user control is using this.
        /// </summary>
        public event TypedEventHandler<AppShell, Rect> TogglePaneButtonRectChanged;

        /// <summary>
        ///     Callback when the SplitView's Pane is toggled open or close.  When the Pane is not visible
        ///     then the floating hamburger may be occluding other content in the app unless it is aware.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TogglePaneButton_Checked(object sender, RoutedEventArgs e)
        {
            CheckTogglePaneButtonSizeChanged();
        }

        /// <summary>
        ///     Check for the conditions where the navigation pane does not occupy the space under the floating
        ///     hamburger button and trigger the event.
        /// </summary>
        private void CheckTogglePaneButtonSizeChanged()
        {
            if (RootSplitView.DisplayMode == SplitViewDisplayMode.Inline ||
                RootSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                var transform = TogglePaneButton.TransformToVisual(this);
                var rect =
                    transform.TransformBounds(new Rect(0, 0, TogglePaneButton.ActualWidth, TogglePaneButton.ActualHeight));
                TogglePaneButtonRect = rect;
            }
            else
            {
                TogglePaneButtonRect = new Rect();
            }

            var handler = TogglePaneButtonRectChanged;
            if (handler != null)
            {
                // handler(this, this.TogglePaneButtonRect);
                handler.DynamicInvoke(this, TogglePaneButtonRect);
            }
        }

        /// <summary>
        ///     Enable accessibility on each nav menu item by setting the AutomationProperties.Name on each container
        ///     using the associated Label of each item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavMenuItemContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue && args.Item != null && args.Item is NavMenuItem)
            {
                args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((NavMenuItem) args.Item).Label);
            }
            else
            {
                args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
            }
        }

#region Navigation

        public void GoToSettings()
        {
            AppFrame.Navigate(typeof(SettingsPage));
        }

        /// <summary>
        ///     Navigate to the Page for the selected <paramref name="listViewItem" />.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="listViewItem"></param>
        private void NavMenuList_ItemInvoked(object sender, ListViewItem listViewItem)
        {
            var item = (NavMenuItem) ((NavMenuListView) sender).ItemFromContainer(listViewItem);

            if (item != null)
            {
                if (item.DestPage != null &&
                    item.DestPage != AppFrame.CurrentSourcePageType)
                {
                    AppFrame.Navigate(item.DestPage, item.Arguments);
                }
            }
        }

        /// <summary>
        ///     Ensures the nav menu reflects reality when navigation is triggered outside of
        ///     the nav menu buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                var item = (from p in NavList where p.DestPage == e.SourcePageType select p).SingleOrDefault();
                if (item == null && AppFrame.BackStackDepth > 0)
                {
                    // In cases where a page drills into sub-pages then we'll highlight the most recent
                    // navigation menu item that appears in the BackStack
                    foreach (var entry in AppFrame.BackStack.Reverse())
                    {
                        item = (from p in NavList where p.DestPage == entry.SourcePageType select p).SingleOrDefault();
                        if (item != null)
                            break;
                    }
                }

                var container = (ListViewItem) NavMenuList.ContainerFromItem(item);

                // While updating the selection state of the item prevent it from taking keyboard focus.  If a
                // user is invoking the back button via the keyboard causing the selected nav menu item to change
                // then focus will remain on the back button.
                if (container != null) container.IsTabStop = false;
                NavMenuList.SetSelectedItem(container);
                if (container != null) container.IsTabStop = true;
            }
        }

        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            // After a successful navigation set keyboard focus to the loaded page
            if (e.Content is Page && e.Content != null)
            {
                var control = (Page) e.Content;
                control.Loaded += Page_Loaded;
            }

            // Update the Back button depending on whether we can go Back.
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppFrame.CanGoBack
                    ? AppViewBackButtonVisibility.Visible
                    : AppViewBackButtonVisibility.Collapsed;
        }

        public void NavigateToPage(Type page, object parameter = null)
        {
            if (LandingPage != null && LandingPage.Visibility == Visibility.Visible)
            {
                LandingPage.Visibility = Visibility.Collapsed;
            }
            var navItem = NavList.FirstOrDefault(n => n.DestPage == page);
            if (navItem != null)
            {
                NavMenuList.SelectedIndex = NavList.IndexOf(navItem);
                AppFrame.Navigate(navItem.DestPage, parameter);
            }
            else
            {
                AppFrame.Navigate(page, parameter);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Page) sender).Focus(FocusState.Programmatic);
            ((Page) sender).Loaded -= Page_Loaded;
            CheckTogglePaneButtonSizeChanged();
        }

        public async Task SetBusyAsync(string text = "Loading...")
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                BusyIndicator.IsActive = true;
                BusyText.Text = text;
                BusyGrid.Visibility = Visibility.Visible;
            });
        }

        public async Task ClearBusyAsync()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                BusyIndicator.IsActive = false;
                BusyText.Text = string.Empty;
                BusyGrid.Visibility = Visibility.Collapsed;
            });
        }

#endregion
    }
}