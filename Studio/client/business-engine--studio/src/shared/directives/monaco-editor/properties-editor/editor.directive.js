import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';

export const BPropertiesContext = new Map();

export function MonacoPropertiesEditor($timeout) {
    return {
        restrict: 'A',
        require: 'ngModel',
        scope: {
            suggestions: '='
        },
        link: function (scope, element, attrs, ngModel) {
            const editor = monaco.editor.create(element[0], {
                language: 'bProperties',
                theme: 'bPropertiesTheme',
                automaticLayout: true,
                lineNumbers: 'off',
                minimap: { enabled: false },
                scrollbar: {
                    vertical: 'hidden',
                    horizontal: 'hidden',
                    handleMouseWheel: false,

                },
                overviewRulerLanes: 0,
                folding: false,
                padding: {
                    top: 4,
                    bottom: 4
                },
                glyphMargin: false,
                lineDecorationsWidth: 8,
                lineNumbersMinChars: 0,
                renderLineHighlight: 'none',
                wordWrap: 'off',
                tabCompletion: 'off',
                fontSize: '14px',
                lineHeight: 2,
                acceptSuggestionOnEnter: 'on',
                suggestOnTriggerCharacters: true,
                acceptSuggestionOnCommitCharacter: true,
                inlineSuggest: {
                    enabled: true
                },
                suggest: {
                    preview: true
                },
            });

            //Single line
            editor.onDidChangeModelContent(() => {
                const value = editor.getValue();
                const singleLine = value.replace(/\n/g, '');
                if (value !== singleLine) {
                    editor.setValue(singleLine);
                }

                ngModel.$setViewValue(singleLine);
            });

            ngModel.$render = () => {
                editor.setValue(ngModel.$viewValue || '');
            };

            const updateContext = () => {
                const model = editor.getModel();
                if (!model) return;

                BPropertiesContext.set(model.uri.toString(), {
                    suggestions: scope.suggestions || {},
                });
            }

            // initial
            updateContext();

            // watch (no deep)
            scope.$watch('suggestions', updateContext);

            scope.$on('$destroy', function () {
                const model = editor.getModel();
                BPropertiesContext.delete(model.uri.toString());
                editor.dispose();
            });

            element.css('height', '2.5rem');
        }
    };
}

