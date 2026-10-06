export function BindImage(app, expressionService, globalService) {
    return {
        compile: function (attrs, element, scope) {
            if (element.__b_image_processed) return;
            element.__b_image_processed = true;

            const expr = attrs['b-image'];
            const render = () => {
                let value = expressionService.evaluateExpression(expr, scope);
                if (value) {
                    const options = globalService.parseInlineOptions(attrs.options);
                    const isThumbnail = expressionService.evaluateExpression(options.thumbnail, scope);
                    const isProfile = expressionService.evaluateExpression(options.isProfile, scope);
                    const mode = isProfile ? 'profilepic' : 'file';
                    const type = isProfile ? 'userid' : 'file';
                    if (isThumbnail || isProfile) {
                        const width = `w=${expressionService.evaluateExpression(options.width, scope) ?? ''}`;
                        const height = `h=${expressionService.evaluateExpression(options.height, scope) ?? ''}`;
                        value = `/DnnImageHandler.ashx?mode=${mode}&${type}=${value}&${width}&${height}`;
                    }
                }
                else
                    value = attrs['data-no-image'];

                if (value) element.src = value;
            }

            render();

            if (attrs.listen !== 'false') app.listenTo(expr, scope, render);
        }
    }
}