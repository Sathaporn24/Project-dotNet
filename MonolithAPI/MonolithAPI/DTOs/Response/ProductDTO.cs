namespace MonolithAPI.DTOs.Reponse;

public class ProductDTO
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public double Price { get; set; }
    public string? ImagePath { get; set; }
    public string? OwnerName { get; set; }
}
