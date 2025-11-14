namespace NeuroTrack.Models;

public class GsLimits
{
    public long IdLimits { get; set; }
    
    public int LimitHours { get; set; }
    
    public int LimitMeetings { get; set; }
    
    public DateTime CreatedAt { get; set; }
}