namespace MedSave.DTOs.Hypermedia;

public class Resource<T>
{
    public T Data { get; set; } = default!;
    public IEnumerable<Link> _links { get; set; } = Enumerable.Empty<Link>();
}