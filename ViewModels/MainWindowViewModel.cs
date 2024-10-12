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

public partial class MainWindowViewModel : ViewModelBase
{
    private bool _canGoBack = false;
    public bool CanGoBack
    {
        get => _canGoBack;
        set => this.RaiseAndSetIfChanged(ref _canGoBack, value);
    }
    public ReactiveCommand<Unit, Unit> NavigateBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NavigateSettingsCommand { get; set; }
    public ISukiDialogManager DialogManager { get; }
    public ISukiToastManager ToastManager { get; }
    public IAvaloniaReadOnlyList<ViewModelBase> Pages;
    public INavigationService _navigationService { get; set; }
    private ViewModelBase _contentViewModel = new();
    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }
    public MainWindowViewModel(INavigationService navigationService, IEnumerable<ViewModelBase> pages, ISukiDialogManager dialogManager, ISukiToastManager toastManager)
    {
        Pages = new AvaloniaList<ViewModelBase>(pages);
        DialogManager = dialogManager;
        ToastManager = toastManager;
        _navigationService = navigationService;
        navigationService.NavigationRequested += NaviagtionRequested;
        navigationService.RequestNaviagtion<AccountsViewModel>();

        NavigateBackCommand = ReactiveCommand.Create(NavigateBack);
        NavigateSettingsCommand = ReactiveCommand.Create(NavigateSettings);
    }

    private void NaviagtionRequested(Type pageType)
    {
        CanGoBack = _navigationService.CanNavigateBack;
        var page = Pages.FirstOrDefault(x => x.GetType() == pageType);
        if (page == null || ContentViewModel.GetType() == pageType) return;
        ContentViewModel = page;
    }

    private void NavigateBack()
        => _navigationService.NavigateBack();

    private void NavigateSettings()
        => _navigationService.RequestNaviagtion<SettingsViewModel>();
}
