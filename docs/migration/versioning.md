# Versionado y releases — migración .NET

## Esquema

SemVer alineado a `v1.4.1` (base `master`) → `v3.0.0` estable .NET (salto major por reescritura + cambio de plataforma).

- Previews: `v3.0.0-net10-preview.N` (N incremental) desde `feature/dotnet-migration`.
- Estable: `v3.0.0` desde `master` tras merge por PR.
- Tags: `git tag -a v3.0.0-net10-preview.1 -m "..." && git push origin v3.0.0-net10-preview.1` (remote SSH: `git@github.com:Lubonch/taiga.git`).

## Branch

- Trabajo: `feature/dotnet-migration` (creado desde `origin/master`, publicado vía SSH).
- Integración por fases (Core → Sync → Track → App) con PRs a `master`. Sin commits directos a `master`.
- Protección requerida (manual en GitHub UI, no hay `gh` en entorno):
  `Settings → Branches → Add rule → Branch name pattern: feature/dotnet-migration`:
  - Require a pull request before merging
  - Require status checks to pass (`dotnet-ci`, `release-dry-run`)
  - Do not allow bypassing the above settings

## Release

Workflow `.github/workflows/release.yml` (trigger tag `v*`): build + test (`linux-x64`, `win-x64`) → publish self-contained + framework-dependent → empaquetar `.deb/.rpm/AppImage/tar.gz/zip` → SHA256 → GitHub Release con notas generadas.

## Corte a estable

Paridad 100% (`parity-matrix.md`), CI verde Linux+Windows, cobertura ≥ 70% Core/Sync/Track, smoke E2E OK (login 3 proveedores, MPRIS + Win32, scrobbling, persistencia) → PR a `master` → tag `v3.0.0`.
