using Microsoft.AspNetCore.Identity;

namespace TmsApi.Domain.Entities;

public class TmsUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Department { get; set; }
}