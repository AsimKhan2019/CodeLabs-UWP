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

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;

            // Insert the M2_TriggerDetails snippet here
            
        }

        // Insert the M2_GetNearest snippet here


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

