using System.ComponentModel.DataAnnotations;

namespace MonolithAPI.DTOs.Request
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        public double Price { get; set; }

        public string? Description { get; set; }

        public IFormFile? Image { get; set; }

        public int Category { get; set; }

        public int Unit { get; set; }
    }
}
