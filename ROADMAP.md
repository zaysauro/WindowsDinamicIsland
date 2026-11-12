# Product roadmap

## Stage 1 — Native foundation (current)
- [x] .NET 8 WPF solution and Windows application manifest
- [x] System tray icon and context menu
- [x] Ctrl+Alt+V global hotkey registration
- [x] Mixer flyout shell with outside-click dismissal
- [x] Top-center Dynamic Island shell with hover expansion
- [x] Fullscreen/borderless monitor-cover detection
- [ ] Build and runtime verification on Windows 10/11

## Stage 2 — Audio mixer
- [x] Enumerate render audio sessions through NAudio/WASAPI
- [x] Read and set per-session volume and mute
- [x] Read master output volume and mute
- [ ] Add per-session slider/mute/pulse-meter UI with 30 FPS polling only while visible
- [ ] Enumerate capture endpoints and control microphone mute/level
- [ ] Implement and test default capture endpoint switching through a guarded policy-config adapter

## Stage 3 — Dynamic Island
- [x] Collapsed and expanded island states
- [x] Fullscreen/borderless auto-hide
- [ ] Caps Lock, Num Lock and volume notifications
- [ ] Multi-monitor placement and DPI-aware positioning

## Stage 4 — Media and wellbeing
- [ ] Windows GSMTC media session integration
- [ ] Album art and transport controls
- [ ] Pomodoro timer
- [ ] Water and posture reminders
- [ ] Low-resource mode

## Stage 5 — Utilities and polish
- [ ] Time-limited keyboard cleaning mode with mouse emergency unlock
- [ ] Dark, light, system and glass themes
- [ ] Portuguese, English and Spanish localization
- [ ] Installer/MSIX and signed release pipeline

## Engineering constraints
- No polling loop should run when its UI surface is hidden.
- The app must not require administrator privileges for normal operation.
- Keyboard blocking must never start automatically and must always expose a mouse-accessible unlock path.
- Undocumented Windows interfaces must be isolated behind adapters and covered by Windows-version-specific tests.
