export class UrlHelper {
    static getUrlParams(url) {
        const params = {};
        const parser = document.createElement('a');
        parser.href = url;
        const query = parser.search.substring(1);
        const vars = query.split('&');

        for (const item of vars) {
            if (!item) continue;
            const [key, value] = item.split('=');
            params[key.toLowerCase()] = decodeURIComponent(value ?? '');
        }

        return params;
    }

    static getParameterByName(name, url) {
        if (!name) return null;

        try {
            const urlObj = new URL(url ?? window.location.href, window.location.origin);
            const lowerName = name.toLowerCase();
            for (const [key, value] of urlObj.searchParams) {
                if (key.toLowerCase() === lowerName) return value;
            }

            return null;
        } catch {
            return null;
        }
    }

    static getUrlQueryFromObject(paramsObject) {
        return Object.entries(paramsObject)
            .map(([key, value]) => `${key}=${value}`)
            .join('&');
    }

    static replaceUrlParam(paramName, paramValue, url) {
        let _url = url ?? document.URL;
        const value = paramValue ?? '';
        const pattern = new RegExp('\\b(' + paramName + '=).*?(&|#|$)');

        if (_url.search(pattern) >= 0) {
            return _url.replace(pattern, `$1${value}$2`);
        }

        _url = _url.replace(/[?#]$/, '');
        return `${_url}${_url.indexOf('?') > 0 ? '&' : '?'}${paramName}=${value}`;
    }

    static pushState(url, title, data) {
        const _title = title ?? document.title;
        const _data = data ?? '';
        window.history.pushState({ pageTitle: _title }, _data, url);
    }
}