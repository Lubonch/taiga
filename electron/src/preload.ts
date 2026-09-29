import { contextBridge } from 'electron';

declare global {
  interface Window {
    __TAIGA__?: { port: number; token: string };
  }
}

// El front lee `window.__TAIGA__` (puerto + token del server local).
contextBridge.exposeInMainWorld('__TAIGA__', {
  port: Number(process.env['TAIGA_PORT'] ?? 6599),
  token: process.env['TAIGA_TOKEN'] ?? '',
});
