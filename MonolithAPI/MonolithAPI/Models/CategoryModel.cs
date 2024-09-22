using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MonolithAPI.Models;

public class CategoryModel
{
    public int Id { get; set; }
    public string? CateName { get; set; }
}