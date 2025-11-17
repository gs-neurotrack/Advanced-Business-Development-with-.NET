namespace NeuroTrack.DTOs;

public record GsScoresDTO(
    long IdScores,
    DateTime DateScore,
    float ScoreValue,
    int TimeRecommendation,
    DateTime CreatedAt,
    long IdStatusRisk,
    long IdUser,
    long IdLog
);