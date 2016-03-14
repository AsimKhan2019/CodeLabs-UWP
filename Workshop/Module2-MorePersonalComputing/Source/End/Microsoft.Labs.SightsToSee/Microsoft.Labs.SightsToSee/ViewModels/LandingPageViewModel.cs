using Microsoft.Labs.SightsToSee.Models;
using Microsoft.Labs.SightsToSee.Mvvm;
using Microsoft.Labs.SightsToSee.Views;

namespace Microsoft.Labs.SightsToSee.ViewModels
{
    public class LandingPageViewModel : ViewModelBase
    {
        public bool InitialExperience => !AppSettings.HasRun;
        public void CreateTrip()
        {
            // NOTE: We are using a hard-coded value from the Seed Data
            AppShell.Current.AddTrip("San Francisco", SeedDataFactory.TripId);
            AppSettings.HasRun = true;
            AppShell.Current.NavigateToPage(typeof (TripDetailPage), SeedDataFactory.TripId.ToString("D"));
            AppShell.Current.Frame?.BackStack.Clear();
        }
    }
}