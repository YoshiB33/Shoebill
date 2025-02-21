using System;
using System.Collections.Generic;
using Shoebill.Models.Api.Schemas;

namespace Shoebill.ViewModels.Dialogs;

public class ApiKeyInfoViewModel(API_Key key, string? secret = null) : ViewModelBase
{
    public string Identifier { get; } = key.Identifier;
    public string Description { get; } = key.Description;
    public List<string> AllowedIps { get; } = key.Allowed_ips;
    public DateTime? LastUsedAt { get; } = key.Last_used_at;

    public string? LastUsedAtText { get; } =
        string.IsNullOrEmpty(key.Last_used_at.ToString()) ? "Never" : key.Last_used_at.ToString();

    public DateTime CreatedAt { get; } = key.Created_at;

    public string? Secret { get; } = secret;

    public bool ShowsSecret { get; } = string.IsNullOrWhiteSpace(secret);
}