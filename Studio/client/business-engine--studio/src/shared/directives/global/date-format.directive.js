import { DateHelper } from '../../helpers';

export function DateFormatDirective() {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            const value = attrs.bDateFormat;
            const isRelative = attrs.relative === 'true'
            const format = attrs.format;
            const content = isRelative
                ? DateHelper.relative(value)
                : DateHelper.format(value, format);
            element.text(content);
        },
        replace: true,
    };
}