using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using Shoebill.Models;
using Shoebill.Services;
using Shoebill.ViewModels.Dialogs;
using SukiUI.Dialogs;

namespace Shoebill.ViewModels;

public class AccountsViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly ISukiDialogManager _dialogManager;
    private readonly INavigationService _navigationService;
    private readonly ISettingsService _settingsService;

    public AccountsViewModel(INavigationService navigationService, ISettingsService settingsService,
        IApiService apiService, ISukiDialogManager dialogManager)
    {
        _navigationService = navigationService;
        _settingsService = settingsService;
        _apiService = apiService;
        _dialogManager = dialogManager;

        navigationService.NavigationRequested += OnNavigatedTo;

        OpenDialogCommand = ReactiveCommand.Create(AddNewApi);
        EnterOverviewCommand = ReactiveCommand.Create<string>(EnterOverview);

        settingsService.ApiKeyUpdated += (apiKey, keyUpdatedAction) =>
        {
            if (keyUpdatedAction == KeyUpdatedAction.Added)
                ApiKeys.Add(apiKey);
            else
                ApiKeys.Remove(apiKey);
        };
    }

    public ReactiveCommand<Unit, Unit> OpenDialogCommand { get; set; }
    public ReactiveCommand<string, Unit> EnterOverviewCommand { get; set; }

    public ObservableCollection<ApiKey> ApiKeys { get; set; } = [];

    private async void OnNavigatedTo(Type page)
    {
        try
        {
            if (page != typeof(AccountsViewModel)) return;

            ApiKeys.Clear();
            var keys = await _settingsService.GetAllApiKeysAsync();
            if (keys == null) return;
            foreach (var key in keys) ApiKeys.Add(key);
        }
        catch (Exception e)
        {
            _dialogManager.CreateDialog()
                .WithTitle($"Error navigation to {nameof(AccountsViewModel)}.")
                .WithContent(e.Message)
                .TryShow();
        }
    }

    private void AddNewApi()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog => new CreateAccountViewModel(_settingsService, dialog))
            .Dismiss().ByClickingBackground()
            .TryShow();
        _dialogManager.CreateDialog()
            .WithTitle("Multi Option Dialog")
            .WithContent("Select any one of the below options:")
            .TryShow();
    }

    private void EnterOverview(string name)
    {
        _apiService.SetApiKey(ApiKeys.First(x => x.Name == name));
        _navigationService.RequestNavigation<ServerOverviewViewModel>();
    }
}