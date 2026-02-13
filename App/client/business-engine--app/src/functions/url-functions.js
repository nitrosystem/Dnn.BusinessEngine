export function Slugify(title) {
    return title
        .toString()
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")   // حذف accentها
        .toLowerCase()
        .trim()
        .replace(/[^a-z0-9\u0600-\u06FF\s-]/g, "") // مجاز کردن حروف فارسی
        .replace(/\s+/g, "-")
        .replace(/-+/g, "-");
}

export function ParsePageParameters(url) {
    const result = {};

    if (!url || typeof url !== 'string' || !url.trim()) return result;

    let parsedUrl;
    try {
        parsedUrl = new URL(url);
    } catch {
        return result;
    }

    const isNumeric = function (str) {
        return !isNaN(parseFloat(str)) && isFinite(str);
    }

    const isKeyLikeValue = function (value) {
        return !value || value.includes('=');
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
            !isNumeric(key) &&
            !isKeyLikeValue(value)
        ) {
            result[key] = value;
            i++; // چون مقدار رو هم برداشتیم
        }
    }

    return result;
}
