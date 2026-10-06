import studioTemplate from '../../../pages/studio.html';

export function StudioDirective() {
    return {
        restrict: 'E',
        templateUrl: studioTemplate,
        link: function (scope, element, attrs) { },
        replace: true,
    };
}