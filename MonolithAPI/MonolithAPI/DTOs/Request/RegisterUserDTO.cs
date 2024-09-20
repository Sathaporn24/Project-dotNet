using System.ComponentModel.DataAnnotations;

namespace MonolithAPI.DTOs.Request;

public class RegisterUserDTO
{
    [Required(ErrorMessage = "First name is required.")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public string? Role { get; set; }

    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Incorrect email.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Confirmation password is required."), Compare(nameof(Password), ErrorMessage = "Password and confirmation password is mismatched.")]
    public string? ConfirmPassword { get; set; }
}
