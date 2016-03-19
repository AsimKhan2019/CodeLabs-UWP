using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Labs.SightsToSee.Library.Models;
using SQLite.Net;
using SQLite.Net.Attributes;
using Microsoft.Labs.SightsToSee.Models;

namespace Microsoft.Labs.SightsToSee.Library.Services.DataModelService
{
    public static class SeedDataFactory
    {
        public static Guid TripId = Guid.Empty;

        /// <summary>
        ///     Loads the seed data.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="force">Optional parameter - if set to <code>true</code> will delete existing data and then reload.</param>
        public static async Task LoadDataAsync(SQLiteConnection connection = null, bool force = false)
        {
#if SQLITE
            var tripCount = connection.Table<Trip>().Any();
            // Only load seed data if we need to
            if (connection.Table<Trip>().Any() && !force)
            {
                TripId = AppSettings.LastTripId;
                return;
            }

            if (force)
            {
                // Deleting children first as Sqlite does not support cascade delete
                connection.DeleteAll<SightFile>();
                connection.DeleteAll<Sight>();
                connection.DeleteAll<Trip>();
                connection.Commit();
            }

            StorageFolder models = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Models");
            StorageFile demoDataFile = await models.GetFileAsync(@"DemoSights.json");
            var trip = await CreateSampleTrip(demoDataFile);
            foreach (var sight in trip.Sights)
            {
                sight.TripId = trip.Id;
                foreach (var sightFile in sight.SightFiles)
                {
                    sightFile.SightId = sight.Id;
                }
            }


            connection.Insert(trip);
            connection.InsertAll(trip.Sights);
            foreach (var sight in trip.Sights)
            {
                connection.InsertAll(sight.SightFiles);
            }
            connection.Commit();

            TripId = trip.Id;

#else
            var dm = DataModelServiceFactory.CurrentDataModelService();

            var trips = await dm.LoadTripsAsync();
            // Only load seed data if we need to
            if (trips.Any() && !force)
            {
                TripId = AppSettings.LastTripId;
                return;
            }

            if (force)
            {
                var tripsComplete = await dm.LoadTripsWithAttractionsAsync();
                // Deleting children first 
                foreach (var trip in tripsComplete)
                {
                    foreach (var sight in trip.Sights)
                    {
                        await dm.DeleteSightAsync(sight);
                    }
                    await dm.DeleteTripAsync(trip);
                }
            }

            StorageFolder models = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Models");
            StorageFile demoDataFile = await models.GetFileAsync(@"DemoSights.json");
            var sampletrip = await CreateSampleTrip(demoDataFile);
            
            TripId = sampletrip.Id;
            AppSettings.LastTripId = TripId;

            await dm.InsertTripAsync(sampletrip);
#endif
            AppSettings.LastTripId = TripId;

        }

