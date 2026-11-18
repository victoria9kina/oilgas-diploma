import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { DataManagementComponent } from './data-management.component';

describe('DataManagementComponent', () => {
  let http: HttpTestingController;
  let component: DataManagementComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataManagementComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    component = TestBed.createComponent(DataManagementComponent).componentInstance;
  });

  afterEach(() => http.verify());

  it('loads all production records without a fixed end date', () => {
    component.load();

    expect(component.loading()).toBe(true);
    const request = http.expectOne('http://localhost:5080/api/production');
    expect(request.request.params.keys()).toEqual([]);
    request.flush([{
      id: 205,
      date: '2026-01-01',
      oilConventional: 10,
      oilShale: 2,
      oilTight: 1,
      oilTotal: 13,
      gasConventional: 20,
      gasShale: 4,
      gasTight: 3,
      gasTotal: 27,
      oilUnconventional: 3,
      oilUnconventionalSharePercent: 23.1,
    }]);

    expect(component.loading()).toBe(false);
    expect(component.rows()[0].date).toBe('2026-01-01');
  });

  it('shows an API error when loading fails', () => {
    component.load();

    http.expectOne('http://localhost:5080/api/production').error(new ProgressEvent('error'));

    expect(component.loading()).toBe(false);
    expect(component.loadError()).toContain('Не удалось получить данные');
  });

  it('returns an empty list when the search has no matches', () => {
    component.rows.set([{
      id: 1,
      date: '2025-11-01',
      oilConventional: 10,
      oilShale: 2,
      oilTight: 1,
      oilTotal: 13,
      gasConventional: 20,
      gasShale: 4,
      gasTight: 3,
      gasTotal: 27,
      oilUnconventional: 3,
      oilUnconventionalSharePercent: 23.1,
    }]);
    component.query.set('2030-01');

    expect(component.visibleRows()).toEqual([]);
  });

  it('shows validation errors and does not send an invalid form', () => {
    component.form.patchValue({ date: '', oilConventional: -1 });

    component.save();

    expect(component.showError('date')).toBe(true);
    expect(component.showError('oilConventional')).toBe(true);
    expect(component.fieldError('oilConventional')).toBe('Значение не может быть отрицательным.');
  });

  it('sends the first day of the month and blocks a repeated save', () => {
    component.form.setValue({
      date: '2025-11',
      oilConventional: 10,
      oilShale: 2,
      oilTight: 1,
      gasConventional: 20,
      gasShale: 4,
      gasTight: 3,
    });

    component.save();
    component.save();

    const request = http.expectOne('http://localhost:5080/api/production');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.date).toBe('2025-11-01');
    request.flush({
      id: 204,
      date: '2025-11-01',
      oilConventional: 10,
      oilShale: 2,
      oilTight: 1,
      oilTotal: 13,
      gasConventional: 20,
      gasShale: 4,
      gasTight: 3,
      gasTotal: 27,
      oilUnconventional: 3,
      oilUnconventionalSharePercent: 23.1,
    });

    expect(component.rows().length).toBe(1);
    expect(component.message()).toBe('Запись успешно добавлена.');
    expect(component.saving()).toBe(false);
  });

  it('keeps entered values and shows the server message after a conflict', () => {
    component.form.setValue({
      date: '2025-11',
      oilConventional: 10,
      oilShale: 2,
      oilTight: 1,
      gasConventional: 20,
      gasShale: 4,
      gasTight: 3,
    });

    component.save();
    http.expectOne('http://localhost:5080/api/production').flush(
      { message: 'Запись за 11/01/2025 уже существует' },
      { status: 409, statusText: 'Conflict' },
    );

    expect(component.form.controls.date.value).toBe('2025-11');
    expect(component.form.controls.oilConventional.value).toBe(10);
    expect(component.message()).toContain('уже существует');
    expect(component.saving()).toBe(false);
  });

  it('sends an edited row with PUT and updates it in the list', () => {
    const row = {
      id: 10,
      date: '2025-10-01',
      oilConventional: 10,
      oilShale: 2,
      oilTight: 1,
      oilTotal: 13,
      gasConventional: 20,
      gasShale: 4,
      gasTight: 3,
      gasTotal: 27,
      oilUnconventional: 3,
      oilUnconventionalSharePercent: 23.1,
    };
    component.rows.set([row]);
    component.startEdit(row);
    component.form.controls.oilConventional.setValue(15);

    component.save();

    const request = http.expectOne('http://localhost:5080/api/production/10');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body.date).toBe('2025-10-01');
    request.flush({ ...row, oilConventional: 15, oilTotal: 18 });

    expect(component.rows()[0].oilConventional).toBe(15);
    expect(component.message()).toBe('Запись успешно обновлена.');
  });
});
