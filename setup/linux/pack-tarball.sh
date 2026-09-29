#!/usr/bin/env bash
# Arma el tarball unificado: shell Electron (dir) + sidecar Taiga.Server + front.
# Uso: ./setup/linux/pack-tarball.sh [versión]  (por defecto: 3.0.0-net10-preview.2)
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
VERSION="${1:-3.0.0-net10-preview.2}"
OUT="$ROOT/dist"
STAGE="$OUT/taiga-$VERSION-linux-x64"

echo "==> frontend"
npm ci --prefix "$ROOT/frontend" >/dev/null 2>&1 || npm install --prefix "$ROOT/frontend"
npm run build --prefix "$ROOT/frontend"

echo "==> Taiga.Server (linux-x64 self-contained)"
dotnet publish "$ROOT/src/Taiga.Server/Taiga.Server.csproj" \
  -c Release -r linux-x64 --self-contained \
  -p:PublishSingleFile=true -o "$ROOT/publish/server-linux-x64"
mkdir -p "$ROOT/publish/server-linux-x64/wwwroot"
cp -r "$ROOT/frontend/dist/frontend/browser/"* "$ROOT/publish/server-linux-x64/wwwroot/"

echo "==> shell Electron (dir)"
npm run build --prefix "$ROOT/electron"
npm run dist-dir --prefix "$ROOT/electron"

echo "==> tarball"
rm -rf "$STAGE"
cp -r "$ROOT/electron/release/linux-unpacked" "$STAGE"
cp "$ROOT/setup/linux/taiga.desktop" "$STAGE/"
mkdir -p "$OUT"
tar -czf "$OUT/taiga-$VERSION-linux-x64.tar.gz" -C "$OUT" "taiga-$VERSION-linux-x64"
(cd "$OUT" && sha256sum "taiga-$VERSION-linux-x64.tar.gz" > "taiga-$VERSION-linux-x64.tar.gz.sha256")

echo "OK: $OUT/taiga-$VERSION-linux-x64.tar.gz"
"$STAGE/resources/bin/Taiga.Server" --self-test
