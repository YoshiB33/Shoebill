using System;
using Shoebill.Models.Api.Schemas;

namespace Shoebill.ViewModels.Dialogs;

public class SshKeyInfoViewModel(SSH_Key key) : ViewModelBase
{
    public string Name { get; } = key.Name;
    public string Fingerprint { get; } = key.Fingerprint;
    public string PublicKey { get; } = key.Public_key;
    public DateTime CreatedAt { get; } = key.Created_at;
}