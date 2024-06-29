using System;
using Material.Icons;
using Shoebill.Services;

namespace Shoebill.ViewModels;

public class ServerAccountViewModel : ServerViewModelBase
{
    public INavigationService _navigationService;
    public ServerAccountViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        navigationService.MasterNavigationRequested += OnNavigatedTo;
    }

    public override MaterialIconKind Icon 
    {
        get => MaterialIconKind.AccountCircle;
    }
    public override int Index 
    {
        get => 7;
    }
    public override string? DisplayName 
    {
        get => "Accounts";
    }

    private void OnNavigatedTo(Type page)
    {
        if (page == typeof(ServerAccountViewModel))
        {
            Console.WriteLine("Navigated to Accounts page!");
        }
    }
}
