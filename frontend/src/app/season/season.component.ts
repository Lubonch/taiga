import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService, Anime } from '../api.service';

/** Temporada: "actualmente viendo" ordenado por progreso (base de la guía de temporada). */
@Component({
  selector: 'app-season',
  imports: [RouterLink],
  template: `
    <h2>Siguiendo</h2>
    <ul class="list">
      @for (a of watching(); track a.id) {
        <li>
          <a [routerLink]="['/anime', a.id]"><strong>{{ a.title }}</strong></a>
          <span>ep. {{ a.watchedEpisodes }}{{ a.episodeCount ? '/' + a.episodeCount : '' }}</span>
        </li>
      } @empty {
        <li class="dim">Nada en seguimiento.</li>
      }
    </ul>
  `,
})
export class SeasonComponent {
  private api = inject(ApiService);
  watching = signal<Anime[]>([]);

  constructor() {
    this.api.library().subscribe({
      next: (items) => this.watching.set(items.filter((a) => a.myStatus === 1)),
      error: () => undefined,
    });
  }
}
