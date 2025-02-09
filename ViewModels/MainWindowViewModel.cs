using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Avalonia.Collections;
using ReactiveUI;
using Shoebill.Services;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace Shoebill.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IAvaloniaReadOnlyList<ViewModelBase> _pages;
    private bool _canGoBack;
    private ViewModelBase _contentViewModel = new();

    public MainWindowViewModel(INavigationService navigationService, IEnumerable<ViewModelBase> pages,
        ISukiDialogManager dialogManager, ISukiToastManager toastManager)
    {
        _pages = new AvaloniaList<ViewModelBase>(pages);
        DialogManager = dialogManager;
        ToastManager = toastManager;
        NavigationService = navigationService;
        navigationService.NavigationRequested += NavigationRequested;
        navigationService.RequestNavigation<AccountsViewModel>();

        NavigateBackCommand = ReactiveCommand.Create(NavigateBack);
        NavigateSettingsCommand = ReactiveCommand.Create(NavigateSettings);
    }

    public bool CanGoBack
    {
        get => _canGoBack;
        set => this.RaiseAndSetIfChanged(ref _canGoBack, value);
    }

    public ReactiveCommand<Unit, Unit> NavigateBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NavigateSettingsCommand { get; set; }
    public ISukiDialogManager DialogManager { get; }
    public ISukiToastManager ToastManager { get; }
    private INavigationService NavigationService { get; }

    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    private void NavigationRequested(Type pageType)
    {
        CanGoBack = NavigationService.CanNavigateBack;
        var page = _pages.FirstOrDefault(x => x.GetType() == pageType);
        if (page == null || ContentViewModel.GetType() == pageType) return;
        ContentViewModel = page;
    }

    private void NavigateBack()
    {
        NavigationService.NavigateBack();
    }

    private void NavigateSettings()
    {
        NavigationService.RequestNavigation<SettingsViewModel>();
    }
}