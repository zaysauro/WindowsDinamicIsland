using NAudio.CoreAudioApi;

namespace WindowsDynamicIsland.Audio;

/// <summary>WASAPI session access. Peak polling is intentionally caller-driven and only runs while the mixer is visible.</summary>
public sealed class AudioMixerService : IAudioMixerService, IDisposable
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private readonly object _gate = new();
    private MMDevice? _monitoredRenderDevice;

    public event EventHandler<MasterVolumeChangedEventArgs>? MasterVolumeChanged;

    public AudioMixerService() => TryStartMasterVolumeNotifications();

    private void TryStartMasterVolumeNotifications()
    {
        try
        {
            _monitoredRenderDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            _monitoredRenderDevice.AudioEndpointVolume.OnVolumeNotification += OnMasterVolumeNotification;
        }
        catch
        {
            _monitoredRenderDevice?.Dispose();
            _monitoredRenderDevice = null;
            // Audio devices can be unavailable during startup; the mixer remains usable when one appears.
        }
    }

    private void OnMasterVolumeNotification(AudioVolumeNotificationData data)
    {
        MasterVolumeChanged?.Invoke(this, new MasterVolumeChangedEventArgs(data.MasterVolume, data.Muted));
    }

    public IReadOnlyList<AudioSessionInfo> GetSessions()
    {
        lock (_gate)
        {
            using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var result = new List<AudioSessionInfo>();
            for (var i = 0; i < device.AudioSessionManager.Sessions.Count; i++)
            {
                var session = device.AudioSessionManager.Sessions[i];
                try
                {
                    var pid = session.GetProcessID;
                    var processName = "Aplicativo";
                    string? icon = null;
                    try
                    {
                        using var process = System.Diagnostics.Process.GetProcessById((int)pid);
                        processName = process.ProcessName;
                        icon = process.MainModule?.FileName;
                    }
                    catch { /* System or elevated process: keep a safe fallback label. */ }

                    result.Add(new AudioSessionInfo(
                        pid,
                        string.IsNullOrWhiteSpace(session.DisplayName) ? processName : session.DisplayName,
                        icon,
                        session.SimpleAudioVolume.Volume,
                        session.SimpleAudioVolume.Mute,
                        session.AudioMeterInformation.MasterPeakValue));
                }
                catch { /* A session may disappear while being enumerated. */ }
            }
            return result;
        }
    }

    public void SetSessionVolume(uint processId, float volume) => WithSession(processId, s => s.SimpleAudioVolume.Volume = Math.Clamp(volume, 0f, 1f));
    public void SetSessionMute(uint processId, bool muted) => WithSession(processId, s => s.SimpleAudioVolume.Mute = muted);

    private void WithSession(uint processId, Action<AudioSessionControl> action)
    {
        lock (_gate)
        {
            using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            for (var i = 0; i < device.AudioSessionManager.Sessions.Count; i++)
            {
                var session = device.AudioSessionManager.Sessions[i];
                if (session.GetProcessID == processId) { action(session); return; }
            }
        }
    }

    public float GetMasterVolume()
    {
        using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return device.AudioEndpointVolume.MasterVolumeLevelScalar;
    }
    public void SetMasterVolume(float volume)
    {
        using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(volume, 0f, 1f);
    }
    public bool GetMasterMute()
    {
        using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return device.AudioEndpointVolume.Mute;
    }
    public void SetMasterMute(bool muted)
    {
        using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.Mute = muted;
    }

    public float GetMicrophoneVolume()
    {
        using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
        return device.AudioEndpointVolume.MasterVolumeLevelScalar;
    }
    public void SetMicrophoneVolume(float volume)
    {
        using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
        device.AudioEndpointVolume.MasterVolumeLevelScalar = Math.Clamp(volume, 0f, 1f);
    }
    public bool GetMicrophoneMute()
    {
        using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
        return device.AudioEndpointVolume.Mute;
    }
    public void SetMicrophoneMute(bool muted)
    {
        using var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
        device.AudioEndpointVolume.Mute = muted;
    }

    public void Dispose()
    {
        if (_monitoredRenderDevice is not null)
        {
            _monitoredRenderDevice.AudioEndpointVolume.OnVolumeNotification -= OnMasterVolumeNotification;
            _monitoredRenderDevice.Dispose();
            _monitoredRenderDevice = null;
        }
        _enumerator.Dispose();
    }
}
