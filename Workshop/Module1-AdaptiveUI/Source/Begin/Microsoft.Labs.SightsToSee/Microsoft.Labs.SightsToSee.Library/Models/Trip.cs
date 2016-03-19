using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using SQLite.Net.Attributes;

namespace Microsoft.Labs.SightsToSee.Library.Models
{
    public class Trip 
    {
#if SQLITE
        [PrimaryKey]
        public Guid Id { get; set; }

#else
        private Guid _id = Guid.Empty;
        [Newtonsoft.Json.JsonIgnore]
        public Guid Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        // string version required by cloud sync
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string DTOId
        {
            get
            {
                return _id.ToString();
            }
            set
            {
                _id = Guid.Parse(value);
            }
        }

#endif

        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Name { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "Date")]
        public DateTime StartDate { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [Ignore]
        public Geopoint Location => new Geopoint(new BasicGeoposition {Latitude = this.Latitude, Longitude = this.Longitude});

        [Newtonsoft.Json.JsonIgnore]
        [Ignore]
        public List<Sight> Sights { get; set; }

    }
}