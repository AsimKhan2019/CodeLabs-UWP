using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Labs.SightsToSee.Models;

namespace Microsoft.Labs.SightsToSee.Services.DataModelService
{
    public class EfCoreDataModelService : IDataModelService
    {
        public Task<List<Trip>> LoadTripsWithAttractionsAsync()
        {
            using (var context = new SightsToSeeDbContext())
            {
                return context.Trips.Include(t => t.Sights).ThenInclude(s => s.SightFiles).ToListAsync();
            }
        }

        public async Task InsertTripAsync(Trip trip)
        {
            using (var context = new SightsToSeeDbContext())
            {
                context.Trips.Add(trip);
                await context.SaveChangesAsync();
            }
        }

        public Task<Trip> LoadTripAsync(Guid tripId)
        {
            using (var context = new SightsToSeeDbContext())
            {
                return
                    context.Trips.Include(t => t.Sights).ThenInclude(s => s.SightFiles).SingleAsync(t => t.Id == tripId);
            }
        }

        public async Task DeleteSightAsync(Sight sight)
        {
            using (var context = new SightsToSeeDbContext())
            {
                context.SiteFiles.RemoveRange(sight.SightFiles);
                context.Sights.Remove(sight);
                await context.SaveChangesAsync();
            }
        }

        public async Task SaveSightAsync(Sight sight)
        {
            using (var context = new SightsToSeeDbContext())
            {
                context.Sights.Update(sight);
                await context.SaveChangesAsync();
            }
        }

        Task<List<Trip>> IDataModelService.LoadTripsAsync()
        {
            return LoadTripsAsync();
        }

        public Task<List<Trip>> LoadTripsAsync()
        {
            using (var context = new SightsToSeeDbContext())
            {
                return context.Trips.ToListAsync();
            }
        }
    }
}