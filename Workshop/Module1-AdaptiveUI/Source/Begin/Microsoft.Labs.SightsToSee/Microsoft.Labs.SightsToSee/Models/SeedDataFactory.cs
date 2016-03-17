using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Microsoft.Labs.SightsToSee.Models
{
    public static class SeedDataFactory
    {
        public static Guid TripId = Guid.ParseExact("{0d97fb97-efb7-4bc6-a9be-a24b4177ea48}", "B");

        /// <summary>
        ///     Loads the seed data.
        /// </summary>
        /// <param name="context">The <see cref="SightsToSeeDbContext" /> used for data loading.</param>
        /// <param name="force">Optional parameter - if set to <code>true</code> will delete existing data and then reload.</param>
        public static async Task LoadDataAsync(SightsToSeeDbContext context, bool force = false)
        {
#if EFCORE
            // Only load seed data if we need to
            if (context.Trips.Any() && !force)
            {
                return;
            }

            if (force)
            {
                // Deleting children first as Sqlite does not support cascade delete
                context.Sights.RemoveRange(context.Sights);
                context.Trips.RemoveRange(context.Trips);
                context.SaveChanges();
            }

            StorageFolder models = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Models");
            StorageFile demoDataFile = await models.GetFileAsync(@"DemoSights.json");
            string demoDataJson = await FileIO.ReadTextAsync(demoDataFile);

            var trip = new Trip
            {
                Id = TripId,
                Name = "San Francisco",
                Latitude = 37.75598,
                Longitude = -122.44214,
                StartDate = DateTime.Today + TimeSpan.FromDays(15),
                Sights = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Sight>>(demoDataJson)
                //Sights = new List<Sight>
                //{
                //    new Sight
                //    {
                //        Id = Guid.NewGuid(),
                //        Name = "Glenwood Canyon",
                //        ImagePath =
                //            "https://upload.wikimedia.org/wikipedia/commons/thumb/f/ff/Glencan.JPG/800px-Glencan.JPG",
                //        Latitude = 39.575,
                //        Longitude = -107.223,
                //        Description =
                //            "Glenwood Canyon is a rugged scenic 12.5 mi (20 km) canyon on the Colorado River in western Colorado in the United States. Its walls climb as high as 1,300 ft (396 m) above the Colorado River",
                //    },
                //    new Sight
                //    {
                //        Id = Guid.NewGuid(),
                //        Name = "Hanging Lake",
                //        ImagePath = "http://internetbrothers.org/images/hanging_lake_falls.jpg",
                //        Latitude = 39.6016,
                //        Longitude = -107.192,
                //        Description =
                //            "Hanging Lake is a lake in the U.S. State of Colorado. It is located in Glenwood Canyon, about 7 miles (11 km) east of Glenwood Springs, Colorado and is a very popular tourist destination.",
                //    },
                //    new Sight
                //    {
                //        Id = Guid.NewGuid(),
                //        Name = "Glenwood Caverns Adventure Park",
                //        ImagePath =
                //            "http://media-cdn.tripadvisor.com/media/photo-o/0a/14/a2/da/20160117-131839-largejpg.jpg",
                //        Latitude = 39.5605,
                //        Longitude = -107.3202,
                //        Description =
                //            "In Glenwood Springs you can explore stunning caverns and formations... in Colorado's largest showcave.",
                //    }
                //}
            };

            // Copy files to local storage
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
            //            FileType = 1,
            //            Uri = tuple.Item1,
            //        });
            //    }
            //}

            // defaults to including dependents: context.Trips.Add(trip, GraphBehavior.IncludeDependents);
            context.Trips.Add(trip);
            await context.SaveChangesAsync();
#endif
        }
    }
}