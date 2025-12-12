export class GlobalService {
    constructor() {
    }

    getParameterByName(name, url) {
        const urlParams = new URLSearchParams(window.location.search);
        const _result = urlParams.get(name);
        if (_result) return _result;

        url = url ? url.toLowerCase() : document.URL.toLowerCase();

        const _url = decodeURIComponent(url);
        const _name = name.toLowerCase().replace(/[\[\]]/g, "\\$&");

        const regex = new RegExp("[?&]" + _name + "(=([^&#]*)|&|#|$)");
        const results = regex.exec(_url);

        if (!results) {
            //parse friendly params
            const friendlyParams = location.pathname
                .toLowerCase()
                .replace(/=/g, "/")
                .split("/");
            if (
                friendlyParams.indexOf(_name) >= 0 &&
                friendlyParams.length >= friendlyParams.indexOf(_name) + 2
            )
                return friendlyParams[friendlyParams.indexOf(_name) + 1];
            else return null;
        } else if (!results[2]) {
            return "";
        } else {
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        }
    }

    isSystemType(type) {
        return ['int', 'float', 'double', 'bool', 'datetime'].includes(type.toLowerCase());
    }

    convertToRealType(value, type) {
        const lowerType = type.toLowerCase();

        if (value === null || value === undefined || value === '')
            return this.getDefaultValueByType(value, type);

        switch (lowerType) {
            case 'int':
            case 'float':
            case 'double':
                return Number(value);
            case 'bool':
                return value === 'true' || value === true;
            case 'datetime':
                return new Date(value);
            default:
                return value;
        }
    }

    getDefaultValueByType(value, type) {
        const lowerType = type.toLowerCase();

        if (value === null || value === undefined || value === '') {
            switch (lowerType) {
                case 'int':
                case 'float':
                case 'double':
                    return 0;
                case 'bool':
                    return false;
                case 'datetime':
                    return new Date(0); // معادل DateTime.MinValue در C#
                default:
                    return '';
            }
        }
    }

    parseJsonItems(items) {
        if (items == null) return items; // null or undefined

        // If it's an array, parse each element
        if (Array.isArray(items)) {
            return items.map(item => this.parseJsonItems(item));
        }

        // If it's an object, parse each property
        if (typeof items === 'object') {
            for (const key in items) {
                if (!Object.hasOwn(items, key)) continue;
                items[key] = this.parseJsonItems(items[key]);
            }
            return items;
        }

        // If it's a string, check if it looks like JSON
        if (typeof items === 'string') {
            const trimmed = items.trim();

            // Quick check to skip obvious non-JSON strings
            if (
                !(
                    (trimmed.startsWith('{') && trimmed.endsWith('}')) || // object
                    (trimmed.startsWith('[') && trimmed.endsWith(']')) || // array
                    /^"(?:\\.|[^"\\])*"$/.test(trimmed) || // quoted string
                    /^-?\d+(\.\d+)?([eE][+\-]?\d+)?$/.test(trimmed) || // number
                    /^(true|false|null)$/.test(trimmed) // boolean/null
                )
            ) {
                return items; // definitely not JSON
            }

            // Try parsing only if it passes the quick check
            try {
                const parsed = JSON.parse(items);
                return this.parseJsonItems(parsed); // Recursively parse
            } catch {
                return items; // invalid JSON
            }
        }

        // Primitive value (number, boolean, etc.)
        return items;
    }

    decodeProtectedData(base64String) {
        if (!base64String) return null;

        // Base64 decode
        const binaryString = atob(base64String);
        const binaryData = new Uint8Array(binaryString.length);

        for (let i = 0; i < binaryString.length; i++) {
            binaryData[i] = binaryString.charCodeAt(i);
        }

        // GZip decompress
        const decompressed = pako.inflate(binaryData, { to: 'string' });

        // Parse JSON
        return JSON.parse(decompressed);
    }

    cloneDeep(obj) {
        if (typeof structuredClone === "function") {
            return structuredClone(obj);
        } else {
            return JSON.parse(JSON.stringify(obj));
        }
    }

    keyBy(array, key) {
        return (array || []).reduce((r, x) => ({ ...r, [key ? x[key] : x]: x }), {})
    }
}