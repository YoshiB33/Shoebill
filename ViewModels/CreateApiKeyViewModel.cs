using System.ComponentModel.DataAnnotations;
using System.Reactive;
using ReactiveUI;
using Shoebill.Attributes;
using Shoebill.Services;

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

    public ReactiveCommand<Unit, Unit> SubmitCommand { get; set; }

    private IApiService _apiService { get; set; }

    public CreateApiKeyViewModel(IApiService apiService)
    {
        _apiService = apiService;
        SubmitCommand = ReactiveCommand.Create(Submit);
    }

    private void Submit()
    {
        IsBusy = true;
    }
}
