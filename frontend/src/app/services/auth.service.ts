import { Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { OilGasApiService } from './oil-gas-api.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly hasToken = Boolean(localStorage.getItem('oilgas.token'));
  readonly username = signal(this.hasToken ? localStorage.getItem('oilgas.username') ?? '' : '');
  readonly role = signal(this.hasToken ? localStorage.getItem('oilgas.role') ?? '' : '');

  constructor(private readonly api: OilGasApiService) {}

  login(username: string, password: string) {
    return this.api.login(username, password).pipe(tap((response) => {
      localStorage.setItem('oilgas.token', response.token);
      localStorage.setItem('oilgas.username', response.username);
      localStorage.setItem('oilgas.role', response.role);
      this.username.set(response.username);
      this.role.set(response.role);
    }));
  }

  logout(): void {
    localStorage.removeItem('oilgas.token');
    localStorage.removeItem('oilgas.username');
    localStorage.removeItem('oilgas.role');
    this.username.set('');
    this.role.set('');
  }
}
