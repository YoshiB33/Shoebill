using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Reactive;
using ReactiveUI;
using Shoebill.Services;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace Shoebill.ViewModels.Dialogs;

public class CreateSshKeyViewModel : ViewModelBase
{
    private readonly ObservableAsPropertyHelper<bool> _canSubmit;
    private readonly ISukiDialog _dialog;
    private readonly ISukiToastManager _toastManager;
    private bool _isSubmitting;
    private string _name = string.Empty;
    private string _publicKey = string.Empty;

    public CreateSshKeyViewModel(IApiService apiService, INavigationService navigationService, ISukiDialog dialog,
        ISukiToastManager toastManager)
    {
        ApiService = apiService;
        NavigationService = navigationService;
        _dialog = dialog;
        _toastManager = toastManager;

        this.WhenAnyValue(x => x.Name, x => x.PublicKey,
            (name, key) =>
                !string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(PublicKey)
        ).ToProperty(this, x => x.CanSubmit, out _canSubmit);

        CancelCommand = ReactiveCommand.Create(Cancel);
        SubmitCommand = ReactiveCommand.Create(Submit);
    }

    [Required(ErrorMessage = "You must enter a name")]
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    [Required(ErrorMessage = "You must enter a valid SSH public key")]
    public string PublicKey
    {
        get => _publicKey;
        set => this.RaiseAndSetIfChanged(ref _publicKey, value);
    }

    public bool IsSubmitting
    {
        get => _isSubmitting;
        set => this.RaiseAndSetIfChanged(ref _isSubmitting, value);
    }

    public bool CanSubmit => _canSubmit.Value;

    public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SubmitCommand { get; set; }

    private IApiService ApiService { get; }
    private INavigationService NavigationService { get; }

    private void Cancel() =>
        _dialog.Dismiss();

    private async void Submit()
    {
        IsSubmitting = true;
        try
        {
            await ApiService.CreateSshKeyAsync(Name, PublicKey);
        }
        catch (HttpRequestException ex)
        {
            _toastManager.CreateToast()
                .WithTitle($"Couldn't create a new SSH key ({(int?)ex.StatusCode})")
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Dismiss().ByClicking()
                .Queue();
        }

        NavigationService.NavigationRequested?.Invoke(typeof(ServerAccountViewModel));
        IsSubmitting = false;
        _dialog.Dismiss();
    }
}