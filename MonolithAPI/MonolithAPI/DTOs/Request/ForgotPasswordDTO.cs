using System.ComponentModel.DataAnnotations;

namespace MonolithAPI.DTOs.Request;

public class ForgotPasswordDTO
{
    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Incorrect email.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Client URI is required.")]
    public string? ClientURI { get; set; }
}
