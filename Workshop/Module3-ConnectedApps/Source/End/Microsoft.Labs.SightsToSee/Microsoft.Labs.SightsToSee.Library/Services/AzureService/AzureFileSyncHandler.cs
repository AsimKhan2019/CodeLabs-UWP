using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.Labs.SightsToSee.Library.Utilities;
using Microsoft.Labs.SightsToSee.Library.Services.DataModelService;

namespace Microsoft.Labs.SightsToSee.Library.Services.AzureService
{
#if !SQLITE
    class AzureFileSyncHandler : IFileSyncHandler
    {
        private readonly AzureDataModelService dataModelService;

        public AzureFileSyncHandler(AzureDataModelService dataService)
        {
            dataModelService = dataService;
        }

        public async Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
        {
            var filePath = await FileHelper.GetLocalFilePathAsync(metadata.ParentDataItemId, metadata.FileName);
            return new PathMobileServiceFileDataSource(filePath);
        }

        static AsyncLock mutex = new AsyncLock();

        public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
        {
            if (action == FileSynchronizationAction.Delete)
            {
                await FileHelper.DeleteLocalFileAsync(file);
            }
            else if (action == FileSynchronizationAction.Create)
            { 
                // Use an AsyncLock mutex to ensure we only handle one file at a time - this event handler gets fired multiple times,
                // often for the same file at the same time, causing conflicts
                using (await mutex.LockAsync())
                {
                    await dataModelService.DownloadFileAsync(file);
                }
            }
            else if (action == FileSynchronizationAction.Update)
            { // Update.
                //await this.todoItemManager.DownloadFileAsync(file);
            }
        }
    }
#endif
}
