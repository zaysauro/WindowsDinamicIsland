namespace WindowsDynamicIsland.Audio;

public sealed record AudioSessionInfo(
    uint ProcessId,
    string ApplicationName,
    string? IconPath,
    float Volume,
    bool IsMuted,
    float Peak);

public sealed class MasterVolumeChangedEventArgs(float volume, bool muted) : EventArgs
{
    public float Volume { get; } = Math.Clamp(volume, 0f, 1f);
    public bool IsMuted { get; } = muted;
}
