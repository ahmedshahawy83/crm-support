import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { I18n } from '../core/i18n';
@Component({ standalone: true, imports: [FormsModule], template: `
  <div class="card"><div class="row">
    <input [(ngModel)]="n.subject" [placeholder]="i.t('subject')"><input [(ngModel)]="n.description" [placeholder]="i.t('description')">
    <select [(ngModel)]="n.customerId"><option [ngValue]="0">{{i.t('customer')}}</option>@for (c of customers; track c.id) { <option [ngValue]="c.id">{{c.name}}</option> }</select>
    <select [(ngModel)]="n.priority">@for (p of priorities; track p) { <option>{{p}}</option> }</select><button (click)="create()">{{i.t('create')}}</button></div></div>
  <div class="card"><table><tr><th>#</th><th>{{i.t('subject')}}</th><th>{{i.t('customer')}}</th><th>{{i.t('priority')}}</th><th>{{i.t('status')}}</th></tr>
  @for (t of tickets; track t.id) { <tr><td>{{t.number}}</td><td>{{t.subject}}</td><td>{{t.customer}}</td><td>{{t.priority}}</td>
    <td><select [ngModel]="t.status" (ngModelChange)="setStatus(t, $event)">@for (s of statuses; track s) { <option>{{s}}</option> }</select>@if (t.breached) { <span class="bad"> ⚠</span> }</td></tr> }</table></div>` })
export class TicketsPage {
  i = inject(I18n); private http = inject(HttpClient); tickets: any[] = []; customers: any[] = [];
  priorities = ['Low', 'Medium', 'High', 'Urgent']; statuses = ['New', 'Open', 'Pending', 'Escalated', 'Resolved', 'Closed'];
  n: any = { subject: '', description: '', customerId: 0, priority: 'Medium', channel: 'Portal' };
  constructor() { this.load(); this.http.get<any[]>('/api/customers').subscribe(r => this.customers = r); }
  load() { this.http.get<any[]>('/api/tickets').subscribe(r => this.tickets = r); }
  create() { if (!this.n.subject || !this.n.customerId) return; this.http.post('/api/tickets', this.n).subscribe(() => { this.n.subject = this.n.description = ''; this.load(); }); }
  setStatus(t: any, s: string) { this.http.put(`/api/tickets/${t.id}/status`, { status: s }).subscribe(() => t.status = s); }
}
