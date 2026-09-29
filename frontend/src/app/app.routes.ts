import { Routes } from '@angular/router';
import { LibraryComponent } from './library/library.component';
import { DetailComponent } from './detail/detail.component';
import { HistoryComponent } from './history/history.component';
import { SeasonComponent } from './season/season.component';
import { SettingsComponent } from './settings/settings.component';

export const routes: Routes = [
  { path: '', component: LibraryComponent },
  { path: 'anime/:id', component: DetailComponent },
  { path: 'history', component: HistoryComponent },
  { path: 'season', component: SeasonComponent },
  { path: 'settings', component: SettingsComponent },
];
