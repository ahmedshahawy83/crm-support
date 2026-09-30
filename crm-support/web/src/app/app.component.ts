import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth';
import { I18n } from './core/i18n';
@Component({ selector: 'app-root', standalone: true, imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `@if (auth.token) { <nav>
    <a routerLink="/dashboard" routerLinkActive="active">{{i.t('dashboard')}}</a><a routerLink="/tickets" routerLinkActive="active">{{i.t('tickets')}}</a>
    <a routerLink="/customers" routerLinkActive="active">{{i.t('customers')}}</a><a routerLink="/kb" routerLinkActive="active">{{i.t('kb')}}</a><span class="sp"></span>
    <button (click)="i.toggle()">{{ i.lang() === 'en' ? 'العربية' : 'English' }}</button><button (click)="auth.logout()">{{i.t('logout')}}</button></nav> }
  <main><router-outlet /></main>` })
export class AppComponent { auth = inject(Auth); i = inject(I18n); }
