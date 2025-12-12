export function OrderByFilter(list, field, desc) {
    if (!Array.isArray(list)) return list;

    const sorted = [...list].sort((a, b) => {
        const av = a?.[field];
        const bv = b?.[field];

        if (av < bv) return -1;
        if (av > bv) return 1;
        return 0;
    });

    return desc ? sorted.reverse() : sorted;
}
