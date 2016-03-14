using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Labs.SightsToSee.Models
{
    public class SightGroup
    {
        public string GroupName { get; set; }
        public List<Sight> Sights  { get; set; }
    }
}
