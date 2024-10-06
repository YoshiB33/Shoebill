using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Collections;
using Avalonia.Styling;
using ReactiveUI;
using Shoebill.Models;
using Shoebill.Services;
using Shoebill.ViewModels.Dialogs;
using SukiUI;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Models;

namespace Shoebill.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private bool _showBackButton = true;
    private readonly SukiTheme _theme;
    private ThemeVariant _baseTheme;
    private bool _isThemeDark;
    private bool _isThemeLight;
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;
    private readonly ISukiDialogManager _dialogManager;
    public ReactiveCommand<Unit, Unit> NavigateBackCommand { get; set; }
    public ReactiveCommand<string, Unit> RemoveApiCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AddApiCommand { get; set; }
    public ReactiveCommand<string, Unit> EditApiCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ToggleBaseThemeCommand { get; set; }
    public ObservableCollection<ApiKey> ApiKeys { get; set; }
    public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; set; }
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
    public SettingsViewModel(ISettingsService settingsService, INavigationService navigationService, ISukiDialogManager dialogManager)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;
        _dialogManager = dialogManager;

        _theme = SukiTheme.GetInstance();
        Themes = _theme.ColorThemes;
        _baseTheme = _theme.ActiveBaseTheme;
        if (_baseTheme != ThemeVariant.Dark)
        {
            _isThemeDark = true;
        }
        else
        {
            _isThemeLight = true;
        }

#pragma warning disable CS8604 // Possible null reference argument.
        ApiKeys = new ObservableCollection<ApiKey>(settingsService.GetAllApiKeys());
#pragma warning restore CS8604 // Possible null reference argument.

        NavigateBackCommand = ReactiveCommand.Create(NavigateBack);
        RemoveApiCommand = ReactiveCommand.Create<string>(RemoveApiKey);
        AddApiCommand = ReactiveCommand.Create(AddApiKey);
        EditApiCommand = ReactiveCommand.Create<string>(EditApiKey);
        ToggleBaseThemeCommand = ReactiveCommand.Create(ToggleBaseTheme);

        settingsService.ApiKeyUpdated += (key, updateAction) =>
        {
            if (updateAction == KeyUpdatedAction.Added)
            {
                ApiKeys.Add(key);
            }
            else
            {
                ApiKeys.Remove(key);
            }
        };
    }

    private async void RemoveApiKey(string Name)
    {
        await _settingsService.RemoveApiAsync(ApiKeys.Where(x => x.Name == Name).First());
    }
    private void AddApiKey()
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog => new CreateAccountViewModel(_settingsService, dialog))
            .Dismiss().ByClickingBackground()
            .TryShow();
    }
    private void EditApiKey(string Name)
    {
        _dialogManager.CreateDialog()
            .WithViewModel(dialog => new CreateAccountViewModel(_settingsService, dialog, ApiKeys.Where(x => x.Name == Name).First()))
            .Dismiss().ByClickingBackground()
            .TryShow();
    }
    private void NavigateBack()
    {
        _navigationService.NavigateBack();
    }

    private void ToggleBaseTheme() =>
        _theme.SwitchBaseTheme();
}
