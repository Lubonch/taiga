import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface Anime {
  id: number;
  title: string;
  englishTitle: string;
  episodeCount: number;
  watchedEpisodes: number;
  myStatus: number;
  myScore: number;
  isInList: boolean;
  imageUrl: string;
  synopsis: string;
  notes: string;
}

export interface QueueItem {
  animeId: number;
  episode: number;
  status: number;
  attempts: number;
  lastError?: string;
}

export interface NowPlaying {
  isPlaying?: boolean;
  playing?: boolean;
  player?: string;
  reason?: string;
}

const TOKEN_KEY = 'taiga-token';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  /** El shell Electron inyecta token/puerto vía `window.__TAIGA__`; en dev, prompt manual. */
  private base(): string {
    const cfg = (window as unknown as { __TAIGA__?: { port: number } }).__TAIGA__;
    return cfg ? `http://127.0.0.1:${cfg.port}` : '';
  }

  private headers(): Record<string, string> {
    const token =
      (window as unknown as { __TAIGA__?: { token: string } }).__TAIGA__?.token ??
      localStorage.getItem(TOKEN_KEY) ??
      '';
    return token ? { 'X-Taiga-Token': token } : {};
  }

  setToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
  }

  library() {
    return this.http.get<Anime[]>(`${this.base()}/api/library`, { headers: this.headers() });
  }

  updateEntry(id: number, body: { episode?: number; status?: number; score?: number; notes?: string }) {
    return this.http.put<Anime>(`${this.base()}/api/library/${id}`, body, { headers: this.headers() });
  }

  nowPlaying() {
    return this.http.get<NowPlaying>(`${this.base()}/api/now-playing`, { headers: this.headers() });
  }

  scan() {
    return this.http.post<NowPlaying>(`${this.base()}/api/scan`, {}, { headers: this.headers() });
  }

  queue() {
    return this.http.get<QueueItem[]>(`${this.base()}/api/queue`, { headers: this.headers() });
  }

  sync() {
    return this.http.post<{ sent: number; failed: number; remaining: number; note?: string }>(
      `${this.base()}/api/sync`, {}, { headers: this.headers() });
  }

  settings() {
    return this.http.get<Record<string, unknown>>(`${this.base()}/api/settings`, { headers: this.headers() });
  }

  saveSettings(body: Record<string, unknown>) {
    return this.http.put(`${this.base()}/api/settings`, body, { headers: this.headers() });
  }

  history() {
    return this.http.get<unknown[]>(`${this.base()}/api/history`, { headers: this.headers() });
  }
}
