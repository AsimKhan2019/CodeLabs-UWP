using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Windows.Devices.Geolocation;
using Windows.UI.Composition;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace Microsoft.Labs.SightsToSee.Models
{
    public class Sight
    {
#if EFCORE
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        [NotMapped]
        public Geopoint Location => new Geopoint(new BasicGeoposition {Latitude = Latitude, Longitude = Longitude});


        public string ImagePath { get; set; }

        [NotMapped]
        public BitmapImage ImageUri => new BitmapImage(new Uri(ImagePath, UriKind.Absolute));

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(2000)]
        public string Notes { get; set; }

        [Column(TypeName = "Date")]
        public DateTime? VisitDate { get; set; }

        // Sqlite does not support cascade delete
        //[Required]
        public Trip Trip { get; set; }

        public List<SightFile> SightFiles { get; set; }

        public int RankInDestination { get; set; }

        public bool IsMySight { get; set; }

#else
        public string Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string ImagePath { get; set; }

        [NotMapped]
        public BackgroundImage ImageUri => new BackgroundImage(new Uri(ImagePath, UriKind.Absolute));

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(2000)]
        public string Notes { get; set; }

        [Column(TypeName = "Date")]
        public DateTime? VisitDate { get; set; }

        // Used to link to parent trip when using Azure Easy Tables
        public string TripId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Geopoint Location => new Geopoint(new BasicGeoposition {Latitude = this.Latitude, Longitude = this.Longitude});

        [Newtonsoft.Json.JsonIgnore]
        public List<SightFile> SightFiles { get; set; }

        public bool IsMySight { get; set; }

#endif
    }
}