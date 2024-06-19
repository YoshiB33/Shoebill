using Microsoft.Extensions.DependencyInjection;
using Shoebill.ViewModels;

namespace Shoebill.Services;

public static class ServiceCollectionExtensions {
    public static void AddCommonServices(this IServiceCollection collection) {
        collection.AddSingleton<INavigationService, NavigationService>();
        collection.AddSingleton<ISettingsService, SettingsService>();
        collection.AddSingleton<IApiService, ApiService>();
        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<ViewModelBase, AccountsViewModel>();
        collection.AddTransient<ViewModelBase, SettingsViewModel>();
        collection.AddTransient<ViewModelBase, ServerOverviewViewModel>();
        collection.AddTransient<ViewModelBase, ServerMasterViewModel>();
    }
}