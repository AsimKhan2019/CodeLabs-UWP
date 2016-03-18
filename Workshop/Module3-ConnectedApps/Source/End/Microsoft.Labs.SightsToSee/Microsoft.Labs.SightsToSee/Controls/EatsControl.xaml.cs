using Windows.UI.Xaml.Controls;
using Microsoft.Labs.SightsToSee.Views;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Labs.SightsToSee.Controls
{
    public sealed partial class EatsControl : UserControl
    {
        public EatsControl()
        {
            InitializeComponent();
        }

        public EatsControlViewModel ViewModel => DataContext as EatsControlViewModel;
    }
}