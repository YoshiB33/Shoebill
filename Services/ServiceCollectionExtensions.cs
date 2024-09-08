using Microsoft.Extensions.DependencyInjection;
using Shoebill.ViewModels;

namespace Shoebill.Services;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<INavigationService, NavigationService>();
        collection.AddSingleton<ISettingsService, SettingsService>();
        collection.AddSingleton<IApiService, ApiService>();
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddSingleton<ViewModelBase, AccountsViewModel>();
        collection.AddSingleton<ViewModelBase, SettingsViewModel>();
        collection.AddSingleton<ViewModelBase, ServerOverviewViewModel>();
        collection.AddSingleton<ViewModelBase, ServerMasterViewModel>();
        collection.AddSingleton<ViewModelBase, ServerAccountViewModel>();
    }
}