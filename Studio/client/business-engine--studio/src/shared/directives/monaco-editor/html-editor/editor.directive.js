import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';

export function MonacoHtmlEditor() {
    return {
        restrict: 'A',
        require: '?ngModel',
        scope: {
            language: '@',
            readOnly: '<?',
            theme: '@?',
            height: '@?'
        },
        link(scope, element, attrs, ngModel) {
            if (!ngModel || !scope.language) return;

            // Set element height
            const height = scope.height || element.data('height');
            if (height) {
                element.css('height', height);
            } else {
                element.css('min-height', '300px');
            }

            // Create editor with options
            const editor = monaco.editor.create(element[0], {
                value: '',
                language: scope.language,
                automaticLayout: true,
                readOnly: scope.readOnly ?? false,
                theme: scope.theme || 'vs-dark',
                minimap: { enabled: false },
                scrollBeyondLastLine: false,
                fontSize: 14,
                wordWrap: 'on',
                tabSize: 2,
            });

            // Sync model -> editor
            ngModel.$render = () => {
                const value = ngModel.$viewValue ?? '';
                const safeValue = typeof value === 'string' ? value : String(value);
                if (editor.getValue() !== safeValue) {
                    editor.setValue(safeValue);
                }
            };

            // Sync editor -> model (debounced)
            let debounceTimer = null;
            const changeDisposable = editor.onDidChangeModelContent(() => {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(() => {
                    const newValue = editor.getValue();
                    if (newValue !== ngModel.$viewValue) {
                        scope.$evalAsync(() => ngModel.$setViewValue(newValue));
                    }
                }, 150);
            });

            // Watch readOnly changes dynamically
            scope.$watch('readOnly', (newVal) => {
                editor.updateOptions({ readOnly: !!newVal });
            });
            
            // Cleanup on destroy (prevent memory leaks)
            scope.$on('$destroy', () => {
                clearTimeout(debounceTimer);
                changeDisposable.dispose();
                editor.getModel()?.dispose();
                editor.dispose();
            });
        },
    };
}
