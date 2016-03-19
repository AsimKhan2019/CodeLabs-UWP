using System.Collections.Generic;

namespace Microsoft.Labs.SightsToSee.Library.Models
{
    public class SightGroup
    {
        public string GroupName { get; set; }
        public List<Sight> Sights  { get; set; }
    }
}
