using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.Labs.SightsToSee.Library.Services.AzureService;

namespace Microsoft.Labs.SightsToSee.Library.Services.DataModelService
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

        public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
        {
            if (action == FileSynchronizationAction.Delete)
            {
                await FileHelper.DeleteLocalFileAsync(file);
            }
            else { // Create or update. We're aggressively downloading all files.
                await this.dataModelService.DownloadFileAsync(file);
            }
        }
    }
#endif
}
