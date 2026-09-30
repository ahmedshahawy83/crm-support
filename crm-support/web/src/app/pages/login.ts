import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Auth } from '../core/auth';
import { I18n } from '../core/i18n';
@Component({ standalone: true, imports: [FormsModule], template: `<div class="card" style="max-width:360px;margin:60px auto">
  <h2>{{i.t('login')}}</h2><div class="row"><input [(ngModel)]="email" [placeholder]="i.t('email')" style="width:100%"></div>
  <div class="row"><input type="password" [(ngModel)]="pw" [placeholder]="i.t('password')" style="width:100%"></div>
  <button (click)="go()">{{i.t('login')}}</button> @if (err) { <p class="bad">Invalid credentials</p> }</div>` })
export class LoginPage {
  i = inject(I18n); private a = inject(Auth); private r = inject(Router); email = 'admin@crm.local'; pw = 'Admin@123'; err = false;
  go() { this.a.login(this.email, this.pw).subscribe({ next: () => this.r.navigate(['/dashboard']), error: () => this.err = true }); }
}
