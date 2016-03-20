using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Labs.SightsToSee.Controls;
using Microsoft.Labs.SightsToSee.Services.FileService;
using Microsoft.Labs.SightsToSee.ViewModels;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Collections;
using Windows.System;
using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Labs.SightsToSee.Library.Services.DataModelService;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Microsoft.Labs.SightsToSee.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SightDetailPage : Page
    {
        public SightDetailPage()
        {
            InitializeComponent();

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string)
            {
                await ViewModel.LoadSightAsync(Guid.ParseExact((string) e.Parameter, "D"));
                //SightDetailControl.BackgroundImage = ViewModel.CurrentSight.ImageUri;
            }
            base.OnNavigatedTo(e);
        }


        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            // save any changes to the notes
            await ViewModel.UpdateSightAsync(ViewModel.CurrentSight);
            base.OnNavigatedFrom(e);
        }

        public SightDetailPageViewModel ViewModel => DataContext as SightDetailPageViewModel;

        private void DeleteSightButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            MobileDeleteFlyout?.Flyout?.Hide();
            DesktopDeleteFlyout?.Flyout?.Hide();
        }
    }
}