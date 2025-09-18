// src/app/core/global.ts
export class Globals {
  // گرفتن پارامتر از URL
  static getQueryParam(name: string): string | null {
    const url = new URL(window.location.href);
    return url.searchParams.get(name);
  }

  // فرمت تاریخ به YYYY-MM-DD
  static formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  // تولید UUID استاندارد
  static generateUUID(): string {
    return crypto.randomUUID();
  }

  // بررسی تهی بودن مقدار
  static isNullOrEmpty(value: any): boolean {
    return value === null || value === undefined || value === '';
  }

  // کلون عمیق با استفاده از structuredClone
  static deepClone<T>(obj: T): T {
    return structuredClone(obj);
  }

  // گروه‌بندی آرایه بر اساس کلید مشخص
  static groupBy<T>(array: T[], key: keyof T): Record<string, T[]> {
    return array.reduce((result, item) => {
      const groupKey = String(item[key]);
      result[groupKey] = result[groupKey] || [];
      result[groupKey].push(item);
      return result;
    }, {} as Record<string, T[]>);
  }

  // تبدیل رشته به عدد با کنترل خطا
  static toNumber(value: any, fallback = 0): number {
    const num = Number(value);
    return isNaN(num) ? fallback : num;
  }

  // تبدیل رشته به boolean
  static toBoolean(value: any): boolean {
    return String(value).toLowerCase() === 'true';
  }

  // حذف فضای خالی از ابتدا و انتهای رشته
  static trim(value: string): string {
    return value?.trim() ?? '';
  }
}
