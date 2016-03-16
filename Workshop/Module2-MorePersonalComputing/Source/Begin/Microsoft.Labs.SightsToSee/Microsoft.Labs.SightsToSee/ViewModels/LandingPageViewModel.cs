using System;
using Microsoft.Labs.SightsToSee.Library.Services.DataModelService;
using Microsoft.Labs.SightsToSee.Models;
using Microsoft.Labs.SightsToSee.Mvvm;
using Microsoft.Labs.SightsToSee.Views;

namespace Microsoft.Labs.SightsToSee.ViewModels
{
    public class LandingPageViewModel : ViewModelBase
    {
        public bool InitialExperience => !AppSettings.HasRun;

        private bool showCreateFirstTrip = false;
        public bool ShowCreateFirstTrip
        {
            get
            {
                return showCreateFirstTrip;
            }
            set
            {
                if (showCreateFirstTrip != value)
                    Set(ref showCreateFirstTrip, value);
            }
        }

        public async void CreateTrip()
        {

           // NOTE: SeedDatafactory sets AppSettings.LastTripId to the trip Id value generated from the Seed Data
            AppShell.Current.AddTrip("San Francisco", AppSettings.LastTripId);
            AppSettings.HasRun = true;

            // the following will launch directly to the detail view of the closest sight
            //var parameter = new TripNavigationParameter { TripId = AppSettings.LastTripId, DisplayClosestSight = true }.GetJson();
            var parameter = new TripNavigationParameter { TripId = AppSettings.LastTripId }.GetJson();
            AppShell.Current.NavigateToPage(typeof (TripDetailPage), parameter);
            AppShell.Current.Frame?.BackStack.Clear();
        }
    }
}