import { monacoDefaultOptions } from "../angular-configs/monac-editor.config";
import { globalFunctions } from "../angular-configs/monac-editor.config";

var bMonacoEditorLanguageBuffer = [];
var bMonacoEditorBuffer = [];
var completionItemProvider;

export function MonacoEditor($rootScope, $filter, $timeout) {
    return {
        restrict: "A",
        replace: true,
        require: "?ngModel",
        priority: 10,
        scope: {
            objects: "="
        },
        link: function (scope, element, attrs, ngModel) {
            var readOnly = attrs.readOnly ? JSON.parse(attrs.readOnly) : false;
            var language = attrs.language;
            if (!language) return;

            // Register a new language
            if (
                $filter("filter")(monaco.languages.getLanguages(), function (l) {
                    return l.id == language;
                }).length == 0
            ) {
                monaco.languages.register({ id: language });
            }

            if (language == 'bProperties')
                element.css("min-height", '2.3rem');
            else if (element.data('height'))
                element.css("height", element.data('height'));
            else
                element.css("min-height", '300px');

            var options = monacoDefaultOptions[language] || {};
            options = angular.extend(options, {
                language: language,
                automaticLayout: true,
                readOnly: readOnly,
                theme: "vs-dark"
            });

            var editor = monaco.editor.create(element[0], options);

            if (language == 'bProperties') {
                const node = editor.getDomNode();
                if (node) node.tabIndex = 1;
            }

            bMonacoEditorBuffer.push(editor);

            ngModel.$render = function () {
                if (editor) {
                    var safeViewValue = ngModel.$viewValue || "";
                    editor.setValue(safeViewValue);
                }
            };

            scope.$on(attrs.bCustomFocus, (e, args) => {
                editor.focus();
            });

            // set code in one line after value change
            editor.onDidChangeModelContent(function (e) {
                var newValue = editor.getValue();
                if (newValue !== ngModel.$viewValue) {
                    scope.$evalAsync(function () {
                        ngModel.$setViewValue(newValue);
                    });
                }

                if (language === 'bProperties' && e.changes.length > 0 && e.changes[0].text.match(/[a-zA-Z0-9_.\[\]]/)) {
                    editor.trigger('keyboard', 'editor.action.triggerSuggest', {});
                }
            });

            editor.onKeyDown((e) => {
                if (language !== 'bProperties') return;

                const isTab = e.keyCode === monaco.KeyCode.Tab;
                const isShift = e.shiftKey;

                // اگر Tab یا Shift+Tab زده شد
                if (isTab) {
                    const suggestController = editor.getContribution('editor.contrib.suggestController');

                    const isSuggestOpen = suggestController.model?.state === 'Open';
                    if (!isSuggestOpen) {
                        e.preventDefault();
                        e.stopPropagation();

                        const editorDom = editor.getDomNode();
                        const target = isShift
                            ? findPreviousFocusable(editorDom)
                            : findNextFocusable(editorDom);

                        if (target) target.focus();
                    }
                }
            });

            function findNextFocusable(current) {
                const focusables = Array.from(document.querySelectorAll(
                    'input, button, select, textarea, [tabindex]:not([tabindex="-1"])'
                )).filter(el => !el.disabled && el.offsetParent !== null);

                let index = focusables.indexOf(current);

                // اگر عنصر فعلی داخل ادیتور بود، textarea بعدی رو هم رد کن
                const isMonaco = current.closest?.(".monaco-editor");

                if (index >= 0 && isMonaco) {
                    // رد شو از textarea داخلی موناکو
                    while (index + 1 < focusables.length && focusables[index + 1].classList.contains("inputarea")) {
                        index++;
                    }
                    index++; // برو به بعدی واقعی
                } else {
                    index++; // عادی
                }

                return focusables[index] ?? null;
            }

            function findPreviousFocusable(current) {
                const focusables = Array.from(document.querySelectorAll(
                    'input, button, select, textarea, [tabindex]:not([tabindex="-1"])'
                )).filter(el => !el.disabled && el.offsetParent !== null);

                let index = focusables.indexOf(current);

                // اگر در موناکو هستیم، textarea‌اش رو هم رد کن
                const isMonaco = current.closest?.(".monaco-editor");

                if (index >= 0 && isMonaco) {
                    while (index - 1 >= 0 && focusables[index - 1].classList.contains("inputarea")) {
                        index--;
                    }
                    index--; // برو به قبلی واقعی
                } else {
                    index--;
                }

                return focusables[index] ?? null;
            }

            showAutocompletion(scope.objects);

            if (bMonacoEditorLanguageBuffer.indexOf(language) >= 0) return;

            bMonacoEditorLanguageBuffer.push(language);

            function showAutocompletion(obj) {
                if (!!completionItemProvider) completionItemProvider.dispose();

                completionItemProvider = monaco.languages.registerCompletionItemProvider("bProperties", {
                    triggerCharacters: ['.', '['],

                    provideCompletionItems: function (model, position) {
                        const textUntilPosition = model.getValueInRange({
                            startLineNumber: position.lineNumber,
                            startColumn: 1,
                            endLineNumber: position.lineNumber,
                            endColumn: position.column
                        });

                        const lastToken = extractLastToken(textUntilPosition);
                        const isMemberAccess = /[.\]]$/.test(lastToken);

                        let contextObject = obj;
                        const pathParts = parsePath(lastToken.replace(/[.\[]$/, ""));

                        // تشخیص واژه ناقص (آخرین بخش)
                        let incomplete = null;
                        if (pathParts.length > 0 && typeof pathParts[pathParts.length - 1] === "string") {
                            incomplete = pathParts.pop(); // حذفش می‌کنیم چون هنوز کامل نشده
                        }

                        // resolve مسیر تا parent
                        for (const part of pathParts) {
                            if (contextObject == null) return { suggestions: [] };

                            if (typeof part === "number" && Array.isArray(contextObject)) {
                                contextObject = contextObject[part];
                            } else if (typeof part === "string" && contextObject.hasOwnProperty(part)) {
                                contextObject = contextObject[part];
                            } else {
                                return { suggestions: [] };
                            }
                        }

                        if (typeof contextObject !== "object" || contextObject == null) return { suggestions: [] };

                        let suggestions = Object.keys(contextObject)
                            .filter(key =>
                                !key.startsWith("__") &&
                                (!incomplete || key.toLowerCase().startsWith(incomplete.toLowerCase()))
                            )
                            .map(key => {
                                const value = contextObject[key];
                                const type = typeof value;
                                const isFunction = type === "function";
                                const detail = isFunction ? "Function" : value?.constructor?.name || type;

                                return {
                                    label: key,
                                    kind: isFunction ? monaco.languages.CompletionItemKind.Function : monaco.languages.CompletionItemKind.Property,
                                    insertText: isFunction ? `${key}()` : key,
                                    documentation: isFunction
                                        ? (value.toString().split("{")[0] || "")
                                        : `Type: ${detail}`,
                                    range: {
                                        startLineNumber: position.lineNumber,
                                        startColumn: position.column - (incomplete?.length ?? 0),
                                        endLineNumber: position.lineNumber,
                                        endColumn: position.column
                                    }
                                };
                            });

                        //parse global functions
                        const functionSuggestions = Object.entries(globalFunctions).map(([name, meta]) => ({
                            label: name,
                            kind: monaco.languages.CompletionItemKind.Function,
                            insertText: `${name}(`,
                            documentation: `${meta.description}\n\nArguments: (${meta.args.join(", ")})`,
                            detail: "Global Function"
                        }));

                        suggestions.push(...functionSuggestions);

                        return { suggestions };
                    }
                });

                function extractLastToken(text) {
                    const match = text.match(/([\w\d_\.\[\]]+)$/);
                    return match ? match[0] : "";
                }

                function parsePath(path) {
                    const parts = [];
                    const regex = /([a-zA-Z_][\w]*)|\[(\d+)\]/g;
                    let match;
                    while ((match = regex.exec(path)) !== null) {
                        if (match[1]) parts.push(match[1]);
                        else if (match[2]) parts.push(parseInt(match[2], 10));
                    }
                    return parts;
                }
            }
        },
    };
}