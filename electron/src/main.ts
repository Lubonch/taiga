import { ChildProcess, spawn } from 'node:child_process';
import * as net from 'node:net';
import * as path from 'node:path';
import { app, BrowserWindow, Tray, Menu, shell, dialog } from 'electron';

let win: BrowserWindow | null = null;
let server: ChildProcess | null = null;
let tray: Tray | null = null;
let apiBase = '';
let apiToken = '';

function serverBinary(): string {
  return (
    process.env['TAIGA_SERVER_PATH'] ??
    path.join(process.resourcesPath, 'bin', process.platform === 'win32' ? 'Taiga.Server.exe' : 'Taiga.Server')
  );
}

function freePort(): Promise<number> {
  return new Promise((resolve) => {
    const s = net.createServer();
    s.listen(0, '127.0.0.1', () => {
      const port = (s.address() as net.AddressInfo).port;
      s.close(() => resolve(port));
    });
  });
}

async function waitHealth(timeoutMs = 20000): Promise<void> {
  const start = Date.now();
  while (Date.now() - start < timeoutMs) {
    try {
      const res = await fetch(`${apiBase}/api/health`, { headers: { 'X-Taiga-Token': apiToken } });
      if (res.ok) {
        return;
      }
    } catch {
      /* aún arrancando */
    }
    await new Promise((r) => setTimeout(r, 300));
  }
  throw new Error('Taiga.Server no respondió a /api/health');
}

async function api(path_: string, init?: RequestInit): Promise<string> {
  const res = await fetch(`${apiBase}${path_}`, {
    ...init,
    headers: { 'X-Taiga-Token': apiToken, 'Content-Type': 'application/json', ...(init?.headers ?? {}) },
  });
  return `${res.status} ${await res.text()}`;
}

async function startServer(): Promise<void> {
  const port = await freePort();
  apiBase = `http://127.0.0.1:${port}`;
  server = spawn(serverBinary(), ['--port', String(port)], { stdio: ['ignore', 'pipe', 'inherit'] });
  server.stdout?.setEncoding('utf8');

  apiToken = await new Promise<string>((resolve, reject) => {
    let buf = '';
    const timer = setTimeout(() => reject(new Error('sin TAIGA_TOKEN en stdout')), 20000);
    server?.stdout?.on('data', (chunk: string) => {
      buf += chunk;
      const m = /^TAIGA_TOKEN=(\S+)/m.exec(buf);
      if (m?.[1]) {
        clearTimeout(timer);
        resolve(m[1]);
      }
    });
    server?.on('error', reject);
    server?.on('exit', (code) => reject(new Error(`server exit ${code}`)));
  });

  process.env['TAIGA_PORT'] = String(port);
  process.env['TAIGA_TOKEN'] = apiToken;
  await waitHealth();
}

function createWindow(): void {
  win = new BrowserWindow({
    width: 1100,
    height: 750,
    autoHideMenuBar: true,
    webPreferences: { preload: path.join(__dirname, 'preload.js') },
  });
  win.webContents.setWindowOpenHandler(({ url }) => {
    void shell.openExternal(url);
    return { action: 'deny' };
  });
  void win.loadURL(apiBase);
  win.on('closed', () => (win = null));
}

function createTray(): void {
  tray = new Tray(path.join(__dirname, '..', 'assets', 'icon.png'));
  const menu = Menu.buildFromTemplate([
    {
      label: 'Detectar ahora',
      click: () =>
        api('/api/scan', { method: 'POST' }).catch((e: unknown) =>
          dialog.showErrorBox('Taiga', `Scan falló: ${String(e)}`),
        ),
    },
    {
      label: 'Sincronizar',
      click: () =>
        api('/api/sync', { method: 'POST' }).catch((e: unknown) =>
          dialog.showErrorBox('Taiga', `Sync falló: ${String(e)}`),
        ),
    },
    { type: 'separator' },
    {
      label: 'Salir',
      click: () => {
        server?.kill();
        app.quit();
      },
    },
  ]);
  tray.setToolTip('Taiga');
  tray.setContextMenu(menu);
  tray.on('click', () => win?.show());
}

async function boot(): Promise<void> {
  const gotLock = app.requestSingleInstanceLock();
  if (!gotLock) {
    app.quit();
    return;
  }
  app.on('second-instance', () => win?.show());

  await app.whenReady();
  try {
    await startServer();
  } catch (e) {
    dialog.showErrorBox('Taiga', `No se pudo arrancar Taiga.Server: ${String(e)}`);
    app.quit();
    return;
  }
  createWindow();
  try {
    createTray();
  } catch {
    /* sin icono disponible: la app sigue funcionando */
  }
  app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
      server?.kill();
      app.quit();
    }
  });
}

void boot();
