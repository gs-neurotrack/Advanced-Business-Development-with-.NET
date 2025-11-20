namespace NeuroTrack.DTOs;

public record GsPredictionsDTO(
    long IdPrediction,
    float StressPredicted,
    string Message,
    DateTime DatePredicted,
    long IdUser,
    long IdScores,
    long IdStatusRisk);