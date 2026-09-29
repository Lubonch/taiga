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

- [x] Crear `src/Taiga.Server` (ASP.NET Core `net10.0`): `Program.cs` (puerto/token, `wwwroot`), endpoints Health/Library/NowPlaying/Scan/Sync/Queue/Settings/History + `WS /ws/events`
- [x] Servir `frontend/dist` como `wwwroot` + `--self-test` del server (persistencia, reconocimiento, cola, detectores)
- [x] Crear `tests/Taiga.Server.Tests` (`WebApplicationFactory`, 8 tests): CRUD, scan, sync, token 401/200
- [x] Eliminar `src/Taiga.App` y `tests/Taiga.App.Tests` del repo y del `.slnx` (superseded por Server+Angular)

## Fase 4b — frontend/ Angular — NUEVO

- [x] Crear `frontend/` (Angular standalone, routing): biblioteca (filtro + búsqueda), detalle, historial, temporada, ajustes, tema oscuro — `ng build` verde
- [x] Cliente API (`api.service.ts`) contra `127.0.0.1` + live-update por WS (`events.service.ts`)
- [x] `npm ci && npm run build` verde; spec del API service compila (`tsc`); `ng test` (karma) pendiente de CI con Chrome

## Fase 4c — electron/ shell — NUEVO

- [x] Crear `electron/` (`main.ts`: puerto libre, sidecar, health-check, instancia única, tray; `preload.ts` mínimo; `electron-builder.yml`)
- [x] `electron-builder --dir` verificado en Arch (runtime + app.asar + sidecar + wwwroot; API 401/200 y scan OK contra sidecar)

## Fase 5 — Instaladores (exe, deb, Arch) y release — NUEVO

- [x] `electron-builder --dir` verificado en Arch; exe (NSIS) y deb se generan en CI (sin dpkg en este entorno)
- [x] `setup/arch/PKGBUILD` (`taiga-bin`) + `.SRCINFO` + `README-arch.md`; `makepkg` construye el paquete con layout verificado (`/opt/taiga`, `.desktop`, `/usr/bin/taiga`)
- [x] `setup/linux/pack-tarball.sh` unificado verificado (tar.gz 152MB + sha256 + self-test OK)
- [x] `release.yml` por tag: dotnet+ng test/build, exe + deb + tarball + PKGBUILD con SHA256 en el GitHub Release
- [x] Publicar `v3.0.0-net10-preview.2` (tag pusheado; release.yml genera exe+deb+tarball) — QA Arch/Windows pendiente de descarga del Release
- [ ] Criterio de corte: API 100% + front paridad (biblioteca/detalle/historial/búsqueda/temporada/ajustes) + instaladores exe/deb/Arch OK → merge a `master` → `v3.0.0`
