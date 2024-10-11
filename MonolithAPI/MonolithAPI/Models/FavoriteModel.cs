using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MonolithAPI.Models;

public class FavoriteModel
{
    [BindNever]
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId {get; set;}
}