        public static async Task<Trip> CreateSampleTrip(StorageFile file)
        {
            string demoDataJson = await FileIO.ReadTextAsync(file);

            var trip = new Trip
            {
                Id = Guid.NewGuid(),
                Name = "San Francisco",
                Latitude = 37.75598,
                Longitude = -122.44214,
                StartDate = DateTime.Today + TimeSpan.FromDays(15),
                Sights = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Sight>>(demoDataJson)
            };

            //// Copy files to local storage
            //var localStorage = ApplicationData.Current.LocalFolder;
            //var fileList = new List<Tuple<string, string>>();

            //for (var i = 1; i < 26; i++)
            //{
            //    // copy the app file to local storage
            //    var resourcePath = $"ms-appx:///Assets/DemoImages/SightImages/{i}.jpg";
            //    var original =
            //        await StorageFile.GetFileFromApplicationUriAsync(new Uri(resourcePath));
            //    var newFile = await original.CopyAsync(localStorage, original.Name, NameCollisionOption.ReplaceExisting);
            //    fileList.Add(new Tuple<string, string>(resourcePath, newFile.Path));
            //}

            //foreach (var sight in trip.Sights)
            //{
            //    sight.SightFiles = new List<SightFile>();
            //    foreach (var tuple in fileList)
            //    {
            //        sight.SightFiles.Add(new SightFile
            //        {
            //            Id = Guid.NewGuid(),
            //            FileName = tuple.Item2,
            //            FileType = SightFileType.Image,
            //            Uri = tuple.Item1,
            //        });
            //    }
            //}

            foreach (var sight in trip.Sights)
            {
                sight.TripId = trip.Id;
                sight.Trip = trip;
                sight.SightFiles = new List<SightFile>();
                if (sight.Name == "Golden Gate Bridge") // Golden gate bridge
                {
                    sight.Id = Guid.NewGuid();
                    sight.SightFiles.Add(
                        new SightFile
                        {
                            Id = Guid.NewGuid(),
                            FileName = "GettyImages-547979731.jpg",
                            FileType = SightFileType.ImageGallery,
                            Uri = "ms-appx:///Assets/DemoImages/GettyImages-547979731.jpg",
                            SightId = sight.Id,
                            Sight = sight
                        });
                    sight.SightFiles.Add(
                        new SightFile
                        {
                            Id = Guid.NewGuid(),
                            FileName = "GettyImages-462144413.jpg",
                            FileType = SightFileType.ImageGallery,
                            Uri = "ms-appx:///Assets/DemoImages/GettyImages-462144413.jpg",
                            SightId = sight.Id,
                            Sight = sight
                        });
                    sight.SightFiles.Add(
                        new SightFile
                        {
                            Id = Guid.NewGuid(),
                            FileName = "GettyImages-578441735.jpg",
                            FileType = SightFileType.ImageGallery,
                            Uri = "ms-appx:///Assets/DemoImages/GettyImages-578441735.jpg",
                            SightId = sight.Id,
                            Sight = sight
                        });
                    sight.SightFiles.Add(
                        new SightFile
                        {
                            Id = Guid.NewGuid(),
                            FileName = "GettyImages-587409351.jpg",
                            FileType = SightFileType.ImageGallery,
                            Uri = "ms-appx:///Assets/DemoImages/GettyImages-587409351.jpg",
                            SightId = sight.Id,
                            Sight = sight
                        });

                    continue;
                }

                if (sight.Name == "Chinatown") // Chinatown
                {
                    sight.Id = Guid.NewGuid();
                    sight.SightFiles.Add(new SightFile
                    {
                        Id = Guid.NewGuid(),
                        FileName = "GettyImages-475046491.jpg",
                        FileType = SightFileType.ImageGallery,
                        Uri = "ms-appx:///Assets/DemoImages/GettyImages-475046491.jpg",
                        SightId = sight.Id,
                        Sight = sight
                    });
                    sight.SightFiles.Add(new SightFile
                    {
                        Id = Guid.NewGuid(),
                        FileName = "GettyImages-512468667.jpg",
                        FileType = SightFileType.ImageGallery,
                        Uri = "ms-appx:///Assets/DemoImages/GettyImages-512468667.jpg",
                        SightId = sight.Id,
                        Sight = sight
                    });
                    sight.SightFiles.Add(new SightFile
                    {
                        Id = Guid.NewGuid(),
                        FileName = "GettyImages-568537447.jpg",
                        FileType = SightFileType.ImageGallery,
                        Uri = "ms-appx:///Assets/DemoImages/GettyImages-568537447.jpg",
                        SightId = sight.Id,
                        Sight = sight
                    });

                    continue;
                }

                // Add at least one sightfiles to remaining sights to match sight image
                sight.Id = Guid.NewGuid();
                sight.SightFiles.Add(new SightFile
                {
                    Id = Guid.NewGuid(),
                    FileName = Path.GetFileName(sight.ImagePath),
                    FileType = SightFileType.ImageGallery,
                    Uri = sight.ImagePath,
                    SightId = sight.Id,
                    Sight = sight
                });
            }
            return trip;
        }

