# Tasks: Taiga multiplataforma — backend .NET + front Angular + shell Electron

## Fase 0 — Línea base, branch y CI mínimo ✅

- [x] Crear y publicar el branch `feature/dotnet-migration` desde `master` (publicado vía SSH; protección manual según `docs/migration/versioning.md`)
- [x] Inventariar `src/**` y publicar matriz de paridad en `docs/migration/parity-matrix.md`
- [x] Extraer fixtures de `sync/*_parsers`, `track/recognition*`, `sync/queue` y corpus de nombres
- [x] Workflow base `.github/workflows/dotnet-ci.yml` (`ubuntu-latest` + `windows-latest`)
- [x] Versionado SemVer (`v3.0.0-net10-preview.N` → `v3.0.0`) en `docs/migration/versioning.md`

## Fase 1 — Taiga.Core multiplataforma ✅

- [x] `Taiga.slnx` (.NET 10) + `src/Taiga.Core` (Nullable, ImplicitUsings, análisis, `dotnet format`)
- [x] `base/` (Clock, FileSystem, Logger, RssReader, Settings, TitleNormalizer) con `IClock`, `IFileSystem`, `ILogger`
- [x] `media/` (AnimeItem, AnimeLibrary, HistoryEntry) + `taiga/` (AppInfo, Session)
- [x] `SettingsStore` JSON + `IPathProvider` (portable/XDG/`%AppData%`) + `V1Importer`
- [x] `tests/Taiga.Core.Tests` en verde en Linux

## Fase 2 — Taiga.Sync (AniList/Kitsu/MAL) ✅

- [x] `src/Taiga.Sync` (`ISyncProvider`, `SyncService`, `SyncQueue` persistente + backoff)
- [x] Proveedores AniList (GraphQL), Kitsu (JSON:API), MyAnimeList (REST) con tests de contrato HTTP
- [x] Cola offline con reintentos verificados en tests

## Fase 3 — Taiga.Track + detección Linux ✅ (parcial)

- [x] `src/Taiga.Track` + `IPlaybackDetector` (playerctl + `/proc`)
- [x] Reconocimiento (parser, normalización, trigramas, `UpdateDecider`) con tests
- [x] Detección Linux + XDG + `.desktop`; D-Bus nativo y Secret Service diferidos
- [ ] `Taiga.Platform.Windows` (Win32) — requiere máquina Windows/CI
- [ ] Polling ≤ 1% CPU y matriz de reproductores en Ubuntu con escritorio real

## Fase 4a — Taiga.Server (API local) — NUEVO

- [ ] Crear `src/Taiga.Server` (ASP.NET Core `net10.0`): `Program.cs` (puerto/token efímeros, `wwwroot`), endpoints Health/Library/NowPlaying/Scan/Sync/Queue/Settings/History + `WS /ws/events`
- [ ] Servir `frontend/dist` como `wwwroot` en Release + `--self-test` del server (reutiliza comprobaciones Core)
- [ ] Crear `tests/Taiga.Server.Tests` (`WebApplicationFactory`): CRUD biblioteca, scan sin reproductor, sync sin proveedor, token requerido/rechazado
- [ ] Eliminar `src/Taiga.App` y `tests/Taiga.App.Tests` del repo y del `.slnx` (superseded por Server+Angular)

## Fase 4b — frontend/ Angular — NUEVO

- [ ] Crear `frontend/` (Angular standalone, routing): biblioteca (filtro + búsqueda), detalle, historial, temporada, ajustes (servicio, tokens, carpetas, intervalo), tema oscuro
- [ ] Cliente API (`api.service.ts`) contra `127.0.0.1` + live-update por WS (`events.service.ts`)
- [ ] `npm ci && npm run build` verde; specs mínimos del servicio API con stub HTTP

## Fase 4c — electron/ shell — NUEVO

- [ ] Crear `electron/` (`main.ts`: puerto libre, sidecar `Taiga.Server`, health-check, instancia única, tray con Detectar/Sincronizar/Salir; `preload.ts` mínimo; `electron-builder.yml`)
- [ ] `electron-builder --dir` verificado en Linux (árbol + arranque manual contra server local)

## Fase 5 — Instaladores (exe, deb, Arch) y release — NUEVO

- [ ] `electron-builder`: `Taiga Setup <v>.exe` (NSIS per-user) y `taiga_<v>_amd64.deb` verificados (CI Windows + Ubuntu)
- [ ] `setup/arch/PKGBUILD` (`taiga-bin`: source tarball + sha256, `/opt/taiga`, `.desktop`, `/usr/bin/taiga`) + `.SRCINFO` + `README-arch.md` (`makepkg -si`, AUR)
- [ ] Extender `setup/linux/pack-tarball.sh` al tarball unificado (server + front + electron dir) con self-test
- [ ] `release.yml` por tag: exe + deb + tarball + PKGBUILD con SHA256 en el GitHub Release
- [ ] Publicar `v3.0.0-net10-preview.2` (esquema Electron) y QA en Arch (`makepkg -si` + arranque desde lanzador) y Windows (exe)
- [ ] Criterio de corte: API 100% + front paridad (biblioteca/detalle/historial/búsqueda/temporada/ajustes) + instaladores exe/deb/Arch OK → merge a `master` → `v3.0.0`
