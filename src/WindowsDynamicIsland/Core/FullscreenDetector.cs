using System.Windows;
using System.Windows.Threading;

namespace WindowsDynamicIsland.Core;

/// <summary>Detects foreground windows covering their monitor (exclusive or borderless fullscreen).</summary>
public sealed class FullscreenDetector : IDisposable
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private bool _disposed;

    public event EventHandler<bool>? FullscreenChanged;

    public FullscreenDetector() => _timer.Tick += (_, _) => Check();

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private void Check()
    {
        var hwnd = NativeMethods.GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return;

        var monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MonitorDefaultToNearest);
        if (monitor == IntPtr.Zero) return;

        var info = new NativeMethods.MONITORINFO { cbSize = System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MONITORINFO>() };
        if (!NativeMethods.GetMonitorInfo(monitor, ref info) || !NativeMethods.GetWindowRect(hwnd, out var rect)) return;

        const int tolerance = 2;
        var full = rect.Left <= info.rcMonitor.Left + tolerance &&
                   rect.Top <= info.rcMonitor.Top + tolerance &&
                   rect.Right >= info.rcMonitor.Right - tolerance &&
                   rect.Bottom >= info.rcMonitor.Bottom - tolerance;

        FullscreenChanged?.Invoke(this, full);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
    }
}
