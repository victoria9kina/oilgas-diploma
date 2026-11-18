import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProductionMonth } from '../../models/oil-gas';
import { AuthService } from '../../services/auth.service';
import { OilGasApiService } from '../../services/oil-gas-api.service';

@Component({
  selector: 'app-data-management',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './data-management.component.html',
  styleUrl: './data-management.component.scss',
})
export class DataManagementComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  readonly rows = signal<ProductionMonth[]>([]);
  readonly query = signal('');
  readonly view = signal<'table' | 'cards'>('table');
  readonly sortAscending = signal(false);
  readonly editing = signal<ProductionMonth | null>(null);
  readonly message = signal('');
  readonly messageType = signal<'success' | 'error'>('error');
  readonly loading = signal(false);
  readonly loadError = signal('');
  readonly saving = signal(false);
  readonly submitted = signal(false);
  readonly visibleRows = computed(() => this.rows()
    .filter(row => row.date.includes(this.query().trim()))
    .sort((a, b) => this.sortAscending() ? a.date.localeCompare(b.date) : b.date.localeCompare(a.date)));

  readonly form = this.formBuilder.nonNullable.group({
    date: ['', Validators.required],
    oilConventional: [0, [Validators.required, Validators.min(0)]],
    oilShale: [0, [Validators.required, Validators.min(0)]],
    oilTight: [0, [Validators.required, Validators.min(0)]],
    gasConventional: [0, [Validators.required, Validators.min(0)]],
    gasShale: [0, [Validators.required, Validators.min(0)]],
    gasTight: [0, [Validators.required, Validators.min(0)]],
  });

  constructor(private readonly api: OilGasApiService, readonly auth: AuthService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.loadError.set('');
    this.api.getProduction().subscribe({
      next: rows => {
        this.rows.set(rows);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.loadError.set('Не удалось получить данные от API. Проверь, запущен ли backend.');
      },
    });
  }

  startEdit(row: ProductionMonth): void {
    if (this.saving()) return;
    this.editing.set(row);
    this.message.set('');
    this.submitted.set(false);
    this.form.setValue({ date: row.date.slice(0, 7), oilConventional: row.oilConventional, oilShale: row.oilShale, oilTight: row.oilTight, gasConventional: row.gasConventional, gasShale: row.gasShale, gasTight: row.gasTight });
  }

  cancelEdit(): void {
    if (this.saving()) return;
    this.editing.set(null);
    this.submitted.set(false);
    this.form.reset({ date: '', oilConventional: 0, oilShale: 0, oilTight: 0, gasConventional: 0, gasShale: 0, gasTight: 0 });
  }

  save(): void {
    if (this.saving()) return;
    this.submitted.set(true);
    this.message.set('');
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }

    const editing = this.editing();
    const values = this.form.getRawValue();
    const production = { ...values, date: `${values.date}-01` };
    const request = editing
      ? this.api.updateProduction({ ...editing, ...production })
      : this.api.createProduction(production);

    this.saving.set(true);
    request.subscribe({
      next: saved => {
        this.rows.update(rows => editing
          ? rows.map(row => row.id === saved.id ? saved : row)
          : [...rows, saved]);
        this.saving.set(false);
        this.messageType.set('success');
        this.message.set(editing ? 'Запись успешно обновлена.' : 'Запись успешно добавлена.');
        this.cancelEdit();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.messageType.set('error');
        this.message.set(this.saveErrorMessage(error));
      },
    });
  }

  showError(controlName: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || this.submitted());
  }

  fieldError(controlName: keyof typeof this.form.controls): string {
    const control = this.form.controls[controlName];
    if (control.hasError('required')) return 'Заполни это поле.';
    if (control.hasError('min')) return 'Значение не может быть отрицательным.';
    return 'Укажи корректное числовое значение.';
  }

  number(value: number): string { return new Intl.NumberFormat('ru-RU', { maximumFractionDigits: 0 }).format(value); }

  private saveErrorMessage(error: HttpErrorResponse): string {
    switch (error.status) {
      case 0: return 'Нет соединения с сервером. Проверь, запущен ли backend.';
      case 400: return 'Сервер отклонил данные. Проверь месяц и числовые значения.';
      case 401: return 'Сеанс входа закончился. Войди в систему ещё раз.';
      case 403: return 'Недостаточно прав. Изменять данные может только администратор.';
      case 404: return 'Запись не найдена. Возможно, она уже была удалена.';
      case 409: return error.error?.message || 'Запись за выбранный месяц уже существует.';
      case 500: return 'Ошибка сервера. Попробуй сохранить запись позже.';
      default: return `Не удалось сохранить запись (ошибка ${error.status}).`;
    }
  }
}
