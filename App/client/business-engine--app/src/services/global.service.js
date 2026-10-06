export class GlobalService {
    /**
     * Get URL parameter by name (supports query strings and friendly URLs)
     */
    getParameterByName(name, url) {
        // Try standard query string first
        const urlParams = new URLSearchParams(window.location.search);
        const result = urlParams.get(name);
        if (result) return result;

        const targetUrl = decodeURIComponent((url || document.URL).toLowerCase());
        const escapedName = name.toLowerCase().replace(/[\[\]]/g, '\\$&');
        const regex = new RegExp(`[?&]${escapedName}(=([^&#]*)|&|#|$)`);
        const match = regex.exec(targetUrl);
        if (match) {
            return match[2] ? decodeURIComponent(match[2].replace(/\+/g, '')) : '';
        }

        // Fallback: parse friendly URL params (e.g. /name/value/)
        const segments = location.pathname.toLowerCase().replace(/=/g, '/').split('/');
        const idx = segments.indexOf(escapedName);
        return idx >= 0 && segments.length > idx + 1 ? segments[idx + 1] : null;
    }

    /**
     * Safely parse JSON string, returns undefined on failure
     */
    parseJson(str) {
        if (typeof str !== 'string' || !str.trim()) return undefined;
        try {
            return JSON.parse(str);
        } catch {
            return undefined;
        }
    }

    /**
     * Recursively parse JSON-like strings inside objects/arrays
     */
    parseJsonItems(items) {
        if (items == null) return items;

        if (Array.isArray(items)) {
            return items.map(item => this.parseJsonItems(item));
        }

        if (typeof items === 'object') {
            for (const key of Object.keys(items)) {
                items[key] = this.parseJsonItems(items[key]);
            }
            return items;
        }

        if (typeof items !== 'string') return items;

        // Quick heuristic check before attempting JSON.parse (performance)
        const trimmed = items.trim();
        const looksLikeJson =
            (trimmed.startsWith('{') && trimmed.endsWith('}')) ||
            (trimmed.startsWith('[') && trimmed.endsWith(']')) ||
            /^"(?:\\.|[^"\\])*"$/.test(trimmed) ||
            /^-?\d+(\.\d+)?([eE][+\-]?\d+)?$/.test(trimmed) ||
            /^(true|false|null)$/.test(trimmed);
        if (!looksLikeJson) return items;

        try {
            return this.parseJsonItems(JSON.parse(items));
        } catch {
            return items;
        }
    }

    /**
     * Parse Angular-style inline options object from an attribute string.
     * Supports: numbers, booleans, null, single/double quoted strings,
     * nested objects/arrays, and values containing commas or colons.
     *
     * @example
     *   parseInlineOptions("{a:1, b:2, c:true, d:'hello, world', e:{x:1}}")
     *   // → { a: 1, b: 2, c: true, d: 'hello, world', e: { x: 1 } }
     */
    parseInlineOptions(str) {
        /**
         * Split a string by a delimiter, ignoring delimiters inside
         * quotes or nested brackets/braces/parens.
         */
        const splitTopLevel = (str, delimiter) => {
            const parts = [];
            let depth = 0;
            let quote = null;
            let start = 0;
            for (let i = 0; i < str.length; i++) {
                const ch = str[i];
                if (quote) {
                    if (ch === quote && str[i - 1] !== '\\') quote = null;
                    continue;
                }

                if (ch === '"' || ch === "'") { quote = ch; continue; }
                if (ch === '{' || ch === '[' || ch === '(') { depth++; continue; }
                if (ch === '}' || ch === ']' || ch === ')') { depth--; continue; }
                if (ch === delimiter && depth === 0) {
                    parts.push(str.slice(start, i));
                    start = i + 1;
                }
            }

            const last = str.slice(start).trim();
            if (last) parts.push(last);
            return parts;
        }

        /**
         * Find the first top-level occurrence of a character.
         */
        const findTopLevel = (str, target) => {
            let depth = 0;
            let quote = null;
            for (let i = 0; i < str.length; i++) {
                const ch = str[i];
                if (quote) {
                    if (ch === quote && str[i - 1] !== '\\') quote = null;
                    continue;
                }

                if (ch === '"' || ch === "'") { quote = ch; continue; }
                if (ch === '{' || ch === '[' || ch === '(') { depth++; continue; }
                if (ch === '}' || ch === ']' || ch === ')') { depth--; continue; }
                if (ch === target && depth === 0) return i;
            }

            return -1;
        }

        /**
         * Convert a raw string value into its proper JS type.
         */
        const parseValue = (raw) => {
            if (!raw) return '';

            // Boolean / null / undefined
            if (raw === 'true') return true;
            if (raw === 'false') return false;
            if (raw === 'null') return null;
            if (raw === 'undefined') return undefined;

            // Number
            if (/^-?\d+(\.\d+)?([eE][+\-]?\d+)?$/.test(raw)) return Number(raw);

            // Quoted string
            const first = raw[0], last = raw[raw.length - 1];
            if ((first === '"' || first === "'") && first === last) {
                return raw.slice(1, -1).replace(/\\(['"\\])/g, '$1');
            }

            // Nested object/array → try JSON.parse with normalization
            if ((first === '{' && last === '}') || (first === '[' && last === ']')) {
                try {
                    // Convert single quotes & unquoted keys to valid JSON
                    const jsonReady = raw
                        .replace(/'/g, '"')
                        .replace(/([{,]\s*)([a-zA-Z_$][\w$]*)\s*:/g, '$1"$2":');
                    return JSON.parse(jsonReady);
                } catch {
                    return raw;
                }
            }

            // Fallback: return as-is string (unquoted identifier)
            return raw;
        }

        const result = Object.create(null);
        if (!str || typeof str !== 'string') return result;

        str = str.trim();
        if (!str) return result;

        // Strip outer braces if present
        if (str[0] === '{' && str[str.length - 1] === '}') {
            str = str.slice(1, -1).trim();
        }

        if (!str) return result;

        // Split top-level entries (respecting quotes & nested brackets)
        const entries = splitTopLevel(str, ',');
        for (const entry of entries) {
            const colonIdx = findTopLevel(entry, ':');
            if (colonIdx === -1) continue;

            const key = entry.slice(0, colonIdx).trim().replace(/^['"]|['"]$/g, '');
            const rawValue = entry.slice(colonIdx + 1).trim();
            if (!key) continue;

            result[key] = parseValue(rawValue);
        }

        return result;
    }

    /**
     * Decode base64 + gzip compressed JSON payload
     */
    decodeProtectedData(base64String) {
        if (!base64String) return null;

        // Decode base64 to bytes using Uint8Array.from (cleaner than manual loop)
        const binaryData = Uint8Array.from(atob(base64String), c => c.charCodeAt(0));

        // Decompress and parse
        const decompressed = pako.inflate(binaryData, { to: 'string' });
        return JSON.parse(decompressed);
    }

    /**
     * Deep clone using structuredClone with JSON fallback
     */
    clone(obj) {
        return typeof structuredClone === 'function'
            ? structuredClone(obj)
            : JSON.parse(JSON.stringify(obj));
    }

    /**
     * Format byte size into human-readable string (KB, MB, GB, ...)
     */
    formatFileSize(bytes, decimals = 2) {
        if (!bytes) return '0 B';

        const k = 1024;
        const units = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];
        const i = Math.min(Math.floor(Math.log(bytes) / Math.log(k)), units.length - 1);
        return `${parseFloat((bytes / Math.pow(k, i)).toFixed(decimals))} ${units[i]}`;
    }

    /**
     * Extract file name from a path or URL
     */
    getFileNameFromUrl(path, { withExtension = true } = {}) {
        if (typeof path !== 'string' || !path) return '';

        // Strip query string and hash
        const cleanPath = path.split(/[?#]/)[0];
        let fileName = cleanPath.split(/[/\\]/).pop() || '';

        // Decode URL-encoded characters (e.g. Persian/spaces)
        try {
            fileName = decodeURIComponent(fileName);
        } catch {
        }

        if (!withExtension) {
            const dotIndex = fileName.lastIndexOf('.');
            return dotIndex > 0 ? fileName.slice(0, dotIndex) : fileName;
        }

        return fileName;
    }

    /**
     * Get lowercase file extension from path/URL
     * (Fixed bug: previously called undefined `getFileName`)
     */
    getFileExtension(path) {
        const name = this.getFileNameFromUrl(path);
        const dotIndex = name.lastIndexOf('.');
        return dotIndex > 0 ? name.slice(dotIndex + 1).toLowerCase() : '';
    }

    /**
     * Check if element is visible in the DOM
     */
    isElementVisible(el) {
        if (!(el instanceof Element)) return false;

        // Modern API
        if (typeof el.checkVisibility === 'function') {
            return el.checkVisibility({ checkOpacity: true, checkVisibilityCSS: true });
        }

        // Fallback: walk up the DOM tree
        for (let current = el; current; current = current.parentElement) {
            const { display, visibility, opacity } = getComputedStyle(current);
            if (display === 'none' || visibility === 'hidden' || opacity === '0') {
                return false;
            }
        }

        return true;
    }

    /**
     * Run callback in next microtask
     */
    nextMicroTask(fn) {
        if (typeof queueMicrotask === 'function') return queueMicrotask(fn);
        if (typeof Promise !== 'undefined') return Promise.resolve().then(fn);

        setTimeout(fn, 0);
    }

    /**
     * Run callback in next macrotask
     */
    nextMacroTask(fn) {
        if (typeof setImmediate === 'function') return setImmediate(fn);

        if (typeof MessageChannel !== 'undefined') {
            const channel = new MessageChannel();
            channel.port1.onmessage = fn;
            channel.port2.postMessage(null);
            return;
        }

        setTimeout(fn, 0);
    }

    /**
     * Run callback on next animation frame (optionally double-frame for layout-after-paint)
     */
    nextAnimationFrame(fn, doubleFrame = false) {
        if (typeof requestAnimationFrame !== 'function') {
            return setTimeout(() => fn(performance.now()), 1000 / 60);
        }

        if (doubleFrame) {
            requestAnimationFrame(() => requestAnimationFrame(fn));
        } else {
            requestAnimationFrame(fn);
        }
    }

    /**
     * Run callback when browser is idle (with polyfill fallback)
     */
    nextIdle(fn, options) {
        if (typeof requestIdleCallback === 'function') {
            return requestIdleCallback(fn, options);
        }

        const timeout = options?.timeout ?? 50;
        const start = Date.now();
        setTimeout(() => {
            fn({
                didTimeout: false,
                timeRemaining: () => Math.max(0, timeout - (Date.now() - start)),
            });
        }, timeout);
    }
}
