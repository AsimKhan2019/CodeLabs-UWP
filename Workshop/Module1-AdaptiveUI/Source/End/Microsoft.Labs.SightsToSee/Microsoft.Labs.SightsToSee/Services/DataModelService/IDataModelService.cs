using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Labs.SightsToSee.Models;

namespace Microsoft.Labs.SightsToSee.Services.DataModelService
{
    public interface IDataModelService
    {
        Task<List<Trip>> LoadTripsAsync();
        Task<List<Trip>> LoadTripsWithAttractionsAsync();
        Task InsertTripAsync(Trip trip);
        Task<Trip> LoadTripAsync(Guid tripId);
        Task DeleteSightAsync(Sight sight);
        Task SaveSightAsync(Sight sight);
    }
}