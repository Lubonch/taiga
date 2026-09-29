import { Component, inject, signal } from '@angular/core';
import { ApiService } from '../api.service';

@Component({
  selector: 'app-settings',
  template: `
    <h2>Ajustes</h2>
    <div class="form">
      <label>Servicio
        <select #svc [value]="settings()['service'] ?? 'MyAnimeList'">
          <option>MyAnimeList</option>
          <option>AniList</option>
          <option>Kitsu</option>
        </select>
      </label>
      <label>Usuario <input #usr [value]="settings()['username'] ?? ''" /></label>
      <label>Token MyAnimeList <input #mal type="password" /></label>
      <label>Token AniList <input #al type="password" /></label>
      <label>Token Kitsu <input #kitsu type="password" /></label>
      <label>Token API local (dev) <input #tok type="password" /></label>
      <label>Intervalo (s) <input #poll type="number" [value]="settings()['pollIntervalSeconds'] ?? 5" /></label>
      <button (click)="save(svc.value, usr.value, mal.value, al.value, kitsu.value, tok.value, +poll.value)">
        Guardar
      </button>
      <p class="statusline">{{ msg() }}</p>
    </div>
  `,
})
export class SettingsComponent {
  private api = inject(ApiService);
  settings = signal<Record<string, unknown>>({});
  msg = signal('');

  constructor() {
    this.api.settings().subscribe({
      next: (s) => this.settings.set(s),
      error: () => this.msg.set('Sin conexión.'),
    });
  }

  save(service: string, username: string, mal: string, al: string, k: string, tok: string, poll: number): void {
    if (tok) {
      this.api.setToken(tok);
    }
    const tokens: Record<string, string> = {};
    if (mal) {
      tokens['myanimelist'] = mal;
    }
    if (al) {
      tokens['anilist'] = al;
    }
    if (k) {
      tokens['kitsu'] = k;
    }
    this.api
      .saveSettings({ service, username, pollIntervalSeconds: poll, tokens })
      .subscribe({
        next: () => this.msg.set('Ajustes guardados.'),
        error: () => this.msg.set('Error al guardar (¿token API local?).'),
      });
  }
}
