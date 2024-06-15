using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using Avalonia.Collections;
using Avalonia.Styling;
using ReactiveUI;
using Shoebill.Models;
using Shoebill.Services;
using SukiUI;
using SukiUI.Controls;
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
    public SettingsViewModel(ISettingsService settingsService, INavigationService navigationService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;

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

        _theme.OnBaseThemeChanged += variant =>
        {
            BaseTheme = variant;
            SukiHost.ShowToast("Succesfully changed theme", $"Successfully changed theme to {variant}");
        };

#pragma warning disable CS8604 // Possible null reference argument.
        ApiKeys = new ObservableCollection<ApiKey>(settingsService.GetAllApiKeys());
#pragma warning restore CS8604 // Possible null reference argument.

        NavigateBackCommand = ReactiveCommand.Create(NavigateBack);
        RemoveApiCommand = ReactiveCommand.Create<string>(RemoveApiKey);
        AddApiCommand = ReactiveCommand.Create(AddApiKey);
        EditApiCommand = ReactiveCommand.Create<string>(EditApiKey);
        ToggleBaseThemeCommand  = ReactiveCommand.Create(ToggleBaseTheme);

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
        SukiHost.ShowDialog(new CreateAccountViewModel(_settingsService), allowBackgroundClose: true);
    }
    private void EditApiKey(string Name)
    {
        SukiHost.ShowDialog(new CreateAccountViewModel(_settingsService, ApiKeys.Where(x => x.Name == Name).First()));
    }
    private void NavigateBack()
    {
        _navigationService.NavigateBack();
    }

    private void ToggleBaseTheme() =>
        _theme.SwitchBaseTheme();
}
