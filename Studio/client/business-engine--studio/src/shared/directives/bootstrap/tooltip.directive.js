import { Tooltip } from 'bootstrap';

export function BootstrapTooltipDirective($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            let tooltipInstance = null;
            $timeout(() => {
                tooltipInstance = new Tooltip(element[0], {
                    trigger: 'hover',   //Show only with hover, not click
                    container: 'body',
                });
            });

            // helper for checking validity
            function isTooltipValid(instance) {
                return instance && instance._element && instance._activeTrigger !== null;
            }

            if (!attrs.disableEvents) {
                element.on('click', function () {
                    if (isTooltipValid(tooltipInstance)) {
                        tooltipInstance.hide();
                    }
                });
            }

            scope.$on('$destroy', function () {
                // First null, then dispose — so the click handler can't afterward trigger the hide sound.
                const instance = tooltipInstance;
                tooltipInstance = null;

                if (instance) {
                    try {
                        instance.hide();
                        instance.dispose();
                    } catch (e) {
                        // ignore — already disposed
                    }
                }

                document.querySelectorAll('.tooltip').forEach(function (el) {
                    el.remove();
                });
            });
        },
    };
}
