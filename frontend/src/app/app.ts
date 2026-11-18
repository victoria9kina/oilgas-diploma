import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { forkJoin } from 'rxjs';
import { BasinInfo, BasinProduction, BasinSummary, ProductionMonth } from './models/oil-gas';
import { OilGasApiService } from './services/oil-gas-api.service';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, FormsModule, RouterLink, RouterLinkActive, RouterOutlet],
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App implements OnInit {
  readonly isDashboard = signal(true);
  readonly fromMonth = signal('2009-01');
  readonly toMonth = signal('2025-11');
  readonly loading = signal(false);
  readonly apiStatus = signal<'loading' | 'online' | 'offline'>('loading');
  readonly errorMessage = signal('');
  readonly productions = signal<ProductionMonth[]>([]);
  readonly basinInfo = signal<BasinInfo[]>([]);
  readonly basinProduction = signal<BasinProduction[]>([]);

  readonly latest = computed(() => this.productions().at(-1) ?? null);
  readonly firstMonth = computed(() => this.productions()[0] ?? null);
  readonly oilChangePercent = computed(() => {
    const first = this.firstMonth()?.oilTotal;
    const latest = this.latest()?.oilTotal;
    if (first == null || latest == null || first === 0) return null;
    return ((latest - first) / first) * 100;
  });
  readonly basins = computed<BasinSummary[]>(() => {
    const totals = new Map<number, number>();
    for (const row of this.basinProduction()) {
      totals.set(row.basinId, (totals.get(row.basinId) ?? 0) + row.value);
    }
    const grandTotal = [...totals.values()].reduce((sum, value) => sum + value, 0);
    return this.basinInfo()
      .map((basin) => {
        const production = totals.get(basin.id) ?? 0;
        return {
          ...basin,
          production,
          sharePercent: grandTotal === 0 ? 0 : (production / grandTotal) * 100,
        };
      })
      .sort((a, b) => b.production - a.production);
  });

  constructor(private readonly api: OilGasApiService, private readonly router: Router, readonly auth: AuthService) {}

  ngOnInit(): void {
    this.loadDashboard();
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) this.isDashboard.set(event.urlAfterRedirects === '/');
    });
  }

  loadDashboard(): void {
    const from = this.asDate(this.fromMonth());
    const to = this.asDate(this.toMonth());
    if (!from || !to || from > to) {
      this.errorMessage.set('Проверьте период: дата начала должна быть раньше даты окончания.');
      return;
    }

    this.loading.set(true);
    this.apiStatus.set('loading');
    this.errorMessage.set('');

    forkJoin({
      productions: this.api.getProduction(from, to),
      basinInfo: this.api.getBasinsInfo(),
      basinProduction: this.api.getBasinProduction(from, to),
    }).subscribe({
      next: (data) => {
        this.productions.set(data.productions);
        this.basinInfo.set(data.basinInfo);
        this.basinProduction.set(data.basinProduction);
        this.loading.set(false);
        this.apiStatus.set('online');
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.apiStatus.set('offline');
        this.errorMessage.set(
          error.status === 0
            ? 'Не удалось связаться с API. Запустите сервер на localhost:5080.'
            : `Сервер вернул ошибку ${error.status}. Проверьте, что API запущен и данные загружены.`,
        );
      },
    });
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/login');
  }

  formatNumber(value: number | null | undefined): string {
    if (value == null || !Number.isFinite(value)) return '—';
    return new Intl.NumberFormat('ru-RU', { maximumFractionDigits: 0 }).format(value);
  }

  formatMonth(date: string | null | undefined): string {
    if (!date) return 'Нет данных';
    return new Intl.DateTimeFormat('ru-RU', { month: 'long', year: 'numeric', timeZone: 'UTC' })
      .format(new Date(`${date.slice(0, 10)}T00:00:00Z`));
  }

  formatPercent(value: number | null): string {
    if (value == null) return '—';
    return `${new Intl.NumberFormat('ru-RU', { maximumFractionDigits: 1 }).format(value)}%`;
  }

  chartPath(field: 'oilTotal' | 'gasTotal'): string {
    const values = this.productions().map((row) => row[field]);
    if (values.length < 2) return '';
    const max = this.chartMaximum(field);
    const width = 900;
    const height = 250;
    return values
      .map((value, index) => {
        const x = (index / (values.length - 1)) * width;
        const y = height - (value / max) * (height - 16) - 8;
        return `${index === 0 ? 'M' : 'L'} ${x.toFixed(1)} ${y.toFixed(1)}`;
      })
      .join(' ');
  }

  chartMaximum(field: 'oilTotal' | 'gasTotal'): number {
    return Math.max(...this.productions().map((row) => row[field]), 1);
  }

  chartAreaPath(field: 'oilTotal' | 'gasTotal'): string {
    const line = this.chartPath(field);
    if (!line) return '';
    return `${line} L 900 250 L 0 250 Z`;
  }

  mapX(basin: BasinSummary): number {
    // Схема страны условная, поэтому долготы растягиваем под её контур.
    return 112 + ((basin.longitude + 71) / 9) * 50;
  }

  mapY(basin: BasinSummary): number {
    return 18 + ((-21 - basin.latitude) / 34) * 350;
  }

  bubbleRadius(basin: BasinSummary): number {
    const maximum = Math.max(...this.basins().map((item) => item.production), 1);
    return 7 + Math.sqrt(basin.production / maximum) * 12;
  }

  private asDate(month: string): string | null {
    return /^\d{4}-(0[1-9]|1[0-2])$/.test(month) ? `${month}-01` : null;
  }
}
