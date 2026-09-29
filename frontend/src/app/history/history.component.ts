import { Component, inject, signal } from '@angular/core';
import { ApiService } from '../api.service';

@Component({
  selector: 'app-history',
  template: `
    <h2>Historial</h2>
    <ul class="list">
      @for (h of items(); track $index) {
        <li>{{ entry(h) }}</li>
      } @empty {
        <li class="dim">Sin entradas todavía.</li>
      }
    </ul>
  `,
})
export class HistoryComponent {
  private api = inject(ApiService);
  items = signal<Record<string, unknown>[]>([]);

  constructor() {
    this.api.history().subscribe({
      next: (h) => this.items.set(h as Record<string, unknown>[]),
      error: () => undefined,
    });
  }

  entry(h: Record<string, unknown>): string {
    return `${h['title'] ?? '?'} ep. ${h['episode'] ?? '?'} · ${h['time'] ?? ''}`;
  }
}
