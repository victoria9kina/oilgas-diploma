import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BasinInfo, BasinProduction, LoginResponse, ProductionMonth } from '../models/oil-gas';

@Injectable({ providedIn: 'root' })
export class OilGasApiService {
  private readonly apiUrl = 'http://localhost:5080/api';

  constructor(private readonly http: HttpClient) {}

  getProduction(from?: string, to?: string): Observable<ProductionMonth[]> {
    return this.http.get<ProductionMonth[]>(`${this.apiUrl}/production`, {
      params: from && to ? this.periodParams(from, to) : undefined,
    });
  }

  getBasinsInfo(): Observable<BasinInfo[]> {
    return this.http.get<BasinInfo[]>(`${this.apiUrl}/basins/info`);
  }

  getBasinProduction(from: string, to: string): Observable<BasinProduction[]> {
    return this.http.get<BasinProduction[]>(`${this.apiUrl}/basins`, {
      params: this.periodParams(from, to),
    });
  }

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, { username, password });
  }

  createProduction(production: Omit<ProductionMonth, 'id' | 'oilTotal' | 'gasTotal' | 'oilUnconventional' | 'oilUnconventionalSharePercent'>): Observable<ProductionMonth> {
    return this.http.post<ProductionMonth>(`${this.apiUrl}/production`, production);
  }

  updateProduction(production: ProductionMonth): Observable<ProductionMonth> {
    return this.http.put<ProductionMonth>(`${this.apiUrl}/production/${production.id}`, production);
  }

  private periodParams(from: string, to: string): HttpParams {
    return new HttpParams().set('from', from).set('to', to);
  }
}
