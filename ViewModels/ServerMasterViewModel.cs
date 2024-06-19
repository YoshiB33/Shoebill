using System.Reactive;
using ReactiveUI;
using Shoebill.Services;

namespace Shoebill.ViewModels;

public class ServerMasterViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; set; }
    private readonly INavigationService _navigationService;

    public ServerMasterViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        GoBackCommand = ReactiveCommand.Create(GoBack);
    }

    private void GoBack()
    {
        _navigationService.NavigateBack();
    }
}
