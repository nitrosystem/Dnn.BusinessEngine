export function FocusDirective($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            scope.$on(attrs.bFocus, function (e) {
                $timeout(function () {
                    element[0].focus();
                });
            });
        },
    };
}