## ADDED Requirements

### Requirement: Backend .NET con API local

The system SHALL exponer la funcionalidad (biblioteca, now-playing, escaneo, sincronización, ajustes, historial) vía API HTTP local en `127.0.0.1` con token efímero, sirviendo además el front Angular.

#### Scenario: Scan vía API sin reproductor

- **WHEN** se llama `POST /api/scan` sin reproductor en ejecución
- **THEN** responde 200 con estado vacío, sin errores, y la biblioteca queda intacta

#### Scenario: API exige token

- **WHEN** se llama a `/api/library` sin el header `X-Taiga-Token` correcto
- **THEN** responde 401; y
- **WHEN** se llama con el token impreso por el server al arrancar
- **THEN** responde 200

#### Scenario: Sync sin proveedor configurado

- **WHEN** se llama `POST /api/sync` sin tokens de AniList/Kitsu/MAL
- **THEN** responde con enviados 0, la cola se conserva y el mensaje indica que falta configuración

### Requirement: Front Angular instalable vía Electron

The system SHALL ofrecer la UI (biblioteca, detalle, historial, búsqueda, temporada, ajustes) como app Angular dentro de una ventana Electron indistinguible de una app de escritorio clásica.

#### Scenario: Navegación completa sin consola de errores

- **WHEN** el usuario abre biblioteca, entra al detalle de un anime, revisa historial y guarda ajustes
- **THEN** todas las vistas cargan desde la API local sin errores en consola

### Requirement: Instaladores Windows (exe), Debian (deb) y Arch Linux

The system SHALL distribuirse como instalador `exe` (NSIS) para Windows, `.deb` para Debian/Ubuntu y paquete pacman para Arch Linux (`PKGBUILD` estilo `-bin` + `.desktop`), todos con checksums SHA256 desde tags `v*`.

#### Scenario: Instalación en Arch con pacman

- **WHEN** el usuario ejecuta `makepkg -si` en `setup/arch/`
- **THEN** el paquete `taiga-bin` se instala en `/opt/taiga` con `.desktop` y symlink `/usr/bin/taiga`; y
- **WHEN** lanza `taiga` desde el menú o terminal
- **THEN** abre la ventana con la biblioteca y el server local corriendo

#### Scenario: Instalación deb y exe

- **WHEN** el usuario instala el `.deb` en Ubuntu/Debian o el `Setup.exe` en Windows
- **THEN** la app aparece en el lanzador/menú inicio y arranca sin dependencias manuales

### Requirement: Detección multimedia en Linux

The system SHALL detectar el episodio en reproducción en Linux vía MPRIS/playerctl con fallback a `/proc`, con la misma decisión de actualización que en Windows.

#### Scenario: Detección vía MPRIS

- **WHEN** MPV o VLC reproduce un fichero reconocido en sesión X11 o Wayland
- **THEN** `GET /api/now-playing` refleja anime y episodio, y la biblioteca se actualiza con la entrada en cola de sync

### Requirement: Sincronización con AniList, Kitsu y MyAnimeList

The system SHALL sincronizar progreso, estados y puntuaciones con AniList, Kitsu y MyAnimeList mediante tokens configurados en Ajustes, cola persistente offline y reintentos con backoff.

#### Scenario: Scrobbling offline con reintentos

- **WHEN** no hay red durante la detección de un episodio
- **THEN** la actualización queda en cola y `POST /api/sync` la reintenta sin duplicar entradas

### Requirement: Branch dedicado de migración

The system SHALL desarrollarse en el branch `feature/dotnet-migration` con integración por fases vía PRs hacia `master`.

#### Scenario: Protección del branch

- **WHEN** se intenta mergear a `master` sin CI verde
- **THEN** el merge queda bloqueado hasta que `dotnet-ci` (y `web-ci` si aplica) pase
