namespace NeuroTrack.Models;

public class GsPredictions
{
    public long IdPrediction { get; set; }
    
    public float StressPredicted { get; set; }
    
    public string Message { get; set; }
    
    public DateTime DatePredicted { get; set; }
    
    public long IdUser { get; set; }
    public GsUser GsUser { get; set; }
    
    public long IdScores { get; set; }
    public GsScores GsScores { get; set; }
    
    public long IdStatusRisk { get; set; }
    public GsStatusRisk GsStatusRisk { get; set; }
}