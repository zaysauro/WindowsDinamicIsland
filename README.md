# Windows Dynamic Island

A lightweight native Windows 10/11 desktop companion built with C# and WPF on .NET 8.

The application combines a system-tray audio mixer, a top-center Dynamic Island, media controls, productivity utilities, and optional keyboard-cleaning mode.

## Technology

- C# / .NET 8 / WPF
- NAudio for WASAPI audio sessions and meters
- Win32 APIs for global hotkeys, window detection, and keyboard hooks
- Windows.Media.Control for system media sessions

## Status

Initial project scaffold is being established. Features will be implemented in independently testable stages. See [ROADMAP.md](ROADMAP.md).

## Build

Requires Windows 10/11 and the .NET 8 SDK with the Windows Desktop workload.

```powershell
dotnet restore .\WindowsDynamicIsland.sln
dotnet build .\src\WindowsDynamicIsland\WindowsDynamicIsland.csproj -c Release
```

## Safety

Keyboard lock is opt-in, time-limited, and must always provide a mouse-accessible emergency unlock. No key blocking is active by default.
