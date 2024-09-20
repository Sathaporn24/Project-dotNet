using System.ComponentModel.DataAnnotations;

namespace MonolithAPI.DTOs.Request;

public class GetProductDTO
{
    [Range(1, 10000)]
    public int PageIndex { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 5;

    public bool OnlyMyItem { get; set; }

    public string? Keyword { get; set; }
}
