using Lumia.Imaging.EditShowcase.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Lumia.Imaging.EditShowcase.Views
{

    public sealed partial class FilterExplorerView : Page
    {
        public FilterExplorerView()
        {
            InitializeComponent();

            DataContext = new FilterExplorerViewModel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var protocolForResultsArgs = e.Parameter as ProtocolForResultsActivatedEventArgs;
            // Set the ProtocolForResultsOperation field.

            if (protocolForResultsArgs != null)
            {
                await (DataContext as FilterExplorerViewModel).LoadForProtocolActivationAsync(protocolForResultsArgs);
            }
        }

    }

}
