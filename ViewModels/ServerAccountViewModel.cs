using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Shoebill.Helpers;
using Shoebill.Models.Api.Responses;
using Shoebill.Services;
using SukiUI.Controls;

namespace Shoebill.ViewModels;

public class ServerAccountViewModel : ViewModelBase
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
    private bool _canChangeEmail;
    public bool CanChangeEmail
    {
        get => _canChangeEmail;
        set => this.RaiseAndSetIfChanged(ref _canChangeEmail, value);
    }
    

    public string? OldEmail => _apiService.CurrentAccount?.Email;
    public bool CanNavigateBack => _navigationService.CanNavigateback;

    public ReactiveCommand<Unit, Unit> NavigateBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> UpdateEmailCommand { get; set; }

    private IApiService _apiService;
    private INavigationService _navigationService;
    public ServerAccountViewModel(INavigationService navigationService, IApiService apiService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        navigationService.NavigationRequested += OnNavigatedTo;

        this.WhenAnyValue(x => x.EmailText, x => x.PasswordText,
            (email, password) =>
                !string.IsNullOrWhiteSpace(email) &&
                !string.IsNullOrWhiteSpace(password) &&
                EmailHelper.IsEmailValid(email) &&
                (email != OldEmail)
            )
            .Subscribe(x => {
                CanChangeEmail = x;
            });
        
        NavigateBackCommand = ReactiveCommand.Create(NavigateBack);
        UpdateEmailCommand = ReactiveCommand.Create(UpdateEmail);
    }

    private async void OnNavigatedTo(Type page)
    {
        if (page == typeof(ServerAccountViewModel))
        {
            GetAccountDetails? account = null;
            try
            {
                account = await _apiService.GetAccountDetailsAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                SukiHost.ShowMessageBox(new SukiUI.Models.MessageBoxModel($"Error found: {(int?)ex.StatusCode}", 
                    $"An error was found when getting account details: {ex.Message}\n{ex.StackTrace}", SukiUI.Enums.NotificationType.Error), true);
                _navigationService.NavigateBack();
            }
            if (account is not null)
            {
                EmailText = account.Attributes.Email;
                _apiService.CurrentAccount = account.Attributes;
            }
        }
    }

    private void NavigateBack()
    {
        _navigationService.NavigateBack();
        EmailText = "";
    }

    private async void UpdateEmail()
    {
        try
        {
            await _apiService.UpdateAccountEmailAsync(EmailText, PasswordText);
            await SukiHost.ShowToast(new SukiUI.Models.ToastModel("Successfully updated email", $"Upadted email to {EmailText}", SukiUI.Enums.NotificationType.Success));
            EmailText = "";
            OnNavigatedTo(typeof(ServerAccountViewModel));
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.HttpRequestError);
            await SukiHost.ShowToast(new SukiUI.Models.ToastModel("Couldn't update email!", ex.Message, Type: SukiUI.Enums.NotificationType.Error));
        }
    }
}
