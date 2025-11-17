namespace MedSave.DTOs.Hypermedia;

public class CollectionResource<T>
{
    public IEnumerable<Resource<T>> Items { get; set; } = Enumerable.Empty<Resource<T>>();
    public object PageInfo { get; set; } = default!;
    public IEnumerable<Link> _links { get; set; } = Enumerable.Empty<Link>();
}