#!/usr/bin/env bash
# Empaqueta el build self-contained linux-x64 en tar.gz + .desktop.
# Uso: ./setup/linux/pack-tarball.sh [versión]  (por defecto: 3.0.0-net10-preview.1)
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
VERSION="${1:-3.0.0-net10-preview.1}"
OUT="$ROOT/dist"
STAGE="$OUT/taiga-$VERSION-linux-x64"

dotnet publish "$ROOT/src/Taiga.App/Taiga.App.csproj" \
  -c Release -r linux-x64 --self-contained \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o "$STAGE"

cp "$ROOT/setup/linux/taiga.desktop" "$STAGE/"
mkdir -p "$OUT"
tar -czf "$OUT/taiga-$VERSION-linux-x64.tar.gz" -C "$OUT" "taiga-$VERSION-linux-x64"
(cd "$OUT" && sha256sum "taiga-$VERSION-linux-x64.tar.gz" > "taiga-$VERSION-linux-x64.tar.gz.sha256")

echo "OK: $OUT/taiga-$VERSION-linux-x64.tar.gz"
"$STAGE/Taiga.App" --self-test
