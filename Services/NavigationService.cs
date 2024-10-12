using System;
using System.Collections.Generic;
using Shoebill.Models;
using Shoebill.ViewModels;

namespace Shoebill.Services;

public class NavigationService() : INavigationService
{
    public Action<Type>? NavigationRequested { get; set; }
    public Action<Type>? MasterNavigationRequested { get; set; }
    public bool CanNavigateBack { get; set; } = false;
    public bool CanNavigateForward { get; set; } = false;

    private readonly List<NavigationHistory> _history = [];
    private int _currentPage = 0;

    public void NavigateBack()
    {
        var page = _history[_currentPage - 2];
        _history.RemoveAt(_currentPage - 1);
        if (_history.Count <= 1)
        {
            CanNavigateBack = false;
        }
        if (!page.IsMasterPage)
        {
            NavigationRequested?.Invoke(page.Page);
        }
        else
        {
            MasterNavigationRequested?.Invoke(page.Page);
        }
        _currentPage--;
    }

    public void NavigateForward()
    {
        var page = _history[_currentPage++];
        if (!page.IsMasterPage)
        {
            NavigationRequested?.Invoke(page.Page);
        }
        else
        {
            MasterNavigationRequested?.Invoke(page.Page);
        }
        _currentPage++;
    }

    public void RequestNaviagtion<T>(bool isMasterPage) where T : ViewModelBase
    {
        if (_history.Count > 0)
        {
            CanNavigateBack = true;
        }
        if (!isMasterPage)
        {
            NavigationRequested?.Invoke(typeof(T));
        }
        else
        {
            MasterNavigationRequested?.Invoke(typeof(T));
        }
        _history.Add(new NavigationHistory(typeof(T), isMasterPage));
        _currentPage++;
    }
}
