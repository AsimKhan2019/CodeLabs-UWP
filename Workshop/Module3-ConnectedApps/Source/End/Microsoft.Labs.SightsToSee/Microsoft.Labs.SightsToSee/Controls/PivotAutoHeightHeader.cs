using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Labs.SightsToSee.Common;

namespace Microsoft.Labs.SightsToSee.Controls
{
    public class PivotAutoHeightHeader : Pivot
    {
        public PivotAutoHeightHeader() : base()
        {
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var headers = XamlUtils.AllChildren<PivotHeaderItem>(this);
            foreach (var headerItem in headers)
            {
                headerItem.Height = double.NaN;
            }
        }
    }
}
