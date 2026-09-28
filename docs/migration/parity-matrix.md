# Matriz de paridad C++ → C# — Taiga .NET migration

Base: branch `feature/dotnet-migration` desde `origin/master` (v1.4.1, 214 ficheros `*.cpp/*.h` en `src/`).
Nota: `origin/develop` (v2.0.0 Qt6, `src/gui/**`) diverge +528 ficheros; esta matriz cubre la base `master` y se ampliará al hacer merge/rebase con `develop`.

## Mapeo por dominio

| Dominio C++ (`src/`) | Ficheros clave | Proyecto C# objetivo | Clases objetivo | Estado |
|---|---|---|---|---|
| `base/` (42 ficheros: atf, base64, command_line, crypto, file, file_monitor, file_search, format, gfx, gzip, html, json, log, oauth, process, random, rss, settings, string, time, timer, url, xml) | `settings.cpp/h`, `file.cpp/h`, `log.h`, `rss.cpp/h`, `xml.cpp/h`, `json.cpp/h`, `oauth.cpp/h`, `time.cpp/h` | `src/Taiga.Core` | `Infrastructure/SettingsStore`, `FileSystem`, `Logging`, `RssReader`, `XmlHelper`, `JsonHelper`, `Auth/OAuthClient`, `Time/Clock` | 🔲 pendiente |
| `compat/` (anime_db, history, settings) | `compat/*.cpp` | `src/Taiga.Core` (`Compat/`) | `V1Importer` (solo lectura) | 🔲 pendiente |
| `media/` (anime_db, filter, item, season, util, library/export|history|list|queue) | `media/library/list.cpp`, `queue.cpp`, `history.cpp`, `export.cpp` | `src/Taiga.Core` (`Media/`) | `Anime`, `AnimeDb`, `AnimeList`, `History`, `ListExporter`, `SyncQueueModel` | 🔲 pendiente |
| `sync/` (anilist, kitsu, myanimelist, service, sync) | `sync/anilist*.cpp`, `kitsu*.cpp`, `myanimelist*.cpp`, `service.cpp`, `sync.cpp` | `src/Taiga.Sync` | `Providers/AniListClient`, `KitsuClient`, `MyAnimeListClient` + `SyncService`, `QueueStore` | 🔲 pendiente |
| `taiga/` (announce, app, config, settings, stats, torrent, update...) | `taiga/app.cpp`, `taiga/settings.cpp`, `taiga/path*` (en develop) | `src/Taiga.Core` + `Platform` | `Session`, `Accounts`, `AppConfig`, `VersionInfo` | 🔲 pendiente |
| `track/` (episode, feed, media, player, recognition...) | `track/recognition*.cpp`, `track/media.cpp`, `track/feed*.cpp` | `src/Taiga.Track` | `Recognition/*`, `EpisodeParser`, `FeedMonitor`, `MediaDetector` | 🔲 pendiente |
| `link/` (discord, http, mirc) | `link/*.cpp` | `src/Taiga.Core` (`Link/`) o `Taiga.Track` | `DiscordPresence`, `HttpServer`, `MircBridge` (alcance a confirmar, posible Out) | 🔲 pendiente |
| `ui/` (Win32 WTL: dlg, list, menu, theme, translate...) | `ui/**` (~80 ficheros) | `src/Taiga.App` (Avalonia) | `Views/*`, `ViewModels/*`, `i18n/*.resx` | 🔲 pendiente |
| `main.cpp` + `project/vs2022/` | entry Win32 + sln VS2022 | `src/Taiga.App` + `Taiga.sln` | `Program.cs` (multi-SO) | 🔲 pendiente |
| `deps/`, `res/`, `setup/`, `data/` | submodules, iconos, instalador NSIS | `setup/linux/` + `Taiga.Platform.*` | `deb/rpm/AppImage/tarball`, `.desktop`, iconos | 🔲 pendiente |

## Fixtures extraídas (ver `docs/migration/fixtures/`)

- `fixtures/parsers/`: respuestas AniList/Kitsu/MAL para tests de contrato.
- `fixtures/recognition/`: corpus de nombres de fichero → (anime, episodio).
- `fixtures/queue/`: secuencias offline/reintentos.

## Criterio de paridad 100%

Cada fila en ✅ con tests verdes en `ubuntu-latest` + `windows-latest` antes del corte `v3.0.0`.
