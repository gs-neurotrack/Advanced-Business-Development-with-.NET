namespace NeuroTrack.DTOs;

public record GsDailyLogsDTO(
    long IdLog,
    int WorkHours,
    int Meetings,
    DateTime LogDate,
    long IdUser
);