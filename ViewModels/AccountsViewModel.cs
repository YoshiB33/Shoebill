using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using Shoebill.Models;
using Shoebill.Services;
using SukiUI.Controls;

namespace Shoebill.ViewModels;

public class AccountsViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> OpenDialogCommand { get; set; }
    public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; set; }
    public ReactiveCommand<string, Unit> EnterOverviewCommand { get; set; }
    private readonly INavigationService _navigationService;
    private readonly ISettingsService _settingsService;
    private readonly IApiService _apiService;

    public ObservableCollection<ApiKey> ApiKeys { get; set; } = [];

    public AccountsViewModel(INavigationService navigationService, ISettingsService settingsService, IApiService apiService)
    {
        _navigationService = navigationService;
        _settingsService = settingsService;
        _apiService = apiService;

        navigationService.NavigationRequested += OnNavigatedTo;

        OpenDialogCommand = ReactiveCommand.Create(AddNewApi);
        OpenSettingsCommand = ReactiveCommand.Create(NavigateSettings);
        EnterOverviewCommand = ReactiveCommand.Create<string>(EnterOverview);

        settingsService.ApiKeyUpdated += (apiKey, KeyUpdatedAction) => 
        {
            if (KeyUpdatedAction == KeyUpdatedAction.Added)
            {
                ApiKeys.Add(apiKey);
            }
            else
            {
                ApiKeys.Remove(apiKey);
            }
        };
    }

    private async void OnNavigatedTo(Type page)
    {
        if (page == typeof(AccountsViewModel))
        {
            ApiKeys.Clear();
            var keys = await _settingsService.GetAllApiKeysAsync();
            foreach (var key in keys)
            {
                ApiKeys.Add(key);
            }
        }
    }

    private void AddNewApi()
    {
        SukiHost.ShowDialog(new CreateAccountViewModel(_settingsService), allowBackgroundClose: true);
    }

    private void NavigateSettings()
    {
        _navigationService.RequestNaviagtion<SettingsViewModel>();
    }

    private void EnterOverview(string name)
    {
        _apiService.SetApiKey(ApiKeys.Where(x => x.Name == name).First());
        _navigationService.RequestNaviagtion<ServerOverviewViewModel>();
    }
}
