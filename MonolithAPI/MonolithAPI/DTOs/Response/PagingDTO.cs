namespace MonolithAPI.DTOs.Reponse;

public class PagingDTO<T> where T : class
{
    public int TotalItems { get; set; }
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}
