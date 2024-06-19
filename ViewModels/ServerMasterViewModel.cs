using Shoebill.Services;

namespace Shoebill.ViewModels;

public class ServerMasterViewModel : ViewModelBase
{
    private INavigationService _navigationService;

    public ServerMasterViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
}
