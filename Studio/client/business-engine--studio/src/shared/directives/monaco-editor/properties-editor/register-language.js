import * as monaco from 'monaco-editor/esm/vs/editor/editor.api.js';

export function registerBPropertiesLanguage() {
    if (monaco.languages.getLanguages().some(l => l.id === 'bProperties')) {
        return;
    }
    monaco.languages.register({ id: 'bProperties' });
    monaco.languages.setMonarchTokensProvider('bProperties', {
        tokenizer: {
            root: [
                [/property|condition/, 'keyword'],
                [/=>/, 'operator'],
                [/[a-zA-Z_]\w*/, 'identifier'],
                [/true|false/, 'number'],
                [/==|!=|>=|<=|>|</, 'operator']
            ]
        }
    });

    monaco.editor.defineTheme('bPropertiesTheme', {
        base: 'vs-dark',
        inherit: true,
        rules: [
            //Root Property
            { token: 'property.root', foreground: '9CDCFE', fontStyle: 'bold' },
            //Child Property
            { token: 'property.child', foreground: '4EC9B0' },
            //Functions (Like VS Code)
            { token: 'builtin-function', foreground: 'DCDCAA', fontStyle: 'bold' },
            //Logical keywords (and, or, not)
            { token: 'keyword.operator', foreground: 'C586C0', fontStyle: 'bold' },
            //Comparison operators
            { token: 'operator', foreground: 'D4D4D4' },
            //Assignment
            { token: 'operator.assignment', foreground: 'D4D4D4' },
            //Boolean
            { token: 'boolean', foreground: '569CD6', fontStyle: 'bold' },
            //Numbers
            { token: 'number', foreground: 'B5CEA8' },
            //Fallback identifiers
            { token: 'identifier', foreground: 'D4D4D4' },
            //Brackets
            { token: 'bracket', foreground: 'FFD700' },
            { token: 'delimiter', foreground: 'BBBBBB' }
        ],
        colors: {
            'editor.background': '#1E1E2E',
            'editor.foreground': '#D4D4D4',
            'editorCursor.foreground': '#AEAFAD',
            'editor.selectionBackground': '#264F78',
            'editor.lineHighlightBackground': '#1E1E2E'
        }
    });

    monaco.languages.setMonarchTokensProvider('bProperties', {
        tokenizer: {
            root: [
                // ====== Functions ======
                [/[a-zA-Z_]\w*(?=\()/, 'builtin-function'],
                // ====== Root Property (Before dot) ======
                [/[a-zA-Z_]\w*(?=\.)/, 'property.root'],
                // ====== Dot ======
                [/\./, 'delimiter'],
                // ====== Child Property (After dot) ======
                [/\.[a-zA-Z_]\w*/, {
                    cases: {
                        '@default': 'property.child'
                    }
                }],
                // ====== Boolean ======
                [/\b(true|false)\b/, 'boolean'],
                // ====== Logical operators ======
                [/\b(and|or|not)\b/, 'keyword.operator'],
                // ====== Comparison operators ======
                [/==|!=|>=|<=|>|</, 'operator'],
                // ====== Assignment ======
                [/=/, 'operator.assignment'],
                // ====== Numbers ======
                [/\d+(\.\d+)?/, 'number'],
                // ====== Identifiers (fallback) ======
                [/[a-zA-Z_]\w*/, 'identifier'],
                // Brackets
                [/[()]/, 'bracket']
            ]
        }
    });
}