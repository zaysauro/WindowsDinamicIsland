using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using WindowsDynamicIsland.Audio;
using WindowsDynamicIsland.Core;
using WindowsDynamicIsland.Services;

namespace WindowsDynamicIsland.UI;

public sealed class TrayApplication : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly MixerWindow _mixer;
    private readonly DynamicIslandWindow _island;
    private readonly GlobalHotKeyService _hotKey;
    private readonly FullscreenDetector _fullscreen;
    private readonly IAudioMixerService _audio;
    private bool _disposed;

    public TrayApplication(MixerWindow mixer, DynamicIslandWindow island, GlobalHotKeyService hotKey, FullscreenDetector fullscreen, IAudioMixerService audio)
    {
        _mixer = mixer;
        _island = island;
        _hotKey = hotKey;
        _fullscreen = fullscreen;
        _audio = audio;
        _audio.MasterVolumeChanged += OnMasterVolumeChanged;
        _icon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Windows Dynamic Island",
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip()
        };
        _icon.ContextMenuStrip.Items.Add("Abrir Mixer", null, (_, _) => ToggleMixer());
        _icon.ContextMenuStrip.Items.Add("Mostrar Dynamic Island", null, (_, _) => _island.Show());
        _icon.ContextMenuStrip.Items.Add("Configurações", null, (_, _) => _mixer.ShowSettingsPlaceholder());
        _icon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _icon.ContextMenuStrip.Items.Add("Sair", null, (_, _) => System.Windows.Application.Current.Shutdown());
        _icon.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ToggleMixer(); };
        _hotKey.Pressed += (_, _) => ToggleMixer();
    }

    private void OnMasterVolumeChanged(object? sender, MasterVolumeChangedEventArgs e)
    {
        if (_disposed) return;
        _island.Dispatcher.BeginInvoke(() =>
        {
            if (!_disposed && _island.IsVisible)
                _island.ShowVolumeNotification(e.Volume, e.IsMuted);
        });
    }

    public void Start()
    {
        _mixer.SourceInitialized += (_, _) =>
        {
            var source = System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(_mixer).Handle);
            if (source is not null)
            {
                try { _hotKey.Register(source); }
                catch (InvalidOperationException ex)
                {
                    _icon.ShowBalloonTip(3500, "Atalho indisponível", ex.Message, ToolTipIcon.Warning);
                }
            }
        };
        _mixer.Show();
        _mixer.Hide();
        _island.Show();
        _fullscreen.FullscreenChanged += (_, full) => _island.Dispatcher.Invoke(() => _island.Visibility = full ? Visibility.Hidden : Visibility.Visible);
        _fullscreen.Start();
    }

    private void ToggleMixer()
    {
        if (_mixer.IsVisible) { _mixer.Hide(); return; }

        var workArea = SystemParameters.WorkArea;
        _mixer.Left = workArea.Right - _mixer.Width - 16;
        _mixer.Top = workArea.Bottom - _mixer.Height - 16;
        _mixer.Show();
        _mixer.Activate();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _audio.MasterVolumeChanged -= OnMasterVolumeChanged;
        _fullscreen.Dispose();
        _hotKey.Dispose();
        _icon.Visible = false;
        _icon.Dispose();
    }
}
