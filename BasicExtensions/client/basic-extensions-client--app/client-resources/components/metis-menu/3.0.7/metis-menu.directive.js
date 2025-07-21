window.bEngineApp.value('metisMenuConfig', {}).directive('metisMenu', ['metisMenuConfig', '$timeout', function(metisMenuConfig, $timeout) {
    var options = {};
    if (metisMenuConfig) {
        angular.extend(options, metisMenuConfig);
    }
    return {
        priority: 1,
        compile: function(tElm, tAttrs) {
            return function(scope, elm, attrs, controller) {
                $timeout(() => {
                    elm.metisMenu();
                });
            };
        }
    };
}]);