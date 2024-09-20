using Microsoft.AspNetCore.Identity;

namespace MonolithAPI.Models;

public class RoleModel : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
