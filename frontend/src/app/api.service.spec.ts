import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ApiService } from './api.service';

describe('ApiService', () => {
  let api: ApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(ApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('library() hace GET a /api/library', () => {
    api.library().subscribe((items) => expect(items.length).toBe(1));
    const req = http.expectOne('/api/library');
    expect(req.request.method).toBe('GET');
    req.flush([{ id: 1, title: 'Naruto' }]);
  });

  it('scan() hace POST a /api/scan', () => {
    api.scan().subscribe((r) => expect(r.playing).toBeFalse());
    const req = http.expectOne('/api/scan');
    expect(req.request.method).toBe('POST');
    req.flush({ playing: false });
  });
});
