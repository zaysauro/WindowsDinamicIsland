using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WindowsDynamicIsland.UI;

public partial class DynamicIslandWindow : Window
{
    private readonly DispatcherTimer _volumeNotificationTimer;
    private bool _expanded;
    private bool _showingVolumeNotification;

    public DynamicIslandWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => PositionAtTopCenter();
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

        IslandStatus.Text = muted ? "Volume silenciado" : $"Volume  {Math.Round(volume * 100)}%";
        VolumeMeter.Value = muted ? 0 : Math.Clamp(volume, 0f, 1f);
        VolumeMeter.Visibility = Visibility.Visible;

        if (!_expanded) AnimateSize(260, 42);
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
            IslandStatus.Text = "Volume";
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
