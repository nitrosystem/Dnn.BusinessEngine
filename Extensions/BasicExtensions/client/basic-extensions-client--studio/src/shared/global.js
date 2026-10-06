export class Global {
    static clone(obj) {
        if (typeof structuredClone === 'function') {
            return structuredClone(obj);
        }
        return JSON.parse(JSON.stringify(obj));
    }

    static getSuggestedName(items, prefix) {
        if (!prefix) return '';

        const usedNumbers = new Set(
            items
                .map(i => {
                    const match = i.match(new RegExp(`^${prefix}(\\d+)$`));
                    return match ? Number(match[1]) : null;
                })
                .filter(n => n !== null)
        );

        let suggestedNumber = 1;
        while (usedNumbers.has(suggestedNumber)) {
            suggestedNumber++;
        }

        return `${prefix}${suggestedNumber}`;
    }

    static getDefaultValueByType(type) {
        if (!type || typeof type !== 'string') return '';

        const lowerType = type.toLowerCase();
        switch (lowerType) {
            case 'int':
            case 'float':
            case 'double':
                return 0;
            case 'bool':
                return false;
            case 'datetime':
                return new Date(0);
            default:
                return '';
        }
    }
}