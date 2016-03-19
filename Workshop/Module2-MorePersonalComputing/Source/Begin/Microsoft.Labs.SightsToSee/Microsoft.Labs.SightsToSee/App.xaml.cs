using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.ApplicationInsights;
using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Labs.SightsToSee.Library.Services.DataModelService;
using Microsoft.Labs.SightsToSee.Views;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.Labs.SightsToSee.Models;

namespace Microsoft.Labs.SightsToSee
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private readonly Task _seedDataTask;

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            WindowsAppInitializer.InitializeAsync(
                WindowsCollectors.Metadata |
                WindowsCollectors.Session);
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                // This just gets in the way.
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            // Insert the M2_LoadVCD snippet here


#if SQLITE
            await SQLiteService.InitDb();
#endif

            SetupShell(args);

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void SetupShell(IActivatedEventArgs args)
        {
            SetupTitleBarColors();

            var shell = Window.Current.Content as AppShell;
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (shell == null)
            {
                // Create a AppShell to act as the navigation context and navigate to the first page
                shell = new AppShell {Language = ApplicationLanguages.Languages[0]};

                shell.AppFrame.NavigationFailed += OnNavigationFailed;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }
            }

            // Place our app shell in the current Window
            Window.Current.Content = shell;
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            SetupShell(args);

            // we need to wait for the shell to load
            while (AppShell.Current == null)
            {
                await Task.Delay(200);
            }
            


            switch (args.Kind)
            {
                case ActivationKind.Protocol:
                    ProtocolActivatedEventArgs protocolArgs = args as ProtocolActivatedEventArgs;
                    Windows.Foundation.WwwFormUrlDecoder decoder = new Windows.Foundation.WwwFormUrlDecoder(protocolArgs.Uri.Query);
                    var siteId = decoder.GetFirstValueByName("LaunchContext");
                    var parameter = new TripNavigationParameter { TripId = AppSettings.LastTripId, SightId = Guid.ParseExact(siteId, "D")}.GetJson();
                    AppShell.Current.NavigateToPage(typeof(TripDetailPage), parameter);
                    break;

                // Insert the M2_VoiceActivation snippet here


                // Insert the M2_ToastActivation snippet here


                default:
                    break;
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        // Insert the M2_HandleVoiceCommand snippet here


        private void SetupTitleBarColors()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            // set up our brushes
            var titleBarBackground = Current.Resources["TitleBarBackgroundThemeBrush"] as SolidColorBrush;
            var titleBarForeground = Current.Resources["TitleBarForegroundThemeBrush"] as SolidColorBrush;
            var titleBarInactiveBackground =
                Current.Resources["TitleBarInactiveBackgroundThemeBrush"] as SolidColorBrush;
            var titleBarInactiveForeground =
                Current.Resources["TitleBarInactiveForegroundThemeBrush"] as SolidColorBrush;

            var titleBarButtonBackground = Current.Resources["TitleBarButtonBackgroundThemeBrush"] as SolidColorBrush;
            var titleBarButtonForeground = Current.Resources["TitleBarButtonForegroundThemeBrush"] as SolidColorBrush;
            var titleBarButtonHoverBackground =
                Current.Resources["TitleBarButtonHoverBackgroundThemeBrush"] as SolidColorBrush;
            var titleBarButtonHoverForeground =
                Current.Resources["TitleBarButtonHoverForegroundThemeBrush"] as SolidColorBrush;
            var titleBarButtonPressedBackground =
                Current.Resources["TitleBarButtonPressedBackgroundThemeBrush"] as SolidColorBrush;
            var titleBarButtonPressedForeground =
                Current.Resources["TitleBarButtonPressedForegroundThemeBrush"] as SolidColorBrush;

            var titleBarButtonInactiveBackground =
                Current.Resources["TitleBarButtonInactiveBackgroundThemeBrush"] as SolidColorBrush;
            var titleBarButtonInactiveForeground =
                Current.Resources["TitleBarButtonInactiveForegroundThemeBrush"] as SolidColorBrush;

            // override colors!
            titleBar.BackgroundColor = titleBarBackground.Color;
            titleBar.ForegroundColor = titleBarForeground.Color;
            titleBar.ButtonInactiveBackgroundColor = titleBarInactiveBackground.Color;
            titleBar.ButtonInactiveForegroundColor = titleBarInactiveForeground.Color;

            titleBar.ButtonBackgroundColor = titleBarButtonBackground.Color;
            titleBar.ButtonForegroundColor = titleBarButtonForeground.Color;
            titleBar.ButtonHoverBackgroundColor = titleBarButtonHoverBackground.Color;
            titleBar.ButtonHoverForegroundColor = titleBarButtonHoverForeground.Color;
            titleBar.ButtonPressedBackgroundColor = titleBarButtonPressedBackground.Color;
            titleBar.ButtonPressedForegroundColor = titleBarButtonPressedForeground.Color;

            titleBar.ButtonInactiveBackgroundColor = titleBarButtonInactiveBackground.Color;
            titleBar.ButtonInactiveForegroundColor = titleBarButtonInactiveForeground.Color;
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}