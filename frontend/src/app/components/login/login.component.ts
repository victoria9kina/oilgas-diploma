import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({ selector: 'app-login', imports: [ReactiveFormsModule], templateUrl: './login.component.html', styleUrl: './login.component.scss' })
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  readonly error = signal('');
  readonly form = this.formBuilder.nonNullable.group({ username: ['', [Validators.required, Validators.minLength(2)]], password: ['', [Validators.required, Validators.minLength(3)]] });
  constructor(private readonly auth: AuthService, private readonly router: Router) {}
  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.error.set('');
    const { username, password } = this.form.getRawValue();
    this.auth.login(username, password).subscribe({
      next: () => this.router.navigateByUrl('/data'),
      error: (error: HttpErrorResponse) => this.error.set(
        error.status === 401
          ? 'Неверный логин или пароль.'
          : error.status === 0
            ? 'Не удалось связаться с сервером.'
            : `Ошибка входа (${error.status}). Попробуй ещё раз.`,
      ),
    });
  }
}
