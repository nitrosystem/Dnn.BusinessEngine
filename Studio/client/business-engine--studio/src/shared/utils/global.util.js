export class GlobalUtil {
    static renderErrorHtml(error) {
        const data = error?.data ?? {};
        const rows = [
            ['Status Code', error?.status],
            ['Status Text', error?.statusText],
            ['Error Message', data]
        ];
        const rowsHtml = rows
            .map(
                ([label, value]) => `
                <tr>
                    <td class='w-50'>${label}</td>
                    <td>${value ?? ''}</td>
                </tr>`
            )
            .join('');

        return `
            <table class='table table-bordered text-light error-table'>
                <tbody>
                    ${rowsHtml}
                </tbody>
            </table>`;
    }

    static bindParams(params, source) {
        // Remove params that are not in source
        for (let i = params.length - 1; i >= 0; i--) {
            const exists = source.some(sp => sp.ParamName === params[i].ParamName);
            if (!exists) params.splice(i, 1);
        }

        // Add missing params from source (sorted by ViewOrder)
        const sortedSource = [...source].sort(
            (a, b) => (a.ViewOrder ?? 0) - (b.ViewOrder ?? 0)
        );

        for (const sp of sortedSource) {
            const exists = params.some(p => p.ParamName === sp.ParamName);
            if (!exists) {
                params.push({
                    ParamName: sp.ParamName,
                    ParamValue: sp.ParamValue,
                    ViewOrder: sp.ViewOrder
                });
            }
        }
    }
}