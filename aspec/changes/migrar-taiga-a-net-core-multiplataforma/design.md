# Design: Taiga multiplataforma — backend .NET + front Angular + shell Electron

## Approach

Sobre `feature/dotnet-migration`, conservando Fase 0–3 (Core/Sync/Track verificados) y sustituyendo la Fase 4 (Avalonia) por API + Angular + Electron:

1. **Taiga.Server (Fase 4a):** proyecto ASP.NET Core (`net10.0`) que expone en `http://127.0.0.1:<puerto>`:
   - `GET /api/library`, `GET /api/library/{id}`, `PUT /api/library/{id}` (progreso/estado/nota → actualiza + encola sync).
   - `GET /api/now-playing`, `POST /api/scan` (corre detectores → `UpdateDecider` → persiste).
   - `POST /api/sync` (vacía la cola contra el proveedor configurado), `GET /api/queue`.
   - `GET/PUT /api/settings` (incluye tokens por proveedor), `GET /api/history`.
   - `WS /ws/events` (now-playing y cambios de cola en push).
   - Puerto: `TAIGA_PORT` o efímero libre; token efímero (`TAIGA_TOKEN`, 256 bits) exigido en header `X-Taiga-Token` salvo loopback sin token en modo dev. OpenAPI en `/swagger` solo en `Development`.
   - Tests: `tests/Taiga.Server.Tests` con `WebApplicationFactory` (CRUD biblioteca, scan sin reproductor, sync sin proveedor, auth del token).
2. **frontend/ (Fase 4b):** Angular 19+ standalone (`npm ci`, `ng build --configuration production` → `frontend/dist/` embebido como `wwwroot` del server en release). Vistas: biblioteca (filtro por estado + búsqueda), detalle, historial, temporada (abanico de `EpisodeCount`/emisión si hay datos), ajustes (servicio, tokens, carpetas, intervalo). Tema oscuro por defecto. Sin SSR.
3. **electron/ (Fase 4c):** `main.ts` (arranca `resources/bin/Taiga.Server`, espera `/api/health`, abre ventana, instancia única, tray con "Detectar ahora" → `POST /api/scan`, "Sincronizar" → `POST /api/sync`, "Salir"). `preload.ts` con `contextBridge` mínimo (el front habla directo al server por HTTP/WS; Electron solo shell). `electron-builder.yml`: `nsis` (exe x64), `deb` (x64), `dir` (base del tarball y del PKGBUILD).
4. **Empaquetado (Fase 5):**
   - Windows: `Taiga Setup <versión>.exe` (NSIS, per-user, sin firma — uso personal).
   - Debian/Ubuntu: `taiga_<versión>_amd64.deb` (depende solo de libc/webkit del sistema vía Electron; el server es self-contained).
   - **Arch**: `setup/arch/PKGBUILD` (`pkgname=taiga-bin`, `source=(taiga-$VERSION-linux-x64.tar.gz)`, `sha256sums`, instala en `/opt/taiga`, `.desktop` en `/usr/share/applications`, symlink `/usr/bin/taiga`; `makepkg -si`; `.SRCINFO` generado con `makepkg --printsrcinfo`). El tarball incluye `Taiga.Server` + `frontend/dist` + `electron/dist` + `taiga.desktop`.
   - `setup/linux/pack-tarball.sh` se extiende para armar ese tarball unificado.
5. **Eliminación de Avalonia:** `git rm src/Taiga.App tests/Taiga.App.Tests`, quitar del `.slnx`; `tests/Taiga.App.Tests` se sustituye por `tests/Taiga.Server.Tests`.

Decisiones clave: **loopback + token efímero** (sin auth pesada, sin exponer LAN); **server sirve el front** (un solo proceso, cero CORS en prod); **electron-builder** para exe/deb y `dir` como materia prima de Arch; **PKGBUILD `-bin`** (aceptado por AUR para binarios).

## Architecture

```text
Taiga.slnx (net10.0)
├── src/Taiga.Core/     # sin cambios (Media, Settings, Platform, Text, Compat, Taiga)
├── src/Taiga.Sync/     # sin cambios (Queue, Service, Providers MAL/AniList/Kitsu)
├── src/Taiga.Track/    # sin cambios (Parser, Recognition, UpdateDecider, Detectores)
├── src/Taiga.Server/   # NUEVO ASP.NET Core (Program.cs, Endpoints/*, wwwroot <- frontend/dist)
│   └── Endpoints/Library, NowPlaying, Scan, Sync, Settings, History, Health + EventsHub(WS)
├── tests/Taiga.Server.Tests/  # NUEVO (WebApplicationFactory)
├── frontend/           # NUEVO Angular (app/routes: library, detail, history, season, settings)
├── electron/           # NUEVO (main.ts, preload.ts, electron-builder.yml)
└── setup/
    ├── linux/pack-tarball.sh   # tarball unificado (server+front+electron dir)
    └── arch/{PKGBUILD,taiga.desktop,README-arch.md}
```

Flujos principales:

- **Arranque (Electron):** elige puerto libre → lanza `Taiga.Server --port X --token Y` → espera `GET /api/health` → abre `http://127.0.0.1:X/` en la ventana. Al salir, mata el sidecar.
- **Detección → UI:** `POST /api/scan` (o polling del front cada N s a `GET /api/now-playing`) → playerctl/`/proc` → parser → `Recognition` → `UpdateDecider` → persiste biblioteca + encola → evento WS → el front refresca.
- **Sync:** `POST /api/sync` → `SyncService.FlushAsync` contra proveedor con token de `ProviderCredentials`.
- **Instalación Arch:** `cd setup/arch && makepkg -si` (o `yay -S taiga-bin` si se sube al AUR) → `/opt/taiga/taiga` + `.desktop` → aparece en el lanzador; `taiga` en terminal.

## Validation

- **API:** `dotnet test` (contrato de cada endpoint + token requerido/rechazado + scan sin reproductor no revienta + sync sin proveedor deja cola).
- **Front:** `npm ci && npm run build` verde; navegación biblioteca→detalle→ajustes sin errores de consola; `ng test` si hay specs (mínimo: servicio API con HttpClient stub).
- **Shell:** `electron-builder --dir` genera el árbol; arranque manual abre la ventana contra el server local (smoke con `--self-test`-equivalente: `Taiga.Server --self-test` reutiliza las comprobaciones del Core).
- **Instaladores:** `.deb` instala en Ubuntu/Debian y abre desde el dock; `.exe` NSIS instala per-user en Windows; en Arch `makepkg -si` instala y `taiga --self-test` (server) pasa; `.desktop` visible en GNOME/KDE.
- **Release:** tag `v3.0.0-net10-preview.2` → `release.yml` produce exe + deb + tarball + PKGBUILD con SHA256 en el GitHub Release.
