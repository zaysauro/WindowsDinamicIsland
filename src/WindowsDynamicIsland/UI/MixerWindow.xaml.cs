using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WindowsDynamicIsland.Audio;

namespace WindowsDynamicIsland.UI;

public partial class MixerWindow : Window
{
    private readonly IAudioMixerService _audio;
    private readonly DispatcherTimer _refreshTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private readonly ObservableCollection<SessionRow> _sessions = new();
    private bool _updatingMaster;
    private bool _updatingMicrophone;

    public MixerWindow(IAudioMixerService audio)
    {
        InitializeComponent();
        _audio = audio;
        SessionsList.ItemsSource = _sessions;
        _refreshTimer.Tick += (_, _) => RefreshSessions();
        Loaded += (_, _) => RefreshSessions();
        IsVisibleChanged += (_, _) =>
        {
            if (IsVisible) { _refreshTimer.Start(); RefreshSessions(); }
            else _refreshTimer.Stop();
        };
    }

    private void RefreshSessions()
    {
        if (!IsVisible) return;
        try
        {
            _updatingMaster = true;
            MasterVolumeSlider.Value = _audio.GetMasterVolume() * 100;
            MasterMuteButton.Content = _audio.GetMasterMute() ? "Ativar som" : "Mudo";
            _updatingMaster = false;

            try
            {
                _updatingMicrophone = true;
                MicrophoneVolumeSlider.Value = _audio.GetMicrophoneVolume() * 100;
                MicrophoneMuteButton.Content = _audio.GetMicrophoneMute() ? "Ativar mic." : "Mudo";
                MicrophoneControls.Visibility = Visibility.Visible;
                NoMicrophoneState.Visibility = Visibility.Collapsed;
            }
            catch
            {
                MicrophoneControls.Visibility = Visibility.Collapsed;
                NoMicrophoneState.Visibility = Visibility.Visible;
            }
            finally { _updatingMicrophone = false; }

            var current = _audio.GetSessions();
            var byPid = current.GroupBy(s => s.ProcessId).ToDictionary(g => g.Key, g => g.First());
            foreach (var row in _sessions.Where(row => !byPid.ContainsKey(row.ProcessId)).ToList())
                _sessions.Remove(row);

            foreach (var session in current)
            {
                var row = _sessions.FirstOrDefault(s => s.ProcessId == session.ProcessId);
                if (row is null)
                {
                    _sessions.Add(new SessionRow(_audio, session));
                }
                else
                {
                    row.UpdateFrom(session);
                }
            }
        }
        catch
        {
            _updatingMaster = false;
        }

        EmptyState.Visibility = _sessions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void MasterVolume_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_updatingMaster && IsLoaded) _audio.SetMasterVolume((float)(e.NewValue / 100));
    }

    private void MasterMute_Click(object sender, RoutedEventArgs e)
    {
        _audio.SetMasterMute(!_audio.GetMasterMute());
        MasterMuteButton.Content = _audio.GetMasterMute() ? "Ativar som" : "Mudo";
    }

    private void MicrophoneVolume_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_updatingMicrophone && IsLoaded)
        {
            try { _audio.SetMicrophoneVolume((float)(e.NewValue / 100)); }
            catch { /* A capture endpoint may be disconnected while dragging. */ }
        }
    }

    private void MicrophoneMute_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _audio.SetMicrophoneMute(!_audio.GetMicrophoneMute());
            MicrophoneMuteButton.Content = _audio.GetMicrophoneMute() ? "Ativar mic." : "Mudo";
        }
        catch { NoMicrophoneState.Visibility = Visibility.Visible; }
    }

    private void SessionMute_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is SessionRow row)
            row.ToggleMute();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Hide();
    private void Window_Deactivated(object? sender, EventArgs e) => Hide();
    public void ShowSettingsPlaceholder() => MessageBox.Show("As configurações serão adicionadas nas próximas etapas.", "Windows Dynamic Island");
}

internal sealed class SessionRow : INotifyPropertyChanged
{
    private readonly IAudioMixerService _audio;
    private float _volume;
    private float _peak;
    private bool _isMuted;

    public uint ProcessId { get; }
    public string Name { get; }
    public float Volume
    {
        get => _volume;
        set
        {
            var next = Math.Clamp(value, 0f, 100f);
            if (Math.Abs(_volume - next) < 0.5f) return;
            _volume = next;
            _audio.SetSessionVolume(ProcessId, next / 100f);
            OnPropertyChanged();
        }
    }
    public float Peak { get => _peak; private set { _peak = value; OnPropertyChanged(); } }
    public bool IsMuted { get => _isMuted; private set { _isMuted = value; OnPropertyChanged(); OnPropertyChanged(nameof(MuteLabel)); } }
    public string MuteLabel => IsMuted ? "Ativar" : "Mudo";

    public SessionRow(IAudioMixerService audio, AudioSessionInfo session)
    {
        _audio = audio;
        ProcessId = session.ProcessId;
        Name = session.ApplicationName;
        _volume = session.Volume * 100;
        _peak = session.Peak * 100;
        _isMuted = session.IsMuted;
    }

    public void UpdateFrom(AudioSessionInfo session)
    {
        if (Math.Abs(_volume - session.Volume * 100) > 1.5f)
        {
            _volume = session.Volume * 100;
            OnPropertyChanged(nameof(Volume));
        }
        Peak = session.Peak * 100;
        IsMuted = session.IsMuted;
    }

    public void ToggleMute()
    {
        _audio.SetSessionMute(ProcessId, !IsMuted);
        IsMuted = !IsMuted;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
