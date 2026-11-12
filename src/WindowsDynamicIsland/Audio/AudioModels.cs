namespace WindowsDynamicIsland.Audio;

public sealed record AudioSessionInfo(
    uint ProcessId,
    string ApplicationName,
    string? IconPath,
    float Volume,
    bool IsMuted,
    float Peak);
