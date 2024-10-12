using System;
using Material.Icons;

namespace Shoebill.ViewModels.ServerSubpages;

public class ServerConsoleViewModel : ServerViewModelBase
{
    public override MaterialIconKind Icon { get; } = MaterialIconKind.Console;

    public override int Index { get; } = 1;

    public override string? DisplayName { get; } = "Console";
}
