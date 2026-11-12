using System.Drawing;
using System.Windows;
using System.Windows.Forms;
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
    private bool _disposed;

    public TrayApplication(MixerWindow mixer, DynamicIslandWindow island, GlobalHotKeyService hotKey, FullscreenDetector fullscreen)
    {
        _mixer = mixer;
        _island = island;
        _hotKey = hotKey;
        _fullscreen = fullscreen;
        _icon = new NotifyIcon
        {
            Icon = SystemIcons.Volume,
            Text = "Windows Dynamic Island",
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip()
        };
        _icon.ContextMenuStrip.Items.Add("Abrir Mixer", null, (_, _) => ToggleMixer());
        _icon.ContextMenuStrip.Items.Add("Mostrar Dynamic Island", null, (_, _) => _island.Show());
        _icon.ContextMenuStrip.Items.Add("Configurações", null, (_, _) => _mixer.ShowSettingsPlaceholder());
        _icon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _icon.ContextMenuStrip.Items.Add("Sair", null, (_, _) => Application.Current.Shutdown());
        _icon.MouseClick += (_, e) => { if (e.Button == MouseButtons.Left) ToggleMixer(); };
        _hotKey.Pressed += (_, _) => ToggleMixer();
    }

    public void Start()
    {
        _mixer.SourceInitialized += (_, _) =>
        {
            var source = System.Windows.Interop.HwndSource.FromVisual(_mixer);
            if (source is not null) _hotKey.Register(source);
        };
        _mixer.Show();
        _mixer.Hide();
        _island.Show();
        _fullscreen.FullscreenChanged += (_, full) => _island.Dispatcher.Invoke(() => _island.Visibility = full ? Visibility.Hidden : Visibility.Visible);
        _fullscreen.Start();
    }

    private void ToggleMixer()
    {
        if (_mixer.IsVisible) _mixer.Hide();
        else { _mixer.Show(); _mixer.Activate(); }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _fullscreen.Dispose();
        _hotKey.Dispose();
        _icon.Visible = false;
        _icon.Dispose();
    }
}
