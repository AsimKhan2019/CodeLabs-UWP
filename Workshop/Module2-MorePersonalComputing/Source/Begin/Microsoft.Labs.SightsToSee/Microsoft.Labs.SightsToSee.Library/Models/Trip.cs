using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using SQLite.Net.Attributes;

namespace Microsoft.Labs.SightsToSee.Library.Models
{
    public class Trip : IGuidTable
    {
        [PrimaryKey]
        public Guid Id { get; set; }


        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "Date")]
        public DateTime StartDate { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        [Ignore]
        public Geopoint Location => new Geopoint(new BasicGeoposition {Latitude = this.Latitude, Longitude = this.Longitude});

        [Ignore]
        public List<Sight> Sights { get; set; }

    }
}