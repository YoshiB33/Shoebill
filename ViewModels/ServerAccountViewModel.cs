using System;
using System.ComponentModel.DataAnnotations;
using Material.Icons;
using ReactiveUI;
using Shoebill.Services;

namespace Shoebill.ViewModels;

public class ServerAccountViewModel : ServerViewModelBase
{
    private string _emailText = "";
    [EmailAddress]
    public string EmailText
    {
        get => _emailText;
        set => this.RaiseAndSetIfChanged(ref _emailText, value);
    }
    private string _passwordText = "";
    [Required]
    public string PasswordText
    {
        get => _passwordText;
        set => this.RaiseAndSetIfChanged(ref _passwordText, value);
    }
    
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
        get => "Account";
    }

    private void OnNavigatedTo(Type page)
    {
        if (page == typeof(ServerAccountViewModel))
        {
            Console.WriteLine("Navigated to Accounts page!");
        }
    }
}
