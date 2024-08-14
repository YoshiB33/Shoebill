using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using Shoebill.Attributes;
using Shoebill.Services;
using SukiUI.Controls;

namespace Shoebill.ViewModels;

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

    public CreateApiKeyViewModel(IApiService apiService)
    {
        _apiService = apiService;
        SubmitCommand = ReactiveCommand.Create(Submit);
        CancelCommand = ReactiveCommand.Create(Cancel);

        this.WhenAnyValue(x => x.DesctriptionText, x => x.IpText,
            (description, ips) =>
                description.Length <= 500 &&
                IpText.Count(x => x == '\n') <= 49
            ).ToProperty(this, x => x.CanAddKey, out _canAddKey);
    }

    private void Submit()
    {
        IsBusy = true;
    }
    private void Cancel()
    {
        SukiHost.CloseDialog();
    }
}
