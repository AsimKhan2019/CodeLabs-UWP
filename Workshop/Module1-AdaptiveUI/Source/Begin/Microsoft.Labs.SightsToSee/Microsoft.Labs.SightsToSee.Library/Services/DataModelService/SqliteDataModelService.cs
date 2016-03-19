using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Labs.SightsToSee.Library.Models;

namespace Microsoft.Labs.SightsToSee.Library.Services.DataModelService
{
    public class SqliteDataModelService : IDataModelService
    {
        public Task<List<Trip>> LoadTripsWithAttractionsAsync()
        {
            var connection = SQLiteService.CreateAsyncConnection();
            return connection.Table<Trip>().ToListAsync();
        }

        public async Task InsertTripAsync(Trip trip)
        {
            var connection = SQLiteService.CreateAsyncConnection();
            await connection.InsertAsync(trip);
        }

        public async Task<Trip> LoadTripAsync(Guid tripId)
        {
            var connection = SQLiteService.CreateAsyncConnection();
            var trip = await connection.Table<Trip>().Where(t => t.Id == tripId).FirstAsync();
            trip.Sights = await connection.Table<Sight>().Where(s => s.TripId == trip.Id).ToListAsync();
            foreach (var sight in trip.Sights)
            {
                sight.SightFiles = await connection.Table<SightFile>().Where(s => s.SightId == sight.Id).ToListAsync();
                sight.Trip = trip;
            }
            return trip;
        }

        public async Task DeleteSightAsync(Sight sight)
        {
            foreach(var sightFile in sight.SightFiles)
            {
                await DeleteSightFileAsync(sightFile);
            }

            var connection = SQLiteService.CreateAsyncConnection();
            await connection.DeleteAsync(sight);
        }

        public async Task SaveSightAsync(Sight sight)
        {
            var connection = SQLiteService.CreateAsyncConnection();
            await connection.UpdateAsync(sight);
        }

        public async Task InsertSights(IEnumerable<Sight> sights)
        {
            var connection = SQLiteService.CreateAsyncConnection();
            var result = await connection.InsertAllAsync(sights);

            foreach(var sight in sights)
            {
                await InsertSightFiles(sight.SightFiles);
            }
        }

        Task<List<Trip>> IDataModelService.LoadTripsAsync()
        {
            return LoadTripsAsync();
        }

        public Task<List<Trip>> LoadTripsAsync()
        {
            var connection = SQLiteService.CreateAsyncConnection();
            return connection.Table<Trip>().ToListAsync();
        }

        public async Task SaveSightFileAsync(SightFile sightFile)
        {
            var connection = SQLiteService.CreateAsyncConnection();
            await connection.InsertAsync(sightFile);
        }

        public async Task UpdateSightFileAsync(SightFile sightFile)
        {
            var connection = SQLiteService.CreateAsyncConnection();
            await connection.UpdateAsync(sightFile);
        }

        public async Task<Sight> LoadSightAsync(Guid sightId)
        {
            var connection = SQLiteService.CreateAsyncConnection();
            var sight = await connection.Table<Sight>().Where(s => s.Id == sightId).FirstAsync();
            sight.Trip = await connection.Table<Trip>().Where(t => t.Id == sight.TripId).FirstAsync();
            sight.SightFiles = await connection.Table<SightFile>().Where(s => s.SightId == sight.Id).ToListAsync();
            return sight;
        }

        public Task DeleteTripAsync(Trip trip)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteSightFileAsync(SightFile sightFile)
        {
            var connection = SQLiteService.CreateAsyncConnection();
            await connection.DeleteAsync(sightFile);
        }

        public async Task InsertSightFiles(IEnumerable<SightFile> sightFiles)
        {
            var connection = SQLiteService.CreateAsyncConnection();
            await connection.InsertAllAsync(sightFiles);
        }

        public async Task<Tuple<bool, string>> AuthenticateAsync()
        {
            // No authentication required for the local SQLite storage
            // This is the async equivalent of an empty method body
            await Task.FromResult(true);
            return Tuple.Create(true, string.Empty);
        }
    }
}