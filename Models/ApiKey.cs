using System;

namespace Shoebill.Models;

public class ApiKey
{
    public string? Name { get; set; }
    public string? ServerAdress { get; set; }
    public string? Key { get; set; }
    public ApiTypes ApiType { get; set; }
}
