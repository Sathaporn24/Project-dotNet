using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MonolithAPI.Models;

public class ProfileModel
{
    public Guid Id { get; set; }
    public string? FullAddress { get; set; }
    public string? District { get; set; }
    public string? Amphoe { get; set; }
    public string? Province { get; set; }
    public string? ZipCode { get; set; }
}
