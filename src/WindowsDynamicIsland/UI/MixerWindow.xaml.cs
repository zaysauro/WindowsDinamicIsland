using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WindowsDynamicIsland.Audio;

namespace WindowsDynamicIsland.UI;

public partial class MixerWindow : Window
{
    private readonly IAudioMixerService _audio;
    private readonly DispatcherTimer _refreshTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private bool _updating;

    public MixerWindow(IAudioMixerService audio)
    {
        InitializeComponent();
        _audio = audio;
        _refreshTimer.Tick += (_, _) => RefreshSessions();
        Loaded += (_, _) => { MasterVolumeSlider.Value = _audio.GetMasterVolume() * 100; RefreshSessions(); };
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
            var sessions = _audio.GetSessions();
            _updating = true;
            MasterVolumeSlider.Value = _audio.GetMasterVolume() * 100;
            _updating = false;
            SessionsList.ItemsSource = sessions.Select(s => new SessionRow(_audio, s)).ToList();
            EmptyState.Visibility = sessions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch { _updating = false; }
    }

    private void MasterVolume_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_updating && IsLoaded) _audio.SetMasterVolume((float)(e.NewValue / 100));
    }

    private void MasterMute_Click(object sender, RoutedEventArgs e) => _audio.SetMasterMute(!_audio.GetMasterMute());
    private void Close_Click(object sender, RoutedEventArgs e) => Hide();
    private void Window_Deactivated(object? sender, EventArgs e) => Hide();
    public void ShowSettingsPlaceholder() => MessageBox.Show("As configurações serão adicionadas nas próximas etapas.", "Windows Dynamic Island");
}

internal sealed class SessionRow
{
    public string Name { get; }
    public float Volume { get; set; }
    public float Peak { get; }
    public SessionRow(IAudioMixerService audio, AudioSessionInfo session)
    {
        Name = session.ApplicationName;
        Volume = session.Volume * 100;
        Peak = session.Peak * 100;
    }
}
