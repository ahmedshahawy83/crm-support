import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { I18n } from '../core/i18n';
@Component({ standalone: true, imports: [FormsModule], template: `
  <div class="card"><div class="row"><input [(ngModel)]="n.name" [placeholder]="i.t('name')"><input [(ngModel)]="n.email" [placeholder]="i.t('email')"><input [(ngModel)]="n.phone" [placeholder]="i.t('phone')"><button (click)="add()">{{i.t('add')}}</button></div></div>
  <div class="card"><div class="row"><input [(ngModel)]="q" [placeholder]="i.t('search')" (keyup.enter)="load()"><button (click)="load()">{{i.t('search')}}</button></div>
  <table><tr><th>{{i.t('name')}}</th><th>{{i.t('email')}}</th><th>{{i.t('phone')}}</th></tr>@for (c of list; track c.id) { <tr><td>{{c.name}}</td><td>{{c.email}}</td><td>{{c.phone}}</td></tr> }</table></div>` })
export class CustomersPage {
  i = inject(I18n); private http = inject(HttpClient); list: any[] = []; q = ''; n: any = { name: '', email: '', phone: '' };
  constructor() { this.load(); }
  load() { this.http.get<any[]>('/api/customers', { params: this.q ? { q: this.q } : {} }).subscribe(r => this.list = r); }
  add() { if (!this.n.name) return; this.http.post('/api/customers', this.n).subscribe(() => { this.n = { name: '', email: '', phone: '' }; this.load(); }); }
}
