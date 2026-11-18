import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let http: HttpTestingController;
  let router: { navigateByUrl: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    localStorage.clear();
    router = { navigateByUrl: vi.fn().mockResolvedValue(true) };
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: router },
      ],
    });
    httpClient = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('does not send an Authorization header without a token', () => {
    httpClient.get('/api/test').subscribe();

    const request = http.expectOne('/api/test');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });

  it('sends the Bearer token and keeps the session after 403', () => {
    localStorage.setItem('oilgas.token', 'test-token');
    httpClient.post('/api/admin', {}).subscribe({ error: () => undefined });

    const request = http.expectOne('/api/admin');
    expect(request.request.headers.get('Authorization')).toBe('Bearer test-token');
    request.flush({}, { status: 403, statusText: 'Forbidden' });

    expect(localStorage.getItem('oilgas.token')).toBe('test-token');
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });

  it('clears the session and opens login after 401', () => {
    localStorage.setItem('oilgas.token', 'expired-token');
    localStorage.setItem('oilgas.username', 'admin');
    localStorage.setItem('oilgas.role', 'admin');
    const auth = TestBed.inject(AuthService);

    httpClient.get('/api/protected').subscribe({ error: () => undefined });
    const request = http.expectOne('/api/protected');
    expect(request.request.headers.get('Authorization')).toBe('Bearer expired-token');
    request.flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(localStorage.getItem('oilgas.token')).toBeNull();
    expect(auth.username()).toBe('');
    expect(auth.role()).toBe('');
    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
  });
});
