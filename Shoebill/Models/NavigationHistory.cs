using System;

namespace Shoebill.Models;

public record NavigationHistory(Type Page, bool IsMasterPage)
{
    public Type Page { get; set; } = Page;
    public bool IsMasterPage { get; set; } = IsMasterPage;
}