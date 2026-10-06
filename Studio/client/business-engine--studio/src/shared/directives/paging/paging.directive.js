import { Paging } from '../../libs';

export function PagingDirective($rootScope) {
    return {
        restrict: 'A',
        scope: {
            options: '='
        },
        link: function (scope, element, attrs) {
            scope.options.initiateStartPageClick = false;
            scope.options.hideOnlyOnePage = true;
            scope.options.startPage = scope.options.startPage || 1;

            //const element = document.getElementById('paging');
            let paging = new Paging(element[0], scope.options);

            $rootScope.$on('onReinitPagination', function (e, args) {
                paging.destroy();
                paging = new Paging(element, scope.options);
            });
        },
    };
}