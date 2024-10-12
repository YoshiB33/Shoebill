using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls.Notifications;
using ReactiveUI;
using Shoebill.Helpers;
using Shoebill.Models.Api.Responses;
using Shoebill.Models.Api.Schemas;
using Shoebill.Services;
using Shoebill.ViewModels.Dialogs;
using SukiUI.Dialogs;
using SukiUI.Toasts;

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
    public bool CanNavigateBack => _navigationService.CanNavigateBack;

    public ReactiveCommand<Unit, Unit> NavigateBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> UpdateEmailCommand { get; set; }
    public ReactiveCommand<Unit, Unit> UpdatePasswordCommand { get; set; }
    public ReactiveCommand<Unit, Unit> OpenCreateApiKeyDialogCommand { get; set; }
    public ReactiveCommand<string, Unit> RemoveApiKeyCommand { get; set; }
    public ReactiveCommand<string, Unit> ShowApiKeyInfoCommand { get; set; }
    public ReactiveCommand<Unit, Unit> OpenCreateSSHKeyDialogCommand { get; set; }
    public ReactiveCommand<string, Unit> RemoveSSHKeyCommand { get; set; }
    public ReactiveCommand<string, Unit> ShowSSHKeyInfoCommand { get; set; }

    private IApiService _apiService;
    private INavigationService _navigationService;
    private readonly ISukiDialogManager _dialogManager;
    private readonly ISukiToastManager _toastManager;
    public ObservableCollection<API_Key> ApiKeys { get; set; } = [];
    public ObservableCollection<SSH_Key> SSH_Keys { get; set; } = [];
    public ServerAccountViewModel(INavigationService navigationService, IApiService apiService, ISukiDialogManager dialogManager, ISukiToastManager toastManager)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        _dialogManager = dialogManager;
        _toastManager = toastManager;
        navigationService.NavigationRequested += OnNavigatedTo;

        this.WhenAnyValue(x => x.EmailText, x => x.EmailPasswordText,
            (email, password) =>
                !string.IsNullOrEmpty(email) &&
                !string.IsNullOrEmpty(password) &&
                EmailHelper.IsEmailValid(email) &&
                (email != OldEmail)
            )
            .Subscribe(x =>
            {
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
            .Subscribe(x =>
            {
                CanChangePassword = x;
            });

        NavigateBackCommand = ReactiveCommand.Create(NavigateBack);
        UpdateEmailCommand = ReactiveCommand.Create(UpdateEmail);
        UpdatePasswordCommand = ReactiveCommand.Create(UpdatePassword);
        OpenCreateApiKeyDialogCommand = ReactiveCommand.Create(OpenCreateApiKeyDialog);
        RemoveApiKeyCommand = ReactiveCommand.Create<string>(RemoveApiKey);
        ShowApiKeyInfoCommand = ReactiveCommand.Create<string>(ShowApiKeyInfo);
        OpenCreateSSHKeyDialogCommand = ReactiveCommand.Create(OpenCreateSSHKeyDialog);
        RemoveSSHKeyCommand = ReactiveCommand.Create<string>(RemoveSSHKey);
        ShowSSHKeyInfoCommand = ReactiveCommand.Create<string>(ShowSSHKeyInfo);
    }

    private async void OnNavigatedTo(Type page)
    {
        if (page == typeof(ServerAccountViewModel))
        {
            GetAccountDetails? account = null;
            ApiKeys.Clear();
            SSH_Keys.Clear();
            try
            {
                account = await _apiService.GetAccountDetailsAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                _toastManager.CreateToast()
                    .WithTitle($"Can't get account details ({(int?)ex.StatusCode})")
                    .WithContent($"An error was found when getting account details: {ex.Message}")
                    .OfType(NotificationType.Error)
                    .Dismiss().After(TimeSpan.FromSeconds(5))
                    .Dismiss().ByClicking()
                    .Queue();
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
                    _toastManager.CreateToast()
                        .WithTitle($"Couldn't get api keys ({(int?)ex.StatusCode})")
                        .WithContent(ex.Message)
                        .OfType(NotificationType.Error)
                        .Dismiss().After(TimeSpan.FromSeconds(5))
                        .Dismiss().ByClicking()
                        .Queue();
                }
                try
                {
                    var ssh_keys = await _apiService.GetSSHKeysAsync();
                    if (ssh_keys is not null)
                    {
                        foreach (var key in ssh_keys.Data)
                        {
                            SSH_Keys.Add(key.Attributes);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    _toastManager.CreateToast()
                        .WithTitle("Couldn't get SSH keys")
                        .WithContent(ex.Message)
                        .OfType(NotificationType.Error)
                        .Dismiss().After(TimeSpan.FromSeconds(5))
                        .Dismiss().ByClicking()
                        .Queue();
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
            _toastManager.CreateToast()
                .WithTitle("Successfully updated email")
                .WithContent($"Updated email to {EmailText}")
                .OfType(NotificationType.Success)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
            EmailText = "";
            OnNavigatedTo(typeof(ServerAccountViewModel)); // This line is to reload the information on this page
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            _toastManager.CreateToast()
                .WithTitle($"Couldn't update email ({(int?)ex.StatusCode})")
                .WithContent(ex.Message)
                .OfType(NotificationType.Error)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
        }
    }
    private async void UpdatePassword()
    {
        try
        {
            await _apiService.UpdateAccountPasswordAsync(CurrentPasswordText, NewPasswordText, ConfirmPasswordText);
            _toastManager.CreateToast()
                .WithTitle("Successfully updated password")
                .OfType(NotificationType.Success)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
            CurrentPasswordText = "";
            NewPasswordText = "";
            ConfirmPasswordText = "";
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            _toastManager.CreateToast()
                .WithTitle($"Couldn't update password ({(int?)ex.StatusCode})")
                .WithContent(ex.Message)
                .OfType(NotificationType.Error)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
        }
    }

    private void OpenCreateApiKeyDialog()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog => new CreateApiKeyViewModel(_apiService, _navigationService, dialog, _toastManager))
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    private async void RemoveApiKey(string id)
    {
        Console.WriteLine("Deletes an api key! Key id: {0}", id);
        try
        {
            await _apiService.DeteteApiKeyAsync(id);
            _toastManager.CreateToast()
                .WithTitle("Successfully deleted api key")
                .OfType(NotificationType.Success)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
            var apiKey = ApiKeys.Where(x => x.Identifier == id).FirstOrDefault();
            if (apiKey is not null)
            {
                ApiKeys.Remove(apiKey);
            }
        }
        catch (HttpRequestException ex)
        {
            _toastManager.CreateToast()
                .WithTitle($"Couldn't update password ({(int?)ex.StatusCode})")
                .WithContent(ex.Message)
                .OfType(NotificationType.Error)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
        }
    }
    private void ShowApiKeyInfo(string Id)
    {
        var key = ApiKeys.Where(x => x.Identifier == Id).FirstOrDefault();
        if (key is not null)
        {
            _dialogManager.CreateDialog()
                .WithViewModel(dialog => new ApiKeyInfoViewModel(key))
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
        else
        {
            _toastManager.CreateToast()
                .WithTitle($"Couldn't display api key info")
                .OfType(NotificationType.Error)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
        }
    }

    private void OpenCreateSSHKeyDialog()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog => new CreateSSHKeyViewModel(_apiService, _navigationService, dialog, _toastManager))
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    private async void RemoveSSHKey(string Fingerprint)
    {
        Console.WriteLine("Deletes an SSH key! Key fingerprint: {0}", Fingerprint);
        try
        {
            await _apiService.DeteteSSHKeyAsync(Fingerprint);
            _toastManager.CreateToast()
                .WithTitle("Successfully deleted SSH key")
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Queue();
            var sshKey = SSH_Keys.Where(x => x.Fingerprint == Fingerprint).FirstOrDefault();
            if (sshKey is not null)
            {
                SSH_Keys.Remove(sshKey);
            }
        }
        catch (HttpRequestException ex)
        {
            _toastManager.CreateToast()
                .WithTitle($"Couldn't delete SSH key ({(int?)ex.StatusCode})")
                .OfType(NotificationType.Error)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
        }
    }
    private void ShowSSHKeyInfo(string Fingerprint)
    {
        var key = SSH_Keys.Where(x => x.Fingerprint == Fingerprint).FirstOrDefault();
        if (key is not null)
        {
            _dialogManager.CreateDialog()
                .WithViewModel(dialog => new SSHKeyInfoViewModel(key))
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
        else
        {
            _toastManager.CreateToast()
                .WithTitle($"Couldn't display SSH key info")
                .OfType(NotificationType.Error)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
        }
    }
}
