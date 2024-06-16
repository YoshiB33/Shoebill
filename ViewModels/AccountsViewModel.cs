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

    public ObservableCollection<ApiKey> ApiKeys { get; set; }

    public AccountsViewModel(INavigationService navigationService, ISettingsService settingsService)
    {
        _navigationService = navigationService;
        _settingsService = settingsService;

        OpenDialogCommand = ReactiveCommand.Create(AddNewApi);
        OpenSettingsCommand = ReactiveCommand.Create(NavigateSettings);
        EnterOverviewCommand = ReactiveCommand.Create<string>(EnterOverview);

#pragma warning disable CS8604 // Possible null reference argument.
        ApiKeys = new ObservableCollection<ApiKey>(settingsService.GetAllApiKeys());
#pragma warning restore CS8604 // Possible null reference argument.

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
        _navigationService.RequestNaviagtion<ServerOverviewViewModel>();
    }
}
