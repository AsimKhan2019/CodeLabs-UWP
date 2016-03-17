using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;
using Microsoft.Labs.SightsToSee.Library.Models;

namespace Microsoft.Labs.SightsToSee.Library.Services.SightsService
{
    public static class SightsHelper
    {
        private static async Task<double> GetWalkingDistanceAsync(Geopoint currentLocation, Sight sight)
        {
            MapService.ServiceToken =
                "7H7lMjEkAfP3PeOrrPVO~IRTU1f4lP6GTdpBxi4gqoQ~AvvbAnSGbHtsowQ98zRfwvaw6PdCgo2vq3x75R3_SbvN2zb7-YcaM_UIPNtNWOWK";
            var mapRouteFinderResult = await MapRouteFinder.GetWalkingRouteAsync(currentLocation, sight.Location);
            return mapRouteFinderResult.Route.LengthInMeters;
        }

        private static async Task<double> GetDrivingDistanceAsync(Geopoint currentLocation, Sight sight)
        {
            MapService.ServiceToken =
                "7H7lMjEkAfP3PeOrrPVO~IRTU1f4lP6GTdpBxi4gqoQ~AvvbAnSGbHtsowQ98zRfwvaw6PdCgo2vq3x75R3_SbvN2zb7-YcaM_UIPNtNWOWK";
            var mapRouteFinderResult = await MapRouteFinder.GetDrivingRouteAsync(currentLocation, sight.Location);
            return mapRouteFinderResult.Route.LengthInMeters;
        }

        /// <summary>
        /// Determines the closest sight to the current location.
        /// </summary>
        /// <remarks>May return null if there are no sights added to the trip (i.e. IsMySight == true)</remarks>
        /// <param name="currentLocation">The current location.</param>
        /// <param name="trip">The trip we are searching.</param>
        /// <param name="useWalkingDistance"><c>true</c> if we are using walking distance; otherwise false.</param>
        /// <returns>returns the closest <see cref="Sight"/> or null if no sights are added to the trip.</returns>
        public static async Task<Sight> FindClosestSightAsync(Geopoint currentLocation, Trip trip, bool useWalkingDistance = true)
        {
            Sight closestSight = null;
            var closestDistance = double.MaxValue;

            // we only wish to search sights that have been added to our trip
            // note: that may mean we return null if there is no sight in our trip!
            foreach (var sight in trip.Sights.Where(s => s.IsMySight))
            {
                double distance;
                if (useWalkingDistance)
                {
                    distance = await GetWalkingDistanceAsync(currentLocation, sight);
                }
                else
                {
                    distance = await GetDrivingDistanceAsync(currentLocation, sight);
                }
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSight = sight;
                }
            }

            // may return null here.
            return closestSight;
        }


        public static async Task<List<Sight>> FindClosestSightsAsync(Geopoint currentLocation, Trip trip, bool useWalkingDistance = true)
        {
            var mySights = new SortedList<double, Sight>();

            // we only wish to search sights that have been added to our trip
            // note: that may mean we return null if there are no sights in our trip!
            foreach (var sight in trip.Sights.Where(s => s.IsMySight))
            {
                double distance;
                if (useWalkingDistance)
                {
                    distance = await GetWalkingDistanceAsync(currentLocation, sight);
                }
                else
                {
                    distance = await GetDrivingDistanceAsync(currentLocation, sight);
                }
                mySights.Add(distance, sight);
            }

            var candidates = mySights.Take(3).Select(s => s.Value).ToList();

            // may return null here.
            return candidates;
        }

    }
}