export function BindImage(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_image_processed) return;
            element.__b_image_processed = true;

            const expr = attrs['b-image'];
            const render = () => {
                let value = expressionService.evaluateExpression(expr, scope);
                if (value) {
                    const options = parseOptionsObject(attrs.options);
                    const isThumbnail = expressionService.evaluateExpression(options.thumbnail, scope);
                    const width = `w=${expressionService.evaluateExpression(options.width, scope) ?? ''}`;
                    const height = `h=${expressionService.evaluateExpression(options.height, scope) ?? ''}`;
                    const isJson = expressionService.evaluateExpression(options.json, scope);
                    const index = isJson & !isNaN(attrs.json)
                        ? parseInt(attrs.json)
                        : 0;

                    if (isJson) value = value[index];
                    if (isThumbnail) value = `/DnnImageHandler.ashx?mode=file&file=${value}&${width}&${height}`;
                }
                else
                    value = attrs['data-no-image'];

                if (value) $(element).attr('src', value);
            }

            function parseOptionsObject(str) {
                const result = Object.create(null); // بدون prototype

                if (!str) return result;

                str = str.trim();
                if (str[0] === '{') str = str.slice(1, -1);

                if (!str) return result;

                const parts = str.split(',');

                for (const part of parts) {
                    const idx = part.indexOf(':');
                    if (idx === -1) continue;

                    const name = part.slice(0, idx).trim();
                    const value = part.slice(idx + 1).trim();

                    result[name] = value;
                }

                return result;
            }

            render();

            if (attrs.listen !== 'false') app.listenTo(expr, scope, render);
        }
    }
}