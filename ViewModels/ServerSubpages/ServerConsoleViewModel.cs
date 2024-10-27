using Material.Icons;

namespace Shoebill.ViewModels.ServerSubpages;

public class ServerConsoleViewModel : ServerViewModelBase
{
    public override MaterialIconKind Icon => MaterialIconKind.Console;
    public override int Index => 1;
    public override string? DisplayName => "Console";
}