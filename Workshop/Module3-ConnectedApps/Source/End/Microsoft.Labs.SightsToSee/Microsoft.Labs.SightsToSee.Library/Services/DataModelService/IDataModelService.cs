using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Labs.SightsToSee.Library.Models;

namespace Microsoft.Labs.SightsToSee.Library.Services.DataModelService
{
    public interface IDataModelService
    {
        Task<Tuple<bool, string>> AuthenticateAsync();
        Task<List<Trip>> LoadTripsAsync();
        Task<List<Trip>> LoadTripsWithAttractionsAsync();
        Task InsertSights(IEnumerable<Sight> sights);
        Task InsertTripAsync(Trip trip);
        Task<Trip> LoadTripAsync(Guid tripId);
        Task DeleteTripAsync(Trip trip);
        Task DeleteSightAsync(Sight sight);
        Task SaveSightAsync(Sight sight);
        Task SaveSightFileAsync(SightFile sightFile);
        Task DeleteSightFileAsync(SightFile sightFile);
        Task<Sight> LoadSightAsync(Guid sightId);
        Task UpdateSightFileAsync(SightFile sightFile);
    }
}