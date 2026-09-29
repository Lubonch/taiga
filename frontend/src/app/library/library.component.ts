import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService, Anime } from '../api.service';
import { EventsService } from '../events.service';

const STATUS_NAMES = ['Fuera de lista', 'Viendo', 'Completado', 'En pausa', 'Abandonado', 'Pendiente'];

@Component({
  selector: 'app-library',
  imports: [RouterLink],
  template: `
    <div class="toolbar">
      <input #q placeholder="Buscar…" (input)="query.set(q.value)" />
      <select #f (change)="filter.set(+f.value)">
        <option value="-1">Todos</option>
        <option value="1">Viendo</option>
        <option value="2">Completado</option>
        <option value="3">En pausa</option>
        <option value="4">Abandonado</option>
        <option value="5">Pendiente</option>
      </select>
      <button (click)="scan()">Detectar ahora</button>
      <button (click)="sync()">Sincronizar</button>
      <span class="now">{{ nowPlaying() }}</span>
    </div>
    <p class="statusline">{{ status() }}</p>
    <ul class="list">
      @for (a of visible(); track a.id) {
        <li>
          <a [routerLink]="['/anime', a.id]"><strong>{{ a.title }}</strong></a>
          <span>ep. {{ a.watchedEpisodes }}{{ a.episodeCount ? '/' + a.episodeCount : '' }}</span>
          <span class="dim">{{ statusName(a.myStatus) }}</span>
        </li>
      }
      @empty {
        <li class="dim">Biblioteca vacía. Añade anime actualizando desde el detalle o importa tus datos.</li>
      }
    </ul>
  `,
})
export class LibraryComponent {
  private api = inject(ApiService);
  private events = inject(EventsService);

  items = signal<Anime[]>([]);
  query = signal('');
  filter = signal(-1);
  nowPlaying = signal('Sin reproducción detectada');
  status = signal('');

  constructor() {
    this.refresh();
    this.events.connect();
    this.events.events.subscribe(() => this.refresh());
  }

  statusName(s: number): string {
    return STATUS_NAMES[s] ?? `Estado ${s}`;
  }

  visible(): Anime[] {
    const q = this.query().toLowerCase();
    const f = this.filter();
    return this.items().filter(
      (a) => (f < 0 || a.myStatus === f) && (!q || a.title.toLowerCase().includes(q)),
    );
  }

  refresh(): void {
    this.api.library().subscribe({
      next: (items) => {
        this.items.set(items);
        this.status.set(`${items.length} títulos`);
      },
      error: () => this.status.set('Sin conexión con Taiga.Server (¿token?). Pégalo en Ajustes.'),
    });
    this.api.queue().subscribe({
      next: (q) => q.length && this.status.update((s) => `${s} · ${q.length} en cola`),
      error: () => undefined,
    });
  }

  scan(): void {
    this.api.scan().subscribe({
      next: (r) => {
        this.nowPlaying.set(r.playing ? `${r.player}: ${r.reason}` : 'Sin reproducción detectada');
        this.refresh();
      },
      error: () => this.status.set('Error al detectar.'),
    });
  }

  sync(): void {
    this.api.sync().subscribe({
      next: (r) => this.status.set(r.note ?? `Enviados ${r.sent} · pendientes ${r.remaining}`),
      error: () => this.status.set('Error al sincronizar.'),
    });
  }
}
