using System;
using System.Collections.Generic;
using Shoebill.Models.Api.Schemas;

namespace Shoebill.ViewModels.Dialogs;

public class ApiKeyInfoViewModel(API_Key key, string? secret = null) : ViewModelBase
{
    public string Identifier { get; } = key.Identifier;
    public string Description { get; } = key.Description;
    public List<string> Allowed_ips { get; } = key.Allowed_ips;
    public DateTime? Last_used_at { get; } = key.Last_used_at;
    public string? Last_used_at_text { get; } = string.IsNullOrEmpty(key.Last_used_at.ToString()) ? "Never" : key.Last_used_at.ToString();
    public DateTime Created_at { get; } = key.Created_at;

    public string? Secret { get; } = secret;

    public bool ShowsSecret { get; } = string.IsNullOrWhiteSpace(secret);
}
