using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using Shoebill.Models.Api.Schemas;
using Shoebill.Services;

namespace Shoebill.ViewModels;

public class ServerOverviewViewModel : ViewModelBase
{
    private bool _isLoading = false;
    public ReactiveCommand<Unit, Unit> NavigateBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NavigateSettingsCommand { get; set; }
    public ObservableCollection<Server> Servers { get; set; } = [];
    private readonly INavigationService _navigationService;
    private readonly IApiService _apiService;
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }
    public ServerOverviewViewModel(INavigationService navigationService, IApiService apiService)
    {
        _navigationService = navigationService;
        _apiService = apiService;

        navigationService.NavigationRequested += OnNavigatedTo;

        NavigateBackCommand = ReactiveCommand.Create(NavigateBack);
        NavigateSettingsCommand = ReactiveCommand.Create(NavigateSettings);
    }

    private async void OnNavigatedTo(System.Type page)
    {
        Servers.Clear();
        IsLoading = true;
        if (page == typeof(ServerOverviewViewModel))
        {
            var response = await _apiService.GetServersAsync() ?? throw new System.Exception("Servers is null");
            foreach (var server in response.Data)
            {
                Servers.Add(server.Attributes);
            }
            IsLoading = false;
        }
    }

    private void NavigateBack() =>
        _navigationService.NavigateBack();
    private void NavigateSettings() =>
        _navigationService.RequestNaviagtion<SettingsViewModel>();
}
