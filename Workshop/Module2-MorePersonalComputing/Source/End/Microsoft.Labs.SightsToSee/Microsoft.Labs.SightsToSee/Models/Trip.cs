using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Windows.Devices.Geolocation;

namespace Microsoft.Labs.SightsToSee.Models
{
    public class Trip
    {
#if EFCORE
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [Column(TypeName = "Date")]
        public DateTime StartDate { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        [NotMapped]
        public Geopoint Location => new Geopoint(new BasicGeoposition {Latitude = this.Latitude, Longitude = this.Longitude});

        public List<Sight> Sights { get; set; }
#else
        public string Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [Column(TypeName = "Date")]
        public DateTime StartDate { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public List<Sight> Sights { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Geopoint Location => new Geopoint(new BasicGeoposition {Latitude = this.Latitude, Longitude = this.Longitude});

#endif
    }
}