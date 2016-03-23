using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.ApplicationModel.AppService;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Labs.SightsToSee.Library.Services.DataModelService;
using Microsoft.Labs.SightsToSee.Library.Services.SightsService;
using Microsoft.Labs.SightsToSee.Models;

namespace BackgroundTasks
{
    public sealed class VoiceCommandService : IBackgroundTask
    {
        VoiceCommandServiceConnection _voiceServiceConnection;
        BackgroundTaskDeferral _serviceDeferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;

            // Get trigger details

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (triggerDetails != null && triggerDetails.Name == "VoiceCommandService")
            {
                try
                {
                    _voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

                    _voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                    VoiceCommand voiceCommand = await _voiceServiceConnection.GetVoiceCommandAsync();

                        switch (voiceCommand.CommandName)
                        {
                            case "NearbySights":
                                GeolocationAccessStatus accessStatus;
                                try
                                {
                                    // If we call this before the app has granted access, we get an exception
                                    accessStatus = await Geolocator.RequestAccessAsync();
                                }
                                catch
                                {
                                    // ensure we have a value
                                    accessStatus = GeolocationAccessStatus.Unspecified;
                                }
                                if (accessStatus == GeolocationAccessStatus.Allowed)
                                {

                                    var geolocator = new Geolocator();
    #if DEBUG
                                    // For testing, fake a location in San Francisco
                                    Geopoint point = new Geopoint(new BasicGeoposition { Latitude = 37.774930, Longitude = -122.419416 });
                                    if (true) { 
    #else
                                    var pos = await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(5));
                                    if (pos != null)
                                    {
                                        Geocoordinate coordinate = pos.Coordinate; 
    #endif

                                        var nearest = await GetNearestSights(point);
                                        if (nearest != null && nearest.Any())
                                        {
                                            await ShowNearestResults(nearest);
                                        }
                                        else
                                        {
                                            await ReportFailureToGetSights();
                                        }
                                    }
                                    else
                                    {
                                        await ReportFailureToGetCurrentLocation();
                                    }
                                }
                                else
                                {
                                    await ReportFailureToGetCurrentLocation();
                                }
                                break;
                            default:
                                break;
                        }
                }
                catch (Exception ex)
                {
                    var userMessage = new VoiceCommandUserMessage();
                    userMessage.SpokenMessage = "Sorry, I can't do that right now - something went wrong.";
                    // useful for debugging
                    userMessage.DisplayMessage = ex.Message;

                    var response = VoiceCommandResponse.CreateResponse(userMessage);

                    response.AppLaunchArgument = "LaunchApp";
                    await _voiceServiceConnection.ReportFailureAsync(response);
                }
            }
        }

        private static async Task<List<Sight>> GetNearestSights(Geopoint point)
        {
            var datamodelService = DataModelServiceFactory.CurrentDataModelService();

            // we are just loading the default trip here
            var trip = await datamodelService.LoadTripAsync(AppSettings.LastTripId);
            var nearest = await SightsHelper.FindClosestSightsAsync(point, trip, false);
            return nearest;
        }

        private async Task ShowNearestResults(List<Sight> nearest)
        {
            var userMessage = new VoiceCommandUserMessage
            {
                DisplayMessage = "Here are your closest Sights:",
                SpokenMessage = "Here are your closest sights"
            };

            var sightsContentTiles = new List<VoiceCommandContentTile>();

            foreach (var sight in nearest)
            {
                var sightTile = new VoiceCommandContentTile();
                sightTile.ContentTileType = VoiceCommandContentTileType.TitleWith68x68IconAndText;
                if (sight.ImagePath.StartsWith("ms-appx"))
                {
                    sightTile.Image =
                        await StorageFile.GetFileFromApplicationUriAsync(new Uri(sight.ImagePath));
                }
                else
                {
                    sightTile.Image = await StorageFile.GetFileFromPathAsync(sight.ImagePath);
                }
                sightTile.Title = sight.Name;
                sightTile.TextLine1 = sight.Description;
                sightTile.AppContext = sight.Id;
                sightTile.AppLaunchArgument = sight.Id.ToString("D");
                sightsContentTiles.Add(sightTile);
            }


            var response = VoiceCommandResponse.CreateResponse(userMessage, sightsContentTiles);
            await _voiceServiceConnection.ReportSuccessAsync(response);
        }

        private async Task ReportFailureToGetCurrentLocation()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.DisplayMessage = userMessage.SpokenMessage = "Sorry, I can't access your location at the moment.";

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "LaunchApp";
            await _voiceServiceConnection.ReportFailureAsync(response);
        }

        private async Task ReportFailureToGetSights()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.DisplayMessage = userMessage.SpokenMessage = "Sorry, I can't find any sights in your trip.";

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "LaunchApp";
            await _voiceServiceConnection.ReportFailureAsync(response);
        }

        // Handle the VoiceCommandCompleted event
        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            // Complete the service deferral
            this._serviceDeferral?.Complete();
        }

        // Clean up on task cancellation
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Task cancelled, clean up");
            // Complete the service deferral
            this._serviceDeferral?.Complete();
        }
    }
}

