using System;
using System.Collections.Generic;
using Shoebill.ViewModels;

namespace Shoebill.Services;

public class NavigationService() : INavigationService
{
    public Action<Type>? NavigationRequested { get; set; }
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

    private readonly List<Type> _history = [typeof(AccountsViewModel)];
    private int _currentPage = 0;

    public void NavigateBack()
    {
        NavigationRequested?.Invoke(_history[_currentPage - 1]);
        _currentPage--;
    }

    public void NavigateForward()
    {
        NavigationRequested?.Invoke(_history[_currentPage + 1]);
        _currentPage++;
    }

    public void RequestNaviagtion<T>() where T : ViewModelBase
    {
        NavigationRequested?.Invoke(typeof(T));
        _history.Add(typeof(T));
        _currentPage++;
    }
}
