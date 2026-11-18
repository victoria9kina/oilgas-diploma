export interface ProductionMonth {
  id: number;
  date: string;
  oilConventional: number;
  oilShale: number;
  oilTight: number;
  oilTotal: number;
  gasConventional: number;
  gasShale: number;
  gasTight: number;
  gasTotal: number;
  oilUnconventional: number;
  oilUnconventionalSharePercent: number;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: 'admin' | 'user';
}

export interface BasinInfo {
  id: number;
  code: string;
  name: string;
  latitude: number;
  longitude: number;
}

export interface BasinProduction {
  id: number;
  date: string;
  basinId: number;
  value: number;
  basin?: BasinInfo;
}

export interface BasinSummary extends BasinInfo {
  production: number;
  sharePercent: number;
}
