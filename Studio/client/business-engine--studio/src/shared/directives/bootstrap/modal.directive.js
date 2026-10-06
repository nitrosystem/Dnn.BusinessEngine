import { Modal } from 'bootstrap';

export function BootstrapModalDirective() {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            if (attrs.id) {
                window[attrs.id] = new Modal(element[0]);
            }
        },
    };
}