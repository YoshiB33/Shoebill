using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using Avalonia.Controls.Notifications;
using ReactiveUI;
using Shoebill.Attributes;
using Shoebill.Services;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace Shoebill.ViewModels.Dialogs;

public class CreateApiKeyViewModel : ViewModelBase
{
    private readonly ObservableAsPropertyHelper<bool> _canAddKey;
    private readonly ISukiDialog _dialog;
    private readonly ISukiToastManager _toastManager;
    private string _desctiptionText = string.Empty;
    private string _ipText = string.Empty;
    private bool _isBusy;

    public CreateApiKeyViewModel(IApiService apiService, INavigationService navigationService, ISukiDialog sukiDialog,
        ISukiToastManager toastManager)
    {
        ApiService = apiService;
        NavigationService = navigationService;
        _dialog = sukiDialog;
        _toastManager = toastManager;

        SubmitCommand = ReactiveCommand.Create(Submit);
        CancelCommand = ReactiveCommand.Create(Cancel);

        this.WhenAnyValue(x => x.DesctriptionText, x => x.IpText,
            (description, ips) =>
                description.Length <= 500 &&
                IpText.Count(x => x == '\n') <= 49
        ).ToProperty(this, x => x.CanAddKey, out _canAddKey);
    }

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

    public ReactiveCommand<Unit, Unit> SubmitCommand { get; set; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }

    private IApiService ApiService { get; }
    private INavigationService NavigationService { get; }

    private async void Submit()
    {
        IsBusy = true;
        var ips = IpText.Split(Environment.NewLine).Where(x => !string.IsNullOrWhiteSpace(x));
        try
        {
            await ApiService.CreateApiKeyAsync(DesctriptionText, ips);
        }
        catch (HttpRequestException ex)
        {
            _toastManager.CreateToast()
                .WithTitle("Couldn't create an API key")
                .WithContent(ex.Message)
                .OfType(NotificationType.Error)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
        }

        NavigationService.NavigationRequested?.Invoke(typeof(ServerAccountViewModel));
        IsBusy = false;
        _dialog.Dismiss();
    }

    private void Cancel() =>
        _dialog.Dismiss();
}