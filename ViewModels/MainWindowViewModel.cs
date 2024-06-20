using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using ReactiveUI;
using Shoebill.Services;

namespace Shoebill.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public IAvaloniaReadOnlyList<ViewModelBase> Pages;
    private ViewModelBase _contentViewModel = new();
    public ViewModelBase ContentViewModel
    {
        get => _contentViewModel;
        private set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }
    public MainWindowViewModel(INavigationService navigationService, IEnumerable<ViewModelBase> pages)
    {
        Pages = new AvaloniaList<ViewModelBase>(pages);
        navigationService.NavigationRequested += NaviagtionRequested;
        navigationService.RequestNaviagtion<AccountsViewModel>(false);
    }

    private void NaviagtionRequested(Type pageType)
    {
        var page = Pages.FirstOrDefault(x => x.GetType() == pageType);
        if (page == null || ContentViewModel.GetType() == pageType)  return;
        ContentViewModel = page;
    }
}
