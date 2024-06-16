using System.Reactive;
using ReactiveUI;
using Shoebill.Services;

namespace Shoebill.ViewModels;

public class ServerOverviewViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> NavigateBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NavigateSettingsCommand { get; set; }
    private readonly INavigationService _navigationService;
    public ServerOverviewViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        NavigateBackCommand = ReactiveCommand.Create(NavigateBack);
        NavigateSettingsCommand = ReactiveCommand.Create(NavigateSettings);
    }

    private void NavigateBack() =>
        _navigationService.NavigateBack();
    private void NavigateSettings() =>
        _navigationService.RequestNaviagtion<SettingsViewModel>();
}
