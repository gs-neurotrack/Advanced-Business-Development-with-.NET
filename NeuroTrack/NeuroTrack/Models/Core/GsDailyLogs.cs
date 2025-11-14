namespace NeuroTrack.Models;

public class GsDailyLogs
{
    public long IdLog { get; set; }
    
    public int WorkHours { get; set; }
    
    public int Meetings { get; set; }
    
    public DateTime LogDate { get; set; }
    
    public long IdUser { get; set; }
    public GsUser GsUser { get; set; }
}