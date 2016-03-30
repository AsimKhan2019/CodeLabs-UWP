using System;
using Newtonsoft.Json;

namespace Microsoft.Labs.SightsToSee.Models
{
    public enum TripPivots
    {
        Sights,
        Eats
    }
    public class TripNavigationParameter
    {
        public Guid TripId { get; set; }
        public Guid SightId { get; set; }
        public bool DisplayClosestSight { get; set; }
        public bool DeleteSight { get; set; }
        public string CuisinePreferences { get; set; }

        // Setting the default to Sights
        public TripPivots ShowPivotName { get; set; } = TripPivots.Sights;

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static TripNavigationParameter CreateFromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<TripNavigationParameter>(json);
            }
            catch
            {
                // if we can't deserialize then we'll return null
                return null;
            }
        }
    }
}