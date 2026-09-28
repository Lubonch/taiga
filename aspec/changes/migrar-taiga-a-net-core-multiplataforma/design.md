# Design: Migración completa de Taiga a .NET moderno multiplataforma (Linux)

## Approach

Migración por reescritura estructurada (no conversión automática C++ → C#), en fases verificables sobre el branch `feature/dotnet-migration` creado desde `master`:

1. **Inventario y línea base (Fase 0):** congelar comportamiento v2.0.0 (`CMakeLists.txt:3-8`, `src/`). Producir matriz de paridad: cada `.cpp/.hpp` mapeado a proyecto/clase C# objetivo + fixtures de parsers (`anilist_parsers`, `kitsu_parsers`, `myanimelist_parsers`), reconocimiento (`track/recognition*`), cola (`sync/queue.cpp`) y ajustes (`base/settings`, `taiga/settings`, `compat/settings`).
2. **Core multiplataforma (Fase 1):** `Taiga.Core` (.NET 10, `net10.0`, Nullable + ImplicitUsings + análisis) con `base/` (chrono, file, log, rss, settings, string, xml), `media/` (anime_db, history, list, season, utils), `compat/` (importador v1 solo lectura) y `taiga/` (session, accounts, config, version). Sin dependencias UI ni Win32. Tests xUnit con paridad de fixtures C++.
3. **Sync (Fase 2):** `Taiga.Sync` con `HttpClientFactory` + `Polly` (reintentos), OAuth2 (AniList/Kitsu/MAL: `*_auth.cpp`, `*_error.cpp`, `*_ratings.cpp`, `*_utils.cpp`), `Service` + `Queue` persistente (SQLite vía `Microsoft.Data.Sqlite` + EF Core o Dapper ligero). Tests de contrato con respuestas grabadas.
4. **Track Linux/Windows (Fase 3):** `Taiga.Track` con `IMediaDetector`, `IProcessScanner`, `IUpdateDecision`. Implementación Windows (Win32 titles/procesos, paridad actual `media_player.cpp`/`scanner.cpp`) e implementación Linux (MPRIS D-Bus vía `Tmds.DBus`, fallback X11 `_NET_WM_NAME` / Wayland `wlr-foreign-toplevel` donde sea posible + escaneo `/proc`). `recognition_*` portado a `System.Text.RegularExpressions` + normalización Unicode.
5. **App Avalonia (Fase 4):** `Taiga.App` (Avalonia 11+, MVVM con `ReactiveUI` o `CommunityToolkit.Mvvm`) que replica `src/gui/{common,history,library,list,main,media,models,search,settings,utils}`. Temas claro/oscuro, traducciones vía `.resx` (migración de `resources/translations/taiga_*.ts`), navegación y bindings testeados con `Avalonia.Headless.XUnit`.
6. **Plataforma, empaquetado y release (Fase 5):** `Taiga.Platform.{Windows,Linux}` (`IPathProvider` XDG vs `%AppData%`, autostart `.desktop` vs Startup/Registry, `ISecureStorage` Credential Manager vs Secret Service/libsecret). Publicación self-contained + framework-dependent para `linux-x64` y `win-x64` (`PublishSingleFile`, `ReadyToRun`). Artefactos: `tar.gz`, `.deb`, `.rpm`, `AppImage` y `.zip` Windows. CI GitHub Actions `build-test-pack-release.yml` + release con tags `v*` y notas generadas.

Decisiones clave: **.NET 10 LTS** como TFM único (`net10.0`, langVersion latest); **Avalonia** sobre MAUI/WinForms/WPF por soporte real Linux; **SQLite + JSON** para settings/portable (`TAIGA_PORTABLE` → carpeta junto al binario o XDG); **branch único de migración** con merges por fase, sin commits directos a `master`.

## Architecture

```text
Taiga.sln (net10.0)
├── src/Taiga.Core/            # base/*, media/*, compat/* (import), taiga/{session,accounts,config,version,settings}
│   ├── Infrastructure/Logging, Time, FileSystem, Xml, Rss, SettingsStore
│   └── Media/Anime, AnimeDb, History, List, Season, Recognition-support types
├── src/Taiga.Sync/            # sync/{service,queue,anilist,kitsu,myanimelist}
│   ├── Abstractions/ISyncService, IAuthFlow, IRateLimiter, IQueueStore
│   └── Providers/AniList|Kitsu|MyAnimeList (client+parsers+errors+ratings+auth)
├── src/Taiga.Track/           # track/*
│   ├── Recognition/*, UpdateSession/Decision/State, Scanner, Play, Episode
│   └── Detection/IMediaDetector → WindowsDetector | LinuxMprisDetector (+FallbackTitleDetector)
├── src/Taiga.Platform.Abstractions/  # IPathProvider, IAutostart, ISecureStorage, IPlatformInfo
├── src/Taiga.Platform.Windows/       # Win32, Credential Manager, Registry/Startup
├── src/Taiga.Platform.Linux/         # XDG, .desktop autostart, Secret Service, MPRIS/D-Bus, inotify
├── src/Taiga.App/             # Avalonia UI (Views/ViewModels) ↔ port de src/gui/**
│   └── i18n/*.resx ← migración de resources/translations/*.ts
└── tests/{Core,Sync,Track,App}.Tests/  # xUnit + FluentAssertions + Headless Avalonia
```

Flujos principales:

- **Detección → decisión → update:** `Scanner` (polling + eventos) → `MediaDetector` específico de SO → `Recognition` (normalize/path/relations/validate/cache) → `UpdateDecision` → `UpdateSession` → `Sync.Queue` → `ISyncService.UpdateAsync`.
- **Settings/paths:** `IPathProvider.GetAppData()` resuelve portable (`TAIGA_PORTABLE` / `.portable`) vs XDG (`~/.config/taiga`, `~/.local/share/taiga`) vs Windows (`%AppData%/Taiga`); `SettingsStore` JSON versionado + migración desde `compat/settings`.
- **Branch/release:** `feature/dotnet-migration` (protegida, CI obligatorio) → PRs por fase → `master`. Tags `v3.0.0-net10-preview.N` para previews Linux; `v3.0.0` estable cuando la matriz de paridad esté al 100% y QA Linux/Windows pase. Workflow `release.yml` (trigger por tag `v*`) compila, firma hashes (SHA256), genera `.deb/.rpm/AppImage/tar.gz/zip`, publica GitHub Release y actualiza notas.

## Validation

- **Paridad funcional:** checklist `src/` → C# al 100%; tests de parsers con fixtures reales de AniList/Kitsu/MAL; tests de reconocimiento con corpus de nombres (`anitomy`/`anime-relations` incluidos); `sync/queue` con reintentos/offline cubierto.
- **Multiplataforma:** `dotnet build/test` verde en `ubuntu-latest` + `windows-latest`; smoke manual en Ubuntu 24.04 (X11 y Wayland) y Windows 11: iniciar, login OAuth en los 3 proveedores, detectar MPV/VLC/mpv-like vía MPRIS y Win32, scrobblar, reiniciar con persistencia.
- **Empaquetado:** instalación limpia de `.deb` y `AppImage` en VM Ubuntu sin .NET previo (self-contained); `win-x64` arranca sin regresiones vs binario Qt 2.0.0.
- **Rendimiento/robustez:** arranque < 2s en hardware medio, polling detector ≤ 1% CPU, sin secretos en logs, análisis Roslyn + `dotnet format` sin warnings, cobertura ≥ 70% en Core/Sync/Track.
- **Release:** `git checkout -b feature/dotnet-migration master` + push + protección de rama verificados; release preview descargable con checksums y notas; criterio de corte a estable documentado en `tasks.md`.
