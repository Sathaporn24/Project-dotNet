using System.ComponentModel.DataAnnotations;

namespace MonolithAPI.Models;

public class ProductModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public double Price { get; set; }
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? CreatedTime { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedTime { get; set; }

    /// <summary>
    /// Product owner
    /// </summary>
    public UserModel? Owner { get; set; }
}
