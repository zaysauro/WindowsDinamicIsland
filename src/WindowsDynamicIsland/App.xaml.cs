using Microsoft.Extensions.DependencyInjection;
using WindowsDynamicIsland.Audio;
using WindowsDynamicIsland.Core;
using WindowsDynamicIsland.Services;
using WindowsDynamicIsland.UI;

namespace WindowsDynamicIsland;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;
    private TrayApplication? _tray;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        var registrations = new ServiceCollection();
        registrations.AddSingleton<IAudioMixerService, AudioMixerService>();
        registrations.AddSingleton<FullscreenDetector>();
        registrations.AddSingleton<GlobalHotKeyService>();
        registrations.AddSingleton<TrayApplication>();
        registrations.AddSingleton<MixerWindow>();
        registrations.AddSingleton<DynamicIslandWindow>();
        _services = registrations.BuildServiceProvider();

        _tray = _services.GetRequiredService<TrayApplication>();
        _tray.Start();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _tray?.Dispose();
        _services?.Dispose();
        base.OnExit(e);
    }
}
