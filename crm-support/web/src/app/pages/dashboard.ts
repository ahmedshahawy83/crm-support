import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { I18n } from '../core/i18n';
@Component({ standalone: true, template: `@if (s) { <div class="kpis">
  <div class="card kpi">{{i.t('total')}}<b>{{s.total}}</b></div><div class="card kpi">{{i.t('open')}}<b>{{s.open}}</b></div>
  <div class="card kpi">{{i.t('breached')}}<b class="bad">{{s.breached}}</b></div><div class="card kpi">{{i.t('rating')}}<b>{{s.avgRating.toFixed(1)}}</b></div></div> }` })
export class DashboardPage { i = inject(I18n); s: any; constructor() { inject(HttpClient).get('/api/reports/summary').subscribe(r => this.s = r); } }
