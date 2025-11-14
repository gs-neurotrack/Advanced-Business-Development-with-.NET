namespace NeuroTrack.Models;

public class GsScores
{
    public long IdScores { get; set; }
    
    public DateTime DateScore { get; set; }
    
    public float ScoreValue { get; set; }
    
    public int TimeRecommendation { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public long IdStatusRisk { get; set; }
    public GsStatusRisk GsStatusRisk { get; set; }
    
    public long IdUser { get; set; }
    public GsUser GsUser { get; set; }
    
    public long IdLog { get; set; }
    public GsDailyLogs GsDailyLogs { get; set; }
}