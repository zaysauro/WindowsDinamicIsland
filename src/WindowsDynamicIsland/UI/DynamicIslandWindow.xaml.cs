using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WindowsDynamicIsland.Core;

namespace WindowsDynamicIsland.UI;

public partial class DynamicIslandWindow : Window
{
    private readonly DispatcherTimer _volumeNotificationTimer;
    private bool _expanded;
    private bool _showingVolumeNotification;
    private string _notificationText = "Dynamic Island";
    private bool? _capsLockState;
    private bool? _numLockState;
    private readonly DispatcherTimer _lockStateTimer;

    public DynamicIslandWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            PositionAtTopCenter();
            _capsLockState = IsToggleOn(0x14);
            _numLockState = IsToggleOn(0x90);
            _lockStateTimer.Start();
        };
        IsVisibleChanged += (_, e) =>
        {
            if ((bool)e.NewValue) _lockStateTimer.Start();
            else _lockStateTimer.Stop();
        };
        _lockStateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        _lockStateTimer.Tick += (_, _) => CheckLockKeys();
        _volumeNotificationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1700) };
        _volumeNotificationTimer.Tick += (_, _) =>
        {
            _volumeNotificationTimer.Stop();
            _showingVolumeNotification = false;
            VolumeMeter.Visibility = Visibility.Collapsed;
            if (!_expanded) SetCollapsedContent();
        };
    }

    private void PositionAtTopCenter()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Left + (area.Width - Width) / 2;
        Top = 8;
    }

    public void ShowVolumeNotification(float volume, bool muted)
    {
        _showingVolumeNotification = true;
        _volumeNotificationTimer.Stop();
        _volumeNotificationTimer.Start();

        _notificationText = muted ? "Volume silenciado" : $"Volume  {Math.Round(volume * 100)}%";
        IslandStatus.Text = _notificationText;
        VolumeMeter.Value = muted ? 0 : Math.Clamp(volume, 0f, 1f);
        VolumeMeter.Visibility = Visibility.Visible;

        if (!_expanded) AnimateSize(260, 42);
    }

    public void ShowLockNotification(string message)
    {
        _showingVolumeNotification = true;
        _volumeNotificationTimer.Stop();
        _volumeNotificationTimer.Start();
        _notificationText = message;
        IslandStatus.Text = message;
        VolumeMeter.Visibility = Visibility.Collapsed;
        if (!_expanded) AnimateSize(250, 42);
    }

    private static bool IsToggleOn(int virtualKey) => (NativeMethods.GetKeyState(virtualKey) & 1) != 0;

    private void CheckLockKeys()
    {
        var caps = IsToggleOn(0x14);
        var num = IsToggleOn(0x90);
        if (_capsLockState.HasValue && caps != _capsLockState.Value)
            ShowLockNotification(caps ? "Caps Lock ativado" : "Caps Lock desativado");
        else if (_numLockState.HasValue && num != _numLockState.Value)
            ShowLockNotification(num ? "Num Lock ativado" : "Num Lock desativado");
        _capsLockState = caps;
        _numLockState = num;
    }

    private void Island_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) => SetExpanded(true);
    private void Island_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => SetExpanded(false);
    private void Island_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => SetExpanded(!_expanded);

    private void SetExpanded(bool expanded)
    {
        _expanded = expanded;
        if (expanded)
        {
            IslandStatus.Text = "Windows Dynamic Island  ·  Central de atividades";
            VolumeMeter.Visibility = Visibility.Collapsed;
            AnimateSize(380, 112);
        }
        else if (_showingVolumeNotification)
        {
            IslandStatus.Text = _notificationText;
            AnimateSize(260, 42);
        }
        else
        {
            SetCollapsedContent();
        }
    }

    private void SetCollapsedContent()
    {
        IslandStatus.Text = "Dynamic Island";
        VolumeMeter.Visibility = Visibility.Collapsed;
        AnimateSize(190, 38);
    }

    private void AnimateSize(double width, double height)
    {
        BeginAnimation(WidthProperty, new DoubleAnimation(width, TimeSpan.FromMilliseconds(180)) { EasingFunction = new QuadraticEase() });
        BeginAnimation(HeightProperty, new DoubleAnimation(height, TimeSpan.FromMilliseconds(180)) { EasingFunction = new QuadraticEase() });
    }
}
