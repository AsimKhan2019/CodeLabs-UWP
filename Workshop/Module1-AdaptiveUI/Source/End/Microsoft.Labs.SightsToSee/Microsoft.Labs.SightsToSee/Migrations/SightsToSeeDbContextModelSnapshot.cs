using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Labs.SightsToSee.Models;

namespace Microsoft.Labs.SightsToSee.Migrations
{
    [DbContext(typeof(SightsToSeeDbContext))]
    partial class SightsToSeeDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("Microsoft.Labs.SightsToSee.Models.Sight", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 500);

                    b.Property<string>("ImagePath");

                    b.Property<bool>("IsMySight");

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<string>("Notes")
                        .HasAnnotation("MaxLength", 2000);

                    b.Property<int>("RankInDestination");

                    b.Property<Guid?>("TripId");

                    b.Property<DateTime?>("VisitDate")
                        .HasAnnotation("Relational:ColumnType", "Date");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Microsoft.Labs.SightsToSee.Models.SightFile", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileName")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<int>("FileType");

                    b.Property<Guid?>("SightId");

                    b.Property<string>("Uri");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Microsoft.Labs.SightsToSee.Models.Trip", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 200);

                    b.Property<DateTime>("StartDate")
                        .HasAnnotation("Relational:ColumnType", "Date");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Microsoft.Labs.SightsToSee.Models.Sight", b =>
                {
                    b.HasOne("Microsoft.Labs.SightsToSee.Models.Trip")
                        .WithMany()
                        .HasForeignKey("TripId");
                });

            modelBuilder.Entity("Microsoft.Labs.SightsToSee.Models.SightFile", b =>
                {
                    b.HasOne("Microsoft.Labs.SightsToSee.Models.Sight")
                        .WithMany()
                        .HasForeignKey("SightId");
                });
        }
    }
}
