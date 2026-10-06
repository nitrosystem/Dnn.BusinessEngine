import { SQL_TYPE_REGEX, SQL_TYPES } from '../constants';

export class GlobalHelper {
    static clone(obj) {
        if (typeof structuredClone === 'function') {
            return structuredClone(obj);
        }
        return JSON.parse(JSON.stringify(obj));
    }

    static normalizeName(input) {
        // 1. Keep only English letters, numbers, underscores and spaces.
        let cleaned = input.replace(/[^A-Za-z0-9_ ]+/g, ' ');

        // 2. Remove leading digits/underscores/spaces
        //    (C# identifiers can't start with a digit, and "_" is not allowed at the start).
        cleaned = cleaned.replace(/^[0-9_\s]+/, '');

        // 3. Split on whitespace.
        const parts = cleaned.trim().split(/\s+/).filter(Boolean);

        // 4. If nothing remains after cleaning, return empty.
        if (parts.length === 0) return '';

        // 5. Join words: the first word keeps its original casing,
        //    the rest are capitalized (PascalCase-style joining).
        return parts
            .map((w, i) => i === 0 ? w : w.charAt(0).toUpperCase() + w.slice(1))
            .join('');
    }

    static getByPath(obj, path, defaultValue = undefined) {
        if (obj == null || !path) return defaultValue;

        const keys = Array.isArray(path)
            ? path
            : String(path).match(/[^.[\]]+/g) || [];
        let result = obj;

        for (const key of keys) {
            if (result == null) return defaultValue;
            result = result[key];
        }

        return result === undefined ? defaultValue : result;
    };

    static setByPath(obj, path, value) {
        if (obj == null || !path) return obj;

        const keys = Array.isArray(path)
            ? path
            : String(path).match(/[^.[\]]+/g) || [];
        let current = obj;

        keys.forEach((key, index) => {
            if (index === keys.length - 1) {
                current[key] = value;
            } else {
                if (current[key] == null || typeof current[key] !== 'object') {
                    current[key] = /^\d+$/.test(keys[index + 1]) ? [] : {};
                }
                current = current[key];
            }
        });

        return obj;
    };

    static checkSqlTypes(type) {
        if (!type) return false;

        const normalized = type.toString().trim().toLowerCase();
        const match = SQL_TYPE_REGEX.exec(normalized);
        if (!match) return false;

        const [, sqlType, length, scale] = match;

        // Short typing
        if (SQL_TYPES.noLength.has(sqlType)) {
            return !length && !scale;
        }

        // Typing with a length (optional) - such as varchar(50) or varchar(max)
        if (SQL_TYPES.singleLength.has(sqlType)) {
            return !!length && !scale;
        }

        // Type precision/scale - such as decimal(18, 2)
        if (SQL_TYPES.precisionScale.has(sqlType)) {
            return !!length && !!scale && length !== 'max';
        }

        return false;
    }

    static generateGuid() {
        if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
            return crypto.randomUUID();
        }

        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = (Math.random() * 16) | 0;
            const v = c === 'x' ? r : (r & 0x3) | 0x8;
            return v.toString(16);
        });
    }

    /**
     * Convert entity name to SQL table name (simple pluralization)
     * Rules:
     *   1. ends with s/x/z/ch/sh  → add "es"   (Box → Boxes)
     *   2. ends with consonant+y  → "ies"      (Company → Companies)
     *   3. otherwise              → add "s"    (User → Users)
     */
    static pluralize = (word) => {
        if (!word) return word;
        const lower = word.toLowerCase();

        // Rule 1: sibilant endings → "es"
        if (/(s|x|z|ch|sh)$/.test(lower)) {
            return word + 'es';
        }

        // Rule 2: consonant + y → replace y with "ies"
        if (/[^aeiou]y$/.test(lower)) {
            return word.slice(0, -1) + 'ies';
        }

        // Rule 3: default → just add "s"
        return word + 's';
    };

    /**
     * Debounce a func
     * @param {Function} fn - A func that must be debounce.
     * @param {number} delay - milliseconds 
     * @returns {Function} Func debounced with cancel method
     * 
     * @example
     * const debouncedSearch = debounce(() => doSearch(), 300);
     * debouncedSearch(); // exec after 300ms
     * debouncedSearch.cancel(); // cancel waiting task
     */
    static debounce(fn, delay = 300) {
        let timerId = null;
        const debounced = (...args) => {
            if (timerId) {
                clearTimeout(timerId);
            }
            timerId = setTimeout(() => {
                timerId = null;
                fn.apply(this, args);
            }, delay);
        }

        debounced.cancel = function () {
            if (timerId) {
                clearTimeout(timerId);
                timerId = null;
            }
        };

        return debounced;
    }
}
