namespace MedSave.DTOs;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public PageInfo PageInfo { get; set; } = new();
}