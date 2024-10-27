using Microsoft.Extensions.DependencyInjection;
using Shoebill.ViewModels;
using Shoebill.ViewModels.ServerSubpages;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace Shoebill.Services;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<INavigationService, NavigationService>();
        collection.AddSingleton<ISettingsService, SettingsService>();
        collection.AddHttpClient<IApiService, ApiService>();
        collection.AddSingleton<IApiService, ApiService>();
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddSingleton<ViewModelBase, AccountsViewModel>();
        collection.AddSingleton<ViewModelBase, SettingsViewModel>();
        collection.AddSingleton<ViewModelBase, ServerOverviewViewModel>();
        collection.AddSingleton<ViewModelBase, ServerMasterViewModel>();
        collection.AddSingleton<ViewModelBase, ServerAccountViewModel>();
        collection.AddSingleton<ServerViewModelBase, ServerConsoleViewModel>();
        collection.AddSingleton<ISukiDialogManager, SukiDialogManager>();
        collection.AddSingleton<ISukiToastManager, SukiToastManager>();
    }
}