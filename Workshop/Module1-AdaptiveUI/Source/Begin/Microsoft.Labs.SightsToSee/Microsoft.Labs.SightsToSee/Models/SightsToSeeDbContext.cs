using Microsoft.Data.Entity;

namespace Microsoft.Labs.SightsToSee.Models
{
    public class SightsToSeeDbContext : DbContext
    {
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Sight> Sights { get; set; }
        public DbSet<SightFile> SiteFiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename=sightstosee.db");
        }
    }
}