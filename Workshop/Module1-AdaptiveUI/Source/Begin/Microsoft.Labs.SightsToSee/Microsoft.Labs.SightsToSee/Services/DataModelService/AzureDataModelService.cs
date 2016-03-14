using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Microsoft.Labs.SightsToSee.Models;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

// offline sync

namespace Microsoft.Labs.SightsToSee.Services.DataModelService
{
#if !EFCORE
    public class AzureDataModelService : IDataModelService
    {
        private readonly IMobileServiceSyncTable<Sight> _attractionTable =
            App.MobileService.GetSyncTable<Sight>(); // offline sync

        //private IMobileServiceTable<TodoItem> todoTable = App.MobileService.GetTable<TodoItem>();
        private readonly IMobileServiceSyncTable<Trip> _tripTable = App.MobileService.GetSyncTable<Trip>();
        // offline sync

        public async Task<List<Trip>> LoadTripsAsync()
        {
            try
            {
                await Initialize();
                return await _tripTable.ToListAsync();
            }
            catch (Exception ex)
            {
                var errorString = "Load failed: " + ex.Message +
                                  "\n\nIf you are still in an offline scenario, " +
                                  "you can try your Pull again when connected with your Mobile Serice.";
                await ShowError(errorString);
                throw;
            }
        }

        public async Task<List<Trip>> LoadTripsWithAttractionsAsync()
        {
            try
            {
                await Initialize();
                var trips = await _tripTable.ToListAsync();
                var attractions = await _attractionTable.ToListAsync();

                foreach (var trip in trips)
                {
                    trip.Sights = new List<Sight>(attractions.Where(a => a.TripId == trip.Id));
                }

                return trips;
            }
            catch (Exception ex)
            {
                var errorString = "Load failed: " + ex.Message +
                                  "\n\nIf you are still in an offline scenario, " +
                                  "you can try your Pull again when connected with your Mobile Serice.";
                await ShowError(errorString);
                throw;
            }
        }

        public async Task InsertTripAsync(Trip trip)
        {
            try
            {
                await Initialize();
                await _tripTable.InsertAsync(trip);

                // should have ID
                var savedTrip = (await _tripTable.Where(t => t.Name == trip.Name).ToListAsync()).Single();
                   
                if (trip.Sights != null)
                {
                    foreach (var attraction in trip.Sights)
                    {
                        attraction.TripId = savedTrip.Id;
                        await _attractionTable.InsertAsync(attraction);
                    }
                }
                await SyncAsync(); // offline sync     
            }
            catch (Exception ex)
            {
                var errorString = "Insert failed: " + ex.Message +
                                  "\n\nIf you are still in an offline scenario, " +
                                  "you can try your Pull again when connected with your Mobile Serice.";
                await ShowError(errorString);
                throw;
            }
        }


        private async Task Initialize()
        {
            if (!App.MobileService.SyncContext.IsInitialized)
            {
                var store = new MobileServiceSQLiteStore("localstore.db");
                store.DefineTable<Trip>();
                store.DefineTable<Sight>();
                await App.MobileService.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            }

            await SyncAsync();
        }

        private async Task SyncAsync()
        {
            string errorString = null;

            try
            {
                await App.MobileService.SyncContext.PushAsync();
                await _tripTable.PullAsync("tripItems", _tripTable.CreateQuery());
                await _attractionTable.PullAsync("attractionItems", _tripTable.CreateQuery());
            }

            catch (MobileServicePushFailedException ex)
            {
                errorString = "Push failed because of sync errors. You may be offine.\nMessage: " +
                              ex.Message + "\nPushResult.Status: " + ex.PushResult.Status;
            }
            catch (Exception ex)
            {
                errorString = "Pull failed: " + ex.Message +
                              "\n\nIf you are still in an offline scenario, " +
                              "you can try your Pull again when connected with your Mobile Serice.";
            }

            if (errorString != null)
            {
                await ShowError(errorString);
            }
        }

        private static async Task ShowError(string errorString)
        {
            var d = new ContentDialog
            {
                Content = errorString,
                Title = "Sync Error",
                PrimaryButtonText = "Ok",
                IsPrimaryButtonEnabled = true
            };
            await d.ShowAsync();
        }
    }
#endif
}