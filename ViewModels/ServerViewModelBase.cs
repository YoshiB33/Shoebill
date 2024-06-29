using Material.Icons;

namespace Shoebill.ViewModels;

public abstract class ServerViewModelBase : ViewModelBase
{
    public abstract MaterialIconKind Icon { get; }
    public abstract int Index { get; }
    public abstract string? DisplayName { get; }
}
