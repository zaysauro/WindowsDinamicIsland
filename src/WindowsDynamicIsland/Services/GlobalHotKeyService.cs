using System.Windows.Interop;
using WindowsDynamicIsland.Core;

namespace WindowsDynamicIsland.Services;

public sealed class GlobalHotKeyService : IDisposable
{
    private const int HotKeyId = 0xB017;
    private HwndSource? _source;
    public event EventHandler? Pressed;

    public void Register(HwndSource source)
    {
        Unregister();
        _source = source;
        _source.AddHook(WndProc);
        if (!NativeMethods.RegisterHotKey(_source.Handle, HotKeyId, NativeMethods.ModControl | NativeMethods.ModAlt, NativeMethods.VkV))
            throw new InvalidOperationException("Não foi possível registrar Ctrl+Alt+V. Verifique se outro aplicativo já utiliza esse atalho.");
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == 0x0312 && wParam.ToInt32() == HotKeyId)
        {
            handled = true;
            Pressed?.Invoke(this, EventArgs.Empty);
        }
        return IntPtr.Zero;
    }

    public void Unregister()
    {
        if (_source is null) return;
        NativeMethods.UnregisterHotKey(_source.Handle, HotKeyId);
        _source.RemoveHook(WndProc);
        _source = null;
    }

    public void Dispose() => Unregister();
}
