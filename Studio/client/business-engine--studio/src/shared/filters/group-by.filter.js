export function GroupBy($parse) {
    const cache = new WeakMap();
    function groupByFilter(items, property) {
        if (!Array.isArray(items) || !property) return items;

        const cached = cache.get(items);
        if (cached && cached.property === property) return cached.result;

        const getter = $parse(property);
        const result = {};
        for (let i = 0; i < items.length; i++) {
            const item = items[i];
            if (item == null) continue;
            const key = getter(item);
            if (key == null) continue;
            const keyStr = String(key);
            if (!result[keyStr]) {
                result[keyStr] = [];
            }
            result[keyStr].push(item);
        }
        
        cache.set(items, { property, result });
        return result;
    }

    groupByFilter.$stateful = true;
    return groupByFilter;
}