        public static Trip CreateDesignTrip()
        {

            var trip = new Trip
            {
                Id = TripId,
                Name = "San Francisco",
                Latitude = 37.75598,
                Longitude = -122.44214,
                StartDate = DateTime.Today + TimeSpan.FromDays(15),
                Sights = new List<Sight>
                {
                    new Sight
                    {
                        Id = Guid.NewGuid(),
                        Name = "Glenwood Canyon",
                        ImagePath =
                            "https://upload.wikimedia.org/wikipedia/commons/thumb/f/ff/Glencan.JPG/800px-Glencan.JPG",
                        Latitude = 39.575,
                        Longitude = -107.223,
                        Description =
                            "Glenwood Canyon is a rugged scenic 12.5 mi (20 km) canyon on the Colorado River in western Colorado in the United States. Its walls climb as high as 1,300 ft (396 m) above the Colorado River",
                    },
                    new Sight
                    {
                        Id = Guid.NewGuid(),
                        Name = "Hanging Lake",
                        ImagePath = "http://internetbrothers.org/images/hanging_lake_falls.jpg",
                        Latitude = 39.6016,
                        Longitude = -107.192,
                        Description =
                            "Hanging Lake is a lake in the U.S. State of Colorado. It is located in Glenwood Canyon, about 7 miles (11 km) east of Glenwood Springs, Colorado and is a very popular tourist destination.",
                    },
                    new Sight
                    {
                        Id = Guid.NewGuid(),
                        Name = "Glenwood Caverns Adventure Park",
                        ImagePath =
                            "http://media-cdn.tripadvisor.com/media/photo-o/0a/14/a2/da/20160117-131839-largejpg.jpg",
                        Latitude = 39.5605,
                        Longitude = -107.3202,
                        Description =
                            "In Glenwood Springs you can explore stunning caverns and formations... in Colorado's largest showcave.",
                    }
                }
            };

            foreach (var sight in trip.Sights)
            {
                sight.SightFiles.AddRange(
                    new []
                    {
                        new SightFile
                        {
                            Id = Guid.NewGuid(),
                            FileName ="https://upload.wikimedia.org/wikipedia/commons/thumb/f/ff/Glencan.JPG/800px-Glencan.JPG",
                            FileType = SightFileType.ImageGallery,
                            Uri = "https://upload.wikimedia.org/wikipedia/commons/thumb/f/ff/Glencan.JPG/800px-Glencan.JPG",
                        },
                        new SightFile
                        {
                            Id = Guid.NewGuid(),
                            FileName ="http://internetbrothers.org/images/hanging_lake_falls.jpg",
                            FileType = SightFileType.ImageGallery,
                            Uri = "http://internetbrothers.org/images/hanging_lake_falls.jpg",
                        },
                        new SightFile
                        {
                            Id = Guid.NewGuid(),
                            FileName ="http://media-cdn.tripadvisor.com/media/photo-o/0a/14/a2/da/20160117-131839-largejpg.jpg",
                            FileType = SightFileType.ImageGallery,
                            Uri = "http://media-cdn.tripadvisor.com/media/photo-o/0a/14/a2/da/20160117-131839-largejpg.jpg",
                        },
                    });
            }

            //// Copy files to local storage
            //var localStorage = ApplicationData.Current.LocalFolder;
            //var fileList = new List<Tuple<string, string>>();

            //for (var i = 1; i < 26; i++)
            //{
            //    // copy the app file to local storage
            //    var resourcePath = $"ms-appx:///Assets/DemoImages/SightImages/{i}.jpg";
            //    var original =
            //        await StorageFile.GetFileFromApplicationUriAsync(new Uri(resourcePath));
            //    var newFile = await original.CopyAsync(localStorage, original.Name, NameCollisionOption.ReplaceExisting);
            //    fileList.Add(new Tuple<string, string>(resourcePath, newFile.Path));
            //}

            //foreach (var sight in trip.Sights)
            //{
            //    sight.SightFiles = new List<SightFile>();
            //    foreach (var tuple in fileList)
            //    {
            //        sight.SightFiles.Add(new SightFile
            //        {
            //            Id = Guid.NewGuid(),
            //            FileName = tuple.Item2,
            //            FileType = SightFileType.Image,
            //            Uri = tuple.Item1,
            //        });
            //    }
            //}
            return trip;
        }

    }
}