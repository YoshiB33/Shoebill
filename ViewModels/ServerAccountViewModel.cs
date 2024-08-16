using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Shoebill.Helpers;
using Shoebill.Models.Api.Responses;
using Shoebill.Models.Api.Schemas;
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
    private string _emailPasswordText = "";
    [Required]
    public string EmailPasswordText
    {
        get => _emailPasswordText;
        set => this.RaiseAndSetIfChanged(ref _emailPasswordText, value);
    }
    private bool _canChangeEmail;
    public bool CanChangeEmail
    {
        get => _canChangeEmail;
        set => this.RaiseAndSetIfChanged(ref _canChangeEmail, value);
    }
    private string _currentPasswordText = "";
    [Required]
    public string CurrentPasswordText
    {
        get => _currentPasswordText;
        set => this.RaiseAndSetIfChanged(ref _currentPasswordText, value);
    }
    private string _newPasswordText = "";
    [Required]
    public string NewPasswordText
    {
        get => _newPasswordText;
        set => this.RaiseAndSetIfChanged(ref _newPasswordText, value);
    }
    private string _confirmPasswordText = "";
    [Required]
    public string ConfirmPasswordText
    {
        get => _confirmPasswordText;
        set => this.RaiseAndSetIfChanged(ref _confirmPasswordText, value);
    }
    private bool _canChangePassword;
    public bool CanChangePassword
    {
        get => _canChangePassword;
        set => this.RaiseAndSetIfChanged(ref _canChangePassword, value);
    }
    

    public string? OldEmail => _apiService.CurrentAccount?.Email;
    public bool CanNavigateBack => _navigationService.CanNavigateback;

    public ReactiveCommand<Unit, Unit> NavigateBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> UpdateEmailCommand { get; set; }
    public ReactiveCommand<Unit, Unit> UpdatePasswordCommand { get; set; }
    public ReactiveCommand<Unit, Unit> OpenCreateApiKeyDialogCommand { get; set; }
    public ReactiveCommand<string, Unit> RemoveApiKeyCommand { get; set; }

    private IApiService _apiService;
    private INavigationService _navigationService;
    public ObservableCollection<API_Key> ApiKeys { get; set; } = [];
    public ServerAccountViewModel(INavigationService navigationService, IApiService apiService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        navigationService.NavigationRequested += OnNavigatedTo;

        this.WhenAnyValue(x => x.EmailText, x => x.EmailPasswordText,
            (email, password) =>
                !string.IsNullOrEmpty(email) &&
                !string.IsNullOrEmpty(password) &&
                EmailHelper.IsEmailValid(email) &&
                (email != OldEmail)
            )
            .Subscribe(x => {
                CanChangeEmail = x;
            });
        this.WhenAnyValue(x => x.CurrentPasswordText, x => x.NewPasswordText, x => x.ConfirmPasswordText,
            (currentPassword, newPassword, confirmPasswordText) =>
                !string.IsNullOrEmpty(currentPassword) &&
                !string.IsNullOrEmpty(newPassword) &&
                !string.IsNullOrEmpty(confirmPasswordText) &&
                (currentPassword != newPassword) &&
                (newPassword == confirmPasswordText)
            )
            .Subscribe(x => {
                CanChangePassword = x;
            });
        
        NavigateBackCommand = ReactiveCommand.Create(NavigateBack);
        UpdateEmailCommand = ReactiveCommand.Create(UpdateEmail);
        UpdatePasswordCommand = ReactiveCommand.Create(UpdatePassword);
        OpenCreateApiKeyDialogCommand = ReactiveCommand.Create(OpenCreateApiKeyDialog);
        RemoveApiKeyCommand = ReactiveCommand.Create<string>(RemoveApiKey);
    }

    private async void OnNavigatedTo(Type page)
    {
        if (page == typeof(ServerAccountViewModel))
        {
            GetAccountDetails? account = null;
            ApiKeys.Clear();
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
                try
                {
                    var keysResponse = await _apiService.GetApiKeysAsync();
                    if (keysResponse is not null)
                    {
                        foreach (var key in keysResponse.Data)
                        {
                            ApiKeys.Add(key.Attributes);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                   Console.WriteLine(ex.Message);
                   Console.WriteLine(ex.StackTrace);
                   await SukiHost.ShowToast(new SukiUI.Models.ToastModel("Couldn't get api keys", ex.Message, SukiUI.Enums.NotificationType.Error)); 
                }

            }
        }
    }

    private void NavigateBack()
    {
        _navigationService.NavigateBack();
        EmailText = "";
        EmailPasswordText = "";
        CurrentPasswordText = "";
        NewPasswordText = "";
        ConfirmPasswordText = "";
        ApiKeys.Clear();
    }

    private async void UpdateEmail()
    {
        try
        {
            await _apiService.UpdateAccountEmailAsync(EmailText, EmailPasswordText);
            await SukiHost.ShowToast(new SukiUI.Models.ToastModel("Successfully updated email", $"Updated email to {EmailText}", SukiUI.Enums.NotificationType.Success));
            EmailText = "";
            OnNavigatedTo(typeof(ServerAccountViewModel)); // This line is to reload the information on this page
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            await SukiHost.ShowToast(new SukiUI.Models.ToastModel("Couldn't update email!", ex.Message, Type: SukiUI.Enums.NotificationType.Error));
        }
    }
    private async void UpdatePassword()
    {
        try
        {
            await _apiService.UpdateAccountPasswordAsync(CurrentPasswordText, NewPasswordText, ConfirmPasswordText);
            await SukiHost.ShowToast(new SukiUI.Models.ToastModel("Successfully updated password", $"Updated password to {NewPasswordText}", SukiUI.Enums.NotificationType.Success));
            CurrentPasswordText = "";
            NewPasswordText = "";
            ConfirmPasswordText = "";
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            await SukiHost.ShowToast(new SukiUI.Models.ToastModel("Couldn't update password!", ex.Message, Type: SukiUI.Enums.NotificationType.Error));
        }
    }

    private void OpenCreateApiKeyDialog()
        => SukiHost.ShowDialog(new CreateApiKeyViewModel(_apiService, _navigationService), allowBackgroundClose: true);

    private async void RemoveApiKey(string id)
    {
        Console.WriteLine("Deletes an api key! Key id: {0}", id);
        try
        {
            await _apiService.DeteteApiKeyAsync(id);
            await SukiHost.ShowToast(new SukiUI.Models.ToastModel($"Successfully deleted api key: {id}", SukiUI.Enums.NotificationType.Success));
            ApiKeys.Clear();
            try
            {
                var keysResponse = await _apiService.GetApiKeysAsync();
                if (keysResponse is not null)
                {
                    foreach (var key in keysResponse.Data)
                    {
                        ApiKeys.Add(key.Attributes);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                await SukiHost.ShowToast(new SukiUI.Models.ToastModel("Couldn't get api keys", ex.Message, SukiUI.Enums.NotificationType.Error)); 
            }
        }
        catch (HttpRequestException ex)
        {
            await SukiHost.ShowToast(new SukiUI.Models.ToastModel($"Couldn't detete api key: {id}", ex.Message, SukiUI.Enums.NotificationType.Error));
        }
    }
}
