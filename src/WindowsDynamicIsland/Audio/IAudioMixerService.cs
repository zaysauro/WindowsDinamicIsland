namespace WindowsDynamicIsland.Audio;

public interface IAudioMixerService
{
    event EventHandler<MasterVolumeChangedEventArgs>? MasterVolumeChanged;
    IReadOnlyList<AudioSessionInfo> GetSessions();
    void SetSessionVolume(uint processId, float volume);
    void SetSessionMute(uint processId, bool muted);
    float GetMasterVolume();
    void SetMasterVolume(float volume);
    bool GetMasterMute();
    void SetMasterMute(bool muted);
    float GetMicrophoneVolume();
    void SetMicrophoneVolume(float volume);
    bool GetMicrophoneMute();
    void SetMicrophoneMute(bool muted);
}
