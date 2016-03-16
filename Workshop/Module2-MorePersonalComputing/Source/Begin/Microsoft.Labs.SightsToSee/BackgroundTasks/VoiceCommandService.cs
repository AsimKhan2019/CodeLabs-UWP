using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
// Insert the M2_Using snippet here


namespace BackgroundTasks
{
    public sealed class VoiceCommandService : IBackgroundTask
    {
        // Insert the M2_ServiceConnection snippet here


        BackgroundTaskDeferral _serviceDeferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnTaskCanceled;

            // Insert the M2_TriggerDetails snippet here

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

