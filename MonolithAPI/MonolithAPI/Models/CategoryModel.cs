using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MonolithAPI.Models;

public class CategoryModel
{
    [BindNever]
    public int Id { get; set; }
    public string? CateName { get; set; }
}