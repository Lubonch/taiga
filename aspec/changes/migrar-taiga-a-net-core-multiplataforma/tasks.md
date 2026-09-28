# Tasks: Migración completa de Taiga a .NET moderno multiplataforma (Linux)

## Fase 0 — Línea base, branch y CI mínimo

- [x] Crear y publicar el branch `feature/dotnet-migration` desde `master` (`git checkout -b feature/dotnet-migration origin/master && git remote set-url origin git@github.com:Lubonch/taiga.git && git push -u origin feature/dotnet-migration`) — publicado vía SSH; protección manual en GitHub UI según `docs/migration/versioning.md` (sin `gh` en entorno)
- [x] Inventariar `src/**` (base, compat, media, sync, taiga, track, gui, resources) y publicar matriz de paridad C++ → C# en `docs/migration/parity-matrix.md`
- [x] Extraer fixtures de `sync/*_parsers`, `track/recognition*`, `sync/queue` y corpus de nombres para tests de contrato
- [x] Crear workflow base `.github/workflows/dotnet-ci.yml` (`ubuntu-latest` + `windows-latest`: `dotnet build` + `dotnet test`) en el branch de migración
- [x] Definir versionado SemVer y política de tags (`v3.0.0-net10-preview.N` → `v3.0.0`) en `docs/migration/versioning.md`

## Fase 1 — Taiga.Core multiplataforma

- [x] Crear `Taiga.slnx` (.NET 10, `net10.0`) + `src/Taiga.Core` (Nullable, ImplicitUsings, análisis Roslyn, `dotnet format`) — verificado `dotnet build/test` en Linux
- [ ] Portar `base/` (chrono, file, log, rss, settings, string, xml) sin dependencias UI/Win32, con abstracciones `IClock`, `IFileSystem`, `ILogger`
- [ ] Portar `media/` (anime_db, history, list, season, utils, export) + `taiga/{session,accounts,config,version,settings}` a `Taiga.Core`
- [ ] Implementar `SettingsStore` JSON versionado + `IPathProvider` (portable vs XDG vs `%AppData%`) + importador solo-lectura `compat/`
- [ ] Tests `tests/Taiga.Core.Tests` (xUnit) con fixtures Fase 0 en verde en Linux y Windows

## Fase 2 — Taiga.Sync (AniList/Kitsu/MAL)

- [ ] Crear `src/Taiga.Sync` con `ISyncService`, `IAuthFlow`, `IQueueStore`, `HttpClientFactory` + `Polly` (reintentos/backoff/rate-limit)
- [ ] Portar proveedor AniList (`anilist*.cpp`: client, auth OAuth2, parsers, errors, ratings, utils) con tests de contrato
- [ ] Portar proveedor Kitsu (client, auth, parsers, errors, ratings, utils) con tests de contrato
- [ ] Portar proveedor MyAnimeList (client, auth PKCE, parsers, errors, ratings, utils) con tests de contrato
- [ ] Portar `sync/{service,queue}.cpp` a cola persistente SQLite (`Microsoft.Data.Sqlite`) con modo offline y reintentos verificados

## Fase 3 — Taiga.Track + plataforma Windows/Linux

- [ ] Crear `src/Taiga.Track` + `Taiga.Platform.Abstractions` (`IMediaDetector`, `IProcessScanner`, `IPlatformInfo`, `IAutostart`, `ISecureStorage`)
- [ ] Portar `track/recognition*` (normalize, path, relations, validate, cache) + `episode`, `play`, `update_*` con tests de corpus
- [ ] Implementar `Taiga.Platform.Windows` (títulos Win32, procesos, autostart Startup/Registry, Credential Manager) con paridad `media_player.cpp`/`scanner.cpp`
- [ ] Implementar `Taiga.Platform.Linux` (MPRIS vía D-Bus `Tmds.DBus`, fallback títulos X11/Wayland, escaneo `/proc`, autostart `.desktop`, Secret Service, XDG/inotify)
- [ ] Verificar polling ≤ 1% CPU y matriz de reproductores (MPV, VLC, mpv-based, navegadores) en Ubuntu 24.04 X11/Wayland

## Fase 4 — Taiga.App (Avalonia UI)

- [ ] Crear `src/Taiga.App` (Avalonia 11+, MVVM) con navegación que replica `src/gui/{main,library,list,history,media,search,settings}`
- [ ] Portar `src/gui/models`, `common`, `utils` a ViewModels testeables + `Avalonia.Headless.XUnit` en `tests/Taiga.App.Tests`
- [ ] Migrar traducciones `resources/translations/*.ts` a `.resx` + temas claro/oscuro y persistencia de layout
- [ ] Validar aceptación visual pantalla por pantalla contra Qt Widgets (checklist con capturas Linux/Windows)
- [ ] Smoke E2E: login 3 proveedores → detectar episodio → scrobblar → reiniciar con persistencia en ambos SO

## Fase 5 — Empaquetado, release y corte a estable

- [ ] Configurar `PublishSingleFile` + `ReadyToRun` para `linux-x64` y `win-x64` (self-contained y framework-dependent)
- [ ] Crear scripts `setup/linux/{deb,rpm,appimage,tarball}.sh` + `.desktop` + iconos y validar instalación limpia sin .NET previo
- [ ] Crear workflow `.github/workflows/release.yml` (trigger tag `v*`): build, test, pack, SHA256, GitHub Release con notas generadas
- [ ] Publicar `v3.0.0-net10-preview.1` desde `feature/dotnet-migration`, QA en Ubuntu 24.04 y Windows 11, y registrar issues de paridad restante
- [ ] Criterio de corte: paridad 100%, CI verde, cobertura ≥ 70% Core/Sync/Track, smoke E2E OK → merge a `master` por PR → tag `v3.0.0` → GitHub Release estable (deb/rpm/AppImage/tar.gz/zip Windows) y anuncio de deprecación del binario Qt
