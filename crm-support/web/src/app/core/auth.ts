import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpInterceptorFn } from '@angular/common/http';
import { CanActivateFn, Router } from '@angular/router';
import { tap } from 'rxjs';
@Injectable({ providedIn: 'root' })
export class Auth {
  private http = inject(HttpClient); private router = inject(Router);
  get token() { return localStorage.getItem('t'); }
  get role() { return localStorage.getItem('r') ?? ''; }
  login(email: string, password: string) {
    return this.http.post<any>('/api/auth/login', { email, password }).pipe(tap(r => { localStorage.setItem('t', r.token); localStorage.setItem('r', r.role); }));
  }
  logout() { localStorage.removeItem('t'); localStorage.removeItem('r'); this.router.navigate(['/login']); }
}
export const authInterceptor: HttpInterceptorFn = (req, next) => { const t = inject(Auth).token; return next(t ? req.clone({ setHeaders: { Authorization: `Bearer ${t}` } }) : req); };
export const authGuard: CanActivateFn = () => inject(Auth).token ? true : inject(Router).parseUrl('/login');
