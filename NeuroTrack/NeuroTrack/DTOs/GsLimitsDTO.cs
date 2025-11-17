namespace NeuroTrack.DTOs;

public record GsLimitsDTO(
    long IdLimits,
    int LimitHours,
    int LimitMeetings,
    DateTime CreatedAt
);