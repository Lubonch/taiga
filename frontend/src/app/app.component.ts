import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  template: `
    <header>
      <h1>Taiga</h1>
      <nav>
        <a routerLink="/">Biblioteca</a>
        <a routerLink="/season">Siguiendo</a>
        <a routerLink="/history">Historial</a>
        <a routerLink="/settings">Ajustes</a>
      </nav>
    </header>
    <main>
      <router-outlet />
    </main>
  `,
})
export class AppComponent {}
