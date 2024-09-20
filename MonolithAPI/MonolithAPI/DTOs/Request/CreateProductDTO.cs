using System.ComponentModel.DataAnnotations;

namespace MonolithAPI.DTOs.Request;

public class CreateProductDTO
{
    [Required(ErrorMessage = "Name is required.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    public double Price { get; set; }

    public string? Description { get; set; }

    public IFormFile? Image { get; set; }
}
