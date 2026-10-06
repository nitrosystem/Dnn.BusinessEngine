export function EsckeyDirective() {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            element.bind('keydown keyup keypress', function (event) {
                if (event.which === 27) { // 27 = esc key
                    scope.$apply(function () {
                        scope.$eval(attrs.bEscKey);
                    });

                    event.preventDefault();
                }
            });
            
            scope.$on('$destroy', function () {
                element.unbind('keydown keypress')
            });
        },
    };
}