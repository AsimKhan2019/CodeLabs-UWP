using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCLStorage;

namespace Microsoft.Labs.SightsToSee.Library.Services.AzureService
{
    public class FileHelper
    {
        public static async Task<string> GetSightFilesPathAsync()
        {
            var storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var filePath = "SightFiles";

            var result = await storageFolder.TryGetItemAsync(filePath);

            if (result == null)
            {
                result = await storageFolder.CreateFolderAsync(filePath);
            }

            return result.Name; // later operations will use relative paths
        }

        public static async Task<string> CopySightFileAsync(string itemId, Windows.Storage.StorageFile sourceFile)
        {
            string recordFilesPath = Path.Combine(await GetSightFilesPathAsync(), itemId);
            Windows.Storage.StorageFolder filesFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync(recordFilesPath, Windows.Storage.CreationCollisionOption.OpenIfExists);

            var targetFile = await sourceFile.CopyAsync(filesFolder, sourceFile.Name, Windows.Storage.NameCollisionOption.ReplaceExisting);

            return targetFile.Path;
        }

        public static async Task<string> GetLocalRelativeFilePathAsync(string itemId, string fileName)
        {
            string recordFilesPath = Path.Combine(await GetSightFilesPathAsync(), itemId);

            var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(recordFilesPath);
            if (checkExists == ExistenceCheckResult.NotFound)
            {
                await FileSystem.Current.LocalStorage.CreateFolderAsync(recordFilesPath, CreationCollisionOption.ReplaceExisting);
            }

            return Path.Combine(recordFilesPath, fileName);
        }

        public static async Task<string> GetLocalFilePathAsync(string itemId, string fileName)
        {
            string recordFilesPath = Path.Combine(await GetSightFilesPathAsync(), itemId);

            var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(recordFilesPath);
            if (checkExists == ExistenceCheckResult.NotFound)
            {
                await FileSystem.Current.LocalStorage.CreateFolderAsync(recordFilesPath, CreationCollisionOption.ReplaceExisting);
            }

            string absoluteRecordFilesPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, recordFilesPath);

            return Path.Combine(absoluteRecordFilesPath, fileName);
        }

        public static async Task DeleteLocalFileAsync(Microsoft.WindowsAzure.MobileServices.Files.MobileServiceFile fileName)
        {
            string localPath = await GetLocalFilePathAsync(fileName.ParentId, fileName.Name);
            var checkExists = await FileSystem.Current.LocalStorage.CheckExistsAsync(localPath);

            if (checkExists == ExistenceCheckResult.FileExists)
            {
                var file = await FileSystem.Current.LocalStorage.GetFileAsync(localPath);
                await file.DeleteAsync();
            }
        }

    }
}
