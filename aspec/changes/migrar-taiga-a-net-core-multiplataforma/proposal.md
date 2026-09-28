# Proposal: Taiga multiplataforma — backend .NET + front Angular + shell Electron

## Problem

Taiga es una aplicación de escritorio solo-Windows (C++ + Win32/WTL en `src/ui/`, ~15 diálogos: main, lista, info, historial, búsqueda, temporada, ajustes, feed, torrent, update, about, más bandeja, menús y temas). No corre en Linux y no hay instaladores para otras plataformas.

El branch `feature/dotnet-migration` ya contiene el backend portado y verificado en Linux: `Taiga.Core` (biblioteca, ajustes XDG/portable, reconocimiento), `Taiga.Sync` (cola offline + AniList/Kitsu/MAL) y `Taiga.Track` (detección playerctl/`/proc`), con 36 tests verdes y binario `linux-x64` autocontenido. El front Avalonia (`src/Taiga.App`) fue un primer paso funcional, pero se decide cambiarlo por **Angular + Electron** para tener una UI web instalable como aplicación de escritorio clásica.

## Proposed change

Reconstruir Taiga como aplicación instalable multiplataforma con:

1. **Backend .NET 10** (`Taiga.slnx`): se conserva `Taiga.Core`/`Taiga.Sync`/`Taiga.Track` y se añade `Taiga.Server` (ASP.NET Core en `localhost`) que expone la API local: biblioteca, now-playing, escaneo, sincronización, ajustes e historial. `src/Taiga.App` (Avalonia) queda **superseded** y se elimina.
2. **Front Angular** (`frontend/`): SPA con biblioteca, detalle, historial, búsqueda, temporada, ajustes (servicio, tokens, carpetas) y estado de cola; consume la API local.
3. **Shell Electron** (`electron/`): ventana de escritorio, instancia única, bandeja (tray) con "Detectar ahora", empaquetado por plataforma. El binario `Taiga.Server` viaja como sidecar y Electron lo arranca al inicio.
4. **Instaladores por plataforma**:
   - Windows: `Taiga Setup.exe` (NSIS vía `electron-builder`).
   - Debian/Ubuntu: `.deb` (vía `electron-builder`).
   - **Arch Linux**: `PKGBUILD` en `setup/arch/` (paquete `-bin` que instala en `/opt/taiga` + `.desktop` + symlink `/usr/bin/taiga`; instalable con `makepkg -si`, listo para AUR) + tarball genérico.
5. Mismo branch `feature/dotnet-migration`, mismo versionado (`v3.0.0-net10-preview.N`), releases por tag `v*` con SHA256.

Uso previsto: **personal, no público** — se prioriza que sea instalable y usable en Windows, Debian y Arch sobre paridad estética total con el cliente Win32.

## Scope

### In scope

- `Taiga.Server`: API local (`/api/library`, `/api/now-playing`, `/api/scan`, `/api/sync`, `/api/settings`, `/api/history`) + WebSocket de eventos + tests de contrato (xUnit + `WebApplicationFactory`).
- `frontend/` Angular: vistas biblioteca, detalle, historial, búsqueda, temporada, ajustes; `ng build` integrado al empaquetado.
- `electron/`: main process (arranque/parada del sidecar, puerto libre, instancia única, tray, menú), preload seguro (`contextIsolation`), `electron-builder` para exe + deb + dir.
- `setup/arch/PKGBUILD` + `.SRCINFO` + `taiga.desktop` + instrucciones (`makepkg -si`, `yay -S taiga-bin` cuando esté en AUR); tarball `linux-x64` como fallback universal.
- CI: `dotnet build/test` + `npm ci/build` + dry-run de empaquetado; `release.yml` por tag genera exe, deb, tarball (+ PKGBUILD como artefacto fuente).

### Out of scope

- Publicación en Microsoft Store, Snap/Flatpak o repos oficiales Debian/Arch (solo AUR como contribución futura; no se mantiene infraestructura de firmado).
- OAuth PKCE interactivo completo (los tokens se pegan en Ajustes; el flujo navegador se deja como follow-up).
- Autostart por plataforma, Secret Service/Credential Manager y D-Bus nativo (siguen diferidos como en la fase anterior).
- Soporte macOS.

## Risks

- Doble runtime (Chromium + .NET, ~200 MB por instalador): aceptado por ser uso personal; mitigación con sidecar self-contained único y `electron-builder` solo con lo necesario.
- Puerto localhost ocupado o bloqueado: mitigación con selección de puerto libre + token efímero impreso por el server y leído por Electron (nunca fijo en código).
- Sincronía de versiones API/front empaquetados: mitigación versionando ambos desde `AppInfo.Version` y chequeo de compatibilidad al arrancar.
- Deriva del front Avalonia ya escrito: mitigación eliminándolo (`git rm src/Taiga.App tests/Taiga.App.Tests`) para no mantener dos UIs.
- AUR rechaza binarios sin fuente: mitigación con paquete `-bin` (pattern aceptado) + `tarball` como `source()` con `sha256sums`.
