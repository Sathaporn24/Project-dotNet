using Microsoft.AspNetCore.Identity;

namespace MonolithAPI.Models;

public class UserModel : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Products that own by this user
    /// </summary>
    public ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();
}
