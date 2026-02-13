import moment from "moment";
import $ from "jquery";
import * as bootstrap from 'bootstrap';
import "twbs-pagination/jquery.twbsPagination"

import studioTemplate from "../../studio.html";

export function StudioDirective() {
    return {
        restrict: "E",
        templateUrl: studioTemplate,
        link: function (scope, element, attrs) { },
        replace: true,
    };
}

export function ChosenDropdownDirective($timeout, $parse) {
    // same regex Angular uses to parse ngOptions (we only need the collection part)
    var NG_OPTIONS_REGEXP = /^\s*([\s\S]+?)(?:\s+as\s+([\s\S]+?))?(?:\s+group\s+by\s+([\s\S]+?))?\s+for\s+(?:([\$\w][\$\w]*)|(?:\(\s*([\$\w][\$\w]*)\s*,\s*([\$\w][\$\w]*)\s*\)))\s+in\s+([\s\S]+?)(?:\s+track\s+by\s+([\s\S]+?))?$/;

    var CHOSEN_ATTR_WHITELIST = [
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
            var ngSelect = ctrls[0];
            var ngModel = ctrls[1];
            var chosen;
            var options = angular.extend({}, {
                width: '100%',
                disableSearchThreshold: 0
            });
            var updateTimer = null;
            var initTimer = null;
            var collectionGetter = null; // function to get options collection
            var collectionWatchDeregister = null;

            // apply whitelist attributes (observe for dynamic values)
            angular.forEach(attrs.$attr, function (_, attrName) {
                if (CHOSEN_ATTR_WHITELIST.indexOf(attrName) !== -1) {
                    attrs.$observe(attrName, function (val) {
                        // convert attribute name to chosen's snake_case
                        var k = attrName.replace(/[A-Z]/g, function (m) { return '_' + m.toLowerCase(); });
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

                        // فوکوس با TAB → باز شدن
                        container.on('focus', () => {
                            // جلوگیری از باز شدن دوبار
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
                var match = attrs.ngOptions.match(NG_OPTIONS_REGEXP);
                if (match) {
                    // match[7] is the "in <collection>" part per regex
                    var collectionExpr = match[7];
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

export function EnterDirective() {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            element.bind("keydown, keypress", function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.ngEnter);
                    });
                    event.preventDefault();
                }
            });
        }
    };
}

export function CustomDateDirective() {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            var value = attrs.bCustomDate;
            if (attrs.relative == "true") {
                var content = moment(value).fromNow();
                element.text(content);
            } else {
                const format = attrs.format || "MM/DD/YYYY";
                var dt = moment(value);
                var content = dt.isValid() ? dt.format(format) : "";
                element.text(content);
            }
        },
        replace: true,
    };
}

export function CustomResizeableDirective() {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            $(element[0]).resizable({
                minWidth: parseFloat(getComputedStyle(document.documentElement).fontSize) * 15,
                maxWidth: parseFloat(getComputedStyle(document.documentElement).fontSize) * 45,
                handles: "e",
                start: (event, ui) => {
                    $(ui).addClass("start-dragged");
                },
                stop: (event, ui) => {
                    $(ui).removeClass("start-dragged");
                },
            });
        },
    };
}

export function CustomTooltipDirective($timeout) {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            $timeout(() => {
                new bootstrap.Tooltip(element[0]);
            });
        },
    };
}

export function CustomPopoverDirective($timeout) {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            $timeout(() => {
                new bootstrap.Popover(element[0], {
                    html: true,
                    sanitize: false,
                    customClass: "b-popover-dark",
                });
            }, 500);
        },
    };
}

export function CustomModalDirective() {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            if (attrs.id) window[attrs.id] = new bootstrap.Modal(element[0]);
        },
    };
}

export function CustomFocusDirective($timeout) {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            scope.$on(attrs.bCustomFocus, function (e) {
                $timeout(function () {
                    element[0].focus();
                });
            });
        },
    };
}

export function CustomSidebarDirective($timeout, $rootScope) {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            element.addClass("sidebar");

            $rootScope.$watch("currentActivityBar", (newVal, oldVal) => {
                if (newVal != oldVal) {
                    if (newVal == attrs.bCustomSidebar) $(element).show();
                    else $(element).hide();
                }
            });
        },
    };
}

export function EsckeyDirective() {
    return {
        restrict: "A",
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

export function NotFieldTypeDirective() {
    return {
        template:
            `
            <div class="b-notify notify-warning mb-2">
                <i class="codicon codicon-search-stop b-icon-2"></i>
                <p class="text mb-0"> 
                    Field type 
                    <b>"{{field.FieldType}}"</b>
                    not found in installed extensions and components!.
                </p>
            </div>
            `,
        restrict: 'E',
        replace: true,
        scope: {
            field: "="
        },
        link: function (scope, element, attrs) {
        }
    };
}

export function CustomPagingDirective($rootScope) {
    return {
        restrict: "A",
        scope: {
            options: '='
        },
        link: function (scope, element, attrs) {
            scope.options.initiateStartPageClick = false;
            scope.options.hideOnlyOnePage = true;
            scope.options.startPage = scope.options.startPage || 1;
            $(element).twbsPagination(scope.options);

            $rootScope.$on('twbsPaginationChangePage', function (e, args) {
                if (args.id == attrs.id) $(element).twbsPagination('show', args.pageIndex);
            });
        },
    };
}

export function CustomStarRatingDirective() {
    return {
        restrict: 'E',
        template: `<ul class="b-custom-star-rating {{size}}">
                        <li ng-repeat="star in stars track by $index">
                            <i class="codicon" ng-class="{'codicon-star-empty':$index+1>=value,'codicon-star-full is-fill':$index<value}"></i>
                        </li> 
                   </ul>`,
        require: '?ngModel',
        scope: {
            max: '=',
            size: '@'
        },
        link: function (scope, elem, attrs, ngModel) {
            scope.stars = Array.apply(null, Array(scope.max));

            //ngModel.$setViewValue = 3;

            ngModel.$render = () => {
                scope.value = ngModel.$viewValue || 0;
            };
        },
        replace: true
    };
}

export function AdjectiveClassDirective(globalService) {
    return {
        restrict: "A",
        link: function (scope, element, attrs) {
            scope.$watch(attrs.bAdjectiveClass, function (newVal, oldVal) {
                if (newVal !== oldVal) {
                    const delay = attrs.adjectiveDelay ?? 10000
                    globalService.addAdjectiveCssClass($(element), attrs.adjectiveClass, delay);
                }
            })
        },
    };
}