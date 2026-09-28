# Tasks: Migración completa de Taiga a .NET moderno multiplataforma (Linux)

## Fase 0 — Línea base, branch y CI mínimo

- [x] Crear y publicar el branch `feature/dotnet-migration` desde `master` (`git checkout -b feature/dotnet-migration origin/master && git remote set-url origin git@github.com:Lubonch/taiga.git && git push -u origin feature/dotnet-migration`) — publicado vía SSH; protección manual en GitHub UI según `docs/migration/versioning.md` (sin `gh` en entorno)
- [x] Inventariar `src/**` (base, compat, media, sync, taiga, track, gui, resources) y publicar matriz de paridad C++ → C# en `docs/migration/parity-matrix.md`
- [x] Extraer fixtures de `sync/*_parsers`, `track/recognition*`, `sync/queue` y corpus de nombres para tests de contrato
- [x] Crear workflow base `.github/workflows/dotnet-ci.yml` (`ubuntu-latest` + `windows-latest`: `dotnet build` + `dotnet test`) en el branch de migración
- [x] Definir versionado SemVer y política de tags (`v3.0.0-net10-preview.N` → `v3.0.0`) en `docs/migration/versioning.md`

## Fase 1 — Taiga.Core multiplataforma

- [x] Crear `Taiga.slnx` (.NET 10, `net10.0`) + `src/Taiga.Core` (Nullable, ImplicitUsings, análisis Roslyn, `dotnet format`) — verificado `dotnet build/test` en Linux
- [x] Portar `base/` (Clock, FileSystem, Logger, RssReader, Settings, TitleNormalizer) sin dependencias UI/Win32, con `IClock`, `IFileSystem`, `ILogger` — verificado en Linux
- [x] Portar `media/` (AnimeItem, AnimeLibrary, HistoryEntry) + `taiga/` (AppInfo, Session) a `Taiga.Core` — season/export diferidos a siguiente iteración
- [x] Implementar `SettingsStore` JSON versionado + `IPathProvider` (portable vs XDG vs `%AppData%`) + importador solo-lectura `compat/` (V1Importer INI)
- [x] Tests `tests/Taiga.Core.Tests` (9 tests xUnit) en verde en Linux; Windows pendiente de CI

## Fase 2 — Taiga.Sync (AniList/Kitsu/MAL)

- [x] Crear `src/Taiga.Sync` con `ISyncProvider`, `SyncService` y `SyncQueue` persistente (JSON; SQLite diferido) + backoff — sin Polly/DI de momento
- [x] Portar proveedor AniList (mutación GraphQL real + Bearer) con test de contrato HTTP (stub)
- [x] Portar proveedor Kitsu (PATCH JSON:API real + Bearer) con tests de contrato HTTP (stub)
- [x] Portar proveedor MyAnimeList (PUT REST real + Bearer) con tests de contrato HTTP (stub); OAuth PKCE interactivo diferido
- [x] Portar `sync/{service,queue}.cpp` a cola persistente (JSON) con modo offline y reintentos verificados en tests

## Fase 3 — Taiga.Track + plataforma Windows/Linux

- [x] Crear `src/Taiga.Track` + `IPlaybackDetector` (playerctl + /proc); `IAutostart`/`ISecureStorage` diferidos
- [x] Portar reconocimiento (parser de ficheros, normalización, trigramas, UpdateDecider) con 16 tests
- [ ] Implementar `Taiga.Platform.Windows` (títulos Win32, procesos, autostart Startup/Registry, Credential Manager) con paridad `media_player.cpp`/`scanner.cpp`
- [x] Implementar detección Linux (playerctl/MPRIS-CLI + `/proc`, XDG en PathProvider, `.desktop` en setup); D-Bus nativo y Secret Service diferidos
- [ ] Verificar polling ≤ 1% CPU y matriz de reproductores (MPV, VLC, mpv-based, navegadores) en Ubuntu 24.04 X11/Wayland

## Fase 4 — Taiga.App (Avalonia UI)

- [x] Crear `src/Taiga.App` (Avalonia 12, MVVM Toolkit) con biblioteca, detalle, detección y sincronización — verificado `dotnet build` Linux
- [x] `MainViewModel` testeable en `tests/Taiga.App.Tests` (3 tests, VM puros); Headless XUnit diferido
- [ ] Migrar traducciones `resources/translations/*.ts` a `.resx` + temas claro/oscuro y persistencia de layout
- [ ] Validar aceptación visual pantalla por pantalla contra Qt Widgets (checklist con capturas Linux/Windows)
- [ ] Smoke E2E: login 3 proveedores → detectar episodio → scrobblar → reiniciar con persistencia en ambos SO

## Fase 5 — Empaquetado, release y corte a estable

- [x] `PublishSingleFile` `linux-x64` self-contained verificado (binario 95MB, `--self-test` exit 0); `win-x64` pendiente de CI
- [x] `setup/linux/pack-tarball.sh` + `taiga.desktop` validados (tar.gz 41MB + sha256 + self-test); deb/rpm/AppImage diferidos
- [x] Crear workflow `.github/workflows/release.yml` (trigger tag `v*`): build, test, pack, SHA256, GitHub Release con notas generadas
- [ ] Publicar `v3.0.0-net10-preview.1` desde `feature/dotnet-migration`, QA en Ubuntu 24.04 y Windows 11, y registrar issues de paridad restante
- [ ] Criterio de corte: paridad 100%, CI verde, cobertura ≥ 70% Core/Sync/Track, smoke E2E OK → merge a `master` por PR → tag `v3.0.0` → GitHub Release estable (deb/rpm/AppImage/tar.gz/zip Windows) y anuncio de deprecación del binario Qt
