# Taiga en Arch Linux

Paquete binario `taiga-bin`: instala el tarball del GitHub Release en
`/opt/taiga`, con `.desktop` y symlink `/usr/bin/taiga`.

## Instalar localmente

```bash
cd setup/arch
makepkg -si
```

## Desde AUR (cuando se publique)

```bash
yay -S taiga-bin
# o: paru -S taiga-bin
```

## Probar sin instalar

```bash
./setup/linux/pack-tarball.sh 3.0.0-net10-preview.2
tar -xzf dist/taiga-3.0.0-net10-preview.2-linux-x64.tar.gz -C /tmp
/tmp/taiga-3.0.0-net10-preview.2-linux-x64/resources/bin/Taiga.Server --self-test
```

## Notas

- El binario `taiga-shell` (Electron) necesita X11 o Wayland para la ventana;
  el server funciona headless (`Taiga.Server --port 6599` + API en `127.0.0.1`).
- Detección MPRIS: instala `playerctl` (`pacman -S playerctl`) o usa
  reproductores con MPRIS (mpv, VLC, celluloid, haruna).
