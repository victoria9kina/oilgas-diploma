import { Routes } from '@angular/router';
import { DataManagementComponent } from './components/data-management/data-management.component';
import { LoginComponent } from './components/login/login.component';

export const routes: Routes = [
  { path: 'data', component: DataManagementComponent },
  { path: 'login', component: LoginComponent },
  { path: '**', redirectTo: '' },
];
