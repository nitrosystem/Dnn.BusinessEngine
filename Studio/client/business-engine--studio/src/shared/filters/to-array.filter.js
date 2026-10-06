export function ToArray() {

    const cache = new WeakMap();
    return function (collection, addKey) {
        if (collection == null) return collection;
        if (Array.isArray(collection)) return collection;
        if (typeof collection !== 'object') return collection;

        addKey = addKey === true;
        const cached = cache.get(collection);
        if (cached && cached.addKey === addKey) return cached.result;

        let result;
        if (addKey) {
            result = Object.keys(collection).map(key => {
                const value = collection[key];

                if (value && typeof value === 'object' && !Array.isArray(value)) {
                    return Object.assign({ $key: key }, value);
                }
                return { $key: key, $value: value };
            });
        } else {
            result = Object.values(collection);
        }
        
        cache.set(collection, { addKey, result });
        return result;
    };
}