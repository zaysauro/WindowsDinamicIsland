# Product roadmap

## Stage 1 — Native foundation (current)
- [x] .NET 8 WPF solution and Windows application manifest
- [x] System tray icon and context menu
- [x] Ctrl+Alt+V global hotkey registration
- [x] Mixer flyout shell with outside-click dismissal
- [x] Top-center Dynamic Island shell with hover expansion
- [x] Fullscreen/borderless monitor-cover detection
- [x] CI build verification on Windows runner
- [ ] Runtime verification on Windows 10/11

## Stage 2 — Audio mixer
- [x] Enumerate render audio sessions through NAudio/WASAPI
- [x] Read and set per-session volume and mute
- [x] Read master output volume and mute
- [x] Add per-session slider/mute/pulse-meter UI with polling only while visible
- [x] Control default microphone mute and input level
- [ ] Enumerate capture endpoints and allow choosing a different microphone
- [ ] Implement and test default input/output endpoint switching through a guarded policy-config adapter

## Stage 3 — Dynamic Island
- [x] Collapsed and expanded island states
- [x] Fullscreen/borderless auto-hide
- [x] Caps Lock and Num Lock notifications
- [x] Event-driven master volume notifications in Dynamic Island (WASAPI endpoint callback)
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
