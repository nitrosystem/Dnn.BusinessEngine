import "chosen-js/chosen.jquery";

export function ChosenDropdownDirective($timeout, $parse) {
    // same regex Angular uses to parse ngOptions (we only need the collection part)
    const NG_OPTIONS_REGEXP = /^\s*([\s\S]+?)(?:\s+as\s+([\s\S]+?))?(?:\s+group\s+by\s+([\s\S]+?))?\s+for\s+(?:([\$\w][\$\w]*)|(?:\(\s*([\$\w][\$\w]*)\s*,\s*([\$\w][\$\w]*)\s*\)))\s+in\s+([\s\S]+?)(?:\s+track\s+by\s+([\s\S]+?))?$/;

    const CHOSEN_ATTR_WHITELIST = [
        'allowSingleDeselect', 'disableSearch', 'disableSearchThreshold', 'enableSplitWordSearch',
        'inheritSelectClasses', 'maxSelectedOptions', 'noResultsText', 'placeholderTextMultiple',
        'placeholderTextSingle', 'searchContains', 'groupSearch', 'singleBackstrokeDelete',
        'width', 'displayDisabledOptions', 'displaySelectedOptions', 'includeGroupLabelInSelected',
        'maxShownResults', 'caseSensitiveSearch', 'hideResultsOnSelect', 'rtl'
    ];

    return {
        restrict: 'A',
        require: ['select', '?ngModel'],
        link: function (scope, element, attrs, ctrls) {
            const ngSelect = ctrls[0];
            const ngModel = ctrls[1];
            const options = { width: '100%', disableSearchThreshold: 0 };
            let chosen;
            let updateTimer = null;
            let initTimer = null;
            let collectionGetter = null; // function to get options collection
            let collectionWatchDeregister = null;

            // apply whitelist attributes (observe for dynamic values)
            angular.forEach(attrs.$attr, function (_, attrName) {
                if (CHOSEN_ATTR_WHITELIST.indexOf(attrName) !== -1) {
                    attrs.$observe(attrName, function (val) {
                        // convert attribute name to chosen's snake_case
                        const k = attrName.replace(/[A-Z]/g, function (m) { return '_' + m.toLowerCase(); });
                        // if wrapped in {{}} Angular will have resolved in attr, otherwise try evaluate
                        try {
                            options[k] = scope.$eval(val);
                        } catch (e) {
                            options[k] = val;
                        }
                        scheduleUpdate();
                    });
                }
            });

            function scheduleUpdate(delay) {
                delay = typeof delay === 'number' ? delay : 50;
                if (updateTimer) $timeout.cancel(updateTimer);
                updateTimer = $timeout(function () {
                    try {
                        element.trigger('chosen:updated');
                    } catch (e) { /* ignore */ }
                    updateTimer = null;
                }, delay);
            }

            function initChosen() {
                if (chosen) return;
                // ensure DOM & ngOptions rendered
                initTimer = $timeout(function () {
                    element.chosen(options);
                    chosen = element.data('chosen');

                    $timeout(() => {
                        const container = element.next('.chosen-container');

                        // Transfer tabindex from original select to chosen
                        const t = element.attr('tabindex');
                        if (t !== undefined) {
                            container.attr('tabindex', t);
                        } else {
                            container.attr('tabindex', 0);
                        }

                        // Focus with TAB → Opening
                        container.on('focus', () => {
                            // Preventing double opening
                            if (!container.hasClass('chosen-container-active')) {
                                container.trigger('mousedown');
                            }
                        });

                        // When dropdown opens, focus the search box
                        element.on('chosen:showing_dropdown', () => {
                            const input = container.find('input');
                            $timeout(() => input.focus(), 10);
                        });
                    });
                }, 0);
            }

            // Extract collection expression from ngOptions if present (so we can watch the actual array/object)
            if (attrs.ngOptions) {
                let match = attrs.ngOptions.match(NG_OPTIONS_REGEXP);
                if (match) {
                    // match[7] is the "in <collection>" part per regex
                    const collectionExpr = match[7];
                    try {
                        collectionGetter = $parse(collectionExpr);
                        // watchCollection the actual collection (handles array changes)
                        collectionWatchDeregister = scope.$watchCollection(function () {
                            return collectionGetter(scope);
                        }, function (newVal, oldVal) {
                            // if undefined -> loading state
                            if (angular.isUndefined(newVal)) {
                                element.prop('disabled', true).addClass('loading');
                            } else {
                                element.prop('disabled', !!attrs.disabled && scope.$eval(attrs.disabled));
                                element.removeClass('loading');
                            }
                            // chosen may not be initialized yet
                            initChosen();
                            scheduleUpdate(30);
                        });
                    } catch (e) {
                        // fallback: don't watchCollection; instead watch the attribute string (rare)
                        scope.$watch(attrs.ngOptions, function () {
                            initChosen();
                            scheduleUpdate();
                        });
                    }
                } else {
                    // no ngOptions match -> fallback to watching the attr (less ideal)
                    scope.$watch(attrs.ngOptions, function () {
                        initChosen();
                        scheduleUpdate();
                    });
                }
            } else {
                // no ngOptions: just init and watch model
                initChosen();
            }

            // watch for model changes (two-way binding)
            if (ngModel) {
                scope.$watch(function () { return ngModel.$modelValue; }, function () {
                    initChosen();
                    scheduleUpdate();
                }, true);
            }

            // watch disabled attribute specifically
            attrs.$observe('disabled', function () {
                initChosen();
                scheduleUpdate();
            });

            // If select element attributes change (e.g. multiple) re-init safely
            attrs.$observe('multiple', function () {
                // we need to re-init chosen (destroy & recreate) when multiple toggles
                if (chosen) {
                    try { element.chosen('destroy'); } catch (e) { }
                    chosen = null;
                }
                initChosen();
            });

            // cleanup on destroy
            scope.$on('$destroy', function () {
                if (updateTimer) $timeout.cancel(updateTimer);
                if (initTimer) $timeout.cancel(initTimer);
                if (collectionWatchDeregister) collectionWatchDeregister();
                try {
                    if (chosen) element.chosen('destroy');
                } catch (e) { /* ignore */ }
            });

            // final init call
            $timeout(initChosen, 0);
        }
    };
}