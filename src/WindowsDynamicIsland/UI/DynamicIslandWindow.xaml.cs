using System.Windows;
using System.Windows.Media.Animation;

namespace WindowsDynamicIsland.UI;

public partial class DynamicIslandWindow : Window
{
    private bool _expanded;

    public DynamicIslandWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => PositionAtTopCenter();
    }

    private void PositionAtTopCenter()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Left + (area.Width - Width) / 2;
        Top = 8;
    }

    private void Island_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) => SetExpanded(true);
    private void Island_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => SetExpanded(false);
    private void Island_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => SetExpanded(!_expanded);

    private void SetExpanded(bool expanded)
    {
        _expanded = expanded;
        var width = expanded ? 380d : 190d;
        var height = expanded ? 112d : 38d;
        BeginAnimation(WidthProperty, new DoubleAnimation(width, TimeSpan.FromMilliseconds(220)) { EasingFunction = new QuadraticEase() });
        BeginAnimation(HeightProperty, new DoubleAnimation(height, TimeSpan.FromMilliseconds(220)) { EasingFunction = new QuadraticEase() });
        IslandStatus.Text = expanded ? "Windows Dynamic Island  ·  Central de atividades" : "Dynamic Island";
    }
}
