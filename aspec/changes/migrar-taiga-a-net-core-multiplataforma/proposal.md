# Proposal: Migración completa de Taiga a .NET moderno multiplataforma (Linux)

## Problem

Taiga es una aplicación de escritorio open-source para Windows (C++23 + CMake + Qt6 Widgets, `src/CMakeLists.txt`, `WIN32_EXECUTABLE ON`, `OUTPUT_NAME Taiga`). El repositorio (`README.md:8`) la define explícitamente como "desktop application for Windows" para detectar anime visto y sincronizar con AniList, Kitsu y MyAnimeList.

No existe ningún proyecto .NET en el repo (cero ficheros `*.csproj`, `*.sln`, `*.cs`). El código actual contiene acoplamientos Windows:

- Entrada Win32 (`WIN32_EXECUTABLE`, `src/main.cpp`, `src/taiga/application.cpp`).
- Detección de reproductores y ventanas (`src/track/media_player.cpp`, `src/track/scanner.cpp`, `src/gui/platforms/`).
- Rutas, autostart, registro y modo portable (`src/taiga/path.cpp`, `src/base/file.cpp`, `TAIGA_PORTABLE`).
- Recursos Qt Widgets y traducciones (`src/gui/`, `src/resources/`).

Resultado: no corre en Linux, no hay pipeline de release Linux, y mantener C++ Windows-only limita contribuidores y distribución multiplataforma.

## Proposed change

Migrar por completo la aplicación a .NET moderno LTS (objetivo: .NET 10 LTS; mínimo aceptable: .NET 8 LTS) multiplataforma, capaz de correr nativamente en Linux (además de Windows como regresión), con:

1. Nueva solución .NET (`Taiga.sln`) que reimplementa los dominios actuales: `base`, `compat`, `media`, `sync/anilist|kitsu|myanimelist`, `taiga`, `track`, `gui`.
2. UI de escritorio multiplataforma con Avalonia UI (reemplazo de Qt Widgets), manteniendo paridad funcional: biblioteca, listas, historial, búsqueda, ajustes, sincronización.
3. Abstracciones de plataforma (`IPlatformServices`, `IMediaDetector`, `IPathProvider`) con implementaciones `Windows` y `Linux` (X11/Wayland, MPRIS, inotify).
4. Branch dedicado `feature/dotnet-migration` (desde `master`) como rama de trabajo de la migración, con PRs por fase hacia `master`.
5. Pipeline CI/CD multiplataforma y generación de release Linux (`.deb`/`.rpm`/`AppImage`/`tar.gz` + `win-x64` como regresión) con versionado SemVer alineado a `project(Taiga VERSION 2.0.0)`.

## Scope

### In scope

- Inventario y mapeo C++ → C# de todos los módulos en `src/`.
- Nueva solución .NET 10 con proyectos `Taiga.Core`, `Taiga.Sync`, `Taiga.Track`, `Taiga.App` (Avalonia), `Taiga.Tests`.
- Persistencia y settings multiplataforma (reemplazo de `QSettings`/SQLite Qt por `Microsoft.Data.Sqlite` + `System.Text.Json`).
- Red (HttpClient + OAuth2 para AniList/Kitsu/MAL), RSS, XML y logging con abstracciones testeables.
- Detección multimedia en Linux (MPRIS + escaneo de procesos/ventanas) con paridad de casos Windows.
- CI (build+test en `ubuntu-latest` y `windows-latest`), análisis, firma/versionado y release automatizado.
- Creación del branch `feature/dotnet-migration`, estrategia de merge y primer release `v2.1.0-net10-preview` / `v3.0.0` estable Linux.

### Out of scope

- Nuevas funcionalidades no presentes en v2.0.0 (p. ej. nuevos proveedores, apps móviles, plugins).
- Migración automática de datos v1 salvo importador `compat/` documentado (se implementa solo lector/importador, no soporte v1 completo).
- Empaquetado Snap/Flatpak oficial en fase 1 (se deja como follow-up tras AppImage/deb estables).
- Soporte macOS oficial en fase 1 (el diseño no lo bloquea, pero no se valida ni se releasea).

## Risks

- Reescritura total subestima paridad funcional (AniList/Kitsu/MAL, reconocimiento, cola sync): mitigación con matriz de paridad por módulo + tests de contrato contra fixtures de `src/sync/*_parsers.cpp` y `src/track/recognition*`.
- Detección multimedia diverge entre Win32 y Linux (X11/Wayland/MPRIS incompleto): mitigación con `IMediaDetector` por plataforma + modo fallback por escaneo de títulos/procesos + telemetría de `update_decision`.
- Qt Widgets → Avalonia cambia UX y atajos/temas: mitigación con inventario `src/gui/**` pantalla por pantalla y criterios de aceptación visual.
- Migración big-bang bloquea `master`: mitigación con branch `feature/dotnet-migration` + integración por fases (Core → Sync → Track → App) con CI verde por fase.
- OAuth/secretos y almacenamiento seguro difieren en Linux (Secret Service vs Credential Manager): mitigación con `ISecureStorage` + keyring Linux y docs de migración.
- Tamaño del cambio rompe releases: mitigación con releases preview `*-net10-preview` paralelos al binario Qt actual hasta corte `v3.0.0`.
