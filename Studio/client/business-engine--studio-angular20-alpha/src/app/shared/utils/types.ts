// ✅ Type Aliases برای وضوح بیشتر
export type Guid = string | null;
export type DateTimeString = string;
export type CurrencyCode = string;
export type EntityKey = string | number;
export type Nullable<T> = T | null | undefined;

// ✅ کلاس کمکی برای Try-style متدها
export class Types {
    static tryGuid(value: any): Guid | null {
        const regex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
        return typeof value === 'string' && regex.test(value) ? value : null;
    }

    static tryDate(value: any): Date | null {
        const date = new Date(value);
        return isNaN(date.getTime()) ? null : date;
    }

    static tryNumber(value: any): number | null {
        const num = Number(value);
        return isNaN(num) ? null : num;
    }

    static tryBoolean(value: any): boolean | null {
        if (typeof value === 'boolean') return value;
        if (typeof value === 'string') {
            const lower = value.toLowerCase();
            if (lower === 'true') return true;
            if (lower === 'false') return false;
        }
        return null;
    }

    static tryCurrencyCode(value: any): CurrencyCode | null {
        return typeof value === 'string' && /^[A-Z]{3}$/.test(value) ? value : null;
    }

    static tryEntityKey(value: any): EntityKey | null {
        return typeof value === 'string' || typeof value === 'number' ? value : null;
    }
}
