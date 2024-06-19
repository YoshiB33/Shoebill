using System.Reactive;
using ReactiveUI;
using Shoebill.Services;

namespace Shoebill.ViewModels;

public class ServerMasterViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NavigateSettingsCommand { get; set; }
    private readonly INavigationService _navigationService;

    public ServerMasterViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        GoBackCommand = ReactiveCommand.Create(GoBack);
        NavigateSettingsCommand = ReactiveCommand.Create(NavigateSettings);
    }

    private void GoBack() =>
        _navigationService.NavigateBack();
    private void NavigateSettings() =>
        _navigationService.RequestNaviagtion<SettingsViewModel>(false);
}
