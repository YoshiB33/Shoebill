using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reactive;
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
    private readonly IApiService _apiService;
    private readonly ISukiDialogManager _dialogManager;
    private readonly INavigationService _navigationService;
    private readonly ISukiToastManager _toastManager;
    private bool _canChangeEmail;
    private bool _canChangePassword;
    private string _confirmPasswordText = "";
    private string _currentPasswordText = "";
    private string _emailPasswordText = "";
    private string _emailText = "";
    private string _newPasswordText = "";

    public ServerAccountViewModel(INavigationService navigationService, IApiService apiService,
        ISukiDialogManager dialogManager, ISukiToastManager toastManager)
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
                    email != OldEmail
            )
            .Subscribe(x => { CanChangeEmail = x; });
        this.WhenAnyValue(x => x.CurrentPasswordText, x => x.NewPasswordText, x => x.ConfirmPasswordText,
                (currentPassword, newPassword, confirmPasswordText) =>
                    !string.IsNullOrEmpty(currentPassword) &&
                    !string.IsNullOrEmpty(newPassword) &&
                    !string.IsNullOrEmpty(confirmPasswordText) &&
                    currentPassword != newPassword &&
                    newPassword == confirmPasswordText
            )
            .Subscribe(x => { CanChangePassword = x; });

        UpdateEmailCommand = ReactiveCommand.Create(UpdateEmail);
        UpdatePasswordCommand = ReactiveCommand.Create(UpdatePassword);
        OpenCreateApiKeyDialogCommand = ReactiveCommand.Create(OpenCreateApiKeyDialog);
        RemoveApiKeyCommand = ReactiveCommand.Create<string>(RemoveApiKey);
        ShowApiKeyInfoCommand = ReactiveCommand.Create<string>(ShowApiKeyInfo);
        OpenCreateSshKeyDialogCommand = ReactiveCommand.Create(OpenCreateSshKeyDialog);
        RemoveSshKeyCommand = ReactiveCommand.Create<string>(RemoveSshKey);
        ShowSshKeyInfoCommand = ReactiveCommand.Create<string>(ShowSshKeyInfo);
    }

    [EmailAddress]
    public string EmailText
    {
        get => _emailText;
        set => this.RaiseAndSetIfChanged(ref _emailText, value);
    }

    [Required]
    public string EmailPasswordText
    {
        get => _emailPasswordText;
        set => this.RaiseAndSetIfChanged(ref _emailPasswordText, value);
    }

    public bool CanChangeEmail
    {
        get => _canChangeEmail;
        set => this.RaiseAndSetIfChanged(ref _canChangeEmail, value);
    }

    [Required]
    public string CurrentPasswordText
    {
        get => _currentPasswordText;
        set => this.RaiseAndSetIfChanged(ref _currentPasswordText, value);
    }

    [Required]
    public string NewPasswordText
    {
        get => _newPasswordText;
        set => this.RaiseAndSetIfChanged(ref _newPasswordText, value);
    }

    [Required]
    public string ConfirmPasswordText
    {
        get => _confirmPasswordText;
        set => this.RaiseAndSetIfChanged(ref _confirmPasswordText, value);
    }

    public bool CanChangePassword
    {
        get => _canChangePassword;
        set => this.RaiseAndSetIfChanged(ref _canChangePassword, value);
    }


    private string? OldEmail => _apiService.CurrentAccount?.Email;
    public bool CanNavigateBack => _navigationService.CanNavigateBack;

    public ReactiveCommand<Unit, Unit> UpdateEmailCommand { get; set; }
    public ReactiveCommand<Unit, Unit> UpdatePasswordCommand { get; set; }
    public ReactiveCommand<Unit, Unit> OpenCreateApiKeyDialogCommand { get; set; }
    public ReactiveCommand<string, Unit> RemoveApiKeyCommand { get; set; }
    public ReactiveCommand<string, Unit> ShowApiKeyInfoCommand { get; set; }
    public ReactiveCommand<Unit, Unit> OpenCreateSshKeyDialogCommand { get; set; }
    public ReactiveCommand<string, Unit> RemoveSshKeyCommand { get; set; }
    public ReactiveCommand<string, Unit> ShowSshKeyInfoCommand { get; set; }
    public ObservableCollection<API_Key> ApiKeys { get; set; } = [];
    public ObservableCollection<SSH_Key> SshKeys { get; set; } = [];

    private async void OnNavigatedTo(Type page)
    {
        if (page != typeof(ServerAccountViewModel)) return;

        GetAccountDetails? account = null;
        ApiKeys.Clear();
        SshKeys.Clear();
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

        if (account is null) return;

        EmailText = account.Attributes.Email;
        _apiService.CurrentAccount = account.Attributes;
        try
        {
            var keysResponse = await _apiService.GetApiKeysAsync();
            if (keysResponse is not null)
                foreach (var key in keysResponse.Data)
                    ApiKeys.Add(key.Attributes);
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
            var sshKeys = await _apiService.GetSshKeysAsync();
            if (sshKeys is not null)
                foreach (var key in sshKeys.Data)
                    SshKeys.Add(key.Attributes);
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
            await _apiService.DeleteApiKeyAsync(id);
            _toastManager.CreateToast()
                .WithTitle("Successfully deleted api key")
                .OfType(NotificationType.Success)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
            var apiKey = ApiKeys.FirstOrDefault(x => x.Identifier == id);
            if (apiKey is not null) ApiKeys.Remove(apiKey);
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

    private void ShowApiKeyInfo(string id)
    {
        var key = ApiKeys.FirstOrDefault(x => x.Identifier == id);
        if (key is not null)
            _dialogManager.CreateDialog()
                .WithViewModel(_ => new ApiKeyInfoViewModel(key))
                .Dismiss().ByClickingBackground()
                .TryShow();
        else
            _toastManager.CreateToast()
                .WithTitle("Couldn't display api key info")
                .OfType(NotificationType.Error)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
    }

    private void OpenCreateSshKeyDialog()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog => new CreateSshKeyViewModel(_apiService, _navigationService, dialog, _toastManager))
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    private async void RemoveSshKey(string fingerprint)
    {
        Console.WriteLine("Deletes an SSH key! Key fingerprint: {0}", fingerprint);
        try
        {
            await _apiService.DeleteSshKeyAsync(fingerprint);
            _toastManager.CreateToast()
                .WithTitle("Successfully deleted SSH key")
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Queue();
            var sshKey = SshKeys.FirstOrDefault(x => x.Fingerprint == fingerprint);
            if (sshKey is not null) SshKeys.Remove(sshKey);
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

    private void ShowSshKeyInfo(string fingerprint)
    {
        var key = SshKeys.FirstOrDefault(x => x.Fingerprint == fingerprint);
        if (key is not null)
            _dialogManager.CreateDialog()
                .WithViewModel(_ => new SshKeyInfoViewModel(key))
                .Dismiss().ByClickingBackground()
                .TryShow();
        else
            _toastManager.CreateToast()
                .WithTitle("Couldn't display SSH key info")
                .OfType(NotificationType.Error)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
    }
}