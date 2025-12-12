export function BindImage(app, expressionService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_image_processed) return;
            element.__b_image_processed = true;

            const expr = attrs['b-image'];
            const render = () => {
                let value = expressionService.evaluateExpression(expr, scope);
                if (value !== null && value !== undefined) {
                    const isThumbnail = attrs.thumbnail === 'true';
                    const width = `w=${attrs.width}`;
                    const height = `h=${attrs.height}`;
                    const isJson = attrs.json === 'true';
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

            render();

            if (attrs.listen !== 'false') app.listenTo(expr, scope, render);
        }
    }
}