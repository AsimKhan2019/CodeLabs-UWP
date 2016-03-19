using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Windows.Storage;
using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Labs.SightsToSee.Models;
using Newtonsoft.Json;

namespace Microsoft.Labs.SightsToSee.Services.RestaurantDataService
{
    public class RestaurantDataService
    {
        private static List<Restaurant> _restaurants;
        private static RestaurantDataService _current;

        // Ensure we only access this via the current property
        private RestaurantDataService()
        {
        }

        public static RestaurantDataService Current => _current ?? (_current = new RestaurantDataService());

        private static async Task<double> GetDrivingDistanceAsync(Geopoint fromLocation, Geopoint toLocation)
        {
            MapService.ServiceToken = AppSettings.MapServiceToken;
            var mapRouteFinderResult = await MapRouteFinder.GetDrivingRouteAsync(toLocation, toLocation);

            // returns 10000 if no route can be found
            return mapRouteFinderResult.Route?.LengthInMeters ?? 10000;
        }

        public async Task<List<Restaurant>> FindClosestRestaurantsAsync(Geopoint currentLocation, int number)
        {
            var restaurantsByDistance = new SortedList<double, Restaurant>();

            foreach (var restaurant in await GetRestaurantsAsync())
            {
                var distance = await GetDrivingDistanceAsync(currentLocation, restaurant.Location);
                restaurantsByDistance.Add(distance, restaurant);
            }

            var candidates = restaurantsByDistance.Take(number).Select(s => s.Value).ToList();

            // may return null here.
            return candidates;
        }

        public async Task<List<Restaurant>> FindClosestRestaurantsAsCrowFliesAsync(Geopoint currentLocation, int number)
        {
            var restaurantsByDistance = new SortedList<double, Restaurant>();

            foreach (var restaurant in await GetRestaurantsAsync())
            {
                var distance = CalculateGreatCircleDistance(currentLocation, restaurant.Location);
                restaurantsByDistance.Add(distance, restaurant);
            }

            var candidates = restaurantsByDistance.Take(number).Select(s => s.Value).ToList();

            // may return null here.
            return candidates;
        }


        private static async Task<List<Restaurant>> LoadDataAsync()
        {
            var models = await Package.Current.InstalledLocation.GetFolderAsync("Models");
            var demoDataFile = await models.GetFileAsync(@"DemoRestaurants.json");
            var demoData = await FileIO.ReadTextAsync(demoDataFile);
            var results = JsonConvert.DeserializeObject<List<Restaurant>>(demoData);
            return results;
        }

        public async Task<List<Restaurant>> GetRestaurantsAsync()
        {
            return _restaurants ?? (_restaurants = await LoadDataAsync());
        }

        public async Task<List<Restaurant>> GetRestaurantsForTripAsync(Trip trip)
        {
            // we only have static data for San Francisco so return all of it
            return (await GetRestaurantsAsync()).OrderBy(r => r.RankInDestination).ToList();
        }

        public async Task<List<Restaurant>> GetRestaurantsForSightAsync(Sight sight)
        {
            // Uses driving directions - slower
            //return await FindClosestRestaurantsAsync(sight.Location, 5);

            // much faster, but line of sight instead
            return await FindClosestRestaurantsAsCrowFliesAsync(sight.Location, 5);
        }

        private static double CalculateGreatCircleDistance(Geopoint point1, Geopoint point2)
        {
            return CalculateGreatCircleDistance(point1.Position.Latitude, point1.Position.Longitude,
                point2.Position.Latitude, point2.Position.Longitude);
        }

        private static double CalculateGreatCircleDistance(double latitude1,
            double longitude1, double latitude2, double longitude2)
        {
            // http://mathforum.org/library/drmath/view/51879.html
            var distance = double.MinValue;
            var latitude1InRadians = latitude1*(Math.PI/180.0);
            var longitude1InRadians = longitude1*(Math.PI/180.0);
            var latitude2InRadians = latitude2*(Math.PI/180.0);
            var longitude2InRadians = longitude2*(Math.PI/180.0);

            var longitude = longitude2InRadians - longitude1InRadians;
            var latitude = latitude2InRadians - latitude1InRadians;

            // square of half of the straight - line distance(chord length) between the two points
            var a = Math.Pow(Math.Sin(latitude/2.0), 2.0) +
                    Math.Cos(latitude1InRadians)*Math.Cos(latitude2InRadians)*
                    Math.Pow(Math.Sin(longitude/2.0), 2.0);

            var circleDistanceInRadians = 2.0*Math.Asin(Math.Sqrt(a));

            const double earthRadiusKms = 6376.5;
            distance = earthRadiusKms*circleDistanceInRadians;

            return distance;
        }
    }
}