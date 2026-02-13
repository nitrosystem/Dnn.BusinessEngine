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
            suggestions: "=",
            disableFunctions: '='
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
                    if (safeViewValue && typeof (safeViewValue) !== 'string') safeViewValue = safeViewValue.toString()
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

                // if (language === 'bProperties' && e.changes.length > 0 && e.changes[0].text.match(/[a-zA-Z0-9_.\[\]]/)) {
                //     editor.trigger('keyboard', 'editor.action.triggerSuggest', {});
                // }
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

            if (scope.suggestions) showAutocompletion(scope.suggestions);

            if (bMonacoEditorLanguageBuffer.indexOf(language) >= 0) return;

            bMonacoEditorLanguageBuffer.push(language);

            function showAutocompletion(obj) {
                // Helper function to return the monaco completion item type of a thing
                function getType(thing, isMember) {
                    isMember =
                        isMember == undefined ?
                            typeof isMember == "boolean" ?
                                isMember :
                                false :
                            false; // Give isMember a default value of false

                    switch ((typeof thing).toLowerCase()) {
                        case "object":
                            return monaco.languages.CompletionItemKind.Class;

                        case "function":
                            return isMember ?
                                monaco.languages.CompletionItemKind.Method :
                                monaco.languages.CompletionItemKind.Function;

                        default:
                            return isMember ?
                                monaco.languages.CompletionItemKind.Property :
                                monaco.languages.CompletionItemKind.Variable;
                    }
                }

                if (!!completionItemProvider) completionItemProvider.dispose();

                // Register object that will return autocomplete items
                completionItemProvider =
                    monaco.languages.registerCompletionItemProvider(language, {
                        // Run this function when the period or open parenthesis is typed (and anything after a space)
                        triggerCharacters: [".", "("],

                        // Function to generate autocompletion results
                        provideCompletionItems: function (model, position, token) {
                            // Split everything the user has typed on the current line up at each space, and only look at the last word
                            var last_chars = model.getValueInRange({
                                startLineNumber: position.lineNumber,
                                startColumn: 0,
                                endLineNumber: position.lineNumber,
                                endColumn: position.column,
                            });
                            var words = last_chars
                                .replace("\t", "")
                                .replace(/(\[(\w+)\])/g, ".0")
                                .split(" ");
                            var active_typing = words[words.length - 1]; // What the user is currently typing (everything after the last space)

                            // If the last character typed is a period then we need to look at member objects of the obj object
                            var is_member =
                                active_typing.charAt(active_typing.length - 1) == ".";

                            // Array of autocompletion results
                            var result = [];

                            // Used for generic handling between member and non-member objects
                            var last_token = obj;
                            var prefix = "";

                            if (is_member) {
                                // Is a member, get a list of all members, and the prefix
                                var parents = active_typing
                                    .substring(0, active_typing.length - 1)
                                    .split(".");
                                last_token = obj[parents[0]];
                                prefix = parents[0];

                                if (last_token) {
                                    // Loop through all the parents the current one will have (to generate prefix)
                                    for (var i = 1; i < parents.length; i++) {
                                        if (last_token.hasOwnProperty(parents[i])) {
                                            prefix += "." + parents[i];
                                            last_token = last_token[parents[i]];
                                        } else {
                                            // Not valid
                                            return result;
                                        }
                                    }
                                }

                                prefix += ".";
                            }

                            // Get all the child properties of the last token
                            for (var prop in last_token) {
                                // Do not show properites that begin with "__"
                                if (last_token.hasOwnProperty(prop) && !prop.startsWith("__")) {
                                    // Get the detail type (try-catch) incase object does not have prototype
                                    var details = "";
                                    try {
                                        details = last_token[prop].__proto__.constructor.name;
                                    } catch (e) {
                                        details = typeof last_token[prop];
                                    }

                                    var insertText = prop;
                                    if (last_token instanceof Array && !isNaN(parseInt(prop))) {
                                        continue;
                                        //in ro felan natonestam...
                                        //insertText = '[' + prop + ']';
                                    }

                                    // Create completion object
                                    var to_push = {
                                        label: prop,
                                        kind: monaco.languages.CompletionItemKind.Variable,
                                        detail: details,
                                        insertText: insertText,
                                        documentation: "...",
                                    };

                                    // Change insertText and documentation for functions
                                    if (to_push.detail.toLowerCase() == "function") {
                                        to_push.insertText += "(";
                                        to_push.documentation = last_token[prop]
                                            .toString()
                                            .split("{")[0]; // Show function prototype in the documentation popup
                                    }

                                    // Add to final results
                                    result.push(to_push);
                                }
                            }

                            if (!scope.disableFunctions) {
                                //parse global functions
                                const functionSuggestions = Object.entries(globalFunctions).map(([name, meta]) => ({
                                    label: name,
                                    kind: monaco.languages.CompletionItemKind.Function,
                                    insertText: `${name}(`,
                                    documentation: `${meta.description}\n\nArguments: (${meta.args.join(", ")})`,
                                    detail: "Global Function"
                                }));

                                result.push(...functionSuggestions);
                            }

                            return {
                                suggestions: result,
                            };
                        },
                    });
            }
        },
    };
}