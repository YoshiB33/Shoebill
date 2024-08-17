using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using ReactiveUI;
using Shoebill.Attributes;
using Shoebill.Services;
using SukiUI.Controls;
using SukiUI.Models;

namespace Shoebill.ViewModels.Dialogs;

public class CreateApiKeyViewModel : ViewModelBase
{
    private string _desctiptionText = string.Empty;
    private string _ipText = string.Empty;
    private bool _isBusy = false;

    [StringLength(maximumLength: 500, ErrorMessage = "The description cannot be longer than 500 characters")]
    public string DesctriptionText
    {
        get => _desctiptionText;
        set => this.RaiseAndSetIfChanged(ref _desctiptionText, value);
    }
    [Count('\n', 49, ErrorMessage = "Can't have more than 50 allowed ips")]
    public string IpText
    {
        get => _ipText;
        set => this.RaiseAndSetIfChanged(ref _ipText, value);
    }
    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
    public bool CanAddKey => _canAddKey.Value;

    private ObservableAsPropertyHelper<bool> _canAddKey;

    public ReactiveCommand<Unit, Unit> SubmitCommand { get; set; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }

    private IApiService _apiService { get; set; }
    private INavigationService _navigationService { get; set; }

    public CreateApiKeyViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        SubmitCommand = ReactiveCommand.Create(Submit);
        CancelCommand = ReactiveCommand.Create(Cancel);

        this.WhenAnyValue(x => x.DesctriptionText, x => x.IpText,
            (description, ips) =>
                description.Length <= 500 &&
                IpText.Count(x => x == '\n') <= 49
            ).ToProperty(this, x => x.CanAddKey, out _canAddKey);
    }

    private async void Submit()
    {
        IsBusy = true;
        var ips = IpText.Split(Environment.NewLine).Where(x => !string.IsNullOrWhiteSpace(x));
        try
        {
            await _apiService.CreateApiKeyAsync(DesctriptionText, ips);
        }
        catch (HttpRequestException ex)
        {
            await SukiHost.ShowToast(new ToastModel("Couldn't create an api key", ex.Message, SukiUI.Enums.NotificationType.Error));
        }
        _navigationService.NavigationRequested?.Invoke(typeof(ServerAccountViewModel));
        IsBusy = false;
        SukiHost.CloseDialog();
    }
    private void Cancel() =>
        SukiHost.CloseDialog();
}
