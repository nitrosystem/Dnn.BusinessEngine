export class UrlService {
    static parsePageParameters(url) {
        const result = {};

        if (!url || typeof url !== 'string' || !url.trim()) return result;

        let parsedUrl;
        try {
            parsedUrl = new URL(url);
        } catch {
            return result;
        }

        // 1. پارس Query String → ?t=10&u=test
        parsedUrl.searchParams.forEach((value, key) => {
            result[key] = value;
        });

        // 2. پارس Segments → /home/t/10/u/test
        const segments = parsedUrl.pathname
            .split('/')
            .filter(Boolean) // حذف ""
            .slice(1); // skip first segment (مثلاً "home")

        for (let i = 0; i < segments.length - 1; i++) {
            const key = segments[i];
            const value = segments[i + 1];

            if (
                !(key in result) &&
                !this.isNumeric(key) &&
                !this.isKeyLikeValue(value)
            ) {
                result[key] = value;
                i++; // چون مقدار رو هم برداشتیم
            }
        }

        return result;
    }

    static isNumeric(str) {
        return !isNaN(parseFloat(str)) && isFinite(str);
    }

    static isKeyLikeValue(value) {
        return !value || value.includes('=');
    }
}
