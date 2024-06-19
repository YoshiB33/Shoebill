using System;

namespace Shoebill.Services;

public interface INavigationService
{
    public Action<Type>? NavigationRequested { get; set; }
    public Action<Type>? MasterNavigationRequested { get; set; }
    public bool CanNavigateback { get; }
    public bool CanNavigateForward { get; }
    public void NavigateForward();
    public void NavigateBack();
    public void RequestNaviagtion<T>(bool isMasterPage) where T : ViewModels.ViewModelBase;
}
