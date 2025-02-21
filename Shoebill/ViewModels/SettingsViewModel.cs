using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Styling;
using ReactiveUI;
using Shoebill.Models;
using Shoebill.Services;
using Shoebill.ViewModels.Dialogs;
using SukiUI;
using SukiUI.Dialogs;

namespace Shoebill.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly ISukiDialogManager _dialogManager;
    private readonly ISettingsService _settingsService;
    private readonly SukiTheme _theme;
    private ThemeVariant _baseTheme;
    private bool _isThemeDark;
    private bool _isThemeLight;
    private bool _showBackButton = true;

    public SettingsViewModel(ISettingsService settingsService,
        ISukiDialogManager dialogManager)
    {
        _settingsService = settingsService;
        _dialogManager = dialogManager;

        _theme = SukiTheme.GetInstance();
        _baseTheme = _theme.ActiveBaseTheme;
        if (_baseTheme != ThemeVariant.Dark)
            _isThemeDark = true;
        else
            _isThemeLight = true;

        ApiKeys = new ObservableCollection<ApiKey>(settingsService.GetAllApiKeys() ?? []);

        RemoveApiCommand = ReactiveCommand.Create<string>(RemoveApiKey);
        AddApiCommand = ReactiveCommand.Create(AddApiKey);
        EditApiCommand = ReactiveCommand.Create<string>(EditApiKey);
        ToggleBaseThemeCommand = ReactiveCommand.Create(ToggleBaseTheme);

        settingsService.ApiKeyUpdated += (key, updateAction) =>
        {
            if (updateAction == KeyUpdatedAction.Added)
                ApiKeys.Add(key);
            else
                ApiKeys.Remove(key);
        };
    }

    public ReactiveCommand<string, Unit> RemoveApiCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AddApiCommand { get; set; }
    public ReactiveCommand<string, Unit> EditApiCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ToggleBaseThemeCommand { get; set; }
    public ObservableCollection<ApiKey> ApiKeys { get; set; }

    public bool IsThemeDark
    {
        get => _isThemeDark;
        set => this.RaiseAndSetIfChanged(ref _isThemeDark, value);
    }

    public bool IsThemeLight
    {
        get => _isThemeLight;
        set => this.RaiseAndSetIfChanged(ref _isThemeLight, value);
    }

    public bool ShowBackButton
    {
        get => _showBackButton;
        set => this.RaiseAndSetIfChanged(ref _showBackButton, value);
    }

    public ThemeVariant BaseTheme
    {
        get => _baseTheme;
        set => this.RaiseAndSetIfChanged(ref _baseTheme, value);
    }

    private async void RemoveApiKey(string name)
    {
        try
        {
            await _settingsService.RemoveApiAsync(ApiKeys.First(x => x.Name == name));
        }
        catch (Exception e)
        {
            _dialogManager.CreateDialog()
                .WithTitle("Error removing API key.")
                .WithContent($"Error removing API key: {e.Message}")
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    private void AddApiKey()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog => new CreateAccountViewModel(_settingsService, dialog))
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    private void EditApiKey(string name)
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog =>
                new CreateAccountViewModel(_settingsService, dialog, ApiKeys.First(x => x.Name == name)))
            .Dismiss().ByClickingBackground()
            .TryShow();
    }

    private void ToggleBaseTheme()
    {
        _theme.SwitchBaseTheme();
    }
}