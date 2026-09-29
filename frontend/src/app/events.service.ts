import { Injectable, NgZone, inject } from '@angular/core';
import { Subject } from 'rxjs';

export interface TaigaEvent {
  type: string;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  payload: any;
}

/** Live-update por WS (`/ws/events`); reintenta con backoff si se cae. */
@Injectable({ providedIn: 'root' })
export class EventsService {
  private zone = inject(NgZone);
  readonly events = new Subject<TaigaEvent>();

  connect(): void {
    const cfg = (window as unknown as { __TAIGA__?: { port: number; token: string } }).__TAIGA__;
    if (!cfg) {
      return;
    }
    const ws = new WebSocket(`ws://127.0.0.1:${cfg.port}/ws/events`);
    ws.onmessage = (msg) => this.zone.run(() => this.events.next(JSON.parse(msg.data) as TaigaEvent));
    ws.onclose = () => setTimeout(() => this.connect(), 5000);
  }
}
