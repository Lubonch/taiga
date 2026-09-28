## ADDED Requirements

### Requirement: Ejecución nativa en Linux con .NET moderno

The system SHALL ejecutarse nativamente en Linux x64 sobre .NET 10 LTS (con .NET 8 LTS como mínimo aceptable) manteniendo la paridad funcional v2.0.0 de gestión de biblioteca y scrobbling.

#### Scenario: Arranque en Ubuntu sin .NET previo

- **WHEN** el usuario instala el paquete self-contained `linux-x64` en Ubuntu 24.04 sin runtime instalado
- **THEN** la aplicación arranca, carga ajustes XDG o modo portable y muestra la biblioteca sin errores Win32

#### Scenario: Persistencia multiplataforma

- **WHEN** el usuario cambia de Windows a Linux con los mismos datos importados desde `compat/`
- **THEN** listas, historial y sesión se conservan con rutas resueltas vía `IPathProvider` (XDG en Linux, `%AppData%` en Windows)

### Requirement: Detección multimedia en Linux

The system SHALL detectar el episodio en reproducción en Linux vía MPRIS D-Bus con fallback a títulos de ventana/procesos, con la misma decisión de actualización que en Windows.

#### Scenario: Detección vía MPRIS

- **WHEN** MPV o VLC reproduce un fichero reconocido vía MPRIS en sesión X11 o Wayland
- **THEN** el sistema identifica anime y episodio, aplica `UpdateDecision` y encola la actualización igual que con `media_player.cpp`/`scanner.cpp` en Windows

### Requirement: Sincronización con AniList, Kitsu y MyAnimeList

The system SHALL sincronizar progreso, estados y puntuaciones con AniList, Kitsu y MyAnimeList mediante OAuth2, cola persistente offline y manejo de errores/rate-limit por proveedor.

#### Scenario: Scrobbling offline con reintentos

- **WHEN** no hay red durante la detección de un episodio
- **THEN** la actualización queda en `Queue` SQLite y se reintenta con backoff hasta confirmación del proveedor sin duplicar entradas

### Requirement: Branch dedicado de migración

The system SHALL desarrollarse en el branch `feature/dotnet-migration` creado desde `master`, con protección de rama e integración por fases vía PRs.

#### Scenario: Creación y protección del branch

- **WHEN** se ejecuta `git checkout -b feature/dotnet-migration master && git push -u origin feature/dotnet-migration`
- **THEN** el branch existe en remoto, requiere CI verde y revisión para merge, y ningún cambio de migración va directo a `master`

### Requirement: Release multiplataforma automatizado

The system SHALL generar releases versionados SemVer para `linux-x64` y `win-x64` (`.deb`, `.rpm`, `AppImage`, `tar.gz`, `zip` Windows) con checksums SHA256 desde tags `v*` vía GitHub Actions.

#### Scenario: Publicación de preview y estable

- **WHEN** se publica el tag `v3.0.0-net10-preview.1` desde el branch de migración
- **THEN** el workflow `release.yml` compila, testea, empaqueta y publica un GitHub Release descargable con notas; y
- **WHEN** se cumple paridad 100% + CI verde + QA Linux/Windows
- **THEN** se hace merge a `master` y se publica `v3.0.0` estable con los mismos artefactos
