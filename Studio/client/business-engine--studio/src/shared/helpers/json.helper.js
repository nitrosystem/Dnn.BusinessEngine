export class JsonHelper {
    static parseJsonItems(items) {
        if (items == null) return items;

        if (Array.isArray(items)) {
            return items.map(item => this.parseJsonItems(item));
        }

        if (typeof items === 'object') {
            for (const key in items) {
                if (!Object.hasOwn(items, key)) continue;
                items[key] = this.parseJsonItems(items[key]);
            }

            return items;
        }

        if (typeof items === 'string') {
            const trimmed = items.trim();

            // Quick check to skip obvious non-JSON strings
            const looksLikeJson =
                (trimmed.startsWith('{') && trimmed.endsWith('}')) ||
                (trimmed.startsWith('[') && trimmed.endsWith(']')) ||
                /^"(?:\\.|[^"\\])*"$/.test(trimmed) ||
                /^-?\d+(\.\d+)?([eE][+\-]?\d+)?$/.test(trimmed) ||
                /^(true|false|null)$/.test(trimmed);

            if (!looksLikeJson) return items;

            try {
                const parsed = JSON.parse(items);
                return this.parseJsonItems(parsed);
            } catch {
                return items;
            }
        }

        return items;
    }

    static isJsonString(str) {
        if (!str) return false;

        try {
            JSON.parse(str);
            return true;
        } catch {
            return false;
        }
    }

    static getJsonString(str) {
        try {
            return JSON.parse(str);
        } catch {
            return str;
        }
    }
}