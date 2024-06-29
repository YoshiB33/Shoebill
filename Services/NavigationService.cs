using System;
using System.Collections.Generic;
using Shoebill.Models;
using Shoebill.ViewModels;

namespace Shoebill.Services;

public class NavigationService() : INavigationService
{
    public Action<Type>? NavigationRequested { get; set; }
    public Action<Type>? MasterNavigationRequested { get; set; }
    public bool CanNavigateback 
    {
        get
        {
            if (_currentPage > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public bool CanNavigateForward 
    { 
        get
        {
            if (_currentPage != _history.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        } 
    }

    private readonly List<NavigationHistory> _history = [];
    private int _currentPage = 0;

    public void NavigateBack()
    {
        var page = _history[_currentPage - 2];
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
