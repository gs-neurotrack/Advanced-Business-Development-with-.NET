namespace MedSave.DTOs.Hypermedia;

public class Link
{
    public string Rel { get; set; } = default!;
    public string Href { get; set; } = default!;
    public string Method { get; set; } = "GET";
}