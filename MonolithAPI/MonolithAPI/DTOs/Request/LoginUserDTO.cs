using System.ComponentModel.DataAnnotations;

namespace MonolithAPI.DTOs.Request;

public class LoginUserDTO
{
    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Invalid email.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; set; }
}
