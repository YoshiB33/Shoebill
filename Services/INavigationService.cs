using System;
using Shoebill.ViewModels;

namespace Shoebill.Services;

public interface INavigationService
{
    public Action<Type>? NavigationRequested { get; set; }
    public Action<Type>? MasterNavigationRequested { get; set; }
    public bool CanNavigateBack { get; }
    public bool CanNavigateForward { get; set; }
    public void NavigateForward();
    public void NavigateBack();
    public void RequestNavigation<T>() where T : ViewModelBase;
}