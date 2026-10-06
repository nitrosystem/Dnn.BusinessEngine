export class DateHelper {
    // ============================================
    // relative() - Relative time formatting
    // ============================================
    static relative(value, locale = 'en') {
        const date = value instanceof Date ? value : new Date(value);
        const diffInSeconds = Math.round((date.getTime() - Date.now()) / 1000);

        // Using built-in Intl.RelativeTimeFormat (ES2020)
        const rtf = new Intl.RelativeTimeFormat(locale, { numeric: 'auto' });
        const units = [
            { unit: 'year', seconds: 31536000 },
            { unit: 'month', seconds: 2592000 },
            { unit: 'week', seconds: 604800 },
            { unit: 'day', seconds: 86400 },
            { unit: 'hour', seconds: 3600 },
            { unit: 'minute', seconds: 60 },
            { unit: 'second', seconds: 1 },
        ];

        const absDiff = Math.abs(diffInSeconds);
        const { unit, seconds } = units.find(u => absDiff >= u.seconds) ?? units.at(-1);

        return rtf.format(Math.round(diffInSeconds / seconds), unit);
    };

    // ============================================
    // format() - Date formatting (moment-like tokens)
    // ============================================
    static format(value, pattern = 'YYYY-MM-DD HH:mm:ss') {
        const date = value instanceof Date ? value : new Date(value);
        if (isNaN(date.getTime())) return 'Invalid Date';

        const pad = (n, len = 2) => String(n).padStart(len, '0');

        // Token map (order matters: longer tokens first)
        const tokens = {
            YYYY: date.getFullYear(),
            YY: String(date.getFullYear()).slice(-2),
            MM: pad(date.getMonth() + 1),
            M: date.getMonth() + 1,
            DD: pad(date.getDate()),
            D: date.getDate(),
            HH: pad(date.getHours()),
            H: date.getHours(),
            hh: pad(((date.getHours() + 11) % 12) + 1),
            h: ((date.getHours() + 11) % 12) + 1,
            mm: pad(date.getMinutes()),
            m: date.getMinutes(),
            ss: pad(date.getSeconds()),
            s: date.getSeconds(),
            SSS: pad(date.getMilliseconds(), 3),
            A: date.getHours() < 12 ? 'AM' : 'PM',
            a: date.getHours() < 12 ? 'am' : 'pm',
        };
        
        // Single regex pass — avoids token collision
        return pattern.replace(
            /YYYY|YY|MM|M|DD|D|HH|H|hh|h|mm|m|ss|s|SSS|A|a/g,
            match => tokens[match]
        );
    };
}