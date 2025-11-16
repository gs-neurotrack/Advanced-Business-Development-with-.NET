namespace NeuroTrack.Models;

public class GsUsers
{
    public long IdUser { get; set; }
    
    public string NameUser { get; set; }
    
    public string EmailUser { get; set; }
    
    public string PasswordUser { get; set; }
    
    public Char Status { get; set; }
    
    public long IdRole { get; set; }
    public GsRole GsRole { get; set; }
    
    public long IdLimits { get; set; }
    public GsLimits GsLimits { get; set; }
}