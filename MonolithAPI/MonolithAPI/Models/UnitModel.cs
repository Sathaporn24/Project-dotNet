using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MonolithAPI.Models;

public class UnitModel
{
    [BindNever]
    public int Id { get; set; }
    public string? UnName { get; set; }
}