export function MonacoDslEditor($timeout) {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            element.css('height', attrs.height || '300px');

            let editor = null;

            // ۱. ثبت زبان (فقط یک‌بار در کل اپلیکیشن)
            function registerMyLanguage() {
                if (monaco.languages.getLanguages().some(lang => lang.id === 'myDSL')) return;

                monaco.languages.register({ id: 'myDSL' });

                // 2. تعریف توکن‌ها (Monarch)
                monaco.languages.setMonarchTokensProvider('myDSL', {
                    keywords: [
                        'if', 'begin', 'end', '||', '&&', '!', 'null', 'false', 'true'
                    ],
                    operators: [
                        '=', '==', '!=', '>', '<', '>=', '<='
                    ],
                    tokenizer: {
                        root: [
                            // شناسایی کلمات کلیدی
                            [/[a-z_$][\w$]*/, {
                                cases: {
                                    '@keywords': 'keyword',
                                    '@default': 'identifier'
                                }
                            }],

                            // شناسایی رشته‌ها (Strings)
                            [/"([^"\\]|\\.)*$/, 'string.invalid'],
                            [/"/, { token: 'string.quote', bracket: '@open', next: '@string' }],

                            // شناسایی اعداد
                            [/\d+/, 'number'],

                            // اپراتورها و جداکننده‌ها
                            [/[{}()\[\]]/, '@brackets'],
                            [/[<>=\!%&|+\-\*\/^]+/, {
                                cases: {
                                    '@operators': 'operator',
                                    '@default': ''
                                }
                            }],

                            // فضای خالی (Whitespace)
                            { include: '@whitespace' }
                        ],

                        string: [
                            [/[^\\"]+/, 'string'],
                            [/\\./, 'string.escape'],
                            [/"/, { token: 'string.quote', bracket: '@close', next: '@pop' }],
                        ],

                        whitespace: [
                            [/[ \t\r\n]+/, 'white'],
                        ],
                    },
                });

                // --- تعریف Autocomplete (هوشمند) ---
                monaco.languages.registerCompletionItemProvider('myDSL', {
                    triggerCharacters: ['.'],
                    provideCompletionItems: (model, position) => {
                        const suggestions = scope.suggestions || {}; // خواندن لحظه‌ای از اسکوپ
                        const lineContent = model.getValueInRange({
                            startLineNumber: position.lineNumber,
                            startColumn: 1,
                            endLineNumber: position.lineNumber,
                            endColumn: position.column
                        });

                        // جدا کردن کلمات برای فهمیدن آخرین کلمه قبل از نقطه
                        const words = lineContent.trim().split(/[ \t]/);
                        const lastWord = words[words.length - 1];

                        // ۱. حالت تایپ بعد از نقطه (مثلاً Product. یا Products.)
                        if (lastWord.includes('.')) {
                            const parts = lastWord.split('.');
                            const parentKey = parts[parts.length - 2]; // کلمه قبل از نقطه آخر

                            const parentObj = suggestions[parentKey];

                            if (parentObj) {
                                let keys = [];
                                // اگر والد یک آبجکت است (مثل Product)
                                if (typeof parentObj === 'object' && !Array.isArray(parentObj)) {
                                    keys = Object.keys(parentObj);
                                }
                                // اگر والد یک لیست است (مثل Products) - فیلدهای اولین آیتم را نشان می‌دهیم
                                else if (Array.isArray(parentObj) && parentObj.length > 0) {
                                    keys = Object.keys(parentObj[0]);
                                }

                                return {
                                    suggestions: keys.map(k => ({
                                        label: k,
                                        kind: monaco.languages.CompletionItemKind.Field,
                                        insertText: k,
                                        detail: `Property of ${parentKey}`
                                    }))
                                };
                            }
                        }

                        // ۲. حالت عمومی (لیست اصلی کلیدها)
                        // این بخش زمانی اجرا می‌شود که کاربر هنوز نقطه نزده است
                        return {
                            suggestions: Object.keys(suggestions).map(key => {
                                let kind = monaco.languages.CompletionItemKind.Variable;
                                if (typeof suggestions[key] === 'object') {
                                    kind = Array.isArray(suggestions[key])
                                        ? monaco.languages.CompletionItemKind.Enum
                                        : monaco.languages.CompletionItemKind.Class;
                                }

                                return {
                                    label: key,
                                    kind: kind,
                                    insertText: key,
                                    detail: 'Global Variable'
                                };
                            }).concat([
                                // اضافه کردن کلمات کلیدی ثابت زبان
                                { label: 'if', kind: monaco.languages.CompletionItemKind.Keyword, insertText: 'if' },
                                { label: 'begin', kind: monaco.languages.CompletionItemKind.Keyword, insertText: 'begin' },
                                { label: 'end', kind: monaco.languages.CompletionItemKind.Keyword, insertText: 'end' }
                            ])
                        };
                    }
                });

                monaco.languages.registerHoverProvider('myDSL', {
                    provideHover: function (model, position) {
                        const word = model.getWordAtPosition(position);

                        const helpDatabase = {
                            'Artist': 'موجودیت اصلی هنرمند. برای دسترسی به مشخصات از نقطه استفاده کنید.',
                            'IsDisabled': 'وضعیت فعال یا غیرفعال بودن هنرمند را مشخص می‌کند (True/False).',
                            'begin': 'شروع یک بلاک منطقی.',
                            'end': 'پایان یک بلاک منطقی.'
                        };

                        if (word && helpDatabase[word.word]) {
                            return {
                                contents: [
                                    { value: '**راهنمای سیستم**' },
                                    { value: helpDatabase[word.word] }
                                ]
                            };
                        }
                    }
                });

                // --- تعریف Folding (تاشو شدن) ---
                monaco.languages.registerFoldingRangeProvider('myDSL', {
                    provideFoldingRanges: (model) => {
                        const ranges = [];
                        const lines = model.getLineCount();
                        let start = -1;
                        for (let i = 1; i <= lines; i++) {
                            const txt = model.getLineContent(i).trim().toLowerCase();
                            if (txt === 'begin') start = i;
                            else if (txt === 'end' && start !== -1) {
                                ranges.push({ start: start, end: i, kind: monaco.languages.FoldingRangeKind.Region });
                                start = -1;
                            }
                        }
                        return ranges;
                    }
                });

                monaco.editor.defineTheme('myDSLTheme', {
                    base: 'vs-dark', // پایه تم تاریک
                    inherit: true,
                    rules: [
                        { token: 'keyword', foreground: 'C586C0', fontStyle: 'bold' }, // بنفش برای کلمات کلیدی
                        { token: 'string', foreground: 'CE9178' },      // نارنجی برای رشته‌ها
                        { token: 'number', foreground: 'B5CEA8' },      // سبز روشن برای اعداد
                        { token: 'operator', foreground: 'D4D4D4' },    // خاکستری برای اپراتورها
                        { token: 'identifier', foreground: '9CDCFE' },  // آبی روشن برای متغیرها
                    ],
                    colors: {
                        'editor.background': '#1E1E1E', // رنگ پس‌زمینه ادیتور
                    }
                });
            }

            // ۲. راه‌اندازی ادیتور
            function initEditor() {
                registerMyLanguage();

                editor = monaco.editor.create(element[0], {
                    value: ngModel.$viewValue || '',
                    language: 'myDSL',
                    theme: 'myDSLTheme',
                    automaticLayout: true,
                    minimap: { enabled: false }
                });

                // ارسال تغییرات ادیتور به ngModel
                editor.onDidChangeModelContent(() => {
                    const newValue = editor.getValue();
                    if (!scope.$$phase)
                        scope.$apply(() => {
                            ngModel.$setViewValue(newValue);
                        });
                });
            }

            // ۳. همگام‌سازی ngModel با ادیتور (زمانی که دیتا از بیرون تغییر می‌کند)
            ngModel.$render = function () {
                if (editor) {
                    const value = ngModel.$viewValue || '';
                    if (editor.getValue() !== value) {
                        editor.setValue(value);
                    }
                }
            };

            // ۴. پاک‌سازی برای جلوگیری از Memory Leak
            scope.$on('$destroy', () => {
                if (editor) {
                    editor.dispose();
                }
            });

            // اجرای راه‌اندازی با کمی تاخیر برای اطمینان از رندر DOM
            $timeout(initEditor, 0);
        }
    };
}