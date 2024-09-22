using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MonolithAPI.Models;

public class UnitModel
{
    public int Id { get; set; }
    public string? UnName { get; set; }
}