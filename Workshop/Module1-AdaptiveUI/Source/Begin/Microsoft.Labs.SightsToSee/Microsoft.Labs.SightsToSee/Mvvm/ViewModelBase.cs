using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Microsoft.Labs.SightsToSee.Services.NavigationService;

namespace Microsoft.Labs.SightsToSee.Mvvm
{
    public abstract class ViewModelBase : BindableBase, INavigatable
    {
        public virtual Task OnNavigatedToAsync(string parameter, NavigationMode mode)
        {
            return Task.FromResult<object>(null);
        }

        public virtual Task OnNavigatedFromAsync(bool suspending)
        {
            return Task.FromResult<object>(null);
        }
    }
}