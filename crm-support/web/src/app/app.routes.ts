import { Routes } from '@angular/router';
import { authGuard } from './core/auth';
export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./pages/login').then(m => m.LoginPage) },
  { path: '', canActivate: [authGuard], children: [
    { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
    { path: 'dashboard', loadComponent: () => import('./pages/dashboard').then(m => m.DashboardPage) },
    { path: 'tickets', loadComponent: () => import('./pages/tickets').then(m => m.TicketsPage) },
    { path: 'customers', loadComponent: () => import('./pages/customers').then(m => m.CustomersPage) },
    { path: 'kb', loadComponent: () => import('./pages/kb').then(m => m.KbPage) } ] },
  { path: '**', redirectTo: '' } ];
