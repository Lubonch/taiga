import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, Anime } from '../api.service';

@Component({
  selector: 'app-detail',
  template: `
    @if (anime(); as a) {
      <h2>{{ a.title }}</h2>
      @if (a.englishTitle) {
        <p class="dim">{{ a.englishTitle }}</p>
      }
      <p>Vistos: {{ a.watchedEpisodes }}{{ a.episodeCount ? '/' + a.episodeCount : '' }}</p>
      <p>Puntuación: {{ a.myScore }}/10</p>
      @if (a.synopsis) {
        <p>{{ a.synopsis }}</p>
      }
      <div class="row">
        <button (click)="bump(1)">+1 episodio</button>
        <button (click)="setStatus(2)">Completar</button>
        <label>Puntos <input #sc type="number" min="0" max="10" [value]="a.myScore" /></label>
        <button (click)="setScore(+sc.value)">Guardar puntos</button>
      </div>
      <p class="statusline">{{ msg() }}</p>
      <a routerLink="/">← Biblioteca</a>
    } @else {
      <p>Cargando…</p>
    }
  `,
  imports: [RouterLink],
})
export class DetailComponent {
  private api = inject(ApiService);
  private route = inject(ActivatedRoute);

  anime = signal<Anime | null>(null);
  msg = signal('');

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.api.library().subscribe({
      next: (items) => this.anime.set(items.find((a) => a.id === id) ?? null),
      error: () => this.msg.set('Sin conexión.'),
    });
  }

  private patch(body: Record<string, unknown>): void {
    const a = this.anime();
    if (!a) {
      return;
    }
    this.api.updateEntry(a.id, body).subscribe({
      next: (updated) => {
        this.anime.set(updated);
        this.msg.set('Guardado y encolado para sync.');
      },
      error: () => this.msg.set('Error al guardar.'),
    });
  }

  bump(d: number): void {
    const a = this.anime();
    if (a) {
      this.patch({ episode: a.watchedEpisodes + d });
    }
  }

  setStatus(s: number): void {
    this.patch({ status: s });
  }

  setScore(s: number): void {
    this.patch({ score: s });
  }
}
