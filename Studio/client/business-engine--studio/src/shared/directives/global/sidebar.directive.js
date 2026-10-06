export function SidebarDirective($timeout, $rootScope) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            element.addClass('sidebar');

            $rootScope.$watch('currentActivityBar', (newVal, oldVal) => {
                if (newVal !== oldVal) {
                    if (newVal === attrs.bSidebar)
                        $(element).show();
                    else
                        $(element).hide();
                }
            });
        },
    };
}