using System.Collections.Generic;
using Windows.UI.Composition;
using Windows.UI.Xaml.Shapes;

namespace Microsoft.Labs.SightsToSee.Library.Models
{
    public class SightVisualMapper
    {
        private static SightVisualMapper _instance;
        private readonly Dictionary<Sight, SightVisual> _map;

        private SightVisualMapper()
        {
            _map = new Dictionary<Sight, SightVisual>();
        }

        public SightVisual this[Sight sight]
        {
            get
            {
                return _map[sight];
            }
            set
            {
                _map[sight] = value;
            }
        }

        public static SightVisualMapper Current => _instance ?? (_instance = new SightVisualMapper());

        public void MapSight(Sight sight, ContainerVisual containerVisual, Rectangle rectImage)
        {
            if (_map.ContainsKey(sight))
            {
                _map[sight].ImageContainerVisual = containerVisual;
                _map[sight].RectImage = rectImage;
            }
            else
            {
                _map.Add(sight, new SightVisual {ImageContainerVisual = containerVisual, RectImage = rectImage});
            }
        }

        public void Remove(Sight sight)
        {
            if (_map.ContainsKey(sight))
            {
                _map.Remove(sight);
            }
        }

        public void Clear()
        {
            _map.Clear();
        }
    }

    public class SightVisual
    {
        public ContainerVisual ImageContainerVisual { get; set; }
        public Rectangle RectImage { get; set; }
    }
}