﻿using System;
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

    private IApiService _apiService;
    private INavigationService _navigationService;
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
        EmailPasswordText = "";
        CurrentPasswordText = "";
        NewPasswordText = "";
        ConfirmPasswordText = "";
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
}
