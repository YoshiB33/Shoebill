using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Reactive;
using ReactiveUI;
using Shoebill.Services;
using SukiUI.Controls;
using SukiUI.Enums;
using SukiUI.Models;

namespace Shoebill.ViewModels.Dialogs;

public class CreateSSHKeyViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private string _publicKey = string.Empty;
    private bool _isSubmitting = false;
    private readonly ObservableAsPropertyHelper<bool> _canSubmit;

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

    public IApiService _apiService { get; set; }
    public INavigationService _navigationService { get; set; }

    public CreateSSHKeyViewModel(IApiService apiService, INavigationService navigationService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        this.WhenAnyValue(x => x.Name, x => x.PublicKey,
            (name, key) =>
                !string.IsNullOrWhiteSpace(Name) &&
                !string.IsNullOrWhiteSpace(PublicKey)
            ).ToProperty(this, x => x.CanSubmit, out _canSubmit);

        CancelCommand = ReactiveCommand.Create(Cancel);
        SubmitCommand = ReactiveCommand.Create(Submit);
    }

    private void Cancel() =>
        SukiHost.CloseDialog();
    private async void Submit()
    {
        IsSubmitting = true;
        try
        {
            await _apiService.CreateSSHKeyAsync(Name, PublicKey);
        }
        catch (HttpRequestException ex)
        {
            await SukiHost.ShowToast(new ToastModel(Title: "Couldn't create a new SSH key", $"{ex.StackTrace}", NotificationType.Error));
        }
        _navigationService.NavigationRequested?.Invoke(typeof(ServerAccountViewModel));
        IsSubmitting = false;
        SukiHost.CloseDialog();
    }
}
