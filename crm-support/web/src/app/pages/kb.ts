import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { I18n } from '../core/i18n';
@Component({ standalone: true, imports: [FormsModule], template: `
  <div class="row"><input [(ngModel)]="q" [placeholder]="i.t('search')" (keyup.enter)="load()"><button (click)="load()">{{i.t('search')}}</button></div>
  @for (a of list; track a.id) { <div class="card"><h3>{{ i.lang() === 'ar' ? (a.titleAr || a.titleEn) : a.titleEn }}</h3><p>{{ i.lang() === 'ar' ? (a.bodyAr || a.bodyEn) : a.bodyEn }}</p></div> }` })
export class KbPage {
  i = inject(I18n); private http = inject(HttpClient); list: any[] = []; q = '';
  constructor() { this.load(); }
  load() { this.http.get<any[]>('/api/kb', { params: this.q ? { q: this.q } : {} }).subscribe(r => this.list = r); }
}
