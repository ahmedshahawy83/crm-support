import { Injectable, signal } from '@angular/core';
const D: Record<string, Record<string, string>> = {
  en: { dashboard: 'Dashboard', tickets: 'Tickets', customers: 'Customers', kb: 'Knowledge Base', logout: 'Logout', login: 'Sign in', email: 'Email', password: 'Password', subject: 'Subject', description: 'Description',
        customer: 'Customer', priority: 'Priority', status: 'Status', create: 'Create', search: 'Search', name: 'Name', phone: 'Phone', add: 'Add', total: 'Total', open: 'Open', breached: 'SLA breached', rating: 'Avg rating' },
  ar: { dashboard: 'لوحة التحكم', tickets: 'التذاكر', customers: 'العملاء', kb: 'قاعدة المعرفة', logout: 'خروج', login: 'تسجيل الدخول', email: 'البريد الإلكتروني', password: 'كلمة المرور', subject: 'الموضوع', description: 'الوصف',
        customer: 'العميل', priority: 'الأولوية', status: 'الحالة', create: 'إنشاء', search: 'بحث', name: 'الاسم', phone: 'الهاتف', add: 'إضافة', total: 'الإجمالي', open: 'مفتوحة', breached: 'تجاوز اتفاقية الخدمة', rating: 'متوسط التقييم' } };
@Injectable({ providedIn: 'root' })
export class I18n {
  lang = signal(localStorage.getItem('l') || 'en');
  constructor() { this.apply(); }
  t = (k: string) => D[this.lang()][k] ?? k;
  toggle() { this.lang.set(this.lang() === 'en' ? 'ar' : 'en'); localStorage.setItem('l', this.lang()); this.apply(); }
  private apply() { document.documentElement.lang = this.lang(); document.documentElement.dir = this.lang() === 'ar' ? 'rtl' : 'ltr'; }
}
