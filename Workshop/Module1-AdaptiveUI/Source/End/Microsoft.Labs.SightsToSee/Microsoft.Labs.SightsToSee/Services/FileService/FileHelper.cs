using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace Microsoft.Labs.SightsToSee.Services.FileService
{
    internal class FileHelper
    {
        public enum StorageStrategies
        {
            Local,
            Roaming,
            Temporary
        }

        /// <summary>Returns if a file is found in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <returns>Boolean: true if found, false if not found</returns>
        public async Task<bool> FileExistsAsync(string key, StorageStrategies location = StorageStrategies.Local)
        {
            return await GetIfFileExistsAsync(key, location) != null;
        }

        public async Task<bool> FileExistsAsync(string key, StorageFolder folder)
        {
            return await GetIfFileExistsAsync(key, folder) != null;
        }

        /// <summary>Deletes a file in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        public async Task<bool> DeleteFileAsync(string key, StorageStrategies location = StorageStrategies.Local)
        {
            var _File = await GetIfFileExistsAsync(key, location);
            if (_File != null)
                await _File.DeleteAsync();
            return !await FileExistsAsync(key, location);
        }

        /// <summary>Reads and deserializes a file into specified type T</summary>
        /// <typeparam name="T">Specified type into which to deserialize file content</typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <returns>Specified type T</returns>
        public async Task<T> ReadFileAsync<T>(string key, StorageStrategies location = StorageStrategies.Local)
        {
            try
            {
                // fetch file
                var _File = await GetIfFileExistsAsync(key, location);
                if (_File == null)
                    return default(T);
                // read content
                var _String = await FileIO.ReadTextAsync(_File);
                // convert to obj
                var _Result = Deserialize<T>(_String);
                return _Result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Serializes an object and write to file in specified storage strategy</summary>
        /// <typeparam name="T">Specified type of object to serialize</typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="location">Location storage strategy</param>
        public async Task<bool> WriteFileAsync<T>(string key, T value,
            StorageStrategies location = StorageStrategies.Local)
        {
            // convert to string
            var _String = Serialize(value);
            Debug.WriteLine(_String);

            // create file
            var _File =
                await CreateFileAsync(key, location, CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);
            Debug.WriteLine("File created/replaced");

            try
            {
                // save string to file
                await FileIO.WriteTextAsync(_File, _String).AsTask().ConfigureAwait(false);
                Debug.WriteLine("Wrote data to file");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw ex;
            }
            Debug.WriteLine("Checking if File exists and returning");

            // result
            return await FileExistsAsync(key, location).ConfigureAwait(false);
        }

        private async Task<StorageFile> CreateFileAsync(string key, StorageStrategies location = StorageStrategies.Local,
            CreationCollisionOption option = CreationCollisionOption.OpenIfExists)
        {
            switch (location)
            {
                case StorageStrategies.Local:
                    return await ApplicationData.Current.LocalFolder.CreateFileAsync(key, option);
                case StorageStrategies.Roaming:
                    return await ApplicationData.Current.RoamingFolder.CreateFileAsync(key, option);
                case StorageStrategies.Temporary:
                    return await ApplicationData.Current.TemporaryFolder.CreateFileAsync(key, option);
                default:
                    throw new NotSupportedException(location.ToString());
            }
        }

        private async Task<StorageFile> GetIfFileExistsAsync(string key, StorageFolder folder,
            CreationCollisionOption option = CreationCollisionOption.FailIfExists)
        {
            StorageFile retval;
            try
            {
                retval = await folder.GetFileAsync(key);
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("GetIfFileExistsAsync:FileNotFoundException");
                return null;
            }
            return retval;
        }

        /// <summary>Returns a file if it is found in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <returns>StorageFile</returns>
        private async Task<StorageFile> GetIfFileExistsAsync(string key,
            StorageStrategies location = StorageStrategies.Local,
            CreationCollisionOption option = CreationCollisionOption.FailIfExists)
        {
            StorageFile retval;
            try
            {
                switch (location)
                {
                    case StorageStrategies.Local:
                        retval = await ApplicationData.Current.LocalFolder.GetFileAsync(key);
                        break;
                    case StorageStrategies.Roaming:
                        retval = await ApplicationData.Current.RoamingFolder.GetFileAsync(key);
                        break;
                    case StorageStrategies.Temporary:
                        retval = await ApplicationData.Current.TemporaryFolder.GetFileAsync(key);
                        break;
                    default:
                        throw new NotSupportedException(location.ToString());
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("GetIfFileExistsAsync:FileNotFoundException");
                return null;
            }

            return retval;
        }

        private string Serialize<T>(T item)
        {
            return JsonConvert.SerializeObject(item,
                Formatting.None, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
                });
        }

        private T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}