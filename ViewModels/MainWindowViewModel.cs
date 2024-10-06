using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using ReactiveUI;
using Shoebill.Services;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace Shoebill.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ISukiDialogManager DialogManager { get; }
    public ISukiToastManager ToastManager { get; }
    public IAvaloniaReadOnlyList<ViewModelBase> Pages;
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
        navigationService.NavigationRequested += NaviagtionRequested;
        navigationService.RequestNaviagtion<AccountsViewModel>(false);
    }

    private void NaviagtionRequested(Type pageType)
    {
        var page = Pages.FirstOrDefault(x => x.GetType() == pageType);
        if (page == null || ContentViewModel.GetType() == pageType) return;
        ContentViewModel = page;
    }
}
