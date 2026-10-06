import { Popover } from 'bootstrap';

export function BootstrapPopoverDirective($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            $timeout(() => {
                new Popover(element[0], {
                    html: true,
                    sanitize: false,
                    customClass: 'b-popover-dark',
                });
            }, 500);
        },
    };
}